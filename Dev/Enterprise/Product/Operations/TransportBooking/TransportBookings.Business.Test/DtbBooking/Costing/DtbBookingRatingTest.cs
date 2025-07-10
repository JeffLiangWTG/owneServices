using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using Directions = Enterprise.MasterFiles.Business.Directions;

namespace Enterprise.TransportBookings.Business.Testing
{
	public sealed class DtbBookingRatingTest : TestCaseWithFactory
	{
		public void TestIAutoRating_QuickCalculator_Measures_Chargeable()
		{
			var transportJob = GetTransportJob();
			var commodity = CreateCommodity("AAA");
			var container = CreatePackage(transportJob, 1, Constants.PkgUnit.Container, commodity, 300m, Constants.Weight.Kilograms, 15m, Constants.Volume.CubicMetres);

			var fromInstruction = CreateFromInstruction(transportJob);
			var toInstruction = CreateToInstruction(transportJob);
			CreateInstructionPkgDivots(fromInstruction, container);
			CreateInstructionPkgDivots(toInstruction, container);

			var iAutoRating = (IAutoRating)new DtbBookingRatingAdapter(transportJob, FreightMode.ROA, fromInstruction, toInstruction);
			var chargeableQuantity = ((RateableMeasureSet)iAutoRating.RateableMeasures).GetQuantity(MeasureType.Chargeable);
			AssertEquals(5000m, chargeableQuantity.Amount); // 3000 CC = 1 KG; 15 CubicMetres equals 15000000 CC, So 15000000 / 3000 = 5000 KG
			AssertEquals(Constants.Weight.Kilograms, chargeableQuantity.Unit);
		}

		public void TestAdapterTypeAndID()
		{
			var booking = GetTransportJob();
			var iAutoRating = GetRatingAdapter(booking);

			AssertEquals(AdapterType.TransportBooking, iAutoRating.AdapterType);
			AssertEquals(booking.KM_JobID, iAutoRating.OperationalJobCode);
			AssertEquals(booking.KM_JobID, iAutoRating.JobID);
		}

		public void TestIAutoRating_StatusInformation_WithUnknownFreigtMode()
		{
			var transportJob = Helper.CreateBooking();
			var fromInstruction = CreateFromInstruction(transportJob);
			var toInstruction = CreateToInstruction(transportJob);

			var iUnknownAutoRating = (IAutoRating)new DtbBookingRatingAdapter(transportJob, FreightMode.UKN, fromInstruction, toInstruction);
			var info = iUnknownAutoRating.StatusInformation;
			var expectedErrorMessage = string.Format("Freight Mode for {0} is empty. Cannot continue.", transportJob.HumanReadableName);
			AssertEquals(false, info.CanExecute);
			AssertEquals(expectedErrorMessage, info.Message);

			transportJob.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;
			iUnknownAutoRating = new DtbBookingRatingAdapter(transportJob, FreightMode.UKN, fromInstruction, toInstruction);
			info = iUnknownAutoRating.StatusInformation;
			expectedErrorMessage = "Cannot find related rates, please check if packages exist and if they've been assigned.";
			AssertEquals(false, info.CanExecute);
			AssertEquals(expectedErrorMessage, info.Message);
		}

		public void TestIAutoRating_FreightMode_RAI()
		{
			var transportJob = Helper.CreateBooking();
			transportJob.KM_TransportMode = Constants.TransportModes.Rail;

			var fromInstruction = CreateFromInstruction(transportJob);
			var toInstruction = CreateToInstruction(transportJob);
			var container = CreatePackage(transportJob, 1, Constants.PkgUnit.Container, CreateCommodity("AAA"), 300m, Constants.Weight.Kilograms, 15m, Constants.Volume.CubicMetres);
			CreateInstructionPkgDivots(toInstruction, container);
			CreateInstructionPkgDivots(fromInstruction, container);

			var autoAdapaterFRA = (IAutoRating)new DtbBookingRatingAdapter(transportJob, FreightMode.FRA, fromInstruction, toInstruction);
			var rateableMeasures = (RateableMeasureSet)autoAdapaterFRA.RateableMeasures;

			AssertEquals("Because Freight Mode is RAI, rating should get chargeable amount of $5000", 5000m, ((RateableMeasureSet)autoAdapaterFRA.RateableMeasures).GetQuantity(MeasureType.Chargeable).Amount);
		}

