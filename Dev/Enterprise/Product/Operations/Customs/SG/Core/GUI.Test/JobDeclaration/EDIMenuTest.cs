using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class EDIMenuTest : TestCaseWithFactory
	{
		public void TestMessagingMenuItemsVisibility_Interface()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			using (var ediMenu = GetEDIMenu())
			{
				ediMenu.RefreshMenu();
				CombineAssertions(() =>
				{
					AssertEquals("separatorMenuItem", false, ediMenu.separatorMenuItem.Visible);
					AssertEquals("resetToWorkingMenuItem", false, ediMenu.resetToWorkingMenuItem.Visible);
					AssertEquals("sendCancellationMenuItem", false, ediMenu.sendCancellationMenuItem.Visible);
					AssertEquals("sendRefundMenuItem", false, ediMenu.sendRefundMenuItem.Visible);
					AssertEquals("sendAmendmentMenuItem", false, ediMenu.sendAmendmentMenuItem.Visible);
					AssertEquals("sendDeclarationMenuItem", false, ediMenu.sendDeclarationMenuItem.Visible);
				});
			}
		}

		public void TestMessagingMenuItemsVisibility_NotInterfaced()
		{
			using (var ediMenu = GetEDIMenu())
			{
				ediMenu.RefreshMenu();
				CombineAssertions(() =>
				{
					AssertEquals("separatorMenuItem", true, ediMenu.separatorMenuItem.Visible);
					AssertEquals("resetToWorkingMenuItem", true, ediMenu.resetToWorkingMenuItem.Visible);
					AssertEquals("sendCancellationMenuItem", true, ediMenu.sendCancellationMenuItem.Visible);
					AssertEquals("sendRefundMenuItem", true, ediMenu.sendRefundMenuItem.Visible);
					AssertEquals("sendAmendmentMenuItem", true, ediMenu.sendAmendmentMenuItem.Visible);
					AssertEquals("sendDeclarationMenuItem", true, ediMenu.sendDeclarationMenuItem.Visible);
				});
			}
		}

		public void TestCanSendAnyMessage_Errors_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				AssertEquals("PreCondition", 0, Declaration.ActiveEntryHeaders.Count);
				Declaration.JE_OH_Forwarder = ZGuid.Invalid;
				Click_SendDeclaration(ediMenu);
				AssertEquals("Error after form validation", 0, Declaration.ActiveEntryHeaders.Count);
			}
		}

		public void TestSendDeclaration_CanSendAnyMessage_MessageErrors_SG4()
		{
			using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSendDeclaration_CanSendAnyMessage_MessageErrors("SG4"); //4.1
			}
		}

		public void TestSendDeclaration_CanSendAnyXmlMessage()
		{
			AssertSendDeclaration_CanSendAnyMessage_MessageErrors("SGX");
		}

		public void TestSendDeclaration_CanSendAnyMessage_MessageErrors_SG4BOTH_SendBothForSG4AndNTP()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (EDIMenu ediMenu = GetEDIMenu())
			{
				AssertEquals("PreCondition", 0, Declaration.ActiveEntryHeaders.Count);
				Click_SendDeclaration(ediMenu, true, DialogResult.Cancel);
				AssertEquals("Message Errors after form validation", 0, Declaration.ActiveEntryHeaders.Count);
				Click_SendDeclaration(ediMenu, true, DialogResult.OK);
				AssertEquals("Message Errors after form validation", 1, Declaration.ActiveEntryHeaders.Count);
				AssertEquals("SG4 message has been sent", 1, Declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SG4" }, Declaration.ActiveEntryHeaders[0].Messages.Cast<EDIMessage>().Select(x => x.EM_ApplicationCode));
			}
		}

		public void TestSendDeclaration_CanSendAnyMessage_MessageErrors_SG4BOTH_SendSG4OnlyWhenMissingNTPCredential()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (SGCustomsDataRegistry.Instance.EnableXMLTradeNetMessaging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (EDIMenu ediMenu = GetEDIMenu())
			{
				AssertEquals("PreCondition", 0, Declaration.ActiveEntryHeaders.Count);
				DoSetBrokerDetailsWithSG4Credential();
				Click_SendDeclaration(ediMenu, false, DialogResult.Cancel);
				AssertEquals("Message Errors after form validation", 0, Declaration.ActiveEntryHeaders.Count);
				Click_SendDeclaration(ediMenu, false, DialogResult.OK);
				AssertEquals("Message Errors after form validation", 1, Declaration.ActiveEntryHeaders.Count);
				AssertEquals("Only SG4 message has been sent", 1, Declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("SG4", Declaration.ActiveEntryHeaders[0].Messages[0].EM_ApplicationCode);
			}
		}

		public void TestSendDeclaration_ZSaveException()
		{
			Declaration.Factory.Save();
			var factory2 = new BusinessObjectFactory()
			{ RefreshEnabled = false };
			Declaration.Factory.RefreshEnabled = false;
			var declaration2 = factory2.Load<JobDeclaration>(Declaration.PK);
			var storageMain2 = declaration2.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration2, "TST");
			var storageMain = Declaration.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration2, "TST");
			factory2.Save();
			declaration2.DocManagerInfo.MasterFactory.Save();
			AssertEquals(storageMain.ParentFK, storageMain2.ParentFK);
			Assert(((BusinessObject)storageMain2).IsInDatabase);
			using (EDIMenu ediMenu = GetEDIMenu())
			{
				AssertEquals("PreCondition", 0, Declaration.ActiveEntryHeaders.Count);
				Click_SendDeclaration(ediMenu, true, DialogResult.OK);
				AssertEquals("One message has been sent", 1, Declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals("While you were working, the eDocs for this record were modified. The system will now need to merge this information. Press OK to have this information loaded and then try saving again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendDeclaration_DeclarationNotYetMade_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			AssertSendDeclaration_DeclarationNotYetMade();
		}

		public void TestSendDeclaration_DeclarationNotYetMade_NTP()
		{
			AssertSendDeclaration_DeclarationNotYetMade();
		}

		public void TestSendDeclaration_PermitNotYetReceived_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
				Click_SendDeclaration(ediMenu);
				AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationPending, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		public void TestSendDeclaration_AwaitingResponse_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.DeclarationSent;
				try
				{
					Click_SendDeclaration(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nCurrently awaiting a response from Singapore Customs.\r\n", e.Message);
				}
			}
		}

		public void TestSendDeclaration_PermitReceived_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
				try
				{
					Click_SendDeclaration(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nA permit has already been received.\r\n", e.Message);
				}
			}
		}

		public void TestSendDeclaration_ConcurrencyError_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				var entryHeader = (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
				entryHeader.Factory.RefreshEnabled = false;
				entryHeader.CH_EntryStatus = "XXX";
				entryHeader.Factory.Save();
				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;
				var concurrentEntry = factory2.Load<CusEntryHeader>(entryHeader.PK);
				concurrentEntry.CH_EntryStatus = "AAA";
				factory2.Save();
				AssertEquals(Core.SGConstants.DeclarationStatus.JobOpenButNoMessageSent, Declaration.ActiveEntryHeaders[0].CH_Status);
				Click_SendDeclaration(ediMenu);
				AssertEquals("Message should not be sent due to concurrency error", Core.SGConstants.DeclarationStatus.JobOpenButNoMessageSent, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		public void TestSendAmendment_CanSendAnyMessage_MessageErrors_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
				Click_SendAmendment(ediMenu, true, DialogResult.Cancel);
				AssertEquals(Core.SGConstants.DeclarationStatus.DeclarationPermitReceived, Declaration.ActiveEntryHeaders[0].CH_Status);
				Click_SendAmendment(ediMenu);
				AssertEquals(Core.SGConstants.DeclarationStatus.AmendmentPending, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		public void TestSendAmendment_DeclarationNotYetMade_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				AssertEquals("PreCondition", 0, Declaration.ActiveEntryHeaders.Count);
				try
				{
					Click_SendAmendment(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nA declaration has not yet been sent.\r\n", e.Message);
				}
			}
		}

		public void TestSendAmendment_PermitNotYetReceived_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
				try
				{
					Click_SendAmendment(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nA permit has not yet been received.\r\n", e.Message);
				}
			}
		}

		public void TestSendAmendment_AwaitingResponse_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.CancellationSent;
				try
				{
					Click_SendAmendment(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nCurrently awaiting a response from Singapore Customs.\r\n", e.Message);
				}
			}
		}

		public void TestAssertSendAmendment_Cancelled_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
				try
				{
					Click_SendAmendment(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nThe permit has already been cancelled.\r\n", e.Message);
				}
			}
		}

		public void TestSendAmendment_DeclarationPermitReceived_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
				Click_SendAmendment(ediMenu);
				AssertEquals(Core.SGConstants.DeclarationStatus.AmendmentPending, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		public void TestSendAmendment_AmendmentPermitReceived_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
				Click_SendAmendment(ediMenu);
				AssertEquals(Core.SGConstants.DeclarationStatus.AmendmentPending, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		public void TestSendAmendment_AmendmentRejected_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms;
				Click_SendAmendment(ediMenu);
				AssertEquals(Core.SGConstants.DeclarationStatus.AmendmentPending, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		[TestDate(2009, 1, 1)]
		public void TestSendDeclarationPostUENCutover_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "12247970000Z";
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				AssertEquals("Pre-condition", Core.SGConstants.DeclarationStatus.JobOpenButNoMessageSent, Declaration.ActiveEntryHeaders[0].CH_Status);
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "12247970000Z";
				var wrapper = SGGlbStaffWrapper.Get(GlbStaff.CurrentUser);
				wrapper.Tradenetv4Password.GP_CurrentPassword = "0KKwb6ydAlmfdLUP+jgqwUGsR6GLqfWCNrg9I0zCnfI=";
				wrapper.Tradenetv4Password.GP_MailBoxID = "TEST";
				GlbStaff.CurrentUser.GS_WorkPhone = "TEST";
				GlbStaff.CurrentUser.GS_RN_NKNationalityCode = "X";
				wrapper.Tradenetv4Password.GP_UserID = "TEST";
				GlbStaff.CurrentUser.GS_Passport = "TEST";
				Click_SendDeclaration(ediMenu, false);
				AssertEquals("Sending should not have been allowed", Core.SGConstants.DeclarationStatus.JobOpenButNoMessageSent, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		public void TestSendRefund_CanSendAnyMessage_MessageErrors_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
				Click_SendRefund(ediMenu, true, DialogResult.Cancel);
				AssertEquals(Core.SGConstants.DeclarationStatus.RefundPending, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		public void TestSendRefund_DeclarationNotYetMade_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				AssertEquals("PreCondition", 0, Declaration.ActiveEntryHeaders.Count);
				try
				{
					Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
					Click_SendRefund(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nA declaration has not yet been sent.\r\n", e.Message);
				}
			}
		}

		public void TestSendRefund_DeclarationWrongType_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				AssertEquals("PreCondition", 0, Declaration.ActiveEntryHeaders.Count);
				try
				{
					Click_SendRefund(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nRefund is only valid for a inward payment (IPT) declaration.\r\n", e.Message);
				}
			}
		}

		public void TestSendRefund_PermitNotYetReceived_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
				try
				{
					Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
					Click_SendRefund(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nA permit has not yet been received.\r\n", e.Message);
				}
			}
		}

		public void TestSendRefund_AwaitingResponse_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.CancellationSent;
				try
				{
					Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
					Click_SendRefund(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nCurrently awaiting a response from Singapore Customs.\r\n", e.Message);
				}
			}
		}

		public void TestSendRefund_Cancelled_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
				try
				{
					Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
					Click_SendRefund(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nThe permit has already been cancelled.\r\n", e.Message);
				}
			}
		}

		public void TestSendRefund_DeclarationPermitReceived_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
				Click_SendRefund(ediMenu);
				AssertEquals(Core.SGConstants.DeclarationStatus.RefundPending, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		public void TestSendRefund_RefundPermitReceived_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
				Click_SendRefund(ediMenu);
				AssertEquals(Core.SGConstants.DeclarationStatus.RefundPending, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		public void TestSendRefund_RefundRejected_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms;
				Click_SendRefund(ediMenu);
				AssertEquals(Core.SGConstants.DeclarationStatus.RefundPending, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		public void TestSendCancellation_CanSendAnyMessage_MessageErrors_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
				Click_SendCancellation(ediMenu, true, DialogResult.Cancel);
				AssertEquals(Core.SGConstants.DeclarationStatus.CancellationPending, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		public void TestSendCancellation_DeclarationNotYetMade_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				AssertEquals("PreCondition", 0, Declaration.ActiveEntryHeaders.Count);
				try
				{
					Click_SendCancellation(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nA declaration has not yet been sent.\r\n", e.Message);
				}
			}
		}

		public void TestSendCancellation_PermitNotYetReceived_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
				try
				{
					Click_SendCancellation(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nA permit has not yet been received.\r\n", e.Message);
				}
			}
		}

		public void TestSendCancellation_AwaitingResponse_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.AmendmentSent;
				try
				{
					Click_SendCancellation(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nCurrently awaiting a response from Singapore Customs.\r\n", e.Message);
				}
			}
		}

		public void TestSendCancellation_AlreadyCancelled_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
				try
				{
					Click_SendCancellation(ediMenu);
					Assert(false);
				}
				catch (ApplicationException e)
				{
					AssertEquals("Error : Sending is not allowed.\r\n\r\nThe permit has already been cancelled.\r\n", e.Message);
				}
			}
		}

		public void TestSendCancellation_DeclarationPermitReceived_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
				Click_SendCancellation(ediMenu);
				AssertEquals(Core.SGConstants.DeclarationStatus.CancellationPending, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		public void TestSendCancellation_AmendmentPermitReceived_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.AmendmentPermitReceived;
				Click_SendCancellation(ediMenu);
				AssertEquals(Core.SGConstants.DeclarationStatus.CancellationPending, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		public void TestSendCancellation_AmendmentRejected_SG4()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			using (var ediMenu = GetEDIMenu())
			{
				Declaration.DoMerge();
				Declaration.ActiveEntryHeaders[0].CH_Status = Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms;
				Click_SendCancellation(ediMenu);
				AssertEquals(Core.SGConstants.DeclarationStatus.CancellationPending, Declaration.ActiveEntryHeaders[0].CH_Status);
			}
		}

		public void TestResetToWorking()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			using (EDIMenu ediMenu = GetEDIMenu())
			{
				AssertNull(ediMenu.MenuItems["Reset to Working"]);
			}

			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			using (EDIMenu ediMenu = GetEDIMenu())
			{
				AssertNotNull(ediMenu.MenuItems["Reset to Working"]);
				Declaration.JE_EntryStatus = "ABC";
				Declaration.ActiveEntryHeaders.AddNew();
				ediMenu.MenuItems["Reset to Working"].PerformClick();
				AssertEquals("", Declaration.JE_EntryStatus);
			}
		}

		public void TestCalculatePayables()
		{
			JobComInvoiceHeader invoiceheader = Declaration.Invoices.AddNew();
			invoiceheader.JobComInvoiceLines.AddNew();
			using (EDIMenu ediMenu = GetEDIMenu())
			{
				ediMenu.GenerateEntriesMenuItem.PerformClick();
				AssertEquals(1, Declaration.ActiveEntryHeaders.Count);
			}
		}

		public void TestMenuItemOrder()
		{
			using (EDIMenu ediMenu = GetEDIMenu())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Send Declaration", ediMenu.MenuItems[0].Text);
					AssertEquals("Send Amendment", ediMenu.MenuItems[1].Text);
					AssertEquals("Send Refund", ediMenu.MenuItems[2].Text);
					AssertEquals("Send Cancellation", ediMenu.MenuItems[3].Text);
					AssertEquals("Reset to Working", ediMenu.MenuItems[4].Text);
					AssertEquals("-", ediMenu.MenuItems[5].Text);
					AssertEquals("Commercial &Invoices", ediMenu.MenuItems[7].Text);
					AssertEquals("Auto Apportion &Weight", ediMenu.MenuItems[8].Text);
					AssertEquals("Allocate Remaining Weight", ediMenu.MenuItems[9].Text);
					AssertEquals("Inventory Management", ediMenu.MenuItems[10].Text);
					AssertEquals("&Copy Previous Invoice Line", ediMenu.MenuItems[11].Text);
					AssertEquals("Create Product Files", ediMenu.MenuItems[12].Text);
					AssertEquals("Refresh Product Data", ediMenu.MenuItems[13].Text);
					AssertEquals("Data", ediMenu.MenuItems[14].Text);
					AssertEquals("Calculate Payables (Merge)", ediMenu.MenuItems[15].Text);
					AssertEquals("Perform Apportionment", ediMenu.MenuItems[16].Text);
				});
			}
		}

		#region Click Declaration
		void Click_SendDeclaration(EDIMenu ediMenu)
		{
			Click_SendDeclaration(ediMenu, true, DialogResult.OK);
		}

		void Click_SendDeclaration(EDIMenu ediMenu, bool setBrokerDetails)
		{
			Click_SendDeclaration(ediMenu, setBrokerDetails, DialogResult.OK);
		}

		void Click_SendDeclaration(EDIMenu ediMenu, bool setBrokerDetails, DialogResult userNotificationResult)
		{
			if (setBrokerDetails)
			{
				DoSetBrokerDetails();
			}

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(userNotificationResult);
			ediMenu.MenuItems["Send Declaration"].PerformClick();
		}
		#endregion

		#region Click Amendment
		void Click_SendAmendment(EDIMenu ediMenu)
		{
			Click_SendAmendment(ediMenu, true, DialogResult.OK);
		}

		void Click_SendAmendment(EDIMenu ediMenu, bool setBrokerDetails, DialogResult userNotificationResult)
		{
			Click_SendAmendment(ediMenu, setBrokerDetails, DialogResult.OK, userNotificationResult);
		}

		void Click_SendAmendment(EDIMenu ediMenu, bool setBrokerDetails, DialogResult resultToReturnFromShowDialog, DialogResult userNotificationResult)
		{
			if (setBrokerDetails)
			{
				DoSetBrokerDetails();
			}

			ZFormModaliser.ResultToReturnFromShowDialog = resultToReturnFromShowDialog;
			UnitTestUserNotification.Instance.AddAnswer(userNotificationResult);
			ediMenu.MenuItems["Send Amendment"].PerformClick();
		}
		#endregion

		#region Click Refund
		void Click_SendRefund(EDIMenu ediMenu)
		{
			Click_SendRefund(ediMenu, true, DialogResult.OK);
		}

		void Click_SendRefund(EDIMenu ediMenu, bool setBrokerDetails, DialogResult userNotificationResult)
		{
			if (setBrokerDetails)
			{
				DoSetBrokerDetails();
			}

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(userNotificationResult);
			ediMenu.MenuItems["Send Refund"].PerformClick();
		}
		#endregion

		#region Click Cancellation
		void Click_SendCancellation(EDIMenu ediMenu)
		{
			Click_SendCancellation(ediMenu, true, DialogResult.OK);
		}

		void Click_SendCancellation(EDIMenu ediMenu, bool setBrokerDetails, DialogResult userNotificationResult)
		{
			if (setBrokerDetails)
			{
				DoSetBrokerDetails();
			}

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			UnitTestUserNotification.Instance.AddAnswer(userNotificationResult);
			ediMenu.MenuItems["Send Cancellation"].PerformClick();
		}
		#endregion

		void DoSetBrokerDetails()
		{
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "199702247W";
			var newFactory = new BusinessObjectFactory();
			var currentUser = newFactory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var wrapper = SGGlbStaffWrapper.Get(currentUser);
			wrapper.Tradenetv4Password.GP_CurrentPassword = "0KKwb6ydAlmfdLUP+jgqwUGsR6GLqfWCNrg9I0zCnfI=";
			wrapper.Tradenetv4Password.GP_MailBoxID = "TEST";
			wrapper.SGNationalTradePlatformPassword.GP_CurrentPassword = "0KKwb6ydAlmfdLUP+jgqwUGsR6GLqfWCNrg9I0zCnfI=";
			wrapper.SGNationalTradePlatformPassword.GP_MailBoxID = "NTPTEST";
			wrapper.SGNationalTradePlatformPassword.GP_UserID = "NTPUSER";
			currentUser.GS_WorkPhone = "TEST";
			currentUser.GS_RN_NKNationalityCode = "X";
			wrapper.Tradenetv4Password.GP_UserID = "TEST";
			currentUser.GS_Passport = "TEST";
			newFactory.Save();
		}

		void DoSetBrokerDetailsWithSG4Credential()
		{
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "199702247W";
			var wrapper = SGGlbStaffWrapper.Get(GlbStaff.CurrentUser);
			wrapper.Tradenetv4Password.GP_CurrentPassword = "0KKwb6ydAlmfdLUP+jgqwUGsR6GLqfWCNrg9I0zCnfI=";
			wrapper.Tradenetv4Password.GP_MailBoxID = "TEST";
			wrapper.SGNationalTradePlatformPassword.CurrentDecryptedPassword = string.Empty;
			wrapper.SGNationalTradePlatformPassword.GP_MailBoxID = ZString.Empty;
			wrapper.SGNationalTradePlatformPassword.GP_UserID = string.Empty;
			wrapper.Tradenetv4Password.GP_UserID = "TEST";
		}

		EDIMenu GetEDIMenu()
		{
			EDIMenu ediMenu = new EDIMenu();
			ediMenu.Declaration = Declaration;
			return ediMenu;
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.MessageInitiator = SendsMessagesToCustomsShutterUpperer;
					declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
					JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
					invoiceHeader.JobComInvoiceLines.AddNew();
				}

				return declaration;
			}
		}
		JobDeclaration declaration;

		SendsMessagesToCustomsShutterUpperer SendsMessagesToCustomsShutterUpperer
			=> sendsMessagesToCustomsShutterUpperer ?? (sendsMessagesToCustomsShutterUpperer = new SendsMessagesToCustomsShutterUpperer());

		SendsMessagesToCustomsShutterUpperer sendsMessagesToCustomsShutterUpperer;

		void AssertSendDeclaration_CanSendAnyMessage_MessageErrors(string expectedApplicationCode)
		{
			using (EDIMenu ediMenu = GetEDIMenu())
			{
				AssertEquals("PreCondition", 0, Declaration.ActiveEntryHeaders.Count);
				Click_SendDeclaration(ediMenu, true, DialogResult.Cancel);
				AssertEquals("Message Errors after form validation", 0, Declaration.ActiveEntryHeaders.Count);
				Click_SendDeclaration(ediMenu, true, DialogResult.OK);
				AssertEquals("Message Errors after form validation", 1, Declaration.ActiveEntryHeaders.Count);
				AssertEquals("One message has been sent", 1, Declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals($"{expectedApplicationCode} message has been sent", expectedApplicationCode, Declaration.ActiveEntryHeaders[0].Messages[0].EM_ApplicationCode);
			}
		}

		void AssertSendDeclaration_DeclarationNotYetMade()
		{
			using (EDIMenu ediMenu = GetEDIMenu())
			{
				AssertEquals("PreCondition", 0, Declaration.ActiveEntryHeaders.Count);
				Click_SendDeclaration(ediMenu);
				AssertEquals(1, Declaration.ActiveEntryHeaders.Count);
			}
		}
	}
}
