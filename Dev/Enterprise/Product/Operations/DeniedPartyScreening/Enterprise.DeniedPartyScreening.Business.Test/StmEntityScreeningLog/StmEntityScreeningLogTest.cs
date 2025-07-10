using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	[TestedType(typeof(StmEntityScreeningLog))]
	public class StmEntityScreeningLogTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestParentDescriptionNoNullReferenceException()
		{
			StmEntityScreeningLog stmEntityScreeningLog = Factory.New<StmEntityScreeningLog>();
			stmEntityScreeningLog.PJ_ParentID = ZGuid.NewZGuid();

			stmEntityScreeningLog.PJ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertEquals(ZString.Empty, stmEntityScreeningLog.ParentDescription);

			stmEntityScreeningLog.PJ_ParentTableCode = RefVesselSchema.Constants.Prefix;
			AssertEquals(ZString.Empty, stmEntityScreeningLog.ParentDescription);

			stmEntityScreeningLog.PJ_ParentTableCode = RefCountrySchema.Constants.Prefix;
			AssertEquals(ZString.Empty, stmEntityScreeningLog.ParentDescription);

			stmEntityScreeningLog.PJ_ParentTableCode = JobDocAddressSchema.Constants.Prefix;
			AssertEquals(ZString.Empty, stmEntityScreeningLog.ParentDescription);
		}

		public void TestConstraintsCauseExceptionPJ_ParentTableCode()
		{
			var stmEntityScreeningLog = Factory.New<StmEntityScreeningLog>();
			stmEntityScreeningLog.PJ_Sequence = 100;

			stmEntityScreeningLog.PJ_ParentTableCode = ZString.Empty;
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());

			stmEntityScreeningLog.PJ_ParentID = Guid.NewGuid();
			stmEntityScreeningLog.PJ_ParentTableCode = "E2";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_ParentTableCode = "JE";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_ParentTableCode = "JK";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_ParentTableCode = "JS";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_ParentTableCode = "JW";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_ParentTableCode = "OH";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_ParentTableCode = "RN";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_ParentTableCode = "RV";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_ParentTableCode = "TH";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_ParentTableCode = "WD";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_ParentTableCode = "JV";
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
		}

		public void TestConstraintsCauseExceptionPJ_SourceTableCode()
		{
			var stmEntityScreeningLog = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			stmEntityScreeningLog.PJ_Sequence = 100;
			stmEntityScreeningLog.PJ_ParentTableCode = "JE";
			stmEntityScreeningLog.PJ_SourceTableCode = ZString.Empty;
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_SourceID = Guid.NewGuid();
			stmEntityScreeningLog.PJ_SourceTableCode = "E2";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_SourceTableCode = "JE";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_SourceTableCode = "JK";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_SourceTableCode = "JS";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_SourceTableCode = "JW";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_SourceTableCode = "OH";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_SourceTableCode = "RN";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_SourceTableCode = "RV";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_SourceTableCode = "TH";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_SourceTableCode = "WD";
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_SourceTableCode = "JV";
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
		}

		public void TestConstraintsCauseExceptionPJ_SourceIDAndTableCode()
		{
			var stmEntityScreeningLog = Factory.New<StmEntityScreeningLog>();
			stmEntityScreeningLog.PJ_Sequence = 100;
			stmEntityScreeningLog.PJ_ParentTableCode = "JE";
			stmEntityScreeningLog.PJ_SourceTableCode = "JE";
			stmEntityScreeningLog.PJ_SourceID = Guid.NewGuid();
			AssertNoExceptionThrown(() => Factory.Save());

			stmEntityScreeningLog.PJ_SourceID = ZGuid.Empty;
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());

			stmEntityScreeningLog.PJ_SourceID = Guid.NewGuid();
			stmEntityScreeningLog.PJ_SourceTableCode = "";
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());

			stmEntityScreeningLog.PJ_SourceID = ZGuid.Empty;
			stmEntityScreeningLog.PJ_SourceTableCode = "";
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestParentDescriptionReturnsDesiredValues()
		{
			var org = (BusinessObject)Factory.New<IOrgHeader>();
			org[OrgHeaderSchema.OH_Code] = "EDDIE";
			org[OrgHeaderSchema.OH_FullName] = "Eddie's Org";

			var orgAddress = (BusinessObject)Factory.New<IOrgAddress>();
			orgAddress[OrgAddressSchema.OA_OH] = org.PK;
			orgAddress[OrgAddressSchema.OA_Address1] = "2 Main Street";
			orgAddress[OrgAddressSchema.OA_RN_NKCountryCode] = "AU";

			var orgAddressCapability = (BusinessObject)Factory.New<IOrgAddressCapability>();
			orgAddressCapability[OrgAddressCapabilitySchema.PZ_OA] = orgAddress.PK;
			orgAddressCapability[OrgAddressCapabilitySchema.PZ_IsMainAddress] = true;
			orgAddressCapability[OrgAddressCapabilitySchema.PZ_AddressType] = "OFC";

			var jobDocAddress = (BusinessObject)Factory.New<IJobDocAddress>();
			jobDocAddress[JobDocAddressSchema.E2_AddressOverride] = true;
			var shipment = Factory.New<IForwardingShipment>();
			jobDocAddress[JobDocAddressSchema.E2_ParentID] = shipment.PK;
			jobDocAddress[JobDocAddressSchema.E2_ParentTableCode] = "JS";

			var consol = Factory.New<IForwardingConsol>();

			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;

			var vessel = (BusinessObject)Factory.New<IRefVessel>();
			vessel[RefVesselSchema.RV_Code] = "ABJEXE SIMPLE";

			var country = (BusinessObject)Factory.New<IRefCountry>();
			country[RefCountrySchema.RN_Code] = "RF";
			country[RefCountrySchema.RN_Desc] = "Russia Federation";

			Factory.Save();

			StmEntityScreeningLog stmEntityScreeningLog = Factory.New<StmEntityScreeningLog>();
			stmEntityScreeningLog.PJ_ParentID = org.PK;

			stmEntityScreeningLog.PJ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;

			AssertEquals("EDDIE - Eddie's Org - AU", stmEntityScreeningLog.ParentDescription);

			stmEntityScreeningLog.PJ_ParentTableCode = RefVesselSchema.Constants.Prefix;
			stmEntityScreeningLog.PJ_ParentID = vessel.PK;
			AssertEquals("ABJEXE SIMPLE", stmEntityScreeningLog.ParentDescription);

			stmEntityScreeningLog.PJ_ParentTableCode = RefCountrySchema.Constants.Prefix;
			stmEntityScreeningLog.PJ_ParentID = country.PK;
			AssertEquals("Country/Region - Russia Federation", stmEntityScreeningLog.ParentDescription);

			stmEntityScreeningLog.PJ_ParentTableCode = JobDocAddressSchema.Constants.Prefix;
			stmEntityScreeningLog.PJ_ParentID = jobDocAddress.PK;
			AssertEquals("Shipment S00001000 - ?", stmEntityScreeningLog.ParentDescription);

			stmEntityScreeningLog.PJ_ParentID = shipment.PK;
			stmEntityScreeningLog.PJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertEquals($"Shipment {shipment.JS_UniqueConsignRef}", stmEntityScreeningLog.ParentDescription);

			stmEntityScreeningLog.PJ_ParentID = consol.PK;
			stmEntityScreeningLog.PJ_ParentTableCode = JobConsolSchema.Constants.Prefix;
			AssertEquals($"Consol {consol.JK_UniqueConsignRef}", stmEntityScreeningLog.ParentDescription);

			stmEntityScreeningLog.PJ_ParentID = declaration.PK;
			stmEntityScreeningLog.PJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			AssertEquals($"Declaration {declaration.JE_DeclarationReference}", stmEntityScreeningLog.ParentDescription);
		}

		public void TestLocalScreenDate()
		{
			var stmEntityScreeningLog = Factory.New<StmEntityScreeningLog>();
			stmEntityScreeningLog.PJ_SystemCreateTimeUtc = ZDateTime.UtcNow;

			AssertEquals(stmEntityScreeningLog.PJ_SystemCreateTimeUtc.ToLocalBranchTime(), stmEntityScreeningLog.PJ_ScreenDate);
		}

		public void TestPJ_SourceInformation()
		{
			var org = (BusinessObject)Factory.New<IOrgHeader>();
			org[OrgHeaderSchema.OH_Code] = "TESTORG";

			var shipment = Factory.New<IForwardingShipment>();

			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_JS = shipment.PK;

			var consol = Factory.New<IForwardingConsol>();

			var vessel = (BusinessObject)Factory.New<IRefVessel>();
			vessel[RefVesselSchema.RV_Code] = "ABJEXE SIMPLE";

			var country = (BusinessObject)Factory.New<IRefCountry>();
			country[RefCountrySchema.RN_Code] = "RF";
			country[RefCountrySchema.RN_Desc] = "Russia Federation";

			Factory.Save();

			var stmEntityScreeningLog = Factory.New<StmEntityScreeningLog>();
			AssertEquals("Not Recorded", stmEntityScreeningLog.SourceInformation);

			stmEntityScreeningLog.PJ_SourceTableCode = OrgHeaderSchema.Constants.Prefix;
			stmEntityScreeningLog.PJ_SourceID = org.PK;
			AssertEquals("Org. TESTORG", stmEntityScreeningLog.SourceInformation);

			stmEntityScreeningLog.PJ_SourceID = ZGuid.NewZGuid();
			AssertEquals("Source Not Exist", stmEntityScreeningLog.SourceInformation);

			stmEntityScreeningLog.PJ_SourceTableCode = RefVesselSchema.Constants.Prefix;
			stmEntityScreeningLog.PJ_SourceID = vessel.PK;
			AssertEquals("Vessel ABJEXE SIMPLE", stmEntityScreeningLog.SourceInformation);

			stmEntityScreeningLog.PJ_SourceTableCode = RefCountrySchema.Constants.Prefix;
			stmEntityScreeningLog.PJ_SourceID = country.PK;
			AssertEquals("Country/Region: RF - Russia Federation", stmEntityScreeningLog.SourceInformation);

			stmEntityScreeningLog.PJ_SourceTableCode = JobShipmentSchema.Constants.Prefix;
			stmEntityScreeningLog.PJ_SourceID = shipment.PK;
			AssertEquals($"Job {shipment.JS_UniqueConsignRef} - Screen", stmEntityScreeningLog.SourceInformation);

			stmEntityScreeningLog.PJ_IsForcedRescreen = true;
			AssertEquals($"Job {shipment.JS_UniqueConsignRef} - Force Re-Screen", stmEntityScreeningLog.SourceInformation);

			stmEntityScreeningLog.PJ_IsForcedRescreen = false;
			stmEntityScreeningLog.PJ_SourceTableCode = JobDeclarationSchema.Constants.Prefix;
			stmEntityScreeningLog.PJ_SourceID = declaration.PK;
			AssertEquals($"Job {declaration.JE_DeclarationReference} - Screen", stmEntityScreeningLog.SourceInformation);

			stmEntityScreeningLog.PJ_SourceTableCode = JobConsolSchema.Constants.Prefix;
			stmEntityScreeningLog.PJ_SourceID = consol.PK;
			AssertEquals($"Job {consol.JK_UniqueConsignRef} - Screen", stmEntityScreeningLog.SourceInformation);

			stmEntityScreeningLog.PJ_SourceTableCode = JobDocAddressSchema.Constants.Prefix;
			stmEntityScreeningLog.PJ_SourceID = ZGuid.NewZGuid();
			AssertEquals("Job Doc Address", stmEntityScreeningLog.SourceInformation);

			stmEntityScreeningLog.PJ_SourceTableCode = ZString.Empty;
			stmEntityScreeningLog.PJ_SourceID = ZGuid.NewZGuid();
			AssertEquals("Not Recorded", stmEntityScreeningLog.SourceInformation);
		}

		#region ClearedReason

		public void TestClearedReason()
		{
			StmEntityScreeningLog stmEntityScreeningLog = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			stmEntityScreeningLog.PJ_ClearedReason = "There are no good reasons...";
			stmEntityScreeningLog.PJ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedObject = newFactory.Load<StmEntityScreeningLog>(stmEntityScreeningLog.PK);

			AssertEquals("Reason should be saved correctly", stmEntityScreeningLog.PJ_ClearedReason, loadedObject.PJ_ClearedReason);
		}

		public void TestClearedReason_DefaultValue()
		{
			StmEntityScreeningLog stmEntityScreeningLog = Factory.NewWithValidTestData<StmEntityScreeningLog>();

			AssertEquals("Reason should have correct default", "Not Requested", stmEntityScreeningLog.PJ_ClearedReason);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return StmEntityScreeningLogCollectionProviderForTest.GetCollection(Factory).AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var item = StmEntityScreeningLogCollectionProviderForTest.GetCollection(factory).AddNew();
			item.FillWithValidTestData();
			return item;
		}

		#endregion
	}
}
