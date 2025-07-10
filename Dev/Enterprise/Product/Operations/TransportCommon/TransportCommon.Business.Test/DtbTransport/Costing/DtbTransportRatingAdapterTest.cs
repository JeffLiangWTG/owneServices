using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportRatingAdapterTest<T> : TestCaseWithFactory
			where T : DtbTransport
	{
		#region IAutoRating Members

		#region IAutoRating Members

		#region TestIAutoRating_ChargeCodeGroups

		public void TestIAutoRating_ChargeCodeGroups()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertContainsExactElementsInAnyOrder(new[] { ChargeCodeGroupList.Codes.TransportBooking }, iAutoRating.ChargeCodeGroups);

			if (SetupAndTestConsolCosts(transportJob))
			{
				var iAutoRatingWithConsolCosts = GetRatingAdapter(transportJob);
				AssertContainsExactElementsInAnyOrder(new[] { ChargeCodeGroupList.Codes.TransportBooking }, iAutoRatingWithConsolCosts.ChargeCodeGroups);
				AssertEquals(ChargeCodeFilter.AutorateNonConsolLevelOnly, iAutoRatingWithConsolCosts.ChargeCodeGroups.CostChargesFilter);
			}
		}

		protected abstract bool SetupAndTestConsolCosts(T transport);

		public abstract void TestAdapterTypeAndID();

		#endregion

		#region TestIAutoRating_ConsumerType

		public void TestIAutoRating_ConsumerType()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertEquals(ExpectedConsumerType, iAutoRating.ConsumerType);
		}

		protected abstract JobInvoicingConsumerType ExpectedConsumerType { get; }

		#endregion

		#region TestIAutoRating_Properties

		public virtual void TestIAutoRating_Properties()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertEquals(MergeChargeOptions.WithinAdapter, iAutoRating.MergeCharges);
			AssertEquals(RateType.TransportBookings, iAutoRating.RateTypeToUse);
			AssertEquals(0, iAutoRating.JobServices.Count);
		}

		#endregion

		#region TestIAutoRating_StatusInformation

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

			void AssertJobDates(DtbTransportInstruction transportInstruction, Action<DtbTransportConfirmation, ZDateTime> action, string expectedError)
			{
				var confirmation1 = (DtbTransportConfirmation)transportInstruction.Confirmations.AddNew();
				action(confirmation1, ZDateTime.Now);
				var confirmation2 = (DtbTransportConfirmation)transportInstruction.Confirmations.AddNew();
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

		#endregion

		#endregion

		#region IAutoRatingLocations Members

		#region TestIAutoRatingLocations_Origin

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

		#endregion

		#region TestIAutoRatingLocations_Properties

		public void TestIAutoRatingLocations_Properties()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertNull(iAutoRating.Destination);
			AssertNull(iAutoRating.GetVia(CostSell.Cost));
			AssertNull(iAutoRating.GetVia(CostSell.Revenue));
		}

		#endregion

		#endregion

		#region IAutoRatingOrganisations Members

		#region TestAutoRatingOrganisations_Carrier

		public void TestIAutoRatingOrganisations_Carrier()
		{
			TestIAutoRatingOrganisations_CarrierCore();
		}

		protected abstract void TestIAutoRatingOrganisations_CarrierCore();

		#endregion

		#region TestIAutoRatingOrganisations_TransportProviders

		public void TestIAutoRatingOrganisations_TransportProviders()
		{
			TestIAutoRatingOrganisations_TransportProvidersCore();
		}

		protected abstract void TestIAutoRatingOrganisations_TransportProvidersCore();

		#endregion

		#endregion

		#region IAutoRatingFreightInfo Members

		#region TestIAutoRatingOrganisations_Properties

		public void TestIAutoRatingOrganisations_Properties()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertEquals(FreightMode.LRO, iAutoRating.FreightMode);
			AssertEquals("", iAutoRating.HousebillReleaseType);
			AssertNull(iAutoRating.PaymentTerm);

			AssertNull(iAutoRating.WharfCTOAddress);
		}

		#endregion

		#region TestIAutoRatingOrganisations_Measures

		#region TestNoDivideByZeroErrors

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

		#endregion

		#region TestIAutoRatingOrganisations_Measures_LRO

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

		#endregion

		#region TestIAutoRatingOrganisations_Measures_FRO

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

		#endregion

		#region TestIAutoRatingOrganisations_Measures_FRO_WeightAssertion

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

		#region TestIAutoRatingOrganisations_Measures_InvalidUnits

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

		#endregion

		#endregion

		#region Implementation

		protected virtual void AddCarton(T transportJob, RefCommodityCode commodity)
		{
			CreatePackage(transportJob, 5, Constants.PkgUnit.Carton, commodity, 50m, Constants.Weight.Kilograms, 0.30m, Constants.Volume.CubicMetres);
		}

		protected virtual void CreateDivots(DtbTransportInstruction fromInstruction, PkgPackage container, PkgPackage box, PkgPackage pallet, DtbTransportInstruction toInstruction)
		{
			CreateInstructionPkgDivots(fromInstruction, container, box, pallet);
			CreateInstructionPkgDivots(toInstruction, container, box, pallet);
		}

		protected RefCommodityCode CreateCommodity(ZString code)
		{
			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = code;

			return commodity;
		}

		protected PkgPackage CreatePackage(T transportJob, int quantity, string quantityUQ, RefCommodityCode commodity, decimal weight, string weightUQ, decimal volume, string volumeUQ)
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

		protected void CreateInstructionPkgDivots(DtbTransportInstruction instruction, params PkgPackage[] packages)
		{
			instruction.PackageDivots.DeleteAll();

			foreach (var package in packages)
			{
				instruction.DivotsWithPackages.AddPackage(package);
			}
		}

		void SetupJobInstructions(T job, bool hasPallet, bool hasContainer)
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

		#endregion

		#endregion

		#region TestIAutoRatingOrganisations_ServiceLevel

		public void TestIAutoRatingOrganisations_ServiceLevel()
		{
			TestIAutoRatingOrganisations_ServiceLevelCore();
		}

		protected abstract void TestIAutoRatingOrganisations_ServiceLevelCore();

		#endregion

		#endregion

		#region IAutoRatingDescriptionMacroExpander Members

		#region TestCanExpandMacros

		public void TestCanExpandMacros()
		{
			var transportJob = GetTransportJob();
			var iExpander = (IAutoRatingDescriptionMacroExpander)GetRatingAdapter(transportJob);
			AssertEquals(true, iExpander.CanExpandMacros);
		}

		#endregion

		#region TestExpandMacro_Empty

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

		#endregion

		#region TestExpandMacro

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

		#endregion

		#endregion

		#region TestAutoRatingOrganisations ImportBroker

		public void TestAutoRatingOrganisationsImportBroker()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertNull(iAutoRating.ImportBroker);
		}

		#endregion

		#region TestAutoRatingOrganisations ExportBroker

		public void TestAutoRatingOrganisationsExportBroker()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertNull(iAutoRating.ExportBroker);
		}

		#endregion

		#region IImportExport Members

		public void TestIImportExport_IsImport()
		{
			var transportJob = GetTransportJob();
			var iAutoRating = GetRatingAdapter(transportJob);
			AssertEquals(Directions.Unknown, iAutoRating.JobDirection);
		}

		#endregion

		#endregion

		#region IJobNumber Members

		public void TestJobNumber()
		{
			var transportJob = GetTransportJob();
			var iJobNumber = (IJobNumber)GetRatingAdapter(transportJob);
			transportJob.KM_JobID = "Test";
			AssertEquals("Test", iJobNumber.JobNumber);
		}

		#endregion

		#region Implementation

		protected abstract T GetTransportJob();
		protected abstract IAutoRating GetRatingAdapter(T transportJob);
		protected abstract IAutoRating GetRatingAdapterFRO(T transportJob);
		protected abstract DtbTransportInstruction CreateFromInstruction(T transport);
		protected abstract DtbTransportInstruction CreateToInstruction(T transport);
		protected abstract PkgPackageJob GetPackageJob(T transportJob);

		#region PackingHelper

		protected PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion

		#endregion
	}
}
