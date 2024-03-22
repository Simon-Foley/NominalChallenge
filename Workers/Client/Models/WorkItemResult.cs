using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Client.Models
{
    public enum Result
    {
        Succeeded,
        Failed
    }

    public class WorkItemResult
    {
        public string FileName { get; set; }
        public Result Code { get; set; }
        public JObject? JsonResult { get; set; }
        public string? ErrorMessage { get; set; }

        // Convenience constructor with no error code
        public WorkItemResult(string fileName, Result code, JObject jsonResult)
            : this(fileName, code, jsonResult, null)
        {
        }

        // Constructor with error code, used for deserializer
        [JsonConstructor]
        public WorkItemResult(string fileName, Result code, JObject? jsonResult, string? errorMessage)
        {
            FileName = fileName;
            Code = code;
            JsonResult = jsonResult;
            ErrorMessage = errorMessage;
        }
    }
}
