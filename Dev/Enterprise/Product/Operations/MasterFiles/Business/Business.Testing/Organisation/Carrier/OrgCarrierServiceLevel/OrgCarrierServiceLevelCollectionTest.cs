using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCarrierServiceLevelCollection))]
	sealed class OrgCarrierServiceLevelCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Load

		public void TestLoadAddsStandardServiceLevel_NoMaster()
		{
			var collection = new OrgCarrierServiceLevelCollection(Factory, false, false);

			collection.Load();
			AssertEquals(0, collection.Count);
		}

		public void TestLoadAddsStandardServiceLevel_WithMaster()
		{
			var org = Factory.New<OrgHeader>();
			var collection = new OrgCarrierServiceLevelCollection(org.MiscServ, false, false);

			collection.Load();
			AssertEquals(0, collection.Count);
		}

		public void TestLoadAddsStandardServiceLevel_WithMaster_WithStd()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgCarrierServiceLevelCollection coll = new OrgCarrierServiceLevelCollection(org.MiscServ);

			coll.Load();
			AssertEquals(1, coll.Count);
			AssertEquals("STD", coll[0].PL_Code);
			AssertEquals("Standard", coll[0].PL_CarrierServiceLevelDescription);

			coll.RemoveAndDeleteAll();
			AssertEquals(0, coll.Count);

			coll.Load();
			AssertEquals(1, coll.Count);

			coll.RemoveAndDeleteAll();
			OrgCarrierServiceLevel svcLvl = coll.AddNew();
			svcLvl.PL_Code = "STD";
			svcLvl.PL_CarrierServiceLevelDescription = "Annoying";

			coll.Load();
			AssertEquals(1, coll.Count);
			AssertEquals("Annoying", coll[0].PL_CarrierServiceLevelDescription);
		}

		public void TestLoadAddsStandardServiceLevel_NoMaster_WithStd()
		{
			OrgCarrierServiceLevelCollection coll = new OrgCarrierServiceLevelCollection(Factory);

			coll.Load();
			AssertEquals(1, coll.Count);
			AssertEquals("STD", coll[0].PL_Code);
			AssertEquals("Standard", coll[0].PL_CarrierServiceLevelDescription);
		}

		public void TestLoadAddsStandardServiceLevel_WithMaster_WithAll()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgCarrierServiceLevelCollection coll = new OrgCarrierServiceLevelCollection(org.MiscServ, true);

			coll.Load();
			AssertEquals(2, coll.Count);
			AssertEquals("STD", coll[0].PL_Code);
			AssertEquals("Standard", coll[0].PL_CarrierServiceLevelDescription);

			AssertEquals("ALL", coll[1].PL_Code);
			AssertEquals("Applies to All Service Levels", coll[1].PL_CarrierServiceLevelDescription);
		}

		public void TestLoadAddsStandardServiceLevel_NoMaster_WithAll()
		{
			OrgCarrierServiceLevelCollection coll = new OrgCarrierServiceLevelCollection(Factory, true);

			coll.Load();
			AssertEquals(2, coll.Count);
			AssertEquals("STD", coll[0].PL_Code);
			AssertEquals("Standard", coll[0].PL_CarrierServiceLevelDescription);

			AssertEquals("ALL", coll[1].PL_Code);
			AssertEquals("Applies to All Service Levels", coll[1].PL_CarrierServiceLevelDescription);

			coll.Load();
			AssertEquals("No further items are added", 2, coll.Count);
		}

		public void TestDefaultsAreNotSavedInNonEnglishLanguage()
		{
			using (var mockRes = Res.UseMockData())
			{
				mockRes.SetResourceGetter(key => new ResourceStringData(key, "疵"));

				var coll = new OrgCarrierServiceLevelCollection(Factory, true);
				coll.Load();
				AssertEquals(2, coll.Count);
				AssertEquals("STD", coll[0].PL_Code);
				AssertEquals("ALL", coll[1].PL_Code);

				Factory.Save();

				Assert("Default items should not be saved in factory", !coll[0].IsSavedByFactory);
				Assert("Default items should not be saved in factory", !coll[1].IsSavedByFactory);

				mockRes.SetResourceGetter(key => new ResourceStringData(key, "病"));
				Factory.Save();
				Assert("Default items should not be saved in factory", !coll[0].IsSavedByFactory);
				Assert("Default items should not be saved in factory", !coll[1].IsSavedByFactory);
			}
		}

		#endregion

		#region TestGetFilterWithNoMaster_DoesNotBlowUp

		public void TestGetFilterWithNoMaster_DoesNotBlowUp()
		{
			OrgCarrierServiceLevelCollection coll = new OrgCarrierServiceLevelCollection(Factory, true);
			AssertNoExceptionThrown(delegate
			{ ZQuery qry = coll.CompleteFilter; });
		}

		#endregion

		#region GetDescriptionFromCode

		public void TestGetDescriptionFromCode()
		{
			OrgCarrierServiceLevelCollection coll = new OrgCarrierServiceLevelCollection(Factory);

			coll.Load();

			OrgCarrierServiceLevel svcLvl = coll.AddNew();
			svcLvl.PL_Code = "2ND";
			svcLvl.PL_CarrierServiceLevelDescription = "2ND DAY";

			AssertEquals("Service Level Description Matches", svcLvl.PL_CarrierServiceLevelDescription, coll.GetDescriptionFromCode(svcLvl.PL_Code));
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader carrier = Factory.New<OrgHeader>();
			return new OrgCarrierServiceLevelCollection(carrier.MiscServ);
		}

		#endregion
	}
}
