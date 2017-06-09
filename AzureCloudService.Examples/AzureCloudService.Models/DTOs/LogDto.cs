
namespace AzureCloudService.Utils.DTOs
{
    using Infrastructures;
    using System.ComponentModel.DataAnnotations;

    public class LogDto : IBaseModel
    {
        [Required, StringLength(50, MinimumLength = 3)]
        public string Owner { get; set; }

        [Required, StringLength(255, MinimumLength = 3)]
        public string Content { get; set; }
    }
}
