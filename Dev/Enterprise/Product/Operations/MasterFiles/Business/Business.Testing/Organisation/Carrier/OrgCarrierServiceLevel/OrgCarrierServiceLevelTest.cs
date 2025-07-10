using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using FluentAssertions;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCarrierServiceLevel))]
	sealed class OrgCarrierServiceLevelTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCarrierServiceLevelDescription()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.CarrierServiceLevels.RemoveAndDeleteAll();
			var orgCarrierServiceLevel = org.MiscServ.CarrierServiceLevels.AddNew();
			orgCarrierServiceLevel.PL_Code = "STD";
			orgCarrierServiceLevel.PL_CarrierServiceLevelDescription = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			Factory.Save();

			AssertEquals
			(
				"PL_CarrierServiceLevelDescription should able to contain 100 characters",
				"1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890",
				orgCarrierServiceLevel.PL_CarrierServiceLevelDescription
			);
		}

		#region CSV Fields

		public void TestCommaSeparatedServiceCode()
		{
			OrgCarrierServiceLevel csl = (OrgCarrierServiceLevel)GetNewBusinessObject();

			csl.PL_CarrierServiceCode = "ABC";
			csl.CarrierServiceCodes.Should().Equal("ABC");

			csl.PL_CarrierServiceCode = "ABC,DEF";
			csl.CarrierServiceCodes.Should().Equal("ABC", "DEF");

			csl.PL_CarrierServiceCode = "	ABC  , DEF         ";
			csl.CarrierServiceCodes.Should().Equal("ABC", "DEF");

			csl.PL_CarrierServiceCode = "	A CODE  , DEF         ";
			csl.CarrierServiceCodes.Should().Equal("A CODE", "DEF");

			Assert("This test uses FluentAssertions", true);
		}

		public void TestCommaSeparatedProductCode()
		{
			OrgCarrierServiceLevel csl = (OrgCarrierServiceLevel)GetNewBusinessObject();

			csl.PL_ProductCode = "ABC";
			csl.ProductCodes.Should().Equal("ABC");

			csl.PL_ProductCode = "ABC,DEF";
			csl.ProductCodes.Should().Equal("ABC", "DEF");

			csl.PL_ProductCode = "	A  , B ";
			csl.ProductCodes.Should().Equal("A", "B");

			csl.PL_ProductCode = "	A CODE  , DEF	";
			csl.ProductCodes.Should().Equal("A CODE", "DEF");

			Assert("This test uses FluentAssertions", true);
		}

		#endregion

		#region Multilingual

		public void TestPL_CarrierServiceLevelDescriptionMultilingual()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var carrierServiceLevel = org.MiscServ.CarrierServiceLevels.AddNew();

			carrierServiceLevel.PL_Code = "STD";
			carrierServiceLevel.PL_CarrierServiceLevelDescription = "Standard Services";

			Factory.Save();

			var key = carrierServiceLevel.PL_CarrierServiceLevelDescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(carrierServiceLevel, "Standard Services").ResourceKey;

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockData = Res.UseMockData())
			{
				mockData.Put(key, new ResourceStringData(key, "标准服务"));
				AssertEquals("标准服务", carrierServiceLevel.PL_CarrierServiceLevelDescriptionMultilingual);
			}
		}

		#endregion

		#region Saving

		public void TestStandardServiceLevelNotSaved()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals(1, org.MiscServ.CarrierServiceLevels.Count);
			AssertEquals("STD", org.MiscServ.CarrierServiceLevels[0].PL_Code);
			AssertEquals("Standard", org.MiscServ.CarrierServiceLevels[0].PL_CarrierServiceLevelDescription);
			AssertEquals(false, org.MiscServ.CarrierServiceLevels[0].IsInDatabase);

			org.MiscServ.CarrierServiceLevels.RemoveAndDeleteAll();
			OrgCarrierServiceLevel svcLvl = org.MiscServ.CarrierServiceLevels.AddNew();
			svcLvl.PL_Code = "STD";
			svcLvl.PL_CarrierServiceLevelDescription = "Annoying";
			Factory.Save();

			AssertEquals(true, svcLvl.IsInDatabase);
			org.MiscServ.CarrierServiceLevels.RemoveAndDeleteAll();
			Factory.Save();
			org.MiscServ.CarrierServiceLevels.Load();

			Factory.Save();

			AssertEquals(false, org.MiscServ.CarrierServiceLevels[0].IsInDatabase);
			AssertEquals(false, svcLvl.IsInDatabase);
			AssertEquals(false, svcLvl.HasChanges);
		}

		public void TestAllServiceLevelNotSaved()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgCarrierServiceLevelCollection coll = new OrgCarrierServiceLevelCollection(org.MiscServ, true);
			coll.Load();
			AssertEquals(2, coll.Count);
			AssertEquals("STD", coll[0].PL_Code);
			AssertEquals("Standard", coll[0].PL_CarrierServiceLevelDescription);
			AssertEquals(false, coll[0].IsInDatabase);

			AssertEquals("ALL", coll[1].PL_Code);
			AssertEquals("Applies to All Service Levels", coll[1].PL_CarrierServiceLevelDescription);
			AssertEquals(false, coll[1].IsInDatabase);

			coll.RemoveAndDeleteAll();
			OrgCarrierServiceLevel svcLvl = coll.AddNew();
			svcLvl.PL_Code = "ALL";
			svcLvl.PL_CarrierServiceLevelDescription = "Annoying";
			Factory.Save();

			AssertEquals(true, svcLvl.IsInDatabase);
			coll.RemoveAndDeleteAll();
			Factory.Save();
			coll.Load();
			AssertEquals(2, coll.Count);

			Factory.Save();

			AssertEquals(false, coll[0].IsInDatabase);
			AssertEquals(false, coll[1].IsInDatabase);
			AssertEquals(false, svcLvl.IsInDatabase);
			AssertEquals(false, svcLvl.HasChanges);
		}

		#endregion

		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldCarrierValue = Env.Security.OrgCarrierModify.IsAllowed;

			try
			{
				OrgCarrierServiceLevel testSvcLevel = OrgInDB.MiscServ.CarrierServiceLevels.AddNew();

				Env.Security.OrgCarrierModify.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testSvcLevel.PL_CodeInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testSvcLevel.PL_CarrierServiceLevelDescriptionInfo.ReadOnly);

				Env.Security.OrgCarrierModify.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testSvcLevel.PL_CodeInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testSvcLevel.PL_CarrierServiceLevelDescriptionInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgCarrierModify.IsAllowed = oldCarrierValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			return org.MiscServ.CarrierServiceLevels.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			OrgHeader org = factory.NewWithValidTestData<OrgHeader>();
			return org.MiscServ.CarrierServiceLevels.AddNew();
		}

		#endregion
	}
}
