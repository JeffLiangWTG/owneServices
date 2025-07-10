using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFHeader))]
	public class CusISFHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIDISHost()
		{
			var contents = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(Db.DatabaseName + "_SD001.dbo." + StorageDocsSchema.Constants.TableName);
			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			var header = documentFactory.New<CusISFHeader>();
			var docManagerInfo = header.DocManagerInfo();
			AssertEquals(0, ((IDISHost)header).EDocs.Count());
			docManagerInfo.Save();
			var filter = new ZQuery(StorageMainSchema.SM_ParentFK, header.PK);
			AssertNull(documentFactory.LoadTop1<IStorageMain>(filter));

			header.DocManagerInfo().AddFileOrDocument(contents, "document.pdf", "ABC");
			AssertEquals(1, ((IDISHost)header).EDocs.Count());
		}

		public void TestCodeAndDescriptionProperty()
		{
			var header = Factory.New<CusISFHeader>();
			Factory.Save();
			var code = "";
			var description = "";
			AssertNoExceptionThrown(() =>
			{
				code = CodePropertyAttribute.CodePropertyNameFromType(typeof(CusISFHeader));
				description = DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeof(CusISFHeader));
			});
			AssertEquals(CusISFHeader.Schema.BF_JobReference, code);
			AssertEquals("HumanReadableName", description);
			AssertEquals(header.BF_JobReference, CodePropertyAttribute.CodeFromBusinessObject(header));
			AssertEquals(header.HumanReadableName, DescriptionPropertyAttribute.DescriptionFromBusinessObject(header));
		}

		public void TestIBaseAutoSendingMessageSupporterProperties()
		{
			var header = Factory.New<CusISFHeader>();
			var supporter = header as IBaseAutoSendingMessageSupporter;
			AssertEquals(header.Branch.PK, supporter.RegistryBranchPK);
			AssertEquals("Enterprise.Customs.Business.BatchProcessor.CustomsStmProcessQueueCreatorProcessor", supporter.CreateStmProcessQueueProcessor(null, WorkflowTriggerActionTypeConstants.Codes.SendISFMessage).GetType().FullName);
		}

		public void TestICusISFAutoSendingMessageSupporterProperties()
		{
			var header = Factory.New<CusISFHeader>();
			var iHeader = header as Integration.Customs.US.ISF.ICusISFAutoSendingMessageSupporter;
			AssertEquals("Enterprise.Customs.US.ISF.Business.ISFMessageWorkflowTriggerProcessor", iHeader.CreateISFMessageWorkflowTriggerProcessor().GetType().FullName);
		}

		public void TestGetNewCusISFHeaderProcessTaskCollection()
		{
			var header = Factory.New<CusISFHeader>();
			AssertEquals("WorkflowItems's type should inherit from ProcessTaskCollection<CusISFHeaderProcessTask, CusISFHeader>", typeof(ProcessTaskCollection<CusISFHeaderProcessTask, CusISFHeader>), ((IWorkflowProvider)header).WorkflowItems.GetType().BaseType);
		}

		public void TestCalculateStatusforISF()
		{
			var header = Factory.New<CusISFHeader>();
			Factory.Save();
			var mock = Factory.NewMoq<MQEDIMessage>();
			var refBIll = header.ReferenceDatas.AddNew();
			refBIll.BB_BillNum = "123321";
			refBIll.BB_CustomsStatus = "12";
			refBIll.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			var refBIll2 = header.ReferenceDatas.AddNew();
			refBIll2.BB_BillNum = "123325";
			refBIll2.BB_CustomsStatus = "15";
			refBIll2.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			Factory.Save();
			MQEDIMessage outgoingMessage = mock.Object;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFDelete;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "~15000";
			outgoingMessage.EM_MessageText = "B013901SV9SF                                                                     Y         SF00001000000000000000000000000";
			outgoingMessage.EM_LinkedObject = header;
			MQEDIMessage changeMessage = Factory.New<MQEDIMessage>();
			changeMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			changeMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;
			changeMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			changeMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFDelete;
			changeMessage.EM_MessageNum = "~15000";
			changeMessage.EM_MessageText = "B013901SV9SN                                               HYEDUSCMT_150284     " +
				"SF10101DCTEI 13-147927000           11SV9-87609069582    13-147927000   018     " +
				"SF9002   ISF DELETED                                                            " +
				"Y  3901SV9SN00002";
			Factory.Save();
			new ISFIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();
			header.Reload();
			AssertEquals("Message Should be CLEAR ISF DELETE", MessageStatusList.Codes.ClearISFDelete, header.BF_CustomsStatus);
			header.HouseBill.Reload();
			AssertEquals("HouseBill Should be XX", DispositionCodeList.Codes.XX, header.HouseBill.BB_CustomsStatus);
			header.OceanBill.Reload();
			AssertEquals("Oceanbill Should be XX", DispositionCodeList.Codes.XX, header.OceanBill.BB_CustomsStatus);
			mock.VerifyAll();
		}

		public void TestHumanReadableShortcutNameCusISFHeader()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = "IMPORTERFULLNAME";
			Factory.Save();
			var header = Factory.New<CusISFHeader>();
			header.BF_JobReference = "BSSSSSS";
			header.BF_OH_Importer = organisation.PK;
			AssertEquals("ISF Job# ImportFullName", string.Format("ISF - {0} - {1}", header.BF_JobReference, header.Importer.OH_FullName), header.HumanReadableShortcutName);
		}

		public void TestWorkflowInformationProvider()
		{
			var header = Factory.New<CusISFHeader>();
			ProcessTask trigger = header.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification taskNotification = trigger.ProcessTaskNotifications.AddNew();
			taskNotification.PQ_EmailText = "A mail from:(*Origin*) to (*Destination*), click (*WebTrackerUrl*) to see details.";
			trigger.P9_Description = "UNDESCRIBABLEBAL";
			WorkflowTriggerNotification notification = new WorkflowTriggerNotification(Lazy.Create(() => new MessageProcessorCommunicationModesResult(new[] { CommunicationsMode }, null)), taskNotification, header, null);
			ErrorReporter.Clear();
			((CargoWise.EntityFramework.IProcessor)notification).Process(null);
			Factory.Save();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestConcurrencyUpdateProcessTasksAndMessagingSendError()
		{
			var header = Factory.New<CusISFHeader>();
			var trigger = header.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.MessageStatusChange.Code;
			trigger.P9_Description = "test concurrency";
			Factory.Save();

			MQEDIMessage message = null;
			var builder = new ImporterSecurityFilingMessageBuilder<ABIInputBlockControlGenerator, APLB, APLY>(header, UpdateActionCode.Add);
			message = builder.PopulateMessage();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var headerCopy = factory2.Load<CusISFHeader>(header.PK);
			var triggerCopy = (ProcessTask)headerCopy.WorkflowItems.Triggers.FindByPK(trigger.PK);
			triggerCopy.TriggerConditions.TriggerCondition = "98";
			factory2.Save();
			try
			{
				trigger.TriggerConditions.TriggerCondition = "99";
				Factory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(e);
			}
			AssertEquals(0, header.Messages.Count);
		}

		public void TestReplacingIllegalCharacters()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_ImporterCodeType = CodeTypeList.Codes.SocialSecurity;
			header.BF_ImporterFullName = "IMPORTER*NAME";
			header.BF_ConsigneeCodeType = CodeTypeList.Codes.SocialSecurity;
			header.BF_ConsigneeFullName = "CONSIGNEE*NAME";
			IImporterSecurityFiling filling = header;
			AssertEquals("IMPORTER NAME", filling.ImporterFullName);
			AssertEquals("CONSIGNEE NAME", filling.ConsigneeFullName);
		}

		public void TestUpdateImporterPassportIssueCountryIfNeeded()
		{
			var passport = Factory.New<OrgCusCode>();
			passport.OK_CodeType = ImporterCodeTypeList.Codes.Passport;
			passport.OK_CustomsRegNo = "111";
			passport.OK_RN_NKCodeCountry = "XX";
			var header = Factory.New<CusISFHeader>();
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.Passport;
			header.BF_ImporterCode = "111";
			AssertEquals("defaut country", "XX", header.BF_CountryOfIssue);
			header.BF_ConsigneeCodeType = ImporterCodeTypeList.Codes.Passport;
			header.BF_ConsigneeCode = "111";
			AssertEquals("defaut country", "XX", header.BF_ConsigneeCountryOfIssue);
		}

		public void TestDiscardedCustomsReference()
		{
			var header = Factory.New<CusISFHeader>();
			var message1 = Factory.New<EDIMessage>();
			message1.EM_ApplicationReference = "REF2";
			message1.EM_Status = EDIMessage.Status.Discarded;
			header.Messages.Add(message1);
			var message2 = Factory.New<EDIMessage>();
			message2.EM_ApplicationReference = "REF1";
			message2.EM_Status = EDIMessage.Status.Discarded;
			header.Messages.Add(message2);
			var message3 = Factory.New<EDIMessage>();
			message3.EM_ApplicationReference = ZString.Empty;
			message3.EM_Status = EDIMessage.Status.Discarded;
			header.Messages.Add(message3);
			var message4 = Factory.New<EDIMessage>();
			message4.EM_ApplicationReference = "REF1";
			message4.EM_Status = EDIMessage.Status.Discarded;
			header.Messages.Add(message4);
			var message5 = Factory.New<EDIMessage>();
			message5.EM_ApplicationReference = "REF3";
			message5.EM_Status = EDIMessage.Status.Received;
			header.Messages.Add(message5);
			var message6 = Factory.New<EDIMessage>();
			message6.EM_ApplicationReference = "REF4";
			message6.EM_Status = EDIMessage.Status.Discarded;
			header.Messages.Add(message6);
			AssertEquals("REF1, REF2, REF4", header.DiscardedCustomsReference);
		}

		public void TestCustomsReferenceReadOnly()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_CustomsStatus = MessageStatusList.Codes.NotSentISF;
			AssertEquals("If the Customs Status is 'Not Sent' the Customs Reference field should not be read only", false, header.BF_CustomsReferenceInfo.ReadOnly);
			header.BF_CustomsStatus = MessageStatusList.Codes.AwaitingISFAdd;
			AssertEquals("If the Customs Status is 'Awaiting ISF Add' the Customs Reference field should be read only", true, header.BF_CustomsReferenceInfo.ReadOnly);
			header.BF_CustomsStatus = MessageStatusList.Codes.AwaitingISFDelete;
			AssertEquals("If the Customs Status is 'Awaiting ISF Delete' the Customs Reference field should be read only", true, header.BF_CustomsReferenceInfo.ReadOnly);
			header.BF_CustomsStatus = MessageStatusList.Codes.AwaitingISFReplace;
			AssertEquals("If the Customs Status is 'Awaiting ISF Replace' the Customs Reference field should be read only", true, header.BF_CustomsReferenceInfo.ReadOnly);
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			AssertEquals("If the Customs Status is 'Clear ISF Add' the Customs Reference field should be read only", true, header.BF_CustomsReferenceInfo.ReadOnly);
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFDelete;
			AssertEquals("If the Customs Status is 'Clear ISF Delete' the Customs Reference field should be read only", true, header.BF_CustomsReferenceInfo.ReadOnly);
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFReplace;
			AssertEquals("If the Customs Status is 'Clear ISF Replace' the Customs Reference field should be read only", true, header.BF_CustomsReferenceInfo.ReadOnly);
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearWithWarningISFAdd;
			AssertEquals("If the Customs Status is 'Clear With Warning ISF Add' the Customs Reference field should be read only", true, header.BF_CustomsReferenceInfo.ReadOnly);
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearWithWarningISFDelete;
			AssertEquals("If the Customs Status is 'Clear With Warning ISF Delete' the Customs Reference field should be read only", true, header.BF_CustomsReferenceInfo.ReadOnly);
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearWithWarningISFReplace;
			AssertEquals("If the Customs Status is 'Clear With Warning ISF Replace' the Customs Reference field should be read only", true, header.BF_CustomsReferenceInfo.ReadOnly);
			header.BF_CustomsStatus = MessageStatusList.Codes.ErrorISFAdd;
			var message1 = header.Messages.AddNew(typeof(MQEDIMessage));
			message1.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			ISFSF90 sf90Block1 = new ISFSF90();
			sf90Block1.MessageTypeCode = ISFMessageStatus.Codes.Accepted;
			message1.MessageBlock.AddMessageBlock(sf90Block1);
			AssertEquals("If the Message Type is a response from Customs and the Message Type Code is 'Accepted', the Customs Reference field should be read only", true, header.BF_CustomsReferenceInfo.ReadOnly);
			header.Messages.RemoveAndDeleteAll();
			var message2 = header.Messages.AddNew(typeof(MQEDIMessage));
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			ISFSF90 sf90Block2 = new ISFSF90();
			sf90Block2.MessageTypeCode = ISFMessageStatus.Codes.AcceptedWithWarning;
			message2.MessageBlock.AddMessageBlock(sf90Block2);
			AssertEquals("If the Message Type is a response from Customs and the Message Type Code is 'Accepted With Warning', the Customs Reference field should be read only", true, header.BF_CustomsReferenceInfo.ReadOnly);
			header.Messages.RemoveAndDeleteAll();
			var message3 = header.Messages.AddNew(typeof(MQEDIMessage));
			message3.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			ISFSF90 sf90Block3 = new ISFSF90();
			sf90Block3.MessageTypeCode = ISFMessageStatus.Codes.RecordAcceptedWithWarning;
			message3.MessageBlock.AddMessageBlock(sf90Block3);
			AssertEquals("If the Message Type is a response from Customs and the Message Type Code is 'Record Accepted With Warning', the Customs Reference field should be read only", true, header.BF_CustomsReferenceInfo.ReadOnly);
			header.Messages.RemoveAndDeleteAll();
			var message4 = header.Messages.AddNew(typeof(MQEDIMessage));
			message4.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			ISFSF90 sf90Block4 = new ISFSF90();
			sf90Block4.MessageTypeCode = ISFMessageStatus.Codes.RecordRejected;
			message4.MessageBlock.AddMessageBlock(sf90Block4);
			AssertEquals("If the Message Type is a response from Customs and the Message Type Code is 'Record Rejected', the Customs Reference field should not be read only", false, header.BF_CustomsReferenceInfo.ReadOnly);
			header.Messages.RemoveAndDeleteAll();
			var message5 = header.Messages.AddNew(typeof(MQEDIMessage));
			message5.EM_MessageType = ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse;
			ISFSF90 sf90Block5 = new ISFSF90();
			sf90Block5.MessageTypeCode = ISFMessageStatus.Codes.Rejected;
			message5.MessageBlock.AddMessageBlock(sf90Block5);
			AssertEquals("If the Message Type is a response from Customs and the Message Type Code is 'Rejected', the Customs Reference field should not be read only", false, header.BF_CustomsReferenceInfo.ReadOnly);
		}

		public void TestDISStatus()
		{
			var header = Factory.New<CusISFHeader>();
			var requireDocument = header.RequiredDocuments.AddNew();
			var requireDocumentAddInfo = requireDocument.AddInfos.AddNew();
			requireDocumentAddInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			requireDocumentAddInfo.EX_Status = ZString.Empty;
			AssertEquals(ZString.Empty, header.DISStatus);
			AssertEquals(ZString.Empty, header.DISStatusDescription);
			requireDocumentAddInfo.EX_Status = Enterprise.Customs.Common.US.DIS.StatusList.Codes.AOS;
			AssertEquals(Enterprise.Customs.Common.US.DIS.StatusList.Codes.AOS, header.DISStatus);
			AssertEquals(Enterprise.Customs.Common.US.DIS.StatusList.Descriptions.AOS, header.DISStatusDescription);
			requireDocument = header.RequiredDocuments.AddNew();
			requireDocumentAddInfo = requireDocument.AddInfos.AddNew();
			requireDocumentAddInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			requireDocumentAddInfo.EX_Status = Enterprise.Customs.Common.US.DIS.StatusList.Codes.AOS;
			AssertEquals(Enterprise.Customs.Common.US.DIS.StatusList.Codes.AOS, header.DISStatus);
			AssertEquals(Enterprise.Customs.Common.US.DIS.StatusList.Descriptions.AOS, header.DISStatusDescription);
			requireDocumentAddInfo.EX_Status = Enterprise.Customs.Common.US.DIS.StatusList.Codes.ARS;
			AssertEquals(Enterprise.Customs.Common.US.DIS.StatusList.Codes.MUL, header.DISStatus);
			AssertEquals(Enterprise.Customs.Common.US.DIS.StatusList.Descriptions.MUL, header.DISStatusDescription);
		}

		public void TestIControllerIDProviderMembers()
		{
			var header = Factory.New<CusISFHeader>();
			IControllerIDProvider provider = header;
			AssertEquals("ControllerID", ControllerIDs.ImporterSecurityFiling, provider.ControllerID);
			AssertEquals("BusinessObjectPK", header.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestIsBond16SingleTransaction()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ISFBond16;
			header.BF_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			Assert(header.IsBond16SingleTransaction);
			header.BF_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			Assert(!header.IsBond16SingleTransaction);
		}

		public void TestMainShipToPartyOrgName()
		{
			OrgHeader buyingParty = Factory.New<OrgHeader>();
			buyingParty.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "W004", Core.Constants.CountryCodes.UnitedStates);
			buyingParty.OH_FullName = "Planet Express";
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.MainShipToParty.OrganisationPK = buyingParty.PK;
			header.MainShipToParty.E2_OA_Address = buyingParty.MainAddress.PK;
			AssertEquals("Planet Express", header.MainShipToPartyOrgName);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			AssertEquals("Register no", "W004", header.MainShipToParty.ISFRequirement.GetRegistrationNumberResult(header.MainShipToParty).RegistrationNumber);
		}

		public void TestBuyingPartyForDisplay()
		{
			var header = Factory.New<CusISFHeader>();
			header.BuyingParty.E2_GovRegNumType = CodeTypeList.Codes.SocialSecurity;
			header.BuyingParty.E2_SocialSecurityNumber = "123-45-6789";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals("***-**-**** DOB:", header.BuyingParty.E2_SocialSecurityNumberDetails);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			AssertEquals("123-45-6789 DOB:", header.BuyingParty.E2_SocialSecurityNumberDetails);
		}

		public void TestSellingPartyForDisplay()
		{
			var header = Factory.New<CusISFHeader>();
			header.SellingParty.E2_GovRegNumType = CodeTypeList.Codes.SocialSecurity;
			header.SellingParty.E2_SocialSecurityNumber = "123-45-6789";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals("***-**-**** DOB:", header.SellingParty.E2_SocialSecurityNumberDetails);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			AssertEquals("123-45-6789 DOB:", header.SellingParty.E2_SocialSecurityNumberDetails);
		}

		public void TestImporterCustomsAddressFallingBackToMainAddress()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			AssertEquals(importer.MainAddress.PK, header.ImporterCustomsAddressFallingBackToMainAddress.PK);
			OrgAddress addressCAR = importer.Addresses.AddNew();
			addressCAR.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			AssertEquals(addressCAR.PK, header.ImporterCustomsAddressFallingBackToMainAddress.PK);
		}

		public void TestRegistrationNumberForPuertoRicoCompany()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "1234", Core.Constants.CountryCodes.UnitedStates);
			var header = Factory.New<CusISFHeader>();
			var docAddress = header.DocAddresses.FindOrCreateWithRequirement(header.ISFDocAddressRequirementProvider.ConsigneeDocAddressRequirement);
			docAddress.OrganisationPK = consignee.PK;
			AssertEquals("1234", docAddress.ISFRequirement.GetRegistrationNumberResult(docAddress).RegistrationNumber);
			var consolidator = Factory.New<OrgHeader>();
			header.Consolidator.OrganisationPK = consolidator.PK;
			var cusCode = consolidator.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "OH1234", Core.Constants.CountryCodes.UnitedStates);
			cusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			var address = consolidator.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "OA5678", Core.Constants.CountryCodes.UnitedStates);
			header.Consolidator.E2_OA_Address = address.PK;
			AssertEquals("OA5678", header.Consolidator.ISFRequirement.GetRegistrationNumberResult(header.Consolidator).RegistrationNumber);
			var buyingParty = Factory.New<OrgHeader>();
			buyingParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "1111", Core.Constants.CountryCodes.UnitedStates);
			header.BuyingParty.OrganisationPK = buyingParty.PK;
			AssertEquals("1111", header.BuyingParty.ISFRequirement.GetRegistrationNumberResult(header.BuyingParty).RegistrationNumber);
		}

		public void TestISFSupportUserDeletionOfAddresses()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.LoadChildEditableObjects();
			IDocAddresses docAddresses = header;
			List<ISFDocAddress> deleteNotSupportedList = new List<ISFDocAddress>(new ISFDocAddress[]
			{
				header.MainShipToParty,
				header.BuyingParty,
				header.SellingParty,
				header.Consolidator,
				header.StuffingLocation,
				header.BookingParty
			});
			foreach (DocAddressType addressType in docAddresses.SupportedAddressTypes)
			{
				header.DocAddresses.CreateWithRequirement(docAddresses.GetDocAddressRequirement(addressType));
			}

			foreach (ISFDocAddress docAddress in header.DocAddresses)
			{
				AssertEquals(!deleteNotSupportedList.Contains(docAddress), docAddresses.CanDeleteAddress(docAddress));
			}
		}

		public void TestISFBillStatus_Description()
		{
			var header = Factory.New<CusISFHeader>();
			var oceanBill1 = CreateBill(header, BillTypeList.Codes.OceanBillOfLading, "OB1", DispositionCodeList.Codes.S1);
			var houseBill1 = CreateBill(header, BillTypeList.Codes.HouseBillOfLading, "HB1", DispositionCodeList.Codes.S1);
			var masterBill1 = CreateBill(header, BillTypeList.Codes.MasterBillOfLading, "MB1", DispositionCodeList.Codes.S1);
			AssertEquals("All statuses are the same", DispositionCodeList.Codes.S1, header.BF_BillStatus);
			masterBill1.BB_CustomsStatus = DispositionCodeList.Codes.S2;
			AssertEquals("No change as Master Bill is not included", DispositionCodeList.Codes.S1, header.BF_BillStatus);
			AssertEquals("No change as Master Bill is not included", DispositionCodeList.Descriptions.S1, header.BF_BillStatusDescription);
			houseBill1.BB_CustomsStatus = DispositionCodeList.Codes.S2;
			AssertEquals("Should be multiple", Common.US.ISF.ISFStatusHelper.Multiple, header.BF_BillStatus);
			AssertEquals("Should be multiple", CusISFHeader.MultipleBillsWithDifferentStatuses, header.BF_BillStatusDescription);
			oceanBill1.BB_CustomsStatus = ZString.Empty;
			AssertEquals("houseBill1 status", DispositionCodeList.Codes.S2 + ",No Status", header.BF_BillStatus);
			AssertEquals("houseBill1 status", DispositionCodeList.Descriptions.S2 + " Also there is a bill without any status.", header.BF_BillStatusDescription);
			houseBill1.BB_CustomsStatus = ZString.Empty;
			AssertEquals("No status", ZString.Empty, header.BF_BillStatus);
			AssertEquals("No status", ZString.Empty, header.BF_BillStatusDescription);
			var houseBill2 = CreateBill(header, BillTypeList.Codes.HouseBillOfLading, "HB2", DispositionCodeList.Codes.S1);
			AssertEquals("houseBill2 status", DispositionCodeList.Codes.S1 + ",No Status", header.BF_BillStatus);
			AssertEquals("houseBill2 status", DispositionCodeList.Descriptions.S1 + " Also there is a bill without any status.", header.BF_BillStatusDescription);
			var oceanBill2 = CreateBill(header, BillTypeList.Codes.OceanBillOfLading, "OB2", DispositionCodeList.Codes.S1);
			AssertEquals("Same status", DispositionCodeList.Codes.S1 + ",No Status", header.BF_BillStatus);
			AssertEquals("Same status", DispositionCodeList.Descriptions.S1 + " Also there is a bill without any status.", header.BF_BillStatusDescription);
			oceanBill2.BB_CustomsStatus = DispositionCodeList.Codes.S3;
			AssertEquals("Should be multiple", Common.US.ISF.ISFStatusHelper.Multiple, header.BF_BillStatus);
			AssertEquals("Should be multiple", CusISFHeader.MultipleBillsWithDifferentStatuses, header.BF_BillStatusDescription);
		}

		public void TestISFFirstMatchedDate()
		{
			var header = Factory.New<CusISFHeader>();
			var oceanBill1 = CreateBill(header, BillTypeList.Codes.MasterBillOfLading, "OB1", DispositionCodeList.Codes.S1);
			var houseBill1 = CreateBill(header, BillTypeList.Codes.HouseBillOfLading, "HB1", DispositionCodeList.Codes.S1);
			var houseBill2 = CreateBill(header, BillTypeList.Codes.HouseBillOfLading, "HB2", DispositionCodeList.Codes.S1);
			AssertEquals("nothing has first matched date", ZDateTime.Empty, header.BF_FirstMatchedDate);
			houseBill1.BB_FirstMatchedDate = new ZDateTime(2013, 10, 14);
			AssertEquals(new ZDateTime(2013, 10, 14), header.BF_FirstMatchedDate);
			houseBill2.BB_FirstMatchedDate = new ZDateTime(2013, 10, 15);
			AssertEquals(new ZDateTime(2013, 10, 14), header.BF_FirstMatchedDate);
			houseBill2.BB_FirstMatchedDate = new ZDateTime(2013, 10, 13);
			AssertEquals(new ZDateTime(2013, 10, 13), header.BF_FirstMatchedDate);
		}

		public void TestIsISF10Entry()
		{
			var header = Factory.New<CusISFHeader>();
			var list = new SubmissionTypeList();
			list.RemoveCode(SubmissionTypeList.Codes.ISF10);
			list.RemoveCode(SubmissionTypeList.Codes.ISF5ToISF10);
			list.RemoveCode(SubmissionTypeList.Codes.LateISF10);
			foreach (ICodeDescription pair in list)
			{
				header.BF_EntryType = pair.Code;
				AssertEquals(false, header.IsISF10Entry);
			}

			foreach (var code in new string[] { SubmissionTypeList.Codes.ISF10, SubmissionTypeList.Codes.ISF5ToISF10, SubmissionTypeList.Codes.LateISF10 })
			{
				header.BF_EntryType = code;
				AssertEquals(true, header.IsISF10Entry);
			}
		}

		public void TestIsISF5Entry()
		{
			var header = Factory.New<CusISFHeader>();
			var list = new SubmissionTypeList();
			list.RemoveCode(SubmissionTypeList.Codes.ISF5);
			list.RemoveCode(SubmissionTypeList.Codes.ISF10ToISF5);
			list.RemoveCode(SubmissionTypeList.Codes.LateISF5);
			foreach (ICodeDescription pair in list)
			{
				header.BF_EntryType = pair.Code;
				AssertEquals(false, header.IsISF5Entry);
			}

			foreach (var code in new string[] { SubmissionTypeList.Codes.ISF5, SubmissionTypeList.Codes.ISF10ToISF5, SubmissionTypeList.Codes.LateISF5 })
			{
				header.BF_EntryType = code;
				AssertEquals(true, header.IsISF5Entry);
			}
		}

		public void TestIsBondDataRequired()
		{
			var header = Factory.New<CusISFHeader>();
			foreach (ICodeDescription submissionType in new SubmissionTypeList())
			{
				header.BF_EntryType = submissionType.Code;
				foreach (ICodeDescription shipmentType in new ShipmentTypeList())
				{
					header.BF_ShipmentType = shipmentType.Code;
					bool expectation = ShipmentTypeList.IsBondDataRequired(shipmentType.Code) && !SubmissionTypeList.IsLateEntry(submissionType.Code);
					AssertEquals(expectation, header.IsBondDataRequired);
				}
			}
		}

		public void TestDocAddressValidation()
		{
			var header = Factory.New<CusISFHeader>();
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			header.BF_EntryNumber = "1";
			header.BF_ShipmentType = ShipmentTypeList.Codes.StandardOrRegularFilings;
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselContainerized;
			header.BF_GB = branch.PK;
			header.BF_OH_Importer = ZGuid.Empty;
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.CompliantTransaction;
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			header.BF_ImporterCode = "45-985412300";
			header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.IRS;
			header.BF_ConsigneeCode = "45-985412300";
			header.BF_BondNumberOrHolder = "45-985412300";
			header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ImporterOrBroker;
			header.BF_BondType = BondTypeList.Codes.ContinuousBond;
			var involvedParty = Factory.NewWithValidTestData<OrgHeader>();
			header.SellingParty.OrganisationPK = involvedParty.PK;
			header.BuyingParty.OrganisationPK = involvedParty.PK;
			header.Consolidator.OrganisationPK = involvedParty.PK;
			header.StuffingLocation.OrganisationPK = involvedParty.PK;
			header.BF_EntryNumber = "FFF12345678";
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Six;
			header.Lines.AddNew();
			header.BF_HouseBill = "SCAC454";
			Factory.Save();
			header.SellingParty.OrganisationPK = ZGuid.Empty;
			var notifications = ISFMessageSendingValidation.New(header, null).CheckBusinessObjectLevelValidation();
			Assert("Notifications should be included", notifications.Count > 0);
			AssertContains("Selling party should be notified", "A Selling Party must be specified", notifications[0].Message);
		}

		public void TestHouseMasterOceanBillReferences()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.ReferenceDatas.AddNew();
			header.ReferenceDatas[0].BB_BillType = "BM";
			header.ReferenceDatas[0].BB_BillNum = "BM1111";
			header.ReferenceDatas.AddNew();
			header.ReferenceDatas[1].BB_BillType = "MB";
			header.ReferenceDatas[1].BB_BillNum = "MB2222";
			header.ReferenceDatas.AddNew();
			header.ReferenceDatas[2].BB_BillType = "OB";
			header.ReferenceDatas[2].BB_BillNum = "OB3333";
			header.ReferenceDatas.AddNew();
			header.ReferenceDatas[3].BB_BillType = "OB";
			header.ReferenceDatas[3].BB_BillNum = "OB4444";
			AssertEquals("House: BM1111 Master: MB2222 Ocean Bill: OB3333,OB4444", header.HouseMasterOceanBillReferences);
		}

		public void TestGetOrgCusCodeObjectMatchingCountryAndCodes()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			RefCountry uSC = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);
			RefCountry australiaC = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "00-1234123AU", australiaC);
			organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "00-1234123US", uSC);
			organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BrokeragePrinter, "WrongCodeUS", uSC);
			Factory.Save();
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = organisation.PK;
			AssertEquals(header.BF_ImporterCode, "00-1234123US");
			AssertEquals(header.BF_ImporterCodeType, OrgCusCode.USACodeTypes.EmployerIdentificationNumber);
		}

		public void TestImporterCodeForDocument()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_ImporterCodeType = ConsigneeCodeTypeList.Codes.SocialSecurity;
			header.BF_ImporterCode = "123-45-6789";

			AssertEquals(ZString.Empty, header.ImporterCodeForDocument);

			header.BF_ImporterCodeType = ConsigneeCodeTypeList.Codes.IRS;
			header.BF_ImporterCode = "00-1234123US";

			AssertEquals("00-1234123US", header.ImporterCodeForDocument);
		}

		public void TestConsigneeTypeAndCodeForDocument()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.SocialSecurity;
			header.BF_ConsigneeCode = "123-45-6789";

			AssertEquals(ZString.Empty, header.ConsigneeTypeAndCodeForDocument);

			header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.CBPAssignedNumber;
			header.BF_ConsigneeCode = "00-1234123US";

			AssertEquals("CBN: 00-1234123US", header.ConsigneeTypeAndCodeForDocument);
		}

		public void TestBondHolderNumberForDocument()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var uSC = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);
			organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "123-45-6789", uSC);
			Factory.Save();

			var header = Factory.New<CusISFHeader>();
			header.BF_BondNumberOrHolder = "123-45-6789";
			AssertEquals(ZString.Empty, header.BondHolderNumberForDocument);

			organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "00-1234123US", uSC);
			Factory.Save();

			header = Factory.New<CusISFHeader>();
			header.BF_BondNumberOrHolder = "00-1234123US";
			AssertEquals("00-1234123US", header.BondHolderNumberForDocument);
		}

		public void TestFieldReadOnly()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			foreach (ICodeDescription pair in Factory.GetCachedValue<ConsigneeCodeTypeList>())
			{
				header.BF_ConsigneeCodeType = pair.Code;
				AssertEquals(pair.Code != ConsigneeCodeTypeList.Codes.Passport, header.BF_ConsigneeCountryOfIssueInfo.ReadOnly);
				AssertEquals(pair.Code != ConsigneeCodeTypeList.Codes.Passport && pair.Code != ConsigneeCodeTypeList.Codes.SocialSecurity, header.BF_ConsigneeDateOfBirthInfo.ReadOnly);
				AssertEquals(pair.Code != ConsigneeCodeTypeList.Codes.Passport && pair.Code != ConsigneeCodeTypeList.Codes.SocialSecurity, header.BF_ConsigneeFullNameInfo.ReadOnly);
			}

			foreach (ICodeDescription pair in Factory.GetCachedValue<ImporterCodeTypeList>())
			{
				header.BF_ImporterCodeType = pair.Code;
				AssertEquals(pair.Code != ImporterCodeTypeList.Codes.Passport, header.BF_CountryOfIssueInfo.ReadOnly);
				AssertEquals(pair.Code != ImporterCodeTypeList.Codes.Passport && pair.Code != ImporterCodeTypeList.Codes.SocialSecurity, header.BF_DateOfBirthInfo.ReadOnly);
				AssertEquals(pair.Code != ImporterCodeTypeList.Codes.Passport && pair.Code != ImporterCodeTypeList.Codes.SocialSecurity, header.BF_ImporterFullNameInfo.ReadOnly);
			}
		}

		public void TestReadOnlyWhenDeactivated()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_JobReference = "ISF23423BD";
			Factory.Save();
			header.IsCancelled = false;
			Factory.Save();
			AssertEquals(false, header.BF_BondIndicatorInfo.ReadOnly);
			header.IsCancelled = true;
			header.OnLoaded();
			AssertEquals(true, header.BF_BondIndicatorInfo.ReadOnly);
		}

		public void TestDefaultingBF_BondNumberOrHolderFromBF_ImporterCode()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			var list = new ShipmentTypeList();
			var validList = new string[]
			{
				ShipmentTypeList.Codes.StandardOrRegularFilings,
				ShipmentTypeList.Codes.ToOrderShipments,
				ShipmentTypeList.Codes.USReturnGoods,
				ShipmentTypeList.Codes.FTZShipments,
				ShipmentTypeList.Codes.OuterContinentalShelfShipments
			};
			foreach (string validCode in validList)
			{
				list.RemoveCode(validCode);
			}

			var importerTypes = new string[]
			{
				ImporterCodeTypeList.Codes.IRS,
				ImporterCodeTypeList.Codes.CBPAssignedNumber,
				ImporterCodeTypeList.Codes.SocialSecurity
			};
			foreach (string importerType in importerTypes)
			{
				header.BF_ImporterCodeType = importerType;
				header.BF_BondNumberOrHolder = ZString.Empty;
				foreach (ICodeDescription pair in list)
				{
					header.BF_ShipmentType = pair.Code;
					header.BF_ImporterCode = pair.Code + "123";
					AssertEquals(ZString.Empty, header.BF_BondNumberOrHolder);
				}

				foreach (string validCode in validList)
				{
					header.BF_ShipmentType = validCode;
					header.BF_ImporterCode = validCode + "123";
					AssertEquals(header.BF_ImporterCode, header.BF_BondNumberOrHolder);
				}
			}
		}

		public void TestSF13Fields()
		{
			var header = Factory.New<CusISFHeader>();
			var sf13Provider = header as IImporterSecurityFiling;
			AssertEquals(ZString.Empty, sf13Provider.ShipmentSubType);
			AssertEquals(ZDecimal.Zero, sf13Provider.EstimatedValue);
			AssertEquals(ZDecimal.Zero, sf13Provider.EstimatedQuantity);
			AssertEquals(ZString.Empty, sf13Provider.UnitOfMeasure);
			AssertEquals(ZDecimal.Zero, sf13Provider.EstimatedWeight);
			AssertEquals(ZString.Empty, sf13Provider.WeightQualifier);
			header.BF_ShipmentSubType = ShipmentSubTypeList.Codes.GeneralNote3eShipments;
			header.BF_EstimatedValue = 10m;
			header.BF_EstimatedQuantity = 30;
			header.BF_EstimatedQuantityUQ = ShippingOrPackingingUnitList.Codes.Aerosol;
			header.BF_EstimatedWeight = 20;
			header.BF_EstimatedWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(ShipmentSubTypeList.Codes.GeneralNote3eShipments, sf13Provider.ShipmentSubType);
			AssertEquals(10m, sf13Provider.EstimatedValue);
			AssertEquals(30m, sf13Provider.EstimatedQuantity);
			AssertEquals(ShippingOrPackingingUnitList.Codes.Aerosol, sf13Provider.UnitOfMeasure);
			AssertEquals(20m, sf13Provider.EstimatedWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, sf13Provider.WeightQualifier);
			header.BF_EstimatedWeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals(20000m, sf13Provider.EstimatedWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, sf13Provider.WeightQualifier);
		}

		public void TestBF_ImporterCodeMaxLength()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			AssertEquals(CusISFHeader.Schema.BF_ImporterCodeMaxLength, header.BF_ImporterCodeInfo.MaxLength);
			header.BF_ImporterCode = "1234567890";
			AssertEquals("1234567890", header.BF_ImporterCode);
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.SCAC;
			AssertEquals(4, header.BF_ImporterCodeInfo.MaxLength);
			AssertEquals("1234", header.BF_ImporterCode);
		}

		public void TestImporterCodeForDisplay()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.SocialSecurity;
			header.BF_ImporterCode = "123-45-6789";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals("***-**-****", header.ImporterCodeForDisplay);

			header.BF_ImporterCodeType = ZString.Empty;
			AssertEquals(ZString.Empty, header.ImporterCodeForDisplay);

			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.SocialSecurity;
			header.BF_ImporterCode = "123-45-6789";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			AssertEquals("123-45-6789", header.ImporterCodeForDisplay);

			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.Passport;
			header.BF_ImporterCode = "L23432K32";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals("L23432K32", header.ImporterCodeForDisplay);
		}

		public void TestImporterCodeForDisplay_ReadOnly()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.SocialSecurity;
			header.BF_ImporterCode = "123-45-6789";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals(true, header.ImporterCodeForDisplayInfo.ReadOnly);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			AssertEquals(false, header.ImporterCodeForDisplayInfo.ReadOnly);

			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.Passport;
			header.BF_ImporterCode = "L23432K32";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals(false, header.ImporterCodeForDisplayInfo.ReadOnly);
		}

		public void TestConsigneeCodeForDisplay()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.SocialSecurity;
			header.BF_ConsigneeCode = "123-45-6789";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals("***-**-****", header.ConsigneeCodeForDisplay);

			header.BF_ConsigneeCodeType = ZString.Empty;
			AssertEquals(ZString.Empty, header.ConsigneeCodeForDisplay);

			header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.SocialSecurity;
			header.BF_ConsigneeCode = "123-45-6789";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			AssertEquals("123-45-6789", header.ConsigneeCodeForDisplay);

			header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.Passport;
			header.BF_ConsigneeCode = "L23432K32";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals("L23432K32", header.ConsigneeCodeForDisplay);
		}

		public void TestConsigneeCodeForDisplay_ReadOnly()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.SocialSecurity;
			header.BF_ConsigneeCode = "123-45-6789";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals(true, header.ConsigneeCodeForDisplayInfo.ReadOnly);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			AssertEquals(false, header.ConsigneeCodeForDisplayInfo.ReadOnly);

			header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.Passport;
			header.BF_ConsigneeCode = "L23432K32";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals(false, header.ConsigneeCodeForDisplayInfo.ReadOnly);
		}

		public void TestBondNumberOrHolderForDisplay()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_BondNumberOrHolder = "123-45-6789";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals("***-**-****", header.BondNumberOrHolderForDisplay);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			AssertEquals("123-45-6789", header.BondNumberOrHolderForDisplay);
		}

		public void TestBondNumberOrHolderForDisplay_ReadOnly()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_BondNumberOrHolder = "123-45-6789";

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertEquals(true, header.BondNumberOrHolderForDisplayInfo.ReadOnly);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			AssertEquals(false, header.BondNumberOrHolderForDisplayInfo.ReadOnly);
		}

		public void TestUpdateImporterCodeDetailsIfNeeded()
		{
			CusISFHeader header = Factory.NewWithValidTestData<CusISFHeader>();
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "sdfdf");
			header.BF_OH_Importer = importer.PK;
			Factory.Save();
			AssertEquals("sdfdf", header.BF_ImporterCode);
			importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "sdfdfsdfdfsdfdfsdfdf");
			header.BF_OH_Importer = importer.PK;
			Factory.Save();
			AssertEquals("???", header.BF_ImporterCode);
			importer = Factory.NewWithValidTestData<OrgHeader>();
			RefCountry uS = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-12321212", uS);
			header.BF_OH_Importer = importer.PK;
			Factory.Save();
			AssertEquals("12-12321212", header.BF_ImporterCode);
		}

		public void TestSetDefaultValues()
		{
			ISFRegistry.Instance.ImporterSecurityFilingNoOfHTSDigits.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, NumberOfHarmonizedDigitsList.Codes.Eight);
			CusISFHeader header = Factory.New<CusISFHeader>();
			AssertEquals(GlbBranch.CurrentBranch.PK, header.BF_GB);
			AssertEquals(TransportModeCodes.Codes.OceanVesselContainerized, header.BF_TransportMode);
			AssertEquals(SubmissionTypeList.Codes.ISF10, header.BF_EntryType);
			AssertEquals(YesNoDefaultList.Codes.Default, header.BF_SendEquipment);
			AssertEquals(MergeStyleList.Codes.Default, header.BF_LineMergeStyle);
			AssertEquals(NumberOfHarmonizedDigitsList.Codes.Eight, header.BF_NumOfHarmChars);
			AssertEquals(MessageStatusList.Codes.NotSentISF, header.BF_CustomsStatus);
			AssertEquals(ActionReasonCodeList.Codes.CompliantTransaction, header.BF_ActionReasonCode);
		}

		public void TestBranchSpecificDefaulting()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "Z!1";
			company1.GC_Name = "DUMMY COMPANY";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "Z!1";
			branch1.GB_BranchName = "DUMMY BRANCH";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var branch1PK = branch1.PK.ToGuid();
			ISFRegistry.Instance.ImporterSecurityFilingNoOfHTSDigits.SetValue(Guid.Empty, branch1PK, Guid.Empty, NumberOfHarmonizedDigitsList.Codes.Ten);
			ISFRegistry.Instance.ImporterSecurityFilingNoOfHTSDigits.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, NumberOfHarmonizedDigitsList.Codes.Eight);
			CusISFHeader header = Factory.New<CusISFHeader>();
			AssertEquals(NumberOfHarmonizedDigitsList.Codes.Eight, header.BF_NumOfHarmChars);
			header.BF_GB = branch1.PK;
			AssertEquals(NumberOfHarmonizedDigitsList.Codes.Ten, header.BF_NumOfHarmChars);
		}

		public void TestNumberOfHarmonisedDigitsRequired()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Six;
			AssertEquals(6, header.NumberOfHarmonisedDigitsRequired);
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Eight;
			AssertEquals(8, header.NumberOfHarmonisedDigitsRequired);
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Ten;
			AssertEquals(10, header.NumberOfHarmonisedDigitsRequired);
			header.BF_NumOfHarmChars = ZString.Empty;
			AssertEquals(10, header.NumberOfHarmonisedDigitsRequired);
			header.BF_NumOfHarmChars = "ZZ";
			AssertEquals(10, header.NumberOfHarmonisedDigitsRequired);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			AssertEquals("Should be 0 business objects with related logs", 0, header.BusinessObjectsWithRelatedEvents.Length);
			header.ReferenceDatas.AddNew();
			AssertEquals("Business objects with related logs", 1, header.BusinessObjectsWithRelatedEvents.Length);
			header.ReferenceDatas.AddNew();
			AssertEquals("Business objects with related logs", 2, header.BusinessObjectsWithRelatedEvents.Length);
			var jobHeader = new JobHeader.Loader(header).TryCreate();
			AssertEquals("Business objects with related logs", 3, header.BusinessObjectsWithRelatedEvents.Length);
			AssertCollectionContains("Business objects with related logs contains the Job", jobHeader, header.BusinessObjectsWithRelatedEvents);
		}

		public void TestIHaveRequiredDocumentsMembers()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_JobReference = "BF234232";
			header.BF_HouseBill = "HB123423";
			header.BF_MasterBill = "MB323435";
			IHaveRequiredDocuments haveRequiredDocuments = header;
			AssertNull(haveRequiredDocuments.AdditionalRefTypes);
			AssertNull(haveRequiredDocuments.ExportBroker);
			AssertEquals("HB123423", haveRequiredDocuments.HouseBill);
			AssertEquals(header.Logs, haveRequiredDocuments.Logs);
			AssertEquals("MB323435", haveRequiredDocuments.MasterBill);
			AssertEquals(header.PK, haveRequiredDocuments.PK);
			AssertEquals(header.RequiredDocuments, haveRequiredDocuments.RequiredDocuments);
			AssertEquals(header.TablePrefix, haveRequiredDocuments.TableCode);
			AssertEquals(header, haveRequiredDocuments.UltimateDocumentParent);
			AssertEquals("BF234232", haveRequiredDocuments.UniqueConsignRef);
		}

		public void TestNoAIDEventLoggingOnBaseJobDeclarationWithCountryRequiredDocuments()
		{
			var testClasses = new RefCountryRequiredDocumentCollectionTest();
			testClasses.CreateRequiredDocumentsForUSAAndNoOrigin();
			var header = Factory.New<CusISFHeader>();
			header.BF_RL_NKPlaceOfDelivery = "USLAX";
			var requiredDocument = header.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.BeneficiaryCertificate;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Import;
			requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = DateTime.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";
			Factory.Save();
			AssertNull("AID event should not be logged", header.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
		}

		public void TestAIDEventLoggingOnBaseJobDeclaration()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_RL_NKPlaceOfDelivery = "USLAX";
			var requiredDocument = header.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.BeneficiaryCertificate;
			requiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Import;
			requiredDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			requiredDocument.EQ_DateReceived = DateTime.Today;
			requiredDocument.EQ_ValidToDate = DateTime.Today;
			requiredDocument.EQ_DocNumber = "0001";
			Factory.Save();
			AssertNotNull("AID event should be logged", header.Logs.MostRecentLogByEventTime(Events.AllImportDocumentsReceived));
		}

		public void TestImporterDetailsDefault()
		{
			var org = Factory.New<OrgHeader>();
			var uS = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);
			org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-12321212", uS);
			var header = Factory.New<CusISFHeader>();
			AssertEquals(ZString.Empty, header.BF_ImporterCode);
			AssertEquals(ZString.Empty, header.BF_ImporterCodeType);
			AssertEquals(ZGuid.Empty, header.BF_OH_Importer);
			header.BF_OH_Importer = org.PK;
			AssertEquals("12-12321212", header.BF_ImporterCode);
			AssertEquals(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, header.BF_ImporterCodeType);
			AssertEquals(org.PK, header.BF_OH_Importer);
			header.BF_OH_Importer = ZGuid.Empty;
			header.BF_ImporterCode = ZString.Empty;
			header.BF_ImporterCode = "12-12321212";
			AssertEquals("12-12321212", header.BF_ImporterCode);
			AssertEquals(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, header.BF_ImporterCodeType);
			AssertEquals(org.PK, header.BF_OH_Importer);
			header.BF_OH_Importer = ZGuid.Empty;
			header.BF_ImporterCodeType = ZString.Empty;
			header.BF_ImporterCodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			AssertEquals(ZString.Empty, header.BF_ImporterCode);
			AssertEquals(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, header.BF_ImporterCodeType);
			AssertEquals(ZGuid.Empty, header.BF_OH_Importer);
		}

		public void TestImporterDetailsDefaultForPassport()
		{
			var org = Factory.New<OrgHeader>();
			var uSCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);
			var cusCode1 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PS242342", uSCountry);
			CusISFHeader header = Factory.New<CusISFHeader>();
			AssertEquals(ZString.Empty, header.BF_ImporterCode);
			AssertEquals(ZString.Empty, header.BF_ImporterCodeType);
			AssertEquals(ZGuid.Empty, header.BF_OH_Importer);
			header.BF_OH_Importer = org.PK;
			AssertEquals("PS242342", header.BF_ImporterCode);
			AssertEquals(OrgCusCode.CodeTypes.PassportID, header.BF_ImporterCodeType);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, header.BF_CountryOfIssue);
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			header.BF_OH_Importer = ZGuid.Empty;
			header.BF_ImporterCode = ZString.Empty;
			header.BF_ImporterCodeType = ZString.Empty;
			header.BF_CountryOfIssue = ZString.Empty;
			header.BF_OH_Importer = org.PK;
			AssertEquals("PS242342", header.BF_ImporterCode);
			AssertEquals(OrgCusCode.CodeTypes.PassportID, header.BF_ImporterCodeType);
			AssertEquals(Core.Constants.CountryCodes.Australia, header.BF_CountryOfIssue);
			var cusCode2 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "UE96584", uSCountry);
			header.BF_OH_Importer = ZGuid.Empty;
			header.BF_ImporterCode = ZString.Empty;
			header.BF_ImporterCodeType = ZString.Empty;
			header.BF_CountryOfIssue = ZString.Empty;
			header.BF_OH_Importer = org.PK;
			AssertEquals("UE96584", header.BF_ImporterCode);
			AssertEquals(OrgCusCode.CodeTypes.PassportID, header.BF_ImporterCodeType);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, header.BF_CountryOfIssue);
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			header.BF_OH_Importer = ZGuid.Empty;
			header.BF_ImporterCode = ZString.Empty;
			header.BF_ImporterCodeType = ZString.Empty;
			header.BF_CountryOfIssue = ZString.Empty;
			header.BF_OH_Importer = org.PK;
			AssertEquals(ZString.Empty, header.BF_ImporterCode);
			AssertEquals(ZString.Empty, header.BF_ImporterCodeType);
			AssertEquals(ZString.Empty, header.BF_CountryOfIssue);
			cusCode1.Delete();
			header.BF_OH_Importer = ZGuid.Empty;
			header.BF_OH_Importer = org.PK;
			AssertEquals("UE96584", header.BF_ImporterCode);
			AssertEquals(OrgCusCode.CodeTypes.PassportID, header.BF_ImporterCodeType);
			AssertEquals(Core.Constants.CountryCodes.NewZealand, header.BF_CountryOfIssue);
			header.BF_OH_Importer = ZGuid.Empty;
			header.BF_ImporterCodeType = OrgCusCode.CodeTypes.PassportID;
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.NewZealand;
			header.BF_ImporterCode = ZString.Empty;
			header.BF_ImporterCode = "UE96584";
			AssertEquals(org.PK, header.BF_OH_Importer);
		}

		public void TestImporterName()
		{
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_IsConsignee = ZBool.True;
			importer.OH_FullName = "ISF Importer Co. P/L.";
			CusISFHeader header = Factory.New<CusISFHeader>();
			AssertEquals("Importer Name", ZString.Empty, header.ImporterName);
			header.BF_OH_Importer = importer.PK;
			AssertEquals("Importer Name", "ISF Importer Co. P/L.", header.ImporterName);
		}

		[TestDate(2009, 6, 10)]
		public void TestConsigneeCodeDefault()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.FIRMS;
			AssertEquals("", header.BF_ConsigneeCodeType);
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			AssertEquals(ConsigneeCodeTypeList.Codes.IRS, header.BF_ConsigneeCodeType);
		}

		public void TestResetToOriginal()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			header.BF_CustomsReference = "BZ2123423";
			header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
			var bill = header.ReferenceDatas.AddNew();
			var billLog = bill.Logs.AddNew(Events.MessageStatusChange, DispositionCodeList.Codes.S2);
			var log = header.Logs.AddNew(Events.MessageStatusChange, MessageStatusList.Codes.ClearISFAdd);
			var message1 = Factory.New<EDIMessage>();
			header.Messages.Add(message1);
			message1.EM_Status = EDIMessage.Status.Received;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var message2 = Factory.New<EDIMessage>();
			header.Messages.Add(message2);
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals(MessageStatusList.Codes.ClearISFAdd, header.BF_CustomsStatus);
			header.ResetToOriginal();
			AssertEquals(MessageStatusList.Codes.NotSentISF, header.BF_CustomsStatus);
			AssertEquals(ZString.Empty, header.BF_CustomsReference);
			AssertEquals(true, billLog.SL_IsCancelled);
			AssertEquals(true, log.SL_IsCancelled);
			AssertEquals(EDIMessage.Status.Discarded, message1.EM_Status);
			AssertEquals(EDIMessage.Status.Discarded, message2.EM_Status);
		}

		public void TestSavingSetsJobReference()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			ZString jobReference = Env.NumberFountains.ImporterSecurityFilingReference.PeekPreliminaryFormatted(Factory);
			AssertNotEquals(ZString.Empty, jobReference);
			AssertEquals("", header.BF_JobReference);
			Factory.Save();
			AssertEquals(jobReference, header.BF_JobReference);
		}

		public void TestIImporterSecurityFilingContainerData()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "Z!1";
			company1.GC_Name = "DUMMY COMPANY";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "Z!1";
			branch1.GB_BranchName = "DUMMY BRANCH";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var branch1PK = branch1.PK.ToGuid();
			ISFRegistry.Instance.ImporterSecurityFilingShouldReportContainerToCustoms.SetValue(Guid.Empty, branch1PK, Guid.Empty, true);
			ISFRegistry.Instance.ImporterSecurityFilingShouldReportContainerToCustoms.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_SendEquipment = YesNoDefaultList.Codes.Default;
			IImporterSecurityFiling isf = header;
			CusISFEquip equipment1 = header.Equipments.AddNew();
			List<IContainerData> containers = new List<IContainerData>(isf.ContainerData);
			AssertEquals(0, containers.Count);
			header.BF_GB = branch1.PK;
			containers = new List<IContainerData>(isf.ContainerData);
			AssertEquals(1, containers.Count);
			AssertEquals(equipment1, containers[0]);
			header.BF_GB = GlbBranch.CurrentBranch.PK;
			ISFRegistry.Instance.ImporterSecurityFilingShouldReportContainerToCustoms.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			containers = new List<IContainerData>(isf.ContainerData);
			AssertEquals(1, containers.Count);
			AssertEquals(equipment1, containers[0]);
			header.BF_SendEquipment = YesNoDefaultList.Codes.No;
			containers = new List<IContainerData>(isf.ContainerData);
			AssertEquals(0, containers.Count);
			header.BF_SendEquipment = YesNoDefaultList.Codes.Yes;
			containers = new List<IContainerData>(isf.ContainerData);
			AssertEquals(1, containers.Count);
			AssertEquals(equipment1, containers[0]);
		}

		public void TestIMessageAttacheeBranch()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "Z!1";
			company1.GC_Name = "DUMMY COMPANY";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "Z!1";
			branch1.GB_BranchName = "DUMMY BRANCH";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var branch2 = company1.Branches.AddNew();
			branch2.GB_Code = "Z!2";
			branch2.GB_BranchName = "DUMMY 2 BRANCH";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var branch1PK = branch1.PK.ToGuid();
			var header = Factory.New<CusISFHeader>();
			header.BF_GB = branch1PK;
			IMessageAttachee attachee = header;
			AssertEquals(branch1, attachee.Branch);
		}

		public void TestManufacturer_CountryCodeChanged()
		{
			var header = Factory.New<CusISFHeader>();
			var manufacturer1 = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
			manufacturer1.E2_CompanyName = "Company 1";
			manufacturer1.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var manufacturer2 = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
			manufacturer2.E2_CompanyName = "Company 2";
			manufacturer2.E2_RN_NKCountryCode = Core.Constants.CountryCodes.NewCaledonia;
			Factory.Save();
			header.ResetDocAddresses();
			var line1 = header.Lines.AddNew();
			line1.BL_ManufacturerDocAddressPK = manufacturer1.PK;
			AssertEquals("AU", line1.BL_RN_NKGoodsOrigin);
			var line2 = header.Lines.AddNew();
			line2.BL_ManufacturerDocAddressPK = manufacturer2.PK;
			AssertEquals("NC", line2.BL_RN_NKGoodsOrigin);
			manufacturer1.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;
			AssertEquals("JP", line1.BL_RN_NKGoodsOrigin);
			AssertEquals("NC", line2.BL_RN_NKGoodsOrigin);
		}

		public void TestBF_EstimatedValue_WillRoundToNearestInteger()
		{
			var header = Factory.New<CusISFHeader>();
			CombineAssertions(() =>
			{
				header.BF_EstimatedValue = 1.33;
				AssertEquals(new ZDecimal(1), header.BF_EstimatedValue);
				header.BF_EstimatedValue = 1.5;
				AssertEquals(new ZDecimal(2), header.BF_EstimatedValue);
				header.BF_EstimatedValue = 1.82;
				AssertEquals(new ZDecimal(2), header.BF_EstimatedValue);
			});
		}

		public void TestIImporterSecurityFilingManufacturerData()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "Z!1";
			company1.GC_Name = "DUMMY COMPANY";
			company1.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "Z!1";
			branch1.GB_BranchName = "DUMMY BRANCH";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var branch1PK = branch1.PK.ToGuid();
			ISFRegistry.Instance.ImporterSecurityFilingShouldMergeLine.SetValue(Guid.Empty, branch1PK, Guid.Empty, true);
			ISFRegistry.Instance.ImporterSecurityFilingShouldMergeLine.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			CusISFHeader header = Factory.New<CusISFHeader>();
			IImporterSecurityFiling isf = header;
			var manufacturer1 = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
			manufacturer1.E2_AddressOverride = true;
			manufacturer1.E2_CompanyName = "Company 1";
			JobDocAddress manufacturer2 = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
			manufacturer2.E2_AddressOverride = true;
			manufacturer2.E2_CompanyName = "Company 2";
			CusISFLine line1 = header.Lines.AddNew();
			line1.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			line1.BL_HarmonisedNum = "1010101010";
			line1.BL_ManufacturerDocAddressPK = manufacturer1.PK;
			CusISFLine line2 = header.Lines.AddNew();
			line2.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.NewCaledonia;
			line2.BL_HarmonisedNum = "2010101010";
			line2.BL_ManufacturerDocAddressPK = ZGuid.Empty;
			CusISFLine line3 = header.Lines.AddNew();
			line3.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.NewZealand;
			line3.BL_HarmonisedNum = "3010101010";
			line3.BL_ManufacturerDocAddressPK = manufacturer2.PK;
			CusISFLine line4 = header.Lines.AddNew();
			line4.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.NewZealand;
			line4.BL_HarmonisedNum = "3010131010";
			line4.BL_ManufacturerDocAddressPK = manufacturer2.PK;
			CusISFLine line5 = header.Lines.AddNew();
			line5.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			line5.BL_HarmonisedNum = "1010101020";
			line5.BL_ManufacturerDocAddressPK = manufacturer1.PK;
			CusISFLine line6 = header.Lines.AddNew();
			line6.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.NewCaledonia;
			line6.BL_HarmonisedNum = "2010102010";
			line6.BL_ManufacturerDocAddressPK = ZGuid.Empty;
			CusISFLine line7 = header.Lines.AddNew();
			line7.BL_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			line7.BL_HarmonisedNum = "1010101020";
			line7.BL_ManufacturerDocAddressPK = manufacturer1.PK;
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Six;
			header.BF_LineMergeStyle = MergeStyleList.Codes.Merge;
			List<IManufacturerData> datas = new List<IManufacturerData>(isf.ManufacturerData);
			AssertEquals(3, datas.Count);
			AssertManufacturerData(datas[0], manufacturer1, new List<TariffData>()
			{ GetTariffData(line1) });
			AssertManufacturerData(datas[1], null, new List<TariffData>()
			{ GetTariffData(line2) });
			AssertManufacturerData(datas[2], manufacturer2, new List<TariffData>()
			{ GetTariffData(line3), GetTariffData(line4) });
			header.BF_LineMergeStyle = MergeStyleList.Codes.NotMerge;
			datas = new List<IManufacturerData>(isf.ManufacturerData);
			AssertEquals(3, datas.Count);
			AssertManufacturerData(datas[0], manufacturer1, new List<TariffData>()
			{ GetTariffData(line1), GetTariffData(line5), GetTariffData(line7) });
			AssertManufacturerData(datas[1], null, new List<TariffData>()
			{ GetTariffData(line2), GetTariffData(line6) });
			AssertManufacturerData(datas[2], manufacturer2, new List<TariffData>()
			{ GetTariffData(line3), GetTariffData(line4) });
			header.BF_LineMergeStyle = MergeStyleList.Codes.Default;
			datas = new List<IManufacturerData>(isf.ManufacturerData);
			AssertEquals(3, datas.Count);
			AssertManufacturerData(datas[0], manufacturer1, new List<TariffData>()
			{ GetTariffData(line1), GetTariffData(line5), GetTariffData(line7) });
			AssertManufacturerData(datas[1], null, new List<TariffData>()
			{ GetTariffData(line2), GetTariffData(line6) });
			AssertManufacturerData(datas[2], manufacturer2, new List<TariffData>()
			{ GetTariffData(line3), GetTariffData(line4) });
			header.BF_GB = branch1.PK;
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Six;
			datas = new List<IManufacturerData>(isf.ManufacturerData);
			AssertEquals(3, datas.Count);
			AssertManufacturerData(datas[0], manufacturer1, new List<TariffData>()
			{ GetTariffData(line1) });
			AssertManufacturerData(datas[1], null, new List<TariffData>()
			{ GetTariffData(line2) });
			AssertManufacturerData(datas[2], manufacturer2, new List<TariffData>()
			{ GetTariffData(line3), GetTariffData(line4) });
		}

		public virtual void TestCanCancel_JobInCurrentCompany()
		{
			var isfHeader = Factory.New<CusISFHeader>();
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = "BF";
			job.JH_ParentID = isfHeader.PK;
			Factory.Save();
			Assert("Can Cancel", string.IsNullOrEmpty(isfHeader.CanCancel()));

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			Assert(charge.JR_OSCostAmt.IsEmpty);
			Assert(charge.JR_OSSellAmt.IsEmpty);
			Factory.Save();
			Assert("Can Cancel", string.IsNullOrEmpty(isfHeader.CanCancel()));

			charge.JR_OSCostAmt = 5m;
			charge.JR_OSCostExRate = 1m;
			Factory.Save();

			var expectedMessage = $@"{isfHeader.HumanReadableName} cannot be deactivated.
Job Invoicing Charge(s) have been saved against this Invoicing Job Header ({job.JH_JobNum}) in the company EDI.";
			AssertEquals("Cannot be deactivated", expectedMessage, isfHeader.CanCancel());

			charge.JR_OSCostAmt = 0m;
			Factory.Save();
			AssertEquals("Cannot be deactivated", expectedMessage, isfHeader.CanCancel());

			charge.Delete();
			Factory.Save();

			expectedMessage = $@"{isfHeader.HumanReadableName} cannot be deactivated.
Accounting Transaction Line(s) have been saved against this Invoicing Job Header ({job.JH_JobNum}) in the company EDI.";
			AssertEquals("Cannot be deactivated", expectedMessage, isfHeader.CanCancel());
		}

		public void TestUpdateAcceptedDate()
		{
			var header = Factory.New<CusISFHeader>();
			IImporterSecurityFiling isf = header;
			var date1 = new ZDateTime(2009, 10, 10, 14, 30, 34);
			isf.UpdateAcceptedDate(date1);
			AssertEquals(date1, header.BF_FirstAcceptedDate);
			AssertEquals(date1, header.BF_LastAcceptedDate);
			var date2 = new ZDateTime(2009, 10, 15, 10, 25, 45);
			isf.UpdateAcceptedDate(date2);
			AssertEquals(date1, header.BF_FirstAcceptedDate);
			AssertEquals(date2, header.BF_LastAcceptedDate);
		}

		public void TestIImporterSecurityFilingReferenceData()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			IImporterSecurityFiling isf = header;
			CusISFBill bill1 = header.ReferenceDatas.AddNew();
			bill1.BB_BillType = BillTypeList.Codes.BondReferenceNumber;
			bill1.BB_BillNum = BillTypeList.Codes.BondReferenceNumber + "123";
			CusISFBill bill2 = header.ReferenceDatas.AddNew();
			bill2.BB_BillType = BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber;
			bill2.BB_BillNum = BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber + "123";
			CusISFBill bill3 = header.ReferenceDatas.AddNew();
			bill3.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			bill3.BB_BillNum = BillTypeList.Codes.MasterBillOfLading + "123";
			CusISFBill bill4 = header.ReferenceDatas.AddNew();
			bill4.BB_BillType = BillTypeList.Codes.SuretyCode;
			bill4.BB_BillNum = BillTypeList.Codes.SuretyCode + "123";
			CusISFBill bill5 = header.ReferenceDatas.AddNew();
			bill5.BB_BillType = BillTypeList.Codes.USCBPEntryNumber;
			bill5.BB_BillNum = BillTypeList.Codes.USCBPEntryNumber + "123";
			CusISFBill bill6 = header.ReferenceDatas.AddNew();
			bill6.BB_BillType = BillTypeList.Codes.UserDefinedReferenceNumber;
			bill6.BB_BillNum = BillTypeList.Codes.UserDefinedReferenceNumber + "123";
			List<IReferenceData> list = new List<IReferenceData>(isf.ReferenceData);
			AssertEquals(6, list.Count);
			AssertIReferenceData(list[0], ReferenceDataCodeList.Codes.BondReferenceNumber, BillTypeList.Codes.BondReferenceNumber + "123");
			AssertIReferenceData(list[1], ReferenceDataCodeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber, BillTypeList.Codes.CarnetIssuingCountryCodeAndCarnetNumber + "123");
			AssertIReferenceData(list[2], ReferenceDataCodeList.Codes.MasterBillOfLading, BillTypeList.Codes.MasterBillOfLading + "123");
			AssertIReferenceData(list[3], ReferenceDataCodeList.Codes.SuretyCode, BillTypeList.Codes.SuretyCode + "123");
			AssertIReferenceData(list[4], ReferenceDataCodeList.Codes.USCBPEntryNumber, BillTypeList.Codes.USCBPEntryNumber + "123");
			AssertIReferenceData(list[5], ReferenceDataCodeList.Codes.UserDefinedReferenceNumber, BillTypeList.Codes.UserDefinedReferenceNumber + "123");
		}

		public void TestShouldSendAdd()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			AssertEquals(true, header.ShouldSendAdd);
			header.BF_CustomsReference = "TEST";
			AssertEquals(false, header.ShouldSendAdd);
		}

		public void TestPopulateJobReferenceIfNeeded()
		{
			CargoWise.Data.Db.Connection.BeginTransaction(); // This is a test that needs to access a number fountain
			try
			{
				CusISFHeader header = Factory.New<CusISFHeader>();
				AssertEquals(ZString.Empty, header.BF_JobReference);
				ZString jobReference = Env.NumberFountains.ImporterSecurityFilingReference.PeekPreliminaryFormatted(Factory);
				header.BF_JobReference = "BSZSSD23423";
				header.PopulateJobReferenceIfNeeded();
				AssertEquals("BSZSSD23423", header.BF_JobReference);
				header.BF_JobReference = ZString.Empty;
				header.PopulateJobReferenceIfNeeded();
				AssertEquals(jobReference, header.BF_JobReference);
			}
			finally
			{
				CargoWise.Data.Db.Connection.RollbackTransaction(); // This is a test that needs to access a number fountain
			}
		}

		public void TestPopulateSingleDeclarationJobNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_HouseBill = "12345678";
			declaration.JE_HouseBillIssuerSCAC = "APLU";
			var header = Factory.New<CusISFHeader>();
			header.PopulateJobReferenceIfNeeded();
			AssertEquals(ZString.Empty, header.DeclarationJobNumber);
			Factory.Save();
			header.BF_HouseBill = "APLU12345678";
			AssertEquals(declaration.JobNumber, header.DeclarationJobNumber);
		}

		public void TestPopulateMultipleDeclarationJobNumber()
		{
			var jobDeclarationNumbers = new List<JobDeclaration>();
			for (int i = 0; i < 4; i++)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				declaration.JE_HouseBill = "12345678";
				declaration.JE_HouseBillIssuerSCAC = "APLU";
				jobDeclarationNumbers.Add(declaration);
			}

			var header = Factory.New<CusISFHeader>();
			header.PopulateJobReferenceIfNeeded();
			AssertEquals(ZString.Empty, header.DeclarationJobNumber);
			Factory.Save();
			var bill = header.BF_HouseBill = "APLU12345678";
			AssertEquals(4, jobDeclarationNumbers.Select(jobDeclaration => jobDeclaration.JobNumber).Distinct().Count());
			AssertContainsExactElementsInAnyOrder(jobDeclarationNumbers.Select(jobDeclaration => jobDeclaration.JobNumber), header.DeclarationJobNumber.Split(','));
		}

		public void TestBF_JobReferenceIsNotAssignedIfSavedFailed()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusISFHeader header1 = Factory.New<CusISFHeader>();
			header1.BF_SCAC = "ABCD";
			CusISFLine line1 = Factory.New<DummyCusISFLine_TestBF_CusISFHeaderTest>();
			line1.BL_BF = header1.PK;
			line1.BL_TextProductCode = "ABC";
			CusISFHeader header2 = factory2.New<CusISFHeader>();
			header2.BF_SCAC = "ABCD";
			CusISFLine line2 = header1.Lines.AddNew();
			line2.BL_TextProductCode = "ABC";
			AssertExceptionThrown(typeof(Exception), delegate
			{
				try
				{
					Factory.Save();
				}
				finally
				{
					AssertEquals("", header1.BF_JobReference);
				}
			});
			AssertNoExceptionThrown(delegate
			{
				try
				{
					factory2.Save();
				}
				finally
				{
					AssertNotEquals("", header2.BF_JobReference);
				}
			});
		}

		public void TestBF_CustomsStatusIsRolledBackIfSaveFailed()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_CustomsStatus = "CIO";
			Factory.Save();
			header.BF_CustomsStatus = "CIR";
			var line = Factory.New<DummyCusISFLine_TestBF_CusISFHeaderTest>();
			line.BL_BF = header.PK;
			AssertExceptionThrown(typeof(Exception), delegate
			{
				try
				{
					Factory.Save();
				}
				finally
				{
					AssertEquals("On save fail, BF_CustomsStatus should be reverted.", "CIO", header.BF_CustomsStatus);
				}
			});
		}

		public void TestIRoutingSupportMembers()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			IRoutingSupport routingSupport = header;
			AssertEquals("Transports", header.Transports, routingSupport.Transports);
			AssertEquals("Transport Mode", Core.Constants.TransportModes.Sea, routingSupport.TransportMode);
		}

		public void TestHumanReadableName()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			AssertEquals("", header.HumanReadableName);
			header.BF_JobReference = "BF234322";
			AssertEquals("BF234322", header.HumanReadableName);
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "ZZZ2234ZZZ";
			header.BF_OH_Importer = org.PK;
			AssertEquals("BF234322 (Importer='ZZZ2234ZZZ')", header.HumanReadableName);
			header.BF_HouseBill = "HB23432";
			header.BF_OceanBill = "OB23432";
			AssertEquals("BF234322 (Importer='ZZZ2234ZZZ', OceanBill='OB23432')", header.HumanReadableName);
			header.BF_OceanBill = ZString.Empty;
			AssertEquals("BF234322 (Importer='ZZZ2234ZZZ', HouseBill='HB23432')", header.HumanReadableName);
		}

		public void TestUpdateBondDetailsFromImporterIfNeeded()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(org);
			US.Business.CusBondDetailCollection bondDetails = wrapper.BondDetails;
			US.Business.CusBondDetail bondData = bondDetails.AddNew();
			bondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData.PW_SuretyCode = "791";
			bondData.PW_BondEffectiveDate = new ZDateTime(2007, 1, 1);
			bondData.PW_BondExpiryDate = ZDateTime.Today.AddDays(-1);
			US.Business.CusBondDetail bondData2 = bondDetails.AddNew();
			bondData2.PW_ActivityCode = ActivityCodeList.Codes._2;
			bondData2.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondData2.PW_SuretyCode = "786";
			bondData2.PW_BondEffectiveDate = new ZDateTime(2007, 2, 1);
			bondData2.PW_BondExpiryDate = ZDateTime.Today.AddMonths(2);
			US.Business.CusBondDetail bondData3 = bondDetails.AddNew();
			bondData3.PW_ActivityCode = ActivityCodeList.Codes._3;
			bondData3.PW_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			bondData3.PW_SuretyCode = "968";
			bondData3.PW_BondEffectiveDate = new ZDateTime(2007, 3, 1);
			bondData3.PW_BondExpiryDate = ZDateTime.Today.AddDays(2);
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_ShipmentType = ShipmentTypeList.Codes.FTZShipments;
			header.BF_OH_Importer = org.PK;
			AssertEquals(ISFBondActivityCodeList.Codes.InternationalCarrier, header.BF_BondActivityCode);
			AssertEquals(ImporterBondTypeList.Codes.SingleTransactionBond, header.BF_BondType);
			AssertEquals("", header.BF_SuretyCode);
			bondData3.PW_ActivityCode = ActivityCodeList.Codes._1;
			header = Factory.New<CusISFHeader>();
			header.BF_ShipmentType = ShipmentTypeList.Codes.FTZShipments;
			header.BF_OH_Importer = org.PK;
			AssertEquals(ISFBondActivityCodeList.Codes.ImporterOrBroker, header.BF_BondActivityCode);
			AssertEquals(ImporterBondTypeList.Codes.SingleTransactionBond, header.BF_BondType);
			AssertEquals("", header.BF_SuretyCode);
			bondData3.PW_ActivityCode = ActivityCodeList.Codes._16;
			header = Factory.New<CusISFHeader>();
			header.BF_ShipmentType = ShipmentTypeList.Codes.FTZShipments;
			header.BF_OH_Importer = org.PK;
			AssertEquals(ISFBondActivityCodeList.Codes.ISFBond16, header.BF_BondActivityCode);
			AssertEquals(ImporterBondTypeList.Codes.SingleTransactionBond, header.BF_BondType);
			AssertEquals("968", header.BF_SuretyCode);
			bondData3.PW_ActivityCode = ActivityCodeList.Codes._1;
			US.Business.CusBondDetail bondData4 = bondDetails.AddNew();
			bondData4.PW_ActivityCode = ActivityCodeList.Codes._16;
			bondData4.PW_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			bondData4.PW_SuretyCode = "863";
			bondData4.PW_BondEffectiveDate = new ZDateTime(2006, 3, 1);
			bondData4.PW_BondExpiryDate = ZDateTime.Today;
			header = Factory.New<CusISFHeader>();
			header.BF_ShipmentType = ShipmentTypeList.Codes.FTZShipments;
			header.BF_OH_Importer = org.PK;
			AssertEquals(ISFBondActivityCodeList.Codes.ISFBond16, header.BF_BondActivityCode);
			AssertEquals(ImporterBondTypeList.Codes.SingleTransactionBond, header.BF_BondType);
			AssertEquals("863", header.BF_SuretyCode);
			header.BF_ShipmentType = ZString.Empty;
			AssertEquals("", header.BF_BondActivityCode);
			AssertEquals("", header.BF_BondType);
			AssertEquals("", header.BF_SuretyCode);
		}

		public void TestBuyingPartyDefaultImporter()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BuyingParty.OrganisationPK = org1.PK;
			AssertEquals(org1.PK, header.BF_OH_Importer);
			header.BuyingParty.OrganisationPK = org2.PK;
			AssertEquals(org1.PK, header.BF_OH_Importer);
			header.BuyingParty.OrganisationPK = ZGuid.Empty;
			AssertEquals(org1.PK, header.BF_OH_Importer);
		}

		public void TestBranchReadOnly()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			AssertEquals(false, header.BF_GBInfo.ReadOnly);
			header.BF_CustomsReference = "SD";
			AssertEquals(true, header.BF_GBInfo.ReadOnly);
			header.BF_CustomsReference = ZString.Empty;
			AssertEquals(false, header.BF_GBInfo.ReadOnly);
			header.BF_CustomsStatus = "Z";
			AssertEquals(true, header.BF_GBInfo.ReadOnly);
			header.BF_CustomsStatus = MessageStatusList.Codes.NotSentISF;
			AssertEquals(false, header.BF_GBInfo.ReadOnly);
			var message = Factory.New<EDIMessage>();
			header.Messages.Add(message);
			AssertEquals(true, header.BF_GBInfo.ReadOnly);
			message.EM_Status = EDIMessage.Status.Discarded;
			AssertEquals(false, header.BF_GBInfo.ReadOnly);
		}

		public void TestBF_BondReferenceNumber()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_BondReferenceNumber = "BF1231";
			CusISFBill bondReferenceNumber = header.BondReferenceNumber;
			AssertEquals(BillTypeList.Codes.BondReferenceNumber, bondReferenceNumber.BB_BillType);
			AssertEquals("BF1231", bondReferenceNumber.BB_BillNum);
			AssertEquals("BF1231", header.BF_BondReferenceNumber);
			bondReferenceNumber.Delete();
			AssertEquals(ZString.Empty, header.BF_BondReferenceNumber);
			AssertNotEquals(bondReferenceNumber, header.BondReferenceNumber);
		}

		[TestDate(2007, 02, 05)]
		[RunInExtraTransaction]
		public void TestPopulateJobReferenceUsingCustomisation()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			ZString nextJobNumber = Env.NumberFountains.ImporterSecurityFilingReference.PeekPreliminaryFormatted(Factory);
			header.OnSaving();
			AssertEquals("BF_JobReference", nextJobNumber, header.BF_JobReference);
			header.OnSaving();
			AssertEquals("BF_JobReference", nextJobNumber, header.BF_JobReference);
			var newCustomisation = new ISFNumberCustomisation();
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, 1).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 2).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
			ISFRegistry.Instance.ImporterSecurityFilingNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCustomisation);
			header.BF_JobReference = "";
			header.OnSaving();
			AssertEquals("BF_JobReference", "ISF7B001", header.BF_JobReference);
			header.OnSaving();
			AssertEquals("BF_JobReference", "ISF7B001", header.BF_JobReference);
			header.BF_JobReference = "";
			header.OnSaving();
			AssertEquals("BF_JobReference", "ISF7B002", header.BF_JobReference);
			header.OnSaving();
			AssertEquals("BF_JobReference", "ISF7B002", header.BF_JobReference);
			newCustomisation = new ISFNumberCustomisation();
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.MonthAsLetter, 1).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsLetter, 2).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
			ISFRegistry.Instance.ImporterSecurityFilingNumberCustomisation.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, newCustomisation);
			header.BF_JobReference = "";
			header.OnSaving();
			AssertEquals("BF_JobReference", "ISFBG001", header.BF_JobReference);
			header.OnSaving();
			AssertEquals("BF_JobReference", "ISFBG001", header.BF_JobReference);
			var company2 = Factory.New<GlbCompany>();
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = company2.PK;
			branch2.GB_Code = "ZZ2";
			newCustomisation = new ISFNumberCustomisation();
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.BranchCode, 1).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsLetter, 2).Fountain = true;
			SetElement(newCustomisation, BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, 50, "3");
			ISFRegistry.Instance.ImporterSecurityFilingNumberCustomisation.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, newCustomisation);
			header.BF_GB = branch2.PK;
			header.BF_JobReference = "";
			header.OnSaving();
			AssertEquals("BF_JobReference", "ISFZZ2G001", header.BF_JobReference);
			header.OnSaving();
			AssertEquals("BF_JobReference", "ISFZZ2G001", header.BF_JobReference);
		}

		public void TestIBillGenerationSupportMembers()
		{
			RefUNLOCO uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_RL_NKPlaceOfDelivery = uslax.Code;
			IBillGenerationSupport billGenerator = header;
			AssertNull(billGenerator.CarrierPrincipal);
			AssertEquals("", billGenerator.TranshipmentIndicator);
			AssertNull(billGenerator.Destination);
			AssertEquals(Factory, billGenerator.Factory);
			AssertNull(billGenerator.Origin);
			AssertEquals(Core.Constants.TransportModes.Sea, billGenerator.TransportMode);
			AssertNull("Load should be NULL", billGenerator.Load);
			AssertNull("Discharge should be NULL", billGenerator.Discharge);
		}

		public void TestFirstUSTransport()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			AssertNull(header.FirstUSTransport);
			Transport transport1 = header.Transports.AddNew();
			transport1.JW_ETA = new ZDateTime(2009, 2, 28);
			Transport transport2 = header.Transports.AddNew();
			transport2.JW_ETA = new ZDateTime(2009, 1, 30);
			Transport transport3 = header.Transports.AddNew();
			transport3.JW_ETA = new ZDateTime(2009, 3, 30);
			AssertEquals(transport2, header.FirstUSTransport);
			transport2.JW_RL_NKDiscPort = "AUSYD";
			AssertEquals(transport1, header.FirstUSTransport);
			transport1.JW_RL_NKDiscPort = "NZAKL";
			AssertEquals(transport3, header.FirstUSTransport);
			transport3.JW_RL_NKDiscPort = "GBLON";
			AssertNull(header.FirstUSTransport);
			transport3.JW_RL_NKDiscPort = "USLAX";
			AssertEquals(transport3, header.FirstUSTransport);
			transport1.JW_RL_NKDiscPort = "USCHI";
			AssertEquals(transport1, header.FirstUSTransport);
			transport2.JW_RL_NKDiscPort = "US";
			AssertEquals(transport2, header.FirstUSTransport);
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "PRSWE";
			transport3.JW_RL_NKDiscPort = "USLAX";
			AssertEquals(transport2, header.FirstUSTransport);
		}

		public void TestFirstUSTransport_CS00160697()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			Transport transport1 = header.Transports.AddNew();
			transport1.JW_ETA = new ZDateTime(2009, 2, 28);
			transport1.JW_IsLinked = true;
			transport1.JW_RL_NKDiscPort = "USCHI";
			Transport transport2 = header.Transports.AddNew();
			transport2.JW_ETA = new ZDateTime(2009, 3, 15);
			transport2.JW_IsLinked = false;
			transport2.JW_RL_NKDiscPort = "USLAX";
			AssertEquals(transport1, header.FirstUSTransport);
		}

		public void TestImporterDetailsClearance()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_ImporterFullName = "SMITH, BOB";
			header.BF_DateOfBirth = ZDate.BrettsBirthday;
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.Australia;
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.SocialSecurity;
			AssertImporterDetails(header, "SMITH, BOB", ZDate.BrettsBirthday, Core.Constants.CountryCodes.Australia);
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.Passport;
			AssertImporterDetails(header, "SMITH, BOB", ZDate.BrettsBirthday, Core.Constants.CountryCodes.Australia);
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			AssertImporterDetails(header, ZString.Empty, ZDate.Empty, ZString.Empty);
		}

		public void TestUpdateConsigneeDetailsFromImporter()
		{
			var header = Factory.New<CusISFHeader>();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.Passport;
			header.BF_ImporterCode = "L23432K32";
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.Australia;
			header.BF_DateOfBirth = new ZDateTime(1980, 1, 2);
			AssertConsigneeDetails(header, "L23432K32", ConsigneeCodeTypeList.Codes.Passport, Core.Constants.CountryCodes.Australia, new ZDateTime(1980, 1, 2));
			header.BF_DateOfBirth = new ZDateTime(1980, 1, 3);
			AssertConsigneeDetails(header, "L23432K32", ConsigneeCodeTypeList.Codes.Passport, Core.Constants.CountryCodes.Australia, new ZDateTime(1980, 1, 3));
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.NewZealand;
			AssertConsigneeDetails(header, "L23432K32", ConsigneeCodeTypeList.Codes.Passport, Core.Constants.CountryCodes.NewZealand, new ZDateTime(1980, 1, 3));
			header.BF_ConsigneeCode = "G23432K32";
			AssertConsigneeDetails(header, "G23432K32", ConsigneeCodeTypeList.Codes.Passport, Core.Constants.CountryCodes.NewZealand, new ZDateTime(1980, 1, 3));
			header.BF_DateOfBirth = new ZDateTime(1980, 1, 2);
			AssertConsigneeDetails(header, "G23432K32", ConsigneeCodeTypeList.Codes.Passport, Core.Constants.CountryCodes.NewZealand, new ZDateTime(1980, 1, 3));
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.Australia;
			AssertConsigneeDetails(header, "G23432K32", ConsigneeCodeTypeList.Codes.Passport, Core.Constants.CountryCodes.NewZealand, new ZDateTime(1980, 1, 3));
			header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.IRS;
			header.BF_ConsigneeCode = "G23432K32";
			AssertConsigneeDetails(header, "G23432K32", ConsigneeCodeTypeList.Codes.IRS, ZString.Empty, ZDateTime.Empty);

			header.BF_ImporterCodeType = ZString.Empty;
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.Passport;
			header.BF_ImporterCode = "K23432K32";
			header.BF_CountryOfIssue = Core.Constants.CountryCodes.Australia;
			header.BF_DateOfBirth = new ZDateTime(1980, 1, 2);
			AssertConsigneeDetails(header, "K23432K32", ConsigneeCodeTypeList.Codes.Passport, Core.Constants.CountryCodes.Australia, new ZDateTime(1980, 1, 2));
		}

		public void TestNoteTypes()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			Assert(PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.SpecialInstructions, header.NoteTypes));
			Assert(PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.ClientVisibleJobNotes, header.NoteTypes));
			Assert(PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.InternalWorkNotes, header.NoteTypes));
			Assert(PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog, header.NoteTypes));
			Assert(PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.AutoRatingAuditLog, header.NoteTypes));
			Assert(PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.UnmatchedOrgDetails, header.NoteTypes));
		}

		public void TestBranch()
		{
			var header = Factory.New<CusISFHeader>();
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			header.BF_GB = branch.PK;
			AssertEquals(branch, header.Branch);
		}

		public void TestIUSDISHostImplement()
		{
			var header = Factory.New<CusISFHeader>();
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			header.BF_EntryNumber = "1";
			header.BF_ShipmentType = ShipmentTypeList.Codes.StandardOrRegularFilings;
			header.BF_TransportMode = TransportModeCodes.Codes.OceanVesselContainerized;
			header.BF_GB = branch.PK;
			header.BF_OH_Importer = ZGuid.Empty;
			header.BF_ActionReasonCode = ActionReasonCodeList.Codes.CompliantTransaction;
			header.BF_ImporterCodeType = ImporterCodeTypeList.Codes.IRS;
			header.BF_ImporterCode = "45-985412300";
			header.BF_ImporterFullName = "ImporterName";
			header.BF_ConsigneeCodeType = ConsigneeCodeTypeList.Codes.IRS;
			header.BF_ConsigneeCode = "45-985412300";
			header.BF_BondNumberOrHolder = "45-985412300";
			header.BF_BondActivityCode = ISFBondActivityCodeList.Codes.ImporterOrBroker;
			header.BF_BondType = BondTypeList.Codes.ContinuousBond;
			var disHost = (IUSDISHost)header;
			AssertEquals("ImporterName", disHost.ImporterName);
			AssertEquals(Core.Constants.Customs.DocumentImageSystemIDs.US_DIS, disHost.ApplicationCodes.First());
			AssertEquals(true, disHost.ShowDISFeatures);
			AssertEquals(header.BF_JobReference, disHost.JobNumber);
			AssertEquals(ZString.Empty, disHost.MessageSendingWarning);
			AssertEquals(CusISFHeader.NoCustomsRefAvailable, disHost.MessageSendingError);
			AssertEquals(branch.PK, disHost.BranchPK);
			AssertEquals(company.PK, disHost.CompanyPK);
		}

		public void TestJobHeaderIsNotDeactivatedWhenFactoryHasInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(true);
		}

		public void TestJobHeaderIsDeactivatedWhenFactoryDoesNotHaveInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(false);
		}

		public void TestIJobInvoicingPlugIn_DefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<CusISFHeader>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent cusISFHeader = Factory.New<CusISFHeader>();
			Assert(cusISFHeader.AllowInvoiceDeletion);
		}

		public void TestIValidateForCustomsMessagingSupporter()
		{
			var header = Factory.New<CusISFHeader>();
			var supporter = (IValidateForCustomsMessagingSupporter)header;
			AssertEquals(true, supporter.SupportValidateCustomsMessaging);
			AssertEquals(header, supporter.GetEntityToValidate(""));
		}

		protected override Type ExpectedMetadataType => typeof(Metadata.Business.USCusISFHeader);

		EDICommunicationsMode CommunicationsMode
		{
			get
			{
				var communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
				communicationsMode.EK_Destination = "test@test.com";
				communicationsMode.EK_Filename = "test_filename";
				communicationsMode.EK_ServerAddressSubject = "subbjectt-(*HelloWorld*)";
				return communicationsMode;
			}
		}

		CusISFBill CreateBill(CusISFHeader header, ZString billType, ZString number, ZString customsStatus)
		{
			var bill = header.ReferenceDatas.AddNew();
			bill.BB_BillType = billType;
			bill.BB_BillNum = number;
			bill.BB_CustomsStatus = customsStatus;
			return bill;
		}

		TariffData GetTariffData(CusISFLine line)
		{
			return new TariffData()
			{ CountryOfOrigin = line.BL_RN_NKGoodsOrigin, HarmonizedTariffNumber = line.HarmonisedNumToReportToCustoms };
		}

		void AssertManufacturerData(IManufacturerData manufacturerData, JobDocAddress manufacturer, List<TariffData> list)
		{
			if (manufacturer != null)
			{
				AssertEquals(manufacturer, ((SanitizedISFDocAddressWrapper)manufacturerData.Manufacturer).DocAddress);
			}
			else
			{
				AssertNull(manufacturerData.Manufacturer);
			}

			AssertContainsExactElementsInAnyOrder(manufacturerData.Tariffs, list);
		}

		void AssertIReferenceData(IReferenceData referenceData, string codeQualifier, string data)
		{
			AssertEquals(codeQualifier, referenceData.CodeQualifier);
			AssertEquals(data, referenceData.ReferenceData);
		}

		BillOfLadingNumberCustomisationElement SetElement(BillOfLadingNumberCustomisation customisation, string key, byte order)
		{
			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			return element;
		}

		BillOfLadingNumberCustomisationElement SetElement(BillOfLadingNumberCustomisation customisation, string key, byte order, string detail)
		{
			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[key];
			element.Include = true;
			element.Order = order;
			element.Detail = detail;
			return element;
		}

		void AssertImporterDetails(CusISFHeader header, ZString name, ZDateTime dateOfBirth, ZString countryOfIssue)
		{
			AssertEquals(name, header.BF_ImporterFullName);
			AssertEquals(dateOfBirth, header.BF_DateOfBirth);
			AssertEquals(countryOfIssue, header.BF_CountryOfIssue);
		}

		void AssertConsigneeDetails(CusISFHeader header, ZString consigneeCode, ZString consigneeCodeType, ZString countryOfIssue, ZDateTime dateOfBirth)
		{
			AssertEquals("Code", consigneeCode, header.BF_ConsigneeCode);
			AssertEquals("Code Type", consigneeCodeType, header.BF_ConsigneeCodeType);
			AssertEquals("Country Of Issue", countryOfIssue, header.BF_ConsigneeCountryOfIssue);
			AssertEquals("Date Of Birth", dateOfBirth, header.BF_ConsigneeDateOfBirth);
		}

		bool PredefinedNoteTypeExist(PredefinedNoteType noteTypeToCheck, NoteTypeCollection noteTypes)
		{
			foreach (PredefinedNoteType noteType in noteTypes)
			{
				if (noteTypeToCheck == noteType)
				{
					return true;
				}
			}

			return false;
		}

		void AssertJobHeaderDeactivation(bool hasInvoicingPlugInGUIContext)
		{
			var iSFHeader = Factory.New<CusISFHeader>();
			var jobLoader = new JobHeader.Loader(iSFHeader);
			var job = jobLoader.TryCreate();
			Factory.Save();
			iSFHeader.IsCancelled = true;
			if (hasInvoicingPlugInGUIContext)
			{
				Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			}

			var assertionMessage1 = string.Format("ISFHeader {0} have InvoicingPluginGUI Business Context", hasInvoicingPlugInGUIContext ? "should" : "should not");
			var assertionMessage2 = string.Format("Job {0} be deactivated by OnSaving method", hasInvoicingPlugInGUIContext ? "should not" : "should");
			Assert("Deactivating ISFHeader, IsCancelled flag should be set to true", iSFHeader.IsCancelled);
			Assert("Deactivating ISFHeader, IsCancelledInfo should have changes", iSFHeader.IsCancelledHasChanged);
			AssertEquals(assertionMessage1, hasInvoicingPlugInGUIContext, iSFHeader.HasContext(BusinessContext.InvoicingPlugInGUI));
			Assert("Job is not yet deactivated", !job.IsCancelled);
			Factory.Save();
			AssertEquals(assertionMessage2, !hasInvoicingPlugInGUIContext, job.IsCancelled);
		}
	}

	public class DummyCusISFLine_TestBF_CusISFHeaderTest : CusISFLine
	{
		public DummyCusISFLine_TestBF_CusISFHeaderTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override void OnSaving()
		{
			throw new Exception("Blah");
		}
	}
}
