using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Events = Enterprise.ZArchitecture.Business.Events;
using MessageType = Enterprise.Customs.Common.AU.PRAMessageTypeConstants.MessageType;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(PRAMessageApplicator))]
	public class PRAMessageApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestValidationErrorLevel()
		{
			var praMessageApplicator = new PRAMessageApplicator(MessageType.Submit, Settings);

			Settings.ErrorBehaviour = PRASettings.Codes.Skip;
			AssertEquals(praMessageApplicator.ValidationErrorLevel, OperationalActionLogErrorLevel.Warning);

			Settings.ErrorBehaviour = PRASettings.Codes.Abort;
			AssertEquals(praMessageApplicator.ValidationErrorLevel, OperationalActionLogErrorLevel.Error);
		}

		public void TestCreatePRAMessageWithReSubmitSuccessful()
		{
			using (Env.SetTemporaryUserContext(newStaff.GS_LoginName, newBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var setting1 = new PRASettings(Factory);
				var consol1 = CreateConsolWithRelevantBusinessObject("C0000002", "1230321", "HPGVSL", "CONT0000011", "CONT0000012");
				Factory.Save();

				var currentPRAUsageCount = PRAUsageCount;
				var log1 = new DummyOperationalActionSectionLog();
				var applicatorSubmit = new PRAMessageApplicator(MessageType.Submit, setting1);

				AssertEquals("Pre-conditions", 2, consol1.Containers.Count);
				AssertEquals("No PRA Messages Have Been Sent.", consol1.Containers[0].CurrentPRAStatus);
				AssertEquals("No PRA Messages Have Been Sent.", consol1.Containers[1].CurrentPRAStatus);

				applicatorSubmit.Apply(log1, new BusinessObject[] { consol1 });

				AssertContains("[HL C0000002 : CONT0000011] - PRA Message generated.", log1.MessagesString());
				AssertContains("[HL C0000002 : CONT0000012] - PRA Message generated.", log1.MessagesString());
				AssertEquals("PRA Submit Message Sent but not responded to yet.", consol1.Containers[0].CurrentPRAStatus);
				AssertEquals("PRA Submit Message Sent but not responded to yet.", consol1.Containers[1].CurrentPRAStatus);

				currentPRAUsageCount += 2;
				AssertEquals("PRA license consumption should increased by 2", currentPRAUsageCount, PRAUsageCount);

				var eventFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageSent.Code);
				var event1 = consol1.Containers[0].Logs.Find(eventFilter);
				var event2 = consol1.Containers[1].Logs.Find(eventFilter);

				AssertEquals("should have added 1 Message Sent event", 1, event1.Length);
				AssertEquals("should have added 1 Message Sent event", 1, event2.Length);
				AssertEquals("Event parameter", "|DEP=1-stop|MST=PRA", event1[0].SL_Reference);
				AssertEquals("Event parameter", "|DEP=1-stop|MST=PRA", event2[0].SL_Reference);

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				var setting2 = new PRASettings(factory2);
				var consol2 = factory2.Load<ForwardingConsol>(consol1.PK);
				var log2 = new DummyOperationalActionSectionLog();
				var applicatorReSubmit = new PRAMessageApplicator(MessageType.ReSubmit, setting2);
				applicatorReSubmit.Apply(log2, new BusinessObject[] { consol2 });

				AssertContains("[HL C0000002 : CONT0000011] - PRA Message generated.", log2.MessagesString());
				AssertContains("[HL C0000002 : CONT0000012] - PRA Message generated.", log2.MessagesString());
				AssertEquals("PRA Submit Message Sent but not responded to yet.", consol2.Containers[0].CurrentPRAStatus);
				AssertEquals("PRA Submit Message Sent but not responded to yet.", consol2.Containers[1].CurrentPRAStatus);
				AssertEquals("PRA license consumption not should not increase.", currentPRAUsageCount, PRAUsageCount);
			}
		}

		public void TestCreatePRAMessageWithSubmitSuccessfulWithSkipErrorShipment()
		{
			using (Env.SetTemporaryUserContext(newStaff.GS_LoginName, newBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var setting1 = new PRASettings(Factory);
				setting1.ErrorBehaviour = PRASettings.Codes.Skip;

				var consol1 = CreateConsolWithRelevantBusinessObject("C0000002", "1230321", "HPGVSL", "CONT0000011", "CONT0000012");
				consol1.Containers[0].JC_SealNum = "";

				var consol2 = CreateConsolWithRelevantBusinessObject("C0000003", "123321", "HPGXXL", "CONT0000021", "CONT0000022");
				Factory.Save();

				var currentPRAUsageCount = PRAUsageCount;
				var log1 = new DummyOperationalActionSectionLog();
				var applicatorSubmit = new PRAMessageApplicator(MessageType.Submit, setting1);

				AssertEquals("Pre-conditions", 2, consol1.Containers.Count);
				AssertEquals("No PRA Messages Have Been Sent.", consol1.Containers[0].CurrentPRAStatus);
				AssertEquals("No PRA Messages Have Been Sent.", consol1.Containers[1].CurrentPRAStatus);
				AssertEquals("No PRA Messages Have Been Sent.", consol2.Containers[0].CurrentPRAStatus);
				AssertEquals("No PRA Messages Have Been Sent.", consol2.Containers[1].CurrentPRAStatus);

				applicatorSubmit.Apply(log1, new BusinessObject[] { consol1, consol2 });

				AssertContains("[HL C0000002 : CONT0000011]\r\nSeal Number Not Entered. (Containers Tab in the Grid)\r\n",
					log1.MessagesString());
				AssertContains("[HL C0000002 : CONT0000012]\r\nSkip shipment that have errors or message errors.",
					log1.MessagesString());
				AssertEquals("No PRA Messages Have Been Sent.", consol1.Containers[0].CurrentPRAStatus);
				AssertEquals("No PRA Messages Have Been Sent.", consol1.Containers[1].CurrentPRAStatus);

				AssertContains("[HL C0000003 : CONT0000021] - PRA Message generated.", log1.MessagesString());
				AssertContains("[HL C0000003 : CONT0000022] - PRA Message generated.", log1.MessagesString());
				AssertEquals("PRA Submit Message Sent but not responded to yet.", consol2.Containers[0].CurrentPRAStatus);
				AssertEquals("PRA Submit Message Sent but not responded to yet.", consol2.Containers[1].CurrentPRAStatus);

				currentPRAUsageCount += 2;
				AssertEquals("PRA license consumption should increased by 2", currentPRAUsageCount, PRAUsageCount);
			}
		}

		public void TestCreatePRAMessageWithSubmitMultipleTimes()
		{
			using (Env.SetTemporaryUserContext(newStaff.GS_LoginName, newBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var setting1 = new PRASettings(Factory);
				var consol1 = CreateConsolWithRelevantBusinessObject("C0000002", "1230321", "HPGVSL", "CONT0000011", "CONT0000012");
				Factory.Save();

				var currentPRAUsageCount = PRAUsageCount;
				var log1 = new DummyOperationalActionSectionLog();
				var applicatorSubmit = new PRAMessageApplicator(MessageType.Submit, setting1);

				AssertEquals("Pre-conditions", 2, consol1.Containers.Count);
				AssertEquals("No PRA Messages Have Been Sent.", consol1.Containers[0].CurrentPRAStatus);
				AssertEquals("No PRA Messages Have Been Sent.", consol1.Containers[1].CurrentPRAStatus);

				applicatorSubmit.Apply(log1, new BusinessObject[] { consol1 });

				AssertContains("[HL C0000002 : CONT0000011] - PRA Message generated.", log1.MessagesString());
				AssertContains("[HL C0000002 : CONT0000012] - PRA Message generated.", log1.MessagesString());
				AssertEquals("PRA Submit Message Sent but not responded to yet.", consol1.Containers[0].CurrentPRAStatus);
				AssertEquals("PRA Submit Message Sent but not responded to yet.", consol1.Containers[1].CurrentPRAStatus);

				currentPRAUsageCount += 2;
				AssertEquals("PRA sent, License consumption increased by 2", currentPRAUsageCount, PRAUsageCount);

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				var setting2 = new PRASettings(factory2);
				var consol2 = factory2.Load<ForwardingConsol>(consol1.PK);
				var log2 = new DummyOperationalActionSectionLog();
				applicatorSubmit = new PRAMessageApplicator(MessageType.Submit, setting2);
				applicatorSubmit.Apply(log2, new BusinessObject[] { consol2 });

				AssertContains("INFO: [HL C0000002 : CONT0000011] - Container is waiting for response. Ignored.",
					log2.MessagesString());
				AssertContains("INFO: [HL C0000002 : CONT0000012] - Container is waiting for response. Ignored.",
					log2.MessagesString());
				AssertEquals("PRA resent, license consumption not changed.", currentPRAUsageCount, PRAUsageCount);
			}
		}

		public void TestCreatePRAMessageWithSubmitErrors()
		{
			ForwardingConsol consol = CreateConsol();
			Factory.Save();

			messageType = MessageType.Submit;
			Settings.ErrorBehaviour = PRASettings.Codes.Skip;

			string expectedLog = string.Format(@"WARNING: [HL {0} : {1}]
Shipping Line Booking Reference. (Consol Tab or Container Tab)
ECN or CRN. (Against Consol or Shipment as applicable)
   NB: If you are relying on the ECN from the Shipment, make sure you have
   this container selected in the 'Packing' section of the relevant Shipment.
   Important: 1-Stop can only accept one reference per container, so if you 
   have multiple shipments in the one container with individual CAN's, you will 
   have to submit a CRN for the Consol to get a single CAN for the contents 
   of the whole container before sending a PRA.
Carrier 1-Stop Code. (Registration Numbers on the Config Tab in the Carrier Master File)
Vessel Name. (Consol Tab)
Voyage. (Consol Tab)
Lloyds Number. (Vessel Master File for selected vessel on Consol Tab)
Departure CTO 1-Stop Code. (Registration Numbers on the Config Tab in the CTO Master File)
ISO Container Type. (Containers Tab in the Grid)
Commodity 1-Stop Code. (Containers Tab in the Grid)
Container Gross Weight. (Containers Tab in the Grid)
Seal Number Not Entered. (Containers Tab in the Grid)
Container Verified Weight Method must be CNT or PKG. (VGM Tab on the Containers Tab)
", consol.JK_UniqueConsignRef, consol.Containers[0].ContainerCode);

			ApplyApplicator(new BusinessObject[] { consol }, expectedLog);
		}

		public void TestCreatePRAMessageWithNoContainer()
		{
			var consol = CreateConsolWithRelevantBusinessObject("C0000002", "1230321", "HPGVSL", "", "");
			Factory.Save();

			var currentPRAUsageCount = PRAUsageCount;
			messageType = MessageType.Submit;
			Settings.ErrorBehaviour = PRASettings.Codes.Skip;

			AssertEquals("Precondition", 0, consol.Containers.Count);

			string expectedLog = "INFO: [HL C0000002] : No containers selected.";
			ApplyApplicator(new BusinessObject[] { consol }, expectedLog);

			AssertEquals("No PRA Messages have been generated, license consumption not changed.", currentPRAUsageCount, PRAUsageCount);
		}

		int PRAUsageCount
		{
			get
			{
				var logFilter = new ZDBOnlyQuery(typeof(StmActivityLog));
				logFilter.AddToFilter(StmActivityLogSchema.S7_FormCaption, Env.Licence.PRAMessagingPerTransaction.Name);
				return Factory.GetDatabaseCount(typeof(StmActivityLog), logFilter);
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PRAMessageApplicator(MessageType, Settings);
		}

		MessageType MessageType
		{
			get { return messageType; }
		}
		MessageType messageType;

		PRASettings Settings
		{
			get { return settings ?? (settings = new PRASettings(Factory)); }
		}
		PRASettings settings;

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "consol1";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT0000001";

			return consol;
		}

		ForwardingConsol CreateConsolWithRelevantBusinessObject(ZString uniqueConsignRef, ZString lloydsNumber, ZString vesselCode, ZString containerNumber1, ZString containerNumber2)
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode1 = shippingLine.CustomsCodes.AddNew();
			cusCode1.OK_CustomsRegNo = "HPG";
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			cusCode1.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;

			var cTOAddress = Factory.New<OrgAddress>();
			cTOAddress.OA_Address1 = "222 AVENGERS STREET";
			cTOAddress.OA_PostCode = "1234";
			cTOAddress.OA_State = "NSW";
			cTOAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			cTOAddress.OA_Code = "NEWCTO";

			var cTO = Factory.NewWithValidTestData<OrgHeader>();
			cTOAddress.OA_OH = cTO.PK;
			var cusCode2 = cTO.CustomsCodes.AddNew();
			cusCode2.OK_CustomsRegNo = "ASLPB";
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			cusCode2.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
			cusCode2.OK_OA_PremisesAddress = cTOAddress.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = uniqueConsignRef;
			consol.JK_BookingReference = "ShippingLineBookingRef001";
			consol.SetDefaultShippingLineAddress(shippingLine);
			consol.JK_OA_DepartureCTOAddress = cTOAddress.PK;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = lloydsNumber;
			vessel.RV_Name = vesselCode;

			var transport = consol.Transports[0];
			transport.JW_Vessel = vessel.RV_FK;
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_VoyageFlight = "V102";

			var shipment = consol.Shipments.AddNew();
			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CAN;
			shipment.CustomsEntryNumber = "ECNOrCan001";

			if (!containerNumber1.IsEmpty)
			{
				var container1 = consol.Containers.AddNew();
				container1.JC_ContainerNum = containerNumber1;
				container1.JC_SealNum = "Seal0011";
				container1.JC_GrossWeight = 2500m;
				container1.JC_TareWeight = 47.236m;
				container1.JC_DunnageWeight = 162.463m;
				container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container1.JC_RH_NKContainerCommodityCode = "GENL";
				container1.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
				container1.JC_GrossWeightVerificationDateTime = new ZDateTime(2016, 5, 1);
				container1.GrossWeightVerifiedByAddress.E2_AddressOverride = true;
				container1.GrossWeightVerifiedByAddress.E2_CompanyName = "CONTAINER WEIGHTING LTD";
				container1.GrossWeightVerifiedByAddress.E2_Address1 = "Address 1";
				container1.GrossWeightVerifiedByAddress.E2_City = "Mascot";
				container1.GrossWeightVerifiedByAddress.E2_Postcode = "2025";
				container1.GrossWeightVerifiedByAddress.E2_RN_NKCountryCode = "AU";

				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.Containers.Add(container1);
			}

			if (!containerNumber1.IsEmpty)
			{
				var container2 = consol.Containers.AddNew();
				container2.JC_ContainerNum = containerNumber2;
				container2.JC_SealNum = "Seal0012";
				container2.JC_GrossWeight = 2500m;
				container2.JC_TareWeight = 47.236m;
				container2.JC_DunnageWeight = 162.463m;
				container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container2.JC_RH_NKContainerCommodityCode = "GENL";
				container2.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
				container2.JC_GrossWeightVerificationDateTime = new ZDateTime(2016, 5, 1);
				container2.GrossWeightVerifiedByAddress.E2_AddressOverride = true;
				container2.GrossWeightVerifiedByAddress.E2_CompanyName = "CONTAINER WEIGHTING LTD";
				container2.GrossWeightVerifiedByAddress.E2_Address1 = "Address 1";
				container2.GrossWeightVerifiedByAddress.E2_City = "Mascot";
				container2.GrossWeightVerifiedByAddress.E2_Postcode = "2025";
				container2.GrossWeightVerifiedByAddress.E2_RN_NKCountryCode = "AU";

				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.Containers.Add(container2);
			}

			return consol;
		}

		GlbBranch newBranch;
		GlbStaff newStaff;

		protected override void SetUp()
		{
			base.SetUp();
			GlbStaff.CurrentUser.GS_FullName = "Developer";
			GlbStaff.CurrentUser.GS_EmailAddress = "Developer@edi.com.au";
			CreateNewCompanyBranch();
		}

		void CreateNewCompanyBranch()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "TTT";
			orgProxy.OH_FullName = "DEMO ORGANISATION";
			orgProxy.MainAddress.OA_Address1 = "111 Demo St";

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "XXY";
			newCompany.GC_Name = "XXY Company";
			newCompany.GC_RN_NKCountryCode = "AU";
			newCompany.GC_RX_NKLocalCurrency = "AUD";
			newCompany.GC_OH_OrgProxy = orgProxy.PK;

			newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "XYZ";
			newBranch.GB_BranchName = "XYZ Branch";
			newBranch.GB_RL_NKHomePort = "AUSYD";
			newBranch.GB_GC = newCompany.PK;

			newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_LoginName = "DEV";
			newStaff.GS_Code = "DEV";
			newStaff.GS_FullName = "Developer";
			newStaff.GS_EmailAddress = "Developer@edi.com.au";
			newStaff.GS_GB_HomeBranch = newBranch.PK;

			Factory.Save();
		}

		#endregion

	}
}
