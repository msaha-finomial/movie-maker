using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDownloader
{
    public class AzureKeyVaultClient
    {
        private static async Task<string> GetAzureKeyVaultAccessToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(
                ConfigurationManager.AppSettings["AADClientId"],
                ConfigurationManager.AppSettings["AADClientSecret"]);

            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
            {
                return null;
            }

            return result.AccessToken;
        }             
        
        public async Task<string> GetToken(string key)
        {
            string url = "https://abootkakv.vault.azure.net/secrets/";
            url = url + key;
            // Create KeyVaultClient with vault credentials
            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetAzureKeyVaultAccessToken));

            // Get a SAS token for our storage from Key Vault
            var sasToken = await kv.GetSecretAsync(url);
            return sasToken.Value;
        }

    }
}
