using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbPortDeliveryTimeForm))]
	sealed class TestGlbPortDeliveryTimeForm : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			GlbPortDeliveryTime deliveryTime = Factory.New<GlbPortDeliveryTime>();
			return new GlbPortDeliveryTimeForm(deliveryTime);
		}

		[RequiresSTA]
		public void TestSaveGlbPortDeliveryTimeWithDuplication()
		{
			var deliverTime1 = Factory.NewWithValidTestData<GlbPortDeliveryTime>();
			deliverTime1.G1_FreightMode = "FCL";
			deliverTime1.G1_JobMode = "FWD";
			deliverTime1.G1_RL_NKDestinationPort = "USLAX";
			deliverTime1.G1_RL_NKDischargePort = "USCHI";
			deliverTime1.G1_DaysDelayFromArrivalToDeliver = 2;
			deliverTime1.G1_DaysDelayFromArrivalToDeliver = 2;
			Factory.Save();

			var deliverTime2 = Factory.NewWithValidTestData<GlbPortDeliveryTime>();
			using (var deliverForm = new GlbPortDeliveryTimeForm(deliverTime2))
			{
				deliverTime2.G1_FreightMode = "FCL";
				deliverTime2.G1_JobMode = "FWD";
				deliverTime2.G1_RL_NKDestinationPort = "USLAX";
				deliverTime2.G1_RL_NKDischargePort = "USCHI";
				deliverTime2.G1_DaysDelayFromArrivalToDeliver = 2;
				deliverTime2.G1_DaysDelayFromArrivalToDeliver = 2;
				deliverForm.FireSaveButton();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
