using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using WTG.Ecommerce.Business;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVSSCCNumberFountainSupporterTest : NonTransactionedTestCase
	{
		[UseSnapshotProtection]
		public void TestGetSSCCNumberFountain_ReturnsNull_ShouldSetOrgHeaderNumberRanges()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgCustomCode = orgHeader.CustomsCodes.AddNew();
			orgCustomCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			orgCustomCode.OK_CustomsRegNo = "12345678";

			Factory.Save();

			var connection = Db.Connection as IDbConnectionInternals;
			var numberFountain = HVLVSSCCNumberFountainSupporter.GetSSCCNumberFountain(orgHeader.PK.ToGuid(),
																 null,
																 Env.CurrentBranch.NKUNLOCO,
																 connection.InternalDbConnection);

			AssertNull("number fountain is null if didn't set number ranges for orgHeader before", numberFountain);

			var orgStmNumSSCC = orgHeader.OrgFountains.AddNew();
			orgStmNumSSCC.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			orgStmNumSSCC.SN_Prefix = "12345678";

			Factory.Save();

			numberFountain = HVLVSSCCNumberFountainSupporter.GetSSCCNumberFountain(orgHeader.PK.ToGuid(),
																null,
																Env.CurrentBranch.NKUNLOCO,
																connection.InternalDbConnection);

			AssertNotNull(numberFountain);
		}

		[UseSnapshotProtection]
		public void TestGetSSCCNumberFountain_WhenOrgAddressIsNotNull()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.AddressCode = "testAddress";
			orgAddress.OA_OH = orgHeader.PK;

			var orgCustomCode1 = CreateGS1Code("11111111", orgHeader, orgAddress);
			orgCustomCode1.OK_OA_PremisesAddress = orgAddress.PK;

			CreateGS1Code("22222222", orgHeader);
			Factory.Save();

			var connection = Db.Connection as IDbConnectionInternals;
			var numberFountainFactory = HVLVSSCCNumberFountainSupporter.GetSSCCNumberFountain(orgHeader.PK.ToGuid(),
																orgAddress.PK.ToGuid(),
																Env.CurrentBranch.NKUNLOCO,
																connection.InternalDbConnection);

			AssertNotNull(numberFountainFactory);
			AssertEquals("sscc prefix is gs1 code which PremisesAddress is orgAddress", "11111111", numberFountainFactory.Prefix);
		}

		[UseSnapshotProtection]
		public void TestGetSSCCNumberFountain_WhenOrgAddressIsNull_SSCCPrefixFallbackToFirstGS1CodeWhichPremisesAddressPortIsCurrentBranchNKUNLOCO()
		{
			const string mockCurrentBranchNKUNLOCO = "AUSYD";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.AddressCode = "testAddress1";
			orgAddress1.OA_OH = orgHeader.PK;
			orgAddress1.OA_RL_NKRelatedPortCode = mockCurrentBranchNKUNLOCO;

			var orgCustomCode1 = CreateGS1Code("11111111", orgHeader, orgAddress1);
			orgCustomCode1.OK_OA_PremisesAddress = orgAddress1.PK;

			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress2.AddressCode = "testAddress2";
			orgAddress2.OA_OH = orgHeader.PK;
			orgAddress2.OA_RL_NKRelatedPortCode = "USLAX";

			var orgCustomCode2 = CreateGS1Code("22222222", orgHeader);
			orgCustomCode2.OK_OA_PremisesAddress = orgAddress2.PK;

			Factory.Save();

			var connection = Db.NewExtraConnectionToMainDb() as IDbConnectionInternals;
			var numberFountainFactory = HVLVSSCCNumberFountainSupporter.GetSSCCNumberFountain(orgHeader.PK.ToGuid(),
																null,
																mockCurrentBranchNKUNLOCO,
																connection.InternalDbConnection);

			AssertNotNull(numberFountainFactory);
			AssertEquals("sscc prefix fallback to first gs1 code which premises address port is current branch NKUNLOCO", "11111111", numberFountainFactory.Prefix);
		}

		[UseSnapshotProtection]
		public void TestGetSSCCNumberFountain_WhenOrgAddressIsNull_NoGS1CodePremisesAddressPortIsCurrentBranchNKUNLOCO()
		{
			const string mockCurrentBranchNKUNLOCO = "AUSYD";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.AddressCode = "testAddress";
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var orgCustomCode1 = CreateGS1Code("11111111", orgHeader, orgAddress);
			orgCustomCode1.OK_OA_PremisesAddress = orgAddress.PK;

			var orgCustomCode2 = CreateGS1Code("22222222", orgHeader);
			Assert("precondition", orgCustomCode2.OK_OA_PremisesAddress.IsEmpty);
			Factory.Save();

			var connection = Db.NewExtraConnectionToMainDb() as IDbConnectionInternals;
			var numberFountainFactory = HVLVSSCCNumberFountainSupporter.GetSSCCNumberFountain(orgHeader.PK.ToGuid(),
																null,
																mockCurrentBranchNKUNLOCO,
																connection.InternalDbConnection);

			AssertNotNull(numberFountainFactory);
			AssertEquals("sscc prefix fallback to first gs1 code which premises address is null", "22222222", numberFountainFactory.Prefix);
		}

		OrgCusCode CreateGS1Code(string prefix, OrgHeader orgHeader, OrgAddress orgAddress = null)
		{
			var orgCustomCode = orgHeader.CustomsCodes.AddNew();
			orgCustomCode.OK_CodeType = OrgCusCode.CodeTypes.GS1;
			orgCustomCode.OK_CustomsRegNo = prefix;

			if (orgAddress != null)
			{
				orgCustomCode.OK_OA_PremisesAddress = orgAddress.PK;
			}

			var orgStmNumSSCC1 = orgHeader.OrgFountains.AddNew();
			orgStmNumSSCC1.SN_Type = OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers;
			orgStmNumSSCC1.SN_Prefix = prefix;

			return orgCustomCode;
		}
	}
}
