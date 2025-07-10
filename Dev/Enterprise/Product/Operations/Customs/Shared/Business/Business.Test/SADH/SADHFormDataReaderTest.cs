using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.SADH.Testing
{
	public class SADHFormDataReaderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestReadFromFillsInAllFieldsFromDeclaration()
		{
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "BLAH";
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "FOOBAR";
			GlbBranch branch = GlbCompany.CurrentCompany.Branches.AddNew();
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "NZD";

			SetupDeclarationFields(consignor, consignee, branch);
			BaseJobComInvoiceHeader firstInvoice = Declaration.Invoices.AddNew();
			SetupInvoiceHeaderFields(firstInvoice, currency);
			BaseJobComInvoiceLine firstLine = firstInvoice.JobComInvoiceLines.GetByLineNo(1);
			SetupInvoiceLineFields(firstLine);

			firstLine.JI_Description = "Something longer than the 280 characters limit. Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah ";
			Reader.ReadFrom(Declaration);

			AssertReadFromFillsInAllFieldsFromDeclaration(consignor, consignee, branch, currency);
		}

		public void TestDeliveryTermsReadFromCommercialInvoiceUsingTheDeclarationAsAFallback()
		{
			Declaration.JE_ShipmentIncoTerm = Constants.IncoTerms.FreeOnBoard;
			Reader.ReadFrom(Declaration);
			AssertEquals("formData.D1_DeliveryTerms", Constants.IncoTerms.FreeOnBoard, FormData.D1_DeliveryTerms);

			BaseJobComInvoiceHeader firstInvoice = Declaration.Invoices.AddNew();
			firstInvoice.JZ_IncoTerm = Constants.IncoTerms.CostAndFreight;
			Reader.ReadFrom(Declaration);
			AssertEquals("formData.D1_DeliveryTerms", Constants.IncoTerms.CostAndFreight, FormData.D1_DeliveryTerms);
		}

		public void TestReaderDoesntAddAnyInvoicesOrInvoiceLines()
		{
			Reader.ReadFrom(Declaration);
			AssertEquals("declaration.Invoices.Count", 0, Declaration.Invoices.Count);
			AssertEquals("declaration.InvoiceLines.Count", 0, Declaration.InvoiceLines.Count);
		}

		public void TestTriggeringReaderDoesntTriggerHasChanges()
		{
			var formData = FormData;
			var declaration = Declaration;
			Factory.Save();
			AssertEquals("Precondition: formData.HasChanges", false, formData.HasChanges);
			AssertEquals("Precondition: Declaration.HasChanges", false, declaration.HasChanges);
			Reader.ReadFrom(declaration);
			AssertEquals("formData.HasChanges", false, formData.HasChanges);
			AssertEquals("Declaration.HasChanges", false, declaration.HasChanges);
		}

		public void TestReadInvoiceHeaderFieldsAfterChangeAssessmentDate()
		{
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "BLAH";
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "FOOBAR";
			GlbBranch branch = GlbCompany.CurrentCompany.Branches.AddNew();

			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "NZD";
			RefExchangeRate exchangeRate = Factory.New<RefExchangeRate>();

			exchangeRate.RE_ExRateType = "CUS";
			exchangeRate.RE_StartDate = ZDateTime.Now.AddMonths(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddMonths(1);
			exchangeRate.RE_SellRate = 0.5m;
			currency.ExchangeRates.Add(exchangeRate);

			RefExchangeRate exchangeRate1 = Factory.New<RefExchangeRate>();

			exchangeRate1.RE_ExRateType = "CUS";
			exchangeRate1.RE_StartDate = ZDateTime.Now.AddMonths(1);
			exchangeRate1.RE_ExpiryDate = ZDateTime.Now.AddMonths(3);
			exchangeRate1.RE_SellRate = 0.7m;
			currency.ExchangeRates.Add(exchangeRate1);

			FormData.D1_RX_InvoiceCurrency = currency.PK;

			var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();

			SetupDeclarationFields(consignor, consignee, branch);
			var invoice = CreateNewInoviceForTest();
			Declaration.Invoices.Add(invoice);
			SetupInvoiceHeaderFields(invoice, currency);

			entryInstruction.CEI_DateForDuty = ZDateTime.Now;
			Reader.ReadFrom(Declaration);
			AssertEquals("FormData.ExchangeRate", 0.5m, FormData.ExchangeRate);

			entryInstruction.CEI_DateForDuty = ZDateTime.Now.AddMonths(2);
			AssertEquals("FormData.ExchangeRate", 0.7m, FormData.ExchangeRate);
		}

		#region Implementation

		#region InvoiceForTest

		class InvoiceHeaderForTest : BaseJobComInvoiceHeader
		{
			public InvoiceHeaderForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			protected override ZDateTime EffectiveValuationDateCore
			{
				get
				{
					var originalEntryInstruction = InvoiceLines.Cast<BaseJobComInvoiceLine>().Where(x => x.EntryInstruction != null).Select(x => x.EntryInstruction)
						.Distinct().OrderBy(x => x.CEI_SubStyle).ThenBy(x => x.CEI_Description).FirstOrDefault(x => x.CEI_DateForDuty.IsValid);
					return originalEntryInstruction?.CEI_DateForDuty ?? base.EffectiveValuationDateCore;
				}
			}
		}

		#endregion

		#region Declaration
		protected BaseJobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = GetNewJobDeclaration()); }
		}
		BaseJobDeclaration fDeclaration;

		protected virtual BaseJobDeclaration GetNewJobDeclaration()
		{
			return Factory.New<BaseJobDeclarationWithEntryInstructions>();
		}
		#endregion

		#region Reader
		protected SADHFormDataReader Reader
		{
			get { return fReader ?? (fReader = GetNewSADHFormDataReader()); }
		}
		SADHFormDataReader fReader;

		protected virtual SADHFormDataReader GetNewSADHFormDataReader()
		{
			return new SADHFormDataReader(FormData);
		}
		#endregion

		#region FormData
		protected SADHFormData FormData
		{
			get { return fFormData ?? (fFormData = GetNewSADHFormData()); }
		}
		SADHFormData fFormData;

		protected virtual SADHFormData GetNewSADHFormData()
		{
			return new SADHFormData(Factory, Declaration);
		}
		#endregion

		protected virtual BaseJobComInvoiceHeader CreateNewInoviceForTest()
		{
			return Factory.New<InvoiceHeaderForTest>();
		}

		protected virtual void SetupInvoiceLineFields(BaseJobComInvoiceLine firstLine)
		{
			SetupInvoiceLineQuantitiesSoTheyDontOverWriteEachOther(firstLine);
			firstLine.JI_Description = "TEST DESCRIPTION OF GOODS";
			firstLine.JI_CountryOfOrigin = "AU";
			firstLine.JI_LinePrice = 12m;
		}

		protected virtual void SetupInvoiceHeaderFields(BaseJobComInvoiceHeader firstInvoice, RefCurrency currency)
		{
			firstInvoice.JobComInvoiceLines.AddNew();
			firstInvoice.JZ_RX_NKInvoice_Currency = currency.RX_Code;
			firstInvoice.JZ_IncoTerm = Constants.IncoTerms.FreeOnBoard;
		}

		protected virtual void SetupDeclarationFields(OrgHeader consignor, OrgHeader consignee, GlbBranch branch)
		{
			BaseJobDeclaration declaration = Declaration;
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee.PK;

			declaration.JE_TotalNoOfPacks = 27;
			declaration.JE_TotalNoOfPacksPackType = Constants.PkgUnit.Box;

			declaration.JE_RL_NKFinalDestination = "GBLON";
			declaration.JE_RL_NKOrigin = "NZAKL";

			declaration.JE_MarksAndNumbers = "MARK II MACH V";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "HLP";

			declaration.JE_TransportMode = "AIR";

			declaration.JE_RL_NKPortOfLoading = "GBLAN";
			declaration.JE_RL_NKPortOfArrival = "NZAKL";

			declaration.JE_ShipmentIncoTerm = Constants.IncoTerms.FreeOnBoard;

			declaration.JE_GB = branch.PK;

			declaration.JE_VoyageFlightNo = "ABER3541";
			declaration.JE_ExportDate = new ZDateTime(2008, 1, 1);
			declaration.JE_VesselName = "USS ENTERPRISE";
		}

		void SetupInvoiceLineQuantitiesSoTheyDontOverWriteEachOther(BaseJobComInvoiceLine firstLine)
		{
			firstLine.JI_Tariff = "2010421010";

			firstLine.JI_InvoiceUQ = "KG";
			firstLine.JI_InvoiceQuantity = 4231.3409m;

			firstLine.JI_WeightUQ = "KG";
			firstLine.JI_Weight = 500m;

			firstLine.JI_CustomsUnitQty = "KG";
			firstLine.JI_CustomsQuantity = 3.45m;
		}

		protected virtual void AssertReadFromFillsInAllFieldsFromDeclaration(OrgHeader consignor, OrgHeader consignee, GlbBranch branch, RefCurrency currency)
		{
			SADHFormData formData = FormData;
			AssertEquals("formData.Consignor", consignor, formData.Consignor);
			AssertEquals("formData.Consignee", consignee, formData.Consignee);
			AssertEquals("formData.D1_GrossMassUnits", "KG", formData.D1_GrossMassUQ);
			AssertEquals("formData.D1_GrossMassKG", 500m, formData.D1_GrossMass);
			AssertEquals("formData.D1_TotalPackages", 27, formData.D1_TotalPackages);
			AssertEquals("formData.D1_TotalPackagesPackType", Constants.PkgUnit.Box, formData.D1_TotalPackagesPackType);
			AssertEquals("formData.D1_CountryOriginCode", "NZAKL", formData.D1_RL_NKCountryOfOrigin);
			AssertEquals("formData.D1_CountryDestinationCode", "GBLON", formData.D1_RL_NKCountryOfDestination);
			AssertEquals("formData.D1_InvoiceCurrency", currency.PK, formData.D1_RX_InvoiceCurrency);
			AssertEquals("formData.D1_PackagesDescriptionGoods", "Something longer than the 280 characters limit. Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Blah Bl", formData.D1_DescriptionOfGoods);
			AssertEquals("formData.D1_PackagesDescriptionGoodsPackageMarksNumberKind", "MARK II MACH V", formData.D1_MarksAndNumbers);
			AssertEquals("formData.D1_FormType", JobMessageTypeList.Codes.Import, formData.D1_MessageType);
			AssertEquals("formData.D1_ModeTransportAtBorder", "AIR", formData.D1_ModeOfTransportAtTheBorder);
			AssertEquals("formData.D1_GoodsCountryOriginCode", "AU", formData.D1_RN_NKItemCountryOfOrigin);
			AssertEquals("formData.D1_CDispExpCode", "GBLAN", formData.D1_RL_NKCountryOfDispatchOrExport);
			AssertEquals("formData.D1_PlaceUnloading", "NZAKL", formData.D1_RL_NKPlaceOfUnloading);
			AssertEquals("formData.D1_DeliveryTerms", Constants.IncoTerms.FreeOnBoard, formData.D1_DeliveryTerms);
			AssertEquals("formData.D1_ItemPrice", 12m, formData.D1_ItemPrice);
			AssertEquals("formData.D1_SupplementaryUnitsQty", 3.45m, formData.D1_SupplementaryQty);
			AssertEquals("formData.D1_SupplementaryUnitsQtyUnits", "KG", formData.D1_SupplementaryUQ);
			AssertEquals("formData.D1_IdentityNationalityActiveTransportBorderFlightNo", "ABER3541", formData.D1_FlightNo);
			AssertEquals("formData.D1_IdentityNationalityActiveTransportBorderFlightDate", new ZDateTime(2008, 1, 1), formData.D1_FlightDate);
			AssertEquals("formData.D1_IdentityNationalityActiveTransportBorderVesselCode", "USS ENTERPRISE", formData.D1_VesselCode);
			AssertEquals("formData.D1_PackagesDescriptionGoodsQty", 4231.3409m, formData.D1_ItemQty);
			AssertEquals("formData.D1_PackagesDescriptionGoodsQtyUnits", "KG", formData.D1_ItemUQ);
			AssertEquals("formData.D1_CommodityCode", "2010421010", formData.D1_CommodityCode);
		}
		#endregion
	}
}
