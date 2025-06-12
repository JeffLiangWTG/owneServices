using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XT.REST.Deployment.Tasks
{
  public class Sign : Task
  {
    public override bool Execute()
    {
      var success = false;
      Log.LogMessage(MessageImportance.High, "Signing files.");
      try
      {
        using (var httpClient = new HttpClient(new HttpClientHandler { UseDefaultCredentials = true }))
        {
          var result = httpClient.GetStringAsync("https://safe.wisetechglobal.com/winapi/passwords/20634").GetAwaiter().GetResult();
          var array = JsonSerializer.Deserialize<JsonArray>(result);
          var secret = array[0]["GenericField3"].Deserialize<string>();
          var signer = new AzureKeyVaultBuildSigner(secret);
          foreach (var file in Files) 
          {
            signer.SignFileAsync(file.GetMetadata("Identity")).GetAwaiter().GetResult();
          }
        }
        success = true;
      }
      finally
      {
        Shared.LogResult(success, Log);
      }
      return true;
    }

    public ITaskItem[] Files { get; set; }
  }
}