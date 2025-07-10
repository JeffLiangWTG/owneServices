using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.ExportUnion;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business
{
	public class ExportUnionDeclarationProvider : IExportUnionDeclaration
	{
		public ExportUnionDeclarationProvider(JobDeclaration jobDeclaration, JobComInvoiceLine invoiceLine, CusEntryInstruction cusEntryInstruction)
		{
			JobDeclaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
			InvoiceLine = invoiceLine;
			EntryInstruction = cusEntryInstruction;
		}
		JobDeclaration JobDeclaration { get; }
		JobComInvoiceLine InvoiceLine { get; }
		CusEntryInstruction EntryInstruction { get; }
		ZDateTime EffectiveDate => InvoiceLine != null ? InvoiceLine.EffectiveAssessmentDate : ZDateTime.Empty;

		public string SoftwareHouseCode => CusEntryMessageConstants.ExportUnionConstants.SoftwareHouseCode;
		public string DeclarationDate => ZDateTime.Now.ToString(CusEntryMessageConstants.DateFormat.DayMonthYear);
		public string UnionSecretaryCode => EntryInstruction != null ?  EntryInstruction.ZG_ExportUnionSecretaryCode : ZString.Empty;
		public string UnionCode => EntryInstruction != null ? EntryInstruction.ZG_ExportUnionCode : ZString.Empty;
		public string CustomsRegistrationCode => ZString.Empty; //TO DO: We need add field other WI
		public string CustomsRegistrationDate => ZString.Empty; //TO DO: We need add field other WI
		public string OrderKindCode => InvoiceLine != null ? InvoiceLine.InvoiceHeader.JobDeclaration.JE_ExportGoodsType : ZString.Empty;
		public string ECommerce => JobDeclaration.ZG_TradeType == TradeTypeList.Codes.ETD ? CusEntryMessageConstants.ExportUnionConstants.TradeTypes.ECommerce : CusEntryMessageConstants.ExportUnionConstants.TradeTypes.NotECommerce;
		public string PaymentType => InvoiceLine != null ? InvoiceLine.ZG_CommercialPaymentCode : ZString.Empty;
		public string DestinationTransitCountryCode => EntryInstruction != null ? EntryInstruction.ZG_ExportUnionCountryCode : ZString.Empty;
		public string Container => JobDeclaration.IsContainerised && JobDeclaration.ContainerMode == Core.Constants.ContainerModes.Containerised ? CusEntryMessageConstants.ExportUnionConstants.Answers.YesValue : CusEntryMessageConstants.ExportUnionConstants.Answers.NoValue;
		public string DeliveryLocation => JobDeclaration.JE_ShipmentIncoTermPlace.SubstringSafe(0, 3);
		public string DeclarationType1 => CusEntryMessageConstants.ExportUnionConstants.DeclarationTypes.ExportType;
		public string DeclarationType2 => CusEntryMessageConstants.ExportUnionConstants.DeclarationTypes.MainActivity;
		public string OriginCountryCode => InvoiceLine != null ? UniversalReferenceDataHelper.MapCW1CountryCodeToCustomsCode(InvoiceLine.Factory, InvoiceLine.JI_CountryOfOrigin, EffectiveDate) : ZString.Empty;
		public decimal CurrencyRate => JobDeclaration.JE_DeclarationExchangeRate;
		public string AgreementType => InvoiceLine != null ? InvoiceLine.JI_ValuationCode : ZString.Empty;
		public string TotalDomesticExpenditureCurrency => Core.Constants.CurrencyCodes.Turkey;
		public string InternalTransportType => EntryInstruction != null ? EntryInstruction.ZG_InlandTransportType : ZString.Empty;
		public string PaymentPassword => ZString.Empty; //TO DO: We need add field other WI

		IReadOnlyCollection<IExportUnionCompanies> fExportUnionCompanies;
		public IReadOnlyCollection<IExportUnionCompanies> Companies
		{
			get
			{
				if (fExportUnionCompanies == null)
				{
					var companyList = new List<IExportUnionCompanies>();

					if (JobDeclaration.SupplierDocumentaryAddress.Address != null)
					{
						companyList.Add(new ExportUnionCompaniesProvider(JobDeclaration.SupplierDocumentaryAddress.Address, DocAddressTypes.Codes.SupplierDocumentaryAddress, EffectiveDate));
					}

					if (JobDeclaration.ImporterDocumentaryAddress.Address != null)
					{
						companyList.Add(new ExportUnionCompaniesProvider(JobDeclaration.ImporterDocumentaryAddress.Address, DocAddressTypes.Codes.ImporterDocumentaryAddress, EffectiveDate));
					}

					if (JobDeclaration.DeclarantAddress != null)
					{
						companyList.Add(new ExportUnionCompaniesProvider(JobDeclaration.DeclarantAddress, DocAddressTypes.Codes.Declarant, EffectiveDate));
					}

					if (JobDeclaration.Representative != null)
					{
						companyList.Add(new ExportUnionCompaniesProvider(JobDeclaration.Representative, DocAddressTypes.Codes.Representative, EffectiveDate));
					}

					if (JobDeclaration.DeclarantAddress != null)
					{
						companyList.Add(new ExportUnionCompaniesProvider(JobDeclaration.DeclarantAddress, DocAddressTypes.Codes.COLSResponsibleParty, EffectiveDate));
					}

					fExportUnionCompanies = companyList.ToArray();
				}
				return fExportUnionCompanies;
			}
		}
	}
}
