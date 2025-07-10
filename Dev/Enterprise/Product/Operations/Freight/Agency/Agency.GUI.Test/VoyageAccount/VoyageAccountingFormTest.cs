using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class VoyageAccountingFormTest : BaseAgencyTest
	{
		[GuiTest]
		public void TestNA_JVInfo_ValueChanged()
		{
			List<IExchangeRate> expected = new List<IExchangeRate>();
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			VoyageAccount voyageAccount = Factory.NewWithValidTestData<VoyageAccount>();
			voyageAccount.NA_OH = principal.PK;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Test Vessel";
			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			voyage.JV_OH_Line = principal.PK;
			JobHeader job = new JobHeader.Loader(voyageAccount).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			VoyageExRate rate = Factory.New<VoyageExRate>();
			rate.E8_RX_NKExCurrency = "USD";
			rate.E8_VoyageExchangeRate = 1.54m;
			voyage.ExRates.Add(rate);
			expected.Add(rate);
			rate = Factory.New<VoyageExRate>();
			rate.E8_RX_NKExCurrency = "UAH";
			rate.E8_VoyageExchangeRate = 0.2m;
			voyage.ExRates.Add(rate);
			expected.Add(rate);
			Factory.Save();
			using (ZForm form = new VoyageAccountingForm(voyageAccount))
			{
				form.Show();
				ZGrid grid = (ZGrid)(form.Controls.Find("JobExRateBoundGrid", true)[0]);
				AssertEquals("ExRates should be Empty", 0, grid.List.Count);
				voyageAccount.NA_JV = voyage.PK;
				AssertEquals("ExRates should NOT be Empty", 2, grid.List.Count);
				for (int i = 0; i < grid.List.Count; i++)
				{
					AssertEquals("Currency Codes should be equal", expected[i].CurrencyCode, ((RefCurrency)((BusinessObject)grid.List[i])["RateCurrency"]).RX_Code);
					AssertEquals("Rates should be equal", expected[i].Rate, ((BusinessObject)grid.List[i])["JF_SellRate"]);
				}
			}
		}

		[GuiTest]
		public void TestVoyageButton_Click()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Test Vessel";
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			VoyageAccount voyageAccount = Factory.NewWithValidTestData<VoyageAccount>();
			voyageAccount.NA_OH = principal.PK;
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			voyage.JV_OH_Line = principal.PK;
			voyageAccount.NA_JV = voyage.PK;
			JobHeader job = new JobHeader.Loader(voyageAccount).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			AccTransactionLines lines = Factory.NewWithValidTestData<AccTransactionLines>();
			charge.JR_AL_ARLine = lines.PK;
			lines = Factory.NewWithValidTestData<AccTransactionLines>();
			charge.JR_AL_APLine = lines.PK;
			charge.APLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			using (ZForm form = new VoyageAccountingForm(voyageAccount))
			{
				form.Show();
				ZButton button = (ZButton)(form.Controls.Find("selectVoyageButton", true)[0]);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				button.PerformClick();
				AssertEquals("Should be Information message", "Unable to choose another Sailing Schedule because Voyage Accounting job has been invoiced.", UnitTestUserNotification.Instance.LastMessage.Text);
				charge.APLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				button.PerformClick();
				AssertEquals("Should NOT be Information message", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}
	}
}
