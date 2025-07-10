using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ContainerPenaltyValidationTest : BaseFreightTest
	{
		public void TestCheckUniqueness()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "CNQIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			var exportPenalty1 = container1.ExportPenalties.AddNew();
			exportPenalty1.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
			var exportPenalty2 = container1.ExportPenalties.AddNew();
			exportPenalty2.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
			exportPenalty2.RunPreSaveValidation();
			AssertHasWarning(exportPenalty2.CPY_CreditorTypeInfo, "You can't enter more than one penalty for same creditor and location.");

			exportPenalty1 = container2.ExportPenalties.AddNew();
			exportPenalty1.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
			exportPenalty1.RunPreSaveValidation();
			AssertNoNotifications(exportPenalty1.CPY_CreditorTypeInfo);
		}

		public void TestCheckCreditorTypeCarrierRule()
		{
			var warningMessage = @"Under the same Container Penalty grid, with Creditor Type of 'CAR' and the Creditor has the same value, either:
An entry with Penalty Type of 'MDD' is allowed, or.
Entries of Penalty Type of 'DET' and/or 'STO' allowed.";

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "CNQIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var container = consol.Containers.AddNew();
			var creditor1 = Factory.New<OrgHeader>();
			var creditor2 = Factory.New<OrgHeader>();

			void AssertCarrierRuleIsNotViolated(params ContainerPenalty[] penalties)
			{
				foreach (var penalty in penalties)
				{
					penalty.RunPreSaveValidation();
					AssertNoWarning(penalty.CPY_PenaltyTypeInfo, warningMessage);
					AssertNoWarning(penalty.CPY_CreditorTypeInfo, warningMessage);
					AssertNoWarning(penalty.CPY_OH_CreditorInfo, warningMessage);
				}
			}

			void AssertCarrierRuleIsViolated(params ContainerPenalty[] penalties)
			{
				foreach (var penalty in penalties)
				{
					penalty.RunPreSaveValidation();
					AssertHasWarning(penalty.CPY_PenaltyTypeInfo, warningMessage);
					AssertHasWarning(penalty.CPY_CreditorTypeInfo, warningMessage);
					AssertHasWarning(penalty.CPY_OH_CreditorInfo, warningMessage);
				}
			}

			var exportPenalty1 = CreateContainerPenaltyForContainer(
				container,
				ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention,
				ContainerPenaltyCreditorType.Codes.Carrier,
				creditor1.PK);
			AssertCarrierRuleIsNotViolated(exportPenalty1);

			var exportPenalty2 = CreateContainerPenaltyForContainer(
				container,
				ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
				ContainerPenaltyCreditorType.Codes.Transport,
				creditor1.PK);
			AssertCarrierRuleIsNotViolated(exportPenalty1, exportPenalty2);

			exportPenalty2.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
			exportPenalty2.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			exportPenalty2.CPY_OH_Creditor = creditor1.PK;
			AssertCarrierRuleIsViolated(exportPenalty1, exportPenalty2);

			exportPenalty2.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
			exportPenalty2.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			exportPenalty2.CPY_OH_Creditor = creditor1.PK;
			AssertCarrierRuleIsViolated(exportPenalty1, exportPenalty2);

			var exportPenalty3 = CreateContainerPenaltyForContainer(
				container,
				ContainerPenaltyPenaltyType.Codes.Detention,
				ContainerPenaltyCreditorType.Codes.Carrier,
				creditor1.PK);
			AssertCarrierRuleIsViolated(exportPenalty1, exportPenalty2, exportPenalty3);

			exportPenalty2.CPY_OH_Creditor = creditor2.PK;
			exportPenalty3.CPY_OH_Creditor = creditor2.PK;
			exportPenalty1.RunPreSaveValidation();
			AssertCarrierRuleIsNotViolated(exportPenalty1, exportPenalty2, exportPenalty3);

			exportPenalty2.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
			exportPenalty2.CPY_OH_Creditor = creditor1.PK;
			exportPenalty1.RunPreSaveValidation();
			AssertCarrierRuleIsNotViolated(exportPenalty1, exportPenalty2, exportPenalty3);
		}

		public void TestCPY_PenaltyType()
		{
			importPenalty.CPY_PenaltyType = ZString.Empty;
			AssertHasErrors(importPenalty.CPY_PenaltyTypeInfo);

			importPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
			AssertNoErrors(importPenalty.CPY_PenaltyTypeInfo);

			importPenalty.CPY_PenaltyType = "XYZ";
			AssertHasErrors(importPenalty.CPY_PenaltyTypeInfo);
		}

		public void TestPenaltyValidationOnlyWhenSupportsContainerPenalties()
		{
			var consol1 = Factory.New<CommonConsol>();
			var consolContainer = consol1.Containers.AddNew();
			var consolPenalty = consolContainer.ImportPenalties.AddNew();

			var declarationContainer = Factory.New<CommonContainer>();
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var cusContainer = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC] = declarationContainer.PK;
			var declarationPenalty = declarationContainer.ImportPenalties.AddNew();

			consolPenalty.CPY_RL_NKLocation = "AUSYD";
			AssertNoErrors(consolPenalty.CPY_RL_NKLocationInfo);

			consolPenalty.CPY_RL_NKLocation = "AU";
			AssertNoErrors(consolPenalty.CPY_RL_NKLocationInfo);

			consolPenalty.CPY_RL_NKLocation = "SIX";
			AssertHasErrors("Consol Container location should have 2 or 5 characters", consolPenalty.CPY_RL_NKLocationInfo);

			consolPenalty.CPY_RL_NKLocation = ZString.Empty;
			AssertHasErrors("Consol Container location should not be empty", consolPenalty.CPY_RL_NKLocationInfo);

			declarationPenalty.CPY_RL_NKLocation = ZString.Empty;
			AssertHasErrors("Declaration Container location should not be empty", declarationPenalty.CPY_RL_NKLocationInfo);

			declarationPenalty.CPY_RL_NKLocation = "FOUR";
			AssertHasErrors("Declaration Container location should have 2 or 5 characters", declarationPenalty.CPY_RL_NKLocationInfo);
		}

		public void TestCPY_CreditorType()
		{
			importPenalty.CPY_CreditorType = ZString.Empty;
			AssertHasErrors(importPenalty.CPY_CreditorTypeInfo);

			importPenalty.CPY_CreditorType = "XYZ";
			AssertHasErrors(importPenalty.CPY_CreditorTypeInfo);

			importPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
			importPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
			AssertHasErrors("Creditor Type is expected to be Carrier.", importPenalty.CPY_CreditorTypeInfo);
			importPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			AssertNoErrors(importPenalty.CPY_CreditorTypeInfo);

			importPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.TruckWaitTime;
			importPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
			AssertHasErrors("Creditor Type is expected to be Transport.", importPenalty.CPY_CreditorTypeInfo);
			importPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Transport;
			AssertNoErrors(importPenalty.CPY_CreditorTypeInfo);

			importPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
			importPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Transport;
			AssertHasErrors("Creditor Type is expected to be CTO or Carrier.", importPenalty.CPY_CreditorTypeInfo);
			importPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
			AssertNoErrors(importPenalty.CPY_CreditorTypeInfo);
			importPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			AssertNoErrors(importPenalty.CPY_CreditorTypeInfo);

			importPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;
			importPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Transport;
			AssertHasErrors("Creditor Type is expected to be Carrier.", importPenalty.CPY_CreditorTypeInfo);
			importPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
			AssertHasErrors("Creditor Type is expected to be Carrier.", importPenalty.CPY_CreditorTypeInfo);
			importPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			AssertNoErrors(importPenalty.CPY_CreditorTypeInfo);
		}

		public void TestCPY_TimeUnit()
		{
			importPenalty.CPY_TimeUnit = ZString.Empty;
			AssertHasErrors(importPenalty.CPY_TimeUnitInfo);

			importPenalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Hours;
			AssertNoErrors(importPenalty.CPY_TimeUnitInfo);

			importPenalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;
			AssertNoErrors(importPenalty.CPY_TimeUnitInfo);
		}

		public void TestCPY_RX_NKCurrency()
		{
			importPenalty.CPY_RX_NKCurrency = ZString.Empty;
			AssertHasErrors(importPenalty.CPY_RX_NKCurrencyInfo);

			importPenalty.CPY_RX_NKCurrency = "AU";
			AssertHasErrors("Currency length should be 3.", importPenalty.CPY_RX_NKCurrencyInfo);

			importPenalty.CPY_RX_NKCurrency = "AUD";
			AssertNoErrors(importPenalty.CPY_RX_NKCurrencyInfo);
		}

		[TestDate(2020, 4, 6)]
		public void TestCheckArrivalCTOStorageDays()
		{
			var unlimitedFreeDaysOptions = new ContainerPenaltyFreeDaysOptions { UnlimitedFreeDays = true };
			var strategy = new Mock<IContainerDefaultingStrategy>();
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var defaultStorageWarning = "Number of days in storage from Wharf Gate Out minus CTO Storage Start plus 1 is calculated to ";
				var today = ZDateTime.Today;

				var container1 = Factory.New<IForwardingContainer>() as CommonContainer;
				strategy.Setup(x => x.CalculateStorageStart(It.IsAny<ZString>())).Returns(() => (container1.JC_FCLAvailable, container1.JC_FCLAvailable, "CTO Available"));
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(It.IsAny<ZString>(), It.IsAny<ZString>())).Returns(() => container1.JC_FCLAvailable);
				strategy.Setup(x => x.CalculateAvailableDateForStorage(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container1.JC_FCLAvailable, "CTO Available"));
				container1.JC_JK = Factory.New<CommonConsol>().PK;
				container1.JC_FCLAvailable = today;
				container1.ArrivalCTOStorageDays = 5;
				var penalty1 = container1.ImportPenalties.FirstOrDefault();
				AssertNotNull(penalty1);
				AssertNoWarnings(penalty1.CPY_DurationInfo); // No Wharf Gate Out set

				penalty1.FreeTimeAsDays = 1;
				container1.JC_FCLWharfGateOut = today.AddDays(3);
				container1.ArrivalCTOStorageDays = 5;
				AssertHasWarning("CTO Storage Start should not be calculated based on FreeTimeAsDays.", penalty1.CPY_DurationInfo, defaultStorageWarning + 4);

				penalty1.FreeTimeAsDays = 0;
				container1.JC_FCLWharfGateOut = today.AddDays(5);
				container1.ArrivalCTOStorageDays = 6;
				AssertNoWarnings(penalty1.CPY_DurationInfo);

				container1.ArrivalCTOStorageDays = 0;
				AssertHasWarning(penalty1.CPY_DurationInfo, defaultStorageWarning + 6);

				container1.JC_FCLWharfGateOut = today.AddDays(-5);
				container1.ArrivalCTOStorageDays = 5;
				AssertHasWarning("If gate out occurs before storage commences then no storage has occurred", penalty1.CPY_DurationInfo, defaultStorageWarning + 0);

				container1.ArrivalCTOStorageDays = 0;
				AssertNoWarning("Storage days now match calculated", penalty1.CPY_DurationInfo, defaultStorageWarning + 0);

				container1.JC_FCLWharfGateOut = today.AddDays(300);
				container1.ArrivalCTOStorageDays = 5;
				AssertHasWarning(penalty1.CPY_DurationInfo, defaultStorageWarning + 255);

				container1.JC_FCLAvailable = ZDateTime.Empty;
				penalty1.RunPreSaveValidation();
				AssertNoWarnings(penalty1.CPY_DurationInfo);

				penalty1 = container1.ImportPenalties.FindOrCreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage,
					  ContainerPenaltyCreditorType.Codes.CTO,
					  timeUnit: ContainerPenaltyTimeUnit.Codes.Days);
				penalty1.DurationAsDays = 99;
				AssertNotNull(penalty1);
				AssertHasWarning("Validation is based on CTO Storage Start which does not reset when penalty dates are empty", penalty1.CPY_DurationInfo, defaultStorageWarning + 255);
			}
		}

		[TestDate(2011, 11, 11)]
		public void TestCheckArrivalCarrierDetentionDays()
		{
			var defaultDetentionWarning = "Number of days in detention from Container Yard Empty Return Gate In minus Empty Return Req. By is calculated to ";
			var today = ZDateTime.Today;

			var container1 = Factory.New<CommonContainer>();
			container1.JC_JK = Factory.New<CommonConsol>().PK;
			container1.JC_EmptyReturnedBy = today;
			container1.ArrivalCarrierDetentionDays = 5;
			var penalty1 = container1.ImportPenalties.FirstOrDefault();
			AssertNotNull(penalty1);
			AssertNoWarnings(penalty1.CPY_DurationInfo);

			penalty1.FreeTimeAsDays = 1;
			container1.JC_ContainerYardEmptyReturnGateIn = today.AddDays(3);
			container1.ArrivalCarrierDetentionDays = 5;
			AssertHasWarning(penalty1.CPY_DurationInfo, defaultDetentionWarning + 3);

			penalty1.FreeTimeAsDays = 0;
			container1.JC_ContainerYardEmptyReturnGateIn = today.AddDays(5);
			container1.ArrivalCarrierDetentionDays = 5;
			AssertNoWarnings(penalty1.CPY_DurationInfo);

			container1.ArrivalCarrierDetentionDays = 0;
			AssertHasWarning(penalty1.CPY_DurationInfo, defaultDetentionWarning + 5);

			container1.JC_ContainerYardEmptyReturnGateIn = today.AddDays(-5);
			container1.ArrivalCarrierDetentionDays = 5;
			AssertHasWarning("If return by occurs before detention start occurs then there should be no detention days", penalty1.CPY_DurationInfo, defaultDetentionWarning + 0);

			container1.ArrivalCarrierDetentionDays = 0;
			AssertNoWarnings("Detention days now match calculated", penalty1.CPY_DurationInfo);

			container1.JC_ContainerYardEmptyReturnGateIn = today.AddDays(300);
			container1.ArrivalCarrierDetentionDays = 5;
			AssertHasWarning(penalty1.CPY_DurationInfo, defaultDetentionWarning + 255);

			container1.JC_EmptyReturnedBy = ZDateTime.Empty;
			penalty1.RunPreSaveValidation();
			AssertNoWarnings(penalty1.CPY_DurationInfo);

			penalty1 = container1.ExportPenalties.FindOrCreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, timeUnit: ContainerPenaltyTimeUnit.Codes.Days);
			penalty1.DurationAsDays = 98;
			AssertNotNull(penalty1);
			AssertNoWarnings(penalty1.CPY_DurationInfo);
		}

		[TestDate(2011, 11, 11)]
		public void TestCPY_FreeTime()
		{
			var today = ZDateTime.Today;

			var strategy = new Mock<IContainerDefaultingStrategy>();
			strategy.Setup(x => x.CalculateRequiredBy()).Returns((ZDateTime.Empty, today, "Wharf Gate Out"));
			strategy.Setup(x => x.CalculateStorageStartAvailableDate(It.IsAny<ZString>(), It.IsAny<ZString>())).Returns(today);
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 1 }))
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var consol = Factory.New(ObjectFactory.GetType<ICommonConsol>()) as CommonConsol;
				var container = Factory.New<IForwardingContainer>() as CommonContainer;
				consol.Containers.Add(container);

				container.JC_FCLAvailable = today;
				container.JC_ArrivalCTOStorageStartDate = today.AddDays(9);

				AssertEquals(1, container.ImportPenalties.Count);

				var importCTOPenalty = container.ImportPenalties.Last();
				importCTOPenalty.FreeTimeAsDays = 1;
				importCTOPenalty.Validation.ValidateCPY_FreeTime();
				AssertNoWarnings(importCTOPenalty.CPY_FreeTimeInfo);

				importCTOPenalty.FreeTimeAsDays = 2;
				importCTOPenalty.Validation.ValidateCPY_FreeTime();

				AssertHasWarning(importCTOPenalty.CPY_FreeTimeInfo, "The number of free days calculates to 1");

				container.JC_FCLWharfGateOut = today;
				AssertEquals(1, container.ImportPenalties.Count);
				var importPenalty = container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(true, false);
				AssertNoWarnings(importPenalty.CPY_FreeTimeInfo);

				container.JC_EmptyReturnedBy = today.AddDays(6);
				importPenalty.FreeTimeAsDays = 1;
				AssertHasWarning(importPenalty.CPY_FreeTimeInfo, "The difference in days between Wharf Gate Out and Empty Return Req. By calculates to 7");

				importPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;
				container.JC_EmptyReturnedBy = today.AddDays(5);
				importPenalty.FreeTimeAsDays = 3;
				AssertHasWarning(importPenalty.CPY_FreeTimeInfo, "The number of free days calculates to 6");

				var deliveryPenalty = container.DeliveryPenalties.AddNew();
				deliveryPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
				deliveryPenalty.FreeTimeAsDays = 2;
				deliveryPenalty.Validation.ValidateCPY_FreeTime();
				AssertHasWarning(deliveryPenalty.CPY_FreeTimeInfo, "The number of free days calculates to 6");

				deliveryPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;
				deliveryPenalty.FreeTimeAsDays = 2;
				deliveryPenalty.Validation.ValidateCPY_FreeTime();
				AssertHasWarning(deliveryPenalty.CPY_FreeTimeInfo, "The number of free days calculates to 6");

				var mockDetentionMatchResult = new Mock<IContainerPenaltyMatchResult>();
				mockDetentionMatchResult.Setup(c => c.FreeDays).Returns(3);

				strategy.Setup(x => x.GetMatchedDetentionPenalty(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(mockDetentionMatchResult.Object);

				var pickupPenalty = container.PickupPenalties.AddNew();
				pickupPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;

				pickupPenalty.FreeTimeAsDays = 2;
				pickupPenalty.Validation.ValidateCPY_FreeTime();
				AssertHasWarning(pickupPenalty.CPY_FreeTimeInfo, "The number of free days calculates to 3");
				
				var mockMDDMatchResult = new Mock<IContainerPenaltyMatchResult>();
				mockMDDMatchResult.Setup(c => c.FreeDays).Returns(3);

				pickupPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;
				strategy.Setup(x => x.GetMatchedMDDPenalty(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(mockMDDMatchResult.Object);
				pickupPenalty.FreeTimeAsDays = 2;
				pickupPenalty.Validation.ValidateCPY_FreeTime();
				AssertHasWarning(pickupPenalty.CPY_FreeTimeInfo, "The number of free days calculates to 3");
			}
		}

		[TestDate(2011, 11, 11)]
		public void TestCheckCPY_TotalCost()
		{
			var today = ZDateTime.Today;
			var strategy = new Mock<IContainerDefaultingStrategy>();

			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var consol = Factory.New(ObjectFactory.GetType<ICommonConsol>()) as CommonConsol;
				var container = Factory.New<IForwardingContainer>() as CommonContainer;
				consol.Containers.Add(container);

				strategy.Setup(x => x.CalculateAvailableDateForStorage(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container.JC_FCLOnBoardVessel, "FCL Loaded"));

				container.JC_FCLWharfGateIn = today;
				container.JC_FCLOnBoardVessel = today.AddDays(2);
				container.ExportPenalties.FindOrCreateDepartureCarrierStoragePenalty(true, false);

				container.ExportPenalties[0].DurationAsDays = (ZByte)(ContainerPenalty.GetDaysBetweenDatesExclusive(container.JC_FCLOnBoardVessel, container.JC_FCLWharfGateIn) - container.ExportPenalties[0].FreeTimeAsDays);
				container.ExportPenalties[0].CPY_PerUnitCost = 12;
				container.ExportPenalties[0].CPY_TotalCost = 1;
				AssertHasWarning(container.ExportPenalties[0].CPY_TotalCostInfo, "Cost Per Unit times duration does not equal Total Cost");

				container.ExportPenalties[0].CPY_TotalCost = 24;
				AssertNoWarnings(container.ExportPenalties[0].CPY_TotalCostInfo);

				container.JC_EmptyReturnedBy = today;
				container.JC_ContainerYardEmptyReturnGateIn = today.AddDays(5);
				container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(true, false);
				container.ImportPenalties[0].DurationAsDays = ContainerPenalty.GetDaysBetweenDatesExclusive(container.JC_ContainerYardEmptyReturnGateIn, container.JC_EmptyReturnedBy);
				container.ImportPenalties[0].CPY_PerUnitCost = 3;
				container.ImportPenalties[0].CPY_TotalCost = 2;
				AssertHasWarning(container.ImportPenalties[0].CPY_TotalCostInfo, "Cost Per Unit times duration does not equal Total Cost");

				container.ImportPenalties[0].CPY_TotalCost = 15;
				AssertNoWarnings(container.ImportPenalties[0].CPY_TotalCostInfo);

				container.JC_FCLAvailable = today;
				container.JC_ArrivalCTOStorageStartDate = today;
				container.JC_FCLWharfGateOut = today.AddDays(3);
				container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(true, false);
				container.ImportPenalties[1].DurationAsDays = (ZByte)(ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_FCLWharfGateOut, container.JC_FCLAvailable) - container.ImportPenalties[1].FreeTimeAsDays);
				container.ImportPenalties[1].CPY_PerUnitCost = 5;
				container.ImportPenalties[1].CPY_TotalCost = 2;
				AssertHasWarning(container.ImportPenalties[1].CPY_TotalCostInfo, "Cost Per Unit times duration does not equal Total Cost");

				container.ImportPenalties[1].CPY_TotalCost = 2;
				container.ImportPenalties[1].CPY_PerUnitCost = 5;
				container.ImportPenalties[1].DurationAsDays = 1;
				AssertHasWarning(container.ImportPenalties[1].CPY_DurationInfo, "Cost Per Unit times duration does not equal Total Cost");
				container.ImportPenalties[1].CPY_PerUnitCost = 50;
				AssertHasWarning(container.ImportPenalties[1].CPY_PerUnitCostInfo, "Cost Per Unit times duration does not equal Total Cost");

				container.JC_ArrivalCTOStorageStartDate = today;
				container.ImportPenalties[1].DurationAsDays = ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_FCLWharfGateOut, container.JC_ArrivalCTOStorageStartDate);
				container.ImportPenalties[1].CPY_PerUnitCost = 5;
				container.ImportPenalties[1].CPY_TotalCost = 20;
				container.ImportPenalties[1].RunPreSaveValidation();
				AssertNoWarnings(container.ImportPenalties[1].CPY_TotalCostInfo);
				AssertNoWarnings(container.ImportPenalties[1].CPY_DurationInfo);
				AssertNoWarnings(container.ImportPenalties[1].CPY_PerUnitCostInfo);
			}
		}

		[TestDate(2022, 8, 4)]
		public void TestCheckCPY_TotalCost_ForShipmentContainerPenalty()
		{
			var today = ZDateTime.Today;
			var strategy = new Mock<IContainerDefaultingStrategy>();

			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var consol = Factory.New(ObjectFactory.GetType<ICommonConsol>()) as CommonConsol;
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_ConsolMode = ContainerModes.FCL;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "NZAKL";

				var shipment = consol.Shipments.AddNew();
				shipment.OuterPackLines.AddNew();

				var container1 = Factory.New<IForwardingContainer>() as CommonContainer;
				consol.Containers.Add(container);

				strategy.Setup(x => x.CalculateAvailableDateForStorage(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container1.JC_FCLOnBoardVessel, "FCL Loaded"));

				container1.JC_FCLWharfGateIn = today;
				container1.JC_FCLOnBoardVessel = today.AddDays(2);

				var penalty1 = shipment.PickupPenalties.AddNew();
				penalty1.CPY_JC_Container = container1.PK;
				penalty1.DurationAsDays = 2;
				penalty1.CPY_PerUnitCost = 12;
				penalty1.CPY_TotalCost = 1;
				AssertHasWarning(penalty1.CPY_TotalCostInfo, "Sell Per Unit times duration does not equal Total Sell");

				penalty1.CPY_TotalCost = 24;
				AssertNoWarnings(penalty1.CPY_TotalCostInfo);
			}
		}

		public void TestCheckDepartureCTOStorageDays()
		{
			var strategy = new Mock<IContainerDefaultingStrategy>();
			var unlimitedFreeDaysOptions = new ContainerPenaltyFreeDaysOptions { UnlimitedFreeDays = true };

			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var container1 = Factory.New<IForwardingContainer>() as CommonContainer;
				strategy.Setup(x => x.CalculateAvailableDateForStorage(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container1.JC_FCLOnBoardVessel, "FCL Loaded"));

				var today = ZDateTime.Today;
				container1.JC_FCLWharfGateIn = today;
				container1.JC_FCLOnBoardVessel = today.AddDays(5);

				container1.ExportPenalties.FindOrCreateDepartureCarrierStoragePenalty(true, false);

				var penalty = container1.ExportPenalties.FirstOrDefault(x => x.CPY_PenaltyType == "STO");
				AssertEquals((ZByte)6, penalty.DurationAsDays);
				AssertEquals("CAR", penalty.CPY_CreditorType);

				penalty.DurationAsDays = 2;
				AssertHasWarning(penalty.CPY_DurationInfo, "Number of days in storage from FCL Loaded minus free days minus FCL Wharf Gate In plus 1 is calculated to 6");
			}
		}

		[TestDate(2011, 11, 11)]
		public void TestCheckDepartureCarrierDetentionDays()
		{
			var today = ZDateTime.Today;
			var strategy = new Mock<IContainerDefaultingStrategy>();

			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var container = Factory.New<IForwardingContainer>() as CommonContainer;
				container.JC_JK = Factory.New<CommonConsol>().PK;
				strategy.Setup(x => x.CalculateAvailableDateForDetention(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container.JC_FCLWharfGateIn, "CTO Gate In"));

				var defaultDetentionWarning = $"Number of days in detention from CTO Gate In minus free days minus {container.JC_ContainerYardEmptyPickupGateOutInfo.HumanReadableName} plus 1 is calculated to ";
				container.JC_ContainerYardEmptyPickupGateOut = today;
				container.DepartureCarrierDetentionDays = 5;
				var penalty = container.ExportPenalties.FirstOrDefault();
				AssertNotNull(penalty);
				AssertNoWarnings(penalty.CPY_DurationInfo);

				penalty.FreeTimeAsDays = 1;
				container.JC_FCLWharfGateIn = today.AddDays(3);
				container.DepartureCarrierDetentionDays = 5;
				AssertHasWarning(penalty.CPY_DurationInfo, defaultDetentionWarning + 3);

				penalty.FreeTimeAsDays = 0;
				container.JC_FCLWharfGateIn = today.AddDays(5);
				container.DepartureCarrierDetentionDays = 6;
				AssertNoWarnings(penalty.CPY_DurationInfo);

				container.DepartureCarrierDetentionDays = 0;
				AssertHasWarning(penalty.CPY_DurationInfo, defaultDetentionWarning + 6);

				container.JC_FCLWharfGateIn = today.AddDays(-5);
				container.DepartureCarrierDetentionDays = 5;
				AssertHasWarning("If wharf gate in by occurs before container yard gate out occurs then there should be no detention days", penalty.CPY_DurationInfo, defaultDetentionWarning + 0);

				container.DepartureCarrierDetentionDays = 0;
				AssertNoWarnings("Detention days now match calculated", penalty.CPY_DurationInfo);

				container.JC_FCLWharfGateIn = today.AddDays(300);
				container.DepartureCarrierDetentionDays = 5;
				AssertHasWarning(penalty.CPY_DurationInfo, defaultDetentionWarning + 255);

				container.JC_ContainerYardEmptyPickupGateOut = ZDateTime.Empty;
				penalty.RunPreSaveValidation();
				AssertNoWarnings(penalty.CPY_DurationInfo);

				penalty = container.ExportPenalties.FindOrCreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, timeUnit: ContainerPenaltyTimeUnit.Codes.Days);
				penalty.DurationAsDays = 98;
				AssertNotNull(penalty);
				AssertNoWarnings(penalty.CPY_DurationInfo);
			}
		}

		[TestDate(2011, 11, 11)]
		public void TestCheckCPY_TotalCostByDurationByTimeUnit()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container1 = consol.Containers.AddNew();

			var today = ZDateTime.Today;

			var penalty = container1.ImportPenalties.AddNew();
			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.TruckWaitTime;
			penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;

			penalty.CPY_PerUnitCost = 10m;
			penalty.CPY_Duration = new ZDateTime(today.Year, 1, 1, 15, 0, 0);
			penalty.CPY_TotalCost = 100m;

			AssertHasWarning(penalty.CPY_TotalCostInfo, "Cost Per Unit times duration does not equal Total Cost");

			penalty.CPY_TotalCost = 150m;
			AssertNoWarnings(penalty.CPY_TotalCostInfo);

			penalty.CPY_PerUnitCost = 20m;
			penalty.CPY_Duration = new ZDateTime(today.Year, 1, 1, 5, 0, 0);
			penalty.CPY_TotalCost = 100m;
			penalty.RunPreSaveValidation();
			AssertNoWarnings(penalty.CPY_TotalCostInfo);
			AssertNoWarnings(penalty.CPY_DurationInfo);
			AssertNoWarnings(penalty.CPY_PerUnitCostInfo);
		}

		#region Implemenation

		CommonContainer container;
		ContainerPenalty importPenalty;

		protected override void SetUp()
		{
			base.SetUp();

			container = Factory.New<CommonContainer>();
			importPenalty = container.ImportPenalties.AddNew();
		}

		ContainerPenalty CreateContainerPenaltyForContainer(CommonContainer container, ZString penaltyType, ZString creditorType, ZGuid creditorPk, bool isExport = true)
		{
			var penalty = isExport ? container.ExportPenalties.AddNew() : container.ImportPenalties.AddNew();
			penalty.CPY_PenaltyType = penaltyType;
			penalty.CPY_CreditorType = creditorType;
			penalty.CPY_OH_Creditor = creditorPk;
			return penalty;
		}

		#endregion
	}
}
