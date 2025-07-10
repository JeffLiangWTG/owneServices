using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.ExportUnion;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business
{
	public class ExportUnionItemsProvider : IExportUnionItems
	{
		public ExportUnionItemsProvider(CusEntryLine cusEntryLine, ZDateTime effectiveDate)
		{
			CusEntryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
			RandomInvoiceLine = (JobComInvoiceLine)CusEntryLine.RandomLine;
			EffectiveDate = effectiveDate;
		}
		CusEntryLine CusEntryLine { get; }
		JobComInvoiceLine RandomInvoiceLine { get; }
		ZDateTime EffectiveDate { get; }

		public string ContainerTypeCode => RandomInvoiceLine.ZG_ExportUnionPackCode;
		public int ContainerCount => (int)DeclarationProviderHelper.GetContainerCount(CusEntryLine);
		public string RegimeCode => RandomInvoiceLine.ProcedureCode;
		public string ProductionYear => RandomInvoiceLine.ZG_ExportUnionProductionYear > 0 ? RandomInvoiceLine.ZG_ExportUnionProductionYear.ToString() : ZString.Empty;
		public string ThreadType => RandomInvoiceLine.ZG_ExportUnionThreadCode;
		public decimal ItemAbroadExpenses => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.TotalForeignCharges);
		public string ItemAbroadExpensesCurrencyCode => DeclarationProviderHelper.GetDefaultCurrencyCode(CusEntryLine, TRIncotermChargeCodeList.Codes.TotalForeignCharges);
		public decimal InternalExpensesAmount => DeclarationProviderHelper.GetChargesTotal(CusEntryLine, ZBool.False, TRIncotermChargeCodeList.Codes.LocalTotalCharges);
		public string Ecological => RandomInvoiceLine.ZG_ExportUnionEcological ? CusEntryMessageConstants.ExportUnionConstants.Answers.YesAbbreviation : CusEntryMessageConstants.ExportUnionConstants.Answers.NoAbbreviation;
		public string SubjectToQuota => CusEntryMessageConstants.ExportUnionConstants.Answers.NoAbbreviation;
		public string SubjectToPricelessExport => RandomInvoiceLine.ZG_PriceType == PriceTypeList.Codes._02 ? CusEntryMessageConstants.ExportUnionConstants.Answers.YesAbbreviation : CusEntryMessageConstants.ExportUnionConstants.Answers.NoAbbreviation;

		public string TransporterTaxid
		{
			get
			{
				var jobDeclaration = RandomInvoiceLine.InvoiceHeader.JobDeclaration;
				var taxID = jobDeclaration.ShippingLine?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey) ?? ZString.Empty;
				if (taxID.IsEmpty)
				{
					taxID = jobDeclaration.Forwarder?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Turkey) ?? ZString.Empty;
				}
				return taxID;
			}
		}

		IReadOnlyCollection<IExportUnionCompanies> fExportUnionCompanies;
		public IReadOnlyCollection<IExportUnionCompanies> Companies
		{
			get
			{
				if (fExportUnionCompanies == null)
				{
					var companyList = new List<IExportUnionCompanies>();
					if (RandomInvoiceLine.ManufacturerAddress != null)
					{
						companyList.Add(new ExportUnionCompaniesProvider(RandomInvoiceLine.ManufacturerAddress, DocAddressTypes.Codes.Manufacturer, EffectiveDate));
					}
					fExportUnionCompanies = companyList.ToArray();
				}

				return fExportUnionCompanies;
			}
		}
	}
}
