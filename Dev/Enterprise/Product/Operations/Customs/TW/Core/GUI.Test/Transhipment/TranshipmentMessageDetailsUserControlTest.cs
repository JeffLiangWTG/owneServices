using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.TW.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	public sealed class TranshipmentMessageDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestMessagesGrid()
		{
			var inBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			using (var form = new TranshipmentForm(inBondHeader))
			{
				form.Show();
				var tabControl = form.Controls.Find("MainTabControl", true)[0] as ZTabControl;
				var messagesTabPage = tabControl.Controls.Find("MessagesTabPage", true)[0] as ZTabPage;
				tabControl.SelectTab(messagesTabPage);
				var messageDetailsUserControl = messagesTabPage.Controls.Find("MessageDetailsUserControl", true)[0] as TranshipmentMessageDetailsUserControl;
				AssertNotNull(messageDetailsUserControl);
				var messagesGrid = messageDetailsUserControl.Controls.Find("InBondMessagesGrid", true)[0] as ZGrid;
				CombineAssertions(() =>
				{
					AssertGridColumnInfo((ZGridColumnInfo)messagesGrid.ColumnStyles[0], "EM_MessageType", "Message Type", 100);
					AssertGridColumnInfo((ZGridColumnInfo)messagesGrid.ColumnStyles[1], "MessageCode", "Message Code", 100);
					AssertGridColumnInfo((ZGridColumnInfo)messagesGrid.ColumnStyles[2], "MessageTypeDescription", "Message Type Description", 160);
					AssertGridColumnInfo((ZGridColumnInfo)messagesGrid.ColumnStyles[3], "EM_Status", "Status", 80);
					AssertGridColumnInfo((ZGridColumnInfo)messagesGrid.ColumnStyles[4], "EM_ReceiveTransmit", "Direction", 80);
					AssertGridColumnInfo((ZGridColumnInfo)messagesGrid.ColumnStyles[5], "EM_SystemCreateTimeUtc", "Create Time UTC", 120);
					AssertGridColumnInfo((ZGridColumnInfo)messagesGrid.ColumnStyles[6], "EM_SystemCreateUser", "Create User", 80);
					Assert("ReadOnly", messagesGrid.ReadOnly);
					AssertEquals("Count", 7, messagesGrid.ColumnStyles.Count);
				}

				);
			}
		}

		void AssertGridColumnInfo(ZGridColumnInfo columnInfo, ZString columnName, ZString columnCaption, ZInt columnWidth)
		{
			AssertEquals(columnName + " ColumnName", columnName, columnInfo.ColumnName);
			AssertEquals(columnName + " Caption", columnCaption, columnInfo.Caption);
			var calculatedWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(columnWidth);
			AssertEquals(columnName + " Width = " + columnWidth, calculatedWidth, columnInfo.Width);
		}

		public void TestMessageDetails()
		{
			var inBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			using (var form = new TranshipmentForm(inBondHeader))
			{
				form.Show();
				var tabControl = form.Controls.Find("MainTabControl", true)[0] as ZTabControl;
				var messagesTabPage = tabControl.Controls.Find("MessagesTabPage", true)[0] as ZTabPage;
				tabControl.SelectTab(messagesTabPage);
				var messageDetailsUserControl = messagesTabPage.Controls.Find("MessageDetailsUserControl", true)[0] as TranshipmentMessageDetailsUserControl;
				var interpretationText = messageDetailsUserControl.Controls.Find("HtmlInterpretationBox", true)[0] as HtmlInterpretationBox;
				AssertContains(EDIMessage.Schema.EM_MessageInterpretation, interpretationText.DataBindings[0].BindingMemberInfo.BindingField);
			}
		}

		public void TestMessageText()
		{
			var inBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			using (var form = new TranshipmentForm(inBondHeader))
			{
				form.Show();
				var tabControl = form.Controls.Find("MainTabControl", true)[0] as ZTabControl;
				var messagesTabPage = tabControl.Controls.Find("MessagesTabPage", true)[0] as ZTabPage;
				tabControl.SelectTab(messagesTabPage);
				var messageDetailsUserControl = messagesTabPage.Controls.Find("MessageDetailsUserControl", true)[0] as TranshipmentMessageDetailsUserControl;
				AssertNotNull(messageDetailsUserControl);
				var messagesGrid = messageDetailsUserControl.Controls.Find("InBondMessagesGrid", true)[0] as ZGrid;
				AssertEquals(0, messagesGrid.VisibleRowCount);
				var detailsTabControl = messageDetailsUserControl.Controls.Find("InBondMessageTabControl", true)[0] as ZTabControl;
				var textTabPage = messageDetailsUserControl.Controls.Find("InBondMessageTextTabPage", true)[0] as ZTabPage;
				detailsTabControl.SelectTab(textTabPage);
				var messageTextTextBox = messageDetailsUserControl.Controls.Find("InBondMessageTextTextBox", true)[0] as ZTextBox;
				CombineAssertions(() =>
				{
					AssertEquals(@"", messageTextTextBox.Text);
					AssertEquals("HideSelection", false, messageTextTextBox.HideSelection);
					AssertEquals("EnableFindDialog", true, messageTextTextBox.EnableFindDialog);
				});
				var message1 = inBondHeader.Messages.AddNew();
				message1.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?><Declaration xsi:schemaLocation=""urn:wco:datamodel:TW:N5301:R-01-01 ..\maindoc\N5301.xsd"" xmlns=""urn:wco:datamodel:TW:N5301:R-01-01"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><FunctionCode>9</FunctionCode><ID>AA0858000001</ID><TotalGrossMassMeasure>257850</TotalGrossMassMeasure><TotalPackageQuantity>9</TotalPackageQuantity><TypeCode>T2</TypeCode><Agent><ID>580</ID><RoleCode>CB</RoleCode><tw_SubBoxID>A</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-09-20</ArrivalDateTime><ID>3EEV</ID><JourneyID>14</JourneyID><Name>DREAMANGEL</Name><tw_Registration>07AW97</tw_Registration><TypeCode>1</TypeCode></BorderTransportMeans><Carrier><ID>96944490</ID></Carrier><Consignment><tw_ManifestSerialNumber>1002</tw_ManifestSerialNumber><tw_ShippingOrderNumber>9002</tw_ShippingOrderNumber><ConsignmentItem><Commodity><CargoDescription>1UNITOF&apos;SK210&apos;NEWEXCAVATOR1UNITOF&apos;SK200-8&apos;NEWEXCAVATOR1UNITOF&apos;SK200-8&apos;2UNITSOF&apos;SK350LC-8&apos;NEWHYDRAULICEXCAVATOR1UNITOF&apos;SK-200-8&apos;3UNITSOF&apos;SK350LC-8&apos;NEWEXCAVATOR</CargoDescription></Commodity><GoodsMeasure><TariffQuantity>9</TariffQuantity><tw_UnitCode>UNT</tw_UnitCode></GoodsMeasure><Packaging><MarksNumbers>KONGCHUNMACHINERYTRADINGCOMPANYYQ12-09986CASENO.1YN12-65612PKGNO.1*****YC11-06521CASEYC11-06523CASEYN12-65615PKG***NO.1*****YC11-06518CASEYC11-06519CASEYC11-06520CASEYN12-65611PKG***NO.1HONGKONGMADEINJAPAN</MarksNumbers><tw_Combination>N</tw_Combination><TypeCode>UNT</TypeCode></Packaging><TransportContractDocument><ID>HSHK180AL999</ID><TypeCode>704</TypeCode></TransportContractDocument></ConsignmentItem><DepartureTransportMeans><ID>3FUG8</ID><Name>POSITIVEPASSION</Name><tw_JourneyID>104</tw_JourneyID><tw_Registration>07AW95</tw_Registration></DepartureTransportMeans><LoadingLocation><ID>JPHIJ</ID></LoadingLocation><TransitTransportMeans><TypeCode>12</TypeCode></TransitTransportMeans><TransportContractDocument><ID>HSHK180AL999</ID><TypeCode>704</TypeCode></TransportContractDocument></Consignment><LoadingLocation><ID>TPEE03TS</ID></LoadingLocation><RepresentativePerson><Name>00650</Name></RepresentativePerson><tw_Applicant><tw_ChineseName>東立物流股份有限公司</tw_ChineseName><tw_ID>80279759</tw_ID><tw_Name>TONGLITLOGISTICSCO.,LTD.</tw_Name><tw_TypeCode>186</tw_TypeCode></tw_Applicant><UnloadingLocation><ID>TPEE03TS</ID></UnloadingLocation></Declaration>";
				message1.EM_FormattedMessageTextInfo.RefreshBinding(); // shouldnt have to do this!
				AssertEquals(1, messagesGrid.VisibleRowCount);
				messagesGrid.Select(0);
				AssertEquals(@"<?xml version=""1.0"" encoding=""UTF-8""?><Declaration xsi:schemaLocation=""urn:wco:datamodel:TW:N5301:R-01-01 ..\maindoc\N5301.xsd"" xmlns=""urn:wco:datamodel:TW:N5301:R-01-01"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><FunctionCode>9</FunctionCode><ID>AA0858000001</ID><TotalGrossMassMeasure>257850</TotalGrossMassMeasure><TotalPackageQuantity>9</TotalPackageQuantity><TypeCode>T2</TypeCode><Agent><ID>580</ID><RoleCode>CB</RoleCode><tw_SubBoxID>A</tw_SubBoxID></Agent><BorderTransportMeans><ArrivalDateTime>2019-09-20</ArrivalDateTime><ID>3EEV</ID><JourneyID>14</JourneyID><Name>DREAMANGEL</Name><tw_Registration>07AW97</tw_Registration><TypeCode>1</TypeCode></BorderTransportMeans><Carrier><ID>96944490</ID></Carrier><Consignment><tw_ManifestSerialNumber>1002</tw_ManifestSerialNumber><tw_ShippingOrderNumber>9002</tw_ShippingOrderNumber><ConsignmentItem><Commodity><CargoDescription>1UNITOF&apos;SK210&apos;NEWEXCAVATOR1UNITOF&apos;SK200-8&apos;NEWEXCAVATOR1UNITOF&apos;SK200-8&apos;2UNITSOF&apos;SK350LC-8&apos;NEWHYDRAULICEXCAVATOR1UNITOF&apos;SK-200-8&apos;3UNITSOF&apos;SK350LC-8&apos;NEWEXCAVATOR</CargoDescription></Commodity><GoodsMeasure><TariffQuantity>9</TariffQuantity><tw_UnitCode>UNT</tw_UnitCode></GoodsMeasure><Packaging><MarksNumbers>KONGCHUNMACHINERYTRADINGCOMPANYYQ12-09986CASENO.1YN12-65612PKGNO.1*****YC11-06521CASEYC11-06523CASEYN12-65615PKG***NO.1*****YC11-06518CASEYC11-06519CASEYC11-06520CASEYN12-65611PKG***NO.1HONGKONGMADEINJAPAN</MarksNumbers><tw_Combination>N</tw_Combination><TypeCode>UNT</TypeCode></Packaging><TransportContractDocument><ID>HSHK180AL999</ID><TypeCode>704</TypeCode></TransportContractDocument></ConsignmentItem><DepartureTransportMeans><ID>3FUG8</ID><Name>POSITIVEPASSION</Name><tw_JourneyID>104</tw_JourneyID><tw_Registration>07AW95</tw_Registration></DepartureTransportMeans><LoadingLocation><ID>JPHIJ</ID></LoadingLocation><TransitTransportMeans><TypeCode>12</TypeCode></TransitTransportMeans><TransportContractDocument><ID>HSHK180AL999</ID><TypeCode>704</TypeCode></TransportContractDocument></Consignment><LoadingLocation><ID>TPEE03TS</ID></LoadingLocation><RepresentativePerson><Name>00650</Name></RepresentativePerson><tw_Applicant><tw_ChineseName>東立物流股份有限公司</tw_ChineseName><tw_ID>80279759</tw_ID><tw_Name>TONGLITLOGISTICSCO.,LTD.</tw_Name><tw_TypeCode>186</tw_TypeCode></tw_Applicant><UnloadingLocation><ID>TPEE03TS</ID></UnloadingLocation></Declaration>", messageTextTextBox.Text);
			}
		}
	}
}
