using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class REQDOCMenuGenerationTest : TestCaseWithFactory
	{
		readonly string responseMessage = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00505655KFN20160602000001'
GIS+8:120:ZZZ'
ERP+1'
ERC+0000'
FTX+AAO+++CUSDEC-00505655KFN20160602000001, Ver=, TraderRef=224915: :CUSDEC TimeStamp = 20160704162512:DocType=830, MFC=9'
FTX+AAO+++CUSRES-00505655KFN20160602000001, Ver=0, TraderRef=224915:SARS Ref = 458078155b64c8b49node1:CUSDEC TimeStamp = 20160704162512, CUSRES Processed=2016070416252:Status=1:Recipient=00626126TST'
FTX+AAO+++CUSRES-00505655KFN20160602000001, Ver=0, TraderRef=224915:SARS Ref = 458078155b64c8b49node1:CUSDEC TimeStamp = 20160704162512, CUSRES Processed=20160704162522:Status=1:Recipient=00626126TST'
FTX+AAO+++CUSDEC-00505655KFN20160602000001, Ver=0, TraderRef=224969: :CUSDEC TimeStamp=20160705112046:DocType=830, MFC=4'
FTX+AAO+++CUSRES-00505655KFN20160602000001, Ver=1, TraderRef=224969:SARS Ref=614043155ba5c3b80node1:CUSDEC TimeStamp=20160705112046, CUSRES Processed=20160705112057:Status=13:Recipient=00626126TST'
FTX+AAO+++CUSRES-00505655KFN20160602000001, Ver=1, TraderRef=224969:SARS Ref=552700155ba6de52cnode1:CUSDEC TimeStamp=20160705112046, CUSRES Processed=20160705114015:Status=33:Recipient=00626126TST'
UNT+11+1'".Replace("\r\n", "");
		public void TestReqdocMenuGeneration()
		{
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.AddNew("CDP", "TST", Core.Constants.CountryCodes.SouthAfrica);
			testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "00626126", Core.Constants.CountryCodes.SouthAfrica);
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			testHelper.CreateCustomsStatusCusCodeEntry("13");
			testHelper.CreateCustomsStatusCusCodeEntry("33");
			Factory.Save();
			var collection = new FinancialAccountNumberPortMapCollection();
			var mapping = collection.AddNew();
			mapping.OrganizationPK = testAgent.PK;
			mapping.CustomsOfficeCode = "BFN";
			mapping.FinancialAccountNumber = "3234002346";
			mapping.ImporterPays = true;
			mapping.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = "IMP";
			declaration.JE_OH_AgentOverride = testAgent.PK;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "00";
			new LineMerger(declaration).DoMerge();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "00505655KFN20160602000001";
			entryHeader.CH_Packages = 1;
			CombineAssertions(() =>
			{
				using (ZForm form = new ZForm())
				using (MessageUserControl userControl = new MessageUserControl())
				{
					form.Controls.Add(userControl);
					userControl.SetDataBinding(declaration, "");
					form.Show();
					var reqdocRoot = userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Request Customs Resend of Responses", false);
					userControl.RefreshReqdocList();
					AssertEquals("Has Menu Item", 1, reqdocRoot.MenuItems.Count);
					AssertEquals("Has Latest Response Menu Item", "Latest Response", reqdocRoot.MenuItems[0].Text);
					reqdocRoot.MenuItems[0].PerformClick();
					AssertEquals("Please select a single Entry to Request a REQDOC", UnitTestUserNotification.Instance.LastMessage.Text);
					userControl.EntriesBoundGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessages();
					reqdocRoot.MenuItems[0].PerformClick();
					AssertEquals("Please save the job before sending messages to Customs.", UnitTestUserNotification.Instance.LastMessage.Text);
					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessages();
					reqdocRoot.MenuItems[0].PerformClick();
					AssertEquals("Create REQDOC message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					reqdocRoot.MenuItems[0].PerformClick();
					AssertEquals("Create REQDOC message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
					var testMessage = Factory.NewWithValidTestData<ZAMessage>();
					testMessage.EM_ApplicationCode = "ZAC";
					testMessage.EM_MessageType = "RSQ";
					testMessage.EM_MessageSubType = "XXX";
					testMessage.EM_ReceiveTransmit = "RCV";
					testMessage.EM_MessageNum = "1";
					testMessage.EM_Status = "QUE";
					testMessage.EM_MessageText = responseMessage;
					entryHeader.Messages.Add(testMessage);
					Factory.Save();
					AssertNoExceptionThrown(() =>
					{
						userControl.RefreshReqdocList();
					});
					AssertEquals("Has Several Menu Items", 6, reqdocRoot.MenuItems.Count);
					AssertEquals("Several Menu Items Has Latest Response", "Latest Response", reqdocRoot.MenuItems[0].Text);
					AssertEquals("Menu Item Text 01", "-", reqdocRoot.MenuItems[1].Text);
					AssertEquals("Menu Item Text 02", "Status 1 - Release (<Invalid>)", reqdocRoot.MenuItems[2].Text);
					AssertEquals("Menu Item Text 03", "Status 1 - Release (04-Jul-16 16:25:22)", reqdocRoot.MenuItems[3].Text);
					AssertEquals("Menu Item Text 04", "Status 13 - Query - Supporting documents required (05-Jul-16 11:20:57)", reqdocRoot.MenuItems[4].Text);
					AssertEquals("Menu Item Text 05", "Status 33 - Supporting documents received (05-Jul-16 11:40:15)", reqdocRoot.MenuItems[5].Text);
				}
			});
		}
	}
}
