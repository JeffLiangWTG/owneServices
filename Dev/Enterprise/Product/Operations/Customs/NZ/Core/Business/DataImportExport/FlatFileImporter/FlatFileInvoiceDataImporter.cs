using System;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MasterFiles;
using ECB = Enterprise.Customs.Business;

namespace Enterprise.Customs.NZ.Business.Data.FlatFileImporter
{
	public class FlatFileInvoiceDataImporter : DataTransfer.FlatFileInvoiceDataImporter
	{
		public FlatFileInvoiceDataImporter(string fileName, JobDeclaration destinationJobDeclaration)
			: base(fileName, destinationJobDeclaration)
		{
		}

		public new class Constants : DataTransfer.FlatFileInvoiceDataImporter.Constants
		{
			public new class InvoiceLineFields : DataTransfer.FlatFileInvoiceDataImporter.Constants.InvoiceLineFields
			{
				public const int Export = OriginState;
			}
		}

		protected override Type GetOrgSupplierPartType()
		{
			return typeof(OrgSupplierPart);
		}

		protected override void PopulateInvoiceLine(ECB.BaseJobComInvoiceLine newInvoiceLine, string[] fieldValues)
		{
			base.PopulateInvoiceLine(newInvoiceLine, fieldValues);

			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)newInvoiceLine;
			SetValue(invoiceLine.JI_RN_NKCountryOfExportInfo, fieldValues, Constants.InvoiceLineFields.Export);
			SetValue(invoiceLine.JI_QualifiesForPreferentialDutyInfo, fieldValues, Constants.InvoiceLineFields.Preference);
			SetValue(invoiceLine.JI_ConcessionCodeInfo, fieldValues, Constants.InvoiceLineFields.Concession);
			invoiceLine.JI_QualifiesForPreferentialDuty = invoiceLine.JI_QualifiesForPreferentialDuty.Left(1) == "Q" ? QualifiesForPreferentialDutyList.Codes.Qualifies : QualifiesForPreferentialDutyList.Codes.NonQualifying;
		}

		protected override void PopulateInvoiceHeader(ECB.BaseJobComInvoiceHeader invHead, string[] fieldValues)
		{
			base.PopulateInvoiceHeader(invHead, fieldValues);
			if (invHead is JobComInvoiceHeader)
			{
				JobComInvoiceHeader header = (JobComInvoiceHeader)invHead;
				SetValue(header.JZ_RelationshipIndicatorInfo, fieldValues, Constants.InvoiceHeaderFields.Related);
			}
		}

		protected override string GetClassificationTypeMatching(ECB.BaseJobComInvoiceLine invoiceLine)
		{
			return ECB.BaseCusClassification.ClassificationType.Both;
		}
	}
}
