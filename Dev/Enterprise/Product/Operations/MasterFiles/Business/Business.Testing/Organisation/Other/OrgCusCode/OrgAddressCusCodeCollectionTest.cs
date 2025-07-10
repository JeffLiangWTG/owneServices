using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAddressCusCodeCollection))]
	sealed class OrgAddressCusCodeCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgAddressCusCodeCollection>
	{
		public void TestAddNewWithExtraParameters()
		{
			var organisation = Factory.New<OrgHeader>();
			OrgAddress mainAddress = organisation.MainAddress;
			AssertEquals(0, mainAddress.CustomsCodes.Count);
			OrgCusCode added = organisation.CustomsCodes.AddNew("CCC", "123435");
			added.OK_OA_PremisesAddress = mainAddress.PK;
			AssertEquals(true, mainAddress.CustomsCodes.Contains(added));
			added = mainAddress.CustomsCodes.AddNew("AAA", "123456");
			AssertEquals("new element is contained in the organisation.CustomsCodes", true, organisation.CustomsCodes.Contains(added));
			AssertEquals("new element is contained in the mainAddress.CustomsCodes", true, mainAddress.CustomsCodes.Contains(added));
			AssertEquals("new element  has 'AAA'", "AAA", added.OK_CodeType);
			AssertEquals("new element has '123456'", "123456", added.OK_CustomsRegNo);
			AssertEquals("new element has a current country", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, added.OK_RN_NKCodeCountry);
			AssertEquals("new element has mainaddress premises", mainAddress.PK, added.OK_OA_PremisesAddress);

			var otherCountry = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

			added = mainAddress.CustomsCodes.AddNew("BBB", "222222", otherCountry);
			AssertEquals("new element is contained in the organisation.CustomsCodes", true, organisation.CustomsCodes.Contains(added));
			AssertEquals("new element is contained in the mainAddress.CustomsCodes", true, mainAddress.CustomsCodes.Contains(added));
			AssertEquals("new element has 'BBB'", "BBB", added.OK_CodeType);
			AssertEquals("new element has '222222'", "222222", added.OK_CustomsRegNo);
			AssertEquals("new element has a current country", otherCountry.Code, added.OK_RN_NKCodeCountry);
			AssertEquals("new element has mainaddress premises", mainAddress.PK, added.OK_OA_PremisesAddress);

			added = mainAddress.CustomsCodes.AddNew("CCC", "333333", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("new element is contained in the organisation.CustomsCodes", true, organisation.CustomsCodes.Contains(added));
			AssertEquals("new element is contained in the mainAddress.CustomsCodes", true, mainAddress.CustomsCodes.Contains(added));
			AssertEquals("new element has 'CCC'", "CCC", added.OK_CodeType);
			AssertEquals("new element has '333333'", "333333", added.OK_CustomsRegNo);
			AssertEquals("new element has a set country", Core.Constants.CountryCodes.UnitedStates, added.OK_RN_NKCodeCountry);
			AssertEquals("new element has mainaddress premises", mainAddress.PK, added.OK_OA_PremisesAddress);
		}

		public void TestUpdateOrAddCustomsCodesIfNoneExists()
		{
			var org = Factory.New<OrgHeader>();
			OrgAddress mainAddress = org.MainAddress;
			OrgCusCode cusCode = mainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("AAA", "123", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(1, mainAddress.CustomsCodes.Count);
			AssertEquals(cusCode, mainAddress.CustomsCodes[0]);
			AssertEquals("123", mainAddress.CustomsCodes.GetCustomsRegNo("AAA", Core.Constants.CountryCodes.UnitedStates));

			cusCode = mainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("AAA", "234", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("Same type", 1, mainAddress.CustomsCodes.Count);
			AssertEquals(cusCode, mainAddress.CustomsCodes[0]);
			AssertEquals("234", mainAddress.CustomsCodes.GetCustomsRegNo("AAA", Core.Constants.CountryCodes.UnitedStates));

			OrgCusCode cusCode2 = mainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("AAA", "633", Core.Constants.CountryCodes.Australia);
			AssertEquals("New Code", 2, mainAddress.CustomsCodes.Count);
			AssertEquals(cusCode, mainAddress.CustomsCodes[0]);
			AssertEquals(cusCode2, mainAddress.CustomsCodes[1]);
			AssertEquals("234", mainAddress.CustomsCodes.GetCustomsRegNo("AAA", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("633", mainAddress.CustomsCodes.GetCustomsRegNo("AAA", Core.Constants.CountryCodes.Australia));
		}

		public void TestGetCustomsCode()
		{
			OrgAddress address2 = Organisation.Addresses.AddNew();
			OrgCusCode cusCode1 = Organisation.CustomsCodes.AddNew("AAA", "1111");
			cusCode1.OK_OA_PremisesAddress = address2.PK;
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			OrgCusCode cusCode2 = Organisation.CustomsCodes.AddNew("AAA", "2222");
			cusCode2.OK_OA_PremisesAddress = Organisation.MainAddress.PK;
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			OrgCusCode cusCode3 = address2.CustomsCodes.AddNew();
			cusCode3.OK_CodeType = "AAA";
			cusCode3.OK_CustomsRegNo = "3333";
			cusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;

			AssertEquals("1111", address2.CustomsCodes.GetCustomsRegNo("AAA", Core.Constants.CountryCodes.Brazil));
			AssertEquals("3333", address2.CustomsCodes.GetCustomsRegNo("AAA", Core.Constants.CountryCodes.Canada));
			AssertEquals("", address2.CustomsCodes.GetCustomsRegNo("AAA", Core.Constants.CountryCodes.Australia));
			AssertEquals("", address2.CustomsCodes.GetCustomsRegNo("BBB", Core.Constants.CountryCodes.Brazil));
		}

		public void TestGetOrgCusCodeObjectForCodeTypeAndCountry()
		{
			OrgAddress address2 = Organisation.Addresses.AddNew();
			OrgCusCode cusCode1 = Organisation.CustomsCodes.AddNew("AAA", "1111");
			cusCode1.OK_OA_PremisesAddress = address2.PK;
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			OrgCusCode cusCode2 = Organisation.CustomsCodes.AddNew("AAA", "2222");
			cusCode2.OK_OA_PremisesAddress = Organisation.MainAddress.PK;
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			OrgCusCode cusCode3 = address2.CustomsCodes.AddNew();
			cusCode3.OK_CodeType = "AAA";
			cusCode3.OK_CustomsRegNo = "3333";
			cusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;

			AssertEquals(cusCode1, address2.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry("AAA", Core.Constants.CountryCodes.Brazil));
			AssertEquals(cusCode3, address2.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry("AAA", Core.Constants.CountryCodes.Canada));
			AssertNull(address2.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry("AAA", Core.Constants.CountryCodes.Australia));
			AssertNull(address2.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry("BBB", Core.Constants.CountryCodes.Brazil));
		}

		public void TestGetOrgCusCodesForCodeIgnoringCountry()
		{
			var address1 = Organisation.MainAddress;
			var address2 = Organisation.Addresses.AddNew();
			var cusCode1 = Organisation.CustomsCodes.AddNew("AAA", "1111");
			cusCode1.OK_OA_PremisesAddress = address1.PK;
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			var cusCode2 = Organisation.CustomsCodes.AddNew("AAA", "2222");
			cusCode2.OK_OA_PremisesAddress = address2.PK;
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;

			CombineAssertions(() =>
			{
				AssertEquals(1, address1.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry("AAA").Length);
				AssertEquals(cusCode1.PK, address1.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry("AAA")[0].PK);
				AssertEquals(1, address2.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry("AAA").Length);
				AssertEquals(cusCode2.PK, address2.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry("AAA")[0].PK);
			});
		}

		public void TestGetOrgCusCodeTypeCollectionForRegNoAndCountry()
		{
			var address = Organisation.MainAddress;
			var cusCode1 = Organisation.CustomsCodes.AddNew("AAA", "1111");
			var cusCode2 = Organisation.CustomsCodes.AddNew("BBB", "1111");

			cusCode1.OK_OA_PremisesAddress = address.PK;
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			cusCode2.OK_OA_PremisesAddress = address.PK;
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;

			CombineAssertions(() =>
			{
				AssertEquals(1, address.CustomsCodes.GetOrgCusCodeTypeCollectionForRegNoAndCountry("1111", Core.Constants.CountryCodes.UnitedStates).Length);
				AssertEquals("AAA", string.Join(", ", address.CustomsCodes.GetOrgCusCodeTypeCollectionForRegNoAndCountry("1111", Core.Constants.CountryCodes.UnitedStates)));
				AssertEquals(0, address.CustomsCodes.GetOrgCusCodeTypeCollectionForRegNoAndCountry("2222", Core.Constants.CountryCodes.UnitedStates).Length);
			});
		}

		public void TestGetCustomsRegNoMatching()
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var address = Organisation.Addresses.AddNew();
			var jas = address.CustomsCodes.AddNew();
			jas.OK_CodeType = JapanCodeTypes.JAS;
			jas.OK_CustomsRegNo = "JAS12345";
			jas.OK_RN_NKCodeCountry = countryCode;
			AssertEquals("JAS", "JAS12345", address.CustomsCodes.GetCustomsRegNoMatching(OrgCusCode.JapanCodeTypes.LPC, OrgCusCode.JapanCodeTypes.CIE, OrgCusCode.JapanCodeTypes.JAS));

			var cie = address.CustomsCodes.AddNew();
			cie.OK_CodeType = JapanCodeTypes.CIE;
			cie.OK_CustomsRegNo = "CIE123450001";
			cie.OK_RN_NKCodeCountry = countryCode;
			AssertEquals("CIE", "CIE123450001", address.CustomsCodes.GetCustomsRegNoMatching(OrgCusCode.JapanCodeTypes.LPC, OrgCusCode.JapanCodeTypes.CIE, OrgCusCode.JapanCodeTypes.JAS));

			var lpc = address.CustomsCodes.AddNew();
			lpc.OK_CodeType = JapanCodeTypes.LPC;
			lpc.OK_CustomsRegNo = "LPC1234567890";
			lpc.OK_RN_NKCodeCountry = countryCode;
			AssertEquals("LPC", "LPC1234567890", address.CustomsCodes.GetCustomsRegNoMatching(OrgCusCode.JapanCodeTypes.LPC, OrgCusCode.JapanCodeTypes.CIE, OrgCusCode.JapanCodeTypes.JAS));
			AssertEquals("CIE", "CIE123450001", address.CustomsCodes.GetCustomsRegNoMatching(OrgCusCode.JapanCodeTypes.CIE, OrgCusCode.JapanCodeTypes.JAS));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				AssertNullOrEmpty(address.CustomsCodes.GetCustomsRegNoMatching(OrgCusCode.JapanCodeTypes.CIE, OrgCusCode.JapanCodeTypes.JAS));
			}
		}

		public void TestIBODocDataProviderGetRow()
		{
			OrgAddress address2 = Organisation.Addresses.AddNew();
			OrgCusCode cusCode1 = Organisation.CustomsCodes.AddNew("AAA", "1111");
			cusCode1.OK_OA_PremisesAddress = address2.PK;
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			OrgCusCode cusCode2 = Organisation.CustomsCodes.AddNew("AAA", "2222");
			cusCode2.OK_OA_PremisesAddress = Organisation.MainAddress.PK;
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			OrgCusCode cusCode3 = address2.CustomsCodes.AddNew();
			cusCode3.OK_CodeType = "AAA";
			cusCode3.OK_CustomsRegNo = "3333";
			cusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;

			IBODocDataProviderCollection collection = address2.CustomsCodes;
			AssertEquals(cusCode1, BODocDataProvider.GetBusinessObject(collection["AAA:" + Core.Constants.CountryCodes.Brazil]));
			AssertEquals(cusCode3, BODocDataProvider.GetBusinessObject(collection["AAA:" + Core.Constants.CountryCodes.Canada]));
			AssertNull(collection["AAA:" + Core.Constants.CountryCodes.Australia]);
			AssertNull(collection["BBB:" + Core.Constants.CountryCodes.Brazil]);
		}

		#region Implementation
		protected override OrgAddressCusCodeCollection GetCollectionToTest()
		{
			return new OrgAddressCusCodeCollection(Organisation.MainAddress);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			OrgCusCode cusCode = Organisation.CustomsCodes.AddNew();
			cusCode.OK_OA_PremisesAddress = Organisation.MainAddress.PK;
			return cusCode;
		}

		OrgHeader Organisation
		{
			get { return organisation ?? (organisation = Factory.New<OrgHeader>()); }
		}
		OrgHeader organisation;
		#endregion
	}
}
