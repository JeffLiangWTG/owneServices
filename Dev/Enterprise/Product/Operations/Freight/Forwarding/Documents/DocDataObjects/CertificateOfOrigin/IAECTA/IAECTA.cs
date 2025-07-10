using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class IAECTA : CertificateOfOriginDocDataObject<IAECTALineItem>
	{
		public IAECTA(ZString sourceType, ZString sourseID)
			: base(sourceType, sourseID)
		{
		}

		public ZString ExportDocumentNumber
		{
			get => exportDocumentNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ExportDocumentNumberInfo, ref exportDocumentNumber, value))
				{
					Validate(ExportDocumentNumberInfo);
				}
			}
		}
		ZString exportDocumentNumber;

		public ZPropertyInfo ExportDocumentNumberInfo => GetZPropertyInfo(nameof(ExportDocumentNumber));
	}
}
