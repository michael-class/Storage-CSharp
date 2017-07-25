using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Common.Logging;
using Paycor.Neo.Common.Base;
using Paycor.Neo.Logging.Format;
using Paycor.Storage.Data.EF.Context;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Test
{
    class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger<Program>();

        const string BASE_URL = "http://localhost";
        //const string BASE_URL = "http://ipv4.fiddler";
        const string UPLOAD_FILE = "Resources.DallasFloorPlan.jpg";
        const string DOC_ID = "3c90a6a395e3e411a707f8165422f4a6";

        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            _logger.Trace("Main()");

            //TestEF();
            var exit = false;
            do
            {
                Console.WriteLine("\nPress: ");
                Console.WriteLine("\t1 - To upload (non multi-part).");
                Console.WriteLine("\t2 - To upload (multi-part).");
                Console.WriteLine("\t3 - To download.");
                Console.WriteLine("\t Any other key to exit.\n");
                var input = Console.ReadKey();

                switch (input.KeyChar)
                {
                    case '1' :
                        TestStreamingStorageApi();
                        break;
                    case '2':
                        TestMultipartStorageApi();
                        break;
                    case '3':
                        TestDownloadStorageApi();
                        break;
                    default:
                        exit = true;
                        break;
                }
            } while (!exit);

        }

        static void TestStreamingStorageApi()
        {
            _logger.Trace("TestStreamingStorageApi()");

            Console.WriteLine("\nStreaming Test: Press any key to continue...");
            Console.ReadKey();

            var uploadUri = string.Format("docs/api/storage/v1/documents/{0}/content", DOC_ID);
            _logger.Debug(m => m("uri={0}", uploadUri));

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(BASE_URL),
                Timeout = TimeSpan.FromMinutes(10)
            };
            httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
            var program = typeof (Program);
            //ListResources(program.Assembly);

            var fileStream = program.Assembly.GetManifestResourceStream(program, UPLOAD_FILE);
            if (fileStream == null)
                throw new ApplicationException("Upload file cannot be loaded from manifest resource.  File=" + UPLOAD_FILE);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            fileContent.Headers.ContentLength = fileStream.Length;

            var responseMessage = httpClient.PostAsync(uploadUri, fileContent).Result;
            var result = responseMessage.Content.ReadAsStringAsync().Result;

            var status = responseMessage.StatusCode;
            _logger.Info(m => m("status={0}", status));
            if (status == HttpStatusCode.OK)
            {
                _logger.Info(m => m("response={0}", result));
            }
            Console.WriteLine("\nresponse={0}", result);
            httpClient.Dispose();

        }

        static void TestMultipartStorageApi()
        {
            _logger.Trace("TestMultipartStorageApi()");

            Console.WriteLine("\nMultipart Test: Press any key to continue...");
            Console.ReadKey();

            var uploadUri = string.Format("docs/api/storage/v1/documents/{0}/content", DOC_ID);
            _logger.Debug(m => m("uri={0}", uploadUri));

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(BASE_URL),
                Timeout = TimeSpan.FromMinutes(10)
            };
            httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
            var program = typeof(Program);
            //ListResources(program.Assembly);

            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(program.Assembly.GetManifestResourceStream(program, UPLOAD_FILE));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "file1", UPLOAD_FILE);

            var responseMessage = httpClient.PostAsync(uploadUri, content).Result;
            var result = responseMessage.Content.ReadAsStringAsync().Result;

            var status = responseMessage.StatusCode;
            _logger.Info(m => m("status={0}", status));
            if (status == HttpStatusCode.OK)
            {
                _logger.Info(m => m("response={0}", result));
            }
            Console.WriteLine("\nresponse={0}", result);
            httpClient.Dispose();

        }

        private static void TestDownloadStorageApi()
        {
            _logger.Trace("TestDownloadStorageApi()");

            Console.WriteLine("\nDownload Test: Press any key to continue...");
            Console.ReadKey();

            var downloadUri = string.Format("docs/api/storage/v1/documents/{0}/content", DOC_ID);
            _logger.Debug(m => m("uri={0}", downloadUri));

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(BASE_URL),
            };
            httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
            httpClient.Timeout = TimeSpan.FromMinutes(10);
            var message = new HttpRequestMessage(HttpMethod.Get, downloadUri);
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
            message.Headers.Add("X-Paycor-Request-ID","ea581061-2573-4d3b-a181-503d41dbde4b");

            var timer = Stopwatch.StartNew();
            var file = new FileInfo("test.jpg");
            var responseMessage = httpClient.SendAsync(message).GetAwaiter().GetResult();
            var content = responseMessage.Content;
            content.ReadAsStreamAsync().ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    using (var fileStream = new FileStream(file.FullName, FileMode.Create))
                    {
                        t.Result.CopyTo(fileStream);
                        file.Refresh();
                        var msg = LogEventFormatter.Format("DocTest").AddEvent("download").Add("file", file.FullName)
                            .Add("size", file.Length).AddElapsedTime(timer).ToString();

                        _logger.Info(m => m("{0}", msg));
                        Console.WriteLine("\n{0}", msg);
                    }
                }
            }).GetAwaiter().GetResult();
            httpClient.Dispose();
        }

        // ReSharper disable once UnusedMember.Local
        static void TestEF()
        {
            _logger.Trace("TestEF()");

            const string DEFAULT = "Default";

            using (var db = new StorageContext())
            {
                var a = db.Apps.Add(new Account(DEFAULT, DEFAULT, DEFAULT, "Azure"));
                var c = a.CreateContainer(DEFAULT);

                db.Documents.Add(c.CreateDocument("test.txt", "txt", "rgeyer"));

                db.SaveChanges();

                foreach (var app in db.Apps)
                {
                    Console.WriteLine("{0}", app);
                }

                foreach (var doc in db.Documents)
                {
                    Console.WriteLine("{0}", doc);
                }

            }
        }

        // ReSharper disable once UnusedMember.Local
        static void ListResources(Assembly assembly)
        {
            foreach (var name in assembly.GetManifestResourceNames())
            {
                _logger.Debug(name);
            }
        }


    }
}
