using System;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.Registry;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	[TestedType(typeof(CMDShipmentWrapper))]
	sealed class CMDShipmentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			ForwardingShipment shipment = new BusinessObjectFactory().New<ForwardingShipment>();
			CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
			AssertEquals("Should use the shipment's factory", shipment.Factory, shipmentWrapper.Factory);
			AssertEquals("Should be assigned in the constructor", shipment, shipmentWrapper.Shipment);
		}

		public void TestCMDShipments()
		{
			AssertEquals(1, ShipmentWrapper.CMDShipments.Length);
			AssertEquals("Should return itself", ShipmentWrapper, ShipmentWrapper.CMDShipments[0]);
		}

		#region TestGetShipmentInvoiceLines
		public void TestGetShipmentInvoiceLines()
		{
			AssertEquals("Pre-condition, no declaration and invoices attached to the shipment", 0, ShipmentWrapper.GetShipmentInvoiceLines().Length);
			BaseJobDeclaration declaration1 = CreateNewDeclaration();
			AssertEquals("Should still return an empty array as there is no invoice lines attached to the declarations", 0, ShipmentWrapper.GetShipmentInvoiceLines().Length);
			BaseJobComInvoiceHeader invoiceHeader1 = declaration1.Invoices.AddNew();
			AssertEquals("Should still return an empty array as there is no invoice lines attached to the Invoice header", 0, ShipmentWrapper.GetShipmentInvoiceLines().Length);
			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader1.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine[] invoiceLines = ShipmentWrapper.GetShipmentInvoiceLines();
			AssertEquals("Should return 2 lines from the declaration", 2, invoiceLines.Length);
			AssertEquals(invoiceLine1, invoiceLines[0]);
			AssertEquals(invoiceLine2, invoiceLines[1]);
			BaseJobDeclaration declaration2 = CreateNewDeclaration();
			BaseJobComInvoiceHeader invoiceHeader2 = declaration2.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine3 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLines = ShipmentWrapper.GetShipmentInvoiceLines();
			AssertEquals("Should return 2 lines from the 1st declaration and 1 line from the 2nd declaration", 3, invoiceLines.Length);
			AssertEquals(invoiceLine1, invoiceLines[0]);
			AssertEquals(invoiceLine2, invoiceLines[1]);
			AssertEquals(invoiceLine3, invoiceLines[2]);
		}

		BaseJobDeclaration CreateNewDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GB = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew().PK;
			declaration.JE_JS = Shipment.PK;
			return declaration;
		}

		#endregion
		public void TestDefaultValues()
		{
			Assert("Should be set in the SetDefaultValues", ShipmentWrapper.ShowActiveMessagesOnly);
		}

		public void TestMessageFilter()
		{
			ShipmentWrapper.ShowActiveMessagesOnly = true;
			Assert("Filter should be ActiveOnly", ShipmentWrapper.ShowActiveMessagesOnly);
			Assert("Filter should be ActiveOnly", !ShipmentWrapper.ShowAll);
			ShipmentWrapper.ShowAll = true;
			Assert("Filter should be All", ShipmentWrapper.ShowAll);
			Assert("Filter should be All", !ShipmentWrapper.ShowActiveMessagesOnly);
		}

		public void TestMessages()
		{
			InsertTestMessages();
			Assert("Default is Active Only Messages", ShipmentWrapper.ShowActiveMessagesOnly);
			AssertEquals(2, ShipmentWrapper.Messages.Count);
			ShipmentWrapper.ShowAll = true;
			Assert(!ShipmentWrapper.ShowActiveMessagesOnly);
			AssertEquals(5, ShipmentWrapper.Messages.Count);
		}

		public void TestRefreshMessageList()
		{
			AssertEquals("Pre-condition", 0, ShipmentWrapper.Messages.Count);
			Assert("Should be loaded in the lazy getter", ShipmentWrapper.Messages.IsLoaded);
			CMDEDIMessage newMessage = Factory.New<CMDEDIMessage>();
			newMessage.EM_MessageText = "Message 2 Test";
			newMessage.EM_LinkedObject = Shipment;
			newMessage.EM_IsActive = true;
			AssertEquals("Hasn't been reloaded, should still return 0", 0, ShipmentWrapper.Messages.Count);
			ShipmentWrapper.RefreshMessageList();
			AssertEquals(1, ShipmentWrapper.Messages.Count);
		}

		public void TestCurrentCMDStatus()
		{
			const string messageError = "The current CMD message has an error";
			CombineAssertions(() =>
			{
				AssertEquals("No Messages Sent Status", "No messages have been sent", ShipmentWrapper.CurrentCMDStatus);
				AssertNoMessageErrors("Not Sent", ShipmentWrapper.CurrentCMDStatusInfo);
				InsertMessageWithReply(CMDGenerator.Constants.GHA, "FNA\r\nACK/asdkjafd\r\nCMD\r\nBlabla\r\n");
				ShipmentWrapper.RefreshMessageList();
				AssertEquals("GHA FNA CMD Status", messageError, ShipmentWrapper.CurrentCMDStatus);
				AssertHasMessageError("GHA FNAMessage Error", ShipmentWrapper.CurrentCMDStatusInfo, messageError);
				InsertMessageWithReply(CMDGenerator.Constants.TDB, "CMA\r\nBlabla\r\n");
				ShipmentWrapper.RefreshMessageList();
				AssertEquals("TDB CMA CMD Status", "The current CMD message has an error", ShipmentWrapper.CurrentCMDStatus);
				AssertHasMessageError("TDB CMA Message Error", ShipmentWrapper.CurrentCMDStatusInfo, messageError);
				InsertMessageWithReply(CMDGenerator.Constants.GHA, "CMA\r\nBlabla\r\n");
				ShipmentWrapper.RefreshMessageList();
				AssertEquals("GHA CMA CMD Status", "The current CMD has been succesfully sent and acknowledged", ShipmentWrapper.CurrentCMDStatus);
				AssertNoMessageErrors("Acknowledged", ShipmentWrapper.CurrentCMDStatusInfo);
				InsertMessageWithReply(CMDGenerator.Constants.GHA, null);
				ShipmentWrapper.RefreshMessageList();
				AssertEquals("GHA Null CMD Status", "No replies have been received for the current CMD", ShipmentWrapper.CurrentCMDStatus);
				AssertNoMessageErrors("No Replies", ShipmentWrapper.CurrentCMDStatusInfo);
				InsertMessageWithReply(CMDGenerator.Constants.GHA, "asdflkj");
				ShipmentWrapper.RefreshMessageList();
				AssertEquals("Invalid Reply Status", "No replies have been received for the current CMD", ShipmentWrapper.CurrentCMDStatus);
				AssertNoMessageErrors("Invalid Reply", ShipmentWrapper.CurrentCMDStatusInfo);
			}

			);
		}

		public void TestGetLastCMDSent()
		{
			ShipmentWrapper.ShowAll = true;
			CMDEDIMessage message1 = InsertMessageWithReply(CMDGenerator.Constants.GHA, "FNA\r\nACK/asdkjafd\r\nCMD\r\nBlabla\r\n");
			CMDEDIMessage message2 = InsertMessageWithReply(CMDGenerator.Constants.TDB, "CMA\r\nBlabla\r\n");
			Factory.Save();
			ShipmentWrapper.RefreshMessageList();
			CMDEDIMessage message = ShipmentWrapper.GetLastCMDSent();
			AssertEquals(message1.PK, message.PK);
			CMDEDIMessage message3 = InsertMessageWithReply(CMDGenerator.Constants.GHA, "CMA\r\nBlabla\r\n");
			Factory.Save();
			ShipmentWrapper.RefreshMessageList();
			message = ShipmentWrapper.GetLastCMDSent();
			AssertEquals(message3.PK, message.PK);
			Thread.Sleep(1); // Ensure that Message3 and Message4 do not have the same time
			CMDEDIMessage message4 = InsertMessageWithReply(CMDGenerator.Constants.GHA, "FNA\r\nBlabla\r\n");
			Factory.Save();
			ShipmentWrapper.RefreshMessageList();
			message = ShipmentWrapper.GetLastCMDSent();
			AssertEquals(message4.PK, message.PK);
			CMDEDIMessage message5 = InsertMessageWithReply(CMDGenerator.Constants.GHA, "CMA\r\nBlabla\r\n", false);
			Factory.Save();
			ShipmentWrapper.RefreshMessageList();
			message = ShipmentWrapper.GetLastCMDSent();
			AssertEquals(message4.PK, message.PK);
		}

		public void TestPermitAndExemptionDetails()
		{
			AddNewCMDDataValue("YYY", "Y1");
			AssertEquals("YYY: Y1", ShipmentWrapper.PermitAndExemptionDetails);
			ShipmentWrapper.CMDDataValues.RemoveAll();
			AddNewCMDDataValue("XXX", "X1");
			AddNewCMDDataValue("XX2", "X2");
			AddNewCMDDataValue("XXX", "X3");
			AssertEquals("Has to be reconstructed", "YYY: Y1", ShipmentWrapper.PermitAndExemptionDetails);
			ShipmentWrapper.ResetPermitAndExemptionDetails();
			AssertEquals("XXX: X1 | XX2: X2 | XXX: X3", ShipmentWrapper.PermitAndExemptionDetails);
		}

		public void TestGetTDBPermits()
		{
			AddNewCusCodeData(CustomsEntryTypeList.Singapore.Permit, "PMT101");
			AddNewCusCodeData(CustomsEntryTypeList.Singapore.Permit, "PMT102");
			AddNewCusCodeData(CustomsEntryTypeList.Singapore.Permit, "PMT103");
			AddNewCusCodeData(CustomsEntryTypeList.Singapore.Certificate, "CER101");
			AddNewCusCodeData(CustomsEntryTypeList.Singapore.SGExemption.Codes.AT, "AT101");
			CusCodeData[] tDBPermits = ShipmentWrapper.GetTDBPermits();
			AssertEquals(3, tDBPermits.Length);
			AssertCusCodeDataExist(tDBPermits, CustomsEntryTypeList.Singapore.Permit, "PMT101");
			AssertCusCodeDataExist(tDBPermits, CustomsEntryTypeList.Singapore.Permit, "PMT102");
			AssertCusCodeDataExist(tDBPermits, CustomsEntryTypeList.Singapore.Permit, "PMT103");
		}

		public void TestGetTDBExemption()
		{
			AddNewCMDDataValue(CustomsEntryTypeList.Singapore.Permit, "PMT101");
			AddNewCMDDataValue(CustomsEntryTypeList.Singapore.Permit, "PMT102");
			AddNewCMDDataValue(CustomsEntryTypeList.Singapore.Permit, "PMT103");
			AddNewCMDDataValue(CustomsEntryTypeList.Singapore.Certificate, "CER101");
			AssertNull("No exemption in the collection", ShipmentWrapper.GetTDBExemption());
			AddNewCMDDataValue(CustomsEntryTypeList.Singapore.SGExemption.Codes.AT, "AT101");
			//AssertEquals("AT101", ShipmentWrapper.GetTDBExemption().CE_EntryNum);
		}

		public void TestHasTDBPermitFromDeclarations()
		{
			AssertEquals(false, ShipmentWrapper.HasTDBPermitFromDeclarations());
			AddNewCMDDataValue(CustomsEntryTypeList.Singapore.Permit, "PMT101");
			AddNewCMDDataValue(CustomsEntryTypeList.Singapore.Permit, "PMT101");
			AddNewCMDDataValue(CustomsEntryTypeList.Singapore.Permit, "PMT101");
			AssertEquals(false, ShipmentWrapper.HasTDBPermitFromDeclarations());
			var declarationOtherCountry = Factory.New<JobDeclaration>();
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "XX";
			var branch = company.Branches.AddNew();
			declarationOtherCountry.JE_GB = branch.PK;
			declarationOtherCountry.JE_JS = ShipmentWrapper.Shipment.PK;
			AssertNotNull("Pre-condition", declarationOtherCountry.Branch);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "TEST";
			declaration.JE_JS = ShipmentWrapper.Shipment.PK;
			AssertNotNull("Pre-condition", declaration.Branch);
			AssertEquals(true, ShipmentWrapper.HasTDBPermitFromDeclarations());
		}

		public void TestCusEntryNumbersForBinding()
		{
			AssertEquals(typeof(CMDDataValueWrapperCollection), ShipmentWrapper.CMDDataValuesForBinding.GetType());
			Assert("Should be loaded in the getter", ShipmentWrapper.CMDDataValuesForBinding.IsLoaded);
		}

		#region Send / Delete
		public void TestSendMessage()
		{
			CMDNotificationBuffer notificationBuffer = new CMDNotificationBuffer(Shipment);
			SetupValidShipmentForSending();
			AssertEquals("Pre-condition", 0, ShipmentWrapper.Messages.Count);
			ShipmentWrapper.SendMessage(notificationBuffer);
			ZString expectedInfoMessages = Shipment.HumanReadableName + "\r\n1 CMD Message(s) sent";
			AssertEquals(expectedInfoMessages, notificationBuffer.GetInfoMessages());
			AssertEquals("Should be refreshed", 1, ShipmentWrapper.Messages.Count);
			notificationBuffer.Clear();
			ShipmentWrapper.SendMessage(notificationBuffer);
			expectedInfoMessages = Shipment.HumanReadableName + "\r\nNo CMD Messages sent, CMD data unchanged since last submission";
			AssertEquals(expectedInfoMessages, notificationBuffer.GetInfoMessages());
			AssertEquals("Should not create a new one", 1, ShipmentWrapper.Messages.Count);
		}

		public void TestSendMessageWithRecipient()
		{
			using (SGCustomsDataRegistry.Instance.SendViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CMDNotificationBuffer notificationBuffer = new CMDNotificationBuffer(Shipment);
				SetupValidShipmentForSending();
				AssertEquals("Pre-condition", 0, ShipmentWrapper.Messages.Count);
				ShipmentWrapper.SendMessage("SATS", notificationBuffer);
				ZString expectedInfoMessages = Shipment.HumanReadableName + "\r\n1 CMD Message(s) sent";
				AssertEquals(expectedInfoMessages, notificationBuffer.GetInfoMessages());
				AssertEquals("Should be refreshed", 1, ShipmentWrapper.Messages.Count);
				EDIInterchange interchange = Factory.Load<EDIInterchange>(ShipmentWrapper.Messages[0].EM_EI);
				Assert("Interchange header should contain the recipient string", interchange.EI_HeaderText.EndsWith("SATS\r\n"));
			}
		}

		public void TestDeleteExistingCMDMessages()
		{
			CMDNotificationBuffer notificationBuffer = new CMDNotificationBuffer(Shipment);
			SetupValidShipmentForSending();
			ShipmentWrapper.SendMessage(notificationBuffer);
			AssertEquals(1, ShipmentWrapper.Messages.Count);
			notificationBuffer.Clear();
			ShipmentWrapper.DeleteExistingCMDMessages(notificationBuffer);
			ZString expectedInfoMessages = Shipment.HumanReadableName + "\r\n1 CMD Message(s) sent";
			AssertEquals(expectedInfoMessages, notificationBuffer.GetInfoMessages());
			AssertEquals(1, ShipmentWrapper.Messages.Count);
			AssertEquals("Should be a delete message", CMD.ActionCodes.Delete, new CMDParser(ShipmentWrapper.Messages[0].EM_MessageText).ActionCode);
		}

		void SetupValidShipmentForSending()
		{
			//AddNewCusEntryNumber(CustomsEntryTypeList.Singapore.Permit, "PMT101");
			AddNewCusCodeData(CustomsEntryTypeList.Singapore.Permit, "PMT101");
			ForwardingConsol consol = Shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.Transports.MostInterestingTransport.JW_ATA = ZDateTime.Now.AddDays(2);
			PackLine outerPack = Shipment.OuterPackLines.AddNew();
			outerPack.JL_PackageCount = 2;
			outerPack.JL_Description = "AASDF";
		}

		#endregion
		#region Validation
		public void TestRunPreSendValidation_NewShipment()
		{
			CMDNotificationBuffer notificationBuffer = new CMDNotificationBuffer(Shipment);
			ShipmentWrapper.RunPreSendValidation(notificationBuffer);
			ZString expectedErrorMessages = Shipment.HumanReadableName + @"
There is no Exemption Code or Permit Numbers entered to send
This shipment is not attached to a Consolidation. MAWB details are required for CMD
Total Outer Packs has to be greater than zero";
			AssertErrorMessage(notificationBuffer, expectedErrorMessages);
		}

		public void TestRunPreSendValidation_OuterPackValidation()
		{
			CMDNotificationBuffer notificationBuffer = new CMDNotificationBuffer(Shipment);
			PackLine outerPack = Shipment.OuterPackLines.AddNew();
			ShipmentWrapper.RunPreSendValidation(notificationBuffer);
			ZString expectedErrorMessages = Shipment.HumanReadableName + @"
There is no Exemption Code or Permit Numbers entered to send
This shipment is not attached to a Consolidation. MAWB details are required for CMD
Total Outer Packs has to be greater than zero

Outer Package
Both Goods Description and Harmonised Code are required for CMD";
			AssertErrorMessage(notificationBuffer, expectedErrorMessages);
			outerPack.JL_PackageCount = 2;
			outerPack.JL_Description = "AASDF";
			notificationBuffer.Clear();
			ShipmentWrapper.RunPreSendValidation(notificationBuffer);
			expectedErrorMessages = Shipment.HumanReadableName + @"
There is no Exemption Code or Permit Numbers entered to send
This shipment is not attached to a Consolidation. MAWB details are required for CMD

Outer Package
Both Goods Description and Harmonised Code are required for CMD";
			AssertErrorMessage(notificationBuffer, expectedErrorMessages);
			outerPack.JL_HarmonisedCode = "12345678";
			notificationBuffer.Clear();
			ShipmentWrapper.RunPreSendValidation(notificationBuffer);
			expectedErrorMessages = Shipment.HumanReadableName + @"
There is no Exemption Code or Permit Numbers entered to send
This shipment is not attached to a Consolidation. MAWB details are required for CMD";
			AssertErrorMessage(notificationBuffer, expectedErrorMessages);
		}

		public void TestRunPreSendValidation_ConsolValidation()
		{
			CMDNotificationBuffer notificationBuffer = new CMDNotificationBuffer(Shipment);
			ForwardingConsol consol = Shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "INBOM";
			ShipmentWrapper.RunPreSendValidation(notificationBuffer);
			ZString expectedErrorMessages = Shipment.HumanReadableName + @"
There is no Exemption Code or Permit Numbers entered to send
This shipment is not attached to a Consolidation. MAWB details are required for CMD
Total Outer Packs has to be greater than zero";
			AssertErrorMessage("Consol has to be an air consol and either import/export", notificationBuffer, expectedErrorMessages);
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			notificationBuffer.Clear();
			ShipmentWrapper.RunPreSendValidation(notificationBuffer);
			expectedErrorMessages = Shipment.HumanReadableName + @"
There is no Exemption Code or Permit Numbers entered to send
This shipment is not attached to a Consolidation. MAWB details are required for CMD
Total Outer Packs has to be greater than zero";
			AssertErrorMessage("Consol has to be either import/export", notificationBuffer, expectedErrorMessages);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			notificationBuffer.Clear();
			ShipmentWrapper.RunPreSendValidation(notificationBuffer);
			expectedErrorMessages = Shipment.HumanReadableName + @"
There is no Exemption Code or Permit Numbers entered to send
Total Outer Packs has to be greater than zero";
			AssertErrorMessage(notificationBuffer, expectedErrorMessages);
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			notificationBuffer.Clear();
			ShipmentWrapper.RunPreSendValidation(notificationBuffer);
			expectedErrorMessages = Shipment.HumanReadableName + @"
There is no Exemption Code or Permit Numbers entered to send
Total Outer Packs has to be greater than zero";
			AssertErrorMessage(notificationBuffer, expectedErrorMessages);
		}

		public void TestRunPreSendValidation_PermitAndExemptionValidation()
		{
			CMDNotificationBuffer notificationBuffer = new CMDNotificationBuffer(Shipment);
			AddNewCMDDataValue(CustomsEntryTypeList.Singapore.Permit, "PMT101");
			ShipmentWrapper.RunPreSendValidation(notificationBuffer);
			ZString expectedErrorMessages = Shipment.HumanReadableName + @"
This shipment is not attached to a Consolidation. MAWB details are required for CMD
Total Outer Packs has to be greater than zero";
			AssertErrorMessage(notificationBuffer, expectedErrorMessages);
			Shipment.CusEntryNumbers.RemoveAndDeleteAll();
			notificationBuffer.Clear();
			AddNewCMDDataValue(CustomsEntryTypeList.Singapore.SGExemption.Codes.DP, "an exemption code");
			ShipmentWrapper.RunPreSendValidation(notificationBuffer);
			expectedErrorMessages = Shipment.HumanReadableName + @"
This shipment is not attached to a Consolidation. MAWB details are required for CMD
Total Outer Packs has to be greater than zero";
			AssertErrorMessage(notificationBuffer, expectedErrorMessages);
		}

		public void TestRunPreSendValidation_AllValid()
		{
			CMDNotificationBuffer notificationBuffer = new CMDNotificationBuffer(Shipment);
			AddNewCMDDataValue(CustomsEntryTypeList.Singapore.Permit, "PMT101");
			ForwardingConsol consol = Shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			PackLine outerPack = Shipment.OuterPackLines.AddNew();
			outerPack.JL_PackageCount = 2;
			outerPack.JL_Description = "AASDF";
			AssertErrorMessage(notificationBuffer, "");
		}

		public void TestRunPreDeleteValidation()
		{
			CMDNotificationBuffer notificationBuffer = new CMDNotificationBuffer(Shipment);
			ShipmentWrapper.RunPreDeleteValidation(notificationBuffer);
			AssertErrorMessage(notificationBuffer, Shipment.HumanReadableName + "\r\nThere is no previously sent CMD Messages to be deleted");
			notificationBuffer.Clear();
			ShipmentWrapper.Messages.AddNew();
			ShipmentWrapper.RunPreDeleteValidation(notificationBuffer);
			AssertErrorMessage(notificationBuffer, "");
		}

		void AssertErrorMessage(CMDNotificationBuffer notificationBuffer, ZString expectedErrorMessages)
		{
			AssertEquals("", expectedErrorMessages, notificationBuffer.GetErrorMessages());
		}

		void AssertErrorMessage(string failureMessage, CMDNotificationBuffer notificationBuffer, ZString expectedErrorMessages)
		{
			AssertEquals(failureMessage, expectedErrorMessages, notificationBuffer.GetErrorMessages());
		}

		#endregion
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			return new CMDShipmentWrapper(shipment);
		}

		void InsertTestMessages()
		{
			CMDEDIMessage message1 = Factory.New<CMDEDIMessage>();
			message1.EM_MessageText = "Message 1 Test";
			message1.EM_LinkedObject = Shipment;
			message1.EM_IsActive = true;
			CMDEDIMessage message2 = Factory.New<CMDEDIMessage>();
			message2.EM_MessageText = "Message 2 Test";
			message2.EM_LinkedObject = Shipment;
			message2.EM_IsActive = false;
			CMDEDIMessage message3 = Factory.New<CMDEDIMessage>();
			message3.EM_MessageText = "Message 3 Test";
			message3.EM_LinkedObject = Shipment;
			message3.EM_IsActive = true;
			CMDEDIMessage message4 = Factory.New<CMDEDIMessage>();
			message4.EM_MessageText = "Message 4 Test";
			message4.EM_LinkedObject = Shipment;
			message4.EM_IsActive = false;
			CMDEDIMessage message5 = Factory.New<CMDEDIMessage>();
			message5.EM_MessageText = "Message 5 Test";
			message5.EM_LinkedObject = Shipment;
			message5.EM_IsActive = false;
		}

		CMDEDIMessage InsertMessageWithReply(ZString subType, ZString replyText)
		{
			return InsertMessageWithReply(subType, replyText, true);
		}

		CMDEDIMessage InsertMessageWithReply(ZString subType, ZString replyText, bool isActive)
		{
			CMDEDIMessage message1 = Factory.New<CMDEDIMessage>();
			message1.EM_MessageSubType = subType;
			message1.EM_LinkedObject = Shipment;
			message1.EM_IsActive = isActive;
			if (!replyText.IsEmpty)
			{
				message1.Reply = new CMDInbound(replyText);
			}

			return message1;
		}

		void AddNewCMDDataValue(ZString code, ZString data)
		{
			var cmdData = Factory.NewWithValidTestData<CMDPermitNumber>();
			cmdData.CY_ParentID = Shipment.PK;
			cmdData.CY_ParentTableCode = "JS";
			cmdData.CY_Code = code;
			cmdData.CY_Data = data;
		}

		void AddNewCusCodeData(ZString code, ZString data)
		{
			var ccData = Factory.NewWithValidTestData<CMDPermitNumber>();
			ccData.CY_ParentID = Shipment.PK;
			ccData.CY_ParentTableCode = "JS";
			ccData.CY_Code = code;
			ccData.CY_Data = data;
		}

		void AssertCusCodeDataExist(CusCodeData[] cusCodeData, ZString code, ZString data)
		{
			bool found = false;
			foreach (CusCodeData ccData in cusCodeData)
			{
				if (ccData.CY_Code == code && ccData.CY_Data == data)
				{
					found = true;
					break;
				}
			}

			Assert(string.Format("CusCodeData with code '{0}' and value '{1}' not found", code, data), found);
		}

		ForwardingShipment Shipment
		{
			get
			{
				return ShipmentWrapper.Shipment;
			}
		}

		CMDShipmentWrapper ShipmentWrapper
		{
			get
			{
				return (CMDShipmentWrapper)CachedBusinessObject;
			}
		}
		#endregion
	}
}
