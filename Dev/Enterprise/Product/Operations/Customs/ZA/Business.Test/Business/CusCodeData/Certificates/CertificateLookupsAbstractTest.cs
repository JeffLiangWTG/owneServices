using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	abstract class CertificateLookupsAbstractTest<T> : BusinessObjectLookupsTestCase where T : CertificateCusCodeData
	{
		public void TestCY_CodeList()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var helper = new PermitTestDataHelper(Factory);
			var permit1 = helper.CreatePermitHeader(importer.PK, "TEST", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), PermitQtyValIndicatorList.Codes.VAL, PermitType, ZString.Empty, 1000m, 1000m);
			var permit2 = helper.CreatePermitHeader(importer.PK, "TEST2", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var collection = GetCertificateCollection(entryInstruction);
			var cert = collection.AddNew();
			var certificates = cert.Lookups.CY_CodeList;
			Assert(certificates.ContainsCode(permit1.CPH_Number));
			Assert(!certificates.ContainsCode(permit2.CPH_Number));
		}

		public void TestCertificates()
		{
			var certificates = SetupCertificateTest();
			if (ExpectedPermitTypes.Length == 1)
			{
				AssertEquals(PermitType + " Permit", PermitType, certificates.FilterBusinessObjectDefaults[CusPermitHeaderCollection.FilterConstants.PermitTypeSubType + ":Property1"].Value);
			}
			else
			{
				int i = 1;
				foreach (var permitType in ExpectedPermitTypes)
				{
					AssertEquals(PermitType + " Permit", permitType, certificates.FilterBusinessObjectDefaults[CusPermitHeaderCollection.FilterConstants.PermitTypeSubType + ":Property1:" + i++].Value);
				}
			}
		}

		protected abstract ZString PermitType { get; }

		protected abstract CusCodeDataCollection<T> GetCertificateCollection(CusEntryInstruction instruction);

		protected abstract ZString[] ExpectedPermitTypes { get; }

		PermitFindBoxCollection SetupCertificateTest()
		{
			var importer = Factory.New<OrgHeader>();
			var permit1 = Factory.NewWithValidTestData<CusPermitHeader>();
			permit1.CPH_OH_PermitHolder = importer.PK;
			permit1.CPH_Type = PermitType;
			var rule1 = permit1.CusPermitRules.AddNew();
			rule1.CPR_RuleCode = PermitRuleCodeList.Codes.TAR;
			var permit2 = Factory.NewWithValidTestData<CusPermitHeader>();
			permit1.CPH_OH_PermitHolder = importer.PK;
			permit2.CPH_Type = PermitTypeList.Codes.IMP;
			var rule2 = permit2.CusPermitRules.AddNew();
			rule2.CPR_RuleCode = PermitRuleCodeList.Codes.TAR;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var collection = GetCertificateCollection(entryInstruction);
			var cert = collection.AddNew();
			var certificates = cert.Lookups.Certificates;
			return certificates;
		}
	}
}
