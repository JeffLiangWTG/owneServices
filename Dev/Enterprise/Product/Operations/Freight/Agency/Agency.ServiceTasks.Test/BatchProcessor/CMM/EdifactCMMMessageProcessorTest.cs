using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.DataTransfer.MessageProcessing;
using Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	[TestedType(typeof(EdifactCMMMessageProcessor))]
	sealed class EdifactCMMMessageProcessorTest : CMMMessageProcessorTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestProcessCODECOMessage_Fail()
		{
			#region MessageText
			string messageText = "Do you feel inadiquite?\r\n" + "Have you tried the little blue pill and failed?\r\n" + "Well now there is a little green pill for thoes who want to colour coordinate.\r\n" + "";
			#endregion
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "imp@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);
			Processor.ProcessMessage(message);
			AssertEquals("EM_LinkUniqueID", ZGuid.Empty, message.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", ZString.Empty, message.EM_LinkTable);
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
			var expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Error Processing CODECO/COARRI Message\r\n" + "CC:\r\n" + "  err@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var email = FindEmail((p) => p.CCRecipients.Contains("err@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.GetCMMErrorResultBody(), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestProcessCODECOMessage_Ack()
		{
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+14+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4214635";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9290127";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0033";
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("EM_LinkTable", "JobContainerMove", message.EM_LinkTable);
			var movement = Factory.Load<ContainerMovement>(message.EM_LinkUniqueID);
			AssertEquals("movement type", ContainerMovementTypes.Codes.DepotGateIn, movement.E9_MovementType);
			AssertEquals("voyage", voyage.PK, movement.E9_JV);
			AssertEquals("is empty", false, movement.E9_ContainerIsEmpty);
			AssertEquals("movement date", new ZDateTime(2008, 06, 27, 10, 10, 00), movement.E9_MovementDate);
			AssertEquals("depot", sender.MainAddress.PK, movement.E9_OA_Depot);
			AssertEquals("movement stock", stock.PK, movement.E9_R6);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers Without Warnings\r\n" + "CC:\r\n" + "  ack@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("ack@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody1(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		public override void TestProcessCODECOMessage_Warning()
		{
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+22G0:102:5+++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+14+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4214635";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9290127";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0033";
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("EM_LinkTable", "JobContainerMove", message.EM_LinkTable);
			var movement = Factory.Load<ContainerMovement>(message.EM_LinkUniqueID);
			AssertEquals("movement type", ContainerMovementTypes.Codes.DepotGateIn, movement.E9_MovementType);
			AssertEquals("voyage", voyage.PK, movement.E9_JV);
			AssertEquals("is empty", false, movement.E9_ContainerIsEmpty);
			AssertEquals("movement date", new ZDateTime(2008, 06, 27, 10, 10, 00), movement.E9_MovementDate);
			AssertEquals("depot", sender.MainAddress.PK, movement.E9_OA_Depot);
			AssertEquals("movement stock", stock.PK, movement.E9_R6);
			const string expectedEmailDef = @"FROM:
  default@wisetechglobal.com
SUBJECT:
  Processed Gate-In CODECO Message From Random Depot - Containers With Warnings
CC:
  dsc@freadnet.org
";
			AssertMultilineASCIIEquals("email", expectedEmailDef, string.Join(System.Environment.NewLine, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail)));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_DefaultOwnerTypeOnNew()
		{
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "NAD+MS+SHCB1:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+1++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+12+1'" + "";
			#endregion
			AgencyRegistry.Instance.CMMCreateMissingContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("EM_LinkTable", "JobContainerMove", message.EM_LinkTable);
			var movement = Factory.Load<ContainerMovement>(message.EM_LinkUniqueID);
			var stock = movement.Stock;
			AssertNotNull("movement stock", stock);
			AssertEquals("owner type", Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned, stock.R6_OwnerType);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody4(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_DefaultOwnerTypeOnUpdate()
		{
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "NAD+MS+SHCB1:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+2++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+12+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4214635";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			stock.R6_OwnerType = "";
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("EM_LinkTable", "JobContainerMove", message.EM_LinkTable);
			var movement = Factory.Load<ContainerMovement>(message.EM_LinkUniqueID);
			AssertEquals("movement stock", stock.PK, movement.E9_R6);
			AssertEquals("owner type", Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned, stock.R6_OwnerType);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers Without Warnings\r\n" + "CC:\r\n" + "  ack@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("ack@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody2(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_DontOverwriteOwnerType()
		{
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "NAD+MS+SHCB1:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+2++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+12+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4214635";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			stock.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.Leased;
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("EM_LinkTable", "JobContainerMove", message.EM_LinkTable);
			var movement = Factory.Load<ContainerMovement>(message.EM_LinkUniqueID);
			AssertEquals("movement stock", stock.PK, movement.E9_R6);
			AssertEquals("owner type", Enterprise.Core.Constants.ContainerOwnership.Codes.Leased, stock.R6_OwnerType);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers Without Warnings\r\n" + "CC:\r\n" + "  ack@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("ack@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody2(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_DefaultVoyageFromShipment()
		{
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+++5'" + "RFF+BM:SUPERMAGICPOWERCODE'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+14+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_RL_NKClosestPort = "AUBNE";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4214635";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9290127";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0033";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			var bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = "V00000100";
			bill.JS_HouseBill = "SUPERMAGICPOWERCODE";
			bill.JS_JX = voyage.Sailings[0].PK;
			var container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = "CCLU4214635";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var countryProcessorMock = new Mock<ICMMCountryProcessor>();

			var processorFactory = processorFactoryMock.Object;
			var countryProcessor = countryProcessorMock.Object;

			processorFactoryMock
				.Setup(m => m.Invoke(It.IsNotNull<CMMEmailGenerator>(), It.IsNotNull<ICMMProcessingAdapter>()))
				.Returns(countryProcessor);

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			countryProcessorMock
				.Verify(m => m.ValidateContainer(
					It.IsNotNull<CMMMessageContainer>(),
					It.Is<AgencyShipmentContainer>((c) => c.PK == container.PK)));

			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("EM_LinkTable", "JobContainerMove", message.EM_LinkTable);
			var movement = Factory.Load<ContainerMovement>(message.EM_LinkUniqueID);
			AssertEquals("movement type", ContainerMovementTypes.Codes.DepotGateIn, movement.E9_MovementType);
			AssertEquals("voyage", voyage.PK, movement.E9_JV);
			AssertEquals("is empty", false, movement.E9_ContainerIsEmpty);
			AssertEquals("movement date", new ZDateTime(2008, 06, 27, 10, 10, 00), movement.E9_MovementDate);
			AssertEquals("depot", sender.MainAddress.PK, movement.E9_OA_Depot);
			AssertEquals("movement stock", stock.PK, movement.E9_R6);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody8(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_DefaultVoyageFromEarlierMovement()
		{
			AgencyRegistry.Instance.CMMDefaultVoyageFromMovement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+14+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_RL_NKClosestPort = "AUBNE";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4214635";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9290127";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0033";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			var prevMovement = stock.Movements.AddNew();
			prevMovement.E9_MovementDate = new ZDateTime(2008, 6, 26, 13, 00, 0);
			prevMovement.E9_MovementType = ContainerMovementTypes.Codes.Discharge;
			prevMovement.E9_JV = voyage.PK;
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("EM_LinkTable", "JobContainerMove", message.EM_LinkTable);
			var movement = Factory.Load<ContainerMovement>(message.EM_LinkUniqueID);
			AssertEquals("movement type", ContainerMovementTypes.Codes.DepotGateIn, movement.E9_MovementType);
			AssertEquals("voyage", voyage.PK, movement.E9_JV);
			AssertEquals("is empty", false, movement.E9_ContainerIsEmpty);
			AssertEquals("movement date", new ZDateTime(2008, 06, 27, 10, 10, 00), movement.E9_MovementDate);
			AssertEquals("depot", sender.MainAddress.PK, movement.E9_OA_Depot);
			AssertEquals("movement stock", stock.PK, movement.E9_R6);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody9(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_ValidateVoyageAgainstEarlierMovement()
		{
			AgencyRegistry.Instance.CMMDefaultVoyageFromMovement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+15+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_RL_NKClosestPort = "AUBNE";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4214635";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Name = "CSCL ZEEBRUGGE";
			vessel1.RV_LloydsNumber = "9314234";
			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage1.JV_VoyageFlight = "0032";
			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "CSCL MELBOURNE";
			vessel2.RV_LloydsNumber = "9290127";
			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage2.JV_VoyageFlight = "0033";
			var prevMovement = stock.Movements.AddNew();
			prevMovement.E9_MovementDate = new ZDateTime(2008, 6, 26, 13, 00, 0);
			prevMovement.E9_MovementType = ContainerMovementTypes.Codes.Discharge;
			prevMovement.E9_JV = voyage1.PK;
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("EM_LinkTable", "JobContainerMove", message.EM_LinkTable);
			var movement = Factory.Load<ContainerMovement>(message.EM_LinkUniqueID);
			AssertEquals("movement type", ContainerMovementTypes.Codes.DepotGateIn, movement.E9_MovementType);
			AssertEquals("voyage", voyage2.PK, movement.E9_JV);
			AssertEquals("is empty", false, movement.E9_ContainerIsEmpty);
			AssertEquals("movement date", new ZDateTime(2008, 06, 27, 10, 10, 00), movement.E9_MovementDate);
			AssertEquals("depot", sender.MainAddress.PK, movement.E9_OA_Depot);
			AssertEquals("movement stock", stock.PK, movement.E9_R6);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody10(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_ReturnToWharf()
		{
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container 1
			"EQD+CN+CCLU4214635+42R0:102:5+++4'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // container 2
			"EQD+CN+CCLU4408380+42R0:102:5+++4'" + "DTM+7:200806261145:203'" + "LOC+165+AUSYD:139:6'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:2'" + "UNT+19+1'" + "";
			#endregion
			var movementDate1 = new ZDateTime(2008, 06, 27, 10, 10, 00);
			var movementDate2 = new ZDateTime(2008, 06, 26, 11, 45, 00);
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Wharf";
			sender.OH_IsSeaCTO = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "CCLU4214635";
			stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var prevMovement1a = stock1.Movements.AddNew();
			prevMovement1a.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			prevMovement1a.E9_MovementDate = movementDate1.AddDays(-2);
			var prevMovement1b = stock1.Movements.AddNew();
			prevMovement1b.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			prevMovement1b.E9_MovementDate = movementDate1.AddDays(-1);
			var stock2 = Factory.New<RefContainerStock>();
			stock2.R6_ContainerNum = "CCLU4408380";
			stock2.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var prevMovement2a = stock2.Movements.AddNew();
			prevMovement2a.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			prevMovement2a.E9_MovementDate = movementDate2.AddDays(-1);
			var prevMovement2b = stock2.Movements.AddNew();
			prevMovement2b.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			prevMovement2b.E9_MovementDate = movementDate2.AddDays(1);
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9290127";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0033";
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("Stock1.Movements.Count", 3, stock1.Movements.Count);
			AssertEquals("Stock2.Movements.Count", 3, stock2.Movements.Count);
			CombineAssertions(delegate
			{
				var movement1 = stock1.Movements[2];
				AssertEquals("Movement 1: Movement Type", ContainerMovementTypes.Codes.ReturnToWharf, movement1.E9_MovementType);
				AssertEquals("Movement 1: Voyage", voyage.PK, movement1.E9_JV);
				AssertEquals("Movement 1: Is Empty", true, movement1.E9_ContainerIsEmpty);
				AssertEquals("Movement 1: Movement Date", movementDate1, movement1.E9_MovementDate);
				AssertEquals("Movement 1: Depot", sender.MainAddress.PK, movement1.E9_OA_Depot);
				AssertEquals("Movement 1: Movement Stock", stock1.PK, movement1.E9_R6);
				var movement2 = stock2.Movements[2];
				AssertEquals("Movement 2: Movement Type", ContainerMovementTypes.Codes.WharfGateIn, movement2.E9_MovementType);
				AssertEquals("Movement 2: Voyage", voyage.PK, movement2.E9_JV);
				AssertEquals("Movement 2: Is Empty", true, movement2.E9_ContainerIsEmpty);
				AssertEquals("Movement 2: Movement Date", movementDate2, movement2.E9_MovementDate);
				AssertEquals("Movement 2: Depot", sender.MainAddress.PK, movement2.E9_OA_Depot);
				AssertEquals("Movement 2: Movement Stock", stock2.PK, movement2.E9_R6);
			});
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Wharf - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock1.PK.ToGuid());
			mappings["(*URL2*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock2.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody13(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestProcessCOARRIMessage_Dsc_NotRequireOrgs()
		{
			AgencyRegistry.Instance.CMMCreateMissingContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			#region messageText
			const string messageText = // headder
			"UNH+1881+COARRI:D:95B:UN:ITG10'" + "BGM+98+1881+9'" + "TDT+20+0084S+1++CSC:172:184+++9224336:146'" + "NAD+MS+AALME:160:184'" + "NAD+CF+ACEPOOL'" + // container 1
			"EQD+CN+CCLU4741410+42R0:102:5+++4'" + "RFF+BN:ACE TO CHINA'" + "DTM+7:200806260954:203'" + "LOC+165+AUMEL:139:6'" + "FTX+DAR++2'" + "FTX+ABS++50'" + "TDT+20++1++ACEPOOL:172:184+++:146'" + "NAD+CN+++ACS'" + // container 2
			"EQD+CN+CCLU4408380+0666:102:5+++4'" + "RFF+BN:ACE TO CHINA'" + "DTM+7:200806261145:203'" + "LOC+165+AUMEL:139:6'" + "FTX+DAR++2'" + "FTX+ABS++50'" + "TDT+20++1++ACEPOOL:172:184+++:146'" + "NAD+CN+++ACS'" + // container 3
			"EQD+CN+CCLU4595589+42R0:102:5+++4'" + "RFF+BN:BLATICUSX'" + "DTM+7:200806261525:203'" + "LOC+165+AUMEL:139:6'" + "FTX+DAR++2'" + "FTX+ABS++50'" + "TDT+20++1++ACEPOOL:172:184+++:146'" + "NAD+CN+++acs'" + // footer
			"CNT+16:3'" + "UNT+30+1881'" + "";
			#endregion
			var companyAU = Factory.New<GlbCompany>();
			companyAU.GC_Code = "AU";
			companyAU.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
			var branchSYD = companyAU.Branches.AddNew();
			branchSYD.GB_Code = "BNX";
			branchSYD.GB_RL_NKHomePort = "AUBNE";
			var companyNZ = Factory.New<GlbCompany>();
			companyNZ.GC_Code = "NZ";
			companyNZ.GC_RN_NKCountryCode = Constants.CountryCodes.NewZealand;
			var branchAKL = companyNZ.Branches.AddNew();
			branchAKL.GB_Code = "AKL";
			branchAKL.GB_RL_NKHomePort = "NZAKL";
			var companySG = Factory.New<GlbCompany>();
			companySG.GC_Code = "SG";
			companySG.GC_RN_NKCountryCode = Constants.CountryCodes.Singapore;
			var branchSIN = companySG.Branches.AddNew();
			branchSIN.GB_Code = "SIX";
			branchSIN.GB_RL_NKHomePort = "SGSIN";
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "PRINCIPAL";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			client1.OH_Code = "CLIENT1";
			client1.CompanyData.OB_IsDebtor = true;
			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			client2.OH_Code = "CLIENT2";
			client2.CompanyData.OB_IsDebtor = true;
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Wharf";
			sender.OH_IsSeaCTO = true;
			sender.MainAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "AALME";
			var stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "CCLU4741410";
			stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var stock3 = Factory.New<RefContainerStock>();
			stock3.R6_ContainerNum = "CCLU4595589";
			stock3.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			var booking = Factory.New<AgencyBooking>();
			booking.JS_UniqueConsignRef = "V00000101";
			booking.JS_CFSReference = "";
			var bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = "V00000102";
			bill.JS_OH_DeliveryAgent = principal.PK;
			bill.JS_CFSReference = "BLATICUSX";
			var headerAU = new JobHeader.Loader(bill).TryCreate();
			headerAU.JH_GC = companyAU.PK;
			headerAU.JH_GB = branchSYD.PK;
			headerAU.JH_GE = GlbDepartment.CurrentDepartment.PK;
			headerAU.JH_OA_LocalChargesAddr = client1.MainAddress.PK;
			var headerNZ = new JobHeader.Loader(bill).TryCreate();
			headerNZ.JH_GC = companyNZ.PK;
			headerNZ.JH_GB = branchAKL.PK;
			headerNZ.JH_GE = GlbDepartment.CurrentDepartment.PK;
			headerNZ.JH_OA_LocalChargesAddr = client2.MainAddress.PK;
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("stock1 sholud have 1 movement", 1, stock1.Movements.Count);
			AssertEquals("should not have created stock for CCLU4408380", null, RefContainerStock.Load(Factory, "CCLU4408380"));
			AssertEquals("stock3 should have 1 movement", 1, stock3.Movements.Count);
			var movement1 = stock1.Movements[0];
			AssertEquals("movement1.E9_MovementType", ContainerMovementTypes.Codes.Discharge, movement1.E9_MovementType);
			AssertEquals("movement1.E9_JV", ZGuid.Empty, movement1.E9_JV);
			AssertEquals("movement1.E9_ContainerIsEmpty", true, movement1.E9_ContainerIsEmpty);
			AssertEquals("movement1.E9_MovementDate", new ZDateTime(2008, 06, 26, 09, 54, 00), movement1.E9_MovementDate);
			AssertEquals("movement1.E9_OA_Depot", sender.MainAddress.PK, movement1.E9_OA_Depot);
			AssertEquals("movement1.E9_OH_Principal", ZGuid.Empty, movement1.E9_OH_Principal);
			AssertEquals("movement1.E9_OH_ResponsibleParty", ZGuid.Empty, movement1.E9_OH_ResponsibleParty);
			var movement3 = stock3.Movements[0];
			AssertEquals("movement3.E9_MovementType", ContainerMovementTypes.Codes.Discharge, movement3.E9_MovementType);
			AssertEquals("movement3.E9_JV", ZGuid.Empty, movement3.E9_JV);
			AssertEquals("movement3.E9_ContainerIsEmpty", true, movement3.E9_ContainerIsEmpty);
			AssertEquals("movement3.E9_MovementDate", new ZDateTime(2008, 06, 26, 15, 25, 00), movement3.E9_MovementDate);
			AssertEquals("movement3.E9_OA_Depot", sender.MainAddress.PK, movement3.E9_OA_Depot);
			AssertEquals("movement3.E9_OH_Principal", ZGuid.Empty, movement3.E9_OH_Principal);
			AssertEquals("movement3.E9_OH_ResponsibleParty", ZGuid.Empty, movement3.E9_OH_ResponsibleParty);
			AssertEquals("movement1 should have 1 message", 1, movement1.Messages.Count);
			AssertEquals("movement3 should have 1 message", 1, movement3.Messages.Count);
			var message1 = movement1.Messages[0];
			AssertEquals("message1.EM_ApplicationCode", EDIMessage.ApplicationCodes.ContainerManagement, message1.EM_ApplicationCode);
			AssertEquals("message1.EM_MessageType", EDIMessage.ApplicationCodes.ContainerManagement, message1.EM_MessageType);
			AssertEquals("message1.EM_Status", EDIMessage.Status.Recognised, message1.EM_Status);
			AssertEquals("message1.EM_LinkTable", "JobContainerMove", message1.EM_LinkTable);
			AssertMultilineASCIIEquals("message1.EM_MessageText", messageText.Replace("'", "'\r\n"), message1.EM_MessageText.Replace("'", "'\r\n"));
			EDIMessage message3 = movement3.Messages[0];
			AssertEquals("message3.EM_ApplicationCode", EDIMessage.ApplicationCodes.ContainerManagement, message3.EM_ApplicationCode);
			AssertEquals("message3.EM_MessageType", EDIMessage.ApplicationCodes.ContainerManagement, message3.EM_MessageType);
			AssertEquals("message3.EM_Status", EDIMessage.Status.Recognised, message3.EM_Status);
			AssertEquals("message3.EM_LinkTable", "JobContainerMove", message3.EM_LinkTable);
			AssertMultilineASCIIEquals("message3.EM_MessageText", messageText.Replace("'", "'\r\n"), message3.EM_MessageText.Replace("'", "'\r\n"));
			AssertCollectionContains("atleast one of the messages must be the original.", message.PK, new ZGuid[] { message1.PK, message3.PK });
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Discharge COARRI Message From Random Wharf\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock1.PK.ToGuid());
			mappings["(*URL2*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock3.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.COARRI.GetResultBody3(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_Dsc_RequireOrgs_Related()
		{
			AgencyRegistry.Instance.CMMCreateMissingContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			#region messageText
			const string messageText = // headder
			"UNH+1881+CODECO:D:95B:UN:ITG10'" + "BGM+36+1881+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+AALME:160:184'" + // container 1
			"EQD+CN+CCLU4741410+42R0:102:5+++4'" + "DTM+7:200806260954:203'" + // footer
			"CNT+16:1'" + "UNT+9+1881'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Wharf";
			sender.OH_IsSeaCTO = true;
			sender.MainAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "AALME";
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9290127";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0033";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4741410";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = "V00000103";
			bill.JS_JX = voyage.Sailings[0].PK;
			var container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = stock.R6_ContainerNum;
			container.JC_RC = stock.R6_RC;
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("stock sholud have 1 movement", 1, stock.Movements.Count);
			var movement = stock.Movements[0];
			CombineAssertions(delegate
			{
				AssertEquals("movement.E9_MovementType", ContainerMovementTypes.Codes.WharfGateOut, movement.E9_MovementType);
				AssertEquals("movement.E9_JV", voyage.PK, movement.E9_JV);
				AssertEquals("movement.E9_ContainerIsEmpty", true, movement.E9_ContainerIsEmpty);
				AssertEquals("movement.E9_MovementDate", new ZDateTime(2008, 06, 26, 09, 54, 00), movement.E9_MovementDate);
				AssertEquals("movement.E9_OA_Depot", sender.MainAddress.PK, movement.E9_OA_Depot);
				AssertEquals("movement.E9_OH_Principal", ZGuid.Empty, movement.E9_OH_Principal);
				AssertEquals("movement.E9_OH_ResponsibleParty", ZGuid.Empty, movement.E9_OH_ResponsibleParty);
			});
			AssertEquals("movement1 should have 1 message", 1, movement.Messages.Count);
			var foundMessage = movement.Messages[0];
			CombineAssertions(delegate
			{
				AssertEquals("foundMessage.EM_ApplicationCode", EDIMessage.ApplicationCodes.ContainerManagement, foundMessage.EM_ApplicationCode);
				AssertEquals("foundMessage.EM_MessageType", EDIMessage.ApplicationCodes.ContainerManagement, foundMessage.EM_MessageType);
				AssertEquals("foundMessage.EM_Status", EDIMessage.Status.Recognised, foundMessage.EM_Status);
				AssertEquals("foundMessage.EM_LinkTable", "JobContainerMove", foundMessage.EM_LinkTable);
				AssertMultilineASCIIEquals("message.EM_MessageText", messageText.Replace("'", "'\r\n"), foundMessage.EM_MessageText.Replace("'", "'\r\n"));
			});
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-Out CODECO Message From Random Wharf - Containers Without Warnings\r\n" + "CC:\r\n" + "  ack@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("ack@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody11(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestProcessCODECOMessage_Dsc_RequireOrgs_Booking()
		{
			AgencyRegistry.Instance.CMMCreateMissingContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			#region messageText
			const string messageText = // headder
			"UNH+1881+CODECO:D:95B:UN:ITG10'" + "BGM+36+1881+9'" + "NAD+MS+AALME:160:184'" + "NAD+CF+ACEPOOL'" + // container 1
			"EQD+CN+CCLU4741410+42R0:102:5+++4'" + "RFF+BN:ACE TO CHINA'" + "DTM+7:200806260954:203'" + "LOC+165+AUMEL:139:6'" + "FTX+DAR++2'" + "FTX+ABS++50'" + "TDT+20++1++ACEPOOL:172:184+++:146'" + "NAD+CN+++ACS'" + // container 2
			"EQD+CN+CCLU4408380+0666:102:5+++4'" + "RFF+BN:ACE TO CHINA'" + "DTM+7:200806261145:203'" + "LOC+165+AUMEL:139:6'" + "FTX+DAR++2'" + "FTX+ABS++50'" + "TDT+20++1++ACEPOOL:172:184+++:146'" + "NAD+CN+++ACS'" + // container 3
			"EQD+CN+CCLU4595589+42R0:102:5+++4'" + "RFF+BN:BLATICUSX'" + "DTM+7:200806261525:203'" + "LOC+165+AUMEL:139:6'" + "FTX+DAR++2'" + "FTX+ABS++50'" + "TDT+20++1++ACEPOOL:172:184+++:146'" + "NAD+CN+++acs'" + // footer
			"CNT+16:3'" + "UNT+30+1881'" + "";
			#endregion
			var companyAU = Factory.New<GlbCompany>();
			companyAU.GC_Code = "AU";
			companyAU.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
			var branchSYD = companyAU.Branches.AddNew();
			branchSYD.GB_Code = "BNX";
			branchSYD.GB_RL_NKHomePort = "AUBNE";
			var companyNZ = Factory.New<GlbCompany>();
			companyNZ.GC_Code = "NZ";
			companyNZ.GC_RN_NKCountryCode = Constants.CountryCodes.NewZealand;
			var branchAKL = companyNZ.Branches.AddNew();
			branchAKL.GB_Code = "AKL";
			branchAKL.GB_RL_NKHomePort = "NZAKL";
			var companySG = Factory.New<GlbCompany>();
			companySG.GC_Code = "SG";
			companySG.GC_RN_NKCountryCode = Constants.CountryCodes.Singapore;
			var branchSIN = companySG.Branches.AddNew();
			branchSIN.GB_Code = "SIX";
			branchSIN.GB_RL_NKHomePort = "SGSIN";
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "PRINCIPAL";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			client1.OH_Code = "CLIENT1";
			client1.CompanyData.OB_IsDebtor = true;
			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			client2.OH_Code = "CLIENT2";
			client2.CompanyData.OB_IsDebtor = true;
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Wharf";
			sender.OH_IsSeaCTO = true;
			sender.MainAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "AALME";
			var stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "CCLU4741410";
			stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var stock3 = Factory.New<RefContainerStock>();
			stock3.R6_ContainerNum = "CCLU4595589";
			stock3.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			var booking = Factory.New<AgencyBooking>();
			booking.JS_UniqueConsignRef = "V00000101";
			booking.JS_CFSReference = "";
			var bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = "V00000102";
			bill.JS_OH_DeliveryAgent = principal.PK;
			bill.JS_CFSReference = "BLATICUSX";
			var headerAU = new JobHeader.Loader(bill).TryCreate();
			headerAU.JH_GC = companyAU.PK;
			headerAU.JH_GB = branchSYD.PK;
			headerAU.JH_GE = GlbDepartment.CurrentDepartment.PK;
			headerAU.JH_OA_LocalChargesAddr = client1.MainAddress.PK;
			var headerNZ = new JobHeader.Loader(bill).TryCreate();
			headerNZ.JH_GC = companyNZ.PK;
			headerNZ.JH_GB = branchAKL.PK;
			headerNZ.JH_GE = GlbDepartment.CurrentDepartment.PK;
			headerNZ.JH_OA_LocalChargesAddr = client2.MainAddress.PK;
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var countryProcessorMock = new Mock<ICMMCountryProcessor>();

			var processorFactory = processorFactoryMock.Object;
			var countryProcessor = countryProcessorMock.Object;

			processorFactoryMock
				.Setup(m => m.Invoke(It.IsAny<CMMEmailGenerator>(), It.IsAny<ICMMProcessingAdapter>()))
				.Returns(countryProcessor);

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			processorFactoryMock
				.Verify(m => m.Invoke(It.IsNotNull<CMMEmailGenerator>(), It.IsNotNull<ICMMProcessingAdapter>()));

			AssertEquals("stock1 sholud have 1 movement", 1, stock1.Movements.Count);
			AssertEquals("should not have created stock for CCLU4408380", null, RefContainerStock.Load(Factory, "CCLU4408380"));
			AssertEquals("stock3 should have 1 movement", 1, stock3.Movements.Count);
			var movement1 = stock1.Movements[0];
			AssertEquals("movement1.E9_MovementType", ContainerMovementTypes.Codes.WharfGateOut, movement1.E9_MovementType);
			AssertEquals("movement1.E9_JV", ZGuid.Empty, movement1.E9_JV);
			AssertEquals("movement1.E9_ContainerIsEmpty", true, movement1.E9_ContainerIsEmpty);
			AssertEquals("movement1.E9_MovementDate", new ZDateTime(2008, 06, 26, 09, 54, 00), movement1.E9_MovementDate);
			AssertEquals("movement1.E9_OA_Depot", sender.MainAddress.PK, movement1.E9_OA_Depot);
			AssertEquals("movement1.E9_OH_Principal", ZGuid.Empty, movement1.E9_OH_Principal);
			AssertEquals("movement1.E9_OH_ResponsibleParty", ZGuid.Empty, movement1.E9_OH_ResponsibleParty);
			var movement3 = stock3.Movements[0];
			AssertEquals("movement3.E9_MovementType", ContainerMovementTypes.Codes.WharfGateOut, movement3.E9_MovementType);
			AssertEquals("movement3.E9_JV", ZGuid.Empty, movement3.E9_JV);
			AssertEquals("movement3.E9_ContainerIsEmpty", true, movement3.E9_ContainerIsEmpty);
			AssertEquals("movement3.E9_MovementDate", new ZDateTime(2008, 06, 26, 15, 25, 00), movement3.E9_MovementDate);
			AssertEquals("movement3.E9_OA_Depot", sender.MainAddress.PK, movement3.E9_OA_Depot);
			AssertEquals("movement3.E9_OH_Principal", principal.PK, movement3.E9_OH_Principal);
			AssertEquals("movement3.E9_OH_ResponsibleParty", client2.PK, movement3.E9_OH_ResponsibleParty);
			AssertEquals("movement1 should have 1 message", 1, movement1.Messages.Count);
			AssertEquals("movement3 should have 1 message", 1, movement3.Messages.Count);
			var message1 = movement1.Messages[0];
			AssertEquals("message1.EM_ApplicationCode", EDIMessage.ApplicationCodes.ContainerManagement, message1.EM_ApplicationCode);
			AssertEquals("message1.EM_MessageType", EDIMessage.ApplicationCodes.ContainerManagement, message1.EM_MessageType);
			AssertEquals("message1.EM_Status", EDIMessage.Status.Recognised, message1.EM_Status);
			AssertEquals("message1.EM_LinkTable", "JobContainerMove", message1.EM_LinkTable);
			AssertMultilineASCIIEquals("message1.EM_MessageText", messageText.Replace("'", "'\r\n"), message1.EM_MessageText.Replace("'", "'\r\n"));
			var message3 = movement3.Messages[0];
			AssertEquals("message3.EM_ApplicationCode", EDIMessage.ApplicationCodes.ContainerManagement, message3.EM_ApplicationCode);
			AssertEquals("message3.EM_MessageType", EDIMessage.ApplicationCodes.ContainerManagement, message3.EM_MessageType);
			AssertEquals("message3.EM_Status", EDIMessage.Status.Recognised, message3.EM_Status);
			AssertEquals("message3.EM_LinkTable", "JobContainerMove", message3.EM_LinkTable);
			AssertMultilineASCIIEquals("message3.EM_MessageText", messageText.Replace("'", "'\r\n"), message3.EM_MessageText.Replace("'", "'\r\n"));
			AssertCollectionContains("atleast one of the messages must be the original.", message.PK, new ZGuid[] { message1.PK, message3.PK });
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-Out CODECO Message From Random Wharf - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock1.PK.ToGuid());
			mappings["(*URL2*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock3.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody6(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestProcessCODECOMessage_Dsc_RequireOrgs_Bill()
		{
			AgencyRegistry.Instance.CMMCreateMissingContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			#region messageText
			const string messageText = // headder
			"UNH+1881+CODECO:D:95B:UN:ITG10'" + "BGM+36+1881+9'" + "NAD+MS+AALME:160:184'" + "NAD+CF+ACEPOOL'" + // container 1
			"EQD+CN+CCLU4741410+42R0:102:5+++4'" + "RFF+BM:ACE TO CHINA'" + "DTM+7:200806260954:203'" + "LOC+165+AUMEL:139:6'" + "FTX+DAR++2'" + "FTX+ABS++50'" + "TDT+20++1++ACEPOOL:172:184+++:146'" + "NAD+CN+++ACS'" + // container 2
			"EQD+CN+CCLU4408380+0666:102:5+++4'" + "RFF+BN:ACE TO CHINA'" + "DTM+7:200806261145:203'" + "LOC+165+AUMEL:139:6'" + "FTX+DAR++2'" + "FTX+ABS++50'" + "TDT+20++1++ACEPOOL:172:184+++:146'" + "NAD+CN+++ACS'" + // container 3
			"EQD+CN+CCLU4595589+42R0:102:5+++4'" + "RFF+BM:BLATICUSX'" + "DTM+7:200806261525:203'" + "LOC+165+AUMEL:139:6'" + "FTX+DAR++2'" + "FTX+ABS++50'" + "TDT+20++1++ACEPOOL:172:184+++:146'" + "NAD+CN+++acs'" + // footer
			"CNT+16:3'" + "UNT+30+1881'" + "";
			#endregion
			var companyAU = Factory.New<GlbCompany>();
			companyAU.GC_Code = "AU";
			companyAU.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
			var branchSYD = companyAU.Branches.AddNew();
			branchSYD.GB_Code = "BNX";
			branchSYD.GB_RL_NKHomePort = "AUBNE";
			var companyNZ = Factory.New<GlbCompany>();
			companyNZ.GC_Code = "NZ";
			companyNZ.GC_RN_NKCountryCode = Constants.CountryCodes.NewZealand;
			var branchAKL = companyNZ.Branches.AddNew();
			branchAKL.GB_Code = "AKL";
			branchAKL.GB_RL_NKHomePort = "NZAKL";
			var companySG = Factory.New<GlbCompany>();
			companySG.GC_Code = "SG";
			companySG.GC_RN_NKCountryCode = Constants.CountryCodes.Singapore;
			var branchSIN = companySG.Branches.AddNew();
			branchSIN.GB_Code = "SIX";
			branchSIN.GB_RL_NKHomePort = "SGSIN";
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "PRINCIPAL";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			client1.OH_Code = "CLIENT1";
			client1.CompanyData.OB_IsDebtor = true;
			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			client2.OH_Code = "CLIENT2";
			client2.CompanyData.OB_IsDebtor = true;
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Wharf";
			sender.OH_IsSeaCTO = true;
			sender.MainAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "AALME";
			var stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "CCLU4741410";
			stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var stock3 = Factory.New<RefContainerStock>();
			stock3.R6_ContainerNum = "CCLU4595589";
			stock3.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			var booking = Factory.New<AgencyBooking>();
			booking.JS_UniqueConsignRef = "V00000101";
			booking.JS_CFSReference = "";
			var bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = "V00000102";
			bill.JS_OH_DeliveryAgent = principal.PK;
			bill.JS_HouseBill = "BLATICUSX";
			var headerAU = new JobHeader.Loader(bill).TryCreate();
			headerAU.JH_GC = companyAU.PK;
			headerAU.JH_GB = branchSYD.PK;
			headerAU.JH_GE = GlbDepartment.CurrentDepartment.PK;
			headerAU.JH_OA_LocalChargesAddr = client1.MainAddress.PK;
			var headerNZ = new JobHeader.Loader(bill).TryCreate();
			headerNZ.JH_GC = companyNZ.PK;
			headerNZ.JH_GB = branchAKL.PK;
			headerNZ.JH_GE = GlbDepartment.CurrentDepartment.PK;
			headerNZ.JH_OA_LocalChargesAddr = client2.MainAddress.PK;
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var countryProcessorMock = new Mock<ICMMCountryProcessor>();

			var processorFactory = processorFactoryMock.Object;
			var countryProcessor = countryProcessorMock.Object;

			processorFactoryMock
				.Setup(m => m.Invoke(It.IsNotNull<CMMEmailGenerator>(), It.IsNotNull<ICMMProcessingAdapter>()))
				.Returns(countryProcessor);

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			processorFactoryMock
				.Verify(m => m.Invoke(It.IsNotNull<CMMEmailGenerator>(), It.IsNotNull<ICMMProcessingAdapter>()));

			AssertEquals("stock1 sholud have 1 movement", 1, stock1.Movements.Count);
			AssertEquals("should not have created stock for CCLU4408380", null, RefContainerStock.Load(Factory, "CCLU4408380"));
			AssertEquals("stock3 should have 1 movement", 1, stock3.Movements.Count);
			var movement1 = stock1.Movements[0];
			AssertEquals("movement1.E9_MovementType", ContainerMovementTypes.Codes.WharfGateOut, movement1.E9_MovementType);
			AssertEquals("movement1.E9_JV", ZGuid.Empty, movement1.E9_JV);
			AssertEquals("movement1.E9_ContainerIsEmpty", true, movement1.E9_ContainerIsEmpty);
			AssertEquals("movement1.E9_MovementDate", new ZDateTime(2008, 06, 26, 09, 54, 00), movement1.E9_MovementDate);
			AssertEquals("movement1.E9_OA_Depot", sender.MainAddress.PK, movement1.E9_OA_Depot);
			AssertEquals("movement1.E9_OH_Principal", ZGuid.Empty, movement1.E9_OH_Principal);
			AssertEquals("movement1.E9_OH_ResponsibleParty", ZGuid.Empty, movement1.E9_OH_ResponsibleParty);
			var movement3 = stock3.Movements[0];
			AssertEquals("movement3.E9_MovementType", ContainerMovementTypes.Codes.WharfGateOut, movement3.E9_MovementType);
			AssertEquals("movement3.E9_JV", ZGuid.Empty, movement3.E9_JV);
			AssertEquals("movement3.E9_ContainerIsEmpty", true, movement3.E9_ContainerIsEmpty);
			AssertEquals("movement3.E9_MovementDate", new ZDateTime(2008, 06, 26, 15, 25, 00), movement3.E9_MovementDate);
			AssertEquals("movement3.E9_OA_Depot", sender.MainAddress.PK, movement3.E9_OA_Depot);
			AssertEquals("movement3.E9_OH_Principal", principal.PK, movement3.E9_OH_Principal);
			AssertEquals("movement3.E9_OH_ResponsibleParty", client2.PK, movement3.E9_OH_ResponsibleParty);
			AssertEquals("movement1 should have 1 message", 1, movement1.Messages.Count);
			AssertEquals("movement3 should have 1 message", 1, movement3.Messages.Count);
			var message1 = movement1.Messages[0];
			AssertEquals("message1.EM_ApplicationCode", EDIMessage.ApplicationCodes.ContainerManagement, message1.EM_ApplicationCode);
			AssertEquals("message1.EM_MessageType", EDIMessage.ApplicationCodes.ContainerManagement, message1.EM_MessageType);
			AssertEquals("message1.EM_Status", EDIMessage.Status.Recognised, message1.EM_Status);
			AssertEquals("message1.EM_LinkTable", "JobContainerMove", message1.EM_LinkTable);
			AssertMultilineASCIIEquals("message1.EM_MessageText", messageText.Replace("'", "'\r\n"), message1.EM_MessageText.Replace("'", "'\r\n"));
			var message3 = movement3.Messages[0];
			AssertEquals("message3.EM_ApplicationCode", EDIMessage.ApplicationCodes.ContainerManagement, message3.EM_ApplicationCode);
			AssertEquals("message3.EM_MessageType", EDIMessage.ApplicationCodes.ContainerManagement, message3.EM_MessageType);
			AssertEquals("message3.EM_Status", EDIMessage.Status.Recognised, message3.EM_Status);
			AssertEquals("message3.EM_LinkTable", "JobContainerMove", message3.EM_LinkTable);
			AssertMultilineASCIIEquals("message3.EM_MessageText", messageText.Replace("'", "'\r\n"), message3.EM_MessageText.Replace("'", "'\r\n"));
			AssertCollectionContains("atleast one of the messages must be the original.", message.PK, new ZGuid[] { message1.PK, message3.PK });
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-Out CODECO Message From Random Wharf - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock1.PK.ToGuid());
			mappings["(*URL2*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock3.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody7(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_AmbigiousDepot()
		{
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+++4'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+14+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Org";
			sender.OH_IsUnpackDepot = true;
			sender.OH_IsSeaCTO = true;
			sender.OH_IsContainerYard = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4214635";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("EM_LinkTable", "JobContainerMove", message.EM_LinkTable);
			var movement = Factory.Load<ContainerMovement>(message.EM_LinkUniqueID);
			AssertEquals("movement type", "", movement.E9_MovementType);
			AssertEquals("voyage", ZGuid.Empty, movement.E9_JV);
			AssertEquals("is empty", true, movement.E9_ContainerIsEmpty);
			AssertEquals("movement date", new ZDateTime(2008, 06, 27, 10, 10, 00), movement.E9_MovementDate);
			AssertEquals("depot", sender.MainAddress.PK, movement.E9_OA_Depot);
			AssertEquals("movement stock", stock.PK, movement.E9_R6);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Org\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody3(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_DscVesselNull()
		{
			#region MessageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+voy12+1++CSH:172:184+++:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUBNE:139:6+SHCB1'" + "LOC+8+AUBNE'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+14+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4214635";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultDscVesselNull(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_DscVoyageNull()
		{
			#region MessageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20++1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+14+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4214635";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9290127";
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultDscVoyageNull(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_DscVoyageVesselIsNullOrEmpty()
		{
			#region MessageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+13+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4214635";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers Without Warnings\r\n" + "CC:\r\n" + "  ack@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("ack@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultDscVoyageVesselIsNullOrEmpty(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_ContainerTypeMapping()
		{
			AgencyRegistry.Instance.CMMCreateMissingContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+2232:102:5+++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+14+1'" + "";
			#endregion
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var pMatch = sender.CreatePatternMatchOverrideForTest();
			pMatch.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ContainerType;
			pMatch.OO_ForeignCode = "2232";
			pMatch.OO_LocalGuid = gp20;
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("EM_LinkTable", "JobContainerMove", message.EM_LinkTable);
			var movement = Factory.Load<ContainerMovement>(message.EM_LinkUniqueID);
			AssertEquals("movement type", ContainerMovementTypes.Codes.DepotGateIn, movement.E9_MovementType);
			AssertEquals("voyage", ZGuid.Empty, movement.E9_JV);
			AssertEquals("is empty", false, movement.E9_ContainerIsEmpty);
			AssertEquals("movement date", new ZDateTime(2008, 06, 27, 10, 10, 00), movement.E9_MovementDate);
			AssertEquals("depot", sender.MainAddress.PK, movement.E9_OA_Depot);
			AssertNotNull("movement stock", movement.Stock);
			AssertEquals("container num", "CCLU4214635", movement.Stock.R6_ContainerNum);
			AssertEquals("container type", gp20, movement.Stock.R6_RC);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, movement.E9_R6.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody4(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_ContainerTypeMappingExisting()
		{
			AgencyRegistry.Instance.CMMCreateMissingContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container 1
			"EQD+CN+TEST4100013+2232:102:5+++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // container 2
			"EQD+CN+TEST4100029+2232:102:5+++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // container 3
			"EQD+CN+TEST4100034+2232:102:5+++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:3'" + "UNT+26+1'" + "";
			#endregion
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var re20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");
			var nor20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20NOR");
			AssertEquals("precondition: 20RE.ISO == 20NOR.ISO", re20.RC_ISOType, nor20.RC_ISOType);
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var pMatch = sender.CreatePatternMatchOverrideForTest();
			pMatch.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ContainerType;
			pMatch.OO_ForeignCode = "2232";
			pMatch.OO_LocalGuid = re20.PK;
			var stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "TEST4100013";
			stock1.R6_RC = gp20.PK;
			var stock2 = Factory.New<RefContainerStock>();
			stock2.R6_ContainerNum = "TEST4100029";
			stock2.R6_RC = re20.PK;
			var stock3 = Factory.New<RefContainerStock>();
			stock3.R6_ContainerNum = "TEST4100034";
			stock3.R6_RC = nor20.PK;
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("EM_LinkTable", "JobContainerMove", message.EM_LinkTable);
			var movement1 = stock1.Movements[0];
			AssertEquals("movement type", ContainerMovementTypes.Codes.DepotGateIn, movement1.E9_MovementType);
			AssertEquals("voyage", ZGuid.Empty, movement1.E9_JV);
			AssertEquals("is empty", false, movement1.E9_ContainerIsEmpty);
			AssertEquals("movement date", new ZDateTime(2008, 06, 27, 10, 10, 00), movement1.E9_MovementDate);
			AssertEquals("depot", sender.MainAddress.PK, movement1.E9_OA_Depot);
			AssertEquals("type not changed", gp20.PK, stock1.R6_RC);
			var movement2 = stock2.Movements[0];
			AssertEquals("movement type", ContainerMovementTypes.Codes.DepotGateIn, movement2.E9_MovementType);
			AssertEquals("voyage", ZGuid.Empty, movement2.E9_JV);
			AssertEquals("is empty", false, movement2.E9_ContainerIsEmpty);
			AssertEquals("movement date", new ZDateTime(2008, 06, 27, 10, 10, 00), movement2.E9_MovementDate);
			AssertEquals("depot", sender.MainAddress.PK, movement2.E9_OA_Depot);
			AssertEquals("type not changed", re20.PK, stock2.R6_RC);
			var movement3 = stock3.Movements[0];
			AssertEquals("movement type", ContainerMovementTypes.Codes.DepotGateIn, movement3.E9_MovementType);
			AssertEquals("voyage", ZGuid.Empty, movement3.E9_JV);
			AssertEquals("is empty", false, movement3.E9_ContainerIsEmpty);
			AssertEquals("movement date", new ZDateTime(2008, 06, 27, 10, 10, 00), movement3.E9_MovementDate);
			AssertEquals("depot", sender.MainAddress.PK, movement3.E9_OA_Depot);
			AssertEquals("type not changed", nor20.PK, stock3.R6_RC);
			const string expectedEmailDef1 = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			const string expectedEmailDef2 = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers Without Warnings\r\n" + "CC:\r\n" + "  ack@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef1, expectedEmailDef2 }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, movement1.E9_R6.ToGuid());
			mappings["(*URL2*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, movement2.E9_R6.ToGuid());
			mappings["(*URL3*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, movement3.E9_R6.ToGuid());
			var ackEmail = FindEmail((p) => p.CCRecipients.Contains("ack@freadnet.org"));
			var dscEmail = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("ACK: expected body", TestFileHelper.CODECO.GetResultBody12_ACK(mappings), ackEmail.Body);
			AssertMultilineASCIIEquals("DSC: expected body", TestFileHelper.CODECO.GetResultBody12_DSC(mappings), dscEmail.Body);
			AssertContainsExactElementsInAnyOrder("ACK: should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(ackEmail.Attachments, (AttachmentDef a) => a.DisplayName));
			AssertContainsExactElementsInAnyOrder("DSC: should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(dscEmail.Attachments, (AttachmentDef a) => a.DisplayName));
			var ackAttachment = FindAttachment(ackEmail, "message.edi");
			var dscAttachment = FindAttachment(dscEmail, "message.edi");
			AssertMultilineASCIIEquals("ACK: attachment content", messageText, Encoding.UTF8.GetString(ackAttachment.Data));
			AssertMultilineASCIIEquals("DSC: attachment content", messageText, Encoding.UTF8.GetString(dscAttachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_ContainerTypeName()
		{
			AgencyRegistry.Instance.CMMCreateMissingContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+2232:102:5+++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+14+1'" + "";
			#endregion
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var cType = Factory.New<RefContainer>();
			cType.RC_Code = "2232";
			cType.RC_ISOType = "2230";
			var cType1 = Factory.New<RefContainer>();
			cType1.RC_Code = "2232_1";
			cType1.RC_ISOType = "2230";
			var cType2 = Factory.New<RefContainer>();
			cType2.RC_Code = "2232_2";
			cType2.RC_ISOType = "2230";
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			Factory.Save();
			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("EM_LinkTable", "JobContainerMove", message.EM_LinkTable);
			var movement = Factory.Load<ContainerMovement>(message.EM_LinkUniqueID);
			AssertEquals("movement type", ContainerMovementTypes.Codes.DepotGateIn, movement.E9_MovementType);
			AssertEquals("voyage", ZGuid.Empty, movement.E9_JV);
			AssertEquals("is empty", false, movement.E9_ContainerIsEmpty);
			AssertEquals("movement date", new ZDateTime(2008, 06, 27, 10, 10, 00), movement.E9_MovementDate);
			AssertEquals("depot", sender.MainAddress.PK, movement.E9_OA_Depot);
			AssertNotNull("movement stock", movement.Stock);
			AssertEquals("container num", "CCLU4214635", movement.Stock.R6_ContainerNum);
			AssertEquals("container type", "2232_3", movement.Stock.Container.RC_Code);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, movement.E9_R6.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody5(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_UpdateSealNumbers()
		{
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container 1
			"EQD+CN+TEST4100013+42R0:102:5+++5'" + "RFF+BN:BLATICUSX'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "SEL+SEAL1A+SH'" + "SEL+SEAL1B+SH'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // container 2
			"EQD+CN+TEST4100029+42R0:102:5+++5'" + "RFF+BN:BLATICUSX'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "SEL+SEAL2A+SH'" + "SEL+SEAL2B+SH'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // container 3
			"EQD+CN+TEST4100034+42R0:102:5+++5'" + "RFF+BN:BLATICUSX'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "SEL+SEAL3A+SH'" + "SEL+SEAL3B+SH'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // container 4
			"EQD+CN+TEST4100042+42R0:102:5+++5'" + "RFF+BN:BLATICUSX'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "SEL+SEAL4A+SH'" + "SEL+SEAL4B+SH'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+35+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random CTO";
			sender.OH_IsSeaCTO = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "TEST4100013";
			stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var stock2 = Factory.New<RefContainerStock>();
			stock2.R6_ContainerNum = "TEST4100029";
			stock2.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var stock3 = Factory.New<RefContainerStock>();
			stock3.R6_ContainerNum = "TEST4100034";
			stock3.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var stock4 = Factory.New<RefContainerStock>();
			stock4.R6_ContainerNum = "TEST4100042";
			stock4.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9290127";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0033";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			var booking = Factory.New<AgencyBooking>();
			booking.JS_JX = voyage.Sailings[0].PK;
			booking.JS_CFSReference = "BLATICUSX";
			var container2 = AddSealedContainers(booking, stock2);
			var container3 = AddSealedContainers(booking, stock3, "MAGIC1", "MAGIC2");
			var container4 = AddSealedContainers(booking, stock4, "SEAL4B");
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var countryProcessorMock = new Mock<ICMMCountryProcessor>();

			var processorFactory = processorFactoryMock.Object;
			var countryProcessor = countryProcessorMock.Object;

			processorFactoryMock
				.Setup(m => m.Invoke(It.IsNotNull<CMMEmailGenerator>(), It.IsNotNull<ICMMProcessingAdapter>()))
				.Returns(countryProcessor);

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			countryProcessorMock
				.Verify(m => m.UpdateContainer(It.IsNotNull<CMMMessageContainer>(), It.IsNotNull<AgencyShipmentContainer>()));

			var filter = new ZQuery();
			filter.AddToFilter(JobContainerSchema.JC_JS_FCLBookingOnlyLink, booking.PK);
			filter.AddToFilter(JobContainerSchema.JC_Purpose, ContainerBookedStatus.Codes.Real);
			filter.AddToFilter(JobContainerSchema.PK, SQLComparisonOperator.NotEqual, new ZGuid[] { container2.PK, container3.PK, container4.PK });
			filter.MaximumRows = 2;
			var containers = (AgencyShipmentContainer[])Factory.Load(container2.GetType(), filter);
			AssertEquals("Shipment should have a new actual container", 1, containers.Length);
			var container1 = containers[0];
			AssertEquals("container1 seal1", "SEAL1A", container1.JC_SealNum);
			AssertEquals("container1 seal2", "SEAL1B", container1.JC_AdditionalSealNum);
			AssertEquals("container1 seal3", "", container1.JC_Additional2SealNum);
			AssertEquals("container2 seal1", "SEAL2A", container2.JC_SealNum);
			AssertEquals("container2 seal2", "SEAL2B", container2.JC_AdditionalSealNum);
			AssertEquals("container2 seal3", "", container2.JC_Additional2SealNum);
			AssertEquals("container3 seal1", "MAGIC1", container3.JC_SealNum);
			AssertEquals("container3 seal2", "MAGIC2", container3.JC_AdditionalSealNum);
			AssertEquals("container3 seal3", "SEAL3A", container3.JC_Additional2SealNum);
			AssertEquals("container4 seal1", "SEAL4B", container4.JC_SealNum);
			AssertEquals("container4 seal2", "SEAL4A", container4.JC_AdditionalSealNum);
			AssertEquals("container4 seal3", "", container4.JC_Additional2SealNum);
			const string expectedAckEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random CTO - Containers Without Warnings\r\n" + "CC:\r\n" + "  ack@freadnet.org\r\n" + "";
			const string expectedDscEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random CTO - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedAckEmailDef, expectedDscEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock1.PK.ToGuid());
			mappings["(*URL2*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock2.PK.ToGuid());
			mappings["(*URL3*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock3.PK.ToGuid());
			mappings["(*URL4*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock4.PK.ToGuid());
			var ackEmail = FindEmail((p) => p.CCRecipients.Contains("ack@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultUpdateSeals_Ack(mappings), ackEmail.Body);
			var dscEmail = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultUpdateSeals_Dsc(mappings), dscEmail.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(ackEmail.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(ackEmail, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_VerifySealNumbers_MessageEmpty()
		{
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+TEST4100013+42R0:102:5+++5'" + "RFF+BN:BLATICUSX'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+15+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9290127";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0033";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			var booking = Factory.New<BillOfLading>();
			booking.JS_JX = voyage.Sailings[0].PK;
			booking.JS_CFSReference = "BLATICUSX";
			var container = AddSealedContainers(booking, stock, "SEAL1A", "SEAL1B");
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var countryProcessorMock = new Mock<ICMMCountryProcessor>();

			var processorFactory = processorFactoryMock.Object;
			var countryProcessor = countryProcessorMock.Object;

			processorFactoryMock
				.Setup(m => m.Invoke(It.IsNotNull<CMMEmailGenerator>(), It.IsNotNull<ICMMProcessingAdapter>()))
				.Returns(countryProcessor);

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			countryProcessorMock
				.Verify(m => m.ValidateContainer(It.IsNotNull<CMMMessageContainer>(), It.IsNotNull<AgencyShipmentContainer>()));

			AssertEquals("container seal1", "SEAL1A", container.JC_SealNum);
			AssertEquals("container seal2", "SEAL1B", container.JC_AdditionalSealNum);
			AssertEquals("container seal3", "", container.JC_Additional2SealNum);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers Without Warnings\r\n" + "CC:\r\n" + "  ack@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("ack@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultVerifySeals(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_VerifySealNumbers_DBEmpty()
		{
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+TEST4100013+42R0:102:5+++5'" + "RFF+BN:BLATICUSX'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "SEL+SEAL1A+SH'" + "SEL+SEAL1B+SH'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+17+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9290127";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0033";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			var booking = Factory.New<BillOfLading>();
			booking.JS_JX = voyage.Sailings[0].PK;
			booking.JS_CFSReference = "BLATICUSX";
			var container = AddSealedContainers(booking, stock);
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var countryProcessorMock = new Mock<ICMMCountryProcessor>();

			var processorFactory = processorFactoryMock.Object;
			var countryProcessor = countryProcessorMock.Object;

			processorFactoryMock
				.Setup(m => m.Invoke(It.IsNotNull<CMMEmailGenerator>(), It.IsNotNull<ICMMProcessingAdapter>()))
				.Returns(countryProcessor);

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			countryProcessorMock
				.Verify(m => m.ValidateContainer(It.IsNotNull<CMMMessageContainer>(), It.IsNotNull<AgencyShipmentContainer>()));

			AssertEquals("container seal1", "", container.JC_SealNum);
			AssertEquals("container seal2", "", container.JC_AdditionalSealNum);
			AssertEquals("container seal3", "", container.JC_Additional2SealNum);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultVerifySealsDbEmpty(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_VerifySealNumbers_Match()
		{
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+TEST4100013+42R0:102:5+++5'" + "RFF+BN:BLATICUSX'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "SEL+SEAL1A+SH'" + "SEL+SEAL1B+SH'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+17+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9290127";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0033";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			var booking = Factory.New<BillOfLading>();
			booking.JS_JX = voyage.Sailings[0].PK;
			booking.JS_CFSReference = "BLATICUSX";
			var container = AddSealedContainers(booking, stock, "SEAL1A", "SEAL1B");
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var countryProcessorMock = new Mock<ICMMCountryProcessor>();

			var processorFactory = processorFactoryMock.Object;
			var countryProcessor = countryProcessorMock.Object;

			processorFactoryMock
				.Setup(m => m.Invoke(It.IsNotNull<CMMEmailGenerator>(), It.IsNotNull<ICMMProcessingAdapter>()))
				.Returns(countryProcessor);

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			countryProcessorMock
				.Verify(m => m.ValidateContainer(It.IsNotNull<CMMMessageContainer>(), It.IsNotNull<AgencyShipmentContainer>()));

			AssertEquals("container seal1", "SEAL1A", container.JC_SealNum);
			AssertEquals("container seal2", "SEAL1B", container.JC_AdditionalSealNum);
			AssertEquals("container seal3", "", container.JC_Additional2SealNum);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers Without Warnings\r\n" + "CC:\r\n" + "  ack@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("ack@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultVerifySeals(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_VerifySealNumbers_Missmatch()
		{
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+TEST4100013+42R0:102:5+++5'" + "RFF+BN:BLATICUSX'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "SEL+SEAL1A+SH'" + "SEL+SEAL1B+SH'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+17+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9290127";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0033";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			var booking = Factory.New<BillOfLading>();
			booking.JS_JX = voyage.Sailings[0].PK;
			booking.JS_CFSReference = "BLATICUSX";
			var container = AddSealedContainers(booking, stock, "SEAL1A", "SEAL1C");
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var countryProcessorMock = new Mock<ICMMCountryProcessor>();

			var processorFactory = processorFactoryMock.Object;
			var countryProcessor = countryProcessorMock.Object;

			processorFactoryMock
				.Setup(m => m.Invoke(It.IsNotNull<CMMEmailGenerator>(), It.IsNotNull<ICMMProcessingAdapter>()))
				.Returns(countryProcessor);

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			countryProcessorMock
				.Verify(m => m.ValidateContainer(It.IsNotNull<CMMMessageContainer>(), It.IsNotNull<AgencyShipmentContainer>()));

			AssertEquals("container seal1", "SEAL1A", container.JC_SealNum);
			AssertEquals("container seal2", "SEAL1C", container.JC_AdditionalSealNum);
			AssertEquals("container seal3", "", container.JC_Additional2SealNum);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultVerifySealsMissMatch(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCODECOMessage_ExistingMovement()
		{
			#region messageText
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+14+1'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4214635";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var movement = stock.Movements.AddNew();
			movement.E9_MovementType = ContainerMovementTypes.Codes.DepotGateIn;
			movement.E9_MovementDate = new ZDateTime(2008, 06, 27, 10, 10, 00);
			movement.E9_OA_Depot = sender.MainAddress.PK;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9290127";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0033";
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status", EDIMessage.Status.Discarded, message.EM_Status);
			AssertEquals("EM_LinkTable", "JobContainerMove", message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", movement.PK, message.EM_LinkUniqueID);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Gate-In CODECO Message From Random Depot - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.CODECO.GetResultBody14(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCOARRIMessage_Ack()
		{
			TestCaseHelper.ClearTable(OrgCusCodeSchema.Constants.TableName);
			#region messageText
			const string messageText = // header
			"UNH+718+COARRI:D:95B:UN:ITG10'" + "BGM+98+13910559+9'" + "TDT+20+0084S+1++CSC:172:184+++9224336:146'" + "LOC+11+AUSYD'" + "NAD+CA+CSC:160:184'" + "NAD+MS+CTLPB:160:184'" + // container 1
			"EQD+CN+CCLU4798734+42R0:102:5+++5'" + "DTM+203:200804051148:203'" + "LOC+7+AUSYD'" + "LOC+9+CNSHK'" + "LOC+11+AUSYD'" + "LOC+147+0501190::5'" + "MEA+AAE+G+KGM:6800'" + "FTX+AAA+++GEN'" + "NAD+CF+CSC:160:184'" + // footer
			"CNT+16:4'" + "UNT+35+718'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Wharf";
			sender.OH_IsSeaCTO = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "CTLPB";
			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4798734";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9224336";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0084S";
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("EM_LinkTable", "JobContainerMove", message.EM_LinkTable);
			var movement = Factory.Load<ContainerMovement>(message.EM_LinkUniqueID);
			AssertEquals("movement type", ContainerMovementTypes.Codes.Discharge, movement.E9_MovementType);
			AssertEquals("voyage", voyage.PK, movement.E9_JV);
			AssertEquals("is empty", false, movement.E9_ContainerIsEmpty);
			AssertEquals("movement date", new ZDateTime(2008, 04, 05, 11, 48, 00), movement.E9_MovementDate);
			AssertEquals("depot", sender.MainAddress.PK, movement.E9_OA_Depot);
			AssertEquals("movement stock", stock.PK, movement.E9_R6);
			const string expectedEmailDef = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Discharge COARRI Message From Random Wharf - Containers Without Warnings\r\n" + "CC:\r\n" + "  ack@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			var mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock.PK.ToGuid());
			var email = FindEmail((p) => p.CCRecipients.Contains("ack@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.COARRI.GetResultBody1(mappings), email.Body);
			AssertContainsExactElementsInAnyOrder("should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(email.Attachments, (AttachmentDef a) => a.DisplayName));
			var attachment = FindAttachment(email, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(attachment.Data));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCOARRIMessage_Dsc()
		{
			AgencyRegistry.Instance.CMMCreateMissingContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestCaseHelper.ClearTable(OrgCusCodeSchema.Constants.TableName);
			#region messageText
			const string messageText = // header
			"UNH+719+COARRI:D:95B:UN:ITG10'" + "BGM+270+13910566+9'" + "TDT+20+0085N+1++CSC:172:184+++9224336:146'" + "LOC+9+AUSYD'" + "NAD+CA+CSC:160:184'" + "NAD+MS+CTLPB:160:184'" + // container 1
			"EQD+CN+GESU4760670+45G0:102:5+++5'" + "RFF+BN:CSG119982'" + "DTM+203:200804051154:203'" + "LOC+11+TWKHH'" + "LOC+147+0340212::5'" + "MEA+AAE+G+KGM:24500'" + "NAD+CF+CSC:160:184'" + // container 2
			"EQD+CN+CCLU4084737+0666:102:5+++4'" + "RFF+BN:CQD85PO'" + "DTM+203:200804051155:203'" + "LOC+11+CNSHA'" + "LOC+147+0340114::5'" + "MEA+AAE+G+KGM:4000'" + "NAD+CF+CSC:160:184'" + // container 3
			"EQD+CN+CCLU8601144+45R1:102:5+++5'" + "RFF+BN:CSG122105'" + "DTM+203:200804051224:203'" + "LOC+11+TWKHH'" + "LOC+147+0340282::5'" + "MEA+AAE+G+KGM:29200'" + "TMP+2+-20:CEL'" + "NAD+CF+CSC:160:184'" + // container 4
			"EQD+CN+CCLU4798734+42R0:102:5+++5'" + "RFF+BN:CSG119982'" + "DTM+203:200804051148:203'" + "LOC+11+TWKHH'" + "LOC+147+0340212::5'" + "MEA+AAE+G+KGM:6800'" + "NAD+CF+CSC:160:184'" + // footer
			"CNT+16:5'" + "UNT+29+719'" + "";
			#endregion
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Wharf";
			sender.OH_IsSeaCTO = true;
			var codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "CTLPB";
			var stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "GESU4760670";
			stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40HC").PK;
			var stock3 = Factory.New<RefContainerStock>();
			stock3.R6_ContainerNum = "CCLU8601144";
			stock3.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL QINGDAO";
			vessel.RV_LloydsNumber = "9224336";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0085N";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			var otherVoyage = Factory.New<JobVoyage>();
			otherVoyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			otherVoyage.JV_VoyageFlight = "069";
			otherVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			otherVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGISN";
			otherVoyage.GenerateSailings();
			Factory.Save();
			var shipment1 = Factory.New<AgencyBooking>();
			shipment1.JS_UniqueConsignRef = "S00000101";
			shipment1.JS_JX = voyage.Sailings[0].PK;
			shipment1.JS_CFSReference = "CSG119982";
			var container1 = shipment1.RealContainers.AddNew();
			container1.JC_ContainerNum = "GESU4760670";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			var shipment2 = Factory.New<BillOfLading>();
			shipment2.JS_UniqueConsignRef = "S00000102";
			shipment2.JS_JX = otherVoyage.Sailings[0].PK;
			shipment2.JS_CFSReference = "CQD85PO";
			var shipment3 = Factory.New<BillOfLading>();
			shipment3.JS_UniqueConsignRef = "S00000103";
			shipment3.JS_JX = ZGuid.Empty;
			shipment3.JS_CFSReference = "CSG122105";
			AgencyShipmentContainer container3 = shipment3.RealContainers.AddNew();
			container3.JC_ContainerNum = "CCLU8601144";
			container3.JC_GrossWeight = 3000m;
			Factory.Save();
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			var message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var countryProcessorMock = new Mock<ICMMCountryProcessor>();

			var processorFactory = processorFactoryMock.Object;
			var countryProcessor = countryProcessorMock.Object;

			processorFactoryMock
				.Setup(m => m.Invoke(It.IsNotNull<CMMEmailGenerator>(), It.IsNotNull<ICMMProcessingAdapter>()))
				.Returns(countryProcessor);

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			countryProcessorMock
				.Verify(m => m.UpdateContainer(It.Is<CMMMessageContainer>(c => c.ContainerNumber == "GESU4760670"), It.IsNotNull<AgencyShipmentContainer>()));

			countryProcessorMock
				.Verify(m => m.UpdateContainer(It.Is<CMMMessageContainer>(c => c.ContainerNumber == "CCLU4798734"), It.IsNotNull<AgencyShipmentContainer>()));

			countryProcessorMock
				.Verify(m => m.ValidateContainer(It.Is<CMMMessageContainer>(c => c.ContainerNumber == "CCLU8601144"), It.IsNotNull<AgencyShipmentContainer>()));

			var stock2 = RefContainerStock.Load(Factory, "CCLU4084737");
			var stock4 = RefContainerStock.Load(Factory, "CCLU4798734");
			AssertEquals("stock1 should have 1 movement", 1, stock1.Movements.Count);
			AssertEquals("stock2 should have 1 movement", 1, stock2.Movements.Count);
			AssertEquals("stock3 should have 1 movement", 1, stock3.Movements.Count);
			AssertEquals("stock4 should have 1 movement", 1, stock4.Movements.Count);
			var movement1 = stock1.Movements[0];
			AssertEquals("movement1.E9_MovementType", ContainerMovementTypes.Codes.Load, movement1.E9_MovementType);
			AssertEquals("movement1.E9_JV", voyage.PK, movement1.E9_JV);
			AssertEquals("movement1.E9_ContainerIsEmpty", false, movement1.E9_ContainerIsEmpty);
			AssertEquals("movement1.E9_MovementDate", new ZDateTime(2008, 04, 05, 11, 54, 00), movement1.E9_MovementDate);
			AssertEquals("movement1.E9_OA_Depot", sender.MainAddress.PK, movement1.E9_OA_Depot);
			var movement2 = stock2.Movements[0];
			AssertEquals("movement2.E9_MovementType", ContainerMovementTypes.Codes.Load, movement2.E9_MovementType);
			AssertEquals("movement2.E9_JV", voyage.PK, movement2.E9_JV);
			AssertEquals("movement2.E9_ContainerIsEmpty", true, movement2.E9_ContainerIsEmpty);
			AssertEquals("movement2.E9_MovementDate", new ZDateTime(2008, 04, 05, 11, 55, 00), movement2.E9_MovementDate);
			AssertEquals("movement2.E9_OA_Depot", sender.MainAddress.PK, movement2.E9_OA_Depot);
			var movement3 = stock3.Movements[0];
			AssertEquals("movement3.E9_MovementType", ContainerMovementTypes.Codes.Load, movement3.E9_MovementType);
			AssertEquals("movement3.E9_JV", voyage.PK, movement3.E9_JV);
			AssertEquals("movement3.E9_ContainerIsEmpty", false, movement3.E9_ContainerIsEmpty);
			AssertEquals("movement3.E9_MovementDate", new ZDateTime(2008, 04, 05, 12, 24, 00), movement3.E9_MovementDate);
			AssertEquals("movement3.E9_OA_Depot", sender.MainAddress.PK, movement3.E9_OA_Depot);
			var movement4 = stock4.Movements[0];
			AssertEquals("movement4.E9_MovementType", ContainerMovementTypes.Codes.Load, movement4.E9_MovementType);
			AssertEquals("movement4.E9_JV", voyage.PK, movement4.E9_JV);
			AssertEquals("movement4.E9_ContainerIsEmpty", false, movement3.E9_ContainerIsEmpty);
			AssertEquals("movement4.E9_MovementDate", new ZDateTime(2008, 04, 05, 11, 48, 00), movement4.E9_MovementDate);
			AssertEquals("movement4.E9_OA_Depot", sender.MainAddress.PK, movement4.E9_OA_Depot);
			AssertEquals("movement1 should have 1 message", 1, movement1.Messages.Count);
			AssertEquals("movement2 should have 1 message", 1, movement2.Messages.Count);
			AssertEquals("movement3 should have 1 message", 1, movement3.Messages.Count);
			AssertEquals("movement4 should have 1 message", 1, movement4.Messages.Count);
			var message1 = movement1.Messages[0];
			AssertEquals("message1.EM_ApplicationCode", EDIMessage.ApplicationCodes.ContainerManagement, message1.EM_ApplicationCode);
			AssertEquals("message1.EM_MessageType", EDIMessage.ApplicationCodes.ContainerManagement, message1.EM_MessageType);
			AssertEquals("message1.EM_Status", EDIMessage.Status.Recognised, message1.EM_Status);
			AssertEquals("message1.EM_LinkTable", "JobContainerMove", message1.EM_LinkTable);
			AssertMultilineASCIIEquals("message1.EM_MessageText", messageText.Replace("'", "'\r\n"), message1.EM_MessageText.Replace("'", "'\r\n"));
			var message2 = movement2.Messages[0];
			AssertEquals("message2.EM_ApplicationCode", EDIMessage.ApplicationCodes.ContainerManagement, message2.EM_ApplicationCode);
			AssertEquals("message2.EM_MessageType", EDIMessage.ApplicationCodes.ContainerManagement, message2.EM_MessageType);
			AssertEquals("message2.EM_Status", EDIMessage.Status.Recognised, message2.EM_Status);
			AssertEquals("message2.EM_LinkTable", "JobContainerMove", message2.EM_LinkTable);
			AssertMultilineASCIIEquals("message2.EM_MessageText", messageText.Replace("'", "'\r\n"), message2.EM_MessageText.Replace("'", "'\r\n"));
			var message3 = movement3.Messages[0];
			AssertEquals("message3.EM_ApplicationCode", EDIMessage.ApplicationCodes.ContainerManagement, message3.EM_ApplicationCode);
			AssertEquals("message3.EM_MessageType", EDIMessage.ApplicationCodes.ContainerManagement, message3.EM_MessageType);
			AssertEquals("message3.EM_Status", EDIMessage.Status.Recognised, message3.EM_Status);
			AssertEquals("message3.EM_LinkTable", "JobContainerMove", message3.EM_LinkTable);
			AssertMultilineASCIIEquals("message3.EM_MessageText", messageText.Replace("'", "'\r\n"), message3.EM_MessageText.Replace("'", "'\r\n"));
			var message4 = movement4.Messages[0];
			AssertEquals("message4.EM_ApplicationCode", EDIMessage.ApplicationCodes.ContainerManagement, message4.EM_ApplicationCode);
			AssertEquals("message4.EM_MessageType", EDIMessage.ApplicationCodes.ContainerManagement, message4.EM_MessageType);
			AssertEquals("message4.EM_Status", EDIMessage.Status.Recognised, message4.EM_Status);
			AssertMultilineASCIIEquals("message4.EM_MessageText", messageText.Replace("'", "'\r\n"), message4.EM_MessageText.Replace("'", "'\r\n"));
			shipment1.RealContainers.Load();
			shipment2.RealContainers.Load();
			shipment3.RealContainers.Load();
			AssertContainsExactElementsInAnyOrder("should not have added a new container to shipment2 as it's a bill of lading", (c) => c.JC_ContainerNum, Array.Empty<BillOfLadingContainer>(), (BillOfLadingContainer[])shipment2.RealContainers.Find(new ZQuery(JobContainerSchema.JC_ContainerNum, "CCLU4084737")));
			var container4 = (AgencyBookingContainer)shipment1.RealContainers.Find(new ZQuery(JobContainerSchema.JC_ContainerNum, "CCLU4798734"))[0];
			AssertEquals("container1.JC_ContainerNum", "GESU4760670", container1.JC_ContainerNum);
			AssertEquals("container1.JC_GrossWeight", 24500m, container1.JC_GrossWeight);
			AssertEquals("container1.JC_RC", "40HC", container1.Container.RC_Code);
			AssertEquals("container3.JC_ContainerNum", "CCLU8601144", container3.JC_ContainerNum);
			AssertEquals("container3.JC_GrossWeight", 3000m, container3.JC_GrossWeight);
			AssertEquals("container3.JC_RC", "20RE", container3.Container.RC_Code);
			AssertEquals("container4.JC_ContainerNum", "CCLU4798734", container4.JC_ContainerNum);
			AssertEquals("container4.JC_GrossWeight", 6800m, container4.JC_GrossWeight);
			AssertEquals("container4.JC_RC", "40NOR", container4.Container.RC_Code);
			AssertCollectionContains("atleast one of the messages must be the original.", message.PK, new ZGuid[] { message1.PK, message2.PK, message3.PK, message4.PK });
			const string expectedEmailDef_Asc = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Load COARRI Message From Random Wharf - Containers Without Warnings\r\n" + "CC:\r\n" + "  ack@freadnet.org\r\n" + "";
			const string expectedEmailDef_Dsc = "FROM:\r\n" + "  default@wisetechglobal.com\r\n" + "SUBJECT:\r\n" + "  Processed Load COARRI Message From Random Wharf - Containers With Warnings\r\n" + "CC:\r\n" + "  dsc@freadnet.org\r\n" + "";
			AssertContainsExactElementsInAnyOrder(new string[] { expectedEmailDef_Asc, expectedEmailDef_Dsc }, ConvertAll(Env.OutgoingMailManager.EmailsCreated, FormatEmail));
			IDictionary<string, string> mappings = new SortedDictionary<string, string>();
			mappings["(*URL1*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock1.PK.ToGuid());
			mappings["(*URL2*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock2.PK.ToGuid());
			mappings["(*URL3*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock3.PK.ToGuid());
			mappings["(*URL4*)"] = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.AgencyContainerManager, stock4.PK.ToGuid());
			var ackEmail = FindEmail((p) => p.CCRecipients.Contains("ack@freadnet.org"));
			var dscEmail = FindEmail((p) => p.CCRecipients.Contains("dsc@freadnet.org"));
			AssertMultilineASCIIEquals("expected body", TestFileHelper.COARRI.GetResultBody2_ACK(mappings), ackEmail.Body);
			AssertMultilineASCIIEquals("expected body", TestFileHelper.COARRI.GetResultBody2_DSC(mappings), dscEmail.Body);
			AssertContainsExactElementsInAnyOrder("asc email should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(ackEmail.Attachments, (AttachmentDef a) => a.DisplayName));
			AssertContainsExactElementsInAnyOrder("dsc email should attach a copy of the response message.", new string[] { "message.edi" }, ConvertAllCast(dscEmail.Attachments, (AttachmentDef a) => a.DisplayName));
			var ascAttachment = FindAttachment(ackEmail, "message.edi");
			var dscAttachment = FindAttachment(dscEmail, "message.edi");
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(ascAttachment.Data));
			AssertMultilineASCIIEquals("attachment content", messageText, Encoding.UTF8.GetString(dscAttachment.Data));
		}

		public void TestProcessCODECOMessage_FailsWhenRegistryIsOff()
		{
			#region Set Up CODECO Message
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + "GID+1'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+++5'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
			"CNT+16:1'" + "UNT+14+1'" + "";
			OrgHeader sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Depot";
			sender.OH_IsUnpackDepot = true;
			OrgCusCode codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "SHCB1";
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4214635";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9290127";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0033";
			Factory.Save();
			#endregion
			AssertEquals("Pre-condition: expected no email to have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Pre-condition: expected no user logs", 0, Logger.UserLogStrings.Count);
			AgencyRegistry.Instance.AllowDirectCODECOCOARRICMMMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			ContainerManagementEDIMessage message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			ContainerMovementCountryProcessorFactory.NewProcessorOverride processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status should be 'failed' as the registry to allow processing CODECO is not enabled", EDIMessage.Status.Failed, message.EM_Status);
			AssertEquals("Expected no email to have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			string failedLogMessage = "This message has failed as the registry setting to allow direct sending of Container Management messages with CODECO/COARRI has not been enabled. Please send CM messages through eHub/eAdaptor or contact Cargowise to enable CODECO/COARRI.";
			AssertEquals("Expected one log to be added", 1, Logger.UserLogStrings.Count);
			AssertEquals(failedLogMessage.Trim(), Logger.UserLogStrings[0].Trim());
			AgencyRegistry.Instance.AllowDirectCODECOCOARRICMMMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status should now be fine as the registry setting has been enabled", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("Expected an email to have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestProcessCOARRIMessage_FailsWhenRegistryIsOff()
		{
			#region Set Up COARRI Message
			const string messageText = // header
			"UNH+718+COARRI:D:95B:UN:ITG10'" + "BGM+98+13910559+9'" + "TDT+20+0084S+1++CSC:172:184+++9224336:146'" + "LOC+11+AUSYD'" + "NAD+CA+CSC:160:184'" + "NAD+MS+CTLPB:160:184'" + // container 1
			"EQD+CN+CCLU4798734+42R0:102:5+++5'" + "DTM+203:200804051148:203'" + "LOC+7+AUSYD'" + "LOC+9+CNSHK'" + "LOC+11+AUSYD'" + "LOC+147+0501190::5'" + "MEA+AAE+G+KGM:6800'" + "FTX+AAA+++GEN'" + "NAD+CF+CSC:160:184'" + // footer
			"CNT+16:4'" + "UNT+35+718'" + "";
			OrgHeader sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Random Wharf";
			sender.OH_IsSeaCTO = true;
			OrgCusCode codes = sender.CustomsCodes.AddNew();
			codes.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			codes.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			codes.OK_CustomsRegNo = "CTLPB";
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CCLU4798734";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "CSCL MELBOURNE";
			vessel.RV_LloydsNumber = "9224336";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "0084S";
			Factory.Save();
			#endregion
			AssertEquals("Pre-condition: expected no email to have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Pre-condition: expected no user logs", 0, Logger.UserLogStrings.Count);
			AgencyRegistry.Instance.AllowDirectCODECOCOARRICMMMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestHelper.SetCMMEmailAddresses(Factory, "ack@freadnet.org", "dsc@freadnet.org", "err@freadnet.org");
			ContainerManagementEDIMessage message = CreateIncommingMessage(messageText);

			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			ContainerMovementCountryProcessorFactory.NewProcessorOverride processorFactory = processorFactoryMock.Object;

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status should be 'failed' as the registry to allow processing COARRI is not enabled", EDIMessage.Status.Failed, message.EM_Status);
			AssertEquals("Expected no email to have been sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			string failedLogMessage = "This message has failed as the registry setting to allow direct sending of Container Management messages with CODECO/COARRI has not been enabled. Please send CM messages through eHub/eAdaptor or contact Cargowise to enable CODECO/COARRI.";
			AssertEquals("Expected one log to be added", 1, Logger.UserLogStrings.Count);
			AssertEquals(failedLogMessage.Trim(), Logger.UserLogStrings[0].Trim());
			AgencyRegistry.Instance.AllowDirectCODECOCOARRICMMMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				Processor.ProcessMessage(message);
			}

			AssertEquals("EM_Status", EDIMessage.Status.Recognised, message.EM_Status);
			AssertEquals("Expected an email to have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			AgencyRegistry.Instance.AllowDirectCODECOCOARRICMMMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
		AgencyShipmentContainer AddSealedContainers(AgencyShipment shipment, RefContainerStock stock, string seal1 = "", string seal2 = "", string seal3 = "")
		{
			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = stock.R6_ContainerNum;
			container.JC_RC = stock.R6_RC;
			container.JC_SealNum = seal1;
			container.JC_AdditionalSealNum = seal2;
			container.JC_Additional2SealNum = seal3;
			return container;
		}

		ContainerManagementEDIMessage CreateIncommingMessage(string messageText)
		{
			var message = Factory.New<ContainerManagementEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ContainerManagement;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = messageText;
			return message;
		}

		EdifactCMMMessageProcessor processor;
		EdifactCMMMessageProcessor Processor => processor ?? (processor = new EdifactCMMMessageProcessor(Logger));

		LoggingInformation logger;
		LoggingInformation Logger => logger ?? (logger = new LoggingInformation());
	}
}