		public void TestIAutoRating_FreightMode_ROA()
		{
			var transportJob = Helper.CreateBooking();
			transportJob.KM_TransportMode = Constants.TransportModes.Road;

			var fromInstruction = CreateFromInstruction(transportJob);
			var toInstruction = CreateToInstruction(transportJob);
			var container = CreatePackage(transportJob, 1, Constants.PkgUnit.Container, CreateCommodity("AAA"), 300m, Constants.Weight.Kilograms, 15m, Constants.Volume.CubicMetres);
			CreateInstructionPkgDivots(toInstruction, container);
			CreateInstructionPkgDivots(fromInstruction, container);

			var autoAdapaterLRA = (IAutoRating)new DtbBookingRatingAdapter(transportJob, FreightMode.LRA, fromInstruction, toInstruction);
			var rateableMeasures = (RateableMeasureSet)autoAdapaterLRA.RateableMeasures;

			AssertEquals("Because Freight Mode is ROA, rating should get chargeable amount of $5000", 5000m, ((RateableMeasureSet)autoAdapaterLRA.RateableMeasures).GetQuantity(MeasureType.Chargeable).Amount);
		}

		public void TestIAutoRatingOrganisations_Carrier()
		{
			var booking = GetTransportJob();
			var iAutoRating = GetRatingAdapter(booking);
			AssertNull(iAutoRating.Carrier);

			var address = Factory.New<OrgAddress>();
			booking.Address.E2_OA_Address = address.PK;
			AssertNull(iAutoRating.Carrier);

			var orgHeader = Factory.New<OrgHeader>();
			address.OA_OH = orgHeader.PK;
			AssertEquals(orgHeader, iAutoRating.Carrier);
		}

		public void TestAutoRatingOrganisations_Consignor()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertNull("Precondition", iAutoRating.DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNR]);
			AssertNull("Precondition", iAutoRating.PickupAddress);
			AssertEquals("Precondition", "", iAutoRating.PickupCartageEquipment);

