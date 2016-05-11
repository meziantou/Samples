using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace ConsoleApplication12
{
    public class ChartConfiguration
    {
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public ChartType ChartType { get; set; }
        public IList<Column> Columns { get; set; }
        public IList<object[]> Rows { get; set; }
        public FileFormat FileFormat { get; set; }

        public string FileExtension
        {
            get
            {
                switch (FileFormat)
                {
                    case FileFormat.Png:
                        return ".png";
                    case FileFormat.Jpeg:
                        return ".jpg";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public string ComputeFileName()
        {
            string hash = ComputeHash();
            return hash + FileExtension;
        }

        public string ComputeHash()
        {
            var serialized = JsonConvert.SerializeObject(this);
            var bytes = Encoding.UTF8.GetBytes(serialized);
            using (var hashAlgorithm = SHA256.Create())
            {
                var hash = hashAlgorithm.ComputeHash(bytes);
                var sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}