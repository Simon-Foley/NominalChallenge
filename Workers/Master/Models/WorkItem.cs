namespace Master.Models
{
    public class WorkItem
    {
        public string FileName { get; set; }
        public string EncryptedText { get; set; } //Base64 encoded data

        public WorkItem(string fileName, string encryptedText)
        {
            FileName = fileName;
            EncryptedText = encryptedText;
        }
    }
}