using Client.Decryptors;
using Client.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;

namespace Client
{
    public class MessageHandler(ILogger logger)
    {

        public WorkItemResult HandleMessage(string message)
        {
            WorkItem workItem = null;
            try
            {
                workItem = JsonConvert.DeserializeObject<WorkItem>(message)
                               ?? throw new JsonException("Deserialized work item is null.");

                logger.LogInformation($"Handling WorkItem: {workItem.FileName}");
                var decryptedString = CsvCaesarDecryptor.Decrypt(workItem.EncryptedText);
                var dataTable = StringCsvToDataTable(decryptedString);
                var resultJson = GetMaxValuesJsonObject(dataTable);

                return new WorkItemResult(workItem.FileName, Result.Succeeded, resultJson);
            }
            catch (Exception ex)
            {
                // Log exceptions and produce a failed work item
                var fileName = workItem?.FileName ?? string.Empty;
                logger.LogError($"Unexpected error handling message for '{fileName}': {ex.Message}");
                return new WorkItemResult(fileName, Result.Failed, null, ex.Message);
            }
        }


        private DataTable StringCsvToDataTable(string data)
        {
            try
            {
                DataTable dt = new DataTable();
                string[] tableData = data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Get columns from headers
                string[] headers = tableData[0].Split(new[] { ',' }, StringSplitOptions.None);
                foreach (string header in headers)
                {
                    dt.Columns.Add(header, typeof(double)); // Assume all columns are doubles
                }

                // Skip the header and iterate over each row
                foreach (var line in tableData.Skip(1))
                {
                    var fields = line.Split(new[] { ',' }, StringSplitOptions.None);
                    var rowValues = fields.Select(field =>
                    {
                        if (double.TryParse(field, out double result))
                        {
                            return (object)result;
                        }
                        else
                        {
                            throw new ArgumentException($"Unable to parse '{field}' as a double.");
                        }
                    }).ToArray();

                    dt.Rows.Add(rowValues);
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Failed to parse CSV to DataTable.", ex);
            }
        }

        private JObject GetMaxValuesJsonObject(DataTable dataTable)
        {
            JObject maxValuesObject = new JObject();

            foreach (DataColumn column in dataTable.Columns)
            {
                var maxValue = dataTable.AsEnumerable()
                    .Select(row => row[column])
                    .Max();
                maxValuesObject.Add(column.ColumnName, JToken.FromObject(maxValue));
            }

            return maxValuesObject;
        }
    }
}
