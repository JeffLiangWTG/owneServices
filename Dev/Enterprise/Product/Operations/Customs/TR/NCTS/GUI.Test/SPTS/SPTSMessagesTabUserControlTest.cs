using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	sealed class SPTSMessagesTabUserControlTest : TestCaseWithFactory
	{
		public void TestInitializeGrid()
		{
			using (var form = new ZForm())
			using (var control = new SPTSMessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var messageGrid = control.FindSingle<ZGrid>("MessageGrid");
				AssertEquals(EDIMessage.Schema.EM_MessageNum, ((ZGridColumnInfo)messageGrid.ColumnStyles[0]).ColumnName);
				AssertEquals(EDIMessage.Schema.EM_MessageDateTime, ((ZGridColumnInfo)messageGrid.ColumnStyles[1]).ColumnName);
				AssertEquals(EDIMessage.Schema.EM_MessageType, ((ZGridColumnInfo)messageGrid.ColumnStyles[2]).ColumnName);
				AssertEquals(EDIMessage.Schema.EM_MessageSubType, ((ZGridColumnInfo)messageGrid.ColumnStyles[3]).ColumnName);
				AssertEquals(EDIMessage.Schema.EM_Status, ((ZGridColumnInfo)messageGrid.ColumnStyles[4]).ColumnName);
				AssertEquals(EDIMessage.Schema.EM_ReceiveTransmit, ((ZGridColumnInfo)messageGrid.ColumnStyles[5]).ColumnName);
				AssertEquals(EDIMessage.Schema.EM_SystemCreateUser, ((ZGridColumnInfo)messageGrid.ColumnStyles[6]).ColumnName);
				AssertEquals("EM_CreateUserFullName", ((ZGridColumnInfo)messageGrid.ColumnStyles[7]).ColumnName);
				AssertEquals(EDIMessage.Schema.EM_SystemCreateTimeUtc, ((ZGridColumnInfo)messageGrid.ColumnStyles[8]).ColumnName);
				AssertEquals(EDIMessage.Schema.EM_InterchangeNumber, ((ZGridColumnInfo)messageGrid.ColumnStyles[9]).ColumnName);
				AssertEquals(EDIMessage.Schema.EM_InterchangeStatus, ((ZGridColumnInfo)messageGrid.ColumnStyles[10]).ColumnName);
			}
		}

		public void TestMessageInterpretation()
		{
			using (var form = new ZForm())
			using (var control = new SPTSMessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var interpretationControl = control.Controls.Find("MessageInterpretationWebBrowser", true).FirstOrDefault();
				AssertNotNull(interpretationControl);
				Assert("There is MessageInterpretationWebBrowser anymore", interpretationControl.Visible);
			}
		}
	}
}
