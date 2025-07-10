using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class MessagesTabUserControlTest : TestCaseWithFactory
	{
		public void TestPermitMenuPresent()
		{
			using (var userControl = new MessagesTabUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				userControl.SetDataBinding(declaration, "");
				MenuItem found = new ZMenuItem();
				found.Text = "Not found yet";
				foreach (MenuItem candidate in userControl.FindSingle<ZGrid>("MessagesGrid").ContextMenu.MenuItems)
				{
					if (candidate.Text == "Print Permit")
					{
						found = candidate;
					}
				}

				AssertNotEquals("Not found yet", found.Text);
				found.PerformClick();
				AssertEquals("Select a message to print.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPermitMenuWorking()
		{
			using (var form = new ZForm(Declaration.CustomsEntryHeaders[0].Messages))
			using (var userControl = new MessagesTabUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				Declaration.CustomsEntryHeaders[0].Messages[0].EM_MessageText = "UNH+1+CUSPMT:0:1:RT:040+OUTUPT'BGM+962:::BKT+XXXXXXXXE47T        200609220011+32'CST++2+AMR'LOC+11+LW222:::TESTING47,28 JOO KOON CIRCLE SINGAPORE 629057'LOC+12+AEFJR'LOC+61+AEFJR'LOC+88+PPLW:::PASIR PANJANG LIGHTER WHARVES, PASIR PANJANG LIGHTER WHARVES'LOC+130+AEFJR'LOC+164+WPN:::WESTERN PETROLUME ANCHORAGE'DTM+136:20060923:102'GEI+5+:Y'MEA+ABK++PKG:2.0000'MEA+AAH++TNE:0.0150'MEA+AAN++:13712.00'FTX+ACF+++AMENDMENTS FOR TOTAL OUTER PACK'RFF+ABT:OX6I000689A'DTM+160:20060922:102'DTM+182:20060922:102'DTM+273:2006092220061005:718'DTM+9:200609220852:203'RFF+AEA:SC'FTX+CCI+++GA APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING:WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID.:FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.'FTX+CCI+++TX THE GOODS DECLARED IN THIS PERMIT ARE IMPORTED/EXPORTED:BY A TAXABLE PERSON'FTX+CCI+++J6 THE GOODS AND THIS PERMIT MUST BE PRODUCED TO CUSTOMS AT:KEPPEL/JURONG/PASIR PANJANG/SEMBAWANG FTZ 'IN' GATE FOR:CUSTOMS CLEARANCE & ENDORSEMENT.FOR GDS RELEASED OR ENTERING:LOB, THE GDS & PERMIT MUST BE PRODUCED AT LOB SECURITY BOOTH'FTX+CCI+++A4 THE FOLLOWING ACKNOWLEDGEMENT MUST BE OBTAINED FROM THE:MASTER/CAPTAIN OF VESSEL,CHIEF CLERK, MALAYAN RAILWAY (KTM),:STOREKEEPER OR POSTMASTER?: --- I CERTIFY THAT I HAVE TO-DAY:RECEIVED ........ PACKAGES/CONTAINERS OF GOODS AS DECLARED.'FTX+CCI+++   ..............................   ........................:VESSEL?'S NAME /GODOWN NO.           DESIGNATION & SIGNATURE:...........    .........       .............................:     DATE        TIME          FULL NAME IN BLOCK LETTERS'FTX+CCI+++AY GOODS NOT EXPORTED/TRANSHIPPED OR BONDED IN A:LICENSED WAREHOUSE OR RECEIVED BY THE CLAIMANT ON THE SAME:DAY OF REMOVAL MUST BE STORED AT A PLACE APPROVED BY A:PROPER OFFICER OF CUSTOMS.'FTX+CCI+++A5 THIS PERMIT MUST BE RETURNED TO PERMITS COMPLIANCE UNIT,:CUSTOMS HQ WITHIN 4 WORKING DAYS AFTER IT HAS BEEN USED. A:COPY OF MPA?'S PORT CLEARANCE MUST BE FURNISHED FOR EXPORT:VIA LOB.'FTX+CCI+++A6 IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR:RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.'FTX+CCI+++J2 AT THE TIME OF DELIVERY, INSERT VEHICLE-NO?:..............:AND (IF APPLICABLE) LOCAL CRAFT-NO/NAME?:....................'FTX+CCI+++J3 VESSEL MUST HAVE A PROPER STORE/COMPARTMENT WHICH CAN BE:SEALED BY CUSTOMS WITH WIRE AND LEAD SEAL.'FTX+CCI+++   ********** END OF CARGO CLEARANCE PERMIT **********'FTX+CUS+++016000000000MF'TDT+12++1+:::CV++++6:::ARCTIC BREEZE'TPL+::::MV'DOC+704+NA'NAD+CA+10042360000C++INCHAPE SHIPPING SERVICES (S) PTE:LTD'NAD+EX+XXXXXXXXE47T++TESTING47'NAD+DT++USER47'CTA+IC+:1619574Z'COM+12345678:TE'NAD+BB'RFF+DAN:D'UNS+D'CST+1+24022090+169'FTX+AAA+++MARLBORO LIGHT CIGS'FTX+PRD+++MARLBORO'LOC+18+SNTB3132'LOC+27+PH'MEA+AAF++KGM:10.0000'MEA+AAE++STK:10000.0000'MEA+AAI++STK:1.0000'MEA+ABA++KGM:10.0000'PAC+1+3+CAR'PAC+50+2+BOX'PAC+10+1+PKT'PAC+20+5+STK'MOA+63:800.00'RFF+AEA:SEASTORE'GIN+AV+019+30'DOC+714+TestOut'UNS+S'CNT+5:1'CNT+6:1'CNT+22:1'TAX+1'MOA+63:800.00'UNT+67+1'";
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);
				MenuItem found = new ZMenuItem();
				foreach (MenuItem candidate in messagesGrid.ContextMenu.MenuItems)
				{
					if (candidate.Text == "Print Permit")
					{
						found = candidate;
					}
				}

				found.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestInterchangeMenuWorking()
		{
			using (var form = new ZForm(Declaration.CustomsEntryHeaders[0].Messages))
			using (var userControl = new MessagesTabUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var interchange = Factory.New<EDIInterchange>();
				interchange.EI_Status = EDIInterchange.Status.Queued;
				Declaration.CustomsEntryHeaders[0].Messages[0].EM_EI = interchange.PK;
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);
				DoResendInterchangeClick(messagesGrid);
				AssertEquals("This interchange is already queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTrySendMessageWithoutInterchange()
		{
			using (var form = new ZForm(Declaration.CustomsEntryHeaders[0].Messages))
			using (var userControl = new MessagesTabUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);
				DoResendInterchangeClick(messagesGrid);
				AssertEquals("This message has no interchange, thus it cannot be resent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTryReSendIncomingInterchange()
		{
			using (var form = new ZForm(Declaration.CustomsEntryHeaders[0].Messages))
			using (var userControl = new MessagesTabUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				Declaration.CustomsEntryHeaders[0].Messages[0].EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				messagesGrid.Select(0);
				DoResendInterchangeClick(messagesGrid);
				AssertEquals("Only outgoing interchanges can be resent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTryReSendCorrectInterchange()
		{
			using (var form = new ZForm(Declaration.CustomsEntryHeaders[0].Messages))
			using (var userControl = new MessagesTabUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var interchange = Factory.New<EDIInterchange>();
				interchange.EI_Status = EDIInterchange.Status.Sent;
				Declaration.CustomsEntryHeaders[0].Messages[0].EM_EI = interchange.PK;
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);
				DoResendInterchangeClick(messagesGrid);
				AssertEquals("Interchange could not be resent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTrySendInterchangeWhereMoreThanOneIsSelected()
		{
			using (var form = new ZForm(Declaration.CustomsEntryHeaders[0].Messages))
			using (var userControl = new MessagesTabUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var interchange = Factory.New<EDIInterchange>();
				interchange.EI_Status = EDIInterchange.Status.Sent;
				Declaration.CustomsEntryHeaders[0].Messages[0].EM_EI = interchange.PK;
				Declaration.CustomsEntryHeaders[0].Messages.AddNew();
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.SelectAllElements();
				DoResendInterchangeClick(messagesGrid);
				AssertEquals("Select 1 message before trying to resend the interchange.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEM_ApplicationVisible()
		{
			using (var form = new ZForm(Declaration.CustomsEntryHeaders[0].Messages))
			using (var userControl = new MessagesTabUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				Assert(messagesGrid.Columns[EDIMessageSchema.Constants.EM_ApplicationReference] != null);
				Assert(messagesGrid.Columns[EDIMessageSchema.Constants.EM_ApplicationReference].IsVisible);
			}
		}

		public void TestEM_ApplicationCaption()
		{
			using (var form = new ZForm(Declaration.CustomsEntryHeaders[0].Messages))
			using (var userControl = new MessagesTabUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				Assert(messagesGrid.Columns[EDIMessageSchema.Constants.EM_ApplicationReference] != null);
				AssertEquals("URN", messagesGrid.Columns[EDIMessageSchema.Constants.EM_ApplicationReference].ColumnStyle.HeaderText);
			}
		}

		void DoResendInterchangeClick(ZGrid grid)
		{
			foreach (MenuItem candidate in grid.ContextMenu.MenuItems)
			{
				if (candidate.Text == "Resend Interchange")
				{
					candidate.PerformClick();
					return;
				}
			}

			return;
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
					var message = entryHeader.Messages.AddNew();
					message.EM_MessageType = Cuspmt09bMessageProcessor.MessageType;
					message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
	}
}
