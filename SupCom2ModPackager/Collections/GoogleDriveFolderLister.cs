using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
namespace SupCom2ModPackager.Collections
{
    public class GoogleDriveFolderLister
    {
        public static async Task<string> GetFolderContentsAsJson(string folderId)
        {
            var credentialPath = @"C:\Users\jonle\Downloads\client_secret_804795972106-m9jgpnp04ojbe825qrh68um56870su7l.apps.googleusercontent.com.json";
            credentialPath = @"C:\Users\jonle\Downloads\client_secret_804795972106-m9jgpnp04ojbe825qrh68um56870su7l.apps.googleusercontent.com.json";
            UserCredential credential;
            GoogleClientSecrets secrets; ;
            using (var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read))
            {
                secrets = GoogleClientSecrets.FromStream(stream);
            }

            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets.Secrets,
                new[] { DriveService.Scope.DriveReadonly },
                "user",
                CancellationToken.None,
                new FileDataStore("Drive.Auth.Store"))
                .ConfigureAwait(false);

            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Drive API .NET Quickstart",
            });

            // List files in the folder
            var request = service.Files.List();
            request.Q = $"'{folderId}' in parents and trashed = false";
            request.Fields = "files(id, name, mimeType, modifiedTime)";

            var result = await request.ExecuteAsync();
            return JsonConvert.SerializeObject(result.Files, Formatting.Indented);
        }
    }
}
