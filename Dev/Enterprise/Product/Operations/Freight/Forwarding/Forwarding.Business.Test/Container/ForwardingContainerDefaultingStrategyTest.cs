using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingContainerDefaultingStrategyTest : TestCaseWithFactory
	{
		public void TestPopulatingJC_EmptyReturnedBy_FromVSDDetentionFreeDays_SetByATA()
		{
			var detention = DetentionForCarrierOnly;
			detention.PD_FreeDayType = ContainerDetentionFreeDayType.VesselArrival;

			var container = Consol.Containers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;

			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_OA_ShippingLineAddress = Carrier.MainAddress.PK;
			Shipment.ConsigneePK = Importer.PK;
			Consol.JK_ConsolMode = ContainerModes.FCL;

			var transport = container.Consol.Transports.AddNew();
			transport.JW_ETA = new ZDateTime(2021, 5, 1);

			Factory.Save();
			AssertEquals("JC_EmptyReturnedBy is NOT populated from VesselArrival detention free days when ATA is still empty", ZDateTime.Empty, container.JC_EmptyReturnedBy);

			transport.JW_ATA = new ZDateTime(2021, 5, 16);
			Factory.Save();
			AssertEquals("JC_EmptyReturnedBy populated from the detention free days VesselArrival date", transport.JW_ATA.AddDays(detention.PD_FreeDays - 1), container.JC_EmptyReturnedBy);
		}

		public void TestCalculateRequiredBy()
		{
			var carrier = Factory.New<OrgHeader>();
			var detention = carrier.CarrierContainerPenalties.AddNew();
			detention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			detention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			detention.PD_DetentionPortOrCountry = Core.Constants.CountryCodes.Australia;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.Transports[0].JW_ATA = new ZDateTime(2020, 1, 15);

			var container = consol.Containers.AddNew();
			container.JC_FCLAvailable = new ZDateTime(2020, 1, 1);
			container.JC_FCLWharfGateOut = new ZDateTime(2020, 1, 5);
			container.JC_FCLUnloadFromVessel = new ZDateTime(2020, 1, 10);

			var strategy = new ForwardingContainerDefaultingStrategy(container);
			detention.PD_FreeDays = 6;
			detention.PD_FreeDayType = ContainerDetentionFreeDayType.CTOAvailable;
			AssertEquals((container.JC_FCLAvailable.AddDays(detention.PD_FreeDays - 1), container.JC_FCLAvailable, (ZString)"CTO Available"), strategy.CalculateRequiredBy());

			detention.PD_FreeDays = 7;
			detention.PD_FreeDayType = ContainerDetentionFreeDayType.CTOGateOut;
			AssertEquals((container.JC_FCLWharfGateOut.AddDays(detention.PD_FreeDays - 1), container.JC_FCLWharfGateOut, (ZString)"CTO Gate Out"), strategy.CalculateRequiredBy());

			detention.PD_FreeDays = 8;
			detention.PD_FreeDayType = ContainerDetentionFreeDayType.DayAfterFCLUnload;
			AssertEquals((container.JC_FCLUnloadFromVessel.AddDays(detention.PD_FreeDays + 1 - 1), container.JC_FCLUnloadFromVessel.AddDays(1), (ZString)"Day after FCL Unload"), strategy.CalculateRequiredBy());

			detention.PD_FreeDays = 8;
			detention.PD_FreeDayType = ContainerDetentionFreeDayType.FCLUnload;
			AssertEquals((container.JC_FCLUnloadFromVessel.AddDays(detention.PD_FreeDays - 1), container.JC_FCLUnloadFromVessel, (ZString)"FCL Unload"), strategy.CalculateRequiredBy());

			detention.PD_FreeDays = 9;
			detention.PD_FreeDayType = ContainerDetentionFreeDayType.VesselArrival;
			AssertEquals((consol.Transports[0].JW_ATA.AddDays(detention.PD_FreeDays - 1), consol.Transports[0].JW_ATA, (ZString)"Vessel Arrival"), strategy.CalculateRequiredBy());

			detention.PD_FreeDays = 0;
			detention.PD_FreeDayType = ContainerDetentionFreeDayType.CTOAvailable;
			AssertEquals((container.JC_FCLAvailable, container.JC_FCLAvailable, (ZString)"CTO Available"), strategy.CalculateRequiredBy());
		}

		public void TestCalculateRequiredBy_ExcludedFreeDays()
		{
			var carrier = Factory.New<OrgHeader>();
			var detention = carrier.CarrierContainerPenalties.AddNew();
			detention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			detention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			detention.PD_DetentionPortOrCountry = Core.Constants.CountryCodes.Australia;

			var freeDayExclusion = Factory.New<ContainerPenaltyDayExclusion>();
			freeDayExclusion.CEX_Thursday = true;
			freeDayExclusion.CEX_Friday = true;

			detention.PD_CEX_FreeDayExclusion = freeDayExclusion.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.Transports[0].JW_ATA = new ZDateTime(2020, 1, 15);

			var container = consol.Containers.AddNew();
			// Jan 1st 2020 = Wednesday
			container.JC_FCLAvailable = new ZDateTime(2020, 1, 1);
			container.JC_FCLWharfGateOut = new ZDateTime(2020, 1, 5);
			container.JC_FCLUnloadFromVessel = new ZDateTime(2020, 1, 10);

			var strategy = new ForwardingContainerDefaultingStrategy(container);
			detention.PD_FreeDays = 6;
			detention.PD_FreeDayType = ContainerDetentionFreeDayType.CTOAvailable;
			AssertEquals((container.JC_FCLAvailable.AddDays(detention.PD_FreeDays - 1 + 2), container.JC_FCLAvailable, (ZString)"CTO Available"), strategy.CalculateRequiredBy());
		}

		public void TestCalculateRequiredBy_ContractMDDPriority()
		{
			var carrier = Factory.New<OrgHeader>();
			var contract = Factory.New<IRatingContract>();
			contract.RCT_ContractNumber = "Nibelungenklage";
			contract.RCT_OH = carrier.PK;
			contract.RCT_StartDate = new ZDate(2024, 01, 01);
			contract.RCT_EndDate = new ZDate(2024, 12, 31);
			contract.RCT_ContractType = "PRO";
			contract.RCT_TransportMode = "SEA";

			var containerDetention = contract.ContainerDetentions.AddNew() as IRatingContractContainerDetention;
			containerDetention.RCD_OriginPortOrCountry = "CN";
			containerDetention.RCD_DetentionPortOrCountry = "AU";
			containerDetention.RCD_Direction = "IMP";
			containerDetention.RCD_FreeDays = 21;
			containerDetention.RCD_PenaltyType = "MDD";
			containerDetention.RCD_FreeDayType = "CTD";

			var detPenaltyOfCarrier = carrier.CarrierContainerPenalties.AddNew();
			detPenaltyOfCarrier.PD_PenaltyType = "DET";
			detPenaltyOfCarrier.PD_Direction = "IMP";
			detPenaltyOfCarrier.PD_FreeDays = 8;
			detPenaltyOfCarrier.PD_FirstFreeDayType = "FCD";
			detPenaltyOfCarrier.PD_DetentionPortOrCountry = "AU";
			detPenaltyOfCarrier.PD_CreditorType = "CAR";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_CarrierContractNumber = "Nibelungenklage";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			consol.Transports[0].JW_ATD = new ZDateTime(2024, 6, 1);

			var container = consol.Containers.AddNew();
			container.JC_FCLUnloadFromVessel = new ZDateTime(2024, 6, 6);
			container.JC_FCLAvailable = new ZDateTime(2024, 6, 4);

			var strategy = new ForwardingContainerDefaultingStrategy(container);
			AssertEquals((container.JC_FCLAvailable.AddDays(containerDetention.RCD_FreeDays - 1), container.JC_FCLAvailable, (ZString)"CTO Available"), strategy.CalculateRequiredBy());
		}

		public void TestCalculateRequiredBy_MDD()
		{
			var carrier = Factory.New<OrgHeader>();
			var mddPenalty = carrier.CarrierContainerPenalties.AddNew();
			mddPenalty.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			mddPenalty.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			mddPenalty.PD_FreeDays = 6;
			mddPenalty.PD_FreeDayType = ContainerDetentionFreeDayType.FCLUnload;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_CarrierContractNumber = "69420";
			consol.Transports[0].JW_ATA = new ZDateTime(2020, 1, 15);
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_FCLAvailable = new ZDateTime(2020, 1, 1);
			container.JC_FCLWharfGateOut = new ZDateTime(2020, 1, 5);
			container.JC_FCLUnloadFromVessel = new ZDateTime(2020, 1, 10);

			var strategy = new ForwardingContainerDefaultingStrategy(container);
			AssertEquals((container.JC_FCLUnloadFromVessel.AddDays(mddPenalty.PD_FreeDays - 1), container.JC_FCLUnloadFromVessel, (ZString)"FCL Unload"), strategy.CalculateRequiredBy());
		}

		public void TestCalculateRequiredBy_MDD_ExcludedFreeDays()
		{
			var carrier = Factory.New<OrgHeader>();
			var mddPenalty = carrier.CarrierContainerPenalties.AddNew();
			mddPenalty.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			mddPenalty.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			mddPenalty.PD_FreeDays = 6;
			mddPenalty.PD_FreeDayType = ContainerDetentionFreeDayType.FCLUnload;

			var freeDayExclusion = Factory.New<ContainerPenaltyDayExclusion>();
			freeDayExclusion.CEX_Thursday = true;
			freeDayExclusion.CEX_Friday = true;

			mddPenalty.PD_CEX_FreeDayExclusion = freeDayExclusion.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_CarrierContractNumber = "69420";
			consol.Transports[0].JW_ATA = new ZDateTime(2020, 1, 15);
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_FCLAvailable = new ZDateTime(2020, 1, 1);
			container.JC_FCLWharfGateOut = new ZDateTime(2020, 1, 5);
			container.JC_FCLUnloadFromVessel = new ZDateTime(2020, 1, 10);

			var strategy = new ForwardingContainerDefaultingStrategy(container);
			AssertEquals((container.JC_FCLUnloadFromVessel.AddDays(mddPenalty.PD_FreeDays - 1 + 3), container.JC_FCLUnloadFromVessel, (ZString)"FCL Unload"), strategy.CalculateRequiredBy());
		}

		#region Calculate Default Duration

		[TestDate(2022, 6, 20)]
		public void TestElapsedFreeTimeAndDurationAsDays()
		{
			var now = ZDateTime.Now;
			var (detention, _, container) = SetupImportCarrierDetention();
			var penalty = container.ImportPenalties.AddNew();
			penalty.CPY_PenaltyType = ContainerDetentionPenaltyType.DET;
			penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;

			detention.PD_FreeDays = 10;
			SetFirstFreeDay(-4);

			var otherFactory = new BusinessObjectFactory();
			var penaltyInOtherFactory = otherFactory.Load<ContainerPenalty>(penalty.PK);
			AssertEquals("Precondition: FirstFreeDay should be calculated whenever it is accessed", now.AddDays(-4), penaltyInOtherFactory.FirstFreeDay);

			AssertGreaterThan("Precondition: The scenario should be that LastFreeDay is after Today's date", penalty.LastFreeDay, ZDateTime.Now);
			AssertEquals("Precondition: container.JC_ContainerYardEmptyReturnGateIn should be empty", ZDateTime.Empty, container.JC_ContainerYardEmptyReturnGateIn);
			AssertEquals(penalty.FirstFreeDay.AddDays(penalty.FreeTimeAsDays - 1), penalty.LastFreeDay);
			AssertEquals(ContainerPenalty.GetDaysBetweenDatesInclusive(now, penalty.FirstFreeDay), penalty.ElapsedFreeTimeAsDays);
			AssertEquals(ZByte.Zero, penalty.ElapsedDurationAsDays);

			SetFirstFreeDay(-9);

			AssertEquals(penalty.FirstFreeDay.AddDays(penalty.FreeTimeAsDays - 1), penalty.LastFreeDay);
			AssertEquals(ContainerPenalty.GetDaysBetweenDatesInclusive(now, penalty.FirstFreeDay), penalty.ElapsedFreeTimeAsDays);
			AssertEquals(ZByte.Zero, penalty.ElapsedDurationAsDays);

			SetFirstFreeDay(-12);

			AssertEquals(penalty.FirstFreeDay.AddDays(penalty.FreeTimeAsDays - 1), penalty.LastFreeDay);
			AssertEquals(penalty.FreeTimeAsDays, penalty.ElapsedFreeTimeAsDays);
			AssertEquals(new ZByte(3), penalty.ElapsedDurationAsDays);

			penalty.FreeTimeAsDays = 3;

			AssertEquals(penalty.FirstFreeDay.AddDays(penalty.FreeTimeAsDays - 1), penalty.LastFreeDay);
			AssertEquals(penalty.FreeTimeAsDays, penalty.ElapsedFreeTimeAsDays);
			AssertEquals(ContainerPenalty.GetDaysBetweenDatesExclusive(now, penalty.LastFreeDay), penalty.ElapsedDurationAsDays);

			SetFirstFreeDay(-4);
			SetDurationAsDays(2);
			penalty.FreeTimeAsDays = 10;

			AssertEquals(penalty.FirstFreeDay.AddDays(penalty.FreeTimeAsDays - 1), penalty.LastFreeDay);
			AssertEquals(penalty.FreeTimeAsDays, penalty.ElapsedFreeTimeAsDays);
			AssertEquals(penalty.DurationAsDays, penalty.ElapsedDurationAsDays);

			SetFirstFreeDay(-25);
			SetDurationAsDays(0);
			AssertLessThan("Precondition: The scenario should be that LastFreeDay is before Today's date", penalty.LastFreeDay, ZDateTime.Now);
			AssertEquals("Precondition: Duration should be zero", ZByte.Zero, penalty.DurationAsDays);
			AssertEquals(penalty.FirstFreeDay.AddDays(penalty.FreeTimeAsDays - 1), penalty.LastFreeDay);
			AssertEquals(penalty.FreeTimeAsDays, penalty.ElapsedFreeTimeAsDays);
			AssertEquals(ZByte.Zero, penalty.ElapsedDurationAsDays);

			container.JC_ContainerYardEmptyReturnGateIn = ZDateTime.Empty;
			AssertEquals(new ZByte(16), penalty.ElapsedDurationAsDays);

			container.JC_FCLUnloadFromVessel = ZDateTime.Empty;
			penalty.FreeTimeAsDays = 10;
			AssertEquals(ZDateTime.Empty, penalty.FirstFreeDay);
			AssertEquals(ZDateTime.Empty, penalty.LastFreeDay);
			AssertEquals(ZByte.Zero, penalty.ElapsedFreeTimeAsDays);
			AssertEquals(ZByte.Zero, penalty.ElapsedDurationAsDays);

			void SetFirstFreeDay(int daysFromNow)
			{
				detention.PD_FreeDayType = ContainerDetentionFreeDayType.FCLUnload;
				container.JC_EmptyReturnedBy = ZDateTime.Empty;
				container.JC_FCLUnloadFromVessel = now.AddDays(daysFromNow);
				Factory.Save();
				AssertEquals("Precondition: FirstFreeDay is set", now.AddDays(daysFromNow), penalty.FirstFreeDay);
			}

			void SetDurationAsDays(int days)
			{
				detention.PD_FreeDayType = ContainerDetentionFreeDayType.FCLUnload;
				Assert("Preconditon: container.JC_FCLUnloadFromVessel should be valid", container.JC_FCLUnloadFromVessel.IsValid);
				container.JC_ContainerYardEmptyReturnGateIn = container.JC_FCLUnloadFromVessel.AddDays(days + penalty.FreeTimeAsDays - 1);
				Factory.Save();
				AssertEquals("Precondition: Duration is set", penalty.DurationAsDays, days);
			}
		}

		[TestDate(2020, 1, 28)]
		public void TestCalculateDefaultDurationAsDaysForImportStorage()
		{
			var (detention, consol, container) = SetupImportStorage();
			Func<ContainerPenalty> getPenalty = () => container.ImportPenalties.FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO);
			Func<string, int, string> getWarningMessage = (string _, int days) => string.Format("Number of days in storage from {0} minus CTO Storage Start plus 1 is calculated to {1}", container.JC_FCLWharfGateOutInfo.HumanReadableName, days);

			AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.CTOAvailable, consol, detention, container, getPenalty, getWarningMessage);
			AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.FCLUnload, consol, detention, container, getPenalty, getWarningMessage);
			AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.DayAfterFCLUnload, consol, detention, container, getPenalty, getWarningMessage);
			AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.VesselArrival, consol, detention, container, getPenalty, getWarningMessage);

			AssertCalculateDefaultDurationAsDaysWithNoMatchingDetention(detention: detention, getPenalty: getPenalty,
				getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_FCLWharfGateOut, container.JC_FCLAvailable) - getPenalty().FreeTimeAsDays,
				lastPenaltyDayPropertyInfo: container.JC_FCLWharfGateOutInfo, firstFreeDayPropertyInfo: container.JC_FCLAvailableInfo, true);
		}

		[TestDate(2020, 1, 28)]
		public void TestCalculateDefaultDurationAsDaysForImportDetention()
		{
			var (detention, consol, container) = SetupImportCarrierDetention();

			Func<ContainerPenalty> getImportPenalty = () => container.ImportPenalties.FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier);
			AssertPenalty(getImportPenalty,
				(string firstFreeDayName, int days) => string.Format("Number of days in detention from {0} minus {1} is calculated to {2}", container.JC_ContainerYardEmptyReturnGateInInfo.HumanReadableName, container.JC_EmptyReturnedByInfo.HumanReadableName, days),
				true);
			AssertCalculateDefaultDurationAsDaysWithNoMatchingDetention(detention: detention, getPenalty: getImportPenalty,
				getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_ContainerYardEmptyReturnGateIn, container.JC_FCLAvailable) - detention.PD_FreeDays,
				lastPenaltyDayPropertyInfo: container.JC_ContainerYardEmptyReturnGateInInfo, firstFreeDayPropertyInfo: container.JC_FCLAvailableInfo, true);

			Func<ContainerPenalty> getDeliveryPenalty = () => container.DeliveryPenalties.FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier);
			AssertPenalty(getDeliveryPenalty,
				(string firstFreeDayName, int days) => string.Format("Number of days in detention from {0} minus free days minus {1} plus 1 is calculated to {2}", container.JC_ContainerYardEmptyReturnGateInInfo.HumanReadableName, firstFreeDayName, days),
				true);
			AssertCalculateDefaultDurationAsDaysWithNoMatchingDetention(detention: detention, getPenalty: getDeliveryPenalty,
				getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_ContainerYardEmptyReturnGateIn, container.JC_FCLAvailable) - getDeliveryPenalty().FreeTimeAsDays,
				lastPenaltyDayPropertyInfo: container.JC_ContainerYardEmptyReturnGateInInfo, firstFreeDayPropertyInfo: container.JC_FCLAvailableInfo, true);

			void AssertPenalty(Func<ContainerPenalty> getPenalty, Func<string, int, string> getWarningMessage, bool hasRegistryFallback)
			{
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.CTOAvailable, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.CTOGateOut, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.FCLUnload, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.DayAfterFCLUnload, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.VesselArrival, consol, detention, container, getPenalty, getWarningMessage);
			}
		}

		[TestDate(2020, 1, 28)]
		public void TestCalculateDefaultDurationAsDaysForImportMDD()
		{
			var (detention, consol, container) = SetupImportCarrierMDD();
			Func<string, int, string> getWarningMessage = (string firstFreeDayName, int days) => string.Format("Number of days in merged demurrage then detention from {0} minus free days minus {1} plus 1 is calculated to {2}", container.JC_ContainerYardEmptyReturnGateInInfo.HumanReadableName, firstFreeDayName, days);

			AssertPenalty(() => container.ImportPenalties.FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier));
			AssertPenalty(() => container.DeliveryPenalties.FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier));

			void AssertPenalty(Func<ContainerPenalty> getPenalty)
			{
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.CTOAvailable, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.CTOGateOut, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.FCLUnload, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.DayAfterFCLUnload, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.VesselArrival, consol, detention, container, getPenalty, getWarningMessage);

				AssertCalculateDefaultDurationAsDaysWithNoMatchingDetention(detention: detention, getPenalty: getPenalty,
					getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_ContainerYardEmptyReturnGateIn, container.JC_FCLAvailable) - getPenalty().FreeTimeAsDays,
					lastPenaltyDayPropertyInfo: container.JC_ContainerYardEmptyReturnGateInInfo, firstFreeDayPropertyInfo: container.JC_FCLAvailableInfo, false);
			}
		}

		[TestDate(2020, 1, 28)]
		public void TestCalculateDefaultDurationAsDaysForExportStorage()
		{
			var (detention, consol, container) = SetupExportStorage();
			Func<ContainerPenalty> getPenalty = () => container.ExportPenalties.FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.Storage, ContainerPenaltyCreditorType.Codes.CTO);
			Func<string, int, string> getWarningMessage = (string firstFreeDayName, int days) => string.Format("Number of days in storage from {0} minus free days minus {1} plus 1 is calculated to {2}", firstFreeDayName, container.JC_FCLWharfGateInInfo.HumanReadableName, days);

			AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.FCLLoad, consol, detention, container, getPenalty, getWarningMessage);
			AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.DayBeforeFCLLoad, consol, detention, container, getPenalty, getWarningMessage);
			AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.VesselDeparture, consol, detention, container, getPenalty, getWarningMessage);
			AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.DayBeforeVesselDeparture, consol, detention, container, getPenalty, getWarningMessage);

			AssertCalculateDefaultDurationAsDaysWithNoMatchingDetention(detention: detention, getPenalty: getPenalty,
				getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_FCLOnBoardVessel, container.JC_FCLWharfGateIn) - getPenalty().FreeTimeAsDays,
				lastPenaltyDayPropertyInfo: container.JC_FCLOnBoardVesselInfo, firstFreeDayPropertyInfo: container.JC_FCLWharfGateInInfo, true);
		}

		[TestDate(2020, 1, 28)]
		public void TestCalculateDefaultDurationAsDaysForExportDetention()
		{
			var (detention, consol, container) = SetupExportCarrierDetention();
			Func<string, int, string> getWarningMessage = (string lastPenaltyDayName, int days) => string.Format("Number of days in detention from {0} minus free days minus {1} plus 1 is calculated to {2}", lastPenaltyDayName, container.JC_ContainerYardEmptyPickupGateOutInfo.HumanReadableName, days);

			AssertPenalty(() => container.ExportPenalties.FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier), true);
			AssertPenalty(() => container.PickupPenalties.FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier), true);

			void AssertPenalty(Func<ContainerPenalty> getPenalty, bool hasRegistryFallback)
			{
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.WharfGateIn, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.FCLLoad, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.DayBeforeFCLLoad, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.VesselDeparture, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.DayBeforeVesselDeparture, consol, detention, container, getPenalty, getWarningMessage);

				AssertCalculateDefaultDurationAsDaysWithNoMatchingDetention(detention: detention, getPenalty: getPenalty,
					getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_FCLWharfGateIn, container.JC_ContainerYardEmptyPickupGateOut) - getPenalty().FreeTimeAsDays,
					lastPenaltyDayPropertyInfo: container.JC_FCLWharfGateInInfo, firstFreeDayPropertyInfo: container.JC_ContainerYardEmptyPickupGateOutInfo, hasRegistryFallback);
			}
		}

		[TestDate(2020, 1, 28)]
		public void TestCalculateDefaultDurationAsDaysForExportMDD()
		{
			var (detention, consol, container) = SetupExportCarrierMDD();
			Func<string, int, string> getWarningMessage = (string lastPenaltyDayName, int days) => string.Format("Number of days in merged detention then demurrage from {0} minus free days minus {1} plus 1 is calculated to {2}", lastPenaltyDayName, container.JC_ContainerYardEmptyPickupGateOutInfo.HumanReadableName, days);

			AssertPenalty(() => container.ExportPenalties.FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier));
			AssertPenalty(() => container.PickupPenalties.FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, ContainerPenaltyCreditorType.Codes.Carrier));

			void AssertPenalty(Func<ContainerPenalty> getPenalty)
			{
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.FCLLoad, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.DayBeforeFCLLoad, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.VesselDeparture, consol, detention, container, getPenalty, getWarningMessage);
				AssertCalculateDefaultDurationAsDays(ContainerDetentionFreeDayType.DayBeforeVesselDeparture, consol, detention, container, getPenalty, getWarningMessage);

				AssertCalculateDefaultDurationAsDaysWithNoMatchingDetention(detention: detention, getPenalty: getPenalty,
					getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_FCLWharfGateIn, container.JC_ContainerYardEmptyPickupGateOut) - getPenalty().FreeTimeAsDays,
					lastPenaltyDayPropertyInfo: container.JC_FCLWharfGateInInfo, firstFreeDayPropertyInfo: container.JC_ContainerYardEmptyPickupGateOutInfo, false);
			}
		}

		[TestDate(2022, 6, 20)]
		public void TestPersistentFirstFreeDay()
		{
			var now = ZDateTime.Now;
			var (detention, _, container) = SetupImportCarrierDetention();
			var penalty = container.ImportPenalties.AddNew();
			penalty.CPY_PenaltyType = ContainerDetentionPenaltyType.DET;
			penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;

			detention.PD_FreeDayType = ContainerDetentionFreeDayType.FCLUnload;
			container.JC_EmptyReturnedBy = ZDateTime.Empty;
			container.JC_FCLUnloadFromVessel = now.AddDays(-4);
			Factory.Save();

			var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");

			AssertEquals("Precondition: FirstFreeDay is set", now.AddDays(-4), penalty.FirstFreeDay);
			AssertEquals("Precondition: FirstFreeDay is set", now.AddDays(-4).ToDateTimeOffset(unloco), penalty.CPY_FirstFreeDay);
		}

		(OrgContainerDetention detention, ForwardingConsol consol, ForwardingContainer container) SetupImportCarrierDetention()
		{
			var consol = SetupConsolForPenalties();
			var consignee = consol.Shipments[0].Consignee;
			var container = consol.Containers[0];

			var detention = consignee.ConsigneeContainerPenalties.AddNew();
			detention.PD_PenaltyType = ContainerDetentionPenaltyType.DET;
			detention.PD_FreeDays = 3;
			detention.PD_CreditorType = "CAR";
			detention.PD_DetentionPortOrCountry = "USLAX";
			detention.PD_OH_Carrier = consol.ShippingLinePK;

			return (detention, consol, container);
		}

		(OrgContainerDetention detention, ForwardingConsol consol, ForwardingContainer container) SetupExportCarrierDetention()
		{
			var consol = SetupConsolForPenalties();
			var consignor = consol.Shipments[0].Consignor;
			var container = consol.Containers[0];

			var detention = consignor.ConsignorContainerPenalties.AddNew();
			detention.PD_PenaltyType = ContainerDetentionPenaltyType.DET;
			detention.PD_FreeDays = 3;
			detention.PD_CreditorType = "CAR";
			detention.PD_DetentionPortOrCountry = "AUSYD";
			detention.PD_OH_Carrier = consol.ShippingLinePK;

			return (detention, consol, container);
		}

		(OrgContainerDetention detention, ForwardingConsol consol, ForwardingContainer container) SetupImportStorage()
		{
			var consol = SetupConsolForPenalties();
			var consignee = consol.Shipments[0].Consignee;
			var container = consol.Containers[0];

			var detention = consignee.ConsigneeCTOStorages.AddNew();
			detention.PD_FreeDays = 3;
			detention.PD_CreditorType = "CTO";
			detention.PD_OH_Carrier = consol.ShippingLinePK;

			return (detention, consol, container);
		}

		(OrgContainerDetention detention, ForwardingConsol consol, ForwardingContainer container) SetupExportStorage()
		{
			var consol = SetupConsolForPenalties();
			var consignor = consol.Shipments[0].Consignor;
			var container = consol.Containers[0];

			var detention = consignor.ConsignorCTOStorages.AddNew();
			detention.PD_FreeDays = 3;
			detention.PD_CreditorType = "CTO";
			detention.PD_OH_Carrier = consol.ShippingLinePK;

			return (detention, consol, container);
		}

		(OrgContainerDetention detention, ForwardingConsol consol, ForwardingContainer container) SetupImportCarrierMDD()
		{
			var consol = SetupConsolForPenalties();
			var consignee = consol.Shipments[0].Consignee;
			var container = consol.Containers[0];

			var detention = consignee.ConsigneeContainerPenalties.AddNew();
			detention.PD_PenaltyType = ContainerDetentionPenaltyType.MDD;
			detention.PD_FreeDays = 3;
			detention.PD_CreditorType = "CAR";
			detention.PD_DetentionPortOrCountry = "USLAX";
			detention.PD_OH_Carrier = consol.ShippingLinePK;

			return (detention, consol, container);
		}

		(OrgContainerDetention detention, ForwardingConsol consol, ForwardingContainer container) SetupExportCarrierMDD()
		{
			var consol = SetupConsolForPenalties();
			var consignor = consol.Shipments[0].Consignor;
			var container = consol.Containers[0];

			var detention = consignor.ConsignorContainerPenalties.AddNew();
			detention.PD_PenaltyType = ContainerDetentionPenaltyType.MDD;
			detention.PD_FreeDays = 3;
			detention.PD_CreditorType = "CAR";
			detention.PD_DetentionPortOrCountry = "AUSYD";
			detention.PD_OH_Carrier = consol.ShippingLinePK;

			return (detention, consol, container);
		}

		ForwardingConsol SetupConsolForPenalties()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.MainAddress.OA_Address1 = "Consignee Address";
			consignee.OH_IsConsignee = true;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "CONSIGNOR";
			consignor.MainAddress.OA_Address1 = "Consignor Address";
			consignor.OH_IsConsignor = true;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			var container = consol.Containers.AddNew();
			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.SetContainer(consol, container);

			return consol;
		}

		void AssertCalculateDefaultDurationAsDays(ZString freeDayType, ForwardingConsol consol, OrgContainerDetention detention, ForwardingContainer container, Func<ContainerPenalty> getPenalty, Func<string, int, string> getWarningMessage)
		{
			ZPropertyInfo returnDateInfo = null;
			if (detention.PD_PenaltyType == ContainerDetentionPenaltyType.DET && detention.PD_Direction == ContainerDetentionDirection.Import)
			{
				returnDateInfo = container.JC_ContainerYardEmptyReturnGateInInfo;
			}
			else if (detention.PD_PenaltyType == ContainerDetentionPenaltyType.DET && detention.PD_Direction == ContainerDetentionDirection.Export)
			{
				returnDateInfo = container.JC_ContainerYardEmptyPickupGateOutInfo;
			}
			else if (detention.PD_PenaltyType == ContainerDetentionPenaltyType.STO && detention.PD_Direction == ContainerDetentionDirection.Import)
			{
				returnDateInfo = container.JC_FCLWharfGateOutInfo;
			}
			else if (detention.PD_PenaltyType == ContainerDetentionPenaltyType.STO && detention.PD_Direction == ContainerDetentionDirection.Export)
			{
				returnDateInfo = container.JC_FCLWharfGateInInfo;
			}
			else if (detention.PD_PenaltyType == ContainerDetentionPenaltyType.MDD && detention.PD_Direction == ContainerDetentionDirection.Import)
			{
				returnDateInfo = container.JC_ContainerYardEmptyReturnGateInInfo;
			}
			else if (detention.PD_PenaltyType == ContainerDetentionPenaltyType.MDD && detention.PD_Direction == ContainerDetentionDirection.Export)
			{
				returnDateInfo = container.JC_ContainerYardEmptyPickupGateOutInfo;
			}

			AssertNotNull("Precondition: returnDateInfo should not be null", returnDateInfo);

			switch (freeDayType)
			{
				case ContainerDetentionFreeDayType.CTOAvailable:
					AssertCalculateDefaultDurationAsDays(
						freeDayType: ContainerDetentionFreeDayType.CTOAvailable,
						detention: detention,
						container: container,
						getPenalty: getPenalty,
						getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive((ZDateTime)returnDateInfo.Value, container.JC_FCLAvailable) - detention.PD_FreeDays,
						getExpectedFirstFreeDay: () => container.JC_FCLAvailable,
						firstFreeDayPropertyInfo: container.JC_FCLAvailableInfo,
						lastPenaltyDayPropertyInfo: returnDateInfo,
						getDurationWarningMessage: days => getWarningMessage("CTO Available", days));
					break;
				case ContainerDetentionFreeDayType.CTOGateOut:
					AssertCalculateDefaultDurationAsDays(
						freeDayType: ContainerDetentionFreeDayType.CTOGateOut,
						detention: detention,
						container: container,
						getPenalty: getPenalty,
						getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive((ZDateTime)returnDateInfo.Value, container.JC_FCLWharfGateOut) - detention.PD_FreeDays,
						getExpectedFirstFreeDay: () => container.JC_FCLWharfGateOut,
						firstFreeDayPropertyInfo: container.JC_FCLWharfGateOutInfo,
						lastPenaltyDayPropertyInfo: returnDateInfo,
						getDurationWarningMessage: days => getWarningMessage("CTO Gate Out", days));
					break;
				case ContainerDetentionFreeDayType.FCLUnload:
					AssertCalculateDefaultDurationAsDays(
						freeDayType: ContainerDetentionFreeDayType.FCLUnload,
						detention: detention,
						container: container,
						getPenalty: getPenalty,
						getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive((ZDateTime)returnDateInfo.Value, container.JC_FCLUnloadFromVessel) - detention.PD_FreeDays,
						getExpectedFirstFreeDay: () => container.JC_FCLUnloadFromVessel,
						firstFreeDayPropertyInfo: container.JC_FCLUnloadFromVesselInfo,
						lastPenaltyDayPropertyInfo: returnDateInfo,
						getDurationWarningMessage: days => getWarningMessage("FCL Unload", days));
					break;
				case ContainerDetentionFreeDayType.DayAfterFCLUnload:
					AssertCalculateDefaultDurationAsDays(
						freeDayType: ContainerDetentionFreeDayType.DayAfterFCLUnload,
						detention: detention,
						container: container,
						getPenalty: getPenalty,
						getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive((ZDateTime)returnDateInfo.Value, container.JC_FCLUnloadFromVessel.AddDays(1)) - detention.PD_FreeDays,
						getExpectedFirstFreeDay: () => container.JC_FCLUnloadFromVessel.AddDays(1),
						firstFreeDayPropertyInfo: container.JC_FCLUnloadFromVesselInfo,
						lastPenaltyDayPropertyInfo: returnDateInfo,
						getDurationWarningMessage: days => getWarningMessage("Day after FCL Unload", days));
					break;
				case ContainerDetentionFreeDayType.VesselArrival:
					AssertCalculateDefaultDurationAsDays(
						freeDayType: ContainerDetentionFreeDayType.VesselArrival,
						detention: detention,
						container: container,
						getPenalty: getPenalty,
						getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive((ZDateTime)returnDateInfo.Value, consol.Transports[0].JW_ATA) - detention.PD_FreeDays,
						getExpectedFirstFreeDay: () => consol.Transports[0].JW_ATA,
						firstFreeDayPropertyInfo: consol.Transports[0].JW_ATAInfo,
						lastPenaltyDayPropertyInfo: returnDateInfo,
						getDurationWarningMessage: days => getWarningMessage("Vessel Arrival", days));
					break;
				case ContainerDetentionFreeDayType.WharfGateIn:
					AssertCalculateDefaultDurationAsDays(
						freeDayType: ContainerDetentionFreeDayType.WharfGateIn,
						detention: detention,
						container: container,
						getPenalty: getPenalty,
						getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_FCLWharfGateIn, (ZDateTime)returnDateInfo.Value) - detention.PD_FreeDays,
						getExpectedFirstFreeDay: () => (ZDateTime)returnDateInfo.Value,
						firstFreeDayPropertyInfo: returnDateInfo,
						lastPenaltyDayPropertyInfo: container.JC_FCLWharfGateInInfo,
						getDurationWarningMessage: days => getWarningMessage("CTO Gate In", days));
					break;
				case ContainerDetentionFreeDayType.FCLLoad:
					AssertCalculateDefaultDurationAsDays(
						freeDayType: ContainerDetentionFreeDayType.FCLLoad,
						detention: detention,
						container: container,
						getPenalty: getPenalty,
						getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_FCLOnBoardVessel, (ZDateTime)returnDateInfo.Value) - detention.PD_FreeDays,
						getExpectedFirstFreeDay: () => (ZDateTime)returnDateInfo.Value,
						firstFreeDayPropertyInfo: returnDateInfo,
						lastPenaltyDayPropertyInfo: container.JC_FCLOnBoardVesselInfo,
						getDurationWarningMessage: days => getWarningMessage("FCL Load", days));
					break;
				case ContainerDetentionFreeDayType.DayBeforeFCLLoad:
					AssertCalculateDefaultDurationAsDays(
						freeDayType: ContainerDetentionFreeDayType.DayBeforeFCLLoad,
						detention: detention,
						container: container,
						getPenalty: getPenalty,
						getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_FCLOnBoardVessel, (ZDateTime)returnDateInfo.Value) - detention.PD_FreeDays - 1,
						getExpectedFirstFreeDay: () => (ZDateTime)returnDateInfo.Value,
						firstFreeDayPropertyInfo: returnDateInfo,
						lastPenaltyDayPropertyInfo: container.JC_FCLOnBoardVesselInfo,
						getDurationWarningMessage: days => getWarningMessage("Day before FCL Load", days));
					break;
				case ContainerDetentionFreeDayType.VesselDeparture:
					AssertCalculateDefaultDurationAsDays(
						freeDayType: ContainerDetentionFreeDayType.VesselDeparture,
						detention: detention,
						container: container,
						getPenalty: getPenalty,
						getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive(consol.Transports[0].JW_ATD, (ZDateTime)returnDateInfo.Value) - detention.PD_FreeDays,
						getExpectedFirstFreeDay: () => (ZDateTime)returnDateInfo.Value,
						firstFreeDayPropertyInfo: returnDateInfo,
						lastPenaltyDayPropertyInfo: consol.Transports[0].JW_ATDInfo,
						getDurationWarningMessage: days => getWarningMessage("Vessel Departure", days));
					break;
				case ContainerDetentionFreeDayType.DayBeforeVesselDeparture:
					AssertCalculateDefaultDurationAsDays(
						freeDayType: ContainerDetentionFreeDayType.DayBeforeVesselDeparture,
						detention: detention,
						container: container,
						getPenalty: getPenalty,
						getExpectedDurationDays: () => ContainerPenalty.GetDaysBetweenDatesInclusive(consol.Transports[0].JW_ATD, (ZDateTime)returnDateInfo.Value) - detention.PD_FreeDays - 1,
						getExpectedFirstFreeDay: () => (ZDateTime)returnDateInfo.Value,
						firstFreeDayPropertyInfo: returnDateInfo,
						lastPenaltyDayPropertyInfo: consol.Transports[0].JW_ATDInfo,
						getDurationWarningMessage: days => getWarningMessage("Day before Vessel Departure", days));
					break;
				default:
					throw new IndexOutOfRangeException($"FreeDayType {freeDayType} is not valid.");
			}
		}

		void AssertCalculateDefaultDurationAsDays(ZString freeDayType, OrgContainerDetention detention, ForwardingContainer container,
			Func<ContainerPenalty> getPenalty, Func<int> getExpectedDurationDays, Func<ZDateTime> getExpectedFirstFreeDay,
			ZPropertyInfo lastPenaltyDayPropertyInfo, ZPropertyInfo firstFreeDayPropertyInfo, Func<int, string> getDurationWarningMessage)
		{
			detention.PD_FreeDayType = freeDayType;
			container.JC_EmptyReturnedBy = ZDateTime.Empty;
			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Empty;

			container.ImportPenalties.DeleteAll();
			container.DeliveryPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			container.PickupPenalties.DeleteAll();

			lastPenaltyDayPropertyInfo.Value = ZDateTime.Today.AddDays(3);
			firstFreeDayPropertyInfo.Value = ZDateTime.Today.AddDays(-20);
			Factory.Save();

			var penalty = getPenalty();
			AssertEquals(getExpectedDurationDays(), penalty.DurationAsDays);
			AssertNoNotifications(penalty.CPY_DurationInfo);

			AssertEquals(getExpectedFirstFreeDay(), penalty.FirstFreeDay);
			AssertEquals(!penalty.FirstFreeDay.IsEmpty ? penalty.FirstFreeDay.AddDays(detention.PD_FreeDays - 1) : ZDateTime.Empty, penalty.LastFreeDay);

			AssertGreaterThan("Precondition: Container is in penalty", ZDateTime.Today, penalty.LastFreeDay);
			AssertEquals(penalty.FreeTimeAsDays, penalty.ElapsedFreeTimeAsDays);
			AssertEquals(penalty.DurationAsDays, penalty.ElapsedDurationAsDays);

			penalty.DurationAsDays = (ZByte)(penalty.DurationAsDays + 1);
			var warningMessage = getDurationWarningMessage(getExpectedDurationDays());
			if (!string.IsNullOrEmpty(warningMessage))
			{
				AssertHasWarning(penalty.CPY_DurationInfo, warningMessage);
			}

			lastPenaltyDayPropertyInfo.Value = ZDateTime.Empty;
			AssertEquals("Precondition: Container is not returned", ZDateTime.Empty, penalty.CPY_Duration);

			AssertEquals(penalty.FirstFreeDay > ZDateTime.Today ? ContainerPenalty.GetDaysBetweenDatesInclusive(ZDateTime.Today, penalty.FirstFreeDay) : penalty.FreeTimeAsDays, penalty.ElapsedFreeTimeAsDays);
			AssertEquals(ContainerPenalty.GetDaysBetweenDatesExclusive(ZDateTime.Today, penalty.LastFreeDay), penalty.ElapsedDurationAsDays);

			penalty.DurationAsDays = new ZByte(123);
			AssertNoNotifications(penalty.CPY_DurationInfo);

			lastPenaltyDayPropertyInfo.Value = ZDateTime.Empty;
			firstFreeDayPropertyInfo.Value = ZDateTime.Empty;
		}

		void AssertCalculateDefaultDurationAsDaysWithNoMatchingDetention(OrgContainerDetention detention, Func<ContainerPenalty> getPenalty,
			Func<int> getExpectedDurationDays, ZPropertyInfo lastPenaltyDayPropertyInfo, ZPropertyInfo firstFreeDayPropertyInfo, bool hasRegistryFallback)
		{
			var penalty = getPenalty();

			var oldDetentionOriginPort = detention.PD_OriginPortOrCountry;
			var oldDetentionPortPort = detention.PD_DetentionPortOrCountry;
			detention.PD_OriginPortOrCountry = "XXZZZ";
			detention.PD_DetentionPortOrCountry = "ZZXXX";

			lastPenaltyDayPropertyInfo.Value = ZDateTime.Today.AddDays(3);
			firstFreeDayPropertyInfo.Value = ZDateTime.Today.AddDays(-20);

			AssertEquals("Duration should be based on default free day type when no matching org detention", getExpectedDurationDays(), penalty.DurationAsDays);
			AssertNoNotifications(penalty.CPY_DurationInfo);

			penalty.Delete();
			AssertNull(getPenalty());

			lastPenaltyDayPropertyInfo.Value = ZDateTime.Empty;
			firstFreeDayPropertyInfo.Value = ZDateTime.Empty;

			penalty = getPenalty();
			if (hasRegistryFallback)
			{
				AssertNotNull(penalty);
			}
			else
			{
				AssertNull(penalty);
			}

			detention.PD_OriginPortOrCountry = oldDetentionOriginPort;
			detention.PD_DetentionPortOrCountry = oldDetentionPortPort;
		}

		#endregion

		public void TestSettingContainerSetsDefaultContainerYards()
		{
			OrgAddress address1 = Factory.New<OrgAddress>();
			OrgAddress address2 = Factory.New<OrgAddress>();

			RefContainer refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "Z123";
			refContainer.RC_StorageClass = "20F";

			OrgHeader carrier = Factory.New<OrgHeader>();
			OrgCarrierAppointedAgentPorts agentPort = carrier.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
			agentPort.O5_PortOrCountry = "AUSYD";
			agentPort.O5_OA_AgentOfficeAddress = address1.PK;
			OrgParkContainerType type = agentPort.ContainerTypes.AddNew();
			type.PT_ContainerStorageClass = "20F";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			ForwardingContainer container = consol.Containers.AddNew();

			container.JC_RC = refContainer.PK;
			AssertEquals("NO carrier set", ZGuid.Empty, container.JC_OA_DepartureContainerYardAddress);
			AssertEquals("NO carrier set", ZGuid.Empty, container.JC_OA_ArrivalContainerYardAddress);

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			container.JC_RC = ZGuid.Empty;
			container.JC_RC = refContainer.PK;
			AssertEquals(address1.PK, container.JC_OA_DepartureContainerYardAddress);
			AssertEquals("No container yard/park set for discharge port", ZGuid.Empty, container.JC_OA_ArrivalContainerYardAddress);

			OrgCarrierAppointedAgentPorts agentPort2 = carrier.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
			agentPort2.O5_PortOrCountry = "USLAX";
			agentPort2.O5_OA_AgentOfficeAddress = address2.PK;
			OrgParkContainerType type2 = agentPort2.ContainerTypes.AddNew();
			type2.PT_ContainerStorageClass = "20F";

			container.JC_RC = ZGuid.Empty;
			container.JC_RC = refContainer.PK;
			AssertEquals(address1.PK, container.JC_OA_DepartureContainerYardAddress);
			AssertEquals(address2.PK, container.JC_OA_ArrivalContainerYardAddress);
		}

		public void TestNullOrgContainerDetention_HasNoException()
		{
			var container = Consol.Containers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			var transport1 = container.Consol.Transports[0];
			transport1.JW_ATA = new ZDateTime(2000, 4, 1);

			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_OA_ShippingLineAddress = Carrier.MainAddress.PK;
			Shipment.ConsigneePK = Importer.PK;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			Carrier.CarrierContainerPenalties.DeleteAll();
			Importer.ConsigneeContainerPenalties.DeleteAll();

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestPopulatingJC_EmptyReturnedBy_FromDetentionFreeDays_ForFCLConsol()
		{
			AssertPopulatingJC_EmptyReturnedBy_FromDetentionFreeDaysIsCalculatedFromFCLAvailableDate_ForConsol(Core.Constants.ContainerModes.FCL, DetentionForCarrierAndImporter);
		}

		public void TestPopulatingJC_EmptyReturnedBy_FromDetentionFreeDays_ForLCLConsol()
		{
			AssertPopulatingJC_EmptyReturnedBy_FromDetentionFreeDaysIsCalculatedFromFCLAvailableDate_ForConsol(Core.Constants.ContainerModes.LCL, DetentionForCarrierOnly);
		}

		void AssertPopulatingJC_EmptyReturnedBy_FromDetentionFreeDaysIsCalculatedFromFCLAvailableDate_ForConsol(ZString containerMode, OrgContainerDetention detention)
		{
			var container = Consol.Containers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			container.JC_FCLAvailable = new ZDateTime(2000, 1, 1);
			container.JC_FCLWharfGateOut = new ZDateTime(2000, 3, 1);

			var transport1 = container.Consol.Transports[0];
			var transport2 = container.Consol.Transports.AddNew();
			transport1.JW_ATA = new ZDateTime(2000, 4, 1);
			transport2.JW_ATA = new ZDateTime(2000, 4, 10);

			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_OA_ShippingLineAddress = Carrier.MainAddress.PK;
			Shipment.ConsigneePK = Importer.PK;
			Consol.JK_ConsolMode = containerMode;

			Factory.Save();
			AssertEquals("PD_FreeDayType should default to CTOAvailable", detention.PD_FreeDayType, Core.Constants.ContainerDetentionFreeDayType.CTOAvailable);
			AssertEquals("JC_EmptyReturnedBy populated from the detention free days and FCL Available date", container.JC_FCLAvailable.AddDays(detention.PD_FreeDays - 1), container.JC_EmptyReturnedBy);

			Consol.JK_ConsolMode = "";
			container.JC_EmptyReturnedBy = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("JC_EmptyReturnedBy remains empty", ZDateTime.Empty, container.JC_EmptyReturnedBy);

			Consol.JK_ConsolMode = containerMode;
			Factory.Save();
			AssertEquals("JC_EmptyReturnedBy populated from the detention free days and FCL Available date - even when no changes made to container itself", container.JC_FCLAvailable.AddDays(detention.PD_FreeDays - 1), container.JC_EmptyReturnedBy);

			detention.PD_FreeDayType = Core.Constants.ContainerDetentionFreeDayType.DayAfterFCLUnload;
			container.JC_EmptyReturnedBy = ZDateTime.Empty;
			AssertEquals("Precondition", ZDateTime.Empty, container.JC_FCLUnloadFromVessel);
			AssertNoExceptionThrown("DayAfterFCLUnload with empty JC_FCLUnloadFromVessel", () => Factory.Save());

			detention.PD_FreeDayType = Core.Constants.ContainerDetentionFreeDayType.DayAfterFCLUnload;
			container.JC_FCLUnloadFromVessel = new ZDateTime(2000, 2, 1);
			container.JC_EmptyReturnedBy = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("JC_EmptyReturnedBy populated from DayAfterFCLUnload date", container.JC_FCLUnloadFromVessel.AddDays(detention.PD_FreeDays + 1 - 1), container.JC_EmptyReturnedBy);

			detention.PD_FreeDayType = Core.Constants.ContainerDetentionFreeDayType.FCLUnload;
			container.JC_EmptyReturnedBy = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("JC_EmptyReturnedBy populated from the detention free days and FCLUnload date", container.JC_FCLUnloadFromVessel.AddDays(detention.PD_FreeDays - 1), container.JC_EmptyReturnedBy);

			detention.PD_FreeDayType = Core.Constants.ContainerDetentionFreeDayType.CTOGateOut;
			container.JC_EmptyReturnedBy = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("JC_EmptyReturnedBy populated from the detention free days CTOGateOut date", container.JC_FCLWharfGateOut.AddDays(detention.PD_FreeDays - 1), container.JC_EmptyReturnedBy);

			detention.PD_FreeDayType = Core.Constants.ContainerDetentionFreeDayType.VesselArrival;
			container.JC_EmptyReturnedBy = ZDateTime.Empty;
			Factory.Save();
			AssertEquals("JC_EmptyReturnedBy populated from the detention free days VesselArrival date", transport2.JW_ATA.AddDays(detention.PD_FreeDays - 1), container.JC_EmptyReturnedBy);
		}

		public void TestPopulatingJC_EmptyReturnedBy_FromDetentionFreeDays_ForFCLDeclaration()
		{
			AssertPopulatingJC_EmptyReturnedBy_FromDetentionFreeDaysIsCalculatedFromFCLAvailableDate_ForDeclaration(Core.Constants.ContainerModes.FCL, DetentionForCarrierAndImporter);
		}

		public void TestPopulatingJC_EmptyReturnedBy_FromDetentionFreeDays_ForLCLDeclaration()
		{
			AssertPopulatingJC_EmptyReturnedBy_FromDetentionFreeDaysIsCalculatedFromFCLAvailableDate_ForDeclaration(Core.Constants.ContainerModes.LCL, DetentionForCarrierAndImporter);
		}

		void AssertPopulatingJC_EmptyReturnedBy_FromDetentionFreeDaysIsCalculatedFromFCLAvailableDate_ForDeclaration(ZString containerMode, OrgContainerDetention detention)
		{
			var cusContainers = (BusinessObjectCollection)Declaration["CusContainers"];
			var container = (ForwardingContainer)cusContainers.AddNew()["JobContainer"];

			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			container.JC_FCLAvailable = new ZDateTime(2000, 1, 1);
			container.JC_FCLUnloadFromVessel = new ZDateTime(2000, 2, 1);
			container.JC_FCLWharfGateOut = new ZDateTime(2000, 3, 1);

			Declaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = "AUSYD";
			Declaration[JobDeclarationSchema.JE_OH_ShippingLine] = Carrier.PK;
			Declaration[JobDeclarationSchema.JE_OH_Importer] = Importer.PK;
			Declaration[JobDeclarationSchema.JE_ContainerMode] = containerMode;

			Factory.Save();

			AssertEquals("JC_EmptyReturnedBy populated from the detention free days and FCL Available date", container.JC_FCLAvailable.AddDays(detention.PD_FreeDays - 1), container.JC_EmptyReturnedBy);
		}

		public void TestNonContainerisedCargoExcluded_ForConsol()
		{
			OrgContainerDetention detentionForCarrierOnly = this.DetentionForCarrierOnly;
			CommonContainer container = Consol.Containers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			container.JC_FCLAvailable = new ZDateTime(2000, 1, 1);
			container.JC_LCLAvailable = new ZDateTime(2000, 1, 1);

			Consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "AUSYD";
			Consol.JK_OA_ShippingLineAddress = Carrier.MainAddress.PK;
			Shipment.ConsigneePK = Importer.PK;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;

			Factory.Save();
			AssertEquals("JC_EmptyReturnedBy not populated unless containerised", true, container.JC_EmptyReturnedBy.IsEmpty);
		}

		public void TestNonContainerisedCargoExcluded_ForDeclaration()
		{
			OrgContainerDetention detentionForCarrierOnly = this.DetentionForCarrierOnly;
			BusinessObjectCollection cusContainers = (BusinessObjectCollection)Declaration["CusContainers"];
			CommonContainer container = (CommonContainer)cusContainers.AddNew()["JobContainer"];
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			container.JC_FCLAvailable = new ZDateTime(2000, 1, 1);
			container.JC_LCLAvailable = new ZDateTime(2000, 1, 1);

			Declaration[JobDeclarationSchema.JE_RL_NKPortOfLoading] = "AUSYD";
			Declaration[JobDeclarationSchema.JE_OH_ShippingLine] = Carrier.PK;
			Declaration[JobDeclarationSchema.JE_OH_Importer] = Importer.PK;
			Declaration[JobDeclarationSchema.JE_ContainerMode] = Core.Constants.ContainerModes.BreakBulk;

			Factory.Save();
			AssertEquals("JC_EmptyReturnedBy not populated unless containerised", true, container.JC_EmptyReturnedBy.IsEmpty);
		}

		public void TestCorrectDetentionClientIsReturnedForMultipleShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();

			var detention1 = Carrier.CarrierContainerPenalties.AddNew();
			detention1.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			detention1.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			detention1.PD_OH_Client = importer1.PK;
			detention1.PD_ContainerType = "20F";
			detention1.PD_OriginPortOrCountry = "AU";
			detention1.PD_FreeDays = 1;

			var detention2 = Carrier.CarrierContainerPenalties.AddNew();
			detention2.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			detention2.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			detention2.PD_OH_Client = importer2.PK;
			detention2.PD_ContainerType = "20F";
			detention2.PD_OriginPortOrCountry = "AU";
			detention2.PD_FreeDays = 3;

			shipment1.ConsigneePK = importer1.PK;
			shipment2.ConsigneePK = importer2.PK;

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			container1.JC_FCLAvailable = new ZDateTime(2015, 1, 1);

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			container2.JC_FCLAvailable = new ZDateTime(2015, 1, 1);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = Carrier.MainAddress.PK;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var packline1 = shipment1.OuterPackLines.AddNew();
			var packline2 = shipment2.OuterPackLines.AddNew();

			packline1.SetContainer(consol, container1);
			packline2.SetContainer(consol, container2);

			Factory.Save();

			AssertEquals("JC_EmptyReturnedBy for container1 is correct", container1.JC_FCLAvailable.AddDays(detention1.PD_FreeDays - 1), container1.JC_EmptyReturnedBy);
			AssertEquals("JC_EmptyReturnedBy for container2 is correct", container2.JC_FCLAvailable.AddDays(detention2.PD_FreeDays - 1), container2.JC_EmptyReturnedBy);
		}

		public void TestCarrierForDetentionCalculation()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			var importCTOStorage1 = carrier1.CarrierContainerPenalties.AddNew();
			importCTOStorage1.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importCTOStorage1.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			importCTOStorage1.PD_FreeDays = 55;

			var exportCTOStorage1 = carrier1.CarrierContainerPenalties.AddNew();
			exportCTOStorage1.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportCTOStorage1.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			exportCTOStorage1.PD_FreeDays = 66;

			var importCTOStorage2 = carrier2.CarrierContainerPenalties.AddNew();
			importCTOStorage2.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importCTOStorage2.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			importCTOStorage2.PD_FreeDays = 77;

			var exportCTOStorage2 = carrier2.CarrierContainerPenalties.AddNew();
			exportCTOStorage2.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportCTOStorage2.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			exportCTOStorage2.PD_FreeDays = 88;

			Factory.Save();
			{
				var consol = Factory.New<ForwardingConsol>();
				var container = consol.Containers.AddNew();

				consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
				consol.JK_OA_ShippingLineAddress = carrier1.MainAddress.PK;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";

				var strategy = new ForwardingContainerDefaultingStrategy(container);

				var matchResult = strategy.GetMatchedStoragePenalty("IMP", "CAR", Moq.It.IsAny<ZString>());
				AssertNotNull(matchResult);
				AssertEquals(new ZByte(55), matchResult.FreeDays);

				matchResult = strategy.GetMatchedStoragePenalty("EXP", "CAR", Moq.It.IsAny<ZString>());
				AssertNotNull(matchResult);
				AssertEquals(new ZByte(66), matchResult.FreeDays);
			}

			{
				var consol = Factory.New<ForwardingConsol>();
				var container = consol.Containers.AddNew();

				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

				var transport1 = consol.Transports[0];
				transport1.JW_TransportMode = "SEA";
				transport1.JW_OA_CarrierAddress = carrier1.MainAddress.PK;
				transport1.JW_RL_NKLoadPort = "AUSYD";
				transport1.JW_RL_NKDiscPort = "JPTYO";

				var transport2 = consol.Transports.AddNew();
				transport2.JW_TransportMode = "RAI";
				transport2.JW_OA_CarrierAddress = carrier2.MainAddress.PK;
				transport2.JW_RL_NKLoadPort = "JPTYO";
				transport2.JW_RL_NKDiscPort = "USLAX";

				var strategy = new ForwardingContainerDefaultingStrategy(container);

				var matchResult1 = strategy.GetMatchedStoragePenalty("IMP", "CAR", Moq.It.IsAny<ZString>());
				AssertNotNull(matchResult1);
				AssertEquals(new ZByte(77), matchResult1.FreeDays);

				var matchResult2 = strategy.GetMatchedStoragePenalty("EXP", "CAR", Moq.It.IsAny<ZString>());
				AssertNotNull(matchResult2);
				AssertEquals(new ZByte(66), matchResult2.FreeDays);
			}
		}

		public void TestCTOForDetentionCalculation()
		{
			var cto1 = Factory.NewWithValidTestData<OrgHeader>();
			var cto2 = Factory.NewWithValidTestData<OrgHeader>();

			var importCTOStorage1 = cto1.ServiceImportCTOStorages.AddNew();
			importCTOStorage1.PD_FreeDays = 55;

			var exportCTOStorage1 = cto1.ServiceExportCTOStorages.AddNew();
			exportCTOStorage1.PD_FreeDays = 66;

			var importCTOStorage2 = cto2.ServiceImportCTOStorages.AddNew();
			importCTOStorage2.PD_FreeDays = 77;

			var exportCTOStorage2 = cto2.ServiceExportCTOStorages.AddNew();
			exportCTOStorage2.PD_FreeDays = 88;

			Factory.Save();
			{
				var consol = Factory.New<ForwardingConsol>();
				var container = consol.Containers.AddNew();

				consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
				consol.JK_OA_DepartureCTOAddress = cto1.MainAddress.PK;
				consol.JK_OA_ArrivalCTOAddress = cto2.MainAddress.PK;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";

				var strategy = new ForwardingContainerDefaultingStrategy(container);

				var matchResult = strategy.GetMatchedStoragePenalty("IMP", "CTO", Moq.It.IsAny<ZString>());
				AssertNotNull(matchResult);
				AssertEquals(new ZByte(77), matchResult.FreeDays);

				matchResult = strategy.GetMatchedStoragePenalty("EXP", "CTO", Moq.It.IsAny<ZString>());
				AssertNotNull(matchResult);
				AssertEquals(new ZByte(66), matchResult.FreeDays);
			}
		}

		public void TestClientForDetentionCalculation()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var importCTOStorage1 = consignee.ConsigneeCTOStorages.AddNew();
			importCTOStorage1.PD_FreeDays = 55;
			importCTOStorage1.PD_CreditorType = "CTO";
			importCTOStorage1.PD_OH_Carrier = carrier.PK;

			var exportCTOStorage1 = consignor.ConsignorCTOStorages.AddNew();
			exportCTOStorage1.PD_FreeDays = 77;
			exportCTOStorage1.PD_CreditorType = "CTO";
			exportCTOStorage1.PD_OH_Carrier = carrier.PK;

			Factory.Save();
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.SetShippingLine(carrier.MainAddress.PK, ZString.Empty);

				var shipment = consol.Shipments.AddNew();
				var container = consol.Containers.AddNew();

				shipment.ConsigneePK = consignee.PK;
				shipment.ConsignorPK = consignor.PK;
				consol.JK_ConsolMode = ContainerModes.FCL;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";

				var strategy = new ForwardingContainerDefaultingStrategy(container);

				var matchResult = strategy.GetMatchedStoragePenalty("IMP", "CTO", Moq.It.IsAny<ZString>());
				AssertNotNull(matchResult);
				AssertEquals(new ZByte(55), matchResult.FreeDays);

				matchResult = strategy.GetMatchedStoragePenalty("EXP", "CTO", Moq.It.IsAny<ZString>());
				AssertNotNull(matchResult);
				AssertEquals(new ZByte(77), matchResult.FreeDays);
			}
		}

		public void TestClientForShipmentDetentionCalculation()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();

			var importCTOStorage1 = consignee.ConsigneeCTOStorages.AddNew();
			importCTOStorage1.PD_FreeDays = 55;
			importCTOStorage1.PD_CreditorType = "CTO";
			importCTOStorage1.PD_OH_Carrier = carrier.PK;

			var exportCTOStorage1 = consignor.ConsignorCTOStorages.AddNew();
			exportCTOStorage1.PD_FreeDays = 77;
			exportCTOStorage1.PD_CreditorType = "CTO";
			exportCTOStorage1.PD_OH_Carrier = carrier.PK;

			var importClientCTOStorage1 = localClient.ConsigneeCTOStorages.AddNew();
			importClientCTOStorage1.PD_FreeDays = 88;
			importClientCTOStorage1.PD_CreditorType = "CTO";
			importClientCTOStorage1.PD_OH_Carrier = carrier.PK;

			var exportClientCTOStorage1 = localClient.ConsignorCTOStorages.AddNew();
			exportClientCTOStorage1.PD_FreeDays = 99;
			exportClientCTOStorage1.PD_CreditorType = "CTO";
			exportClientCTOStorage1.PD_OH_Carrier = carrier.PK;

			var importClientCTOStorage2 = controllingCustomer.ConsigneeCTOStorages.AddNew();
			importClientCTOStorage2.PD_FreeDays = 101;
			importClientCTOStorage2.PD_CreditorType = "CTO";
			importClientCTOStorage2.PD_OH_Carrier = carrier.PK;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.SetShippingLine(carrier.MainAddress.PK, ZString.Empty);

			var shipment = consol.Shipments.AddNew();
			shipment.CreateShipmentJobHeaderWithMutex();
			Factory.Save();

			shipment.ShipmentJobHeader.LocalChargesPK = localClient.PK;

			var container = consol.Containers.AddNew();

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;

			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var strategy = new ForwardingContainerDefaultingStrategy(container);

			var matchResult = strategy.GetMatchedStoragePenalty("IMP", "CTO", ContainerPenaltyProcessType.Delivery);
			AssertEquals("Free Days should default from Controlling Customer consignee configuration", new ZByte(101), matchResult.FreeDays);

			shipment.ControllingCustomerAddress.E2_OA_Address = ZGuid.Empty;

			matchResult = strategy.GetMatchedStoragePenalty("IMP", "CTO", ContainerPenaltyProcessType.Delivery);
			AssertEquals("Free Days should default from Local Client consignee configuration", new ZByte(88), matchResult.FreeDays);

			matchResult = strategy.GetMatchedStoragePenalty("EXP", "CTO", ContainerPenaltyProcessType.Pickup);
			AssertEquals("Free Days should default from Local Client consignor configuration", new ZByte(99), matchResult.FreeDays);

			consol.JK_ConsolMode = ContainerModes.LCL;
			matchResult = strategy.GetMatchedStoragePenalty("IMP", "CTO", ContainerPenaltyProcessType.Delivery);
			AssertEquals("Free Days should default from Local Client consignor configuration", new ZByte(88), matchResult.FreeDays);

			consol.JK_ConsolMode = ContainerModes.BuyersConsol;
			matchResult = strategy.GetMatchedStoragePenalty("IMP", "CTO", ContainerPenaltyProcessType.Delivery);
			AssertEquals("Free Days should default from Local Client consignor configuration", new ZByte(88), matchResult.FreeDays);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			AssertEquals("Precondition", false, shipment.SupportsDeliveryPenalties);
			AssertEquals("Precondition", false, shipment.SupportsPickupPenalties);
			AssertEquals("Free Days should not default because it does not support penalty calculations", null, strategy.GetMatchedStoragePenalty("IMP", "CTO", ContainerPenaltyProcessType.Delivery, shipment));
			AssertEquals("Free Days should not default because it does not support penalty calculations", null, strategy.GetMatchedStoragePenalty("EXP", "CTO", ContainerPenaltyProcessType.Pickup, shipment));

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			AssertEquals("Precondition", true, shipment.SupportsDeliveryPenalties);
			AssertEquals("Precondition", false, shipment.SupportsPickupPenalties);
			AssertEquals("Free Days should default from Local Client consignee configuration", new ZByte(88), strategy.GetMatchedStoragePenalty("IMP", "CTO", ContainerPenaltyProcessType.Delivery, shipment)?.FreeDays);
			AssertEquals("Free Days should not default because it does not support penalty calculations", null, strategy.GetMatchedStoragePenalty("EXP", "CTO", ContainerPenaltyProcessType.Pickup, shipment)?.FreeDays);

			consol.JK_ConsolMode = ContainerModes.FCL;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			shipment.ShipmentJobHeader.LocalChargesPK = ZGuid.Empty;

			matchResult = strategy.GetMatchedStoragePenalty("IMP", "CTO", ContainerPenaltyProcessType.Delivery, shipment);
			AssertNotNull(matchResult);
			AssertEquals("Free Days should default from Consignee when LocalClient is null", new ZByte(55), matchResult.FreeDays);

			matchResult = strategy.GetMatchedStoragePenalty("EXP", "CTO", ContainerPenaltyProcessType.Pickup, shipment);
			AssertNotNull(matchResult);
			AssertEquals("Free Days should default from Consignor when LocalClient is null", new ZByte(77), matchResult.FreeDays);

			shipment.ShipmentJobHeader.LocalChargesPK = localClient.PK;
			localClient.ConsigneeCTOStorages.DeleteAll();
			localClient.ConsignorCTOStorages.DeleteAll();
			Factory.Save();

			matchResult = strategy.GetMatchedStoragePenalty("IMP", "CTO", ContainerPenaltyProcessType.Delivery, shipment);
			AssertNotNull("Free Days should default from Consignee when LocalClient do not have penalties", matchResult);
			AssertEquals("Free Days should default from Consignee when LocalClient do not have penalties", new ZByte(55), matchResult?.FreeDays);

			matchResult = strategy.GetMatchedStoragePenalty("EXP", "CTO", ContainerPenaltyProcessType.Pickup, shipment);
			AssertNotNull("Free Days should default from Consignor when LocalClient do not have penalties", matchResult);
			AssertEquals("Free Days should default from Consignor when LocalClient do not have penalties", new ZByte(77), matchResult?.FreeDays);
		}

		public void TestCalculateStorageStartAvailableDate()
		{
			var today = new ZDateTime(2020, 1, 1);
			Consol.JK_TransportMode = TransportModes.Sea;
			var transport = consol.Transports[0];
			transport.JW_ATA = today.AddDays(10);
			transport.JW_VoyageFlight = "ZZ1234";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_RL_NKLoadPort = "NZAKL";

			var container = consol.Containers.AddNew();
			container.JC_FCLAvailable = today;
			container.JC_FCLWharfGateOut = today.AddDays(3);
			container.JC_FCLUnloadFromVessel = today.AddDays(8);
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Consol.JK_OA_ShippingLineAddress = organization.MainAddress.PK;

			var detention1 = organization.CarrierContainerPenalties.AddNew();
			detention1.PD_Direction = ContainerDetentionDirection.Import;
			detention1.PD_PenaltyType = ContainerDetentionPenaltyType.STO;
			detention1.PD_FreeDays = 3;
			detention1.PD_OH_Carrier = organization.PK;
			detention1.PD_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;

			var strategy = new ForwardingContainerDefaultingStrategy(container);
			detention1.PD_FreeDayType = ContainerDetentionFreeDayType.CTOAvailable;
			AssertEquals(today, strategy.CalculateStorageStartAvailableDate(ContainerDetentionDirection.Import, ContainerPenaltyCreditorType.Codes.CTO));

			detention1.PD_FreeDayType = ContainerDetentionFreeDayType.CTOGateOut;
			AssertEquals(today.AddDays(3), strategy.CalculateStorageStartAvailableDate(ContainerDetentionDirection.Import, ContainerPenaltyCreditorType.Codes.CTO));

			detention1.PD_FreeDayType = ContainerDetentionFreeDayType.DayAfterFCLUnload;
			AssertEquals(today.AddDays(9), strategy.CalculateStorageStartAvailableDate(ContainerDetentionDirection.Import, ContainerPenaltyCreditorType.Codes.CTO));

			detention1.PD_FreeDayType = ContainerDetentionFreeDayType.FCLUnload;
			AssertEquals(today.AddDays(8), strategy.CalculateStorageStartAvailableDate(ContainerDetentionDirection.Import, ContainerPenaltyCreditorType.Codes.CTO));

			detention1.PD_FreeDayType = ContainerDetentionFreeDayType.VesselArrival;
			AssertEquals(today.AddDays(10), strategy.CalculateStorageStartAvailableDate(ContainerDetentionDirection.Import, ContainerPenaltyCreditorType.Codes.CTO));

			detention1.Delete();

			var detention2 = organization.CarrierContainerPenalties.AddNew();
			detention2.PD_Direction = ContainerDetentionDirection.Import;
			detention2.PD_PenaltyType = ContainerDetentionPenaltyType.STO;
			detention2.PD_FreeDays = 6;
			detention2.PD_FreeDayType = ContainerDetentionFreeDayType.FCLUnload;
			detention2.PD_OH_Carrier = organization.PK;
			detention2.PD_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
			AssertEquals((today.AddDays(14), today.AddDays(8), (ZString)"FCL Unload"), strategy.CalculateStorageStart(ContainerDetentionDirection.Import));

			detention2.Delete();

			var detention3 = organization.ServiceImportCTOStorages.AddNew();
			detention3.PD_Direction = ContainerDetentionDirection.Import;
			detention3.PD_PenaltyType = ContainerDetentionPenaltyType.STO;
			detention3.PD_FreeDays = 4;
			detention3.PD_FreeDayType = ContainerDetentionFreeDayType.DayAfterFCLUnload;
			detention3.PD_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;

			Consol.JK_OA_ArrivalCTOAddress = organization.MainAddress.PK;
			AssertEquals((today.AddDays(13), today.AddDays(9), (ZString)"Day after FCL Unload"), strategy.CalculateStorageStart(ContainerDetentionDirection.Import));
		}

		public void TestCalculateStorageStart_RegistryFallbackIsLast()
		{
			var today = new ZDateTime(2020, 1, 1);
			Consol.JK_TransportMode = TransportModes.Sea;
			var transport = consol.Transports[0];
			transport.JW_ATA = today.AddDays(10);
			transport.JW_VoyageFlight = "ZZ1234";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_RL_NKLoadPort = "NZAKL";

			var container = consol.Containers.AddNew();
			container.JC_OverrideFCLAvailableStorage = false;
			container.JC_FCLAvailable = today;
			container.JC_FCLWharfGateOut = today.AddDays(3);
			container.JC_FCLUnloadFromVessel = today.AddDays(8);
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			Consol.JK_OA_ShippingLineAddress = organization.MainAddress.PK;

			var arrivalTransport = new TransportOrderHelper(container.ContainerParent.Transports).LastLeg;
			arrivalTransport.JW_TerminalAvailabilityDate = today;

			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10, UnlimitedFreeDays = false }))
			{
				var strategy = new ForwardingContainerDefaultingStrategy(container);

				SetStorageStartFallbacks(container);
				container.CalculateJC_ArrivalCTOStorageStartDate();
				AssertEquals("Uses the registry", today.AddDays(10), container.JC_ArrivalCTOStorageStartDate);

				SetStorageStartFallbacks(container, arrivalTransportTerminalStorageDate: today.AddDays(20));
				AssertEquals("pre: Uses the arrival transport", today.AddDays(20), container.JC_ArrivalCTOStorageStartDate);
				container.CalculateJC_ArrivalCTOStorageStartDate();
				AssertEquals("Uses the arrival transport", today.AddDays(20), container.JC_ArrivalCTOStorageStartDate);

				SetStorageStartFallbacks(container, arrivalTransportTerminalStorageDate: today.AddDays(20), field: today.AddDays(30));
				AssertEquals("pre: Uses the field transport", today.AddDays(30), container.JC_ArrivalCTOStorageStartDate);
				container.CalculateJC_ArrivalCTOStorageStartDate();
				AssertEquals("Uses the field transport", today.AddDays(30), container.JC_ArrivalCTOStorageStartDate);
			}
		}

		void SetStorageStartFallbacks(ForwardingContainer container, ZDateTime? field = null, ZDateTime? arrivalTransportTerminalStorageDate = null)
		{
			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Empty;
			container.JC_OverrideFCLAvailableStorage = false;

			if (field.HasValue)
			{
				container.JC_ArrivalCTOStorageStartDate = field.Value;
				container.JC_OverrideFCLAvailableStorage = true;
			}

			var arrivalTransport = new TransportOrderHelper(container.ContainerParent.Transports).LastLeg;
			arrivalTransport.JW_TerminalStorageDate = arrivalTransportTerminalStorageDate ?? ZDateTime.Empty;
		}

		public void TestCalculateDetentionFreeDays()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForExport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 9 }))
			{
				var organization = Factory.NewWithValidTestData<OrgHeader>();
				var detention = organization.CarrierContainerPenalties.AddNew();
				detention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				detention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detention.PD_OriginPortOrCountry = "DEHAM";
				detention.PD_DetentionPortOrCountry = "DEHAM";
				detention.PD_FreeDays = 2;
				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "DEHAM";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_OA_ShippingLineAddress = organization.MainAddress.PK;

				var container = consol.Containers.AddNew();
				var strategy = new ForwardingContainerDefaultingStrategy(container);
				AssertEquals((ZByte)2, strategy.GetMatchedDetentionPenalty("DEHAM", ContainerDetentionDirection.Export, Moq.It.IsAny<ZString>(), null)?.FreeDays);
			}

			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 9 }))
			{
				var organization = Factory.NewWithValidTestData<OrgHeader>();
				var detention = organization.CarrierContainerPenalties.AddNew();
				detention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				detention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detention.PD_OriginPortOrCountry = "AUSYD";
				detention.PD_DetentionPortOrCountry = "DEBRE";
				detention.PD_FreeDays = 8;
				detention.PD_FreeDayType = "CTD";
				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "DEBRE";
				consol.JK_OA_ShippingLineAddress = organization.MainAddress.PK;

				var container = consol.Containers.AddNew();
				var strategy = new ForwardingContainerDefaultingStrategy(container);
				AssertEquals((ZByte)8, strategy.GetMatchedDetentionPenalty("DEBRE", ContainerDetentionDirection.Import, Moq.It.IsAny<ZString>(), null)?.FreeDays);
			}
		}

		public void TestCalculateMDDFreeDaysForPickUpPenaltyDefaulting()
		{
			AssertFreeDaysForPickupPenaltyDefaulted_BasedOnControllingCustomerConsignorPenaltiesConfig(Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
			AssertFreeDaysForPickupPenaltyDefaulted_BasedOnLocalClientConsignorPenaltiesConfig(Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
			AssertFreeDaysForPickupPenaltyDefaulted_BasedOnConsignorPenaltyConfig_WhenShipmentDoesNotHaveLocalClient(Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
			AssertFreeDaysForPickUpPenaltyDefaulted_BasedOnConsignorPenaltyConfig_WhenShipmentHasLocalClientButPenaltyTypeDoesNotMatch(
				Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
			AssertFreeDaysForPickupPenaltyDefaulted_BasedOnCarrierPenaltiesConfig_ClientIsPrioritized(Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
		}

		public void TestCalculateMDDFreeDaysForDeliveryPenaltyDefaulting()
		{
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnControllingCustomerConsigneePenaltiesConfig(Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnLocalClientConsigeePenaltiesConfig(Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnConsigneePenaltyConfig_WhenShipmentDoesNotHaveLocalClient(Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnConsigneePenaltyConfig_WhenShipmentHasLocalClientButPenaltyTypeDoesNotMatch(
				Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnCarrierPenaltiesConfig_ClientIsPrioritized(Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
		}

		public void TestCalculateDetentionFreeDaysForPickUpPenaltyDefaulting()
		{
			AssertFreeDaysForPickupPenaltyDefaulted_BasedOnControllingCustomerConsignorPenaltiesConfig(Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
			AssertFreeDaysForPickupPenaltyDefaulted_BasedOnLocalClientConsignorPenaltiesConfig(Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
			AssertFreeDaysForPickupPenaltyDefaulted_BasedOnConsignorPenaltyConfig_WhenShipmentDoesNotHaveLocalClient(Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
			AssertFreeDaysForPickUpPenaltyDefaulted_BasedOnConsignorPenaltyConfig_WhenShipmentHasLocalClientButPenaltyTypeDoesNotMatch(
				Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention, Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
			AssertFreeDaysForPickupPenaltyDefaulted_BasedOnCarrierPenaltiesConfig_ClientIsPrioritized(Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
		}

		public void TestCalculateDetentionFreeDaysForDeliveryPenaltyDefaulting()
		{
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnControllingCustomerConsigneePenaltiesConfig(Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnLocalClientConsigeePenaltiesConfig(Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnConsigneePenaltyConfig_WhenShipmentDoesNotHaveLocalClient(Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnConsigneePenaltyConfig_WhenShipmentHasLocalClientButPenaltyTypeDoesNotMatch(
				Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention, Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnCarrierPenaltiesConfig_ClientIsPrioritized(Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention);
		}

		public void TestCalculateStorageFreeDaysForPickUpPenaltyDefaulting()
		{
			AssertFreeDaysForPickupPenaltyDefaulted_BasedOnControllingCustomerConsignorPenaltiesConfig(Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage);
			AssertFreeDaysForPickupPenaltyDefaulted_BasedOnLocalClientConsignorPenaltiesConfig(Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage);
			AssertFreeDaysForPickupPenaltyDefaulted_BasedOnConsignorPenaltyConfig_WhenShipmentDoesNotHaveLocalClient(Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage);
			AssertFreeDaysForPickUpPenaltyDefaulted_BasedOnConsignorPenaltyConfig_WhenShipmentHasLocalClientButPenaltyTypeDoesNotMatch(
				Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage, Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
			AssertFreeDaysForPickupPenaltyDefaulted_BasedOnCarrierPenaltiesConfig_ClientIsPrioritized(Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage);
		}

		public void TestCalculateStorageFreeDaysForDeliveryPenaltyDefaulting()
		{
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnControllingCustomerConsigneePenaltiesConfig(Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage);
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnLocalClientConsigeePenaltiesConfig(Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage);
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnConsigneePenaltyConfig_WhenShipmentDoesNotHaveLocalClient(Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage);
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnConsigneePenaltyConfig_WhenShipmentHasLocalClientButPenaltyTypeDoesNotMatch(
				Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage, Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
			AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnCarrierPenaltiesConfig_ClientIsPrioritized(Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage);
		}

		void AssertFreeDaysForPickupPenaltyDefaulted_BasedOnControllingCustomerConsignorPenaltiesConfig(ZString penaltyType)
		{
			var result = CreateShipmentAndContainer();
			try
			{
				var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
				controllingCustomer.OH_IsControllingCustomer = true;
				result.Shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;

				result.Shipment.CreateShipmentJobHeaderWithMutex();
				var localClient = Factory.NewWithValidTestData<OrgHeader>();
				localClient.OH_IsConsignor = true;

				result.Shipment.JobHeader.LocalChargesPK = localClient.PK;

				var controllingCustomerPenalty = result.Shipment.ControllingCustomer.ConsignorContainerPenalties.AddNew();
				controllingCustomerPenalty.PD_FreeDays = 5;
				controllingCustomerPenalty.PD_PenaltyType = penaltyType;
				controllingCustomerPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				controllingCustomerPenalty.PD_Direction = ContainerDetentionDirection.Export;

				var localClientPenalty = result.Shipment.JobHeader.LocalCharges.ConsignorContainerPenalties.AddNew();
				localClientPenalty.PD_FreeDays = 6;
				localClientPenalty.PD_PenaltyType = penaltyType;
				localClientPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				localClientPenalty.PD_Direction = ContainerDetentionDirection.Export;

				var penalty = result.Shipment.PickupPenalties.AddNew();
				penalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				penalty.CPY_JC_Container = result.Container.PK;
				penalty.CPY_PenaltyType = penaltyType;

				AssertEquals(5, (int)penalty.FreeTimeAsDays);
			}
			finally
			{
				result.Shipment.Job.Dispose();
			}
		}

		void AssertFreeDaysForPickupPenaltyDefaulted_BasedOnLocalClientConsignorPenaltiesConfig(ZString penaltyType)
		{
			var result = CreateShipmentAndContainer();
			try
			{
				result.Shipment.CreateShipmentJobHeaderWithMutex();
				var localClient = Factory.NewWithValidTestData<OrgHeader>();
				localClient.OH_IsConsignor = true;

				result.Shipment.JobHeader.LocalChargesPK = localClient.PK;

				var orgPenalty = result.Shipment.JobHeader.LocalCharges.ConsignorContainerPenalties.AddNew();
				orgPenalty.PD_FreeDays = 6;
				orgPenalty.PD_PenaltyType = penaltyType;
				orgPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				orgPenalty.PD_Direction = ContainerDetentionDirection.Export;

				var penalty = result.Shipment.PickupPenalties.AddNew();
				penalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				penalty.CPY_JC_Container = result.Container.PK;
				penalty.CPY_PenaltyType = penaltyType;

				AssertEquals(6, (int)penalty.FreeTimeAsDays);
			}
			finally
			{
				result.Shipment.Job.Dispose();
			}
		}

		void AssertFreeDaysForPickupPenaltyDefaulted_BasedOnConsignorPenaltyConfig_WhenShipmentDoesNotHaveLocalClient(ZString penaltyType)
		{
			var result = CreateShipmentAndContainer();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			result.Shipment.ConsignorPK = consignor.PK;

			var orgPenalty = result.Shipment.Consignor.ConsigneeContainerPenalties.AddNew();
			orgPenalty.PD_FreeDays = 6;
			orgPenalty.PD_PenaltyType = penaltyType;
			orgPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			orgPenalty.PD_Direction = ContainerDetentionDirection.Export;

			var penalty = result.Shipment.PickupPenalties.AddNew();
			penalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			penalty.CPY_JC_Container = result.Container.PK;
			penalty.CPY_PenaltyType = penaltyType;

			AssertEquals(6, (int)penalty.FreeTimeAsDays);
		}

		public void AssertFreeDaysForPickUpPenaltyDefaulted_BasedOnConsignorPenaltyConfig_WhenShipmentHasLocalClientButPenaltyTypeDoesNotMatch(
						ZString penaltyType, ZString unmatchedPenaltyType)
		{
			var result = CreateShipmentAndContainer();
			try
			{
				result.Shipment.CreateShipmentJobHeaderWithMutex();
				var localClient = Factory.NewWithValidTestData<OrgHeader>();
				localClient.OH_IsConsignor = true;

				result.Shipment.JobHeader.LocalChargesPK = localClient.PK;

				var localClientPenalty = result.Shipment.JobHeader.LocalCharges.ConsignorContainerPenalties.AddNew();
				localClientPenalty.PD_FreeDays = 12;
				localClientPenalty.PD_PenaltyType = unmatchedPenaltyType;
				localClientPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				localClientPenalty.PD_Direction = ContainerDetentionDirection.Export;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				result.Shipment.ConsignorPK = consignor.PK;

				var consignorPenalty = result.Shipment.Consignor.ConsignorContainerPenalties.AddNew();
				consignorPenalty.PD_FreeDays = 8;
				consignorPenalty.PD_PenaltyType = penaltyType;
				consignorPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				consignorPenalty.PD_Direction = ContainerDetentionDirection.Export;

				var penalty = result.Shipment.PickupPenalties.AddNew();
				penalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				penalty.CPY_JC_Container = result.Container.PK;
				penalty.CPY_PenaltyType = penaltyType;

				AssertEquals(8, (int)penalty.FreeTimeAsDays);
			}
			finally
			{
				result.Shipment.Job.Dispose();
			}
		}

		void AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnControllingCustomerConsigneePenaltiesConfig(ZString penaltyType)
		{
			var result = CreateShipmentAndContainer();
			try
			{
				var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
				controllingCustomer.OH_IsControllingCustomer = true;
				result.Shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;

				result.Shipment.CreateShipmentJobHeaderWithMutex();
				var localClient = Factory.NewWithValidTestData<OrgHeader>();
				localClient.OH_IsConsignor = true;

				result.Shipment.JobHeader.LocalChargesPK = localClient.PK;

				var controllingCustomerPenalty = result.Shipment.ControllingCustomer.ConsignorContainerPenalties.AddNew();
				controllingCustomerPenalty.PD_FreeDays = 5;
				controllingCustomerPenalty.PD_PenaltyType = penaltyType;
				controllingCustomerPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				controllingCustomerPenalty.PD_Direction = ContainerDetentionDirection.Import;

				var localClientPenalty = result.Shipment.JobHeader.LocalCharges.ConsignorContainerPenalties.AddNew();
				localClientPenalty.PD_FreeDays = 6;
				localClientPenalty.PD_PenaltyType = penaltyType;
				localClientPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				localClientPenalty.PD_Direction = ContainerDetentionDirection.Import;

				var penalty = result.Shipment.DeliveryPenalties.AddNew();
				penalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				penalty.CPY_JC_Container = result.Container.PK;
				penalty.CPY_PenaltyType = penaltyType;

				AssertEquals(5, (int)penalty.FreeTimeAsDays);
			}
			finally
			{
				result.Shipment.Job.Dispose();
			}
		}

		void AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnLocalClientConsigeePenaltiesConfig(ZString penaltyType)
		{
			var result = CreateShipmentAndContainer();
			try
			{
				result.Shipment.CreateShipmentJobHeaderWithMutex();
				var localClient = Factory.NewWithValidTestData<OrgHeader>();
				localClient.OH_IsConsignee = true;

				result.Shipment.JobHeader.LocalChargesPK = localClient.PK;

				var orgPenalty = result.Shipment.JobHeader.LocalCharges.ConsigneeContainerPenalties.AddNew();
				orgPenalty.PD_FreeDays = 6;
				orgPenalty.PD_PenaltyType = penaltyType;
				orgPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				orgPenalty.PD_Direction = ContainerDetentionDirection.Import;

				var penalty = result.Shipment.DeliveryPenalties.AddNew();
				penalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				penalty.CPY_JC_Container = result.Container.PK;
				penalty.CPY_PenaltyType = penaltyType;

				AssertEquals(6, (int)penalty.FreeTimeAsDays);
			}
			finally
			{
				result.Shipment.Job.Dispose();
			}
		}

		void AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnConsigneePenaltyConfig_WhenShipmentDoesNotHaveLocalClient(ZString penaltyType)
		{
			var result = CreateShipmentAndContainer();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			result.Shipment.ConsigneePK = consignee.PK;

			var orgPenalty = result.Shipment.Consignee.ConsigneeContainerPenalties.AddNew();
			orgPenalty.PD_FreeDays = 6;
			orgPenalty.PD_PenaltyType = penaltyType;
			orgPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			orgPenalty.PD_Direction = ContainerDetentionDirection.Import;

			var penalty = result.Shipment.DeliveryPenalties.AddNew();
			penalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			penalty.CPY_JC_Container = result.Container.PK;
			penalty.CPY_PenaltyType = penaltyType;

			AssertEquals(6, (int)penalty.FreeTimeAsDays);
		}

		void AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnConsigneePenaltyConfig_WhenShipmentHasLocalClientButPenaltyTypeDoesNotMatch(
						ZString penaltyType, ZString unmatchedPenaltyType)
		{
			var result = CreateShipmentAndContainer();
			try
			{
				result.Shipment.CreateShipmentJobHeaderWithMutex();
				var localClient = Factory.NewWithValidTestData<OrgHeader>();
				localClient.OH_IsConsignee = true;

				result.Shipment.JobHeader.LocalChargesPK = localClient.PK;

				var localClientPenalty = result.Shipment.JobHeader.LocalCharges.ConsigneeContainerPenalties.AddNew();
				localClientPenalty.PD_FreeDays = 12;
				localClientPenalty.PD_PenaltyType = unmatchedPenaltyType;
				localClientPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				localClientPenalty.PD_Direction = ContainerDetentionDirection.Import;

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				result.Shipment.ConsigneePK = consignee.PK;

				var consigneePenalty = result.Shipment.Consignee.ConsigneeContainerPenalties.AddNew();
				consigneePenalty.PD_FreeDays = 8;
				consigneePenalty.PD_PenaltyType = penaltyType;
				consigneePenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				consigneePenalty.PD_Direction = ContainerDetentionDirection.Import;

				var penalty = result.Shipment.DeliveryPenalties.AddNew();
				penalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				penalty.CPY_JC_Container = result.Container.PK;
				penalty.CPY_PenaltyType = penaltyType;

				AssertEquals(8, (int)penalty.FreeTimeAsDays);
			}
			finally
			{
				result.Shipment.Job.Dispose();
			}
		}

		void AssertFreeDaysForPickupPenaltyDefaulted_BasedOnCarrierPenaltiesConfig_ClientIsPrioritized(ZString penaltyType)
		{
			var result = CreateShipmentAndContainer();
			try
			{
				var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
				controllingCustomer.OH_IsControllingCustomer = true;
				result.Shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;

				result.Shipment.CreateShipmentJobHeaderWithMutex();
				var localClient = Factory.NewWithValidTestData<OrgHeader>();
				localClient.OH_IsConsignor = true;
				result.Shipment.JobHeader.LocalChargesPK = localClient.PK;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				result.Shipment.ConsignorPK = consignor.PK;

				var carrierPenalty = result.Shipment.Consols[0].ShippingLine.CarrierContainerPenalties.AddNew();
				carrierPenalty.PD_FreeDays = 5;
				carrierPenalty.PD_PenaltyType = penaltyType;
				carrierPenalty.PD_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				carrierPenalty.PD_Direction = ContainerDetentionDirection.Export;

				var controllingCustomerPenalty = result.Shipment.Consols[0].ShippingLine.CarrierContainerPenalties.AddNew();
				controllingCustomerPenalty.PD_OH_Client = result.Shipment.ControllingCustomer.PK;
				controllingCustomerPenalty.PD_FreeDays = 6;
				controllingCustomerPenalty.PD_PenaltyType = penaltyType;
				controllingCustomerPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				controllingCustomerPenalty.PD_Direction = ContainerDetentionDirection.Export;

				var localClientPenalty = result.Shipment.Consols[0].ShippingLine.CarrierContainerPenalties.AddNew();
				localClientPenalty.PD_OH_Client = result.Shipment.JobHeader.LocalCharges.PK;
				localClientPenalty.PD_FreeDays = 7;
				localClientPenalty.PD_PenaltyType = penaltyType;
				localClientPenalty.PD_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				localClientPenalty.PD_Direction = ContainerDetentionDirection.Export;

				var consignorPenalty = result.Shipment.Consols[0].ShippingLine.CarrierContainerPenalties.AddNew();
				consignorPenalty.PD_OH_Client = result.Shipment.Consignor.PK;
				consignorPenalty.PD_FreeDays = 8;
				consignorPenalty.PD_PenaltyType = penaltyType;
				consignorPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				consignorPenalty.PD_Direction = ContainerDetentionDirection.Export;

				var penalty = result.Shipment.PickupPenalties.AddNew();
				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				penalty.CPY_JC_Container = result.Container.PK;
				penalty.CPY_PenaltyType = penaltyType;
				AssertEquals(6, (int)penalty.FreeTimeAsDays);

				result.Shipment.ControllingCustomerAddress.E2_OA_Address = ZGuid.Empty;
				penalty.CalculateDefaultFreeTimeAsDays();
				AssertEquals(7, (int)penalty.FreeTimeAsDays);

				result.Shipment.JobHeader.LocalChargesPK = ZGuid.Empty;
				penalty.CalculateDefaultFreeTimeAsDays();
				AssertEquals(8, (int)penalty.FreeTimeAsDays);
			}
			finally
			{
				result.Shipment.Job.Dispose();
			}
		}

		void AssertFreeDaysForDeliveryPenaltyDefaulted_BasedOnCarrierPenaltiesConfig_ClientIsPrioritized(ZString penaltyType)
		{
			var result = CreateShipmentAndContainer();
			try
			{
				var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
				controllingCustomer.OH_IsControllingCustomer = true;
				result.Shipment.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;

				result.Shipment.CreateShipmentJobHeaderWithMutex();
				var localClient = Factory.NewWithValidTestData<OrgHeader>();
				localClient.OH_IsConsignee = true;
				result.Shipment.JobHeader.LocalChargesPK = localClient.PK;

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				result.Shipment.ConsigneePK = consignee.PK;

				var carrierPenalty = result.Shipment.Consols[0].ShippingLine.CarrierContainerPenalties.AddNew();
				carrierPenalty.PD_FreeDays = 5;
				carrierPenalty.PD_PenaltyType = penaltyType;
				carrierPenalty.PD_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				carrierPenalty.PD_Direction = ContainerDetentionDirection.Import;

				var controllingCustomerPenalty = result.Shipment.Consols[0].ShippingLine.CarrierContainerPenalties.AddNew();
				controllingCustomerPenalty.PD_OH_Client = result.Shipment.ControllingCustomer.PK;
				controllingCustomerPenalty.PD_FreeDays = 6;
				controllingCustomerPenalty.PD_PenaltyType = penaltyType;
				controllingCustomerPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				controllingCustomerPenalty.PD_Direction = ContainerDetentionDirection.Import;

				var localClientPenalty = result.Shipment.Consols[0].ShippingLine.CarrierContainerPenalties.AddNew();
				localClientPenalty.PD_OH_Client = result.Shipment.JobHeader.LocalCharges.PK;
				localClientPenalty.PD_FreeDays = 7;
				localClientPenalty.PD_PenaltyType = penaltyType;
				localClientPenalty.PD_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				localClientPenalty.PD_Direction = ContainerDetentionDirection.Import;

				var consigneePenalty = result.Shipment.Consols[0].ShippingLine.CarrierContainerPenalties.AddNew();
				consigneePenalty.PD_OH_Client = result.Shipment.Consignee.PK;
				consigneePenalty.PD_FreeDays = 8;
				consigneePenalty.PD_PenaltyType = penaltyType;
				consigneePenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				consigneePenalty.PD_Direction = ContainerDetentionDirection.Import;

				var penalty = result.Shipment.DeliveryPenalties.AddNew();
				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				penalty.CPY_JC_Container = result.Container.PK;
				penalty.CPY_PenaltyType = penaltyType;
				AssertEquals(6, (int)penalty.FreeTimeAsDays);

				result.Shipment.ControllingCustomerAddress.E2_OA_Address = ZGuid.Empty;
				penalty.CalculateDefaultFreeTimeAsDays();
				AssertEquals(7, (int)penalty.FreeTimeAsDays);

				result.Shipment.JobHeader.LocalChargesPK = ZGuid.Empty;
				penalty.CalculateDefaultFreeTimeAsDays();
				AssertEquals(8, (int)penalty.FreeTimeAsDays);
			}
			finally
			{
				result.Shipment.Job.Dispose();
			}
		}

		public void TestCalculateMDDFreeDays_BasedOnPenaltyConfig_WhenContainerIsChanged()
		{
			var shpCnt = CreateShipmentAndContainer();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shpCnt.Shipment.ConsignorPK = consignor.PK;

			var orgPenalty = shpCnt.Shipment.Consignor.ConsigneeContainerPenalties.AddNew();
			orgPenalty.PD_FreeDays = 6;
			orgPenalty.PD_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;
			orgPenalty.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			orgPenalty.PD_Direction = ContainerDetentionDirection.Export;

			var penalty = shpCnt.Shipment.PickupPenalties.AddNew();
			penalty.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;
			penalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			penalty.CPY_JC_Container = shpCnt.Container.PK;

			AssertEquals("We should calculate FreeDays and other defaults in CPY_JC_Container.set (When user selects container after PenaltyType in the UI).", 6, (int)penalty.FreeTimeAsDays);
		}

		public void TestCalculateAvailableDateForImportDetentionWhenMultipleTransportLegs()
		{
			var detention = DetentionForCarrierOnly;
			detention.PD_FreeDayType = ContainerDetentionFreeDayType.VesselArrival;
			detention.PD_FreeDays = 7;
			detention.PD_OriginPortOrCountry = ZString.Empty;

			var container = Consol.Containers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;

			Consol.JK_ConsolMode = ContainerModes.FCL;
			Consol.JK_OA_ShippingLineAddress = Carrier.MainAddress.PK;
			Consol.JK_TransportMode = TransportModes.Sea;

			var refVessel1 = Factory.New<RefVessel>();
			refVessel1.RV_Name = "Vessal A";

			var refVessel2 = Factory.New<RefVessel>();
			refVessel2.RV_Name = "Vessal B";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				AssertAvailableDate(refVessel1.RV_Name, refVessel2.RV_Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				AssertAvailableDate(refVessel1.RV_Name, refVessel1.RV_Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				AssertAvailableDate(refVessel1.RV_Name, refVessel2.RV_Name);
			}

			void AssertAvailableDate(ZString transport1Vessel, ZString transport2Vessel)
			{
				var transport2 = Consol.Transports[0];
				transport2.JW_TransportMode = TransportModes.Sea;
				transport2.JW_RL_NKLoadPort = "AUBNE";
				transport2.JW_RL_NKDiscPort = "AUMEL";
				transport2.JW_Vessel = transport2Vessel;
				transport2.JW_ETD = new ZDateTime(2023, 2, 21);
				transport2.JW_ETA = new ZDateTime(2023, 2, 22);
				transport2.JW_ATD = new ZDateTime(2023, 2, 15);
				transport2.JW_ATA = new ZDateTime(2023, 2, 16);
				transport2.JW_LegOrder = 2;

				var transport1 = Consol.Transports.AddNew();
				transport1.JW_TransportMode = TransportModes.Sea;
				transport1.JW_RL_NKLoadPort = "USLAX";
				transport1.JW_RL_NKDiscPort = "AUBNE";
				transport1.JW_Vessel = transport1Vessel;
				transport1.JW_ETD = new ZDateTime(2023, 2, 20);
				transport1.JW_ETA = new ZDateTime(2023, 2, 21);
				transport1.JW_ATD = new ZDateTime(2023, 2, 10);
				transport1.JW_ATA = new ZDateTime(2023, 2, 11);
				transport1.JW_LegOrder = 1;

				var strategy = new ForwardingContainerDefaultingStrategy(container);
				var (availableDate, _) = strategy.CalculateAvailableDateForDetention(ZString.Empty, ContainerDetentionDirection.Import, ContainerPenaltyProcessType.Import);
				AssertEquals(transport2.JW_ATA, availableDate);

				transport2.JW_TransportMode = "AIR";
				(availableDate, _) = strategy.CalculateAvailableDateForDetention(ZString.Empty, ContainerDetentionDirection.Import, ContainerPenaltyProcessType.Import);
				AssertEquals(transport2.JW_ATA, availableDate);
			}
		}

		public void TestCalculateAvailableDateForDetentionWillNotThrowNullReferenceExceptionWhenConsolIsNull()
		{
			var detention = Factory.New<IBaseJobDeclaration>();
			var declarationContainer = Factory.New<IBaseCusContainer>();
			declarationContainer.CO_JE = detention.PK;

			var carrier = Factory.New<OrgHeader>();
			var carrierDetention = carrier.CarrierContainerPenalties.AddNew();
			carrierDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			carrierDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			carrierDetention.PD_FreeDayType = ContainerDetentionFreeDayType.DayBeforeVesselDeparture;
			carrierDetention.PD_FreeDays = 9;

			detention["JE_OH_ShippingLine"] = carrier.PK;

			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.JC_FCLAvailable = new ZDateTime(2020, 1, 1);
			container.JC_FCLWharfGateOut = new ZDateTime(2020, 1, 5);
			container.JC_FCLUnloadFromVessel = new ZDateTime(2020, 1, 10);

			declarationContainer.CO_JC = container.PK;

			var strategy = new ForwardingContainerDefaultingStrategy(container);
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(new ContainerPenaltyDate(ZDateTime.Empty, (ZString)"Day before Vessel Departure"), strategy.CalculateAvailableDateForDetention(carrierDetention.PD_DetentionPortOrCountry, carrierDetention.PD_Direction, ContainerPenaltyProcessType.Export, null));
			});
		}

		public void TestCalculateAvailableDateForExportDetentionWhenMultipleTransportLegs()
		{
			var detention = DetentionForCarrierOnly;
			detention.PD_Direction = ContainerDetentionDirection.Export;
			detention.PD_FreeDayType = ContainerDetentionFreeDayType.VesselDeparture;
			detention.PD_FreeDays = 7;
			detention.PD_OriginPortOrCountry = ZString.Empty;

			var container = Consol.Containers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;

			Consol.JK_ConsolMode = ContainerModes.FCL;
			Consol.JK_OA_ShippingLineAddress = Carrier.MainAddress.PK;
			Consol.JK_TransportMode = TransportModes.Sea;

			var refVessel1 = Factory.New<RefVessel>();
			refVessel1.RV_Name = "Vessal A";

			var refVessel2 = Factory.New<RefVessel>();
			refVessel2.RV_Name = "Vessal B";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				AssertAvailableDate(refVessel1.RV_Name, refVessel2.RV_Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				AssertAvailableDate(refVessel1.RV_Name, refVessel1.RV_Name);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				AssertAvailableDate(refVessel1.RV_Name, refVessel2.RV_Name);
			}

			void AssertAvailableDate(ZString transport1Vessel, ZString transport2Vessel)
			{
				var transport1 = Consol.Transports[0];
				transport1.JW_TransportMode = TransportModes.Sea;
				transport1.JW_RL_NKLoadPort = "AUMEL";
				transport1.JW_RL_NKDiscPort = "AUBNE";
				transport1.JW_Vessel = transport1Vessel;
				transport1.JW_ETD = new ZDateTime(2023, 2, 20);
				transport1.JW_ETA = new ZDateTime(2023, 2, 21);
				transport1.JW_ATD = new ZDateTime(2023, 2, 10);
				transport1.JW_ATA = new ZDateTime(2023, 2, 11);

				var transport2 = Consol.Transports.AddNew();
				transport2.JW_TransportMode = TransportModes.Sea;
				transport2.JW_RL_NKLoadPort = "AUBNE";
				transport2.JW_RL_NKDiscPort = "USLAX";
				transport2.JW_Vessel = transport2Vessel;
				transport2.JW_ETD = new ZDateTime(2023, 2, 21);
				transport2.JW_ETA = new ZDateTime(2023, 2, 22);
				transport2.JW_ATD = new ZDateTime(2023, 2, 15);
				transport2.JW_ATA = new ZDateTime(2023, 2, 16);

				var strategy = new ForwardingContainerDefaultingStrategy(container);
				var (availableDate, _) = strategy.CalculateAvailableDateForDetention(ZString.Empty, ContainerDetentionDirection.Export, ContainerPenaltyProcessType.Export);
				AssertEquals(transport1.JW_ATD, availableDate);

				transport1.JW_TransportMode = "AIR";
				(availableDate, _) = strategy.CalculateAvailableDateForDetention(ZString.Empty, ContainerDetentionDirection.Export, ContainerPenaltyProcessType.Export);
				AssertEquals(transport1.JW_ATD, availableDate);
			}
		}

		public void TestCalculateDetentionFreeDaysWhenMultipleTransportLegs()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForExport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 9 }))
			{
				var organization = Factory.NewWithValidTestData<OrgHeader>();
				var detention = organization.CarrierContainerPenalties.AddNew();
				detention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				detention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detention.PD_OriginPortOrCountry = "DEHAM";
				detention.PD_DetentionPortOrCountry = "DEHAM";
				detention.PD_FreeDays = 2;
				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "DEHAM";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_OA_ShippingLineAddress = organization.MainAddress.PK;

				OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
				var carrierExportDetention = carrier.CarrierContainerPenalties.AddNew();
				carrierExportDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				carrierExportDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				carrierExportDetention.PD_FreeDays = 5;

				var transport1 = consol.Transports[0];
				transport1.JW_RL_NKLoadPort = "DEHAM";
				transport1.JW_RL_NKDiscPort = "AUMEL";
				transport1.CarrierPK = carrier.PK;

				var transport2 = consol.Transports.AddNew();
				transport2.JW_RL_NKLoadPort = "AUMEL";
				transport2.JW_RL_NKDiscPort = "AUSYD";

				var container = consol.Containers.AddNew();
				var strategy = new ForwardingContainerDefaultingStrategy(container);

				AssertEquals("Consol Transports Count", 2, consol.Transports.Count);
				AssertNotNull("Departure Transport Carrier", consol.Transports.DepartureTransport.Carrier);
				AssertEquals((ZByte)5, strategy.GetMatchedDetentionPenalty("DEHAM", ContainerDetentionDirection.Export, Moq.It.IsAny<ZString>(), null)?.FreeDays);

				transport1.CarrierPK = ZGuid.Empty;
				AssertNull("Departure Transport Carrier", consol.Transports.DepartureTransport.Carrier);
				AssertEquals((ZByte)2, strategy.GetMatchedDetentionPenalty("DEHAM", ContainerDetentionDirection.Export, Moq.It.IsAny<ZString>(), null)?.FreeDays);
			}

			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 9 }))
			{
				var organization = Factory.NewWithValidTestData<OrgHeader>();
				var detention = organization.CarrierContainerPenalties.AddNew();
				detention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				detention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				detention.PD_OriginPortOrCountry = "AUSYD";
				detention.PD_DetentionPortOrCountry = "DEBRE";
				detention.PD_FreeDays = 8;
				detention.PD_FreeDayType = "CTD";
				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "DEBRE";
				consol.JK_OA_ShippingLineAddress = organization.MainAddress.PK;

				OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
				var carrierImportDetention = carrier.CarrierContainerPenalties.AddNew();
				carrierImportDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				carrierImportDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				carrierImportDetention.PD_FreeDays = 4;

				var transport1 = consol.Transports[0];
				transport1.JW_RL_NKLoadPort = "AUSYD";
				transport1.JW_RL_NKDiscPort = "AUMEL";

				var transport2 = consol.Transports.AddNew();
				transport2.JW_RL_NKLoadPort = "AUMEL";
				transport2.JW_RL_NKDiscPort = "DEBRE";
				transport2.CarrierPK = carrier.PK;

				var container = consol.Containers.AddNew();
				var strategy = new ForwardingContainerDefaultingStrategy(container);

				AssertEquals("Consol Transports Count", 2, consol.Transports.Count);
				AssertNotNull("Arrival Transport Carrier", consol.Transports.ArrivalTransport.Carrier);
				AssertEquals((ZByte)4, strategy.GetMatchedDetentionPenalty("DEBRE", ContainerDetentionDirection.Import, Moq.It.IsAny<ZString>(), null)?.FreeDays);

				transport2.CarrierPK = ZGuid.Empty;
				AssertNull("Arrival Transport Carrier", consol.Transports.ArrivalTransport.Carrier);
				AssertEquals((ZByte)8, strategy.GetMatchedDetentionPenalty("DEBRE", ContainerDetentionDirection.Import, Moq.It.IsAny<ZString>(), null)?.FreeDays);
			}
		}

		public void TestCalculateStorageFreeDaysForDetentionPort()
		{
			var departureCTO = Factory.NewWithValidTestData<OrgHeader>();
			var arrivalCTO = Factory.NewWithValidTestData<OrgHeader>();

			var exportCTOStorage = departureCTO.ServiceExportCTOStorages.AddNew();
			exportCTOStorage.PD_FreeDays = 66;
			exportCTOStorage.PD_OriginPortOrCountry = "XXX";
			exportCTOStorage.PD_DetentionPortOrCountry = "AUSYD";

			var importCTOStorage = arrivalCTO.ServiceImportCTOStorages.AddNew();
			importCTOStorage.PD_FreeDays = 77;
			importCTOStorage.PD_OriginPortOrCountry = "";
			importCTOStorage.PD_DetentionPortOrCountry = "USLAX";

			Factory.Save();
			{
				var consol = Factory.New<ForwardingConsol>();
				var container = consol.Containers.AddNew();

				consol.JK_ConsolMode = ContainerModes.FCL;
				consol.JK_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;
				consol.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";

				var strategy = new ForwardingContainerDefaultingStrategy(container);

				var matchResult = strategy.GetMatchedStoragePenalty(ContainerDetentionDirection.Import, ContainerPenaltyCreditorType.Codes.CTO, Moq.It.IsAny<ZString>());
				AssertNotNull(matchResult);
				AssertEquals(new ZByte(77), matchResult.FreeDays);

				matchResult = strategy.GetMatchedStoragePenalty(ContainerDetentionDirection.Export, ContainerPenaltyCreditorType.Codes.CTO, Moq.It.IsAny<ZString>());
				AssertNotNull(matchResult);
				AssertEquals(new ZByte(66), matchResult.FreeDays);
			}
		}

		#region TestCalculateStorageFreeDaysForDetentionPortAndLoadPort

		public void TestCalculateStorageFreeDaysForDetentionPortAndLoadPort()
		{
			var departureCTO = Factory.NewWithValidTestData<OrgHeader>();
			var arrivalCTO = Factory.NewWithValidTestData<OrgHeader>();

			var importCTOStorage1 = arrivalCTO.ServiceImportCTOStorages.AddNew();
			importCTOStorage1.PD_FreeDays = 66;
			importCTOStorage1.PD_OriginPortOrCountry = ZString.Empty;
			importCTOStorage1.PD_DetentionPortOrCountry = "USLAX";

			var importCTOStorage2 = arrivalCTO.ServiceImportCTOStorages.AddNew();
			importCTOStorage2.PD_FreeDays = 77;
			importCTOStorage2.PD_OriginPortOrCountry = "DE";
			importCTOStorage2.PD_DetentionPortOrCountry = "USLAX";

			var importCTOStorage3 = arrivalCTO.ServiceImportCTOStorages.AddNew();
			importCTOStorage3.PD_FreeDays = 88;
			importCTOStorage3.PD_OriginPortOrCountry = "AUSYD";
			importCTOStorage3.PD_DetentionPortOrCountry = "USLAX";

			Factory.Save();

			AssertCTOStorageFreeDays(departureCTO, arrivalCTO, "AUSYD", "USLAX", 88);
			AssertCTOStorageFreeDays(departureCTO, arrivalCTO, "DEHAM", "USLAX", 77);
			AssertCTOStorageFreeDays(departureCTO, arrivalCTO, "NZAKL", "USLAX", 66);
			AssertCTOStorageFreeDays(departureCTO, arrivalCTO, "NZAKL", "AUBNE", null);
		}

		void AssertCTOStorageFreeDays(OrgHeader departureCTO, OrgHeader arrivalCTO, ZString loadPort, ZString dischargePort, ZByte? freeDays)
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();

			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;

			var strategy = new ForwardingContainerDefaultingStrategy(container);
			var matchResult = strategy.GetMatchedStoragePenalty("IMP", "CTO", Moq.It.IsAny<ZString>());
			AssertEquals(freeDays, matchResult?.FreeDays);
		}

		#endregion

		#region Test Shipment Penalty Fallback To Consol

		[TestDate(2022, 11, 09, 22, 30, 11, 11)]
		public void TestShipmentPenaltyFallbackToConsol()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 1 }))
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 1 }))
			{
				var consol = SetupConsolForPenalties();
				var shipment = consol.Shipments[0];
				var container = consol.Containers[0];

				var detention = consol.ShippingLine.CarrierContainerPenalties.AddNew();
				detention.PD_Direction = ContainerDetentionDirection.Import;
				detention.PD_PenaltyType = ContainerDetentionPenaltyType.DET;
				detention.PD_FreeDays = 18;
				detention.PD_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				detention.PD_OH_Carrier = consol.ShippingLinePK;
				detention.PD_FreeDayType = ContainerDetentionFreeDayType.FCLUnload;

				var storage = consol.ShippingLine.CarrierContainerPenalties.AddNew();
				storage.PD_Direction = ContainerDetentionDirection.Import;
				storage.PD_PenaltyType = ContainerDetentionPenaltyType.STO;
				storage.PD_FreeDays = 19;
				storage.PD_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				storage.PD_OH_Carrier = consol.ShippingLinePK;
				storage.PD_FreeDayType = ContainerDetentionFreeDayType.FCLUnload;

				var ctoStorage = consol.ShippingLine.CarrierContainerPenalties.AddNew();
				ctoStorage.PD_Direction = ContainerDetentionDirection.Import;
				ctoStorage.PD_PenaltyType = ContainerDetentionPenaltyType.STO;
				ctoStorage.PD_FreeDays = 20;
				ctoStorage.PD_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
				ctoStorage.PD_OH_Carrier = consol.ShippingLinePK;
				ctoStorage.PD_FreeDayType = ContainerDetentionFreeDayType.FCLUnload;

				container.JC_ContainerYardEmptyReturnGateIn = ZDateTime.Now.AddDays(30);
				container.JC_FCLWharfGateOut = ZDateTime.Now.AddDays(30);
				container.JC_FCLUnloadFromVessel = ZDateTime.Now.AddHours(1);

				Factory.Save();

				var detPenalty = container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false);
				AssertEquals("Precondition", 18, (int)detPenalty.FreeTimeAsDays);
				AssertEquals("Precondition", ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_ContainerYardEmptyReturnGateIn, container.JC_FCLUnloadFromVessel) - detention.PD_FreeDays, (int)detPenalty.DurationAsDays);

				var stoPenalty = container.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(false);
				AssertEquals("Precondition", 19, (int)stoPenalty.FreeTimeAsDays);
				AssertEquals("Precondition", (ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_FCLWharfGateOut, container.JC_FCLUnloadFromVessel) - storage.PD_FreeDays), (int)stoPenalty.DurationAsDays);

				var ctoStoPenalty = container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false);
				ctoStoPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
				ctoStoPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
				AssertEquals("Precondition", 20, (int)ctoStoPenalty.FreeTimeAsDays);
				AssertEquals("Precondition", (ContainerPenalty.GetDaysBetweenDatesInclusive(container.JC_FCLWharfGateOut, container.JC_FCLUnloadFromVessel) - ctoStorage.PD_FreeDays), (int)ctoStoPenalty.DurationAsDays);

				container.JC_FCLUnloadFromVessel = container.JC_FCLUnloadFromVessel.AddHours(1);
				container.JC_FCLUnloadFromVessel = container.JC_FCLUnloadFromVessel.AddHours(-1);
				var dlvDetPenalty = container.DeliveryPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false, shipment: shipment);
				AssertEquals("Free Time for Delivery Penalty should equal Import Penalty", detPenalty.FreeTimeAsDays, dlvDetPenalty.FreeTimeAsDays);
				AssertEquals("Duration for Delivery Penalty should equal Import Penalty", detPenalty.DurationAsDays, dlvDetPenalty.DurationAsDays);

				container.JC_FCLUnloadFromVessel = container.JC_FCLUnloadFromVessel.AddHours(1);
				container.JC_FCLUnloadFromVessel = container.JC_FCLUnloadFromVessel.AddHours(-1);
				var dlvStoPenalty = container.DeliveryPenalties.FindOrCreateArrivalCarrierStoragePenalty(false, shipment: shipment);
				AssertEquals("Free Time for Delivery Penalty should equal Import Penalty", stoPenalty.FreeTimeAsDays, dlvStoPenalty.FreeTimeAsDays);
				AssertEquals("Duration for Delivery Penalty should equal Import Penalty", stoPenalty.DurationAsDays, dlvStoPenalty.DurationAsDays);

				container.JC_FCLUnloadFromVessel = container.JC_FCLUnloadFromVessel.AddHours(1);
				container.JC_FCLUnloadFromVessel = container.JC_FCLUnloadFromVessel.AddHours(-1);
				var dlvCtoStoPenalty = container.DeliveryPenalties.FindOrCreateArrivalCTOStoragePenalty(false, shipment: shipment);
				AssertEquals("Free Time for Delivery Penalty should equal Import Penalty", ctoStoPenalty.FreeTimeAsDays, dlvCtoStoPenalty.FreeTimeAsDays);
				AssertEquals("Duration for Delivery Penalty should equal Import Penalty", ctoStoPenalty.DurationAsDays, dlvCtoStoPenalty.DurationAsDays);
			}
		}

		#endregion

		#region Application Performance

		public void TestNoUnnecessaryContainersLoadedInFactory_ShipmentSave()
		{
			AssertNoUnnecessaryContainersLoadedInFactory(Factory, ShipmentAction, new string[] { "CTNR1", "CTNR2" });

			void ShipmentAction(CommonShipment shipment)
			{
				foreach (var packLine in shipment.OuterPackLines.Cast<PackLine>())
				{
					foreach (var container in packLine.Containers.Cast<CommonContainer>())
					{
						var relatedShipments = container.GetRelatedShipmentsForPenaltyDefaulting(ContainerPenaltyProcessType.Import);
					}
				}
			}
		}

		public void TestNoUnnecessaryContainersLoadedInFactory_Container1()
		{
			AssertNoUnnecessaryContainersLoadedInFactory(Factory, (shipment) =>
			{
				var container = FindContainer(shipment, "CTNR1");
				SetAllContainerDates(container);
				shipment.Factory.Save();
			},
			new string[] { "CTNR1", "CTNR2", "CTNR3" });
		}

		public void TestNoUnnecessaryContainersLoadedInFactory_Container2()
		{
			AssertNoUnnecessaryContainersLoadedInFactory(Factory, (shipment) =>
			{
				var container = FindContainer(shipment, "CTNR2");
				SetAllContainerDates(container);
				shipment.Factory.Save();
			},
			new string[] { "CTNR1", "CTNR2", "CTNR4" });
		}

		static void SetAllContainerDates(CommonContainer container)
		{
			container.JC_FCLAvailable = ZDateTime.Now;
			container.JC_FCLUnloadFromVessel = ZDateTime.Now;
			container.JC_FCLOnBoardVessel = ZDateTime.Now;
			container.JC_FCLWharfGateOut = ZDateTime.Now;
			container.JC_FCLWharfGateIn = ZDateTime.Now;
			container.JC_ContainerYardEmptyPickupGateOut = ZDateTime.Now;
			container.JC_ContainerYardEmptyReturnGateIn = ZDateTime.Now;
			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Now;
		}

		static void AssertNoUnnecessaryContainersLoadedInFactory(BusinessObjectFactory factory, Action<CommonShipment> shipmentAction, string[] expectedContainerNums)
		{
			var carrier = factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "ORGCAR1";
			var consignor = factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "ORGCOR1";
			var consignee = factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "ORGCSE1";

			var consol1 = CreateConsol("CONS1");
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CTNR1";
			var shipment1 = CreateShipment(consol1, "SHIP1");
			var shipment2 = CreateShipment(consol1, "SHIP2");

			var consol3 = CreateConsol("CONS3");
			var container3 = consol3.Containers.AddNew();
			container3.JC_ContainerNum = "CTNR3";
			consol3.Shipments.Add(shipment2);

			var consol2 = CreateConsol("CONS2");
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CTNR2";
			consol2.Shipments.Add(shipment1);
			var shipment3 = CreateShipment(consol2, "SHIP3");

			var consol4 = CreateConsol("CONS4");
			var container4 = consol4.Containers.AddNew();
			container4.JC_ContainerNum = "CTNR4";
			consol4.Shipments.Add(shipment3);

			AddContainerizedPackLinesToShipment(shipment1);
			AddContainerizedPackLinesToShipment(shipment2);
			AddContainerizedPackLinesToShipment(shipment3);

			AssertEquals("Precondition", container1, shipment1.OuterPackLines[0].Containers.FirstOrDefault(x => x.PK == container1.PK));
			AssertEquals("Precondition", container2, shipment1.OuterPackLines[1].Containers.FirstOrDefault(x => x.PK == container2.PK));
			AssertEquals("Precondition", container1, shipment2.OuterPackLines[0].Containers.FirstOrDefault(x => x.PK == container1.PK));
			AssertEquals("Precondition", container3, shipment2.OuterPackLines[1].Containers.FirstOrDefault(x => x.PK == container3.PK));
			AssertEquals("Precondition", container2, shipment3.OuterPackLines[0].Containers.FirstOrDefault(x => x.PK == container2.PK));
			AssertEquals("Precondition", container4, shipment3.OuterPackLines[1].Containers.FirstOrDefault(x => x.PK == container4.PK));

			factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newShipment1 = newFactory.Load<CommonShipment>(shipment1.PK);

			shipmentAction(newShipment1);

			var bizosLoaded = ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects;
			AssertEquals(expectedContainerNums.Length, bizosLoaded.Count(x => x.TableName == JobContainerSchema.Constants.TableName));

			var containerNums = new string[] { container1.JC_ContainerNum, container2.JC_ContainerNum, container3.JC_ContainerNum, container4.JC_ContainerNum };

			foreach (var containerNum in containerNums)
			{
				var bizo = bizosLoaded.OfType<CommonContainer>().FirstOrDefault(x => x.JC_ContainerNum == containerNum);
				if (bizo != null && expectedContainerNums.Any(x => x == bizo.JC_ContainerNum))
				{
					AssertNotNull($"container {containerNum} should be loaded.", bizo);
				}
				else
				{
					AssertNull($"container {containerNum} should NOT be loaded", bizo);
				}
			}

			CommonConsol CreateConsol(ZString uniqueConsignRef)
			{
				var consol = factory.NewWithValidTestData<CommonConsol>();
				consol.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
				consol.JK_UniqueConsignRef = uniqueConsignRef;
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				return consol;
			}

			CommonShipment CreateShipment(CommonConsol consol, ZString uniqueConsignRef)
			{
				var shipment = consol.Shipments.AddNew();
				shipment.ConsignorPK = consignor.PK;
				shipment.ConsigneePK = consignee.PK;
				shipment.JS_UniqueConsignRef = uniqueConsignRef;
				shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
				return shipment;
			}

			void AddContainerizedPackLinesToShipment(CommonShipment shipment)
			{
				foreach (CommonConsol consol in shipment.Consols)
				{
					foreach (CommonContainer container in consol.Containers)
					{
						var packLine = shipment.OuterPackLines.AddNew();
						packLine.SetContainer(container.PK);
					}
				}
			}
		}

		static CommonContainer FindContainer(CommonShipment shipment, string containerNum)
		{
			foreach (var packLine in shipment.OuterPackLines.Cast<PackLine>())
			{
				foreach (var container in packLine.Containers.Cast<CommonContainer>())
				{
					if (containerNum == container.JC_ContainerNum)
					{
						return container;
					}
				}
			}

			return null;
		}

		#endregion

		#region Implementation

		ForwardingConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = Factory.New<ForwardingConsol>();
				}
				return consol;
			}
		}
		ForwardingConsol consol;

		ForwardingShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Consol.Shipments.AddNew();
				}
				return shipment;
			}
		}
		ForwardingShipment shipment;

		BusinessObject Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					declaration[JobDeclarationSchema.Constants.JE_TransportMode] = Core.Constants.TransportModes.Sea;
				}
				return declaration;
			}
		}
		BusinessObject declaration;

		OrgHeader Carrier
		{
			get
			{
				if (carrier == null)
				{
					carrier = Factory.NewWithValidTestData<OrgHeader>();
				}
				return carrier;
			}
		}
		OrgHeader carrier;

		OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = Factory.NewWithValidTestData<OrgHeader>();
				}
				return importer;
			}
		}
		OrgHeader importer;

		OrgContainerDetention DetentionForCarrierOnly
		{
			get
			{
				if (detentionForCarrierOnly == null)
				{
					detentionForCarrierOnly = Carrier.CarrierContainerPenalties.AddNew();
					detentionForCarrierOnly.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
					detentionForCarrierOnly.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
					detentionForCarrierOnly.PD_ContainerType = "20F";
					detentionForCarrierOnly.PD_OriginPortOrCountry = "AU";
					detentionForCarrierOnly.PD_FreeDays = 1;
				}
				return detentionForCarrierOnly;
			}
		}
		OrgContainerDetention detentionForCarrierOnly;

		OrgContainerDetention DetentionForCarrierAndImporter
		{
			get
			{
				if (detentionForCarrierAndImporter == null)
				{
					detentionForCarrierAndImporter = Carrier.CarrierContainerPenalties.AddNew();
					detentionForCarrierAndImporter.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
					detentionForCarrierAndImporter.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
					detentionForCarrierAndImporter.PD_OH_Client = Importer.PK;
					detentionForCarrierAndImporter.PD_ContainerType = "20F";
					detentionForCarrierAndImporter.PD_OriginPortOrCountry = "AU";
					detentionForCarrierAndImporter.PD_FreeDays = 3;
				}
				return detentionForCarrierAndImporter;
			}
		}
		OrgContainerDetention detentionForCarrierAndImporter;

		(ForwardingShipment Shipment, ForwardingContainer Container) CreateShipmentAndContainer()
		{
			var shippingLine = Factory.New<OrgHeader>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;

			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var packingLine = shipment.OuterPackLines.AddNew();
			packingLine.Containers.Add(container);

			return (shipment, container);
		}

		#endregion
	}
}
