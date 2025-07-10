using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Integration;
using Newtonsoft.Json;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DeniedPartyScreenerAsyncTest : TestCaseWithFactory
	{
		public void TestScreen()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			var orgHeader2 = Factory.New<OrgHeader>();
			var unmatchedOrg = OrgHeader.UnmatchOrg(Factory);

			var jobDocAddress1 = Factory.New<JobDocAddress>();
			jobDocAddress1.E2_AddressOverride = false;
			jobDocAddress1.E2_OA_Address = orgHeader1.MainAddress.PK;

			var jobDocAddress2 = Factory.New<JobDocAddress>();
			jobDocAddress2.E2_AddressOverride = true;

			var vessel = Factory.New<RefVessel>();

			var notLinkedVessel = Factory.New<ITransport>();
			notLinkedVessel.ParentType = typeof(ITransportParentCommon);
			notLinkedVessel.JW_Vessel = "Code";

			var screeningParties = new[]
			{
				new ScreeningParty(orgHeader2, "A", orgHeader2),
				new ScreeningParty(jobDocAddress1, "B", jobDocAddress1),
				new ScreeningParty(jobDocAddress2, "C", jobDocAddress2),
				new ScreeningParty(vessel, "D", vessel),
				new ScreeningParty(notLinkedVessel as BusinessObject, "E", notLinkedVessel as IScreeningPartyForVessel),
				new ScreeningParty(vessel, "F", unmatchedOrg)
			};

			using (new DeniedPartyScreeningHttpServiceForTest(false))
			{
				var results = AsyncTaskSynchronizer.Run(() => DeniedPartyScreenerAsync.Screen(screeningParties));

				AssertEquals(6, results.Count);
				AssertContainsExactElementsInAnyOrder(new[] { orgHeader1.PK, orgHeader2.PK, jobDocAddress2.PK, vessel.PK, (notLinkedVessel as BusinessObject).PK.ToGuid(), unmatchedOrg.PK }, results.Select(u => u.ScreeningParty.ScreeningEntity.PK));

				screeningParties[0].IsCurrentScreeningStatusValid = true;
				results = AsyncTaskSynchronizer.Run(() => DeniedPartyScreenerAsync.Screen(screeningParties));
				AssertEquals(6, results.Count);
				AssertEquals(2, results.Count(u => u.Response == null));
			}
		}

		public void TestScreenForRefCountry()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();

			var screeningParties = new[]
			{
				new ScreeningParty(country, "country", country)
			};

			using (new DeniedPartyScreeningHttpServiceForTest(false))
			{
				var results = AsyncTaskSynchronizer.Run(() => DeniedPartyScreenerAsync.Screen(screeningParties));

				AssertEquals(1, results.Count);
				AssertEquals(country.PK, results[0].ScreeningParty.ScreeningEntity.PK);
			}
		}

		public void TestScreenForVesselCountryOfRegistration()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_RN_NKCountryOfReg = country.Code;

			var screeningParties = new[]
			{
				new ScreeningParty(vessel, "country", country)
			};

			using (new DeniedPartyScreeningHttpServiceForTest(false))
			{
				var results = AsyncTaskSynchronizer.Run(() => DeniedPartyScreenerAsync.Screen(screeningParties));

				AssertEquals(1, results.Count);
				AssertEquals(country.PK, results[0].ScreeningParty.ScreeningEntity.PK);
			}
		}

		public void TestScreenForNaturalPerson()
		{
			var parent = Factory.NewWithValidTestData<RefVessel>();
			Assert("Precondition", parent is IScreeningStatusProvider);

			var screeningParties = new[]
			{
				new ScreeningParty(parent, "Person", "Good King Moggle Mog XII", "Address 1", "Address 2", "City", "Postcode", "State", "Country", "Additional Address Line")
			};

			using (new DeniedPartyScreeningHttpServiceForTest(false))
			{
				var results = AsyncTaskSynchronizer.Run(() => DeniedPartyScreenerAsync.Screen(screeningParties));

				AssertEquals(1, results.Count);
				AssertEquals("Good King Moggle Mog XII", results[0].ScreeningParty.NaturalPerson.Name);
			}
		}

		public void TestMergeDuplicateCandidates()
		{
			OrgHeader parent1 = Helper.CreateOrganisation("PARENT1", "AUSYD");
			OrgHeader parent2 = Helper.CreateOrganisation("PARENT2", "AUMEL");
			OrgHeader parent3 = Helper.CreateOrganisation("PARENT3", "AUBNE");

			OrgHeader org1 = Helper.CreateOrganisation("ORG1", "AUSYD");
			OrgHeader org2 = Helper.CreateOrganisation("ORG2", "AUSYD");
			OrgHeader org3 = Helper.CreateOrganisation("ORG3", "AUSYD");

			ScreeningParty screeningParty1 = new ScreeningParty(parent1, "candidate1", org1);

			ScreeningParty screeningParty2 = new ScreeningParty(parent2, "candidate2", org1);
			ScreeningParty screeningParty3 = new ScreeningParty(parent2, "candidate3", org2);

			ScreeningParty screeningParty4 = new ScreeningParty(parent3, "candidate4", org1);
			ScreeningParty screeningParty5 = new ScreeningParty(parent3, "candidate5", org2);
			ScreeningParty screeningParty6 = new ScreeningParty(parent3, "candidate6", org3);

			ScreeningParty[] mergedCandidates = DeniedPartyScreenerAsync.MergeDuplicateParties(new ScreeningParty[] { screeningParty1, screeningParty2, screeningParty3, screeningParty4, screeningParty5, screeningParty6 });
			AssertEquals("Candidates are merged", 3, mergedCandidates.Length);

			AssertEquals("PARENTSYD: candidate1; PARENTMEL: candidate2; PARENTBNE: candidate4", mergedCandidates[0].ParentsDescription);
			AssertEquals("Multiple parents", 3, mergedCandidates[0].Parents.Count);
			AssertEquals("Org1 from Parent1 merged", true, mergedCandidates[0].Parents.Contains(parent1));
			AssertEquals("Org1 from Parent2 merged", true, mergedCandidates[0].Parents.Contains(parent2));
			AssertEquals("Org1 from Parent3 merged", true, mergedCandidates[0].Parents.Contains(parent3));

			AssertEquals("PARENTMEL: candidate3; PARENTBNE: candidate5", mergedCandidates[1].ParentsDescription);
			AssertEquals("Multiple parents", 2, mergedCandidates[1].Parents.Count);
			AssertEquals("Org2 from Parent2 merged", true, mergedCandidates[1].Parents.Contains(parent2));
			AssertEquals("Org2 from Parent3 merged", true, mergedCandidates[1].Parents.Contains(parent3));

			AssertEquals("PARENTBNE: candidate6", mergedCandidates[2].ParentsDescription);
			AssertEquals("Single parent", 1, mergedCandidates[2].Parents.Count);
			AssertEquals("Org3 from Parent3", true, mergedCandidates[2].Parents.Contains(parent3));
		}

		public void TestMergeDuplicateCandidates_NaturalPerson()
		{
			var parent = Helper.CreateOrganisation("PARENT", "AUSYD");
			Assert("Precondition", parent is IScreeningStatusProvider);

			var screeningParty1 = new ScreeningParty(parent, "Description", "Name", "Addr1", "Addr2", "City", "Postcode", "State", "Country", "AddAddrLine");
			var screeningParty2 = new ScreeningParty(parent, "Description", "Name", "DIFFERENT Addr1", "Addr2", "City", "Postcode", "State", "Country", "AddAddrLine");
			var screeningParty3 = new ScreeningParty(parent, "Description", "Name", "Addr1", "DIFFERENT Addr2", "City", "Postcode", "State", "Country", "AddAddrLine");
			var screeningParty4 = new ScreeningParty(parent, "Description", "Name", "Addr1", "Addr2", "DIFFERENT City", "Postcode", "State", "Country", "AddAddrLine");
			var screeningParty5 = new ScreeningParty(parent, "Description", "Name", "Addr1", "Addr2", "City", "DIFFERENT Postcode", "State", "Country", "AddAddrLine");
			var screeningParty6 = new ScreeningParty(parent, "Description", "Name", "Addr1", "Addr2", "City", "Postcode", "DIFFERENT State", "Country", "AddAddrLine");
			var screeningParty7 = new ScreeningParty(parent, "Description", "Name", "Addr1", "Addr2", "City", "Postcode", "State", "DIFFERENT Country", "AddAddrLine");
			var screeningParty8 = new ScreeningParty(parent, "Description", "Name", "Addr1", "Addr2", "City", "Postcode", "State", "Country", "DIFFERENT AddAddrLine");
			var screeningParty9 = new ScreeningParty(parent, "Description", "DIFFERENT Name", "Addr1", "Addr2", "City", "Postcode", "State", "Country", "AddAddrLine");

			var candidates = DeniedPartyScreenerAsync.MergeDuplicateParties(new ScreeningParty[] { screeningParty1, screeningParty2, screeningParty3, screeningParty4, screeningParty5, screeningParty6, screeningParty7, screeningParty8, screeningParty9 });
			AssertEquals("All candidates are considered unique as none of the fields are the same", 9, candidates.Length);

			screeningParty1 = new ScreeningParty(parent, "Description", "Name", "Addr1", "Addr2", "City", "Postcode", "State", "Country", "AddAddrLine");
			screeningParty2 = new ScreeningParty(parent, "Description", "Name", "Addr1", "Addr2", "City", "Postcode", "State", "Country", "AddAddrLine");

			screeningParty3 = new ScreeningParty(parent, "Description", "DIFFERENT Name", "Addr1", "Addr2", "City", "Postcode", "State", "Country", "AddAddrLine");
			screeningParty4 = new ScreeningParty(parent, "Description", "DIFFERENT Name", "Addr1", "Addr2", "City", "Postcode", "State", "Country", "AddAddrLine");

			screeningParty5 = new ScreeningParty(parent, "Description", "DIFFERENT Name", "DIFFERENT Addr1", "Addr2", "City", "Postcode", "State", "Country", "AddAddrLine");
			screeningParty6 = new ScreeningParty(parent, "Description", "DIFFERENT Name", "DIFFERENT Addr1", "Addr2", "City", "Postcode", "State", "Country", "AddAddrLine");

			screeningParty7 = new ScreeningParty(parent, "Description", "DIFFERENT Name", "DIFFERENT Addr1", "DIFFERENT Addr2", "City", "Postcode", "State", "Country", "AddAddrLine");
			screeningParty8 = new ScreeningParty(parent, "Description", "DIFFERENT Name", "DIFFERENT Addr1", "Addr2", "DIFFERENT City", "Postcode", "State", "Country", "AddAddrLine");
			screeningParty9 = new ScreeningParty(parent, "Description", "DIFFERENT Name", "DIFFERENT Addr1", "Addr2", "DIFFERENT City", "DIFFERENT Postcode", "State", "Country", "AddAddrLine");

			candidates = DeniedPartyScreenerAsync.MergeDuplicateParties(new ScreeningParty[] { screeningParty1, screeningParty2, screeningParty3, screeningParty4, screeningParty5, screeningParty6, screeningParty7, screeningParty8, screeningParty9 });
			AssertEquals("Candidates are merged due to duplication", 6, candidates.Length);
		}

		public void TestMergeDuplicateCandidates_UnlinkedVessels()
		{
			var vessel1 = CreateNotLinkedVessel("NotLinkedVessel1");
			var vessel2 = CreateNotLinkedVessel("NotLinkedVessel2");
			var vessel3 = CreateNotLinkedVessel("NotLinkedVessel1");
			var vessel4 = CreateNotLinkedVessel("NotLinkedVessel1");

			var mergedCandidates = DeniedPartyScreenerAsync.MergeDuplicateParties(vessel1, vessel2, vessel3, vessel4);
			AssertContainsExactElementsInAnyOrder("Vessels are merged if they have the same vessel name", new[] { vessel1, vessel2 }, mergedCandidates);
		}

		ScreeningParty CreateNotLinkedVessel(string vesselName)
		{
			var transport = Factory.New<ITransport>();
			transport.ParentType = typeof(ITransportParentCommon);
			transport.JW_Vessel = vesselName;
			return new ScreeningParty(transport as BusinessObject, "Transport", transport as IScreeningPartyForVessel);
		}

		public void TestGetDeniedParties()
		{
			var parent1 = Helper.CreateOrganisation("PARENT1", "AUSYD");
			var parent2 = Helper.CreateOrganisation("PARENT2", "AUMEL");
			var parent3 = Helper.CreateOrganisation("PARENT3", "AUBNE");

			var org1 = Helper.CreateOrganisation("ORG1", "AUSYD");
			var org2 = Helper.CreateOrganisation("ORG2", "AUSYD");
			org2.OH_ScreeningStatus = "MAT";
			var org3 = Helper.CreateOrganisation("ORG3", "AUSYD");
			org3.OH_ScreeningStatus = "CLR";
			var org4 = Helper.CreateOrganisation("ORG4", "AUSYD");
			org4.OH_ScreeningStatus = "CLP";
			var org5 = Helper.CreateOrganisation("ORG5", "AUSYD");
			org5.OH_ScreeningStatus = "JCL";

			var screeningParty1 = new ScreeningParty(parent1, "candidate1", org1);

			var screeningParty2 = new ScreeningParty(parent2, "candidate2", org1);
			var screeningParty3 = new ScreeningParty(parent2, "candidate3", org2);

			var screeningParty4 = new ScreeningParty(parent3, "candidate4", org1);
			var screeningParty5 = new ScreeningParty(parent3, "candidate5", org2);
			var screeningParty6 = new ScreeningParty(parent3, "candidate6", org3);
			var screeningParty7 = new ScreeningParty(parent3, "candidate6", org4);
			var screeningParty8 = new ScreeningParty(parent3, "candidate6", org5);

			var parties = new ScreeningParty[] { screeningParty1, screeningParty2, screeningParty3, screeningParty4, screeningParty5, screeningParty6, screeningParty7, screeningParty8 };
			var deniedParties = DeniedPartyScreenerAsync.GetDeniedParties(parties);

			AssertCollectionContains(screeningParty1, deniedParties);
			AssertCollectionContains(screeningParty3, deniedParties);
			AssertCollectionNotContains(screeningParty6, deniedParties);
			AssertCollectionNotContains(screeningParty7, deniedParties);
			AssertCollectionNotContains(screeningParty8, deniedParties);

			AssertEquals("MAT should be first in list", screeningParty3, deniedParties[0]);
			AssertEquals("UNK should follow MAT in list", screeningParty1, deniedParties[1]);
		}

		public void TestProfileHeaderInfoJsonConverter()
		{
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo
				{
					SourceProfileID = Guid.NewGuid(),
					ProfileNotes = Compressor.Zip("test msg is \n here"),
					TypeOfEntity = "COY",
					SourceListCodes = new List<string> { "AAA" }
				},
			};
			var dpsResponse = new DpsResponse
			{
				Profiles = profiles,
			};

			var result = JsonConvert.SerializeObject(dpsResponse, Formatting.Indented, new ProfileHeaderInfoJsonConverter());
			Assert("result contains test msg", result.Contains("\"ProfileNotes\": \"test msg is \\n here\""));
			Assert("result contains unescape test msg and replace \n with newline in textfield", result.Replace("\\r", string.Empty).Replace("\\n", System.Environment.NewLine).Contains("\"ProfileNotes\": \"test msg is \r\n here\""));
		}

		#region Implementation

		MasterFilesTestHelper Helper
		{
			get { return fHelper ?? (fHelper = new MasterFilesTestHelper(Factory)); }
		}
		MasterFilesTestHelper fHelper;

		#endregion
	}
}
