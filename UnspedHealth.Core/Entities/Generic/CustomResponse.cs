using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnspedHealth.Core.Entities
{
    public class CustomResponse<T>
    {
        [JsonProperty(PropertyName = "Data")]
        public T? Data { get; set; }

        [JsonProperty(PropertyName = "Status")]
        public CustomResponseStatus Status { get; set; }

        [JsonProperty(PropertyName = "Messages")]
        public List<string>? Messages { get; set; }

        [JsonProperty(PropertyName = "Successful")]
        public bool Successful => this.Status != CustomResponseStatus.Error;

        // factory methods.
        public static CustomResponse<T> Success(T data, List<string> messages)
        {
            return new CustomResponse<T> { Data = data, Status = CustomResponseStatus.Success, Messages = (messages != null ? messages : new List<string>()) };
        }

        public static CustomResponse<T> Success(List<string> messages)
        {
            return new CustomResponse<T>
            {
                Data = default(T),
                Status = CustomResponseStatus.Success,
                Messages = (messages != null ? messages : new List<string>())
            };
        }

        public static CustomResponse<T> SuccessDefault()
        {
            return new CustomResponse<T>
            {
                Data = default(T),
                Status = CustomResponseStatus.Success,
                Messages = new List<string>
                {
                    "İşleminiz başarıyla gerçekleştirilmiştir."
                }
            };
        }

        public static CustomResponse<T> Warning(T data, List<string> messages)
        {
            return new CustomResponse<T> { Data = data, Status = CustomResponseStatus.Warning, Messages = (messages != null ? messages : new List<string>()) };
        }

        public static CustomResponse<T> Warning(List<string> messages)
        {
            return new CustomResponse<T>
            {
                Data = default(T),
                Status = CustomResponseStatus.Warning,
                Messages = (messages != null ? messages : new List<string>())
            };
        }

        public static CustomResponse<T> Fail(List<string> messages)
        {
            return new CustomResponse<T>
            {
                Messages = (messages != null ? messages : new List<string>()),
                Status = CustomResponseStatus.Error
            };
        }

        public static CustomResponse<T> FailDefault()
        {
            return new CustomResponse<T>
            {
                Data = default(T),
                Status = CustomResponseStatus.Error,
                Messages = new List<string>
                {
                    "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin."
                }
            };
        }
    }
}
