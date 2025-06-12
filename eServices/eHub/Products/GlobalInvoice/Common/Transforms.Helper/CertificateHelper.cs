using System;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Shared.Crypto;


namespace CargoWise.eHub.Products.GlobalInvoice.Common.Transforms.Helper
{
  public class CertificateHelper
  {
    public virtual string CreateSignedMessageBase64(string message, string recipientId, string category)
    {
      using (var context = GetContext())
      {
        var senderCert = context.eHubCertificates.SingleOrDefault(x => x.eHubClient.CC_ID == recipientId && x.CE_Category == category);
        var encryptedContent = CmsHelpers.ComputeSignature(Encoding.UTF8.GetBytes(message), senderCert.CE_BinaryContainer,
          senderCert.CE_Password, new[] { new Pkcs9SigningTime() }, false);
        return Convert.ToBase64String(encryptedContent);
      }
    }

    internal static Func<eHubTransactionsContext> GetContext = () => new eHubTransactionsContext();
  }
}