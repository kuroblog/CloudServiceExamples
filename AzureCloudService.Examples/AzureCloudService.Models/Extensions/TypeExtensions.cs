
namespace AzureCloudService.Utils.Extensions
{
    using Infrastructures;
    using Models;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Linq;

    public static class TypeExtensions
    {
        public static VerifyResult[] VerifyAll<TDto>(this TDto dto) where TDto : IBaseModel, new()
        {
            var result = new List<VerifyResult>();

            dto.GetType().GetProperties().ToList().ForEach(p =>
            {
                p.GetCustomAttributes(typeof(ValidationAttribute), false).ToList().ForEach(a =>
                {
                    var attribute = a as ValidationAttribute;
                    var value = p.GetValue(dto);

                    var isValid = attribute.IsValid(value);
                    var validMessage = isValid ? string.Empty : attribute.FormatErrorMessage(p.Name);

                    result.Add(new VerifyResult
                    {
                        Name = p.Name,
                        Type = p.PropertyType,
                        Value = value,
                        IsValid = isValid,
                        Message = validMessage
                    });
                });
            });

            return result.ToArray();
        }

        public static VerifyResult Verify<TDto>(this TDto dto) where TDto : IBaseModel, new()
        {
            return dto.VerifyAll().FirstOrDefault(p => p.IsValid == false);
        }

        public static string ToJsonString<TDto>(this TDto dto) where TDto : IBaseModel, new()
        {
            return JsonConvert.SerializeObject(dto);
        }

        public static void OutputToDebug<TDto>(this TDto dto) where TDto : IBaseModel, new()
        {
            Debug.WriteLine(dto.ToJsonString());
        }
    }
}
