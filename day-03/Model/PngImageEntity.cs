using Microsoft.WindowsAzure.Storage.Table;

namespace day_03.Model
{
    public class PngImageEntity : TableEntity
    {
        public PngImageEntity(string imageName)
        {
            PartitionKey = imageName;
            RowKey = imageName;
        }

        public string Url { get; set; }
    }
}
