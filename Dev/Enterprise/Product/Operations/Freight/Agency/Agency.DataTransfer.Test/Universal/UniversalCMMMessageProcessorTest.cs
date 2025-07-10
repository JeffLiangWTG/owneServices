using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.DataTransfer.MessageProcessing;
using Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	[TestedType(typeof(UniversalCMMMessageProcessor))]
	internal class UniversalCMMMessageProcessorTest : CMMMessageProcessorTest
	{
		public void CreateInstance()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new UniversalCMMMessageProcessor(null));
			AssertNoExceptionThrown(() => new UniversalCMMMessageProcessor(Factory));
		}

		public override void TestProcessCODECOMessage_Ack()
		{
			int numberOfEDIMessages = Factory.GetDatabaseCount(typeof(EDIMessage));
			var container20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var containerStock = Factory.New<RefContainerStock>();
			containerStock.R6_ContainerNum = "AAAA00000007";
			containerStock.R6_RC = container20GP.PK;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "Lloyds1";
			vessel.RV_Name = "XXX";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "Voyage1";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Rumburak";
			sender.OH_RL_NKClosestPort = "AUSYD";
			sender.OH_IsPackDepot = true;
			var code = sender.CustomsCodes.AddNew();
			code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			code.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			code.OK_CustomsRegNo = "eHubbertId";
			Factory.Save();
			var processor = new UniversalCMMMessageProcessor(Factory);
			var universalEvent = new UniversalEvent { EventTime = new ZDateTimeOffset(2012, 04, 01), EventType = Events.GateInCode, EventReference = "Blah", ContextCollection = new List<Context> { new Context { Type = "DepotCode", Value = "eHubbertId" }, new Context { Type = "LloydsNumber", Value = "Lloyds1" }, new Context { Type = "VoyageNumber", Value = "Voyage1" }, new Context { Type = "ContainerNumber", Value = "AAAA00000007" }, new Context { Type = "ContainerISOCode", Value = container20GP.RC_ISOType }, new Context { Type = "PositioningDateTime", Value = new ZDateTime(2012, 03, 31, 10, 30, 11).ToStandardDateTimeString() } }, };
			processor.ProcessEvent(universalEvent);
			Factory.Save();
			AssertEquals("One new message created in the database", numberOfEDIMessages + 1, Factory.GetDatabaseCount(typeof(EDIMessage)));
			const string expectedEmailDef = @"FROM:
  Default@edi.com.au
SUBJECT:
  Processed Gate-In CODECO Message From Rumburak - Containers Without Warnings
CC:
  ack1@example.com
";
			AssertMultilineASCIIEquals("email", expectedEmailDef, string.Join(System.Environment.NewLine, Env.OutgoingMailManager.EmailsCreated.Select(FormatEmail)));
			AssertEquals(1, containerStock.Movements.Count);
			AssertEquals(voyage.PK, containerStock.Movements[0].E9_JV);
			AssertEquals("DGI", containerStock.Movements[0].E9_MovementType);
			AssertEquals(false, containerStock.Movements[0].E9_ContainerIsEmpty);
			AssertEquals(new ZDateTime(2012, 03, 31, 10, 30, 0), containerStock.Movements[0].E9_MovementDate);
			AssertEquals(sender.MainAddress.PK, containerStock.Movements[0].E9_OA_Depot);
			AssertEquals(1, containerStock.Movements[0].Messages.Count);
			AssertEquals("EDIMessage attached to movement", "Event Type: GIN'" + "Event Reference: Blah'" + "Event Time: 01-Apr-2012 00:00:00'" + "Depot Code - eHubbertId'" + "Lloyds Number - Lloyds1'" + "Voyage Number - Voyage1'" + "Container Number - AAAA00000007'" + "Container ISO Code - 22G0'" + "Positioning Date Time - 31-Mar-2012 10:30:11'", containerStock.Movements[0].Messages[0].EM_MessageText.ToString());
		}

		public override void TestProcessCODECOMessage_Warning()
		{
			int numberOfEDIMessages = Factory.GetDatabaseCount(typeof(EDIMessage));
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Rumburak";
			sender.OH_RL_NKClosestPort = "AUSYD";
			sender.OH_IsPackDepot = true;
			var code = sender.CustomsCodes.AddNew();
			code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			code.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			code.OK_CustomsRegNo = "eHubbertId";
			Factory.Save();
			var processor = new UniversalCMMMessageProcessor(Factory);
			var universalEvent = new UniversalEvent { EventTime = new ZDateTimeOffset(2012, 02, 27), EventType = Events.GateInCode, EventReference = "Blah", ContextCollection = new List<Context> { new Context { Type = "DepotCode", Value = "eHubbertId" }, new Context { Type = "LloydsNumber", Value = "lloydsNumberValue" }, new Context { Type = "VoyageNumber", Value = "voyageNumberValue" } } };
			processor.ProcessEvent(universalEvent);
			Factory.Save();
			AssertEquals("No new messages have been saved in the database", numberOfEDIMessages, Factory.GetDatabaseCount(typeof(EDIMessage)));
			const string expectedEmailDef = @"FROM:
  Default@edi.com.au
SUBJECT:
  Processed Gate-In CODECO Message From Rumburak
CC:
  impediment1@example.com
";
			AssertMultilineASCIIEquals("email", expectedEmailDef, string.Join(System.Environment.NewLine, Env.OutgoingMailManager.EmailsCreated.Select(FormatEmail)));
		}

		public override void TestProcessCODECOMessage_Fail()
		{
			var processor = new CMMUniversalEventMessageProcessorForTest(Factory);
			var adapter = new Mock<ICMMProcessingAdapter>();
			adapter.Setup(a => a.Load()).Callback(new Action(() =>
			{
				throw new Exception("zoom-ba");
			}));
			adapter.Setup(a => a.MessageSenderReference).Returns("joe the postman");
			adapter.Setup(a => a.Factory).Returns(Factory);
			adapter.Setup(a => a.MessageText).Returns("***message***");
			processor.GetCMMProcessingAdapterImpl = message => adapter.Object;
			processor.ProcessEvent(new UniversalEvent());
			const string expectedEmailDef = @"FROM:
  Default@edi.com.au
SUBJECT:
  Error Processing CODECO/COARRI Message
CC:
  err1@example.com
";
			AssertMultilineASCIIEquals("email", expectedEmailDef, string.Join(System.Environment.NewLine, Env.OutgoingMailManager.EmailsCreated.Select(FormatEmail)));
		}

		#region Implementation
		class CMMUniversalEventMessageProcessorForTest : UniversalCMMMessageProcessor
		{
			public CMMUniversalEventMessageProcessorForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public Func<EDIMessage, ICMMProcessingAdapter> GetCMMProcessingAdapterImpl { get; set; }

			protected override ICMMProcessingAdapter GetCMMProcessingAdapter(EDIMessage message)
			{
				return GetCMMProcessingAdapterImpl != null ? GetCMMProcessingAdapterImpl(message) : null;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			AgencyRegistry.Instance.CMMErrorEmailGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateErrorGroup().PK.ToGuid());
			AgencyRegistry.Instance.CMMDiscrepanciesEmailGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateImpedimentGroup().PK.ToGuid());
			AgencyRegistry.Instance.CMMAcknowledgementEmailGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateAcknowledgementGroup().PK.ToGuid());
			Factory.Save();
		}

		GlbGroup CreateAcknowledgementGroup()
		{
			var acknowledgementGroup = Factory.New<GlbGroup>();
			acknowledgementGroup.GG_Code = "ACK";
			var staff = acknowledgementGroup.Staff.AddNew();
			staff.GS_EmailAddress = "ack1@example.com";
			staff.GS_IsActive = true;
			staff.GS_LoginName = "ack.one.guy";
			staff.GS_Code = "AK1";
			return acknowledgementGroup;
		}

		GlbGroup CreateImpedimentGroup()
		{
			var impedimentGroup = Factory.New<GlbGroup>();
			impedimentGroup.GG_Code = "IMP";
			var staff = impedimentGroup.Staff.AddNew();
			staff.GS_EmailAddress = "impediment1@example.com";
			staff.GS_IsActive = true;
			staff.GS_LoginName = "impediment.one.guy";
			staff.GS_Code = "IM1";
			return impedimentGroup;
		}

		GlbGroup CreateErrorGroup()
		{
			var errorGroup = Factory.New<GlbGroup>();
			errorGroup.GG_Code = "ERR";
			var staff1 = errorGroup.Staff.AddNew();
			staff1.GS_EmailAddress = "err1@example.com";
			staff1.GS_IsActive = true;
			staff1.GS_LoginName = "error.one.guy";
			staff1.GS_Code = "ER1";
			var staff2 = errorGroup.Staff.AddNew();
			staff2.GS_EmailAddress = "";
			staff2.GS_IsActive = true;
			staff2.GS_LoginName = "error.two.guy";
			staff2.GS_Code = "ER2";
			return errorGroup;
		}
		#endregion
	}
}
