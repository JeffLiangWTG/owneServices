using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class CommonUniversalFreightHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestIsUNDGDataObjectMatchedToUNDGBusinessObject()
		{
			var undgDO = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);
			var undgBO = Factory.New<UNDGDataItem>();

			Assert(!CommonUniversalFreightHelper.IsUNDGDataObjectMatchedToUNDGBusinessObject(null, null));
			Assert(!CommonUniversalFreightHelper.IsUNDGDataObjectMatchedToUNDGBusinessObject(undgDO, null));
			Assert(!CommonUniversalFreightHelper.IsUNDGDataObjectMatchedToUNDGBusinessObject(null, undgBO));
			Assert(!CommonUniversalFreightHelper.IsUNDGDataObjectMatchedToUNDGBusinessObject(undgDO, undgBO));

			Action<ZString, ZString, ZString, ZString, ZBool> assertMatch = (undgCodeForDO, imoClassForDO, undgCodeForBO, imoClassForBO, isMatched) =>
			{
				var subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = undgCodeForBO;

				undgDO.UNDGCode = undgCodeForDO;
				undgDO.IMOClass = imoClassForDO;
				undgBO.DI_DG = undgCodeForBO.IsEmpty ? ZGuid.Empty : subs.PK;
				undgBO.DI_IMOClass = imoClassForBO;

				AssertEquals(isMatched, CommonUniversalFreightHelper.IsUNDGDataObjectMatchedToUNDGBusinessObject(undgDO, undgBO));
			};

			assertMatch(string.Empty, string.Empty, string.Empty, string.Empty, false);

			assertMatch(string.Empty, "1.1a", string.Empty, "1.1a", true);
			assertMatch(string.Empty, "1.1a", string.Empty, "1.2a", false);
			assertMatch("2.1", "1.1a", string.Empty, "1.1a", false);
			assertMatch(string.Empty, "1.1a", "2.1", "1.1a", false);
			assertMatch("2.1", "1.1a", "3.1", "1.1a", false);

			assertMatch("1.1a", string.Empty, "1.1a", string.Empty, true);
			assertMatch("1.2a", string.Empty, "1.1a", string.Empty, false);
			assertMatch("1.1a", "2.1", "1.1a", string.Empty, true);
			assertMatch("1.1a", string.Empty, "1.1a", "2.1", true);
			assertMatch("1.1a", "2.1", "1.1a", "3.1", true);

			assertMatch("1.1a", "2.1", "1.1a", "2.1", true);
		}

		#region HasSCAC

		public void TestHasSCAC()
		{
			var factory = new BusinessObjectFactory();

			var org1 = SetupOrgWithSCAC(factory);
			Assert(org1.HasSCAC("YMLU"));
			Assert(org1.HasSCAC("YmLu"));
			Assert(org1.HasSCAC("ymlu"));
			Assert(!org1.HasSCAC("ABCD"));

			var org2 = SetupOrgWithSCAC(factory, "TestOrg2", "aaaa");
			Assert(org2.HasSCAC("AAAA"));
			Assert(org2.HasSCAC("AAaa"));
			Assert(org2.HasSCAC("aaaa"));
			Assert(!org1.HasSCAC("ABCD"));

			var org3 = SetupOrgWithSCAC(factory, "TestOrg3", "bbbb", OrgCusCode.CodeTypes.AgentCode);
			Assert(!org3.HasSCAC("bbbb"));

			var org4 = SetupOrgWithSCAC(factory, "TestOrg4", "cccc", OrgCusCode.CodeTypes.CarrierCode, Constants.CountryCodes.Canada);
			Assert(!org4.HasSCAC("cccc"));

			OrgHeader org5 = null;
			Assert(!org5.HasSCAC("ABCD"));
		}

		OrgHeader SetupOrgWithSCAC(BusinessObjectFactory factory,
			string code = "TestOrg",
			string regNo = "YMLU",
			string regType = OrgCusCode.CodeTypes.CarrierCode,
			string country = Constants.CountryCodes.UnitedStates)
		{
			var org = factory.New<OrgHeader>();
			org.OH_Code = code;
			org.MainAddress.Address1 = "Test Street";

			var orgCusCode = org.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = regType;
			orgCusCode.OK_CustomsRegNo = regNo;
			orgCusCode.OK_RN_NKCodeCountry = country;

			return org;
		}

		#endregion
	}
}
