using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ShipmentDocManagerInfo))]
	public class ShipmentDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			return shipment;
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";

			consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			consol = shipment.Consols.AddNew();
			container1 = consol.Containers.AddNew();
			container2 = consol.Containers.AddNew();

			PackLine line1 = shipment.OuterPackLines.AddNew();
			line1.Containers.Add(container1);

			PackLine line2 = shipment.OuterPackLines.AddNew();
			line2.Containers.Add(container2);

			declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;

			commInvoice = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			commInvoice[JobComInvoiceHeaderSchema.Constants.JZ_JE] = declaration.PK;

			cusEntryHeader = (BusinessObject)Factory.New<Enterprise.Integration.Customs.ICusEntryHeader>();
			cusEntryHeader[CusEntryHeaderSchema.Constants.CH_JE] = declaration.PK;

			var countryNZ = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.NewZealand);
			var companyNZ = Factory.New<GlbCompany>();
			companyNZ.GC_Code = "WEL";
			companyNZ.GC_RN_NKCountryCode = countryNZ.Code;
			var branchNZ = Factory.New<GlbBranch>();
			branchNZ.GB_Code = "BR1";
			branchNZ.GB_GC = companyNZ.PK;

			declarationNZ = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declarationNZ[JobDeclarationSchema.Constants.JE_GB] = branchNZ.PK;
			declarationNZ[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;

			commInvoiceNZ = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			commInvoiceNZ[JobComInvoiceHeaderSchema.Constants.JZ_JE] = declarationNZ.PK;

			debtor = Factory.New<OrgHeader>();
			debtor.OH_IsDebtor = ZBool.True;
			debtor.OH_Code = "DEBTOR";

			invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice.AH_OH = debtor.PK;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

			// shouldn't be included in the related objects
			nonShipmentInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			nonShipmentInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			nonShipmentInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			nonShipmentInvoice.AH_OH = debtor.PK;
			nonShipmentInvoice.AH_GB = GlbBranch.CurrentBranch.PK;

			rateHeader = Factory.New(ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().QuoteType);
			rateHeader[RatingHeaderSchema.TH_QuoteNumber] = "12345";

			Factory.Save();

			JobHeader shipmentJob = new JobHeader.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			shipmentJob.JH_TH_NKQuoteNumber = (ZString)rateHeader[RatingHeaderSchema.TH_QuoteNumber];

			Factory.Save();

			return shipment;
		}

		public virtual void TestAllRelatedObjectsRetrieved()
		{
			CommonShipment shipment = (CommonShipment)GetPopulatedParentBusinessObject();
			IList relatedObjects = shipment.DocManagerInfo.RelatedObjects;
			AssertEquals("Should have the containers in the related business objects", true, relatedObjects.Contains(container1));
			AssertEquals("Should have the containers in the related business objects", true, relatedObjects.Contains(container2));
			AssertEquals("Should have the declaration in the related business objects", true, relatedObjects.Contains(declaration));
			AssertEquals("Should NOT have the declaration NZ in the related business objects", false, relatedObjects.Contains(declarationNZ));
			AssertEquals("Should have the commercial invoice in the related business objects", true, relatedObjects.Contains(commInvoice));
			AssertEquals("Should NOT have the commercial invoice NZ in the related business objects", false, relatedObjects.Contains(commInvoiceNZ));
			AssertEquals("Should have the consol in the related business objects", true, relatedObjects.Contains(consol));
			AssertEquals("Should have the consignee in the related business objects", true, relatedObjects.Contains(consignee));
			AssertEquals("Should have the consignor in the related business objects", true, relatedObjects.Contains(consignor));
			AssertEquals("Should have the invoice in the related business objects", true, relatedObjects.Contains(invoice));
			AssertEquals("Should NOT have the non Shipment invoice in the related business objects", false, relatedObjects.Contains(nonShipmentInvoice));
			AssertEquals("Should have the cus entry header in the related business objects", true, relatedObjects.Contains(cusEntryHeader));
			AssertEquals("Should have the Quote found by the Quote Number in the related business objects", true, relatedObjects.Contains(rateHeader));

			shipment.JS_TH_OneTimeQuote = Factory.New(ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().QuoteType).PK;

			Factory.Save();

			relatedObjects = shipment.DocManagerInfo.RelatedObjects;
			AssertEquals("Should NOT have the Quote found by the Quote Number in the related business objects", false, relatedObjects.Contains(rateHeader));
		}

		public void TestOldSGDeclarationIsNotLoaded()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";

			GlbBranch sgbranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SIN");

			declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			declaration[JobDeclarationSchema.Constants.JE_GB] = sgbranch.PK;
			declaration[JobDeclarationSchema.Constants.JE_ApplicationCode] = "";

			Factory.Save();

			IList relatedObjects = shipment.DocManagerInfo.RelatedObjects;
			AssertEquals("Should not have have the sg3 declaration in the related business objects", false, relatedObjects.Contains(declaration));
		}

		public void TestNewSGDeclarationIsLoaded()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S00001234";

			GlbBranch sgbranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SIN");

			declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			declaration[JobDeclarationSchema.Constants.JE_GB] = sgbranch.PK;
			declaration[JobDeclarationSchema.Constants.JE_ApplicationCode] = "SG4";

			Factory.Save();

			IList relatedObjects = shipment.DocManagerInfo.RelatedObjects;
			AssertEquals("Should have have the sg4 declaration in the related business objects", true, relatedObjects.Contains(declaration));
		}

		OrgHeader debtor;
		AccTransactionHeader invoice;
		AccTransactionHeader nonShipmentInvoice;
		BusinessObject declaration;
		BusinessObject commInvoice;
		BusinessObject cusEntryHeader;
		BusinessObject declarationNZ;
		BusinessObject commInvoiceNZ;
		OrgHeader consignee;
		OrgHeader consignor;
		CommonConsol consol;
		CommonContainer container1;
		CommonContainer container2;
		BusinessObject rateHeader;
	}
}
