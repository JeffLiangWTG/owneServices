using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefAirline))]
	sealed class RefAirlineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsActiveFilter()
		{
			var airline1 = Factory.New<RefAirline>();
			airline1.RM_AirlinePrefix = "_1";
			airline1.RM_AirlineName1 = "Test Airline Name 1";
			airline1.RM_TwoCharacterCode = "XZ";
			airline1.RM_IsActive = false;

			var airline2 = Factory.New<RefAirline>();
			airline2.RM_AirlinePrefix = "_2";
			airline2.RM_AirlineName1 = "Test Airline Name 2";
			airline2.RM_TwoCharacterCode = "XZ";
			airline2.RM_IsActive = true;

			var airline3 = Factory.New<RefAirline>();
			airline3.RM_AirlineName1 = "Test Airline Name 3";
			airline3.RM_AirlinePrefix = "_3";
			airline3.RM_TwoCharacterCode = "XZ";
			airline3.RM_IsActive = true;

			var airline4 = Factory.New<RefAirline>();
			airline4.RM_AirlineName1 = "Test Airline Name 4";
			airline4.RM_AirlinePrefix = "_4";
			airline4.RM_TwoCharacterCode = "XY";
			airline4.RM_IsActive = true;

			Factory.Save();

			var airlines = Factory.Load<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, "XZ"));

			AssertCollectionContains(airline2, airlines);
			AssertCollectionContains(airline3, airlines);
			AssertCollectionNotContains(airline1, airlines);
			AssertCollectionNotContains(airline4, airlines);
		}

		public void TestHasSignedEAWBAgreement()
		{
			var airline1 = Factory.New<RefAirline>();
			airline1.RM_AirlinePrefix = "_1";
			airline1.RM_AirlineName1 = "Test Airline Name 1";

			var airline2 = Factory.New<RefAirline>();
			airline2.RM_AirlinePrefix = "_2";
			airline2.RM_AirlineName1 = "Test Airline Name 2";

			airline1.HasSignedEAWBAgreement = true;
			AssertEquals("airline1.HasSignedEAWBAgreement", true, airline1.HasSignedEAWBAgreement);
			AssertEquals("airline1.HasChanges", true, airline1.HasChanges);
			Factory.Save();

			var secondFactory = new BusinessObjectFactory();

			var reloadedAirline1 = secondFactory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_AirlinePrefix, "_1"));
			AssertEquals("reloadedAirline1.HasSignedEAWBAgreement", true, reloadedAirline1.HasSignedEAWBAgreement);
			AssertEquals("reloadedAirline1.HasChanges", false, reloadedAirline1.HasChanges);

			var reloadedAirline2 = secondFactory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_AirlinePrefix, "_2"));
			AssertEquals("reloadedAirline2.HasSignedEAWBAgreement", false, reloadedAirline2.HasSignedEAWBAgreement);
			AssertEquals("reloadedAirline2.HasChanges", false, reloadedAirline2.HasChanges);

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = newCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(User.SupportUserName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var thirdFactory = new BusinessObjectFactory();

				var rereloadedAirline1 = thirdFactory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_AirlinePrefix, "_1"));
				AssertEquals("rereloadedAirline1.HasSignedEAWBAgreement", false, rereloadedAirline1.HasSignedEAWBAgreement);
				AssertEquals("rereloadedAirline1.HasChanges", false, rereloadedAirline1.HasChanges);

				var rereloadedAirline2 = thirdFactory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_AirlinePrefix, "_2"));
				AssertEquals("rereloadedAirline2.HasSignedEAWBAgreement", false, rereloadedAirline2.HasSignedEAWBAgreement);
				AssertEquals("rereloadedAirline2.HasChanges", false, rereloadedAirline2.HasChanges);
			}
		}

		public void TestLoadFromAirlinePrefix()
		{
			CreateRefAirlines();
			AssertEquals("QF", RefAirline.LoadFromAirlinePrefix(Factory, "081").RM_TwoCharacterCode);
			AssertNull(RefAirline.LoadFromAirlinePrefix(Factory, "XXX"));
		}

		[ExpectNoExceptions]
		public void TestLoadFromAirlinePrefixConflict()
		{
			RefAirline airline1 = Factory.New<RefAirline>();
			RefAirline airline2 = Factory.New<RefAirline>();
			RefAirline airline3 = Factory.New<RefAirline>();

			airline1.RM_TwoCharacterCode = "~1";
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "0XX";
			airline1.RM_AirlineName1 = "Test Airline Name 1";
			airline2.RM_TwoCharacterCode = "~2";
			airline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "0XX";
			airline2.RM_IsCASSControlled = true;
			airline2.RM_AirlineName1 = "Test Airline Name 2";
			airline2.RM_IsActive = false;
			airline3.RM_TwoCharacterCode = "~3";
			airline3.RM_EagleAddedAirlinePrefixOrAccountingCode = "0XX";
			airline3.RM_AirlineName1 = "Test Airline Name 3";
			airline3.RM_IsActive = false;

			Factory.Save();

			var loadedAirline = RefAirline.LoadFromAirlinePrefix(Factory, "0XX");
			AssertEquals("Should get Active Airline first.", airline1.RM_TwoCharacterCode, loadedAirline.RM_TwoCharacterCode);
		}

		public void TestLoadFromAirline2LetterCode()
		{
			CreateRefAirlines();
			var airline1 = Factory.NewWithValidTestData<RefAirline>();
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "0XX";
			airline1.RM_TwoCharacterCode = "XX";
			airline1.RM_IsActive = false;

			AssertEquals("081", RefAirline.LoadFromAirline2LetterCode(Factory, "QF").RM_EagleAddedAirlinePrefixOrAccountingCode);
			AssertEquals("618", RefAirline.LoadFromAirline2LetterCode(Factory, "SQ").RM_EagleAddedAirlinePrefixOrAccountingCode);
			AssertNull(RefAirline.LoadFromAirline2LetterCode(Factory, "XX"));
		}

		public void TestIsValidAirline2LetterCode()
		{
			CreateRefAirlines();
			var airline1 = Factory.NewWithValidTestData<RefAirline>();
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "0XX";
			airline1.RM_TwoCharacterCode = "XX";
			airline1.RM_IsActive = false;
			AssertEquals("Invalid Airline code", false, RefAirline.IsValidAirline2LetterCode(Factory, "XX"));

			AssertNotNull(RefAirline.LoadFromAirline2LetterCode(Factory, "SQ"));
			AssertEquals("Standard Singapore Airlines", true, RefAirline.IsValidAirline2LetterCode(Factory, "SQ"));

			var sqFreight = Factory.NewWithValidTestData<RefAirline>();
			sqFreight.RM_TwoCharacterCode = "SQ";
			sqFreight.RM_ThreeLetterCode = "SQC";
			sqFreight.RM_AirlineName1 = "Singapore Airlines Cargo Pte. Ltd.";
			sqFreight.RM_AddressLine1 = "30 Airline Road";
			sqFreight.RM_AddressLine2 = "05-J SATS Airfreight Terminal 5";
			sqFreight.RM_AirlineCity = "Singapore";
			sqFreight.RM_EagleAddedAirlinePrefixOrAccountingCode = "618";

			// LoadFromAirline2LetterCode will not return an airline when multiple values of the airline code exist (even though that code is valid...):
			AssertNull(RefAirline.LoadFromAirline2LetterCode(Factory, "SQ"));
			AssertEquals("Singapore Airlines 'SQ' prefix with multiple entries is still a valid airline code", true, RefAirline.IsValidAirline2LetterCode(Factory, "SQ"));
		}

		public void TestIsAutoLogged()
		{
			RefAirline loggedAirline = Factory.NewWithValidTestData<RefAirline>();
			loggedAirline.Factory.Save();

			AssertEquals("Expect that log is added.", true, loggedAirline.Logs.GetAllLogs()[0].IsInDatabase);
			AssertEquals("Expect that only one log exists.", 1, loggedAirline.Logs.GetAllLogs().Count);

			loggedAirline.HasSignedEAWBAgreement = true;
			loggedAirline.Factory.Save();
			AssertEquals("Wait for it...", 1, loggedAirline.Logs.GetAllLogs().Count);

			loggedAirline.HasSignedEAWBAgreement = false;
			loggedAirline.Factory.Save();
			AssertEquals("Waiiiiit foooor iiiiit...", 1, loggedAirline.Logs.GetAllLogs().Count);

			loggedAirline.IsTopLevel = true;
			loggedAirline.HasSignedEAWBAgreement = true;
			loggedAirline.Factory.Save();
			AssertEquals("If we tick/untick and it's the top level business object, that's enough to add an EDT.", 2, loggedAirline.Logs.GetAllLogs().Count);

			loggedAirline.HasSignedEAWBAgreement = false;
			loggedAirline.Factory.Save();
			AssertEquals("And again.", 3, loggedAirline.Logs.GetAllLogs().Count);
		}

		public void TestDocManagerInfo()
		{
			RefAirline airline = Factory.New<RefAirline>();

			AssertEquals("DocManagerInfo's DocManagerCode should be ALN.", "ALN", ((IDocManagerSupport)airline).DocManagerInfo.DocManagerCode);
			AssertEquals("DocManagerInfo's BusinessEntity should be our Airline.", airline, ((IDocManagerSupport)airline).DocManagerInfo.BusinessEntity);
		}

		public void TestGetCorrespondingCarrierOrganisation()
		{
			RefAirline airline = Factory.New<RefAirline>();

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "aaa";

			OrgHeader org2 = Factory.New<OrgHeader>();
			org1.OH_Code = "bbb";

			AssertNull("Carrier", airline.GetCorrespondingCarrierOrganisation());

			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "000";
			AssertNull("Carrier", airline.GetCorrespondingCarrierOrganisation());

			org1.OH_RL_NKClosestPort = "DEHAM";
			org1.MiscServ.OM_RM_Airline = airline.PK;
			AssertEquals(org1, airline.GetCorrespondingCarrierOrganisation());

			org2.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			org2.MiscServ.OM_RM_Airline = airline.PK;
			AssertEquals(org2, airline.GetCorrespondingCarrierOrganisation());
		}

		public void TestHumanReadableNameCore()
		{
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_TwoCharacterCode = "AB";
			airline.RM_AirlineName1 = "Testing";

			AssertEquals("Airline - AB - Testing", airline.HumanReadableName);
		}

		public void TestSupportedNotes()
		{
			var noteTypes = Factory.New<RefAirline>().NoteTypes.Cast<PredefinedNoteType>();

			AssertContainsExactElementsInAnyOrder("note types", new[]
			{
				"Terms and Conditions"
			}, noteTypes.Select(nt => nt.Description));
		}

		void CreateRefAirlines()
		{
			var query1 = new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "618");
			var airline1 = Factory.LoadTop1<RefAirline>(query1);
			if (airline1 == null)
			{
				airline1 = Factory.NewWithValidTestData<RefAirline>();
				airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "618";
			}
			airline1.RM_TwoCharacterCode = "SQ";
			airline1.RM_IsActive = true;

			var query2 = new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "081");
			var airline2 = Factory.LoadTop1<RefAirline>(query2);
			if (airline2 == null)
			{
				airline2 = Factory.NewWithValidTestData<RefAirline>();
				airline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "081";
			}
			airline2.RM_TwoCharacterCode = "QF";
			airline2.RM_IsActive = true;

			Factory.Save();
		}
	}
}
