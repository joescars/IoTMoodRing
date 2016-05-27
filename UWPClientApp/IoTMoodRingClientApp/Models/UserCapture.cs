using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTMoodRing.Models
{
    class UserCapture
    {
        public string Text { get; set; }
        public string Url { get; set; }

        public string MessageType { get; }

        public UserCapture()
        {
            this.MessageType = "interactive";
        }

        public async Task UploadImage(Stream stream)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Config.StorageAccountConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("captures");
            await container.CreateIfNotExistsAsync();

            await container.SetPermissionsAsync(new BlobContainerPermissions{
                PublicAccess = BlobContainerPublicAccessType.Blob
            });

            var guid = Guid.NewGuid().ToString();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(guid + ".jpg");

            //var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///Assets/mickey.jpg"));
            await blockBlob.UploadFromStreamAsync(stream);
            //await blockBlob.UploadFromFileAsync(file);
            this.Url = blockBlob.Uri.ToString();
        }
    }
}
