using Microsoft.WindowsAzure.Storage.Table;

namespace day_03.Model
{
    public class PngImageEntity : TableEntity
    {
        public PngImageEntity(string key, string row)
        {
            this.PartitionKey = key;
            this.RowKey = row;
        }

        public PngImageEntity() { }

        public string Url { get; set; }
        public string Name { get; set; }
    }
}
