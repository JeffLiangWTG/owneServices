using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	class PatternMatchingOrgAddressMaintenanceTest : PatternMatchingSourceTest<OrgAddress, PatternMatchingOrgAddressMaintenance>
	{
		protected override OrgAddress BizO => address ?? (address = CreateOrganisation().Addresses.AddNew());
		OrgAddress address;

		protected override PatternMatchingOrgAddressMaintenance MaintenanceObject => new PatternMatchingOrgAddressMaintenance(BizO);

		protected override void AssertPatternMatchingDataExists()
		{
			var patternMatchingAddresses = Factory.Load<PatternMatchingAddress>(new ZQuery(PatternMatchingAddressSchema.PMA_ParentId, BizO.PK));
			var patternMatchingDomains = Factory.Load<PatternMatchingDomain>(new ZQuery(PatternMatchingDomainSchema.PMD_ParentId, BizO.PK));
			var patternMatchingEmails = Factory.Load<PatternMatchingEmail>(new ZQuery(PatternMatchingEmailSchema.PME_ParentId, BizO.PK));
			var patternMatchingNames = Factory.Load<PatternMatchingName>(new ZQuery(PatternMatchingNameSchema.PMN_ParentId, BizO.PK));
			var patternMatchingPhones = Factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, BizO.PK));
			var patternMatchingRegCodes = Factory.Load<PatternMatchingRegCode>(new ZQuery(PatternMatchingRegCodeSchema.PMR_ParentId, BizO.PK));

			var valueAddressToHash = (BizO.OA_Address1 + BizO.OA_Address2 + BizO.OA_City + BizO.OA_PostCode + BizO.OA_State).ToUpperInvariant();
			var valueDomainToHash = "wisetechglobal.com".ToUpperInvariant();
			var valueEmailToHash = "scott@wisetechglobal.com".ToUpperInvariant();
			var valueNameToHash = "Linfox".ToUpperInvariant();

			var patternAddress = patternMatchingAddresses[0];
			var patternDomain = patternMatchingDomains[0];
			var patternEmail = patternMatchingEmails[0];
			var patternName = patternMatchingNames[0];

			AssertEquals(1, patternMatchingAddresses.Length);
			AssertEquals(1, patternMatchingDomains.Length);
			AssertEquals(1, patternMatchingEmails.Length);
			AssertEquals(1, patternMatchingNames.Length);
			AssertEquals(3, patternMatchingPhones.Length);
			AssertEquals(0, patternMatchingRegCodes.Length);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueAddressToHash), patternAddress.PMA_HashedValue);
			AssertEquals(address.PK, patternAddress.PMA_ParentId);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueDomainToHash), patternDomain.PMD_HashedValue);
			AssertEquals(address.PK, patternDomain.PMD_ParentId);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueEmailToHash), patternEmail.PME_HashedValue);
			AssertEquals(address.PK, patternEmail.PME_ParentId);

			AssertEquals(TextStandardizerHelper.ComputeStringHashFast(valueNameToHash), patternName.PMN_HashedValue);
			AssertEquals(address.PK, patternName.PMN_ParentId);

			AssertContainsExactElementsInAnyOrder(
				new ZInt[]
				{
					TextStandardizerHelper.ComputeStringHashFast("61449743938"),
					TextStandardizerHelper.ComputeStringHashFast("3892322"),
					TextStandardizerHelper.ComputeStringHashFast("61416021377")
				},
				Factory.Load<PatternMatchingPhone>(new ZQuery(PatternMatchingPhoneSchema.PMP_ParentId, address.PK))
					.Select(p => p.PMP_HashedValue).ToArray());
			AssertEquals(address.PK, patternMatchingPhones[0].PMP_ParentId);
		}

		protected override void SetupBizO()
		{
			BizO.OA_Address1 = "72 O'Riordan Street";
			BizO.OA_Address2 = "";
			BizO.OA_City = "SYDNEY";
			BizO.OA_PostCode = "2015";
			BizO.OA_State = "NSW";
			BizO.OA_Email = "scott@wisetechglobal.com";
			BizO.OA_CompanyNameOverride = "Linfox Pty";
			BizO.OA_Phone = "+61449743938";
			BizO.OA_Fax = "3892322";
			BizO.OA_Mobile = "+61416021377";
			BizO.OA_RN_NKCountryCode = "AU";

			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestMaintainPatternData_WhenOrgAddressCountryCode_IsNull()
		{
			//	Arrange
			var maintenance = new PatternMatchingOrgAddressMaintenance(BizO);

			//	Act
			var result = maintenance.CreateOrUpdatePatternMatchingTables();
		}
	}
}
