using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ImportJobDeclaration))]
	sealed class ImportJobDeclarationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBrokerToPayIndicator()
		{
			GlbCompany.CurrentCompany.SetCountry("TW");

			var twCompany = Factory.New<GlbCompany>();
			twCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			twCompany.GC_Code = "CTW";
			var twBranch = twCompany.Branches.AddNew();
			twBranch.GB_RL_NKHomePort = "TWTPE";
			twBranch.GB_Code = "BTW";

			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany.GC_Code = "CUS";
			var usBranch = twCompany.Branches.AddNew();
			usBranch.GB_RL_NKHomePort = "USCHI";
			usBranch.GB_Code = "BUS";

			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			supplier.OH_IsConsignor = true;
			supplier.OH_RL_NKClosestPort = "TWTPE";

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_IsConsignee = true;
			importer.OH_RL_NKClosestPort = "USCHI";
			var countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = importer.PK;
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;

			var addInfo = new OrgImpAddInfo((ZPropertyInfoString)countryData.OV_ImportCustomsDefaultAddInfoInfo);
			addInfo.ZO_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			addInfo.ZO_BrokerToPay = YesNoDefaultList.Codes.No;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "TWTPE";
			consol.JK_RL_NKDischargePort = "USCHI";

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = shipment.PK;
			declaration.JE_GB = twBranch.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_TotalNoOfPacks = 1;

			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("US");

			var newFactory = new BusinessObjectFactory();
			var decImporter = new ImportJobDeclaration(newFactory);
			decImporter.LoadDeclarationForShipment(shipment.JS_UniqueConsignRef);
			var resultDec = decImporter.CreateDeclarationAgainstShipment() as JobDeclaration;

			AssertEquals("Test BrokerToPayIndicator", "N", resultDec.BrokerToPayIndicator);
		}
	}
}
