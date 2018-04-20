using Microsoft.Azure;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDownloader
{
    public class Class1
    {
        public enum ContainerCategory
        {
            TenantSpecific,
            OCR,
            Internal,
            Public,
            Others
        }

        public string GetContainerName(ContainerCategory containerCategory)
        {
            switch (containerCategory)
            {
                case ContainerCategory.OCR:
                    string ocrContainerName = CloudConfigurationManager.GetSetting("OcrContainerName");
                    if (!string.IsNullOrEmpty(ocrContainerName))
                    {
                        return ocrContainerName;
                    }
                    return "ocr";

                default:
                    string docContainerName = CloudConfigurationManager.GetSetting("DocContainerName");
                    if (!string.IsNullOrEmpty(docContainerName))
                    {
                        return docContainerName;
                    }
                    return "docs";
            }
        }

        private static async Task<string> GetAzureKeyVaultAccessToken(string authority, string resource, string scope)
        {
            string clientId = ConfigurationManager.AppSettings["AADClientId"];
            string clientSecret = ConfigurationManager.AppSettings["AADClientSecret"];

            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(clientId, clientSecret);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);
            if (result == null)
            {
                //ErrorAquiringAzureKeyVaultAuthenticationToken(authority, resource);
                throw new InvalidOperationException();
            }

            return result.AccessToken;
        }
        public string RemoveContainerName(string filePath)
        {
            int firstIndex = filePath.IndexOf('/');
            if (firstIndex > 0)
            {
                return filePath.Substring(firstIndex + 1);
            }
            return filePath;
        }

        public string GetContainerName(string filePath)
        {
            int firstIndex = filePath.IndexOf('/');
            if (firstIndex > 0)
            {
                return filePath.Substring(0, firstIndex);
            }
            else
            {
                return null;
            }
        }
        private async Task<ICloudBlob> DownloadEncryptedBlobReferenceFromServerAsync(string filePath, string blobStorageAccountName, string blobStorageKey, bool isSecondary = false)
        {
            //LogDownloadInfo("DownloadEncryptedBlobReferenceFromServerAsync");

            var creds = new StorageCredentials(blobStorageAccountName, blobStorageKey);
            var storageAccount = new CloudStorageAccount(creds, true);
            string containerName = GetContainerName(filePath) ?? GetContainerName(ContainerCategory.TenantSpecific);

            var resolver = new KeyVaultKeyResolver(GetAzureKeyVaultAccessToken);

            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);

            var downloadOptions = new BlobRequestOptions();
            downloadOptions.EncryptionPolicy = new BlobEncryptionPolicy(null, resolver);
            downloadOptions.RequireEncryption = true;

            if (isSecondary)
            {
                client.DefaultRequestOptions = downloadOptions;
                downloadOptions.LocationMode = LocationMode.SecondaryOnly;
                //LogDownloadInfo("Blob Location Mode Secondary inside DownloadEncryptedBlobReferenceFromServerAsync ");
            }
            else
            {
                downloadOptions.LocationMode = LocationMode.PrimaryOnly;
                //LogDownloadInfo("Blob Location Mode Primary inside DownloadEncryptedBlobReferenceFromServerAsync ");
            }

            ICloudBlob blob = await container.GetBlobReferenceFromServerAsync(RemoveContainerName(filePath), null, downloadOptions, null);

            if (isSecondary)
            {
                //Log Storage Uri
                string storageUri = !string.IsNullOrEmpty(blob.StorageUri.SecondaryUri.ToString()) ? blob.StorageUri.SecondaryUri.ToString() : blob.StorageUri.PrimaryUri.ToString();
                //LogStorageInfo("Got Blob reference Successfully from location : " + storageUri);

                //Log Storage Replication Status
                //LogServiceStatus(client);
            }

            return blob;
        }
    }
}
