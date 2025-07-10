using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class USInvoiceLineFSISLineCollection : DependentCusAddInfoCollection<USInvoiceLineFSISLine, JobComInvoiceLine>, IPGADataCorrectionCollection
	{
		public USInvoiceLineFSISLineCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine, CusAddInfoTypeAttribute.Codes.USFSISCertificate)
		{ }

		public void ClearAllNotifications()
		{
			foreach (USInvoiceLineFSISLine line in this)
			{
				line.ClearAllNotifications();
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var previousLine = Count > 0 ? this[Count - 1] : null;
			var newLine = (USInvoiceLineFSISLine)child;
			var declaration = Master.Declaration;
			if (declaration != null)
			{
				newLine.US_DateOfInspection = declaration.US_InspecDate;
			}
			if (previousLine != null)
			{
				newLine.US_CommercialDescription = previousLine.US_CommercialDescription;
				newLine.US_UC_NKCertificateIssuerCountry = previousLine.US_UC_NKCertificateIssuerCountry;
				newLine.US_UC_NKCountryOfOrigin = previousLine.US_UC_NKCountryOfOrigin;
				newLine.US_ImportingEstNo = previousLine.US_ImportingEstNo;
				newLine.US_ExportingEstNo = previousLine.US_ExportingEstNo;
				newLine.US_ProductIDQualifier = previousLine.US_ProductIDQualifier;
				newLine.US_ProductID = previousLine.US_ProductID;
				newLine.US_IntendedUseCode = previousLine.US_IntendedUseCode;
				newLine.US_CertifyingIndividual = previousLine.US_CertifyingIndividual;
				newLine.US_PGAContactName = previousLine.US_PGAContactName;
				newLine.US_PGAContactPhoneNo = previousLine.US_PGAContactPhoneNo;
				newLine.US_PGAContactEmail = previousLine.US_PGAContactEmail;

				foreach (USFSISLot lot in previousLine.Lots)
				{
					newLine.Lots.Add((USFSISLot)lot.Clone());
				}
				newLine.Lots.OfType<USFSISLot>().ToList().ForEach(x => x.ResetValueAfterClone());
			}
			else if (Master != null)
			{
				newLine.US_CommercialDescription = Master.JI_Description.Left(USFSISLineAddInfo.Schema.US_CommercialDescriptionMaxLength);
				newLine.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
				if (declaration != null)
				{
					newLine.US_ImportingEstNo = declaration.US_FSISInspec;
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return AllowAddNewPGALines && base.AllowNewCore; }
		}

		public bool AllowAddNewPGALines
		{
			get
			{
				if (!fAllowAddNewPGALines.HasValue)
				{
					var invoiceLine = Master;
					fAllowAddNewPGALines = invoiceLine?.AllowAddNewLineToPGACollection() ?? true;
				}
				return fAllowAddNewPGALines.Value;
			}
			set
			{
				fAllowAddNewPGALines = value;
				if (value)
				{
					var invoiceLine = Master;
					invoiceLine?.Declaration?.UpdatePGADataReplacementUpdateRequired();
				}
			}
		}
		bool? fAllowAddNewPGALines;

		System.Collections.Generic.IEnumerable<IPGADataCorrection> IPGADataCorrectionCollection.CorrectionItems => this.Cast<IPGADataCorrection>();

		BusinessObject IPGADataCorrectionCollection.Master => Master;
	}
}
