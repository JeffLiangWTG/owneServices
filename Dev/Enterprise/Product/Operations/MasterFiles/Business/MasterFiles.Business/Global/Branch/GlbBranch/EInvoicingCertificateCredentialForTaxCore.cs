using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public sealed class EInvoicingCertificateCredentialForTaxCore : EInvoicingCertificateCredential
	{
		public EInvoicingCertificateCredentialForTaxCore(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_GB = GlbBranch.CurrentBranch.PK;
			GP_PasswordType = PasswordTypesList.Codes.FPC;
		}

		protected override string GetSerialNumber(X509Certificate2 certificate)
		{
			var subjectFields = GetDistinguishedNameFields(certificate.SubjectName);
			return subjectFields.TryGetValue("SERIALNUMBER", out var subjectNameSerialNumberTemp) ? subjectNameSerialNumberTemp : string.Empty;
		}

		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new EInvoicingCertificateCredentialForTaxCoreValidation(this);
		}
	}
}
