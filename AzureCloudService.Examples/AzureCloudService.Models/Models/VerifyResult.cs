
namespace AzureCloudService.Utils.Models
{
    using Infrastructures;
    using System;

    public class VerifyResult : IBaseModel
    {
        public string Name { get; set; } = string.Empty;

        public Type Type { get; set; } = null;

        public object Value { get; set; } = null;

        public bool IsValid { get; set; } = false;

        public string Message { get; set; } = string.Empty;
    }
}
