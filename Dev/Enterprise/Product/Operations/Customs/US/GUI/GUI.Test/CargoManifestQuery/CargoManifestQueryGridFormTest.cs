using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(CargoManifestQueryGridForm))]
	sealed class CargoManifestQueryGridFormTest : ZFormBasherTest
	{
		public void TestIsGroupQueriesCheckBox()
		{
			var header = new CargoManifestQueryHeader(Factory);
			using (var form = new CargoManifestQueryGridForm(header))
			{
				form.Show();
				var isGroupQueriesCheckBox = form.FindSingle<ZCheckBox>("IsGroupQueriesCheckBox");
				Assert("Not ticked default", !isGroupQueriesCheckBox.Checked);
			}
		}

		public void TestClickSendButtonWhenInvalid()
		{
			using (var form = (CargoManifestQueryGridForm)GetFormToBash())
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.BusinessEntity.ActionCode = ZString.Empty;
				AssertHasErrorContaining(form.BusinessEntity.ActionCodeInfo, "Action Code");
				var sendButton = form.FindSingle<ZButton>("SendButton");
				sendButton.PerformClick();
				AssertEquals("Please fix the errors first", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.None, form.DialogResult);
				form.BusinessEntity.ActionCode = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;
				form.BusinessEntity.SendingObjects.OfType<CargoManifestQuerySendingObject>().ToList().ForEach(x => x.ShouldSendMessage = false);
				sendButton.PerformClick();
				AssertEquals("There is nothing to send.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.None, form.DialogResult);
			}
		}

		public void TestClickSendButtonForCargoManifestQuery()
		{
			var header = new CargoManifestQueryHeader(Factory);
			using (var form = new CargoManifestQueryGridForm(header))
			{
				form.Show();
				header.ActionCode = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;
				var obj = header.SendingObjects.AddNew();
				obj.Issuer = "TEST";
				obj.MasterBillNumber = "TESTMASTER";
				obj.OutputOption = LimitOutputCodeList.Codes._2AllAvailableResults;
				var sendButton = form.FindSingle<ZButton>("SendButton");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				sendButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestLevelsGridColumnsVisibilityAndOrders()
		{
			var header = new CargoManifestQueryHeader(Factory);
			using (var form = new CargoManifestQueryGridForm(header))
			{
				form.Show();
				var columns = form.FindSingle<ZGrid>("LevelsGrid").Columns;
				ZGridColumn col1, col2, col3, col4;
				col1 = columns[0];
				col2 = columns[1];
				CombineAssertions(() =>
				{
					AssertEquals(2, columns.Count);
					AssertEquals(CargoManifestQueryBizObj.Schema.RequestForRelatedBills, col1.ColumnName);
					AssertEquals(CargoManifestQueryBizObj.Schema.OutputOption, col2.ColumnName);
				});
				header.ActionCode = CargoManifestStatusQueryActionList.Codes.AIR;
				col1 = columns[0];
				col2 = columns[1];
				col3 = columns[2];
				col4 = columns[3];
				CombineAssertions(() =>
				{
					AssertEquals(4, columns.Count);
					AssertEquals(CargoManifestQueryBizObj.Schema.MasterBillNumber, col1.ColumnName);
					AssertEquals(CargoManifestQueryBizObj.Schema.HouseBillNumber, col2.ColumnName);
					AssertEquals(CargoManifestQueryBizObj.Schema.RequestForRelatedBills, col3.ColumnName);
					AssertEquals(CargoManifestQueryBizObj.Schema.OutputOption, col4.ColumnName);
					AssertEquals("Master Bill Number", col1.ColumnStyle.HeaderText);
					AssertEquals("House Bill Number", col2.ColumnStyle.HeaderText);
					AssertEquals("Request For Related Bills", col3.ColumnStyle.HeaderText);
					AssertEquals("Output Option", col4.ColumnStyle.HeaderText);
				});
				header.ActionCode = CargoManifestStatusQueryActionList.Codes.InBond;
				col1 = columns[0];
				col2 = columns[1];
				col3 = columns[2];
				CombineAssertions(() =>
				{
					AssertEquals(3, columns.Count);
					AssertEquals(CargoManifestQueryBizObj.Schema.InBondNumber, col1.ColumnName);
					AssertEquals(CargoManifestQueryBizObj.Schema.RequestForRelatedBills, col2.ColumnName);
					AssertEquals(CargoManifestQueryBizObj.Schema.OutputOption, col3.ColumnName);
					AssertEquals("In-Bond Bill Number", col1.ColumnStyle.HeaderText);
					AssertEquals("Request For Related Bills", col2.ColumnStyle.HeaderText);
					AssertEquals("Output Option", col3.ColumnStyle.HeaderText);
				});
				header.ActionCode = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;
				col1 = columns[0];
				col2 = columns[1];
				col3 = columns[2];
				col4 = columns[3];
				CombineAssertions(() =>
				{
					AssertEquals(4, columns.Count);
					AssertEquals(CargoManifestQueryBizObj.Schema.Issuer, col1.ColumnName);
					AssertEquals(CargoManifestQueryBizObj.Schema.MasterBillNumber, col2.ColumnName);
					AssertEquals(CargoManifestQueryBizObj.Schema.RequestForRelatedBills, col3.ColumnName);
					AssertEquals(CargoManifestQueryBizObj.Schema.OutputOption, col4.ColumnName);
					AssertEquals("Issuer Code", col1.ColumnStyle.HeaderText);
					AssertEquals("Bill Number", col2.ColumnStyle.HeaderText);
					AssertEquals("Request For Related Bills", col3.ColumnStyle.HeaderText);
					AssertEquals("Output Option", col4.ColumnStyle.HeaderText);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = new CargoManifestQueryHeader(Factory);
			return new CargoManifestQueryGridForm(header);
		}
	}
}
