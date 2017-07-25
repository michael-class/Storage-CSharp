using System;
using System.IO;
using Common.Logging;
using Paycor.Neo.Common.Base;
using Paycor.Neo.Common.Extensions;
using Paycor.Neo.Logging.Format;
using Paycor.Neo.Domain.Entity;

namespace Paycor.Storage.Domain.Entity
{
    public class Document : Entity<Guid>
    {
        private static readonly ILog _logger = LogManager.GetLogger<Document>();

        private const string SOURCE = "Document";

        private Stream _contents;
        private DocumentStatus _status;
        private DateTime _documentDate;

        public Container Container { get; private set; }
        public Guid ContainerId { get; private set; }
        public string Name { get; private set; }
        public string OwnerId { get; private set; }
        public string DocumentType { get; private set; }

        public DateTime DocumentDate
        {
            get { return _documentDate.Date; }
            private set { _documentDate = value.Date; }
        }

        public DateTime StatusDateTime { get; private set; }

        public DocumentStatus Status
        {
            get { return _status; }
            private set
            {
                StatusDateTime = DateTime.Now;
                _status = value;
            }
        }

        public long Size { get; private set; }
        public int UploadCount { get; private set; }
        public int DownloadCount { get; private set; }
        public BlobInfo BlobInfo { get; private set; }

        // non persistent fields
        public string DocumentPath
        {
            get
            {
                var date = DocumentDate;
                return string.Format("{0}/{1}/{2}/{3}/{4}", 
                    date.Year, date.Month.ToString("D2"), date.Day.ToString("D2"), OwnerId, Id);
            }
        }

        public string StorageAccount
        {
            get { return Container == null ? null : Container.StorageAccount; }
        }

        public string StorageProvider
        {
            get { return Container == null ? null : Container.StorageProvider; }
        }

        public Stream ContentStream
        {
            get
            {
                if (_contents == null)
                {
                    throw new InvalidOperationException("Document has not been retrieved");
                }
                return _contents;
            }
            set { _contents = value; }
        }

        public bool CanBeUploaded
        {
            get { return Status == DocumentStatus.Created || Status == DocumentStatus.Committed; }
        }

        public bool CanBeDownloaded
        {
            get { return Status == DocumentStatus.Committed; }
        }

        public bool CanBeRecovered
        {
            get { return Status == DocumentStatus.PendingDelete; }
        }

        private Document()
            : base(Guid.NewGuid())
        {
            DownloadCount = 0;
            UploadCount = 0;
            Size = -1;
            Status = DocumentStatus.Created;
            BlobInfo = new BlobInfo();
        }

        internal Document(Container container, string name, string documentType, string ownerId, DateTime? documentDate = null) : this()
        {
            Check.NotNull(container, "container");
            Check.NotNullOrEmpty(name, "name");
            Check.NotNullOrEmpty(documentType, "documentType");
            Check.NotNullOrEmpty(ownerId, "ownerId");

            Container = container;
            ContainerId = container.Id;
            Name = name;
            DocumentType = documentType;
            OwnerId = ownerId;
            var date = documentDate == null ? DateTime.Now : documentDate.Value;
            DocumentDate = date.Date;
        }

        public Document OnUpload(Stream contentStream, BlobInfo blobInfo, long length)
        {
            Check.NotNull(contentStream, "contentStream");
            Check.NotNull(blobInfo, "blobInfo");

            ValidateUpload();

            UploadCount++;
            BlobInfo = blobInfo;
            Status = DocumentStatus.Committed;
            ContentStream = contentStream;
            Size = length;
            _logger.Debug(m => m("{0}", LogEventFormatter.Format(SOURCE).AddEvent("Upload").Add("doc", this)));

            return this;
        }

        public Document OnDownload(Stream contentStream, BlobInfo blobInfo)
        {
            Check.NotNull(contentStream, "contentStream");
            Check.NotNull(blobInfo, "blobInfo");

            ValidateDownload();
            DownloadCount++;
            BlobInfo = blobInfo;
            ContentStream = contentStream;

            _logger.Debug(m => m("{0}", LogEventFormatter.Format(SOURCE).AddEvent("Download").Add("doc", this)));

            return this;
        }

        public Document OnDelete()
        {
            Status = DocumentStatus.Deleted;
            _logger.Debug(m => m("{0}", LogEventFormatter.Format(SOURCE).AddEvent("Deleted").Add("doc", this)));
            return this;
        }

        public Document OnPendingDelete()
        {
            ValidateDownload();

            Status = DocumentStatus.PendingDelete;
            _logger.Debug(m => m("{0}", LogEventFormatter.Format(SOURCE).AddEvent("PendingDelete").Add("doc", this)));
            return this;
        }

        public Document OnRecover()
        {
            ValidateRecover();

            Status = DocumentStatus.Committed;
            _logger.Debug(m => m("{0}", LogEventFormatter.Format(SOURCE).AddEvent("Recover").Add("doc", this)));
            return this;
        }

        public void ValidateRecover()
        {
            if (CanBeRecovered) return;
            throw CreateValidationException("recovered");
        }

        public void ValidateUpload()
        {
            if (CanBeUploaded) return;
            throw CreateValidationException("uploaded");
        }

        public void ValidateDownload()
        {
            if (CanBeDownloaded) return;
            throw CreateValidationException("dowloaded");
        }

        private Exception CreateValidationException(string operation)
        {
            var msg = string.Format("Document cannot be {2}, invalid status; docId={0}; status={1}", Id, Status, operation);
            return new InvalidOperationException(msg);
        }

        public override string ToString()
        {
            return this.ToStringHelper()
                .Add("Id", Id)
                .Add("Name", Name)
                .Add("DocumentPath", DocumentPath)
                .Add("DocumentType", DocumentType)
                .Add("DocumentDate", DocumentDate.ToDateString())
                .Add("StatusDateTime", StatusDateTime.ToDateTimeString())
                .Add("Status", Status)
                .Add("OwnerId", OwnerId)
                .Add("Size", Size)
                .Add("UploadCount", UploadCount)
                .Add("DownloadCount", DownloadCount)
                .AddValue(BlobInfo)
                .AddValue(Container)
                .OmitNullValues()
                .ToString();
        }
    }
}
