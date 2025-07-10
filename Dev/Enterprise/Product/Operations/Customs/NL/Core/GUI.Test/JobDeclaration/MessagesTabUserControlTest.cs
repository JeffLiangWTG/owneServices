using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

class MessagesTabUserControlTest : TestCaseWithFactory
{
	public void TestGridColumns()
	{
		var testDec = Factory.New<JobDeclaration>();

		using (var form = new ZForm(testDec))
		using (var userControl = new MessageUserControl())
		{
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(testDec, ".");
			form.Show();

			var messagesGrid = (ZGrid)userControl.Controls.Find("MessagesGrid", true).First();

			CombineAssertions(() =>
			{
				AssertNotNull("User control should have EM_MessageNum column", messagesGrid.Columns[EDIMessage.Schema.EM_MessageNum]);
				AssertNotNull("User control should have EM_MessageType column", messagesGrid.Columns[EDIMessage.Schema.EM_MessageType]);
				AssertNotNull("User control should have EM_MessageSubType column", messagesGrid.Columns[EDIMessage.Schema.EM_MessageSubType]);
				AssertNotNull("User control should have EM_MessageDateTime column", messagesGrid.Columns[EDIMessage.Schema.EM_MessageDateTime]);
				AssertNotNull("User control should have EM_SystemCreateTimeUtc column", messagesGrid.Columns[EDIMessage.Schema.EM_SystemCreateTimeUtc]);
				AssertNotNull("User control should have EM_InterchangeNumber column", messagesGrid.Columns[EDIMessage.Schema.EM_InterchangeNumber]);
				AssertNotNull("User control should have EM_DateTimeInterchangeSent column", messagesGrid.Columns[EDIMessage.Schema.EM_DateTimeInterchangeSent]);
				AssertNotNull("User control should have EM_User column", messagesGrid.Columns[EDIMessage.Schema.EM_User]);
				AssertNotNull("User control should have EM_Status column", messagesGrid.Columns[EDIMessage.Schema.EM_Status]);
				AssertNotNull("User control should have EM_ReceiveTransmit column", messagesGrid.Columns[EDIMessage.Schema.EM_ReceiveTransmit]);
				AssertNotNull("User control should have EntryStatus column", messagesGrid.Columns["EntryStatus"]);
				AssertNotNull("User control should have EntryHeaderStatusDescription column", messagesGrid.Columns["EntryStatusDescription"]);
				AssertNotNull("User control should have EM_HeldUntilDate column", messagesGrid.Columns[EDIMessage.Schema.EM_HeldUntilDate]);
				AssertNotNull("User control should have EM_IsTestMessage column", messagesGrid.Columns[EDIMessage.Schema.EM_IsTestMessage]);
			});
		}
	}
}
