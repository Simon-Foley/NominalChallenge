using System.Text;

namespace Client.Decryptors
{
    public static class CsvCaesarDecryptor
    {
        // ASSUMPTION: File ends in a newline (ASCII # 10)
        public static string Decrypt(string base64EncryptedData)
        {
            var encryptedBytes = Convert.FromBase64String(base64EncryptedData);

            // Grab the last byte
            var lastByte = encryptedBytes[encryptedBytes.Length - 1];

            // Figure out the shift to move it to 10 
            var shift = (lastByte - 10 + 128) % 128;

            // Caesar shift the whole array by that
            var decryptedBytes = encryptedBytes.Select(b => (byte)((b - shift + 128) % 128)).ToArray();

            return Encoding.UTF8.GetString(decryptedBytes);
        }

    }
}
