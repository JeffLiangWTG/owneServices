using CargoWise.Types;

namespace Enterprise.Customs.US.DIS.Business
{
	class DISCertificateDataWrapper : IDISCertificate
	{
		public DISCertificateDataWrapper(DISCertificateData data, string importerOfRecordNo)
		{
			this.data = data;
			this.importerOfRecordNo = importerOfRecordNo;
		}

		readonly DISCertificateData data;
		readonly string importerOfRecordNo;

		ZString IDISCertificate.Number
		{
			get { return data.CertificateNumber; }
		}

		ZString IDISCertificate.Type
		{
			get { return data.CertificateType; }
		}

		ZString IDISCertificate.Statement
		{
			get { return data.Statement; }
		}

		ZDateTime IDISCertificate.IssueDate
		{
			get { return data.IssueDate; }
		}

		ZDateTime IDISCertificate.ExpiryDate
		{
			get { return data.ExpiryDate; }
		}

		ZString IDISCertificate.ImporterOfRecord
		{
			get { return importerOfRecordNo; }
		}

		ZString IDISCertificate.InspectionLocation
		{
			get { return data.InspectionLocation; }
		}

		ZDecimal IDISCertificate.GrossTonnage
		{
			get { return data.GrossTonnage; }
		}

		ZDecimal IDISCertificate.NetTonnage
		{
			get { return data.NetTonnage; }
		}
	}
}
