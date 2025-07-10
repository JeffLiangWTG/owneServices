
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DataTransfer.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.NZ.Business.Data
{
	public class NZInvoiceDataTransferTool : InvoiceDataTransferTool
	{
		public NZInvoiceDataTransferTool(bool isStandAlone, AddInfoDataTransferTool addInfoDataTransferTool)
			: base(isStandAlone)
		{
			AddInfoDataTransferTool = addInfoDataTransferTool;
		}

		protected readonly AddInfoDataTransferTool AddInfoDataTransferTool;

		#region Import

		protected override void ImportInvoiceHeaderAdditionalInfo(BaseJobComInvoiceHeader invHead, Xsd.AdditionalCustomsInformationCollection addCustomsDetails, IValueObjectImportContext context)
		{
			JobComInvoiceHeader nZInvHeader = invHead as JobComInvoiceHeader;
			if (nZInvHeader != null)
			{
				foreach (Xsd.AdditionalCustomsInformation addCustomsInfo in addCustomsDetails)
				{
					if (addCustomsInfo.CustomsDetailType == "ExchangeRateIndicator")
					{
						nZInvHeader.JZ_ExchangeRateIndicator = addCustomsInfo.CustomsDetailValue;
					}
				}

				AddInfoDataTransferTool.ImportAddInfos(nZInvHeader, addCustomsDetails, context);
			}
		}

		protected override void ImportInvoiceLinesAdditionalInfo(BaseJobComInvoiceLine invLine, Xsd.AdditionalCustomsInformationCollection addCustomsDetails, IValueObjectImportContext context)
		{
			if (invLine is JobComInvoiceLine nZInvLine)
			{
				AddInfoDataTransferTool.ImportAddInfos(nZInvLine, addCustomsDetails, context);
			}
		}

		protected override void ImportInvoiceLineDetail(BaseJobComInvoiceLine invoiceLine, Xsd.InvoiceLine xsdInvoiceLine, IValueObjectImportContext context)
		{
			base.ImportInvoiceLineDetail(invoiceLine, xsdInvoiceLine, context);

			if (xsdInvoiceLine.LineClassification != null && !xsdInvoiceLine.LineClassification.Preference.IsEmpty)
			{
				JobComInvoiceLine nZInvoiceLine = invoiceLine as JobComInvoiceLine;
				if (nZInvoiceLine != null)
				{
					context.SetPropertyInfoValue(nZInvoiceLine.JI_QualifiesForPreferentialDutyInfo, xsdInvoiceLine.LineClassification.Preference, xsdInvoiceLine.LineClassification.PreferenceSpecified);
				}
			}
		}

		protected override string GetClassificationTypeMatching(BaseJobComInvoiceHeader invoice)
		{
			return BaseCusClassification.ClassificationType.Both;
		}

		#endregion

		#region Export

		protected override void ExportInvoiceHeaderAdditionalInfo(Xsd.AdditionalCustomsInformationCollection addCustomsDetails, BaseJobComInvoiceHeader invHead, IValueObjectExportContext context)
		{
			JobComInvoiceHeader nZInvHeader = invHead as JobComInvoiceHeader;
			if (nZInvHeader != null)
			{
				if (!nZInvHeader.JZ_ExchangeRateIndicator.IsEmpty)
				{
					Xsd.AdditionalCustomsInformation addCustomsDetail = addCustomsDetails.AddNew();
					addCustomsDetail.CustomsDetailType = "ExchangeRateIndicator";
					addCustomsDetail.CustomsDetailValue = nZInvHeader.JZ_ExchangeRateIndicator;
				}
				AddInfoDataTransferTool.ExportAddInfos(addCustomsDetails, nZInvHeader);
			}
		}

		protected override void ExportInvoiceLinesAdditionalInfo(Xsd.AdditionalCustomsInformationCollection addCustomsDetails, BaseJobComInvoiceLine invLine, IValueObjectExportContext context)
		{
			JobComInvoiceLine nZInvLine = invLine as JobComInvoiceLine;
			if (nZInvLine != null)
			{
				AddInfoDataTransferTool.ExportAddInfos(addCustomsDetails, nZInvLine);
			}
		}

		public override void ExportInvoiceLineDetails(Xsd.InvoiceLine newInvoiceLine, BaseJobComInvoiceLine invoiceLine, INotifications notify)
		{
			base.ExportInvoiceLineDetails(newInvoiceLine, invoiceLine, notify);

			JobComInvoiceLine nZInvoiceLine = invoiceLine as JobComInvoiceLine;
			if (nZInvoiceLine != null && !nZInvoiceLine.JI_QualifiesForPreferentialDuty.IsEmpty)
			{
				newInvoiceLine.LineClassification.Preference = nZInvoiceLine.JI_QualifiesForPreferentialDuty;
			}
		}

		#endregion
	}
}
