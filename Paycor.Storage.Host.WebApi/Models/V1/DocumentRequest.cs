using System;
using Newtonsoft.Json;
using Paycor.Neo.Common.Extensions;
using Paycor.Neo.Rest.WebApi.Json;

namespace Paycor.Storage.Host.WebApi.Models.V1
{
    public class DocumentRequest
    {
        [JsonProperty(Order = 11)]
        public string Name { get; set; }
        [JsonProperty(Order = 12)]
        public string DocumentType { get; set; }
        [JsonProperty(Order = 13)]
        public string OwnerId { get; set; }

        [JsonProperty(Order = 14)]
        [JsonConverter(typeof(DateOnlyConverter))]
        public DateTime? DocumentDate { get; set; }

        public override string ToString()
        {
            return this.ToStringHelper()
                .Add("Name", Name)
                .Add("DocumentType", DocumentType)
                .Add("DocumentDate", DocumentDate)
                .Add("OwnerId", OwnerId)
                .ToString();
        }
        
    }
}