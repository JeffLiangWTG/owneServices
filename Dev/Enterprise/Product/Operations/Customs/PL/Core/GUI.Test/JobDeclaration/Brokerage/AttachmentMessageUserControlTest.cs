using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

class AttachmentMessageUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new AttachmentMessageUserControl())
		{
			CombineAssertions(() =>
			{
				AssertNotNull("MessageGridGroupBox", control.FindSingleOrDefault<ZGroupBox>("MessageGridGroupBox"));
				AssertNotNull("MessageGrid", control.FindSingleOrDefault<Messaging.GUI.MessageZGrid>("MessageGrid"));
				AssertNotNull("MessageTextGroupBox", control.FindSingleOrDefault<ZGroupBox>("MessageTextGroupBox"));
				AssertNotNull("MessageTextBoundTextBox", control.FindSingleOrDefault<ZTextBox>("MessageTextBoundTextBox"));
			});
		}
	}

	public void TestMessageGrid()
	{
		using (var control = new AttachmentMessageUserControl())
		{
			var grid = control.FindSingleOrDefault<Messaging.GUI.MessageZGrid>("MessageGrid");

			CombineAssertions(() =>
			{
				AssertEquals("Grid should have 10 columns", 10, grid.ColumnStyles.Count);
				AssertEquals("EM_MessageType", ((MessageCodeColumnStyleInfo)grid.ColumnStyles[0]).ColumnName);
				AssertEquals("EM_MessageNum", ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[1]).ColumnName);
				AssertEquals("EM_MessageDateTime", ((ZDateEditColumnStyleInfo)grid.ColumnStyles[2]).ColumnName);
				AssertEquals("EM_SystemCreateTimeUtc", ((ZDateEditColumnStyleInfo)grid.ColumnStyles[3]).ColumnName);
				AssertEquals("EM_InterchangeNumber", ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[4]).ColumnName);
				AssertEquals("EM_DateTimeInterchangeSent", ((ZDateEditColumnStyleInfo)grid.ColumnStyles[5]).ColumnName);
				AssertEquals("EM_User", ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[6]).ColumnName);
				AssertEquals("EM_Status", ((MessageCodeColumnStyleInfo)grid.ColumnStyles[7]).ColumnName);
				AssertEquals("EM_ReceiveTransmit", ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[8]).ColumnName);
				AssertEquals("EM_MessageSubType", ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[9]).ColumnName);
			});
		}
	}
}
