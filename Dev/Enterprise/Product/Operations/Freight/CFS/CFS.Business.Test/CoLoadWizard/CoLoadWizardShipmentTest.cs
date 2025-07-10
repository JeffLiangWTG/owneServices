using System.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CoLoadWizardShipment))]
	public class CoLoadWizardShipmentTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestCopyValuesToShipment_NoMaxLengthExceededException()
		{
			PackUnpackShipment shipment = Factory.New<PackUnpackShipment>();
			TestCoLoadWizardShipment testWizardShipment = new TestCoLoadWizardShipment(shipment);

			((IBusinessObjectInternals)testWizardShipment).IsCopying = true;
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(testWizardShipment))
			{
				if (property.PropertyType == typeof(ZString))
				{
					ZPropertyInfo propertyInfo = null;
					try
					{
						propertyInfo = testWizardShipment.ZPropertyInfoHash[property.Name];
					}
					catch { }
					if (propertyInfo != null)
					{
						property.SetValue(testWizardShipment, new ZString('x', propertyInfo.MaxLength > 0 ? propertyInfo.MaxLength : 500));
					}
				}
			}
			((IBusinessObjectInternals)testWizardShipment).IsCopying = false;

			// expect no max-length exceptions
			testWizardShipment.CopyValuesToShipment(shipment);
		}

		public void TestExistance()
		{
			CoLoadWizardShipment testWizardShipment = new CoLoadWizardShipment(CoLoadShipment);
			AssertEquals("Co-Load Forwarder", CoLoadShipment.ConsigneePK, testWizardShipment.CW_OH_CoLoadForwarder);
		}

		public void TestColoadWizardStep_WelcomeStep()
		{
			CoLoadWizardShipment testWizardShipment = new CoLoadWizardShipment(CoLoadShipment);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.WelcomeStep, CoLoadWizardSteps.UltimateDetails));
			AssertEquals("No CoLoadShipments should be in the collection yet", 0, testWizardShipment.CoLoadShipments.Count);
		}

		public void TestColoadWizardStep_UltimateDetails()
		{
			CoLoadWizardShipment testWizardShipment = new CoLoadWizardShipment(CoLoadShipment);
			testWizardShipment.CW_EnterConsignor = false;
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.WelcomeStep, CoLoadWizardSteps.UltimateDetails));
			FillUltimateDetails(testWizardShipment);
			WizardSteppingEventArgs eventArgs = new WizardSteppingEventArgs(CoLoadWizardSteps.UltimateDetails, CoLoadWizardSteps.ConsignorDetails);
			testWizardShipment.OnStepping(eventArgs);
			AssertEquals("Next page should be consignee if enter consignor is false", CoLoadWizardSteps.ConsigneeDetails, eventArgs.NextStep);
			eventArgs = new WizardSteppingEventArgs(CoLoadWizardSteps.ConsigneeDetails, CoLoadWizardSteps.AddConsignor);
			testWizardShipment.OnStepping(eventArgs);
			AssertEquals("Previous page should be ultimate Details", CoLoadWizardSteps.UltimateDetails, eventArgs.NextStep);
			testWizardShipment.CW_EnterConsignor = true;
			eventArgs = new WizardSteppingEventArgs(CoLoadWizardSteps.UltimateDetails, CoLoadWizardSteps.ConsignorDetails);
			testWizardShipment.OnStepping(eventArgs);
			AssertEquals("Next page should be consignee if enter consignor is true", CoLoadWizardSteps.ConsignorDetails, eventArgs.NextStep);
		}

		public void TestColoadWizardStep_ConsignorDetails()
		{
			CoLoadWizardShipment testWizardShipment = new CoLoadWizardShipment(CoLoadShipment);
			testWizardShipment.CW_EnterConsignor = true;
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.WelcomeStep, CoLoadWizardSteps.UltimateDetails));
			FillUltimateDetails(testWizardShipment);
			WizardSteppingEventArgs eventArgs = new WizardSteppingEventArgs(CoLoadWizardSteps.UltimateDetails, CoLoadWizardSteps.ConsignorDetails);
			testWizardShipment.OnStepping(eventArgs);
			AssertEquals("Next page should be Consignor if enter consignor is false", CoLoadWizardSteps.ConsignorDetails, eventArgs.NextStep);
			eventArgs = new WizardSteppingEventArgs(CoLoadWizardSteps.ConsigneeDetails, CoLoadWizardSteps.UltimateDetails);
			testWizardShipment.OnStepping(eventArgs);
			AssertEquals("Previous page should be ultimate Details", CoLoadWizardSteps.AddConsignor, eventArgs.NextStep);
			testWizardShipment.CW_EnterConsignor = false;
			eventArgs = new WizardSteppingEventArgs(CoLoadWizardSteps.UltimateDetails, CoLoadWizardSteps.ConsigneeDetails);
			testWizardShipment.OnStepping(eventArgs);
			AssertEquals("Next page should be consignee if enter consignor is true", CoLoadWizardSteps.ConsigneeDetails, eventArgs.NextStep);
		}

		public void TestColoadWizardStep_AddConsignor()
		{
			CoLoadWizardShipment testWizardShipment = new CoLoadWizardShipment(CoLoadShipment);
			testWizardShipment.CW_EnterConsignor = true;
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.WelcomeStep, CoLoadWizardSteps.UltimateDetails));
			FillUltimateDetails(testWizardShipment);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.UltimateDetails, CoLoadWizardSteps.ConsignorDetails));
			FillConsignorDetails(testWizardShipment);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.ConsignorDetails, CoLoadWizardSteps.AddConsignee));
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.ConsigneeDetails, CoLoadWizardSteps.AddConsignee));
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.AddConsignee, CoLoadWizardSteps.AddColoadShipment));
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.AddConsignee, CoLoadWizardSteps.AddColoadShipment));
			Assert("Consignee should be set", ZGuid.Empty != testWizardShipment.CW_OH_Consignor);
		}

		public void TestColoadWizardStep_ConsigneeDetails()
		{
			CoLoadWizardShipment testWizardShipment = new CoLoadWizardShipment(CoLoadShipment);
			testWizardShipment.CW_EnterConsignor = false;
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.WelcomeStep, CoLoadWizardSteps.UltimateDetails));
			FillUltimateDetails(testWizardShipment);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.UltimateDetails, CoLoadWizardSteps.ConsignorDetails));
			FillConsigneeDetails(testWizardShipment);
			// Enter Consignee Details that do not match anything in the database
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.ConsigneeDetails, CoLoadWizardSteps.AddConsignee));
			AssertEquals("Temporary Org should be checked", true, testWizardShipment.CW_TempOrgMarkTemporary);
			AssertEquals("Create New Org should be checked", true, testWizardShipment.CW_TempOrgCreateNew);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.AddConsignee, CoLoadWizardSteps.ConsignorDetails));
			testWizardShipment.CW_OH_Consignee = GetConsigneePK();
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.ConsigneeDetails, CoLoadWizardSteps.AddConsignee));
			AssertEquals("Temporary Org should not be checked", false, testWizardShipment.CW_TempOrgMarkTemporary);
			AssertEquals("Create New Org should not be checked", false, testWizardShipment.CW_TempOrgCreateNew);
		}

		public void TestColoadWizardStep_AddConsignee()
		{
			CoLoadWizardShipment testWizardShipment = new CoLoadWizardShipment(CoLoadShipment);
			testWizardShipment.CW_EnterConsignor = false;
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.WelcomeStep, CoLoadWizardSteps.UltimateDetails));
			FillUltimateDetails(testWizardShipment);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.UltimateDetails, CoLoadWizardSteps.ConsignorDetails));
			FillConsigneeDetails(testWizardShipment);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.ConsigneeDetails, CoLoadWizardSteps.AddConsignee));

			testWizardShipment.CW_OH_Consignee = ZGuid.Empty;
			testWizardShipment.CW_OH_Consignee = ZGuid.Invalid;
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.AddConsignee, CoLoadWizardSteps.ConsigneeDetails));

			Assert("Consignee should not be set", ZGuid.Empty != testWizardShipment.CW_OH_Consignee);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.ConsigneeDetails, CoLoadWizardSteps.AddConsignee));

			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.AddConsignee, CoLoadWizardSteps.AddColoadShipment));
			Assert("Consignee should be set", ZGuid.Empty != testWizardShipment.CW_OH_Consignee);
		}

		public void TestColoadWizardStep_AddColoadShipment()
		{
			CoLoadWizardShipment testWizardShipment = new CoLoadWizardShipment(CoLoadShipment);
			testWizardShipment.CW_EnterConsignor = false;
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.WelcomeStep, CoLoadWizardSteps.UltimateDetails));
			FillUltimateDetails(testWizardShipment);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.UltimateDetails, CoLoadWizardSteps.ConsignorDetails));
			FillConsigneeDetails(testWizardShipment);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.ConsigneeDetails, CoLoadWizardSteps.AddConsignee));
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.AddConsignee, CoLoadWizardSteps.AddColoadShipment));

			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.AddColoadShipment, CoLoadWizardSteps.AnotherShipment));
			AssertEquals("1 Co Load shipment should now exist", 1, testWizardShipment.CoLoadShipments.Count);
		}

		public void TestColoadWizardStep_AnotherShipment()
		{
			CoLoadWizardShipment testWizardShipment = new CoLoadWizardShipment(CoLoadShipment);
			testWizardShipment.CW_EnterConsignor = false;
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.WelcomeStep, CoLoadWizardSteps.UltimateDetails));
			FillUltimateDetails(testWizardShipment);

			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.UltimateDetails, CoLoadWizardSteps.ConsignorDetails));
			// Enter Consignee Details that do not match anything in the database
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.ConsigneeDetails, CoLoadWizardSteps.AddConsignee));
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.AddConsignee, CoLoadWizardSteps.AddColoadShipment));

			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.AddConsignee, CoLoadWizardSteps.AnotherShipment));
			testWizardShipment.CW_CreateAnotherShipment = true;
			WizardSteppingEventArgs args = new WizardSteppingEventArgs(CoLoadWizardSteps.AnotherShipment, CoLoadWizardSteps.FinishWizard);
			testWizardShipment.OnStepping(args);
			AssertEquals("Expected next step should be UltimateDetails", CoLoadWizardSteps.UltimateDetails, args.NextStep);
			AssertEquals("Ultimate Details should be cleared", "", testWizardShipment.CW_HouseBill);
			args.NextStep = CoLoadWizardSteps.FinishWizard;
			testWizardShipment.CW_CreateAnotherShipment = false;
			testWizardShipment.OnStepping(args);
			AssertEquals("Should be at the end of the wizard now", CoLoadWizardSteps.FinishWizard, args.NextStep);
		}

		public void TestDefaultingOfShipmentDetails()
		{
			CoLoadWizardShipment testWizardShipment = new CoLoadWizardShipment(CoLoadShipment);
			testWizardShipment.CW_EnterConsignor = false;
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.WelcomeStep, CoLoadWizardSteps.UltimateDetails));
			FillUltimateDetails(testWizardShipment);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.UltimateDetails, CoLoadWizardSteps.ConsignorDetails));
			// Enter Consignee Details that do not match anything in the database
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.ConsigneeDetails, CoLoadWizardSteps.AddConsignee));
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.AddConsignee, CoLoadWizardSteps.AddColoadShipment));

			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.AddColoadShipment, CoLoadWizardSteps.AnotherShipment));
			testWizardShipment.CW_CreateAnotherShipment = true;
			WizardSteppingEventArgs args = new WizardSteppingEventArgs(CoLoadWizardSteps.AnotherShipment, CoLoadWizardSteps.FinishWizard);
			testWizardShipment.OnStepping(args);
			AssertEquals("PreCondition : Expected step should be UltimateDetails", CoLoadWizardSteps.UltimateDetails, args.NextStep);
			AssertEquals("TestWizard HouseBill 2nd time", "", testWizardShipment.CW_HouseBill);
			AssertEquals("TestWizard Weight", 3997.1m, testWizardShipment.CW_Weight);
			AssertEquals("TestWizard Volume", 4m, testWizardShipment.CW_Volume);
			AssertEquals("TestWizard No Packs", 30, testWizardShipment.CW_NumberOfPackages);
		}

		public void TestAttachingExistingAutoCreatedShipment()
		{
			AddAutoCreatedCoLoads(CoLoadShipment.ConsigneePK);
			Factory.Save();

			CoLoadWizardShipment testWizardShipment = new CoLoadWizardShipment(CoLoadShipment);
			testWizardShipment.CW_EnterConsignor = false;
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.WelcomeStep, CoLoadWizardSteps.UltimateDetails));
			FillSubHouse1UltimateDetails(testWizardShipment);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.UltimateDetails, CoLoadWizardSteps.ConsignorDetails));
			FillConsigneeDetails(testWizardShipment);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.ConsigneeDetails, CoLoadWizardSteps.AddConsignee));
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.AddConsignee, CoLoadWizardSteps.AddColoadShipment));
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.AddColoadShipment, CoLoadWizardSteps.AnotherShipment));
			AssertEquals("Sub house should already be in the database and should have been taken over", true, testWizardShipment.CoLoadShipments[0].IsInDatabase);
			AssertEquals("Sub house should not be forward registered", false, testWizardShipment.CoLoadShipments[0].JS_IsForwardRegistered);
		}

		public void TestSimilarOrganisationsNotLoadedFromDefaultValues()
		{
			Db.Connection.ExecuteNonQuery("DELETE from dbo.OrgPatternMatch");

			OrgHeader similarOrg = AddSimilarOrg();
			AssertEquals("Similar Org Pattern Match", 1, Factory.GetDatabaseCount(typeof(OrgPatternMatch), new ZQuery(OrgPatternMatchSchema.OS_OH, similarOrg.PK)));
			CoLoadWizardShipment testWizardShipment = new CoLoadWizardShipment(CoLoadShipment);
			testWizardShipment.CW_EnterConsignor = false;
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.WelcomeStep, CoLoadWizardSteps.UltimateDetails));
			FillUltimateDetails(testWizardShipment);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.UltimateDetails, CoLoadWizardSteps.ConsignorDetails));
			AssertEquals("Co Load Wizard Shipment UNLOCO", "AUSYD", CoLoadShipment.JS_RL_NKDestination);
			AssertEquals("Similar Organisations Count", 0, testWizardShipment.SimilarOrganisations.Count);

			testWizardShipment.CW_TempOrgName = "Connie Consignee";
			testWizardShipment.CW_TempOrgAddress1 = "453 Pittwater Rd";
			testWizardShipment.CW_TempOrgCity = "Brookvale";
			testWizardShipment.CW_TempOrgState = "NSW";
			testWizardShipment.CW_TempOrgPostcode = "2097";

			AssertEquals("Similar Organisations Count", 1, testWizardShipment.SimilarOrganisations.Count);
		}

		public void TestCW_HouseBillValidation_ValueBelongsToParentShipment_AddError()
		{
			var parentShipment = Factory.NewWithValidTestData<PackUnpackShipment>();
			parentShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			parentShipment.JS_HouseBill = "MatchesParent";
			var arrivalConsol = parentShipment.Consols.AddNew();

			var coLoadShipment = new CoLoadWizardShipment(parentShipment);
			coLoadShipment.CW_HouseBill = "MatchesParent";

			AssertHasError(coLoadShipment.CW_HouseBillInfo, "The House bill 'MatchesParent' belongs to the selected co-load master shipment.");
		}

		public void TestCW_HouseBillValidation_ValueBelongsToAnotherCoLoadMasterShipment_AddError()
		{
			var parentShipment = Factory.NewWithValidTestData<PackUnpackShipment>();
			parentShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			var arrivalConsol = parentShipment.Consols.AddNew();
			var shipment = arrivalConsol.Shipments.AddNew();
			shipment.JS_HouseBill = "MatchesAnother";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment.CoLoadShipments.AddNew();

			var coLoadShipment = new CoLoadWizardShipment(parentShipment);
			coLoadShipment.CW_HouseBill = "MatchesAnother";

			AssertHasError(coLoadShipment.CW_HouseBillInfo, "The House bill 'MatchesAnother' belongs to another co-load master shipment.");
		}

		public void TestCW_MarksAndNumbers()
		{
			PackUnpackShipment shipment = Factory.New<PackUnpackShipment>();
			shipment.JS_MarksAndNumbers = "A bunch of characters to exceed the maximum string limits and thus should be shortened to a single XXXXXXXXXXXXXXXXX";

			TestCoLoadWizardShipment testWizardShipment = new TestCoLoadWizardShipment(shipment);
			ZString testText = shipment.JS_MarksAndNumbers.Left(testWizardShipment.CW_MarksAndNumbersInfo.MaxLength);
			testWizardShipment.OnStepping(new WizardSteppingEventArgs(CoLoadWizardSteps.WelcomeStep, CoLoadWizardSteps.UltimateDetails));
			AssertEquals("marks and numbers field within word limit", testText, testWizardShipment.CW_MarksAndNumbers);
		}

		public void TestCW_HouseBillMaxLength()
		{
			var testWizardShipment = new CoLoadWizardShipment(CoLoadShipment);
			AssertEquals("CW_HouseBill max length", AutoJobShipment.Schema.JS_HouseBillMaxLength, testWizardShipment.CW_HouseBillInfo.MaxLength);
		}

		#region Implementation

		PackUnpackShipment CoLoadShipment;

		const string SubHouse1 = "SUBH1";
		const string SubHouse2 = "SUBH2";
		const string SubHouse3 = "F8833928";

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CoLoadWizardShipment(CoLoadShipment);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CoLoadShipment = Factory.New<PackUnpackShipment>();
			CoLoadShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			CoLoadShipment.ConsigneePK = GetCoLoadForwarder();
			CoLoadShipment.JS_UnitOfWeight = "KG";
			CoLoadShipment.JS_UnitOfVolume = "M3";
			CoLoadShipment.JS_HouseBill = "COLOADMASTER001";
			CoLoadShipment.JS_ActualVolume = 8.2m;
			CoLoadShipment.JS_ActualWeight = 8000.4m;
			CoLoadShipment.JS_OuterPacks = 50;
			CoLoadShipment.JS_RL_NKDestination = "AUSYD";
			CoLoadShipment.JS_RL_NKOrigin = "USLAX";

			CommonConsol consol = CoLoadShipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = "AUSYD";

			consol.Shipments.Add(CoLoadShipment);
		}

		protected OrgHeader AddSimilarOrg()
		{
			OrgHeader result = AddOrgHeader("Conny Colsigner", "453 Pittwater Rd", "Brookvale", "NSW", "2097", "AUSYD");
			Factory.Save();
			return result;
		}

		OrgHeader AddOrgHeader(ZString name, ZString address1, ZString city, ZString state, ZString postCode, ZString uNLOCO)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = name;
			result.MainAddress.OA_Address1 = address1;
			result.MainAddress.OA_City = city;
			result.MainAddress.OA_State = state;
			result.MainAddress.OA_PostCode = postCode;
			result.OH_RL_NKClosestPort = uNLOCO;
			return result;
		}

		protected ZGuid GetCoLoadForwarder()
		{
			ZQuery coLoadFilter = new ZQuery(OrgHeaderSchema.OH_IsForwarder, ZBool.True);
			var result = Factory.LoadTop1<OrgHeader>(coLoadFilter);
			return result.PK;
		}

		protected void FillUltimateDetails(CoLoadWizardShipment testWizardShipment)
		{
			testWizardShipment.CW_HouseBill = "SYD-220102";
			testWizardShipment.CW_NumberOfPackages = 20;
			testWizardShipment.CW_Weight = 4003.3m;
			testWizardShipment.CW_WeightUQ = "KG";
			testWizardShipment.CW_Volume = 4.2m;
			testWizardShipment.CW_VolumeUQ = "M3";
			testWizardShipment.CW_GoodsDescription = "Goods";
			testWizardShipment.CW_MarksAndNumbers = "Marks";
		}

		protected void FillSubHouse1UltimateDetails(CoLoadWizardShipment testWizardShipment)
		{
			testWizardShipment.CW_HouseBill = SubHouse1;
			testWizardShipment.CW_NumberOfPackages = 7;
			testWizardShipment.CW_Weight = 4003.3m;
			testWizardShipment.CW_WeightUQ = "KG";
			testWizardShipment.CW_Volume = 4.2m;
			testWizardShipment.CW_VolumeUQ = "M3";
			testWizardShipment.CW_GoodsDescription = "Goods";
			testWizardShipment.CW_MarksAndNumbers = "Marks";
		}

		protected void FillConsigneeDetails(CoLoadWizardShipment testWizardShipment)
		{
			testWizardShipment.CW_TempOrgName = "Connie Consignee";
			testWizardShipment.CW_TempOrgAddress1 = "451 Pittwater Rd";
			testWizardShipment.CW_TempOrgAddress2 = "Level 2";
			testWizardShipment.CW_TempOrgCity = "Manly";
			testWizardShipment.CW_TempOrgState = "NSW";
			testWizardShipment.CW_TempOrgPostcode = "2095";
			testWizardShipment.CW_TempOrgUNLOCO = "AUSYD";
		}

		protected void FillConsignorDetails(CoLoadWizardShipment testWizardShipment)
		{
			testWizardShipment.CW_TempOrgName = "Corrie Consignor";
			testWizardShipment.CW_TempOrgAddress1 = "45 Look Ave";
			testWizardShipment.CW_TempOrgCity = "Los Angeles";
			testWizardShipment.CW_TempOrgState = "CA";
			testWizardShipment.CW_TempOrgPostcode = "90245";
			testWizardShipment.CW_TempOrgUNLOCO = "USLAX";
		}

		protected ZGuid GetConsigneePK()
		{
			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, ZBool.True));
			return consignee.PK;
		}

		protected void AddAutoCreatedCoLoads(ZGuid forwarder)
		{
			CoLoadShipment.ArrivalConsol.Shipments.Add(CreateAutogeneratedShipment(SubHouse1, 7, forwarder));
			CoLoadShipment.ArrivalConsol.Shipments.Add(CreateAutogeneratedShipment(SubHouse2, 15, forwarder));
			CoLoadShipment.ArrivalConsol.Shipments.Add(CreateAutogeneratedShipment(SubHouse3, 8, forwarder));
		}

		protected PackUnpackShipment CreateAutogeneratedShipment(string houseBill, int numberPacks, ZGuid forwarder)
		{
			PackUnpackShipment result = Factory.New<PackUnpackShipment>();
			result.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			result.JS_OH_HandledOnBehalfOfForwarder = forwarder;
			result.JS_HouseBill = houseBill;
			result.JS_OuterPacks = numberPacks;
			result.JS_RL_NKDestination = "AUSYD";
			result.JS_RL_NKOrigin = "USLAX";
			return result;
		}

		protected class TestCoLoadWizardShipment : CoLoadWizardShipment
		{
			public TestCoLoadWizardShipment(PackUnpackShipment parentShipment) : base(parentShipment)
			{
			}

			public new void CopyValuesToShipment(PackUnpackShipment shipment)
			{
				base.CopyValuesToShipment(shipment);
			}
		}

		#endregion
	}
}