			var instruction = CreateFromInstruction(transportJob);
			iAutoRating = GetRatingAdapter(transportJob);
			AssertNull(iAutoRating.DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNR]);
			AssertEquals(instruction.Address, iAutoRating.PickupAddress);
			AssertEquals("", iAutoRating.PickupCartageEquipment);

			var address = Factory.New<OrgAddress>();
			instruction.Address.E2_OA_Address = address.PK;
			AssertNull(iAutoRating.DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNR]);
			AssertEquals(instruction.Address, iAutoRating.PickupAddress);
			AssertEquals("", iAutoRating.PickupCartageEquipment);

			var orgHeader = Factory.New<OrgHeader>();
			address.OA_OH = orgHeader.PK;
			instruction.KN_DropMode = "ABC";
			AssertEquals(orgHeader, iAutoRating.DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNR]);
			AssertEquals(instruction.Address, iAutoRating.PickupAddress);
			AssertEquals("ABC", iAutoRating.PickupCartageEquipment);
		}

		public void TestAutoRatingOrganisations_Consignee()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertNull("Precondition", iAutoRating.DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNE]);
			AssertNull("Precondition", iAutoRating.DeliveryAddress);
			AssertEquals("Precondition", "", iAutoRating.DeliveryCartageEquipment);

			var instructionFrom = CreateFromInstruction(transportJob);
			var instructionTo = CreateToInstruction(transportJob);
			iAutoRating = GetRatingAdapter(transportJob);
			AssertNull(iAutoRating.DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNE]);
			AssertEquals(instructionTo.Address, iAutoRating.DeliveryAddress);
			AssertEquals("", iAutoRating.DeliveryCartageEquipment);

			var address = Factory.New<OrgAddress>();
			address.OA_LCLEquipmentNeeded = "PSL";
			instructionTo.Address.E2_OA_Address = address.PK;
			AssertNull(iAutoRating.DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNE]);
			AssertEquals(instructionTo.Address, iAutoRating.DeliveryAddress);
			AssertEquals("", iAutoRating.DeliveryCartageEquipment);

			var orgHeader = Factory.New<OrgHeader>();
			address.OA_OH = orgHeader.PK;
			instructionTo.KN_DropMode = "ABC";
			AssertEquals(orgHeader, iAutoRating.DebtorOrgs[Registry.Business.RatingDebtorOrgTypes.CNE]);
			AssertEquals(instructionTo.Address, iAutoRating.DeliveryAddress);
			AssertEquals("ABC", iAutoRating.DeliveryCartageEquipment);
		}

		public void TestIAutoRatingOrganisations_TransportProviders()
		{
			var booking = GetTransportJob();
			var iAutoRating = GetRatingAdapter(booking);
			AssertEquals(0, iAutoRating.Creditors.AllOrgs.Count);

			var address = Factory.New<OrgAddress>();
			booking.Address.E2_OA_Address = address.PK;
			AssertEquals(0, iAutoRating.Creditors.AllOrgs.Count);

			var orgHeader = Factory.New<OrgHeader>();
			address.OA_OH = orgHeader.PK;
			AssertContainsExactElementsInAnyOrder(new[] { orgHeader }, iAutoRating.Creditors.AllOrgs);
		}

		public void TestIAutoRatingOrganisations_Measures_LRO_Multi()
		{
			// 1x container	 300 KG	  15 M3
			// 3x BOX		  30 KG	   1 M3
			// 2x PLT		  20 KG	0.15 M3
			var transportJob = GetTransportJob();
			var commodity = CreateCommodity("AAA");
			var commodityBBB = CreateCommodity("BBB");
			var container = CreatePackage(transportJob, 1, Constants.PkgUnit.Container, commodity, 300m, Constants.Weight.Kilograms, 15m, Constants.Volume.CubicMetres);
			var box = CreatePackage(transportJob, 3, Constants.PkgUnit.Box, commodity, 30m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);
			var pallet = CreatePackage(transportJob, 2, Constants.PkgUnit.Pallet, commodity, 20m, Constants.Weight.Kilograms, 0.15m, Constants.Volume.CubicMetres);
			var carton = CreatePackage(transportJob, 6, Constants.PkgUnit.Carton, commodityBBB, 66m, Constants.Weight.Kilograms, 0.66m, Constants.Volume.CubicMetres);

			var fromInstruction = CreateFromInstruction(transportJob);
			var toInstruction = CreateToInstruction(transportJob);
			var toInstruction2 = CreateToInstruction(transportJob);
			CreateInstructionPkgDivots(fromInstruction, container, box, pallet, carton);
			CreateInstructionPkgDivots(toInstruction, container, box, pallet);
			CreateInstructionPkgDivots(toInstruction2, carton);

			var iAutoRating = (IAutoRating)new DtbBookingRatingAdapter(transportJob, FreightMode.LRO, fromInstruction, toInstruction);
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			// Packages
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.Package));
			AssertContainsExactElementsInAnyOrder(new[] { "AAA" }, rateableMeasures.GetPackageUniqueCommodities());
			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Box, Constants.PkgUnit.Pallet }, rateableMeasures.GetPackageUniquePackageTypes());

			// Weight
			AssertEquals(50m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertContainsExactElementsInAnyOrder(new[] { "AAA" }, rateableMeasures.GetWeightUniqueCommodities());
			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Box, Constants.PkgUnit.Pallet }, rateableMeasures.GetWeightUniquePackageTypes());

			// Volume
			AssertEquals(1.15m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertContainsExactElementsInAnyOrder(new[] { "AAA" }, rateableMeasures.GetVolumeUniqueCommodities());
			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Box, Constants.PkgUnit.Pallet }, rateableMeasures.GetVolumeUniquePackageTypes());

			// Containers Count
			Assert(!rateableMeasures.HasMeasureType(MeasureType.ContainerCount));

			iAutoRating = new DtbBookingRatingAdapter(transportJob, FreightMode.LRO, fromInstruction, toInstruction2);
			rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			// Packages
			AssertEquals(6m, rateableMeasures.GetActual(MeasureType.Package));
			AssertContainsExactElementsInAnyOrder(new[] { "BBB" }, rateableMeasures.GetPackageUniqueCommodities());
			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Carton }, rateableMeasures.GetPackageUniquePackageTypes());

			// Weight
			AssertEquals(66m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertContainsExactElementsInAnyOrder(new[] { "BBB" }, rateableMeasures.GetWeightUniqueCommodities());
			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Carton }, rateableMeasures.GetWeightUniquePackageTypes());

			// Volume
			AssertEquals(0.66m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertContainsExactElementsInAnyOrder(new[] { "BBB" }, rateableMeasures.GetVolumeUniqueCommodities());
			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Carton }, rateableMeasures.GetVolumeUniquePackageTypes());

			// Containers Count
			Assert(!rateableMeasures.HasMeasureType(MeasureType.ContainerCount));
		}

		public void TestIAutoRatingOrganisations_ServiceLevel()
		{
			var booking = GetTransportJob();
			var iAutoRating = GetRatingAdapter(booking);
			AssertEquals(2, iAutoRating.ServiceLevel.ServiceLevelData.Length);
			iAutoRating.ServiceLevel.ServiceLevelData.Single(s => s.ServiceLevelType == ServiceLevelType.Client && s.ServiceLevel == "");
			iAutoRating.ServiceLevel.ServiceLevelData.Single(s => s.ServiceLevelType == ServiceLevelType.Carrier && s.ServiceLevel == "");

			booking.KM_RS_NKServiceLevel = "BOB";
			booking.KM_PL_NKCarrierServiceLevel = "TED";
			AssertEquals(2, iAutoRating.ServiceLevel.ServiceLevelData.Length);
			iAutoRating.ServiceLevel.ServiceLevelData.Single(s => s.ServiceLevelType == ServiceLevelType.Client && s.ServiceLevel == "BOB");
			iAutoRating.ServiceLevel.ServiceLevelData.Single(s => s.ServiceLevelType == ServiceLevelType.Carrier && s.ServiceLevel == "TED");
		}

		public void TestConditionsSupporter()
		{
			var transportJob = GetTransportJob();
			AssertType<DtbBookingRateLineConditionsSupporter>(((IAutoRatingFreightConditionsSupportable)GetRatingAdapter(transportJob)).ConditionsSupporter);
		}

		public void TestIAutoRating_ChargeCodeGroups()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertContainsExactElementsInAnyOrder(new[] { ChargeCodeGroupList.Codes.TransportBooking }, iAutoRating.ChargeCodeGroups);
		}

		public void TestIAutoRating_ConsumerType()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertEquals(ExpectedConsumerType, iAutoRating.ConsumerType);
		}

		JobInvoicingConsumerType ExpectedConsumerType
		{
			get { return JobInvoicingConsumerTypes.TransportBooking; }
		}

		public void TestIAutoRating_Properties()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertEquals(MergeChargeOptions.WithinAdapter, iAutoRating.MergeCharges);
			AssertEquals(RateType.TransportBookings, iAutoRating.RateTypeToUse);
			AssertEquals(0, iAutoRating.JobServices.Count);
		}

		public void TestIAutoRating_StatusInformation()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			var expectedErrorMessage = ZString.Format("{0} doesn't have at least 2 Instructions. Cannot continue.", transportJob.HumanReadableName);

			var info1 = iAutoRating.StatusInformation;
			AssertEquals("Precondition", false, info1.CanExecute);
			AssertEquals("Precondition", expectedErrorMessage, info1.Message);

			var fromInstruction = CreateFromInstruction(transportJob);
			var info2 = iAutoRating.StatusInformation;
			AssertEquals(false, info2.CanExecute);
			AssertEquals(expectedErrorMessage, info2.Message);

			var toInstruiction = CreateToInstruction(transportJob);
			iAutoRating = GetRatingAdapter(transportJob);
			var info3 = iAutoRating.StatusInformation;
			AssertEquals(true, info3.CanExecute);
			AssertEquals("", info3.Message);

			AssertJobDates(fromInstruction, (confirmation, datetime) => confirmation.KK_Actual = datetime, "Please enter valid Actual date.");
			AssertJobDates(toInstruiction, (confirmation, datetime) => confirmation.KK_Actual = datetime, "Please enter valid Actual date.");
			AssertJobDates(fromInstruction, (confirmation, datetime) => confirmation.KK_Estimated = datetime, "Please enter valid Estimated date.");
			AssertJobDates(toInstruiction, (confirmation, datetime) => confirmation.KK_Estimated = datetime, "Please enter valid Estimated date.");

			void AssertJobDates(DtbBookingInstruction bookingInstruction, Action<DtbBookingConfirmation, ZDateTime> action, string expectedError)
			{
				var confirmation1 = bookingInstruction.Confirmations.AddNew();
				action(confirmation1, ZDateTime.Now);
				var confirmation2 = bookingInstruction.Confirmations.AddNew();
				action(confirmation2, ZDateTime.Invalid);

				var statusInfo = iAutoRating.StatusInformation;
				AssertEquals(false, statusInfo.CanExecute);
				AssertEquals(expectedError, statusInfo.Message);

				action(confirmation2, ZDateTime.Now);

				statusInfo = iAutoRating.StatusInformation;
				AssertEquals(true, statusInfo.CanExecute);
				AssertEquals("", statusInfo.Message);
			}
		}

		public void TestIAutoRatingLocations_Origin()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertNull("Precondition", iAutoRating.Origin);

			var fromInstruction = CreateFromInstruction(transportJob);
			iAutoRating = GetRatingAdapter(transportJob);
			AssertNull(iAutoRating.Origin);

			var address = Factory.New<OrgAddress>();
			fromInstruction.Address.E2_OA_Address = address.PK;
			AssertNull(iAutoRating.Origin);

			address.OA_RL_NKRelatedPortCode = "AUSYD";
			AssertEquals("AUSYD", iAutoRating.Origin.Code);

			fromInstruction.Address.E2_AddressOverride = true;
			fromInstruction.Address.E2_RN_NKCountryCode = "NZ";
			AssertEquals("NZ", iAutoRating.Origin.Code);
		}

		public void TestIAutoRatingLocations_Properties()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertNull(iAutoRating.Destination);
			AssertNull(iAutoRating.GetVia(CostSell.Cost));
			AssertNull(iAutoRating.GetVia(CostSell.Revenue));
		}

		public void TestIAutoRatingOrganisations_Properties()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertEquals(FreightMode.LRO, iAutoRating.FreightMode);
			AssertEquals("", iAutoRating.HousebillReleaseType);
			AssertNull(iAutoRating.PaymentTerm);

			AssertNull(iAutoRating.WharfCTOAddress);
		}

		public void TestNoDivideByZeroErrors()
		{
			// 0x container	 300 KG	  15 M3
			// 0x BOX		  30 KG	   1 M3
			// 0x PLT		  20 KG	0.15 M3
			var transportJob = GetTransportJob();
			var commodity = CreateCommodity("AAA");
			var container = CreatePackage(transportJob, 0, Constants.PkgUnit.Container, commodity, 300m, Constants.Weight.Kilograms, 15m, Constants.Volume.CubicMetres);
			var box = CreatePackage(transportJob, 0, Constants.PkgUnit.Box, commodity, 30m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);
			var pallet = CreatePackage(transportJob, 0, Constants.PkgUnit.Pallet, commodity, 20m, Constants.Weight.Kilograms, 0.15m, Constants.Volume.CubicMetres);
			AddCarton(transportJob, commodity);

			var fromInstruction = CreateFromInstruction(transportJob);
			var toInstruction = CreateToInstruction(transportJob);

			CreateDivots(fromInstruction, container, box, pallet, toInstruction);

			var iAutoRating = GetRatingAdapter(transportJob);

			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			// Packages
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Package));
			// Note, commodities and package types of the packages are not meaningful if there are zero packages
			AssertContainsExactElementsInAnyOrder(new[] { "AAA" }, rateableMeasures.GetPackageUniqueCommodities());
			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Box, Constants.PkgUnit.Pallet }, rateableMeasures.GetPackageUniquePackageTypes());

			// Weight
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertContainsExactElementsInAnyOrder(new[] { "AAA" }, rateableMeasures.GetWeightUniqueCommodities());
			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Box, Constants.PkgUnit.Pallet }, rateableMeasures.GetWeightUniquePackageTypes());

			// Volume
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertContainsExactElementsInAnyOrder(new[] { "AAA" }, rateableMeasures.GetVolumeUniqueCommodities());
			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Box, Constants.PkgUnit.Pallet }, rateableMeasures.GetVolumeUniquePackageTypes());

			// Containers Count
			Assert(!rateableMeasures.HasMeasureType(MeasureType.ContainerCount));
		}

		public void TestIAutoRatingOrganisations_Measures_LRO()
		{
			// 1x container	 300 KG	  15 M3
			// 3x BOX		  30 KG	   1 M3
			// 2x PLT		  20 KG	0.15 M3
			var transportJob = GetTransportJob();
			var commodity = CreateCommodity("AAA");
			var container = CreatePackage(transportJob, 1, Constants.PkgUnit.Container, commodity, 300m, Constants.Weight.Kilograms, 15m, Constants.Volume.CubicMetres);
			var box = CreatePackage(transportJob, 3, Constants.PkgUnit.Box, commodity, 30m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);
			var pallet = CreatePackage(transportJob, 2, Constants.PkgUnit.Pallet, commodity, 20m, Constants.Weight.Kilograms, 0.15m, Constants.Volume.CubicMetres);
			AddCarton(transportJob, commodity);

			var fromInstruction = CreateFromInstruction(transportJob);
			var toInstruction = CreateToInstruction(transportJob);

			CreateDivots(fromInstruction, container, box, pallet, toInstruction);

			var iAutoRating = GetRatingAdapter(transportJob);
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			// Packages
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.Package));
			AssertContainsExactElementsInAnyOrder(new[] { "AAA" }, rateableMeasures.GetPackageUniqueCommodities());
			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Box, Constants.PkgUnit.Pallet }, rateableMeasures.GetPackageUniquePackageTypes());

			// Weight
			AssertEquals(50m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertContainsExactElementsInAnyOrder(new[] { "AAA" }, rateableMeasures.GetWeightUniqueCommodities());
			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Box, Constants.PkgUnit.Pallet }, rateableMeasures.GetWeightUniquePackageTypes());

			// Volume
			AssertEquals(1.15m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertContainsExactElementsInAnyOrder(new[] { "AAA" }, rateableMeasures.GetVolumeUniqueCommodities());
			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Box, Constants.PkgUnit.Pallet }, rateableMeasures.GetVolumeUniquePackageTypes());

			// Containers Count
			Assert(!rateableMeasures.HasMeasureType(MeasureType.ContainerCount));
		}

		public void TestIAutoRatingOrganisations_Measures_FRO()
		{
			var job = GetTransportJob();
			SetupJobInstructions(job, hasPallet: false, hasContainer: true);
			var commodity = CreateCommodity("AAA");
			var container = job.Containers.First().Package;
			container.KP_PackageID = "CONT1";
			container.KP_RH_NKCommodityCode = "AAA";
			var iAutoRating = GetRatingAdapterFRO(job);
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(false, rateableMeasures.HasMeasureType(MeasureType.Package));
			AssertEquals(1000m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals(1111m, rateableMeasures.GetActual(MeasureType.Volume));

			var containerInfos = rateableMeasures.GetAllContainers().ToList();
			AssertEquals(1, containerInfos.Count);
			AssertEquals(1m, containerInfos.Sum(c => c.TEU));
			AssertEquals("CONT1", containerInfos[0].ContainerNumber);
			AssertEquals((ZDecimal)1000m, containerInfos[0].ContainerWeightInKG);
			AssertEquals(gp20.PK, rateableMeasures.GetContainerTypePKs().Single());
			var containerGroups = rateableMeasures.GetContainerGroups().ToList();
			AssertEquals("CONT1", containerGroups.Select(x => x.ContainerNumber).Single());
			AssertEquals("AAA", containerGroups.Select(x => x.CommodityCode).Single());
		}

		public void TestIAutoRatingOrganisations_WeightAssertion_ContainerOnly()
		{
			var job = GetTransportJob();
			SetupJobInstructions(job, hasPallet: false, hasContainer: true);
			var iAutoRating = GetRatingAdapterFRO(job);
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			var containerInfos = rateableMeasures.GetAllContainers().ToList();
			AssertEquals("Container Count", 1, containerInfos.Count);
			AssertEquals("Container Gross Weight should be added to the MeasureType.Weight.", 1000m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals("Container Volume should be added to the MeasureType.Volume.", 1111m, rateableMeasures.GetActual(MeasureType.Volume));
		}

		public void TestIAutoRatingOrganisations_FRO_WeightAssertion_ContainerAndPallet()
		{
			var job = GetTransportJob();
			SetupJobInstructions(job, hasPallet: true, hasContainer: true);
			var iAutoRating = GetRatingAdapterFRO(job);
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals("Container Gross Weight should be added to the MeasureType.Weight.", 1020m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals("Container Volume should be added to the MeasureType.Volume.", 1111m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals("Container Count", 1, rateableMeasures.GetAllContainers().Count());
		}

		public void TestIAutoRatingOrganisations_WeightAssertion_PalletOnly()
		{
			var job = GetTransportJob();
			SetupJobInstructions(job, hasPallet: true, hasContainer: false);
			var iAutoRating = GetRatingAdapterFRO(job);
			AssertEquals("Pallet Only weight", 20m, ((RateableMeasureSet)iAutoRating.RateableMeasures).GetActual(MeasureType.Weight));
		}

		public void TestIAutoRatingOrganisations_InvalidWeightUnit()
		{
			var job = GetTransportJob();
			var pallet = CreatePackage(job, 1, Constants.PkgUnit.Pallet, null, 1000m, "!!", 1111m, Constants.Volume.CubicFeet);
			var fromInstruction = CreateFromInstruction(job);
			var toInstruction = CreateToInstruction(job);
			CreateInstructionPkgDivots(fromInstruction, pallet);
			CreateInstructionPkgDivots(toInstruction, pallet);
			var iAutoRating = GetRatingAdapter(job);

			AssertEquals(false, iAutoRating.StatusInformation.CanExecute);
			AssertEquals("Invalid unit of weight: '!!'.", iAutoRating.StatusInformation.Message);
		}

		public void TestIAutoRatingOrganisations_InvalidVolumeUnit()
		{
			var job = GetTransportJob();
			var pallet = CreatePackage(job, 1, Constants.PkgUnit.Pallet, null, 1000m, Constants.Weight.Kilograms, 1111m, "!!");
			var fromInstruction = CreateFromInstruction(job);
			var toInstruction = CreateToInstruction(job);
			CreateInstructionPkgDivots(fromInstruction, pallet);
			CreateInstructionPkgDivots(toInstruction, pallet);
			var iAutoRating = GetRatingAdapter(job);

			AssertEquals(false, iAutoRating.StatusInformation.CanExecute);
			AssertEquals("Invalid unit of volume: '!!'.", iAutoRating.StatusInformation.Message);
		}

		public void TestCanExpandMacros()
		{
			var transportJob = GetTransportJob();
			var iExpander = (IAutoRatingDescriptionMacroExpander)GetRatingAdapter(transportJob);
			AssertEquals(true, iExpander.CanExpandMacros);
		}

		public void TestExpandMacro_Empty()
		{
			var transportJob = GetTransportJob();
			var iExpander = (IAutoRatingDescriptionMacroExpander)GetRatingAdapter(transportJob);

			AssertEquals("FromName", "", iExpander.ExpandMacro("FromName"));
			AssertEquals("FromCity", "", iExpander.ExpandMacro("FromCity"));
			AssertEquals("FromState", "", iExpander.ExpandMacro("FromState"));
			AssertEquals("FromPostcode", "", iExpander.ExpandMacro("FromPostcode"));
			AssertEquals("ToName", "", iExpander.ExpandMacro("ToName"));
			AssertEquals("ToCity", "", iExpander.ExpandMacro("ToCity"));
			AssertEquals("ToState", "", iExpander.ExpandMacro("ToState"));
			AssertEquals("ToPostcode", "", iExpander.ExpandMacro("ToPostcode"));
		}

		public void TestExpandMacro()
		{
			var transportJob = GetTransportJob();
			var from = CreateFromInstruction(transportJob);
			var to = CreateToInstruction(transportJob);

			from.Address.E2_AddressOverride = true;
			from.Address.E2_CompanyName = "DoodyCo";
			from.Address.E2_Postcode = "2000";
			from.Address.E2_State = "NSW";
			from.Address.E2_City = "Alexandria";

			to.Address.E2_AddressOverride = true;
			to.Address.E2_CompanyName = "OrionCo";
			to.Address.E2_Postcode = "3000";
			to.Address.E2_State = "QLD";
			to.Address.E2_City = "Rockhampton";

			var iExpander = (IAutoRatingDescriptionMacroExpander)GetRatingAdapter(transportJob);

			AssertEquals("FromName", "DoodyCo", iExpander.ExpandMacro("FromName"));
			AssertEquals("FromCity", "Alexandria", iExpander.ExpandMacro("FromCity"));
			AssertEquals("FromState", "NSW", iExpander.ExpandMacro("FromState"));
			AssertEquals("FromPostcode", "2000", iExpander.ExpandMacro("FromPostcode"));
			AssertEquals("ToName", "OrionCo", iExpander.ExpandMacro("ToName"));
			AssertEquals("ToCity", "Rockhampton", iExpander.ExpandMacro("ToCity"));
			AssertEquals("ToState", "QLD", iExpander.ExpandMacro("ToState"));
			AssertEquals("ToPostcode", "3000", iExpander.ExpandMacro("ToPostcode"));
		}

		public void TestAutoRatingOrganisationsImportBroker()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertNull(iAutoRating.ImportBroker);
		}

		public void TestAutoRatingOrganisationsExportBroker()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertNull(iAutoRating.ExportBroker);
		}

		public void TestIImportExport_IsImport()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertEquals(Directions.Unknown, iAutoRating.JobDirection);
		}

		public void TestJobNumber()
		{
			var transportJob = GetTransportJob();
			var iJobNumber = (IJobNumber)GetRatingAdapter(transportJob);
			transportJob.KM_JobID = "Test";
			AssertEquals("Test", iJobNumber.JobNumber);
		}

		void AddCarton(DtbBooking transportJob, RefCommodityCode commodity)
		{
			CreatePackage(transportJob, 5, Constants.PkgUnit.Carton, commodity, 50m, Constants.Weight.Kilograms, 0.30m, Constants.Volume.CubicMetres);
		}

		void CreateDivots(DtbBookingInstruction fromInstruction, PkgPackage container, PkgPackage box, PkgPackage pallet, DtbBookingInstruction toInstruction)
		{
			CreateInstructionPkgDivots(fromInstruction, container, box, pallet);
			CreateInstructionPkgDivots(toInstruction, container, box, pallet);
		}

		RefCommodityCode CreateCommodity(ZString code)
		{
			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = code;

			return commodity;
		}

		PkgPackage CreatePackage(DtbBooking transportJob, int quantity, string quantityUQ, RefCommodityCode commodity, decimal weight, string weightUQ, decimal volume, string volumeUQ)
		{
			var result = Factory.New<PkgPackage>();//()transportJob.AssignedPackages.AddNew();
			result.KP_PackageQty = quantity;
			result.KP_F3_NKPackType = quantityUQ;
			if (result.IsContainer)
			{
				result.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			}
			result.KP_Weight = weight;
			result.KP_WeightUQ = weightUQ;
			result.KP_Volume = volume;
			result.KP_VolumeUQ = volumeUQ;
			result.KP_RH_NKCommodityCode = commodity != null ? commodity.RH_Code : ZString.Empty;
			result.KP_KJ_ParentPackageJob = GetPackageJob(transportJob).PK;

			return result;
		}

		void CreateInstructionPkgDivots(DtbBookingInstruction instruction, params PkgPackage[] packages)
		{
			instruction.PackageDivots.DeleteAll();

			foreach (var package in packages)
			{
				instruction.DivotsWithPackages.AddPackage(package);
			}
		}

		void SetupJobInstructions(DtbBooking job, bool hasPallet, bool hasContainer)
		{
			//check out volume
			var container = CreatePackage(job, 1, Constants.PkgUnit.Container, null, 1000m, Constants.Weight.Kilograms, 1111m, Constants.Volume.CubicMetres);
			var pallet = CreatePackage(job, 2, Constants.PkgUnit.Pallet, null, 20m, Constants.Weight.Kilograms, 0.15m, Constants.Volume.CubicMetres);
			var fromInstruction = CreateFromInstruction(job);
			var toInstruction = CreateToInstruction(job);
			var packageList = new List<PkgPackage>();

			if (hasPallet)
			{
				packageList.Add(pallet);
			}

			if (hasContainer)
			{
				packageList.Add(container);
			}

			CreateInstructionPkgDivots(fromInstruction, packageList.ToArray());
			CreateInstructionPkgDivots(toInstruction, packageList.ToArray());
		}

		protected override void SetUp()
		{
			base.SetUp();

			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider();
			transportBookingTestCache = TransportBookingTestCache.Instance;
		}

		protected override void TearDown()
		{
			dummyWriterDecider.Dispose();
			transportBookingTestCache.Dispose();
			base.TearDown();
		}

		IDisposable dummyWriterDecider;
		IDisposable transportBookingTestCache;

		DtbBooking GetTransportJob()
		{
			return Helper.CreateBooking();
		}

		IAutoRating GetRatingAdapter(DtbBooking transportJob)
		{
			return new DtbBookingRatingAdapter(transportJob, FreightMode.LRO, transportJob.FirstPickup, transportJob.LastDelivery);
		}

		IAutoRating GetRatingAdapterFRO(DtbBooking transportJob)
		{
			return new DtbBookingRatingAdapter(transportJob, FreightMode.FRO, transportJob.FirstPickup, transportJob.LastDelivery);
		}

		DtbBookingInstruction CreateFromInstruction(DtbBooking booking)
		{
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			picInstruction.KN_IsLooseRateable = true;

			return picInstruction;
		}

		DtbBookingInstruction CreateToInstruction(DtbBooking booking)
		{
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			dlvInstruction.KN_IsLooseRateable = true;

			return dlvInstruction;
		}

		PkgPackageJob GetPackageJob(DtbBooking transportJob)
		{
			return transportJob.ConsolidationSingleJob.PackageJob;
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
