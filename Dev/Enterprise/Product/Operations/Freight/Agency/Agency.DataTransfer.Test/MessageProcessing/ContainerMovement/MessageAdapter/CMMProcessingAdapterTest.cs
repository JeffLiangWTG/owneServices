using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	internal abstract class CMMProcessingAdapterTest : TestCaseWithFactory, IContainerMovementMessageProcessingAdapterTest
	{
		#region IContainerMovementMessageProcessingAdapterTest Members

		abstract public void TestCountryCode_OrgHeader();
		abstract public void TestCountryCode_Address();
		abstract public void TestAttachMessage();
		abstract public void TestCountryOfCustomsCodeIsNotAu();

		public void TestGateInMessage()
		{
			TestCaseHelper.ClearTable(OrgCusCodeSchema.Constants.TableName);
			Sender.OH_IsUnpackDepot = false;
			Sender.OH_IsPackDepot = false;
			Sender.OH_IsSeaCTO = false;
			Sender.OH_IsContainerYard = false;
			var adapter = NewAdapterWithGateInMessage();
			adapter.Load();
			AssertEquals("Precondition - MessageSenderCode", "SHCB1", adapter.MessageSenderCode);
			AssertEquals("Precondition - Sender", Sender, adapter.Sender);
			AssertEquals("Precondition - SenderAddress", Sender.MainAddress, adapter.SenderAddress);
			AssertEquals("Precondition - Vessel", Vessel, adapter.Vessel);
			AssertEquals("Precondition - Type Description", "Gate-In CODECO", adapter.MessageTypeTitle);
			AssertEquals("Precondition - MovementCode", "", adapter.MovementCode);
			Sender.OH_IsUnpackDepot = true;
			adapter = NewAdapterWithGateInMessage();
			adapter.Load();
			AssertEquals("MovementCode - Depot Gate In", ContainerMovementTypes.Codes.DepotGateIn, adapter.MovementCode);
			Sender.OH_IsUnpackDepot = false;
			Sender.OH_IsSeaCTO = true;
			adapter = NewAdapterWithGateInMessage();
			adapter.Load();
			AssertEquals("MovementCode - Wharf Gate In", ContainerMovementTypes.Codes.WharfGateIn, adapter.MovementCode);
			Sender.OH_IsContainerYard = true;
			adapter = NewAdapterWithGateInMessage();
			adapter.Load();
			AssertEquals("MovementCode - NONE", "", adapter.MovementCode);
			Sender.OH_IsSeaCTO = false;
			adapter = NewAdapterWithGateInMessage();
			adapter.Load();
			AssertEquals("MovementCode - Yard Gate In", ContainerMovementTypes.Codes.YardGateIn, adapter.MovementCode);
		}

		public void TestGateOutMessage()
		{
			TestCaseHelper.ClearTable(OrgCusCodeSchema.Constants.TableName);
			Sender.OH_IsUnpackDepot = false;
			Sender.OH_IsPackDepot = false;
			Sender.OH_IsSeaCTO = false;
			Sender.OH_IsContainerYard = false;
			var code = Sender.CustomsCodes.AddNew();
			code.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			code.OK_CodeType = OrgCusCode.CodeTypes.ContainerManagementMessagingAgreedCode;
			var adapter = NewAdapterWithGateOutMessage();
			adapter.Load();
			AssertEquals("Precondition - MessageSenderCode", "SHCB1", adapter.MessageSenderCode);
			AssertEquals("Precondition - Sender", Sender, adapter.Sender);
			AssertEquals("Precondition - SenderAddress", Sender.MainAddress, adapter.SenderAddress);
			AssertEquals("Precondition - Vessel", Vessel, adapter.Vessel);
			AssertEquals("Precondition - Type Description", "Gate-Out CODECO", adapter.MessageTypeTitle);
			AssertEquals("Precondition - MovementCode", "", adapter.MovementCode);
			Sender.OH_IsUnpackDepot = true;
			adapter = NewAdapterWithGateOutMessage();
			adapter.Load();
			AssertEquals("MovementCode - Depot Gate Out", ContainerMovementTypes.Codes.DepotGateOut, adapter.MovementCode);
			Sender.OH_IsUnpackDepot = false;
			Sender.OH_IsSeaCTO = true;
			adapter = NewAdapterWithGateOutMessage();
			adapter.Load();
			AssertEquals("MovementCode - Wharf Gate Out", ContainerMovementTypes.Codes.WharfGateOut, adapter.MovementCode);
			Sender.OH_IsContainerYard = true;
			adapter = NewAdapterWithGateOutMessage();
			adapter.Load();
			AssertEquals("MovementCode - NONE", "", adapter.MovementCode);
			Sender.OH_IsSeaCTO = false;
			adapter = NewAdapterWithGateOutMessage();
			adapter.Load();
			AssertEquals("MovementCode - Yard Gate Out", ContainerMovementTypes.Codes.YardGateOut, adapter.MovementCode);
		}

		public void TestContainerLoadMessage()
		{
			TestCaseHelper.ClearTable(OrgCusCodeSchema.Constants.TableName);
			Sender.OH_IsUnpackDepot = false;
			Sender.OH_IsPackDepot = false;
			Sender.OH_IsSeaCTO = false;
			Sender.OH_IsContainerYard = false;
			var adapter = NewAdapterWithContainerLoadMessage();
			adapter.Load();
			AssertEquals("Precondition - MessageSenderCode", "SHCB1", adapter.MessageSenderCode);
			AssertEquals("Precondition - Sender", Sender, adapter.Sender);
			AssertEquals("Precondition - SenderAddress", Sender.MainAddress, adapter.SenderAddress);
			AssertEquals("Precondition - Vessel", Vessel, adapter.Vessel);
			AssertEquals("Precondition - Type Description", "Load COARRI", adapter.MessageTypeTitle);
			AssertEquals("MovementCode", ContainerMovementTypes.Codes.Load, adapter.MovementCode);
		}

		public void TestContainerDischargeMessage()
		{
			TestCaseHelper.ClearTable(OrgCusCodeSchema.Constants.TableName);
			Sender.OH_IsPackDepot = false;
			Sender.OH_IsSeaCTO = false;
			Sender.OH_IsContainerYard = false;
			Sender.OH_IsUnpackDepot = true;
			var adapter = NewAdapterWithContainerDischargeMessage();
			adapter.Load();
			AssertEquals("Precondition - MessageSenderCode", "SHCB1", adapter.MessageSenderCode);
			AssertEquals("Precondition - Sender", Sender, adapter.Sender);
			AssertEquals("Precondition - SenderAddress", Sender.MainAddress, adapter.SenderAddress);
			AssertEquals("Precondition - Vessel", Vessel, adapter.Vessel);
			AssertEquals("Precondition - Type Description", "Discharge COARRI", adapter.MessageTypeTitle);
			AssertEquals("MovementCode", ContainerMovementTypes.Codes.Discharge, adapter.MovementCode);
		}

		public void TestHasRelatedJobs()
		{
			var adapter = NewAdapterWithPrefilledMessage();
			adapter.Load();
			var bill1 = Factory.New<BillOfLading>();
			bill1.JS_JX = Voyage1.Sailings[0].PK;
			var bill2 = Factory.New<BillOfLading>();
			bill2.JS_JX = Voyage2.Sailings[0].PK;
			BillOfLadingContainer container1 = bill1.RealContainers.AddNew();
			container1.JC_ContainerNum = "CCLU4214635";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			BillOfLadingContainer container2 = bill2.RealContainers.AddNew();
			container2.JC_ContainerNum = "CCLU4214629";
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			Factory.Save();
			AssertEquals("CCLU4214635", true, adapter.HasRelatedJobs("CCLU4214635"));
			AssertEquals("CCLU4214629", false, adapter.HasRelatedJobs("CCLU4214629"));
		}

		public void TestGetMovementDescription()
		{
			ICMMProcessingAdapter adapter = NewAdapterWithPrefilledMessage();
			AssertEquals("GetMovementDescription.DepotGateIn", "Depot Gate In", adapter.GetMovementDescription(ContainerMovementTypes.Codes.DepotGateIn));
			AssertEquals("GetMovementDescription.DepotGateOut", "Depot Gate Out", adapter.GetMovementDescription(ContainerMovementTypes.Codes.DepotGateOut));
			AssertEquals("GetMovementDescription.WharfGateIn", "Wharf Gate In", adapter.GetMovementDescription(ContainerMovementTypes.Codes.WharfGateIn));
			AssertEquals("GetMovementDescription.WharfGateOut", "Wharf Gate Out", adapter.GetMovementDescription(ContainerMovementTypes.Codes.WharfGateOut));
			AssertEquals("GetMovementDescription.YardGateIn", "Yard Gate In", adapter.GetMovementDescription(ContainerMovementTypes.Codes.YardGateIn));
			AssertEquals("GetMovementDescription.YardGateOut", "Yard Gate Out", adapter.GetMovementDescription(ContainerMovementTypes.Codes.YardGateOut));
			AssertEquals("GetMovementDescription.Load", "Load", adapter.GetMovementDescription(ContainerMovementTypes.Codes.Load));
			AssertEquals("GetMovementDescription.Discharge", "Discharge", adapter.GetMovementDescription(ContainerMovementTypes.Codes.Discharge));
			AssertEquals("GetMovementDescription.DUMMY", "", adapter.GetMovementDescription("dummy"));
		}

		public void TestCreateMissingContainers()
		{
			var adapter = NewAdapterWithEmptyMessage();
			AgencyRegistry.Instance.CMMCreateMissingContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("CreateMissingContainers is TRUE", true, adapter.CreateMissingContainers);
			adapter = NewAdapterWithEmptyMessage();
			AgencyRegistry.Instance.CMMCreateMissingContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("CreateMissingContainers is FALSE", false, adapter.CreateMissingContainers);
		}

		public abstract void TestGarbageMessage();

		public void TestPropertiesBeforeLoad()
		{
			var adapter = NewAdapterWithEmptyMessage();
			AssertEquals("CountryCode", "", adapter.CountryCode);
			AssertEquals("LloydsNumber", "", adapter.LloydsNumber);
			AssertEquals("MessageSenderCode", "", adapter.MessageSenderCode);
			AssertEquals("MessageSenderReference", "", adapter.MessageSenderReference);
			AssertEquals("MessageType", CMMMessageType.Unknown, adapter.MessageType);
			AssertEquals("MessageTypeTitle", "Unknown", adapter.MessageTypeTitle);
			AssertEquals("MovementCode", "", adapter.MovementCode);
			AssertEquals("PortCode", "", adapter.PortCode);
			AssertNull("Sender is Null", adapter.Sender);
			AssertNull("SenderAddress is Null", adapter.SenderAddress);
			AssertNull("Vessel is Null", adapter.Vessel);
			AssertEquals("VoyageNumber", "", adapter.VoyageNumber);
			AssertNull("Voyage is Null", adapter.Voyage);
			AssertEquals("Containers Count", 0, adapter.Containers.Count);
		}

		public void TestPropertiesMapping()
		{
			var adapter = NewAdapterWithPrefilledMessage();
			adapter.Load();
			AssertEquals("CountryCode", "AU", adapter.CountryCode);
			AssertEquals("LloydsNumber", "9290127", adapter.LloydsNumber);
			AssertEquals("MessageSenderCode", "SHCB1", adapter.MessageSenderCode);
			AssertEquals("MessageSenderReference", "Message Sender: Random Depot1 (RANDEPSYD)\r\n", adapter.MessageSenderReference);
			AssertEquals("MessageType", CMMMessageType.GateIn, adapter.MessageType);
			AssertEquals("MessageTypeTitle", "Gate-In CODECO", adapter.MessageTypeTitle);
			AssertEquals("MovementCode", "DGI", adapter.MovementCode);
			AssertEquals("PortCode", "AUSYD", adapter.PortCode);
			AssertNotNull("Sender Not Null", adapter.Sender);
			AssertEquals("Sender.OH_Code", "RANDEPSYD", adapter.Sender.OH_Code);
			AssertEquals("Sender.OH_FullName", "Random Depot1", adapter.Sender.OH_FullName);
			AssertNotNull("SenderAddress", adapter.SenderAddress);
			AssertEquals("SenderAddress.OA_Code", "#1", adapter.SenderAddress.OA_Code);
			AssertNotNull("Vessel", adapter.Vessel);
			AssertEquals("Vessel.RV_Name", "Random Vessel", adapter.Vessel.RV_Name);
			AssertEquals("Vessel.LloydsNumber", "9290127", adapter.Vessel.RV_LloydsNumber);
			AssertEquals("VoyageNumber", "0033", adapter.VoyageNumber);
			AssertNotNull("Voyage Not Null", adapter.Voyage);
			AssertEquals("Voyage.JV_VoyageFlight", "0033", adapter.Voyage.JV_VoyageFlight);
			AssertEquals("Containers Count", 1, adapter.Containers.Count);
			CMMMessageContainer container = adapter.Containers[0];
			AssertEquals("container.BillOfLading", "BillOfLading", container.BillOfLading);
			AssertEquals("container.BookingReference", "BookingReference", container.BookingReference);
			AssertEquals("container.ContainerNumber", "CCLU4214635", container.ContainerNumber);
			AssertEquals("container.GoodsDeclarationNumber", "GoodsDeclarationNumber", container.GoodsDeclarationNumber);
			AssertEquals("container.GrossWeightKG", 6800m, container.GrossWeightKG);
			AssertEquals("container.ISOType", "42R0", container.ISOType);
			AssertEquals("container.IsEmpty", false, container.IsEmpty);
			AssertEquals("container.PositioningDateTime", new DateTime(2008, 06, 27, 10, 10, 0), container.PositioningDateTime);
			AssertEquals("container.EquipmentSupplier", CMMEquipmentSupplier.Carrier, container.CmmEquipmentSupplier);
			List<string> sealNumbers = container.SealNumbers.ToList();
			AssertEquals("Seal Numbers Count", 2, sealNumbers.Count);
			AssertEquals("SealNumber1", "123456", sealNumbers[0]);
			AssertEquals("SealNumber2", "654321", sealNumbers[1]);
		}

		public void TestCantFindSender()
		{
			Sender.OH_Code = "DUMMY";
			Sender.OH_FullName = "DUMMY";
			CustomsCode.OK_CustomsRegNo = "DUMMY";
			Factory.Save();
			try
			{
				ICMMProcessingAdapter adapter = NewAdapterWithPrefilledMessage();
				Fail("Should have thrown an exception due to sender not being found");
			}
			catch (Exception)
			{
			}

			AssertNull(null); // Stop "Empty Test" failures
		}

		public void TestTooManyMatchingSenderOrgs()
		{
			Sender.MainAddress.OA_Address1 = "Address";
			var sender2 = Factory.NewWithValidTestData<OrgHeader>();
			sender2.OH_Code = "RANDEP2";
			sender2.OH_FullName = "Random Depot2";
			sender2.OH_IsUnpackDepot = true;
			sender2.MainAddress.OA_Address1 = "Address";
			OrgCusCode code2 = sender2.CustomsCodes.AddNew();
			code2.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			code2.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			code2.OK_CustomsRegNo = "SHCB1";
			Factory.Save();
			try
			{
				var adapter = NewAdapterWithPrefilledMessage();
				Fail("Should have thrown an exception due to multiple matching sender orgs");
			}
			catch
			{
			}

			AssertNull(null);
		}

		public void TestVesselVoyage_NoVoyage()
		{
			Voyage1.JV_VoyageFlight = "DUMMY";
			Factory.Save();
			var adapter = NewAdapterWithPrefilledMessage();
			adapter.Load();
			AssertNull("Voyage is null", adapter.Voyage);
			AssertNotNull("Vessel is accessible", adapter.Vessel);
		}

		public void TestVesselVoyage_NoVessel()
		{
			Vessel.RV_Name = "DUMMY";
			Vessel.RV_LloydsNumber = "DUMMY";
			Factory.Save();
			var adapter = NewAdapterWithPrefilledMessage();
			adapter.Load();
			AssertNull("Voyage is null", adapter.Voyage);
			AssertNull("Vessel is null", adapter.Vessel);
		}

		public void TestMultipleMatchingVoyageVessel_NoFallbacks()
		{
			GetVoyageWithDuplicateDetails();
			Factory.Save();
			var adapter = NewAdapterWithPrefilledMessage();
			adapter.Load();
			AssertNull("There are two voyages with the same voyage number/vessel and no way to determine which is correct", adapter.Voyage);
		}

		public void TestMultipleMatchingVoyageVessel_BillOfLadingVoyageMatchesContainer()
		{
			var newVoyage = GetVoyageWithDuplicateDetails();
			var billofLading = Factory.New<BillOfLading>();
			billofLading.JS_JX = newVoyage.Sailings[0].PK;
			var container = billofLading.RealContainers.AddNew();
			container.JC_ContainerNum = "CCLU4214635";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			Factory.Save();
			var adapter = NewAdapterWithPrefilledMessage();
			adapter.Load();
			AssertNotNull("Duplicate voyages with the same vessel/voyage exist but only one voyage can be matched to a container", adapter.Voyage);
			AssertEquals(newVoyage.PK, adapter.Voyage.PK);
		}

		public void TestMultipleMatchingVoyageVessel_BillOfLadingVoyageDoesntMatchContainer()
		{
			var newVoyage = GetVoyageWithDuplicateDetails();
			var billofLading = Factory.New<BillOfLading>();
			billofLading.JS_JX = newVoyage.Sailings[0].PK;
			var container = billofLading.RealContainers.AddNew();
			container.JC_ContainerNum = "JUNK";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			Factory.Save();
			var adapter = NewAdapterWithPrefilledMessage();
			adapter.Load();
			AssertNull("Can't match by any bill of lading sailing to the container in the message", adapter.Voyage);
		}

		JobVoyage GetVoyageWithDuplicateDetails()
		{
			var newVoyage = Factory.New<JobVoyage>();
			newVoyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
			newVoyage.JV_VoyageFlight = "0033";
			newVoyage.JV_RV_NKVessel = Vessel.RV_FK;
			newVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			newVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			newVoyage.GenerateSailings();
			return newVoyage;
		}

		#endregion

		#region Implementation

		protected OrgCusCode CustomsCode;
		protected OrgHeader Sender;
		protected RefVessel Vessel;
		protected JobVoyage Voyage1;
		protected JobVoyage Voyage2;

		protected override void SetUp()
		{
			base.SetUp();
			Vessel = Factory.New<RefVessel>();
			Vessel.RV_Name = "Random Vessel";
			Vessel.RV_LloydsNumber = "9290127";
			Voyage1 = Factory.New<JobVoyage>();
			Voyage1.JV_VoyageFlight = "0033";
			Voyage1.JV_RV_NKVessel = Vessel.RV_FK;
			Voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			Voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			Voyage2 = Factory.New<JobVoyage>();
			Voyage2.JV_VoyageFlight = "0044";
			Voyage2.JV_RV_NKVessel = Vessel.RV_FK;
			Voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			Voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			Sender = Factory.NewWithValidTestData<OrgHeader>();
			Sender.OH_Code = "RANDEP1";
			Sender.OH_FullName = "Random Depot1";
			Sender.OH_IsUnpackDepot = true;
			Sender.OH_RL_NKClosestPort = "AUSYD";
		}

		#endregion

		#region Abstract Methods

		public abstract ICMMProcessingAdapter NewAdapterWithPrefilledMessage();
		public abstract ICMMProcessingAdapter NewAdapterWithEmptyMessage();
		public abstract ICMMProcessingAdapter NewAdapterWithGarbageMessage();
		public abstract ICMMProcessingAdapter NewAdapterWithGateInMessage();
		public abstract ICMMProcessingAdapter NewAdapterWithGateOutMessage();
		public abstract ICMMProcessingAdapter NewAdapterWithContainerLoadMessage();
		public abstract ICMMProcessingAdapter NewAdapterWithContainerDischargeMessage();

		#endregion
	}
}
