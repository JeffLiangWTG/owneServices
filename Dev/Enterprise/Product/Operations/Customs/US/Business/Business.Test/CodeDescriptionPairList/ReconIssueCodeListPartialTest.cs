namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconIssueCodeListTest : NUnit.Framework.TestCase
	{
		public void TestICodeDescriptionPairListProviderMembers()
		{
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider list = new ReconIssueCodeList();
			AssertEquals(list, list.GetCodeDescriptionPairList());
		}

		public void TestConvertToENSCode()
		{
			AssertEquals("001", ReconIssueCodeList.ConvertToENSOtherIssueCode(ReconIssueCodeList.Codes.ValueRecon));
			AssertEquals("002", ReconIssueCodeList.ConvertToENSOtherIssueCode(ReconIssueCodeList.Codes.ClassRecon));
			AssertEquals("003", ReconIssueCodeList.ConvertToENSOtherIssueCode(ReconIssueCodeList.Codes._9802Recon));
			AssertEquals("004", ReconIssueCodeList.ConvertToENSOtherIssueCode(ReconIssueCodeList.Codes.ValueClassRecon));
			AssertEquals("005", ReconIssueCodeList.ConvertToENSOtherIssueCode(ReconIssueCodeList.Codes.Value9802Recon));
			AssertEquals("006", ReconIssueCodeList.ConvertToENSOtherIssueCode(ReconIssueCodeList.Codes.Class9802Recon));
			AssertEquals("007", ReconIssueCodeList.ConvertToENSOtherIssueCode(ReconIssueCodeList.Codes.ValueClass9802Recon));
			AssertEquals("NA - 008 needs to still return blank when converting to ENS code", "", ReconIssueCodeList.ConvertToENSOtherIssueCode(ReconIssueCodeList.Codes.NotApplicable));
			AssertEquals("", ReconIssueCodeList.ConvertToENSOtherIssueCode(ReconIssueCodeList.Codes.FTA));
		}

		public void TestConvertFromENSCode()
		{
			AssertEquals(ReconIssueCodeList.Codes.ValueRecon, ReconIssueCodeList.ConvertFromENSIssueCode("001"));
			AssertEquals(ReconIssueCodeList.Codes.ClassRecon, ReconIssueCodeList.ConvertFromENSIssueCode("002"));
			AssertEquals(ReconIssueCodeList.Codes._9802Recon, ReconIssueCodeList.ConvertFromENSIssueCode("003"));
			AssertEquals(ReconIssueCodeList.Codes.ValueClassRecon, ReconIssueCodeList.ConvertFromENSIssueCode("004"));
			AssertEquals(ReconIssueCodeList.Codes.Value9802Recon, ReconIssueCodeList.ConvertFromENSIssueCode("005"));
			AssertEquals(ReconIssueCodeList.Codes.Class9802Recon, ReconIssueCodeList.ConvertFromENSIssueCode("006"));
			AssertEquals(ReconIssueCodeList.Codes.ValueClass9802Recon, ReconIssueCodeList.ConvertFromENSIssueCode("007"));
			AssertEquals(ReconIssueCodeList.Codes.NotApplicable, ReconIssueCodeList.ConvertFromENSIssueCode("008"));
		}

		public void TestIsValidForConsumptionQuotaVisaEntryType()
		{
			AssertEquals(false, ReconIssueCodeList.IsValidForConsumptionQuotaVisaEntryType(ReconIssueCodeList.Codes.ClassRecon));
			AssertEquals(true, ReconIssueCodeList.IsValidForConsumptionQuotaVisaEntryType(ReconIssueCodeList.Codes._9802Recon));
			AssertEquals(true, ReconIssueCodeList.IsValidForConsumptionQuotaVisaEntryType(ReconIssueCodeList.Codes.ValueRecon));
		}

		public void TestIsClassificationRecon()
		{
			AssertEquals(true, ReconIssueCodeList.IsClassificationRecon(ReconIssueCodeList.Codes.ClassRecon));
			AssertEquals(true, ReconIssueCodeList.IsClassificationRecon(ReconIssueCodeList.Codes.Class9802Recon));
			AssertEquals(true, ReconIssueCodeList.IsClassificationRecon(ReconIssueCodeList.Codes.ValueClass9802Recon));
			AssertEquals(true, ReconIssueCodeList.IsClassificationRecon(ReconIssueCodeList.Codes.ValueClassRecon));
			AssertEquals(false, ReconIssueCodeList.IsClassificationRecon(ReconIssueCodeList.Codes.ValueRecon));
			AssertEquals(false, ReconIssueCodeList.IsClassificationRecon(ReconIssueCodeList.Codes._9802Recon));
			AssertEquals(false, ReconIssueCodeList.IsClassificationRecon(ReconIssueCodeList.Codes.Value9802Recon));
			AssertEquals(false, ReconIssueCodeList.IsClassificationRecon(ReconIssueCodeList.Codes.FTA));
		}

		public void TestNeedSendSPI()
		{
			AssertEquals(true, ReconIssueCodeList.NeedSendSPI(ReconIssueCodeList.Codes.FTA));
			AssertEquals(false, ReconIssueCodeList.NeedSendSPI(ReconIssueCodeList.Codes.ClassRecon));
			AssertEquals(false, ReconIssueCodeList.NeedSendSPI(ReconIssueCodeList.Codes.Class9802Recon));
			AssertEquals(false, ReconIssueCodeList.NeedSendSPI(ReconIssueCodeList.Codes.ValueClass9802Recon));
			AssertEquals(false, ReconIssueCodeList.NeedSendSPI(ReconIssueCodeList.Codes.ValueClassRecon));
			AssertEquals(false, ReconIssueCodeList.NeedSendSPI(ReconIssueCodeList.Codes.ValueRecon));
			AssertEquals(false, ReconIssueCodeList.NeedSendSPI(ReconIssueCodeList.Codes._9802Recon));
			AssertEquals(false, ReconIssueCodeList.NeedSendSPI(ReconIssueCodeList.Codes.Value9802Recon));
		}

		public void NeedSendTariff()
		{
			AssertEquals(false, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes.FTA, false));
			AssertEquals(false, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes.FTA, true));
			AssertEquals(true, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes.ClassRecon, false));
			AssertEquals(true, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes.ClassRecon, true));
			AssertEquals(true, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes.Class9802Recon, false));
			AssertEquals(true, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes.Class9802Recon, true));
			AssertEquals(true, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes.ValueClass9802Recon, false));
			AssertEquals(true, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes.ValueClass9802Recon, true));
			AssertEquals(true, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes.ValueClassRecon, false));
			AssertEquals(true, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes.ValueClassRecon, true));
			AssertEquals(false, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes.ValueRecon, false));
			AssertEquals(true, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes.ValueRecon, true));
			AssertEquals(false, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes._9802Recon, false));
			AssertEquals(false, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes._9802Recon, true));
			AssertEquals(false, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes.Value9802Recon, false));
			AssertEquals(true, ReconIssueCodeList.NeedSendTariff(ReconIssueCodeList.Codes.Value9802Recon, true));
		}

		public void TestNeedSendHTSChangedDueToValue()
		{
			AssertEquals(false, ReconIssueCodeList.NeedSendHTSChangedDueToValue(ReconIssueCodeList.Codes.FTA));
			AssertEquals(false, ReconIssueCodeList.NeedSendHTSChangedDueToValue(ReconIssueCodeList.Codes.ClassRecon));
			AssertEquals(false, ReconIssueCodeList.NeedSendHTSChangedDueToValue(ReconIssueCodeList.Codes.Class9802Recon));
			AssertEquals(true, ReconIssueCodeList.NeedSendHTSChangedDueToValue(ReconIssueCodeList.Codes.ValueClass9802Recon));
			AssertEquals(true, ReconIssueCodeList.NeedSendHTSChangedDueToValue(ReconIssueCodeList.Codes.ValueClassRecon));
			AssertEquals(true, ReconIssueCodeList.NeedSendHTSChangedDueToValue(ReconIssueCodeList.Codes.ValueRecon));
			AssertEquals(false, ReconIssueCodeList.NeedSendHTSChangedDueToValue(ReconIssueCodeList.Codes._9802Recon));
			AssertEquals(true, ReconIssueCodeList.NeedSendHTSChangedDueToValue(ReconIssueCodeList.Codes.Value9802Recon));
		}

		public void TestGetReconIssuesValue()
		{
			var code = ReconIssueCodeList.GetReconIssuesValue(ReconIssueCodeList.Codes._9802Recon);
			AssertEquals(ReconIssues._98, code);

			code = ReconIssueCodeList.GetReconIssuesValue(ReconIssueCodeList.Codes.Class9802Recon);
			AssertEquals(ReconIssues._98 | ReconIssues.CL, code);

			code = ReconIssueCodeList.GetReconIssuesValue(ReconIssueCodeList.Codes.Value9802Recon);
			AssertEquals(ReconIssues._98 | ReconIssues.VL, code);

			code = ReconIssueCodeList.GetReconIssuesValue(ReconIssueCodeList.Codes.ValueClass9802Recon);
			AssertEquals(ReconIssues._98 | ReconIssues.VL | ReconIssues.CL, code);

			code = ReconIssueCodeList.GetReconIssuesValue(ReconIssueCodeList.Codes.ClassRecon);
			AssertEquals(ReconIssues.CL, code);

			code = ReconIssueCodeList.GetReconIssuesValue(ReconIssueCodeList.Codes.ValueClassRecon);
			AssertEquals(ReconIssues.CL | ReconIssues.VL, code);

			code = ReconIssueCodeList.GetReconIssuesValue(ReconIssueCodeList.Codes.ValueRecon);
			AssertEquals(ReconIssues.VL, code);
		}
	}
}
