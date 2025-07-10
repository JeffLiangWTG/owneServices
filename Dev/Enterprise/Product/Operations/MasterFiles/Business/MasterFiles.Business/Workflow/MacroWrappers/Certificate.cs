using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.MasterFiles.Business.Macros
{
	class Certificate : ICertificate
	{
		public Certificate(GenRegCertAccredMaintList certificate)
		{
			this.certificate = certificate;
		}
		readonly GenRegCertAccredMaintList certificate;

		public ICodeDescription Type => type
			?? (type = new CertificateType()
			{
				Code = certificate?.XZ_Type ?? ZString.Empty,
				Description = certificate?.XZ_TypeDescription ?? ZString.Empty,
				Codes = certificate?.Lookups?.CertificateTypes
			});
		ICodeDescription type;

		public ZDateTime ExpiryDate
		{
			get
			{
				if (!expiryDate.HasValue)
				{
					expiryDate = certificate?.XZ_ExpiryOrDueDate ?? ZDateTime.Empty;
				}

				return expiryDate.Value;
			}
		}
		ZDateTime? expiryDate;
	}
}
