using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingContainerTest : CommonContainerTest2
	{
		public void TestShouldSetAllocationRoute()
		{
			var contract = Factory.New<RatingContract>();
			var allocationLine1 = contract.Allocations.AddNew();
			var allocationLine2 = contract.Allocations.AddNew();

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_Code = "BG";

			var fallbackAccountPivot1 = allocationLine1.Contract.NamedAccountPivots.AddNew();
			var fallbackAccountPivot2 = allocationLine2.Contract.NamedAccountPivots.AddNew();

			fallbackAccountPivot1.RNP_OH_NamedAccount = carrier.PK;
			fallbackAccountPivot2.RNP_OH_NamedAccount = carrier.PK;

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();

			using (container.GetValidationSuspender())
			{
				AssertEquals(ZGuid.Empty, container.JC_RCA_AllocationLine);

				container.JC_RCA_AllocationLine = allocationLine1.PK;

				AssertEquals(
					"Should update JC_RCA_AllocationLine when the allocation line has an associated rating contract & not all shipments have a matching org in fallback header named account collection",
					allocationLine1.PK,
					container.JC_RCA_AllocationLine
				);

				shipment.ConsigneePK = carrier.PK;
				container.JC_RCA_AllocationLine = allocationLine2.PK;

				AssertEquals(
					"Should update JC_RCA_AllocationLine when all shipments have a matching org in fallback header named account collection",
					allocationLine2.PK,
					container.JC_RCA_AllocationLine
				);

				var accountPivot = allocationLine1.NamedAccountPivots.AddNew();
				accountPivot.RNP_OH_NamedAccount = carrier.PK;

				shipment.ConsigneePK = ZGuid.Empty;
				container.JC_RCA_AllocationLine = allocationLine1.PK;

				AssertEquals(
					"Should update JC_RCA_AllocationLine when the allocation line has an associated rating contract & not all shipments have a matching org in header named account collection",
					allocationLine1.PK,
					container.JC_RCA_AllocationLine
				);

				shipment.ConsigneePK = carrier.PK;
				container.JC_RCA_AllocationLine = allocationLine2.PK;

				AssertEquals(
					"Should update JC_RCA_AllocationLine when all shipments have a matching org in header named account collection",
					allocationLine2.PK,
					container.JC_RCA_AllocationLine
				);

				container.JC_RCA_AllocationLine = ZGuid.Empty;

				AssertEquals(
					"Should update JC_RCA_AllocationLine when allocation line PK is empty",
					ZGuid.Empty,
					container.JC_RCA_AllocationLine
				);
			}
		}

		public void TestJC_RCA_AllocationLineReadonly()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			var contract = Factory.New<RatingContract>();
			var allocationLine = contract.Allocations.AddNew();

			consol.JK_RCA_AllocationLine = allocationLine.PK;

			AssertEquals("Allocation route should be readonly if parent is set", true, container1.JC_RCA_AllocationLine_ReadOnly);
			AssertEquals("Allocation route should be readonly if parent is set", true, container2.JC_RCA_AllocationLine_ReadOnly);
		}

		public void TestJC_RCA_AllocationLineNotReadOnlyWhenDifferent()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			var contract = Factory.New<RatingContract>();
			var allocationLine1 = contract.Allocations.AddNew();
			var allocationLine2 = contract.Allocations.AddNew();

			consol.JK_RCA_AllocationLine = allocationLine1.PK;
			container2.JC_RCA_AllocationLine = allocationLine2.PK;

			AssertEquals("Allocation route should be readonly if parent is set", true, container1.JC_RCA_AllocationLine_ReadOnly);
			AssertEquals("Allocation route should not be readonly if different to parent", false, container2.JC_RCA_AllocationLine_ReadOnly);
		}

		public void TestMarkingParentConsolForValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;

			var container = consol.Containers.AddNew();

			var consolMarkedAsNeedingValidation = 0;
			var previousConsolMarkedAsNeedingValidationCount = 0;

			Factory.MarkedAsNeedingValidation += (BusinessObject obj) =>
			{
				if (obj is ForwardingConsol)
				{
					consolMarkedAsNeedingValidation++;
				}
			};

			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

			Assert("Has been marked as needing validation at least once", consolMarkedAsNeedingValidation > previousConsolMarkedAsNeedingValidationCount);
			previousConsolMarkedAsNeedingValidationCount = consolMarkedAsNeedingValidation;

			container.JC_ContainerCount = 2;
			Assert("Has been marked as needing validation at least once", consolMarkedAsNeedingValidation > previousConsolMarkedAsNeedingValidationCount);
		}

		public void TestSettingCommodityCodeMarksConsolForValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();

			var consolWasMarkedAsNeedingValidation = false;
			Factory.MarkedAsNeedingValidation += (BusinessObject bizo) => consolWasMarkedAsNeedingValidation |= bizo == consol;

			container.JC_RH_NKContainerCommodityCode = "AMMN";
			Assert("Setting Commodity Code on Container should have marked Consol as needing validation.", consolWasMarkedAsNeedingValidation);
		}

		[TestDate(2021, 1, 1)]
		public void TestTryCalculateStorageStart()
		{
			var containerMatch = new Mock<IContainerPenaltyMatchResult>();
			containerMatch.Setup(x => x.PenaltyType).Returns("STO");
			containerMatch.Setup(x => x.CreditorType).Returns("CTO");
			var containerMatches = new List<IContainerPenaltyMatchResult>();
			containerMatches.Add(containerMatch.Object);

			var today = ZDateTime.Today;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			var transport = consol.Transports[0];
			transport.JW_ATA = today.AddDays(10);
			transport.JW_VoyageFlight = "ZZ1234";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_TerminalAvailabilityDate = today;
			transport.JW_TerminalStorageDateForBinding = today.AddDays(2);

			var strategy = new Mock<IContainerDefaultingStrategy>();

			using (Globals.SetIsUserInteractiveForTest(true))
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", c => strategy.Object))
			{
				var matchResultMock = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock.Setup(x => x.FreeDays).Returns(3);

				var container = Factory.New<ForwardingContainer>();
				container.JC_OverrideFCLAvailableStorage = true;
				strategy.Setup(x => x.CalculateStorageStart(Moq.It.IsAny<ZString>())).Returns(() => (GetStorageStartFromAvailableDate(container.JC_FCLAvailable, 4), container.JC_FCLAvailable, "CTO Available"));
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>())).Returns(() => container.JC_FCLAvailable);
				strategy.Setup(x => x.GetMatchedStoragePenalty(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), null)).Returns(matchResultMock.Object);
				strategy.Setup(x => x.CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Import, null)).Returns(containerMatches);

				consol.Containers.Add(container);
				AssertEquals(ZDateTime.Empty, container.JC_FCLAvailable);
				AssertEquals("JC_ArrivalCTOStorageStartDate should not be calculated when overriden.", ZDateTime.Empty, container.JC_ArrivalCTOStorageStartDate);

				container.JC_FCLAvailable = today.AddDays(1);
				AssertEquals("JC_ArrivalCTOStorageStartDate should be calculated when Free Days are matched and changed by JC_FCLAvailable.", container.JC_FCLAvailable.AddDays(4), container.JC_ArrivalCTOStorageStartDate);

				container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays = 5;
				AssertEquals("JC_ArrivalCTOStorageStartDate should not be calculated when overriden.", container.JC_FCLAvailable.AddDays(4), container.JC_ArrivalCTOStorageStartDate);
			}

			using (Globals.SetIsUserInteractiveForTest(true))
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", c => strategy.Object))
			{
				consol.Containers.RemoveAll();
				var matchResultMock = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock.Setup(x => x.FreeDays).Returns(3);

				var container = Factory.New<ForwardingContainer>();
				strategy.Reset();
				strategy.Setup(x => x.CalculateStorageStart(Moq.It.IsAny<ZString>())).Returns(() => (GetStorageStartFromAvailableDate(container.JC_FCLAvailable, 4), container.JC_FCLAvailable, "CTO Available"));
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>())).Returns(() => container.JC_FCLAvailable);
				strategy.Setup(x => x.GetMatchedStoragePenalty(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), null)).Returns(matchResultMock.Object);
				strategy.Setup(x => x.CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Import, null)).Returns(containerMatches);

				consol.Containers.Add(container);
				AssertEquals("Precondition", false, container.JC_OverrideFCLAvailableStorage);
				AssertEquals(today, container.JC_FCLAvailable);
				AssertEquals("JC_ArrivalCTOStorageStartDate should not be calculated based on FreeTimeAsDays.", container.JC_FCLAvailable.AddDays(4), container.JC_ArrivalCTOStorageStartDate);

				container.JC_FCLAvailableCore = today.AddDays(1);
				AssertEquals("Precondition", false, container.JC_OverrideFCLAvailableStorage);
				AssertEquals("JC_ArrivalCTOStorageStartDate should not be calculated based on FreeTimeAsDays.", container.JC_FCLAvailable.AddDays(4), container.JC_ArrivalCTOStorageStartDate);

				container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays = 5;
				AssertEquals("JC_ArrivalCTOStorageStartDate should not be calculated based on FreeTimeAsDays.", container.JC_FCLAvailable.AddDays(4), container.JC_ArrivalCTOStorageStartDate);
			}

			using (Globals.SetIsUserInteractiveForTest(true))
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", c => strategy.Object))
			{
				consol.Containers.RemoveAll();
				var container = Factory.New<ForwardingContainer>();
				var matchResultMock = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock.Setup(x => x.FreeDays).Returns(7);
				strategy.Reset();
				strategy.Setup(x => x.CalculateStorageStart(Moq.It.IsAny<ZString>())).Returns(() => (GetStorageStartFromAvailableDate(container.JC_FCLWharfGateOut, 5), container.JC_FCLWharfGateOut, "CTO Gate Out"));
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>())).Returns(() => container.JC_FCLWharfGateOut);
				strategy.Setup(x => x.GetMatchedStoragePenalty(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), null)).Returns(matchResultMock.Object);
				strategy.Setup(x => x.CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Import, null)).Returns(containerMatches);

				consol.Containers.Add(container);
				AssertEquals(today, container.JC_FCLAvailable);
				AssertEquals(today.AddDays(2), container.JC_ArrivalCTOStorageStartDate);

				container.JC_FCLWharfGateOut = today.AddDays(3);
				AssertEquals("JC_ArrivalCTOStorageStartDate should not be calculated based on FreeTimeAsDays.", container.JC_FCLWharfGateOut.AddDays(5), container.JC_ArrivalCTOStorageStartDate);
			}

			using (Globals.SetIsUserInteractiveForTest(true))
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", c => strategy.Object))
			{
				consol.Containers.RemoveAll();
				var container = Factory.New<ForwardingContainer>();

				var matchResultMock1 = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock1.Setup(x => x.FreeDays).Returns(7);
				strategy.Reset();
				strategy.Setup(x => x.CalculateStorageStart(Moq.It.IsAny<ZString>())).Returns(() => (GetStorageStartFromAvailableDate(container.JC_FCLUnloadFromVessel, 8), container.JC_FCLUnloadFromVessel, "FCL Unload"));
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>())).Returns(() => container.JC_FCLUnloadFromVessel);
				strategy.Setup(x => x.GetMatchedStoragePenalty(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), null)).Returns(matchResultMock1.Object);
				strategy.Setup(x => x.CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Import, null)).Returns(containerMatches);

				consol.Containers.Add(container);
				AssertEquals(today, container.JC_FCLAvailable);
				AssertEquals(today.AddDays(2), container.JC_ArrivalCTOStorageStartDate);

				container.JC_FCLUnloadFromVessel = today.AddDays(6);
				AssertEquals("JC_ArrivalCTOStorageStartDate should not be calculated based on FreeTimeAsDays.", container.JC_FCLUnloadFromVessel.AddDays(8), container.JC_ArrivalCTOStorageStartDate);
			}

			var matchResultMock2 = new Mock<IContainerPenaltyMatchResult>();
			matchResultMock2.Setup(x => x.FreeDays).Returns(7);
			strategy.Reset();
			strategy.Setup(x => x.CalculateStorageStart(Moq.It.IsAny<ZString>())).Returns(() => (GetStorageStartFromAvailableDate(transport.JW_ATA, 9), transport.JW_ATA, "Vessel Arrival"));
			strategy.Setup(x => x.CalculateStorageStartAvailableDate(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>())).Returns(() => transport.JW_ATA);
			strategy.Setup(x => x.GetMatchedStoragePenalty(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), null)).Returns(matchResultMock2.Object);
			strategy.Setup(x => x.CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Import, null)).Returns(containerMatches);

			using (Globals.SetIsUserInteractiveForTest(true))
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", c => strategy.Object))
			{
				consol.Containers.RemoveAll();
				var container = Factory.New<ForwardingContainer>();

				consol.Containers.Add(container);
				AssertEquals("JC_ArrivalCTOStorageStartDate should not be calculated based on FreeTimeAsDays.", transport.JW_ATA.AddDays(9), container.JC_ArrivalCTOStorageStartDate);

				transport.JW_ATA = today.AddDays(13);
				AssertEquals("JC_ArrivalCTOStorageStartDate should not be calculated based on FreeTimeAsDays.", transport.JW_ATA.AddDays(9), container.JC_ArrivalCTOStorageStartDate);
			}
		}

		static ZDateTime GetStorageStartFromAvailableDate(ZDateTime availableDate, int days) => availableDate.IsValid ? availableDate.AddDays(days) : availableDate;

		public void TestCalculateDefaultStoragePenalty()
		{
			var today = ZDateTime.Today;
			var strategy = new Mock<IContainerDefaultingStrategy>();
			var containerMatch = new Mock<IContainerPenaltyMatchResult>();
			containerMatch.Setup(x => x.PenaltyType).Returns("STO");
			containerMatch.Setup(x => x.CreditorType).Returns("CTO");
			containerMatch.Setup(x => x.FreeDays).Returns(2);
			var containerMatches = new List<IContainerPenaltyMatchResult>();
			containerMatches.Add(containerMatch.Object);

			strategy.Setup(x => x.CalculateStorageStartAvailableDate(Moq.It.IsAny<ZString>(), "CTO")).Returns(today.AddDays(3));
			strategy.Setup(x => x.GetMatchedStoragePenalty(Moq.It.IsAny<ZString>(), "CTO", Moq.It.IsAny<ZString>(), null)).Returns(containerMatch.Object);
			strategy.Setup(x => x.CalculateMatchedPenalties(Core.Constants.ContainerPenaltyProcessType.Import, null)).Returns(containerMatches);

			using (Globals.SetIsUserInteractiveForTest(true))
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", c => strategy.Object))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				var transport = consol.Transports[0];
				transport.JW_ATA = today.AddDays(1);
				transport.JW_TerminalAvailabilityDateForBinding = today.AddDays(3);
				transport.JW_TerminalStorageDateForBinding = today.AddDays(5);
				transport.JW_VoyageFlight = "ZZ1234";
				transport.JW_RL_NKDiscPort = "AUSYD";
				transport.JW_RL_NKLoadPort = "NZAKL";
				var container = consol.Containers.AddNew();

				AssertEquals(1, container.ImportPenalties.Count);
				AssertEquals("STO", container.ImportPenalties[0].CPY_PenaltyType);
				AssertEquals((ZByte)2, container.ImportPenalties[0].FreeTimeAsDays);
			}
		}

		public void TestCalculateDefaultDetention()
		{
			var today = ZDateTime.Today;

			var mockDetentionMatchResult = new Mock<IContainerPenaltyMatchResult>();
			mockDetentionMatchResult.Setup(x => x.FreeDays).Returns(3);

			var strategy = new Mock<IContainerDefaultingStrategy>();
			strategy.Setup(x => x.GetMatchedDetentionPenalty(Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), Moq.It.IsAny<ZString>(), Moq.It.IsAny<CommonShipment>())).Returns(mockDetentionMatchResult.Object);

			using (Globals.SetIsUserInteractiveForTest(true))
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", c => strategy.Object))
			{
				AssertDetention(TransportModes.Sea, ContainerModes.FCL, ShipmentTypes.StandardHouse, false, false);
				AssertDetention(TransportModes.Sea, ContainerModes.BuyersConsol, ShipmentTypes.StandardHouse, false, false);
				AssertDetention(TransportModes.Sea, ContainerModes.BuyersConsol, ShipmentTypes.BuyersConsolLead, false, false);
				AssertDetention(TransportModes.Air, ContainerModes.FCL, ShipmentTypes.StandardHouse, false, false);

				void AssertDetention(ZString consolTransportMode, ZString shipmentContainerMode, ZString shipmentType, bool hasDelivery, bool hasPickup)
				{
					var consol = Factory.New<ForwardingConsol>();
					consol.JK_TransportMode = consolTransportMode;

					var container = consol.Containers.AddNew();

					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_PackingMode = shipmentContainerMode;
					shipment.JS_ShipmentType = shipmentType;

					consol.Shipments.Add(shipment);
					consol.JK_RL_NKLoadPort = "AUSYD";
					consol.JK_RL_NKDischargePort = "NZAKL";

					var packLine = shipment.OuterPackLines.AddNew();
					packLine.Containers.Add(container);

					Factory.Save();

					container.JC_FCLWharfGateOut = DateTime.Now;
					container.JC_FCLWharfGateIn = DateTime.Now;

					var penalty = container.DeliveryPenalties.FirstOrDefault(x => x.CPY_PenaltyType == "DET");
					AssertPenalty(hasDelivery);

					penalty = container.PickupPenalties.FirstOrDefault(x => x.CPY_PenaltyType == "DET");
					AssertPenalty(hasPickup);

					void AssertPenalty(bool hasPenalty)
					{
						if (hasPenalty)
						{
							AssertNotNull("DET", penalty);
							AssertEquals((ZByte)3, penalty.FreeTimeAsDays);
						}
						else
						{
							AssertNull("DET", penalty);
						}
					}
				}
			}
		}

		public void TestFreeTimeAsDaysForRegistryItem()
		{
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 11 }))
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 12 }))
			{
				var container = Factory.New<ForwardingContainer>();
				var importPenalty = container.ImportPenalties.AddNew();
				importPenalty.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage;
				importPenalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;

				AssertEquals((ZByte)12, importPenalty.FreeTimeAsDays);

				var exportPenalty = container.ExportPenalties.AddNew();
				exportPenalty.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage;
				exportPenalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals((ZByte)11, exportPenalty.FreeTimeAsDays);
			}
		}

		public void TestAdditionalReferenceNumbers()
		{
			var container = Factory.New<ForwardingContainer>();
			container.AMSNumber = "MB1";
			container.ITReferenceNumber = "V1";
			AssertEquals("AMS Num", "MB1", container.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS).CE_EntryNum);
			AssertEquals("IT Ref. Num", "V1", container.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(UnitedStatesAdditionalReferenceNumberTypes.Codes.IT).CE_EntryNum);

			Assert("has a new BOL?", container.HasNewMBOL);
		}

		public void TestDocumentSupporter_Shipment()
		{
			var container = Factory.New<ForwardingContainer>();
			var shipment = Factory.New<ForwardingShipment>();
			container.LinkedShipment = shipment;
			AssertType("DocumentSupporter is ContainerDocumentSupporterShipment", typeof(ContainerDocumentSupporterShipment), container.DocumentSupporter);
		}

		public void TestDocumentSupporter_Consol()
		{
			AssertType("DocumentSupporter is ContainerDocumentSupporterConsol", typeof(ContainerDocumentSupporterConsol), Factory.New<ForwardingContainer>().DocumentSupporter);
		}

		public void TestGetEDocsProviderSupporter()
		{
			IEDocsProvider container = Factory.New<ForwardingContainer>();
			AssertType("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), container.GetEDocsProviderSupporter());
		}

		public void TestGrossWeightResetOnUnitChange()
		{
			ForwardingContainer container = Factory.New<ForwardingConsol>().Containers.AddNew();
			container.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			container.JC_TareWeight = 2000m;

			PackLine packLine = container.Consol.Shipments.AddNew().OuterPackLines.AddNew();
			packLine.SetContainer(container.Consol, container);
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 1000;

			AssertEquals(3000m, container.JC_GrossWeight);

			packLine.JL_ActualWeight = 1500m;
			AssertEquals(3500m, container.JC_GrossWeight);

			container.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;
			AssertEquals(5306.934m, container.JC_GrossWeight);
		}

		public void TestGrossWeightNotUpdatedAutomaticallyWhenOverridden()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			var container = consol.Containers.AddNew();
			container.JC_IsGrossWeightOverridden = true;
			container.JC_GrossWeightUQ = Weight.Tonnes;
			container.JC_TareWeight = 1000;
			container.JC_GrossWeight = 7000;

			container.JC_GrossWeightUQ = Weight.Kilograms;
			AssertEquals("JC_GrossWeightUQ change should not update JC_GrossWeight", 7000m, container.JC_GrossWeight);
		}

		public void TestIsGrossWeightReadOnly()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			var container = consol.Containers.AddNew();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;

			container.JC_IsGrossWeightOverridden = false;
			AssertEquals(true, container.IsGrossWeightReadOnly);

			container.JC_IsGrossWeightOverridden = true;
			AssertEquals(false, container.IsGrossWeightReadOnly);
		}

		public void TestIsGrossWeightOverriddenForBinding()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;

			var container = consol.Containers.AddNew();
			container.JC_GrossWeightUQ = Weight.Kilograms;
			container.IsGrossWeightOverriddenForBinding = true;
			container.JC_TareWeight = 1500;
			container.JC_GrossWeight = 7654m;
			AssertEquals(7654m, container.JC_GrossWeight);

			container.IsGrossWeightOverriddenForBinding = false;
			AssertEquals("removing the override restores the calculated JC_GrossWeight", 1500m, container.JC_GrossWeight);
		}

		public void TestIsGrossWeightOverrideAvailable()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;

			var container = consol.Containers.AddNew();

			// Air consol
			consol.JK_TransportMode = Constants.TransportModes.Air;

			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			AssertEquals(true, container.IsGrossWeightOverrideAvailable);

			consol.JK_ConsolMode = Constants.ContainerModes.ULD;
			AssertEquals(true, container.IsGrossWeightOverrideAvailable);

			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			AssertEquals(false, container.IsGrossWeightOverrideAvailable);

			consol.JK_ConsolMode = Constants.ContainerModes.Other;
			AssertEquals(false, container.IsGrossWeightOverrideAvailable);

			// Sea consol
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			AssertEquals(false, container.IsGrossWeightOverrideAvailable);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertEquals(false, container.IsGrossWeightOverrideAvailable);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			AssertEquals(false, container.IsGrossWeightOverrideAvailable);

			consol.JK_ConsolMode = Constants.ContainerModes.Other;
			AssertEquals(false, container.IsGrossWeightOverrideAvailable);

			// Road
			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertEquals(false, container.IsGrossWeightOverrideAvailable);

			// Rail
			consol.JK_TransportMode = Constants.TransportModes.Rail;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertEquals(false, container.IsGrossWeightOverrideAvailable);
		}

		[ExpectNoExceptions]
		public void TestInvalidWeightUnitOnVerifiedGrossWeightDoesNotThrowException()
		{
			var consol = Factory.New<ForwardingConsol>();

			var container = consol.Containers.AddNew();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeightVerificationDateTime = ZDateTime.Now;
			container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container.JC_GrossWeightUQ = "11";
		}

		public void TestIBillDetails()
		{
			ForwardingContainer container = Factory.New<ForwardingConsol>().Containers.AddNew();
			container.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			container.JC_TareWeight = 2000m;
			container.AMSNumber = "34278";
			container.ITReferenceNumber = "432098";

			var shipment = container.Consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.Consol, container);
			packLine.JL_PackageCount = 10;
			packLine.JL_F3_NKPackType = "KG";
			var billDetails = (IBillDetails)container;

			AssertEquals("34278", billDetails.BillNumberInfo.Value);
			var numberOfPacksInfo = billDetails.GetNumberOfPackesInfos(shipment);
			var typeOfPacksInfo = billDetails.GetTypeOfPackesInfos(shipment);
			AssertEquals(1, numberOfPacksInfo.Length);
			AssertEquals(1, typeOfPacksInfo.Length);
			AssertEquals(10, numberOfPacksInfo[0].Value);
			AssertEquals("KG", typeOfPacksInfo[0].Value);
			AssertEquals(0, billDetails.SCACIssuers.Count());
		}

		public void TestJC_GrossWeightUQ_Conversion()
		{
			var container = Factory.New<ForwardingContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_GrossWeight = 33;
			container.JC_GrossWeightUQ = "KG";
			container.JC_GrossWeightUQ = "G";

			AssertEquals((ZDecimal)33000, container.JC_GrossWeight);
		}

		public void TestSettingGrossWeightUQDefaultsTareAndDunnageWeights()
		{
			ForwardingContainer container = Factory.New<ForwardingConsol>().Containers.AddNew();
			container.JC_RC = RC_20GP_PK;
			container.JC_GrossWeightUQ = Core.Constants.Weight.Tonnes;

			var defaultWeight = container.JC_Calc_TareWeight;
			container.JC_DunnageWeight = 0;
			container.JC_TareWeight = defaultWeight;

			container.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			var newDefaultWeight = container.JC_Calc_TareWeight;

			AssertEquals("JC_GrossWeight should be changed", Core.Constants.Weight.Kilograms, container.JC_GrossWeightUQ);
			AssertNotEquals("Default Weights should be different", defaultWeight, newDefaultWeight);
			AssertEquals("JC_TareWeight should be changed", newDefaultWeight, container.JC_TareWeight);
			AssertEquals("JC_DunnageWeight should not be changed", (ZDecimal)0, container.JC_DunnageWeight);

			ZDecimal doNotDefaultWeight = 123.123;
			container.JC_DunnageWeight = doNotDefaultWeight;
			container.JC_TareWeight = doNotDefaultWeight;
			container.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;

			AssertEquals("JC_GrossWeight should be changed", Core.Constants.Weight.Pounds, container.JC_GrossWeightUQ);
			AssertEquals("JC_TareWeight should not be changed", doNotDefaultWeight, container.JC_TareWeight);
			AssertEquals("JC_DunnageWeight should not be changed", doNotDefaultWeight, container.JC_DunnageWeight);
		}

		public void TestJC_FCLOnBoardVessel_CheckingFacilitySpecificEvents_MatchesFacilityAndLocation()
		{
			var eventTime = ZDateTimeOffset.Now;

			var container = Factory.New<ForwardingContainer>();

			container.Logs.AddNew(Events.FreightLoaded, "|LOC=USLAX", eventTime);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, container.JC_FCLOnBoardVessel);

			container.Logs.AddNew(Events.FreightLoaded, "", eventTime);
			AssertEquals("Location matched - both are empty, date updated", eventTime.ToZDateTime(), container.JC_FCLOnBoardVessel);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			container = consol.Containers.AddNew();

			container.Logs.AddNew(Events.FreightLoaded, "|LOC=NZAKL", eventTime);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, container.JC_FCLOnBoardVessel);

			container.Logs.AddNew(Events.FreightLoaded, "", eventTime);
			AssertEquals("Location not matched, date not updated", ZDateTime.Empty, container.JC_FCLOnBoardVessel);

			container.Logs.AddNew(Events.FreightLoaded, "|LOC=AUSYD", eventTime);
			AssertEquals("Location matched, date updated", eventTime.ToZDateTime(), container.JC_FCLOnBoardVessel);

			var containerFAC = Factory.New<ForwardingContainer>();

			containerFAC.Logs.AddNew(Events.FreightLoaded, "|FAC=Office", eventTime);
			AssertEquals("Facility not matched, date not updated", ZDateTime.Empty, containerFAC.JC_FCLOnBoardVessel);

			containerFAC.Logs.AddNew(Events.FreightLoaded, "|FAC=CTO", eventTime);
			AssertEquals("Facility matched, date updated", eventTime.ToZDateTime(), containerFAC.JC_FCLOnBoardVessel);
		}

		#region Container Event Dates are updated from Gate In/Out/Dehire Events

		[TestDate(2014, 09, 09)]
		public void TestContainerFieldsChangesAddGateInEvents()
		{
			var today = ZDateTime.Today;
			var container = GetNewContainerOnNewImportConsol();
			container.JC_FCLWharfGateIn = today;

			var logs = container.Logs;
			var gateInLog = logs.MostRecentLogByEventTime(Events.GateIn);

			AssertNotNull("Expected event to have been created by updating the container date", gateInLog);
			AssertEquals(container.JC_FCLWharfGateIn, gateInLog.SL_EventTime);
			AssertEquals(EventConstants.Facilities.Code.Terminal, gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals("CNSHA", gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			container.JC_ContainerYardEmptyReturnGateIn = today.AddDays(2);
			gateInLog = logs.MostRecentLogByEventTime(Events.GateIn);

			AssertEquals(container.JC_ContainerYardEmptyReturnGateIn, gateInLog.SL_EventTime);
			AssertEquals(EventConstants.Facilities.Code.ContainerYard, gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals("Location should be consol's discharge port",
				"AUSYD", gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			var containerYard = Factory.NewWithValidTestData<OrgHeader>();
			containerYard.OH_IsContainerYard = true;
			containerYard.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			container.JC_OA_ArrivalContainerYardAddress = containerYard.MainAddress.PK;

			container.JC_ContainerYardEmptyReturnGateIn = today.AddDays(3);
			gateInLog = logs.MostRecentLogByEventTime(Events.GateIn);

			AssertEquals(container.JC_ContainerYardEmptyReturnGateIn, gateInLog.SL_EventTime);
			AssertEquals("CY event should prefer the container yard address",
				"AUBNE", gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			container.JC_FCLWharfGateIn = today.AddDays(4);
			gateInLog = logs.MostRecentLogByEventTime(Events.GateIn);

			AssertEquals(container.JC_FCLWharfGateIn, gateInLog.SL_EventTime);
			AssertEquals("Wharf Gate In event always match the consol's last discharge port",
				"CNSHA", gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
		}

		[TestDate(2014, 09, 09)]
		public void TestContainerFieldsChangesAddGateOutEvents()
		{
			var today = ZDateTime.Today;
			var container = GetNewContainerOnNewImportConsol();
			container.JC_FCLWharfGateOut = today;

			var logs = container.Logs;
			var gateOutLog = logs.MostRecentLogByEventTime(Events.GateOut);

			AssertNotNull("Expected event to have been created by updating the container date", gateOutLog);
			AssertEquals(container.JC_FCLWharfGateOut, gateOutLog.SL_EventTime);
			AssertEquals(EventConstants.Facilities.Code.Terminal, gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals("AUSYD", gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			container.JC_ContainerYardEmptyPickupGateOut = today.AddDays(2);
			gateOutLog = logs.MostRecentLogByEventTime(Events.GateOut);

			AssertEquals(container.JC_ContainerYardEmptyPickupGateOut, gateOutLog.SL_EventTime);
			AssertEquals(EventConstants.Facilities.Code.ContainerYard, gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals("Location should be consol's load port",
				"CNSHA", gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			var containerYard = Factory.NewWithValidTestData<OrgHeader>();
			containerYard.OH_IsContainerYard = true;
			containerYard.MainAddress.OA_RL_NKRelatedPortCode = "CNCAN";
			container.JC_OA_DepartureContainerYardAddress = containerYard.MainAddress.PK;

			container.JC_ContainerYardEmptyPickupGateOut = today.AddDays(3);
			gateOutLog = logs.MostRecentLogByEventTime(Events.GateOut);

			AssertEquals(container.JC_ContainerYardEmptyPickupGateOut, gateOutLog.SL_EventTime);
			AssertEquals("CY event should prefer the container yard address",
				"CNCAN", gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			container.JC_FCLWharfGateOut = today.AddDays(4);
			gateOutLog = logs.MostRecentLogByEventTime(Events.GateOut);

			AssertEquals(container.JC_FCLWharfGateOut, gateOutLog.SL_EventTime);
			AssertEquals("Wharf Gate Out event always match the consol's first load port",
				"AUSYD", gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
		}

		[TestDate(2017, 01, 05)]
		public void TestContainerFieldsUpdatedFromGateInEvents()
		{
			var today = ZDateTime.Today;
			var container = GetNewContainerOnNewImportConsol();

			var log = container.Logs.AddNew(Events.GateIn);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today;
				log.SL_Reference = "";
			}

			Assert("Expected no date change without relevant parameter information", container.JC_FCLWharfGateIn.IsEmpty);
			Assert("Expected no date change without relevant parameter information", container.JC_ContainerYardEmptyReturnGateIn.IsEmpty);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddHours(4);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "CNSHA";
			}

			AssertEquals("Expected GateIn event for a wharf to update the container event date", today.AddHours(4), container.JC_FCLWharfGateIn);
			Assert("Expected no change as the facility is not applicable", container.JC_ContainerYardEmptyReturnGateIn.IsEmpty);

			container = GetNewContainerOnNewImportConsol();
			log = container.Logs.AddNew(Events.GateIn);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(1);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			}

			AssertEquals("Expected Container yard event to update container event date", today.AddDays(1), container.JC_ContainerYardEmptyReturnGateIn);
			Assert("Wharf event should remain unchanged", container.JC_FCLWharfGateIn.IsEmpty);

			container = GetNewContainerOnNewImportConsol();
			log = container.Logs.AddNew(Events.GateIn);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(2);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "";
			}

			AssertEquals("Container yard should be updatable with an empty location", today.AddDays(2), container.JC_ContainerYardEmptyReturnGateIn);

			container = GetNewContainerOnNewImportConsol();
			log = container.Logs.AddNew(Events.GateIn);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(3);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "CNSHA";
			}

			AssertEquals("Expected wharf date to be updated", today.AddDays(3), container.JC_FCLWharfGateIn);
			Assert("Container Yard event date should remain unchanged", container.JC_ContainerYardEmptyReturnGateIn.IsEmpty);

			container = GetNewContainerOnNewImportConsol();
			log = container.Logs.AddNew(Events.GateIn);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(5);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "GBLON";
			}

			Assert("Expected no change for unrelated location", container.JC_ContainerYardEmptyReturnGateIn.IsEmpty);

			container = GetNewContainerOnNewImportConsol();
			var gateOutLog = container.Logs.AddNew(Events.GateOut);
			using (gateOutLog.LockForUpdatingKeyFieldsForTesting())
			{
				gateOutLog.SL_EventTime = today.AddDays(5);
				gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			}

			using (gateOutLog.LockForUpdatingKeyFieldsForTesting())
			{
				gateOutLog.SL_EventTime = today.AddDays(5);
				gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "CNSHA";
			}

			Assert("Expected Gate Out event not to change Gate In date", container.JC_FCLWharfGateIn.IsEmpty);
			Assert("Expected Gate Out event not to change Gate In date", container.JC_ContainerYardEmptyReturnGateIn.IsEmpty);
		}

		[TestDate(2017, 01, 05)]
		public void TestContainerFieldsUpdatedFromGateOutEvents()
		{
			var today = ZDateTime.Today;
			var container = GetNewContainerOnNewImportConsol();

			var log = container.Logs.AddNew(Events.GateOut);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today;
				log.SL_Reference = "";
			}

			Assert("Expected no date change without relevant parameter information", container.JC_FCLWharfGateOut.IsEmpty);
			Assert("Expected no date change without relevant parameter information", container.JC_ContainerYardEmptyPickupGateOut.IsEmpty);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddHours(6);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			}

			AssertEquals("Expected GateOut event for a wharf to update the container event date", today.AddHours(6), container.JC_FCLWharfGateOut);
			Assert("Expected no change as the facility is not applicable", container.JC_ContainerYardEmptyPickupGateOut.IsEmpty);

			container = GetNewContainerOnNewImportConsol();
			log = container.Logs.AddNew(Events.GateOut);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(1);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "CNSHA";
			}

			AssertEquals("Expected date change to now apply", today.AddDays(1), container.JC_ContainerYardEmptyPickupGateOut);
			Assert("Wharf event should remain unchanged", container.JC_FCLWharfGateOut.IsEmpty);

			container = GetNewContainerOnNewImportConsol();
			log = container.Logs.AddNew(Events.GateOut);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(2);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "";
			}

			AssertEquals("Container yard should be updatable with an empty location", today.AddDays(2), container.JC_ContainerYardEmptyPickupGateOut);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(4);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			}

			AssertEquals("Expected wharf date to be updated", today.AddDays(4), container.JC_FCLWharfGateOut);

			container = GetNewContainerOnNewImportConsol();
			log = container.Logs.AddNew(Events.GateOut);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(5);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "USLAX";
			}

			AssertEquals("GOU event wouldn't check location deciding to update JC_ContainerYardEmptyPickupGateOut", today.AddDays(5), container.JC_ContainerYardEmptyPickupGateOut);

			container = GetNewContainerOnNewImportConsol();
			var gateInLog = container.Logs.AddNew(Events.GateIn);
			using (gateInLog.LockForUpdatingKeyFieldsForTesting())
			{
				gateInLog.SL_EventTime = today.AddDays(10);
				gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "";
			}

			using (gateInLog.LockForUpdatingKeyFieldsForTesting())
			{
				gateInLog.SL_EventTime = today.AddDays(10);
				gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "CNSHA";
			}

			Assert("Expected Gate In event not to change Gate Out date", container.JC_ContainerYardEmptyPickupGateOut.IsEmpty);
			Assert("Expected Gate In event not to change Gate Out date", container.JC_FCLWharfGateOut.IsEmpty);
		}

		[TestDate(2014, 09, 09)]
		public void TestDehireEvents()
		{
			var today = ZDateTime.Today;
			var container = GetNewContainerOnNewImportConsol();

			var dehireLog = container.Logs.AddNew(Events.Dehire);
			using (dehireLog.LockForUpdatingKeyFieldsForTesting())
			{
				dehireLog.SL_EventTime = today;
			}

			AssertEquals("Date should be updated from Dehire event without any parameters", today, container.JC_ContainerYardEmptyReturnGateIn);

			using (dehireLog.LockForUpdatingKeyFieldsForTesting())
			{
				dehireLog.SL_EventTime = today.AddDays(3);
				dehireLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				dehireLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "GBLON";
			}

			AssertEquals("Date should be updated regardless of parameters", today.AddDays(3), container.JC_ContainerYardEmptyReturnGateIn);

			container.JC_ContainerYardEmptyReturnGateIn = today.AddDays(5);

			dehireLog = container.Logs.MostRecentLogByEventTime(Events.Dehire);

			AssertEquals("Log with a new event time should have been created", today.AddDays(5), dehireLog.SL_EventTime);
		}

		public void TestGateInContainerYardCreatesAndUpdatesDehireEvents_WhenGateInContainerYardDateIsSetDirectlyFromImportTab()
		{
			var arrivalContainerYard = Factory.NewWithValidTestData<OrgHeader>();
			var arrivalAddress = arrivalContainerYard.MainAddress;
			arrivalAddress.OA_RL_NKRelatedPortCode = "CNCAN";

			var today = ZDateTime.Today;
			var container = GetNewContainerOnNewImportConsol();
			container.JC_OA_ArrivalContainerYardAddress = arrivalAddress.PK;
			container.JC_ContainerYardEmptyReturnGateIn = today;

			var gateInLog = container.Logs.MostRecentLogByEventTime(Events.GateIn);
			var dehireLog = container.Logs.MostRecentLogByEventTime(Events.Dehire);
			AssertEquals("Expected GateIn event to be created with date matching JC_ContainerYardEmptyReturnGateIn", container.JC_ContainerYardEmptyReturnGateIn, gateInLog.SL_EventTime);
			AssertEquals("Expected Dehire event to be created with date matching GateIn log time", gateInLog.SL_EventTime, dehireLog.SL_EventTime);

			AssertEquals("Expected Dehire event to prefer container yard address over consol dischange port", "CNCAN", dehireLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals("Expected Dehire event facility parameter to be container yard", EventConstants.Facilities.Code.ContainerYard, dehireLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
		}

		public void TestGateInContainerYardCreatesAndUpdatesDehireEvents_WhenGateInContainerYardDateIsSetFromNewGateInEvent()
		{
			var today = ZDateTimeOffset.Today;
			var container = GetNewContainerOnNewImportConsol();

			var parameters = new Dictionary<string, string>();
			parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
			parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";

			var newGateInEvent1 = new EventValue(Events.GateIn, eventTime: today, parameters: parameters);
			container.Logs.AddNew(newGateInEvent1);

			var logCount = container.Logs.GetAllLogs().Count;
			AssertEquals("Empty JC_ContainerYardEmptyReturnGateIn field should be updated from Gate In event", today.ToZDateTime(), container.JC_ContainerYardEmptyReturnGateIn);
			AssertEquals("Expected GateIn event to cause a single dehire event to be created as a sub-product of updating an empty JC_ContainerYardEmptyReturnGateIn", 2, logCount);

			var gateInLog = container.Logs.MostRecentLogByEventTime(Events.GateIn);
			var dehireLog = container.Logs.MostRecentLogByEventTime(Events.Dehire);
			AssertEquals("Expected GateIn event to be created with date matching JC_ContainerYardEmptyReturnGateIn", container.JC_ContainerYardEmptyReturnGateIn, gateInLog.SL_EventTime);
			AssertEquals("Expected Dehire event to be created with date matching GateIn log time", gateInLog.SL_EventTime, dehireLog.SL_EventTime);

			var newGateInEvent2 = new EventValue(Events.GateIn, eventTime: today.AddDays(1), parameters: parameters);
			container.Logs.AddNew(newGateInEvent2);

			logCount = container.Logs.GetAllLogs().Count;
			AssertEquals("New GateIn event should not update an existing container event date", today.ToZDateTime(), container.JC_ContainerYardEmptyReturnGateIn);
			AssertEquals("Only a single GateIn event should be added when JC_ContainerYardEmptyReturnGateIn remains unchanged", 3, logCount);

			gateInLog = container.Logs.MostRecentLogByEventTime(Events.GateIn);
			dehireLog = container.Logs.MostRecentLogByEventTime(Events.Dehire);
			AssertEquals("The most recent gate in event should match the second event created", newGateInEvent2.EventTime, gateInLog.SL_EventTimeOffset);
			AssertEquals("The most recent dehire event should match the first event created", newGateInEvent1.EventTime, dehireLog.SL_EventTimeOffset);
		}

		[TestDate(2014, 09, 09)]
		public void TestGateInOutAndDehireEventsUpdateContainersConsol()
		{
			var today = ZDateTime.Today;

			var container1 = GetNewContainerOnNewImportConsol();
			var consol = container1.Consol;
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_TransportMode = Constants.TransportModes.Road;
			var container2 = consol.Containers.AddNew();

			container1.JC_ContainerYardEmptyReturnGateIn = today;
			container1.JC_ContainerYardEmptyPickupGateOut = today.AddDays(4);

			AssertNotNull("Pre-condition: container has event generated",
				container1.Logs.MostRecentLogByEventTime(Events.GateIn));
			AssertNotNull(container1.Logs.MostRecentLogByEventTime(Events.Dehire));
			AssertNotNull(container1.Logs.MostRecentLogByEventTime(Events.GateOut));

			var consolLogs = consol.Logs;
			AssertNull("Expected no event on the consol until all containers have the event",
				consolLogs.MostRecentLogByEventTime(Events.GateIn));
			AssertNull(consolLogs.MostRecentLogByEventTime(Events.Dehire));
			AssertNull(consolLogs.MostRecentLogByEventTime(Events.GateOut));

			container2.JC_ContainerYardEmptyReturnGateIn = today.AddDays(1);

			AssertNotNull("Pre-condition", container2.Logs.MostRecentLogByEventTime(Events.GateIn));
			AssertNotNull("Pre-condition", container2.Logs.MostRecentLogByEventTime(Events.Dehire));

			AssertNotNull("Consol should have event log now that both containers have Gate In Events",
				consolLogs.MostRecentLogByEventTime(Events.GateIn));
			AssertNotNull(consolLogs.MostRecentLogByEventTime(Events.Dehire));
			AssertNull("Expected still to be null as container 2 doesn't have a gate out",
				consolLogs.MostRecentLogByEventTime(Events.GateOut));

			var container3 = consol.Containers.AddNew();
			container2.JC_ContainerYardEmptyPickupGateOut = today.AddDays(21);

			AssertNull("Expected still to be null a new container has been added before container 2 generated event",
				consolLogs.MostRecentLogByEventTime(Events.GateOut));

			container3.JC_ContainerYardEmptyPickupGateOut = today.AddDays(20);

			AssertNotNull("Expected to have been generated now that all three containers have the event",
				consolLogs.MostRecentLogByEventTime(Events.GateOut));
		}

		[TestDate(2014, 09, 09)]
		public void TestConsolPropagation_GateInAndOutEvents()
		{
			var today = ZDateTime.Today;

			var container1 = GetNewContainerOnNewImportConsol();
			var consol = container1.Consol;
			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			var container2 = consol.Containers.AddNew();
			container2.JC_FCLWharfGateIn = today.AddHours(8);
			container2.JC_FCLWharfGateOut = today.AddDays(26);

			container1.JC_ContainerYardEmptyPickupGateOut = today;
			container1.JC_ContainerYardEmptyReturnGateIn = today.AddDays(30);

			var containerGateIn = consol.Logs.MostRecentLogByEventTime(Events.GateIn);
			var containerGateOut = consol.Logs.MostRecentLogByEventTime(Events.GateOut);

			AssertNull("Both containers have a GIN event but the parameters don't match so consol event should NOT be propagated", containerGateIn);
			AssertNull("Both containers have a GOU event but the parameters don't match so consol event should NOT be propagated", containerGateOut);

			container1.JC_FCLWharfGateIn = today.AddHours(8);
			container2.JC_ContainerYardEmptyPickupGateOut = today.AddDays(28);

			containerGateIn = consol.Logs.MostRecentLogByEventTime(Events.GateIn);
			containerGateOut = consol.Logs.MostRecentLogByEventTime(Events.GateOut);

			AssertNotNull("Both containers have a GIN event with matching parameters so consol should have an event propagated", containerGateIn);
			AssertNotNull("Both containers have a GOU event with matching parameters so consol should have an event propagated", containerGateOut);
		}

		[TestDate(2017, 01, 05)]
		public void TestContainerFieldsUpdatedFromGateInEventsAndTransportLeg()
		{
			var today = ZDateTime.Today;
			var container = GetNewContainerOnNewImportConsol();
			container.Consol.JK_RL_NKLoadPort = "";
			container.Consol.JK_RL_NKDischargePort = "";

			container.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			container.Consol.Transports[0].JW_RL_NKDiscPort = "USNYC";
			container.Consol.Transports.AddNew("USNYC", "AUBNE");

			var log = container.Logs.AddNew(Events.GateIn);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today;
				log.SL_Reference = "";
			}

			Assert("Expected no date change without relevant parameter information", container.JC_FCLWharfGateIn.IsEmpty);
			Assert("Expected no date change without relevant parameter information", container.JC_ContainerYardEmptyReturnGateIn.IsEmpty);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(5);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "GBLON";
			}

			Assert("Expected no change for unrelated location", container.JC_ContainerYardEmptyReturnGateIn.IsEmpty);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddHours(4);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			}

			AssertEquals("Expected GateIn event for a wharf to update the container event date", today.AddHours(4), container.JC_FCLWharfGateIn);
			Assert("Expected no change as the facility is not applicable", container.JC_ContainerYardEmptyReturnGateIn.IsEmpty);

			container = GetNewContainerOnNewImportConsol();
			container.Consol.JK_RL_NKLoadPort = "";
			container.Consol.JK_RL_NKDischargePort = "";

			container.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			container.Consol.Transports[0].JW_RL_NKDiscPort = "USNYC";
			container.Consol.Transports.AddNew("USNYC", "AUBNE");

			log = container.Logs.AddNew(Events.GateIn);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(1);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUBNE";
			}

			AssertEquals("Expected date change to now apply", today.AddDays(1), container.JC_ContainerYardEmptyReturnGateIn);
			Assert("Wharf event should remain unchanged", container.JC_FCLWharfGateIn.IsEmpty);
		}

		[TestDate(2017, 01, 05)]
		public void TestContainerFieldsUpdatedFromGateOutEventsAndTransportLeg()
		{
			var today = ZDateTime.Today;
			var container = GetNewContainerOnNewImportConsol();
			container.Consol.JK_RL_NKLoadPort = "";
			container.Consol.JK_RL_NKDischargePort = "";

			container.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			container.Consol.Transports[0].JW_RL_NKDiscPort = "USNYC";
			container.Consol.Transports.AddNew("USNYC", "AUBNE");

			var log = container.Logs.AddNew(Events.GateOut);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today;
				log.SL_Reference = "";
			}

			Assert("Expected no date change without relevant parameter information", container.JC_FCLWharfGateOut.IsEmpty);
			Assert("Expected no date change without relevant parameter information", container.JC_ContainerYardEmptyPickupGateOut.IsEmpty);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(5);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "USLAX";
			}

			AssertEquals("GOU event wouldn't check location deciding to update JC_ContainerYardEmptyPickupGateOut", today.AddDays(5), container.JC_ContainerYardEmptyPickupGateOut);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddHours(6);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUBNE";
			}

			AssertEquals("Expected GateOut event for a wharf to update the container event date", today.AddHours(6), container.JC_FCLWharfGateOut);
			AssertEquals("Expected no change as the facility is not applicable", today.AddDays(5), container.JC_ContainerYardEmptyPickupGateOut);

			container = GetNewContainerOnNewImportConsol();
			container.Consol.JK_RL_NKLoadPort = "";
			container.Consol.JK_RL_NKDischargePort = "";

			container.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			container.Consol.Transports[0].JW_RL_NKDiscPort = "USNYC";
			container.Consol.Transports.AddNew("USNYC", "AUBNE");

			log = container.Logs.AddNew(Events.GateOut);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(1);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			}

			AssertEquals("Expected date change to now apply", today.AddDays(1), container.JC_ContainerYardEmptyPickupGateOut);
			Assert("Wharf event should remain unchanged", container.JC_FCLWharfGateOut.IsEmpty);
		}

		[TestDate(2017, 01, 05)]
		public void TestContainerFieldsUpdatedFromFCLWharfGateInEventsOnlyWhenEmpty()
		{
			var today = ZDateTime.Today;
			var container = GetNewContainerOnNewImportConsol();

			var log1 = container.Logs.AddNew(Events.GateIn);
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_EventTime = today;
				log1.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				log1.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "CNSHA";
			}

			AssertEquals("Expected GateIn event to update the container event date", today, container.JC_FCLWharfGateIn);

			var log2 = container.Logs.AddNew(Events.GateIn);
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_EventTime = today.AddDays(3);
				log2.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				log2.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "CNSHA";
			}

			AssertEquals("Existing date should not be updated when new log is added", today, container.JC_FCLWharfGateIn);
		}

		[TestDate(2017, 01, 05)]
		public void TestContainerFieldsUpdatedFromFCLWharfGateOutEventsOnlyWhenEmpty()
		{
			var today = ZDateTime.Today;
			var container = GetNewContainerOnNewImportConsol();

			var log1 = container.Logs.AddNew(Events.GateOut);
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_EventTime = today;
				log1.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				log1.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			}

			AssertEquals("Expected GateOut event to update the container event date", today, container.JC_FCLWharfGateOut);

			var log2 = container.Logs.AddNew(Events.GateOut);
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_EventTime = today.AddDays(3);
				log2.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				log2.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			}

			AssertEquals("Existing date should not be updated when new log is added", today, container.JC_FCLWharfGateOut);
		}

		[TestDate(2017, 01, 05)]
		public void TestContainerFieldsUpdatedFromContainerYardEmptyReturnGateInEventsOnlyWhenEmpty()
		{
			var today = ZDateTime.Today;
			var container = GetNewContainerOnNewImportConsol();

			var log1 = container.Logs.AddNew(Events.GateIn);
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_EventTime = today;
				log1.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log1.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			}

			AssertEquals("Expected GateIn event to update the container event date", today, container.JC_ContainerYardEmptyReturnGateIn);

			var log2 = container.Logs.AddNew(Events.GateIn);
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_EventTime = today.AddDays(3);
				log2.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log2.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			}

			AssertEquals("Existing date should not be updated when new log is added", today, container.JC_ContainerYardEmptyReturnGateIn);
		}

		[TestDate(2017, 01, 05)]
		public void TestContainerFieldsUpdatedFromContainerYardEmptyPickupGateOutEventsOnlyWhenEmpty()
		{
			var today = ZDateTime.Today;
			var container = GetNewContainerOnNewImportConsol();

			var log1 = container.Logs.AddNew(Events.GateOut);
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_EventTime = today;
				log1.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log1.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "CNSHA";
			}

			AssertEquals("Expected GateOut event to update the container event date", today, container.JC_ContainerYardEmptyPickupGateOut);

			var log2 = container.Logs.AddNew(Events.GateOut);
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_EventTime = today.AddDays(3);
				log2.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log2.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "CNSHA";
			}

			AssertEquals("Existing date should not be updated when new log is added", today, container.JC_ContainerYardEmptyPickupGateOut);
		}

		public void TestSailingCTOAvailableDate()
		{
			var container = Factory.New<ForwardingContainer>();
			AssertEquals(ZDateTime.Empty, container.SailingCTOAvailableDate);

			VoyageDestination destination = Factory.NewWithValidTestData<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			JobSailing sailing = Factory.NewWithValidTestData<JobSailing>();
			Factory.Save();
			sailing.JX_JB = destination.PK;
			container.JC_JX = sailing.PK;
			destination.JB_AvailabilityDate = ZDateTime.Today.AddDays(1);

			AssertEquals(destination.JB_AvailabilityDate, container.SailingCTOAvailableDate);
		}

		public void TestRequireCalculateJC_EmptyReturnedBy()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			AssertEquals(1, consol.Transports.Count);
			var transport = consol.Transports[0];
			transport.JW_ETA = ZDateTime.Today.AddDays(10);

			var container = Factory.New<ForwardingContainer>();
			container.JC_ContainerNum = "ZIMU3144818";
			consol.Containers.Add(container);

			Factory.Save();

			AssertEquals(ZDateTime.Empty, transport.JW_ATA);
			AssertEquals(ZDateTime.Empty, consol.JK_ATAForLastTransport);

			transport.JW_ATA = ZDateTime.Today.AddDays(11);

			AssertEquals(ZDateTime.Today.AddDays(11), consol.JK_ATAForLastTransport);
			Assert(transport.JW_ATAInfo.HasChanges);
			Assert(consol.JK_ATAForLastTransportInfo.HasChanges);
			Assert(container.RequireCalculateJC_EmptyReturnedBy);
			Factory.Save();
			AssertEquals(false, container.RequireCalculateJC_EmptyReturnedBy);

			transport.JW_ETD = ZDateTime.Today;
			Assert(consol.JK_DepartureForFirstTransportInfo.HasChanges);
			AssertEquals(transport.JW_ETD, consol.JK_DepartureForFirstTransport);
			Assert(container.RequireCalculateJC_EmptyReturnedBy);
			Factory.Save();
			AssertEquals(false, container.RequireCalculateJC_EmptyReturnedBy);

			transport.JW_ATD = ZDateTime.Today.AddDays(1);
			Assert(consol.JK_DepartureForFirstTransportInfo.HasChanges);
			AssertEquals(transport.JW_ATD, consol.JK_DepartureForFirstTransport);
			Assert(container.RequireCalculateJC_EmptyReturnedBy);
			Factory.Save();
			AssertEquals(false, container.RequireCalculateJC_EmptyReturnedBy);

			container.JC_FCLWharfGateOut = ZDateTime.Today;
			Assert(container.RequireCalculateJC_EmptyReturnedBy);
			Factory.Save();
			AssertEquals(false, container.RequireCalculateJC_EmptyReturnedBy);

			container.JC_FCLWharfGateIn = ZDateTime.Today;
			Assert(container.RequireCalculateJC_EmptyReturnedBy);
			Factory.Save();
			AssertEquals(false, container.RequireCalculateJC_EmptyReturnedBy);

			container.JC_FCLUnloadFromVessel = ZDateTime.Today;
			Assert(container.RequireCalculateJC_EmptyReturnedBy);
			Factory.Save();
			AssertEquals(false, container.RequireCalculateJC_EmptyReturnedBy);

			container.JC_FCLOnBoardVessel = ZDateTime.Today;
			Assert(container.RequireCalculateJC_EmptyReturnedBy);
			Factory.Save();
			AssertEquals(false, container.RequireCalculateJC_EmptyReturnedBy);

			VoyageDestination destination = Factory.NewWithValidTestData<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			JobSailing sailing = Factory.NewWithValidTestData<JobSailing>();
			Factory.Save();
			sailing.JX_JB = destination.PK;
			container.JC_JX = sailing.PK;
			transport.JW_JX = sailing.PK;
			destination.JB_AvailabilityDate = ZDateTime.Today.AddDays(1);

			Assert(container.SailingCTOAvailableDateInfo.HasChanges);
			Assert(container.RequireCalculateJC_EmptyReturnedBy);
			Factory.Save();
			AssertEquals(false, container.RequireCalculateJC_EmptyReturnedBy);

			container.JC_FCLAvailable = ZDateTime.Today;
			Assert(container.RequireCalculateJC_EmptyReturnedBy);
			Factory.Save();
			AssertEquals(false, container.RequireCalculateJC_EmptyReturnedBy);

			var shipment = consol.Shipments.AddNew();
			var cartage = (BusinessObject)Factory.New<ICommonCartage>();
			Factory.Save();
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			container.JC_JX = ZGuid.Empty;
			transport.JW_JX = sailing.PK;
			container.JC_JS_FCLBookingOnlyLink = shipment.PK;
			cartage[JobCartageSchema.JJ_ParentID] = shipment.PK;
			cartage[JobCartageSchema.JJ_ParentTableCode] = JobShipmentSchema.Constants.Prefix;
			cartage[JobCartageSchema.JJ_ConsignmentID] = shipment.JS_UniqueConsignRef + "/I";
			cartage[JobCartageSchema.JJ_JX_Sailing] = sailing.PK;

			var bookedCartageContainerMove = (BusinessObject)Factory.New<ICommonBookedCtgMove>();
			bookedCartageContainerMove[JobBookedCtgMoveSchema.EW_JC_Container] = container.PK;
			bookedCartageContainerMove[JobBookedCtgMoveSchema.EW_JJ] = cartage.PK;

			container.JC_OverrideFCLAvailableStorage = true;
			transport.JW_TerminalAvailabilityDate = ZDateTime.Today.AddDays(1);
			destination.JB_AvailabilityDate = ZDateTime.Today.AddDays(1);
			AssertEquals("JC_OverrideFCLAvailableStorage is true and JC_FCLAvailable has not been changed.", false, container.RequireCalculateJC_EmptyReturnedBy);
			Factory.Save();

			transport.JW_TerminalAvailabilityDate = ZDateTime.Today.AddDays(2);
			container.JC_OverrideFCLAvailableStorage = false;
			Assert("JC_OverrideFCLAvailableStorage is false and ArrivalTransport.JW_TerminalAvailabilityDate has been changed.", container.RequireCalculateJC_EmptyReturnedBy);
			Factory.Save();

			destination.JB_AvailabilityDate = ZDateTime.Today.AddDays(2);
			AssertEquals("JC_OverrideFCLAvailableStorage is false and ArrivalTransport.JW_TerminalAvailabilityDate has not been changed.", false, container.RequireCalculateJC_EmptyReturnedBy);

			container.JC_JK = ZGuid.Empty;
			Factory.Save();
			destination.JB_AvailabilityDate = ZDateTime.Today.AddDays(3);
			Assert("JC_OverrideFCLAvailableStorage is false and no ArrivalTransport set, Destination.JB_AvailabilityDate has been changed.", container.RequireCalculateJC_EmptyReturnedBy);
		}

		#endregion

		#region PackSynchronise

		public void TestPackSynchronise()
		{
			var container = Factory.New<ForwardingConsol>().Containers.AddNew();
			container.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			container.JC_TareWeight = 2000m;

			var sync = new PackSynchroniser();
			container.PackLineSynchroniser = sync;

			container.ITReferenceNumber = "432098";
			AssertEquals(2, sync.Index);

			container.AMSNumber = "34278";
			AssertEquals(4, sync.Index);

			AssertEquals("34278", ((IBillDetails)container).BillNumberInfo.Value);
		}

		class PackSynchroniser : Enterprise.Integration.Freight.IPackLineSynchronise
		{
			public int Index;
			public void MarkSyncDirty()
			{
				Index++;
			}

			public void CleanForConcurrency()
			{
			}
		}

		#endregion

		#region Container Tracking subsciption test

		public void TestSaving_RequestTrackingSubscription_NoShippingLine_NoSubscription()
		{
			TestSubscriptionCase(
				shippingLine: null,
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				isEventExpected: false);
		}

		public void TestSaving_RequestTrackingSubscription_NoSCAC_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>(),
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				isEventExpected: true);
		}

		public void TestSaving_RequestTrackingSubscription_NoBookingReference_NoMasterBill_NoSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				containerNum: "AAAA1234566",
				isEventExpected: false);
		}

		public void TestSaving_RequestTrackingSubscription_InvalidBookingReference1_NoSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: "1234567",
				containerNum: "AAAA1234566",
				isEventExpected: false);
		}

		public void TestSaving_RequestTrackingSubscription_InvalidBookingReference2_NoSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: "123456789012345678901",
				containerNum: "AAAA1234566",
				isEventExpected: false);
		}

		public void TestSaving_RequestTrackingSubscription_InvalidBookingReference3_NoSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: "1234567@",
				containerNum: "AAAA1234566",
				isEventExpected: false);
		}

		public void TestSaving_RequestTrackingSubscription_NoContainerNumber_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				masterBillNumber: "12345678",
				isEventExpected: true);
		}

		public void TestSaving_RequestTrackingSubscription_TransportModeIsNotSEA_NoSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				masterBillNumber: "12345678",
				transportMode: Constants.TransportModes.Air,
				containerNum: "AAAA1234566",
				isEventExpected: false);
		}

		public void TestSaving_RequestTrackingSubscription_SCACIsNotForUS_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "AU"),
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				isEventExpected: true);
		}

		public void TestSaving_RequestTrackingSubscription_InvalidSCAC_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "12345", "US"),
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				isEventExpected: true);
		}

		public void TestSaving_RequestTrackingSubscription_EverythingProvided1_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: @"12345678901234567-/\",
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				isEventExpected: true);
		}

		public void TestSaving_RequestTrackingSubscription_EverythingProvided2_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				isEventExpected: true);
		}

		public void TestSaving_RequestTrackingSubscription_ConsolIsTooOld_WithSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				isEventExpected: true,
				additionalSetup: (consol, container) => { consol.Transports[0].JW_ATA = ZDateTime.Today.AddMonths(-2).AddDays(-1); });
		}

		public void TestSaving_RequestTrackingSubscription_ConsolIsRecent_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				isEventExpected: true,
				additionalSetup: (consol, container) => { consol.Transports[0].JW_ATA = ZDateTime.Today.AddDays(-1); });
		}

		public void TestSaving_RequestTrackingSubscription_ContainerValidDehireEvent_NoSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				isEventExpected: false,
				additionalSetup: (consol, container) => { container.Logs.AddNew(Events.Dehire); });
		}

		public void TestSaving_RequestTrackingSubscription_ContainerTrackingIsDisabled_NoSubscription()
		{
			TestSubscriptionCase(
				isTrackingEnabled: false,
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				isEventExpected: false);
		}

		public void TestSaving_RequestTrackingSubscription_Resave_EverythingProvided_ResendSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US"),
				bookingReference: "66666666",
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				eventAlreadyExists: true,
				isEventExpected: true);
		}

		public void TestSaving_RequestTrackingSubscription_Resave_NoContainerNumber_RequestSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA"),
				masterBillNumber: "12345678",
				containerNum: "",
				eventAlreadyExists: true,
				isEventExpected: true);
		}

		public void TestSaving_RequestTrackingSubscription_NoBookingReference_NoMasterBill_CancelSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA"),
				containerNum: "AAAA1234566",
				eventAlreadyExists: true,
				isEventExpected: false); //TODO: Shouldn't we resend such events?
		}

		public void TestSaving_RequestTrackingSubscription_Resave_InvalidMasterBill1_ResendSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA"),
				masterBillNumber: "1234567",
				containerNum: "AAAA1234566",
				eventAlreadyExists: true,
				isEventExpected: true);
		}

		public void TestSaving_RequestTrackingSubscription_Resave_InvalidMasterBill2_ResendSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA"),
				masterBillNumber: "123456789012345678901",
				containerNum: "AAAA1234566",
				eventAlreadyExists: true,
				isEventExpected: true);
		}

		public void TestSaving_RequestTrackingSubscription_Resave_InvalidMasterBill3_ResendSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA"),
				masterBillNumber: "1234567@",
				containerNum: "AAAA1234566",
				eventAlreadyExists: true,
				isEventExpected: true);
		}

		public void TestSaving_RequestTrackingSubscription_Resave_NoSCAC_ResendSubscription()
		{
			TestSubscriptionCase(
				shippingLine: Factory.NewWithValidTestData<OrgHeader>(),
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				eventAlreadyExists: true,
				isEventExpected: true);
		}

		public void TestSaving_RequestTrackingSubscription_Resave_NoShippingLine_CancelSubscription()
		{
			TestSubscriptionCase(
				shippingLine: null,
				masterBillNumber: "12345678",
				containerNum: "AAAA1234566",
				eventAlreadyExists: true,
				isEventExpected: false); //TODO: Shouldn't we resend such events?
		}

		public void TestSaving_SubscriptionDetailsAreModified_RenewSubscription()
		{
			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var eventParameters = new[] { EventConstants.EventReferenceParameters.Codes.Type.AsKeyFor(Constants.EventReferenceParameterTypes.ContainerTracking) };
				var eventReference = StmALog.GenerateEventReference("", eventParameters);

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_MasterBillNum = "12345678";
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_OA_ShippingLineAddress = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA", "US").MainAddress.PK;

				consol.Transports[0].JW_ETA = ZDateTime.Today;

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "AAAA1111113";

				Factory.Save();

				var log11 = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNotNull("Log", log11);

				var log12 = container.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNull("Log", log12);

				// Master Bill change
				consol.JK_MasterBillNum = "23456789";
				Factory.Save();

				var log21 = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				Assert("Event has been recreated after master bill change", log21.SL_EventTime > log11.SL_EventTime);
				AssertEquals("Event should have local datetime", ZDateTime.Now.ToSmallDateTime(), log21.SL_EventTime.ToSmallDateTime());

				var log22 = container.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNull("Log", log22);

				// Shipping Line change
				consol.JK_OA_ShippingLineAddress = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "BBBB", "US").MainAddress.PK;
				Factory.Save();
				var log31 = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				Assert("Event has been recreated after shipping line change", log31.SL_EventTime > log21.SL_EventTime);

				var log32 = container.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNull("Log", log32);

				// Container Number change
				container.JC_ContainerNum = "AAAA2222220";
				Factory.Save();
				var log41 = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				Assert("Event has been recreated after container number change", log41.SL_EventTime > log31.SL_EventTime);

				var log42 = container.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNull("Log", log42);

				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
				consol.JK_BookingReference = "12345678";
				container.JC_ContainerNum = "AAAA3333336";
				Factory.Save();

				var log51 = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				Assert("Event has been recreated after container number change for coload consol", log51.SL_EventTime > log41.SL_EventTime);

				var log52 = container.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
				AssertNull("Log", log52);
			}
		}

		public void TestSaving_SubscriptionDetailsAreModified_DeleteContainer()
		{
			// Arrange
			var eventParameters = new[] { EventConstants.EventReferenceParameters.Codes.Type.AsKeyFor(Constants.EventReferenceParameterTypes.ContainerTracking) };
			var eventReference = StmALog.GenerateEventReference("", eventParameters);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_MasterBillNum = "12345678";
			consol.JK_BookingReference = "12345678";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_ShippingLineAddress = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAAA").MainAddress.PK;

			consol.Transports[0].JW_ETA = ZDateTime.Today;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA3333336";
			Factory.Save();

			var logBeforeDelete = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
			AssertNotNull(logBeforeDelete);

			// Act
			container.Delete();
			Factory.Save();

			// Assert
			var logAfterDelete = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
			AssertNotNull(logAfterDelete);
			Assert(logAfterDelete.SL_EventTime > logBeforeDelete.SL_EventTime);
		}

		void TestSubscriptionCase(
			bool isTrackingEnabled = true,
			OrgHeader shippingLine = null,
			string masterBillNumber = "",
			string bookingReference = "",
			string containerNum = "",
			string transportMode = Constants.TransportModes.Sea,
			bool eventAlreadyExists = false,
			bool isEventExpected = false,
			Action<ForwardingConsol, ForwardingContainer> additionalSetup = null)
		{
			var eventParameters = new[] { EventConstants.EventReferenceParameters.Codes.Type.AsKeyFor(Constants.EventReferenceParameterTypes.ContainerTracking) };
			var eventReference = StmALog.GenerateEventReference("", eventParameters);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_MasterBillNum = masterBillNumber;
			consol.JK_BookingReference = bookingReference;
			consol.JK_TransportMode = transportMode;
			consol.JK_OA_ShippingLineAddress = shippingLine != null ? shippingLine.MainAddress.PK : ZGuid.Empty;

			consol.Transports[0].JW_ETA = ZDateTime.Today;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = containerNum;

			if (eventAlreadyExists)
			{
				consol.Logs.AddNew(Events.SubscriptionRequested, eventReference, ZDateTimeOffset.Now, false);
			}

			additionalSetup?.Invoke(consol, container);

			using (FreightDataRegistry.Instance.ContainerAutomation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isTrackingEnabled))
			{
				Factory.Save();
			}

			var log = consol.Logs.MostRecentLogByEventTime(Events.SubscriptionRequested, eventReference);
			AssertEquals("Event created", isEventExpected, log != null);
		}

		#endregion

		#region Pickup/Delivery date event log

		public void TestJC_DepartureEstimatedPickupEventLog()
		{
			var container = Factory.New<ForwardingContainer>();
			var stmALog = container.Logs.MostRecentLogByEventTime(Events.PickedUp);
			AssertNull("No PickedUp event log", stmALog);

			var eventTime = ZDateTime.Now;
			container.JC_DepartureEstimatedPickup = eventTime;
			stmALog = container.Logs.MostRecentLogByEventTime(Events.PickedUp);
			AssertNotNull("PickedUp event log created", stmALog);
			AssertEquals("PickedUp event time", eventTime, stmALog.SL_EventTime);
			AssertEquals("SL_IsEstimate", true, stmALog.SL_IsEstimate);
			AssertEquals("SL_Reference", "|TYP=FUL", stmALog.SL_Reference);
			var count = container.Logs.Find(log => log.SL_SE_NKEvent == Events.PickedUpCode).Count();
			AssertEquals("PickedUp event log count should be 1", 1, count);

			var eventTime2 = eventTime.AddSeconds(1);
			container.JC_DepartureEstimatedPickup = eventTime2;
			stmALog = container.Logs.MostRecentLogByEventTime(Events.PickedUp);
			AssertNotNull("PickedUp event log updated", stmALog);
			AssertEquals("PickedUp event time 2", eventTime2, stmALog.SL_EventTime);
			count = container.Logs.Find(log => log.SL_SE_NKEvent == Events.PickedUpCode).Count();
			AssertEquals("PickedUp event log count should be still 1", 1, count);
		}

		public void TestJC_DepartureCartageCompleteEventLog()
		{
			var container = Factory.New<ForwardingContainer>();
			var stmALog = container.Logs.MostRecentLogByEventTime(Events.PickedUp);
			AssertNull("No PickedUp event log", stmALog);

			var eventTime = ZDateTime.Now;
			container.JC_DepartureCartageComplete = eventTime;
			stmALog = container.Logs.MostRecentLogByEventTime(Events.PickedUp);
			AssertNotNull("PickedUp event log created", stmALog);
			AssertEquals("PickedUp event time", eventTime, stmALog.SL_EventTime);
			AssertEquals("IsEstimate", false, stmALog.SL_IsEstimate);
			AssertEquals("SL_Reference", "|TYP=FUL", stmALog.SL_Reference);
			var count = container.Logs.Find(log => log.SL_SE_NKEvent == Events.PickedUpCode).Count();
			AssertEquals("PickedUp event log count should be 1", 1, count);

			var eventTime2 = eventTime.AddSeconds(1);
			container.JC_DepartureCartageComplete = eventTime2;
			stmALog = container.Logs.MostRecentLogByEventTime(Events.PickedUp);
			AssertNotNull("PickedUp event log updated", stmALog);
			AssertEquals("PickedUp event time 2", eventTime2, stmALog.SL_EventTime);
			count = container.Logs.Find(log => log.SL_SE_NKEvent == Events.PickedUpCode).Count();
			AssertEquals("PickedUp event log count should be still 1", 1, count);
		}

		public void TestJC_ArrivalEstimatedDeliveryEventLog()
		{
			var container = Factory.New<ForwardingContainer>();
			var stmALog = container.Logs.MostRecentLogByEventTime(Events.Delivered);
			AssertNull("No Delivered event log", stmALog);

			var eventTime = ZDateTime.Now;
			container.JC_ArrivalEstimatedDelivery = eventTime;
			stmALog = container.Logs.MostRecentLogByEventTime(Events.Delivered);
			AssertNotNull("Delivered event log created", stmALog);
			AssertEquals("Delivered event time", eventTime, stmALog.SL_EventTime);
			AssertEquals("IsEstimate", true, stmALog.SL_IsEstimate);
			AssertEquals("SL_Reference", "|TYP=FUL", stmALog.SL_Reference);
			var count = container.Logs.Find(log => log.SL_SE_NKEvent == Events.DeliveredCode).Count();
			AssertEquals("Delivered event log count should be 1", 1, count);

			var eventTime2 = eventTime.AddSeconds(1);
			container.JC_ArrivalEstimatedDelivery = eventTime2;
			stmALog = container.Logs.MostRecentLogByEventTime(Events.Delivered);
			AssertNotNull("Delivered event log updated", stmALog);
			AssertEquals("Delivered event time 2", eventTime2, stmALog.SL_EventTime);
			count = container.Logs.Find(log => log.SL_SE_NKEvent == Events.DeliveredCode).Count();
			AssertEquals("Delivered event log count should be 1", 1, count);
		}

		public void TestJC_ArrivalCartageCompleteEventLog()
		{
			var container = Factory.New<ForwardingContainer>();
			var stmALog = container.Logs.MostRecentLogByEventTime(Events.Delivered);
			AssertNull("No Delivered event log", stmALog);

			var eventTime = ZDateTime.Now;
			container.JC_ArrivalCartageComplete = eventTime;
			stmALog = container.Logs.MostRecentLogByEventTime(Events.Delivered);
			AssertNotNull("Delivered event log created", stmALog);
			AssertEquals("Delivered event time", eventTime, stmALog.SL_EventTime);
			AssertEquals("IsEstimate", false, stmALog.SL_IsEstimate);
			AssertEquals("SL_Reference", "|TYP=FUL", stmALog.SL_Reference);
			var count = container.Logs.Find(log => log.SL_SE_NKEvent == Events.DeliveredCode).Count();
			AssertEquals("Delivered event log count should be 1", 1, count);

			var eventTime2 = eventTime.AddSeconds(1);
			container.JC_ArrivalCartageComplete = eventTime2;
			stmALog = container.Logs.MostRecentLogByEventTime(Events.Delivered);
			AssertNotNull("Delivered event log updated", stmALog);
			AssertEquals("Delivered event time 2", eventTime2, stmALog.SL_EventTime);
			count = container.Logs.Find(log => log.SL_SE_NKEvent == Events.DeliveredCode).Count();
			AssertEquals("Delivered event log count should be 1", 1, count);
		}

		#endregion

		#region ProcessLogCore Update Date

		public void TestProcessLogCoreUpdateDate_Pickup()
		{
			var container = Factory.New<ForwardingContainer>();

			var eventTime = DateTime.Now;
			container.JC_IsEmptyContainer = false;
			var pickupLog = container.Logs.AddNew();
			using (pickupLog.LockForUpdatingKeyFieldsForTesting())
			{
				pickupLog.SL_SE_NKEvent = Events.PickedUpCode;
				pickupLog.SL_Reference = "|FAC=CNR|TYP=FUL";
				pickupLog.SL_EventTime = eventTime;
				pickupLog.SL_IsEstimate = true;
			}

			(container as IStmALogParent).ProcessLog(pickupLog);
			AssertEquals("Pickup event", "PUP", pickupLog.SL_SE_NKEvent);
			AssertEquals("Pickup reference", "|FAC=CNR|TYP=FUL", pickupLog.SL_Reference);
			AssertEquals("Departure IsEstimate", true, pickupLog.SL_IsEstimate);
			AssertEquals("Departure Estimated time", eventTime, container.JC_DepartureEstimatedPickup);

			var eventTime2 = eventTime.AddSeconds(1);
			container.JC_DepartureEstimatedPickup = eventTime2;
			AssertEquals("Departure Estimated time 2", eventTime2, container.JC_DepartureEstimatedPickup);
			var stmALog = container.Logs.MostRecentLogByEventTime(Events.PickedUp);
			AssertEquals("pickupLog should be the same reference with stmALog", stmALog, pickupLog);
			AssertNotNull("PickedUp event log 2", stmALog);
			AssertEquals("PickedUp event time 2", eventTime2, stmALog.SL_EventTime);
			AssertEquals("IsEstimate 2", true, stmALog.SL_IsEstimate);
			AssertEquals("SL_Reference 2", "|FAC=CNR|TYP=FUL", stmALog.SL_Reference);
		}

		public void TestProcessLogCoreUpdateDate_GateIn()
		{
			var container = Factory.New<ForwardingContainer>();

			var eventTime = new DateTime(2016, 2, 6);
			container.JC_IsEmptyContainer = false;
			var gateInLog = container.Logs.AddNew();
			using (gateInLog.LockForUpdatingKeyFieldsForTesting())
			{
				gateInLog.SL_SE_NKEvent = Events.GateInCode;
				gateInLog.SL_Reference = "|FAC=CTO|TYP=FUL";
				gateInLog.SL_EventTime = eventTime;
				gateInLog.SL_IsEstimate = false;
			}

			(container as IStmALogParent).ProcessLog(gateInLog);
			AssertEquals("GIN event", "GIN", gateInLog.SL_SE_NKEvent);
			AssertEquals("GIN reference", "|FAC=CTO|TYP=FUL", gateInLog.SL_Reference);
			AssertEquals("GIN IsEstimate", false, gateInLog.SL_IsEstimate);
			AssertEquals("GIN event time", eventTime, gateInLog.SL_EventTime);

			AssertEquals("JC_DepartureEstimatedPickup should be empty", ZDateTime.Empty, container.JC_DepartureEstimatedPickup);
			AssertEquals("JC_DepartureCartageComplete should be empty", ZDateTime.Empty, container.JC_DepartureCartageComplete);
			AssertEquals("JC_ArrivalEstimatedDelivery should be empty", ZDateTime.Empty, container.JC_ArrivalEstimatedDelivery);
			AssertEquals("JC_ArrivalCartageComplete should be empty", ZDateTime.Empty, container.JC_ArrivalCartageComplete);
		}

		#endregion

		#region ProcessLogCore Update VGM Status

		public void TestProcessLogCoreUpdateVGMStatus_Sent()
		{
			var container = Factory.New<ForwardingContainer>();

			CreateVGMLog(container, Events.MessageSent);
			Factory.Save();

			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent, container.JC_GrossWeightVerificationStatus);
		}

		public void TestProcessLogCoreUpdateVGMStatus_WithdrawSent()
		{
			var container = Factory.New<ForwardingContainer>();

			CreateVGMLog(container, Events.MessageWithdrawCancelRequest);
			Factory.Save();

			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawSent, container.JC_GrossWeightVerificationStatus);
		}

		public void TestProcessLogCoreUpdateVGMStatus_WithdrawRejected_MRJ()
		{
			var container = Factory.New<ForwardingContainer>();

			CreateVGMLog(container, Events.MessageWithdrawCancelRequest);
			Factory.Save();

			System.Threading.Thread.Sleep(1);

			CreateVGMLog(container, Events.MessageRejected);
			Factory.Save();

			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawRejected, container.JC_GrossWeightVerificationStatus);
		}

		public void TestProcessLogCoreUpdateVGMStatus_WithdrawAcknowledged_IRA()
		{
			var container = Factory.New<ForwardingContainer>();

			CreateVGMLog(container, Events.MessageWithdrawCancelRequest);
			Factory.Save();

			System.Threading.Thread.Sleep(1);

			CreateVGMLog(container, Events.InterchangeReceiptAcknowledged);
			Factory.Save();

			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawAcknowledged, container.JC_GrossWeightVerificationStatus);

			var container2 = Factory.New<ForwardingContainer>();

			CreateVGMLog(container2, Events.MessageWithdrawCancelAccepted);
			Factory.Save();

			System.Threading.Thread.Sleep(1);

			CreateVGMLog(container2, Events.InterchangeReceiptAcknowledged);
			Factory.Save();

			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawAcknowledged, container2.JC_GrossWeightVerificationStatus);
		}

		public void TestProcessLogCoreUpdateVGMStatus_Acknowledged()
		{
			var container = Factory.New<ForwardingContainer>();

			CreateVGMLog(container, Events.InterchangeReceiptAcknowledged);
			Factory.Save();

			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.Acknowledged, container.JC_GrossWeightVerificationStatus);
		}

		public void TestProcessLogCoreUpdateVGMStatus_Rejected_IRJ()
		{
			var container = Factory.New<ForwardingContainer>();

			CreateVGMLog(container, Events.InterchangeRejected);
			Factory.Save();

			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.Rejected, container.JC_GrossWeightVerificationStatus);
		}

		public void TestProcessLogCoreUpdateVGMStatus_Rejected_MRJ()
		{
			var container = Factory.New<ForwardingContainer>();
			container.JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.Acknowledged;

			CreateVGMLog(container, Events.MessageRejected);
			Factory.Save();

			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.Rejected, container.JC_GrossWeightVerificationStatus);
		}

		public void TestProcessLogCoreUpdateVGMStatus_WithdrawRejected_IRJ()
		{
			var container = Factory.New<ForwardingContainer>();

			CreateVGMLog(container, Events.MessageWithdrawCancelRequest);
			Factory.Save();

			System.Threading.Thread.Sleep(1);

			CreateVGMLog(container, Events.InterchangeRejected);
			Factory.Save();

			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawRejected, container.JC_GrossWeightVerificationStatus);
		}

		public void TestProcessLogCoreUpdateVGMStatus_Accepted()
		{
			var container = Factory.New<ForwardingContainer>();

			CreateVGMLog(container, Events.MessageAccepted);
			Factory.Save();

			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.Accepted, container.JC_GrossWeightVerificationStatus);
		}

		public void TestProcessLogCoreUpdateVGMStatus_WithdrawAcknowledged_MAA()
		{
			var container = Factory.New<ForwardingContainer>();

			CreateVGMLog(container, Events.MessageWithdrawCancelRequest);
			Factory.Save();

			System.Threading.Thread.Sleep(1);

			CreateVGMLog(container, Events.MessageAccepted);
			Factory.Save();

			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawAcknowledged, container.JC_GrossWeightVerificationStatus);

			var container2 = Factory.New<ForwardingContainer>();

			CreateVGMLog(container2, Events.MessageWithdrawCancelAccepted);
			Factory.Save();

			System.Threading.Thread.Sleep(1);

			CreateVGMLog(container2, Events.MessageAccepted);
			Factory.Save();

			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawAcknowledged, container2.JC_GrossWeightVerificationStatus);
		}

		public void TestProcessLogCoreUpdateVGMStatus_StatusUpdated_ResetToOriginal()
		{
			var container = Factory.New<ForwardingContainer>();

			CreateVGMLog(container, Events.MessageSent);
			Factory.Save();

			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent, container.JC_GrossWeightVerificationStatus);

			System.Threading.Thread.Sleep(1);

			CreateVGMLog(container, Events.StatusUpdated, $"|TYP={Constants.EventReferenceMessageTypes.ResetToOriginal}");
			Factory.Save();

			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent, container.JC_GrossWeightVerificationStatus);
		}

		StmALog CreateVGMLog(ForwardingContainer container, Event @event, string additionalParameters = null)
		{
			AssertNotNull(container);

			var eventTime = DateTime.Now;
			var log = container.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = @event.Code;
				log.SL_Reference = $"|MST=Verified Gross Container Weight{additionalParameters}";
				log.SL_EventTime = eventTime;
				log.SL_IsEstimate = true;
			}

			return log;
		}

		#endregion

		#region Deleted from row factory

		public void TestDeletedFromRowFactory()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();

			var row = ((INeedRow)container).Row;
			row.Delete();
			AssertEquals("pre: state is detached", DataRowState.Detached, row.RowState);
			AssertEquals("pre: bizo IsDeleted", true, container.IsDeleted);

			AssertEquals(0, ErrorReporter.TotalErrorCount);

			var containerNum = container.JC_ContainerNum;

			CombineAssertions(() =>
			{
				AssertEquals("error reporter count", 1, ErrorReporter.TotalErrorCount);
				AssertContains("bizo deletion stack trace is not recorded", "Delete stack never collected", ErrorReporter.LastMessageReported);
				AssertContains("row deleted stack trace is recorded", "DataRowDeleting_EventHandler", ErrorReporter.LastMessageReported);
			});

			ErrorReporter.Clear();
		}

		#endregion

		#region TestRefContainerLoadListOrSupplierBooking

		public void TestRefContainerLoadListOrJobSupplierBooking()
		{
			TestRefContainerLoadListOrJobSupplierBooking_WithoutAttached(true);
			TestRefContainerLoadListOrJobSupplierBooking_AttachedToJobSupplierBooking(true);
			TestRefContainerLoadListOrJobSupplierBooking_AttachedToContainerLoadPlan(true);
			TestRefContainerLoadListOrJobSupplierBooking_AllocatedToContainerLoadList(true);

			TestRefContainerLoadListOrJobSupplierBooking_WithoutAttached(false);
			TestRefContainerLoadListOrJobSupplierBooking_AttachedToJobSupplierBooking(false);
			TestRefContainerLoadListOrJobSupplierBooking_AttachedToContainerLoadPlan(false);
			TestRefContainerLoadListOrJobSupplierBooking_AllocatedToContainerLoadList(false);
		}

		void TestRefContainerLoadListOrJobSupplierBooking_AttachedToJobSupplierBooking(bool enableAdvOrmFeature)
		{
			AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
			{
				var container = BuildContainerAttachedToSupplierBooking(SupplierBookingStatus.Approved);

				AssertEquals(ZGuid.Empty, container.RelatedContainerLoadListPK);
				AssertEquals(true, container.RelatedContainerLoadList_ReadOnly);
				AssertEquals(false, container.RelatedContainerLoadListVisible);
				if (enableAdvOrmFeature)
				{
					AssertEquals(true, container.RelatedSupplierBookingVisible);
				}
				else
				{
					AssertEquals(false, container.RelatedSupplierBookingVisible);
				}
			});

			AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
			{
				var container = BuildContainerAttachedToSupplierBooking(SupplierBookingStatus.Cancelled);

				AssertEquals(ZGuid.Empty, container.RelatedContainerLoadListPK);
				AssertEquals(true, container.RelatedContainerLoadList_ReadOnly);
				AssertEquals(false, container.RelatedContainerLoadListVisible);
				if (enableAdvOrmFeature)
				{
					AssertEquals(true, container.RelatedSupplierBookingVisible);
				}
				else
				{
					AssertEquals(false, container.RelatedSupplierBookingVisible);
				}
			});
		}

		void TestRefContainerLoadListOrJobSupplierBooking_AllocatedToContainerLoadList(bool enableAdvOrmFeature)
		{
			AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
			{
				var jobSupplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
				jobSupplierBooking.JSB_BookingId = "JSB001";
				jobSupplierBooking.JSB_Status = SupplierBookingStatus.Approved;
				var bookingLine = jobSupplierBooking.SupplierBookingLines.AddNew();

				var container = Factory.NewWithValidTestData<ForwardingContainer>();
				container.JC_JSB_SupplierBooking = jobSupplierBooking.PK;

				var loadListHeader = Factory.NewWithValidTestData<CYContainerLoadList>();
				loadListHeader.CLH_JSB_Booking = jobSupplierBooking.PK;
				loadListHeader.CLH_LoadListId = "CLL002";
				loadListHeader.CLH_Status = ContainerLoadListHeaderStatus.Incomplete;

				var loadListLine = loadListHeader.LoadListLines.AddNew();
				loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
				loadListLine.CLL_JC_Container = container.PK;

				AssertEquals(enableAdvOrmFeature ? loadListHeader.PK : ZGuid.Empty, container.RelatedContainerLoadListPK);
				AssertEquals(true, container.RelatedContainerLoadList_ReadOnly);
				if (enableAdvOrmFeature)
				{
					AssertEquals(true, container.RelatedContainerLoadListVisible);
				}
				else
				{
					AssertEquals(false, container.RelatedContainerLoadListVisible);
				}
				AssertEquals(false, container.RelatedSupplierBookingVisible);
			});
		}

		void TestRefContainerLoadListOrJobSupplierBooking_AttachedToContainerLoadPlan(bool enableAdvOrmFeature)
		{
			AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
			{
				var container = BuildContainerAttachedToContainerLoadPlan(ContainerLoadListHeaderStatus.Incomplete);

				AssertEquals(ZGuid.Empty, container.RelatedContainerLoadListPK);
				AssertEquals(true, container.RelatedContainerLoadList_ReadOnly);
				AssertEquals(false, container.RelatedContainerLoadListVisible);
				if (enableAdvOrmFeature)
				{
					AssertEquals(true, container.RelatedSupplierBookingVisible);
				}
				else
				{
					AssertEquals(false, container.RelatedSupplierBookingVisible);
				}
			});
		}

		void TestRefContainerLoadListOrJobSupplierBooking_WithoutAttached(bool enableAdvOrmFeature)
		{
			AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
			{
				var container = Factory.NewWithValidTestData<ForwardingContainer>();

				AssertEquals(ZGuid.Empty, container.RelatedContainerLoadListPK);
				AssertEquals(true, container.RelatedContainerLoadList_ReadOnly);
				AssertEquals(false, container.RelatedContainerLoadListVisible);
				if (enableAdvOrmFeature)
				{
					AssertEquals(true, container.RelatedSupplierBookingVisible);
				}
				else
				{
					AssertEquals(false, container.RelatedSupplierBookingVisible);
				}
			});
		}

		#endregion

		public void TestSystemOnlyAdditionalReferenceNumber()
		{
			var container = Factory.New<ForwardingContainer>();
			var entryTypesForSystemOnly = new ZString[]
			{
				ContainerNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierMessageReference
			};

			foreach (var entryType in entryTypesForSystemOnly)
			{
				var entryNum = container.AdditionalReferenceNumbers.AddNew();
				entryNum.CE_EntryType = entryType;
				entryNum.CE_EntryIsSystemGenerated = true;
			}

			Factory.Save();

			foreach (var entryType in entryTypesForSystemOnly)
			{
				var reloadedContainer = new BusinessObjectFactory().Load<ForwardingContainer>(container.PK);
				var reloadedEntryNum = reloadedContainer.AdditionalReferenceNumbers.Cast<CusEntryNumber>().First(x => x.CE_EntryType == entryType);
				AssertEquals(entryType, reloadedEntryNum.CE_EntryType);

				Assert($"{entryType} additional ref number should be read-only after loading", reloadedEntryNum.ReadOnly);
				Assert($"{entryType} cannot be deleted", !reloadedEntryNum.CanDelete);
				AssertEquals($"The {entryType} is system generated and cannot be deleted.", reloadedEntryNum.ReasonForNotAbleToDelete);
			}
		}

		#region TestContainerLoadPlanAndSupplierBooking

		public void TestContainerLoadPlanAndSupplierBooking()
		{
			TestContainerLoadPlanAndSupplierBooking_AttachedToJobSupplierBooking(true);
			TestContainerLoadPlanAndSupplierBooking_AttachedToJobSupplierBooking(false);
			TestContainerLoadPlanAndSupplierBooking_AttachedToContainerLoadPlan(true);
			TestContainerLoadPlanAndSupplierBooking_AttachedToContainerLoadPlan(false);
			TestContainerLoadPlanAndSupplierBooking_WithoutAttached(true);
			TestContainerLoadPlanAndSupplierBooking_WithoutAttached(false);
			TestContainerLoadPlanAndSupplierBooking_ChangeAttachment(true);
			TestContainerLoadPlanAndSupplierBooking_ChangeAttachment(false);
		}

		void TestContainerLoadPlanAndSupplierBooking_AttachedToJobSupplierBooking(bool enableAdvOrmFeature)
		{
			AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
			{
				var container = BuildContainerAttachedToSupplierBooking(SupplierBookingStatus.Approved);
				AssertEquals(ZGuid.Empty, container.JC_CLH_LoadListPlan);
				AssertNotNull(container.SupplierBooking);
				AssertEquals(false, container.JC_JSB_SupplierBooking_ReadOnly);
				AssertEquals(true, container.JC_CLH_LoadListPlan_ReadOnly);
				AssertEquals(enableAdvOrmFeature, container.RelatedContainerLoadPlanVisible);
			});
		}

		void TestContainerLoadPlanAndSupplierBooking_AttachedToContainerLoadPlan(bool enableAdvOrmFeature)
		{
			AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
			{
				var container = BuildContainerAttachedToContainerLoadPlan(ContainerLoadListHeaderStatus.Approved);
				AssertNull(container.SupplierBooking);
				AssertEquals(true, container.JC_JSB_SupplierBooking_ReadOnly);
				AssertEquals(false, container.JC_CLH_LoadListPlan_ReadOnly);
				AssertEquals(enableAdvOrmFeature, container.RelatedContainerLoadPlanVisible);
			});
		}

		void TestContainerLoadPlanAndSupplierBooking_WithoutAttached(bool enableAdvOrmFeature)
		{
			AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
			{
				var container = Factory.NewWithValidTestData<ForwardingContainer>();
				AssertEquals(ZGuid.Empty, container.JC_CLH_LoadListPlan);
				AssertNull(container.SupplierBooking);
				AssertEquals(false, container.JC_JSB_SupplierBooking_ReadOnly);
				AssertEquals(false, container.JC_CLH_LoadListPlan_ReadOnly);
				AssertEquals(enableAdvOrmFeature, container.RelatedContainerLoadPlanVisible);
			});
		}

		void TestContainerLoadPlanAndSupplierBooking_ChangeAttachment(bool enableAdvOrmFeature)
		{
			AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
			{
				var container = BuildContainerAttachedToContainerLoadPlan(ContainerLoadListHeaderStatus.Approved);
				AssertNull(container.SupplierBooking);
				AssertEquals(true, container.JC_JSB_SupplierBooking_ReadOnly);
				AssertEquals(false, container.JC_CLH_LoadListPlan_ReadOnly);
				AssertEquals(enableAdvOrmFeature, container.RelatedContainerLoadPlanVisible);
				container = BuildContainerAttachedToSupplierBooking(SupplierBookingStatus.Approved);
				AssertEquals(ZGuid.Empty, container.JC_CLH_LoadListPlan);
				AssertNotNull(container.SupplierBooking);
				AssertEquals(false, container.JC_JSB_SupplierBooking_ReadOnly);
				AssertEquals(true, container.JC_CLH_LoadListPlan_ReadOnly);
				AssertEquals(enableAdvOrmFeature, container.RelatedContainerLoadPlanVisible);
			});
		}

		#endregion

		#region CO2e

		public void TestJobCO2eCollection()
		{
			// Arrange
			var container = Factory.New<ForwardingContainer>();
			AssertEquals(0, container.JobCO2eCollection.Count);
			ErrorReporter.Clear();

			// Act & Assert
			container.SetCO2ePerTonneInKg(2m, "DUC");
			AssertContains("ValidateCO2eType - type not allowed", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			// Act & Assert
			container.SetCO2ePerTonneInKg(2m);
			AssertContains("ValidateCO2eType - type not allowed", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			// Act & Assert
			container.SetCO2ePerTonneInKg(2m, CO2eTypes.EmptyPickup);
			container.SetCO2ePerTonneInKg(3m, CO2eTypes.EmptyReturn);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			AssertEquals(2m, container.GetCO2ePerTonneInKg(CO2eTypes.EmptyPickup));
			AssertEquals(3m, container.GetCO2ePerTonneInKg(CO2eTypes.EmptyReturn));
		}

		public void TestCO2eBindingVariables()
		{
			// Arrange
			var container = Factory.New<ForwardingContainer>();
			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull("refContainer", refContainer);

			refContainer.RC_TEU = 0;
			container.JC_RC = refContainer.PK;
			Factory.Save();

			AssertNullOrEmpty("TotalCO2eForEmptyPickupForBinding should start as empty", container.TotalCO2eForEmptyPickupForBinding);
			AssertNullOrEmpty("TotalCO2eForEmptyReturnForBinding should start as empty", container.TotalCO2eForEmptyReturnForBinding);

			// Act & Assert
			refContainer.RC_TareWeight = 100000m;
			container.SetCO2ePerTonneInKg(20m, CO2eTypes.EmptyPickup);
			container.SetCO2ePerTonneInKg(42.01m, CO2eTypes.EmptyReturn);
			CombineAssertions("CO2e binding strings - basic format", () =>
			{
				AssertEquals("TotalCO2eForEmptyPickupForBinding", "2,000", container.TotalCO2eForEmptyPickupForBinding);
				AssertEquals("TotalCO2eForEmptyReturnForBinding", "4,201", container.TotalCO2eForEmptyReturnForBinding);
			});

			// Act & Assert
			refContainer.RC_TareWeight = 2800m;
			container.SetCO2ePerTonneInKg(4.6m, CO2eTypes.EmptyPickup);
			container.SetCO2ePerTonneInKg(4.3m, CO2eTypes.EmptyReturn);
			CombineAssertions("CO2e binding strings - remove leading zeroes from binding string", () =>
			{
				AssertEquals("TotalCO2eForEmptyPickupForBinding", "12.88", container.TotalCO2eForEmptyPickupForBinding);
				AssertEquals("TotalCO2eForEmptyReturnForBinding", "12.04", container.TotalCO2eForEmptyReturnForBinding);
			});

			// Act & Assert
			refContainer.RC_TareWeight = 637.718m;
			container.SetCO2ePerTonneInKg(5.61, CO2eTypes.EmptyPickup);
			container.SetCO2ePerTonneInKg(4.39, CO2eTypes.EmptyReturn);
			CombineAssertions("CO2e binding strings - rounding after 3dp", () =>
			{
				AssertEquals("TotalCO2eForEmptyPickupForBinding", "3.578", container.TotalCO2eForEmptyPickupForBinding);
				AssertEquals("TotalCO2eForEmptyReturnForBinding", "2.8", container.TotalCO2eForEmptyReturnForBinding);
			});

			// Act & Assert
			refContainer.RC_TEU = 2.29m;
			refContainer.RC_TareWeight = 0;
			container.SetCO2ePerTEUInKg(14.299, CO2eTypes.EmptyPickup);
			container.SetCO2ePerTEUInKg(9.927, CO2eTypes.EmptyReturn);
			CombineAssertions("CO2e binding strings - expected results using TEU for calculations", () =>
			{
				AssertEquals("TotalCO2eForEmptyPickupForBinding", "32.745", container.TotalCO2eForEmptyPickupForBinding);
				AssertEquals("TotalCO2eForEmptyReturnForBinding", "22.733", container.TotalCO2eForEmptyReturnForBinding);
			});
		}

		public void TestCo2eStatusIsNCU_WhenContainerImportProcessTransportModeIsChanged()
		{
			//Arrange
			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.JC_OA_ArrivalContainerYardAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			container.EmptyReturnToTransportMode = "RAI";
			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyReturn);

			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyReturn));
			AssertNoWarning(container.TotalCO2eForEmptyReturnForBindingInfo, CO2eTestHelper.CO2eStaleWarning);

			//Action
			Factory.Save();
			container.EmptyReturnToTransportMode = "IWT";

			//Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, container.GetCO2eStatus(CO2eTypes.EmptyReturn));
			AssertHasWarning(container.TotalCO2eForEmptyReturnForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(container, "EmptyReturnToTransportMode [RAI]->[IWT]");
		}

		public void TestCo2eStatusIsNCU_WhenContainerImportProcessEmptyReturnToAddressIsChanged()
		{
			//Arrange
			var address1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address1.OA_RL_NKRelatedPortCode = "AUADL";

			var address2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address2.OA_RL_NKRelatedPortCode = "AUMEL";

			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.GetOrCreateJobCO2e(CO2eTypes.EmptyReturn);
			container.JC_OA_ArrivalContainerYardAddress = address1.PK;
			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyReturn);

			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyReturn));
			AssertNoWarning(container.TotalCO2eForEmptyReturnForBindingInfo, CO2eTestHelper.CO2eStaleWarning);

			//Action
			Factory.Save();
			container.JC_OA_ArrivalContainerYardAddress = address2.PK;

			//Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, container.GetCO2eStatus(CO2eTypes.EmptyReturn));
			AssertHasWarning(container.TotalCO2eForEmptyReturnForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(container, $"JC_OA_ArrivalContainerYardAddress [{address1.PK}]->[{address2.PK}]");
		}

		RefContainer NewRefContainer(ZString code, ZString isoType)
		{
			var refContainer = RefContainer.New(Factory);
			refContainer.RC_Code = code;
			refContainer.RC_ISOType = isoType;

			return refContainer;
		}

		RefContainer NewRefContainer(ZString code, ZString isoType, decimal teu, decimal tareWeight)
		{
			var refContainer = RefContainer.New(Factory);
			refContainer.RC_Code = code;
			refContainer.RC_ISOType = isoType;
			refContainer.RC_TEU = teu;
			refContainer.RC_TareWeight = tareWeight;

			return refContainer;
		}

		public void TestCo2eStatusIsNCU_WhenContainerTypeIsChanged()
		{
			// Arrange
			var refContainer1 = NewRefContainer("35H1", "35H1");
			var refContainer2 = NewRefContainer("35H0", "35H0");

			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.GetOrCreateJobCO2e(CO2eTypes.EmptyPickup);
			container.GetOrCreateJobCO2e(CO2eTypes.EmptyReturn);
			container.JC_RC = refContainer1.PK;

			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyPickup);
			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyReturn);

			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyPickup));
			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyReturn));
			AssertNoWarning(container.TotalCO2eForEmptyPickupForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			AssertNoWarning(container.TotalCO2eForEmptyReturnForBindingInfo, CO2eTestHelper.CO2eStaleWarning);

			Factory.Save();

			//Action
			container.JC_RC = refContainer2.PK;

			//Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, container.GetCO2eStatus(CO2eTypes.EmptyPickup));
			AssertEquals(CO2eStatusList.Codes.NotCurrent, container.GetCO2eStatus(CO2eTypes.EmptyReturn));
			AssertHasWarning(container.TotalCO2eForEmptyPickupForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			AssertHasWarning(container.TotalCO2eForEmptyReturnForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(container, $"JC_RC [{refContainer1.PK}]->[{refContainer2.PK}]");
		}

		public void TestCo2eStatusIsNCU_WhenContainerCountIsChanged()
		{
			//Arrange
			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyReturn);
			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyPickup);

			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyPickup));
			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyReturn));
			AssertNoWarning(container.TotalCO2eForEmptyPickupForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			AssertNoWarning(container.TotalCO2eForEmptyReturnForBindingInfo, CO2eTestHelper.CO2eStaleWarning);

			Factory.Save();

			//Action
			container.JC_ContainerCount = 2;

			//Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, container.GetCO2eStatus(CO2eTypes.EmptyPickup));
			AssertEquals(CO2eStatusList.Codes.NotCurrent, container.GetCO2eStatus(CO2eTypes.EmptyReturn));
			AssertHasWarning(container.TotalCO2eForEmptyPickupForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			AssertHasWarning(container.TotalCO2eForEmptyReturnForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(container, "JC_ContainerCount [1]->[2]");
		}

		public void TestCo2eStatusIsNCU_WhenContainerExportProcessTransportModeIsChanged()
		{
			//Arrange
			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyPickup);
			container.JC_OA_DepartureContainerYardAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			container.EmptyPickupByTransportMode = "RAI";

			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyPickup);
			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyPickup));
			AssertNoWarning(container.TotalCO2eForEmptyPickupForBindingInfo, CO2eTestHelper.CO2eStaleWarning);

			//Action
			Factory.Save();
			container.EmptyPickupByTransportMode = "IWT";

			//Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, container.GetCO2eStatus(CO2eTypes.EmptyPickup));
			AssertHasWarning(container.TotalCO2eForEmptyPickupForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(container, "EmptyPickupByTransportMode [RAI]->[IWT]");
		}

		public void TestCo2eStatusIsNCU_WhenContainerExportProcessEmptyPickUpFromAddressIsChanged()
		{
			//Arrange
			var address1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address1.OA_RL_NKRelatedPortCode = "AUADL";

			var address2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address2.OA_RL_NKRelatedPortCode = "AUMEL";

			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.GetOrCreateJobCO2e(CO2eTypes.EmptyPickup);
			container.JC_OA_DepartureContainerYardAddress = address1.PK;

			container.SetCO2eStatus(CO2eStatusList.Codes.Current, CO2eTypes.EmptyPickup);

			AssertEquals(CO2eStatusList.Codes.Current, container.GetCO2eStatus(CO2eTypes.EmptyPickup));
			AssertNoWarning(container.TotalCO2eForEmptyPickupForBindingInfo, CO2eTestHelper.CO2eStaleWarning);

			//Action
			Factory.Save();
			container.JC_OA_DepartureContainerYardAddress = address2.PK;

			//Assert
			AssertEquals(CO2eStatusList.Codes.NotCurrent, container.GetCO2eStatus(CO2eTypes.EmptyPickup));
			AssertHasWarning(container.TotalCO2eForEmptyPickupForBindingInfo, CO2eTestHelper.CO2eStaleWarning);
			CO2eTestHelper.AssertSTUEvent(container, $"JC_OA_DepartureContainerYardAddress [{address1.PK}]->[{address2.PK}]");
		}

		public void TestContainerEmptyWeightPerTEU_IsSumOfTareWeightAndDunnageWeightPerTEU()
		{
			// Arrange
			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			var refContainer1 = NewRefContainer("20GP", "22G0", 2.3m, 2280m);
			container.JC_RC = refContainer1.PK;
			container.JC_DunnageWeight = 100m;

			// Act
			var containerEmptyWeightPerTEU = ((ICO2eTEUProvider)container).ContainerEmptyWeightPerTEU;

			// Assert
			AssertEquals(1034.78, Math.Round((double)containerEmptyWeightPerTEU, 2));
		}

		#endregion

		#region Implementation

		ForwardingContainer GetNewContainerOnNewImportConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";

			return consol.Containers.AddNew();
		}

		protected override CommonConsol GetNewConsol()
		{
			return Factory.New<ForwardingConsol>();
		}

		protected override CommonContainer GetNewContainer()
		{
			return Factory.New<ForwardingContainer>();
		}

		protected override void SetUp()
		{
			base.SetUp();
		}

		ForwardingContainer BuildContainerAttachedToSupplierBooking(ZString status, ForwardingContainer container = null)
		{
			var jobSupplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			jobSupplierBooking.JSB_BookingId = "JSB001";
			jobSupplierBooking.JSB_Status = status;
			var newContainer = container ?? Factory.NewWithValidTestData<ForwardingContainer>();
			newContainer.JC_JSB_SupplierBooking = jobSupplierBooking.PK;

			return newContainer;
		}

		ForwardingContainer BuildContainerAttachedToContainerLoadPlan(ZString status, string loadMode = CommonContainerLoadListLoadModeList.Codes.CFS)
		{
			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			var containerLoadPlan = Factory.NewWithValidTestData<CFSContainerLoadList>();
			containerLoadPlan.CLH_LoadListId = "CLL001";
			containerLoadPlan.CLH_Status = status;
			container.JC_CLH_LoadListPlan = containerLoadPlan.PK;

			return container;
		}

		#endregion
	}
}
