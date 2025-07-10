using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	sealed class MessagesTabUserControlTest : TestCaseWithFactory
	{
		public void TestEM_CreateUserFullNameAvailable()
		{
			var columns = control.MessageGrid_Exposed.ColumnStyles.Cast<ZGridColumnInfo>();
			var expectedColumn = "EM_CreateUserFullName";
			Assert("EM_CreateUserFullName should be added.", columns.Any(x => x.ColumnName == expectedColumn));
		}

		public void TestVisibleAndColumnsInSortOrder()
		{
			for (int i = 0; i < ExpectedColumnNamesInSortOrderList.Count; i++)
			{
				ZGridColumnInfo columnInfo = control.MessageGrid_Exposed.ColumnStyles[i] as ZGridColumnInfo;
				AssertNotNull(columnInfo);
				ZString expectedColumnName = ExpectedColumnNamesInSortOrderList[i];
				AssertEquals("Expected", expectedColumnName, columnInfo.ColumnName);
				AssertEquals("Visible", true, columnInfo.IsVisible);
			}
		}

		List<ZString> ExpectedColumnNamesInSortOrderList
		{
			get
			{
				if (fExpectedColumnNamesInSortOrderList == null)
				{
					fExpectedColumnNamesInSortOrderList = new List<ZString>();
					fExpectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageNum);
					fExpectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageDateTime);
					fExpectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageType);
					fExpectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_MessageSubType);
					fExpectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_ReceiveTransmit);
					fExpectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_SystemCreateUser);
					fExpectedColumnNamesInSortOrderList.Add("EM_CreateUserFullName");
					fExpectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_SystemCreateTimeUtc);
					fExpectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_InterchangeNumber);
					fExpectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_InterchangeStatus);
					fExpectedColumnNamesInSortOrderList.Add(EDIMessage.Schema.EM_ApplicationReference);
				}
				return fExpectedColumnNamesInSortOrderList;
			}
		}
		List<ZString> fExpectedColumnNamesInSortOrderList;

		class MessageTabUserControlForTest : MessagesTabUserControl
		{
			public ZGrid MessageGrid_Exposed => MessageGrid;
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new MessageTabUserControlForTest();
		}
		MessageTabUserControlForTest control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
