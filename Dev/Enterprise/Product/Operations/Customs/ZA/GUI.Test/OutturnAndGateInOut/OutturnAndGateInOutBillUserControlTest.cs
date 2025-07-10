using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(OutturnAndGateInOutBillUserControl))]
	sealed class OutturnAndGateInOutBillUserControlTest : TestCaseWithFactory
	{
		public void TestzTextBoxCustomsCPC_Visible()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new ZForm();
			using var userControl = new OutturnAndGateInOutBillUserControl();
			form.Controls.Add(userControl);
			userControl.SetDataBinding(header, "");
			form.Show();

			AssertEquals(true, userControl.zTextBoxCustomsCPC.Visible);
			header.AMA_Nature = NatureList.Codes.Import23;
			AssertEquals(true, userControl.zTextBoxCustomsCPC.Visible);
			header.AMA_Nature = NatureList.Codes.Transhipment28;
			AssertEquals(true, userControl.zTextBoxCustomsCPC.Visible);
			header.AMA_Nature = NatureList.Codes.Transit24;
			AssertEquals(true, userControl.zTextBoxCustomsCPC.Visible);
			header.AMA_Nature = NatureList.Codes.FreightRemainingOnBoard;
			AssertEquals(true, userControl.zTextBoxCustomsCPC.Visible);
			header.AMA_Nature = NatureList.Codes.Export22;
			AssertEquals(true, userControl.zTextBoxCustomsCPC.Visible);
		}

		public void TestzTextBoxLRN_Visible()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new ZForm();
			using var userControl = new OutturnAndGateInOutBillUserControl();
			form.Controls.Add(userControl);
			userControl.SetDataBinding(header, "");
			form.Show();

			AssertEquals(true, userControl.zTextBoxLRN.Visible);
			header.AMA_Nature = NatureList.Codes.Import23;
			AssertEquals(true, userControl.zTextBoxLRN.Visible);
			header.AMA_Nature = NatureList.Codes.Transhipment28;
			AssertEquals(true, userControl.zTextBoxLRN.Visible);
			header.AMA_Nature = NatureList.Codes.Transit24;
			AssertEquals(true, userControl.zTextBoxLRN.Visible);
			header.AMA_Nature = NatureList.Codes.FreightRemainingOnBoard;
			AssertEquals(true, userControl.zTextBoxLRN.Visible);
			header.AMA_Nature = NatureList.Codes.Export22;
			AssertEquals(true, userControl.zTextBoxLRN.Visible);
		}

		public void TestColumnCustomsCPC_Availability()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new ZForm();
			using var userControl = new OutturnAndGateInOutBillUserControl();
			form.Controls.Add(userControl);
			userControl.SetDataBinding(header, "");
			form.Show();

			AssertEquals(true, userControl.zGridBills.Columns.Contains("CustomsCPC"));
			header.AMA_Nature = NatureList.Codes.Import23;
			AssertEquals(true, userControl.zGridBills.Columns.Contains("CustomsCPC"));
			header.AMA_Nature = NatureList.Codes.Transhipment28;
			AssertEquals(true, userControl.zGridBills.Columns.Contains("CustomsCPC"));
			header.AMA_Nature = NatureList.Codes.Transit24;
			AssertEquals(true, userControl.zGridBills.Columns.Contains("CustomsCPC"));
			header.AMA_Nature = NatureList.Codes.FreightRemainingOnBoard;
			AssertEquals(true, userControl.zGridBills.Columns.Contains("CustomsCPC"));
			header.AMA_Nature = NatureList.Codes.Export22;
			AssertEquals(true, userControl.zGridBills.Columns.Contains("CustomsCPC"));
		}

		public void TestColumnLRN_Availability()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new ZForm();
			using var userControl = new OutturnAndGateInOutBillUserControl();
			form.Controls.Add(userControl);
			userControl.SetDataBinding(header, "");
			form.Show();

			AssertEquals(true, userControl.zGridBills.Columns.Contains("LRN"));
			header.AMA_Nature = NatureList.Codes.Import23;
			AssertEquals(true, userControl.zGridBills.Columns.Contains("LRN"));
			header.AMA_Nature = NatureList.Codes.Transhipment28;
			AssertEquals(true, userControl.zGridBills.Columns.Contains("LRN"));
			header.AMA_Nature = NatureList.Codes.Transit24;
			AssertEquals(true, userControl.zGridBills.Columns.Contains("LRN"));
			header.AMA_Nature = NatureList.Codes.FreightRemainingOnBoard;
			AssertEquals(true, userControl.zGridBills.Columns.Contains("LRN"));
			header.AMA_Nature = NatureList.Codes.Export22;
			AssertEquals(true, userControl.zGridBills.Columns.Contains("LRN"));
		}
	}
}
