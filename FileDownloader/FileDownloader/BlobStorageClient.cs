using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDownloader
{
    public class BlobStorageClient
    {
        public static string GetPath(string filename)
        {
            string saveTo = Path.GetDirectoryName(Environment.CurrentDirectory) + @"\temp\";  //Folder to Save
            if (!Directory.Exists(saveTo))
            {
                Directory.CreateDirectory(saveTo);
            }
            return saveTo + filename;
        }

        string containerName = "bootcampdocs";
        string blobEndpoint = "https://abootkstorage.blob.core.windows.net/";
        string blobSecurityToken = "abootkstorage-blobSAStoken";
        public async Task<string> UploadImage(byte[] document, string fileName)
        {
            try
            {
                AzureKeyVaultClient _client = new AzureKeyVaultClient();
                var token = await _client.GetToken(blobSecurityToken);
                var creds = new StorageCredentials(token);
                var accountWithSas = new CloudStorageAccount(creds, new Uri(blobEndpoint), null, null, null);
                var blobClientWithSas = accountWithSas.CreateCloudBlobClient();

                var container = blobClientWithSas.GetContainerReference(containerName);
                container.CreateIfNotExists();

                var blob = container.GetBlockBlobReference(fileName);
                blob.Properties.ContentType = @"image\jpeg";
                await blob.UploadFromByteArrayAsync(document, 0, document.Length);

                return string.Format("{0}/{1}", containerName, fileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> DownloadImage(string fileName)
        {
            try
            {
                AzureKeyVaultClient _client = new AzureKeyVaultClient();
                var token = await _client.GetToken(blobSecurityToken);
                var creds = new StorageCredentials(token);
                var accountWithSas = new CloudStorageAccount(creds, new Uri(blobEndpoint), null, null, null);
                var blobClientWithSas = accountWithSas.CreateCloudBlobClient();

                var container = blobClientWithSas.GetContainerReference(containerName);
                var blob = container.GetBlockBlobReference(fileName);
                blob.Properties.ContentType = @"image\jpeg";

                blob.DownloadToFile(GetPath(fileName), FileMode.Create);

                return string.Format("{0}/{1}", containerName, fileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> UploadVideo(byte[] video, string videoFilename)
        {
            try
            {
                AzureKeyVaultClient _client = new AzureKeyVaultClient();
                var token = await _client.GetToken(blobSecurityToken);
                var creds = new StorageCredentials(token);
                var accountWithSas = new CloudStorageAccount(creds, new Uri(blobEndpoint), null, null, null);
                var blobClientWithSas = accountWithSas.CreateCloudBlobClient();

                var container = blobClientWithSas.GetContainerReference(containerName);
                container.CreateIfNotExists();

                var blob = container.GetBlockBlobReference(videoFilename);
                blob.Properties.ContentType = @"video/x-msvideo";
                await blob.UploadFromByteArrayAsync(video, 0, video.Length);

                return string.Format("{0}/{1}", containerName, videoFilename);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
