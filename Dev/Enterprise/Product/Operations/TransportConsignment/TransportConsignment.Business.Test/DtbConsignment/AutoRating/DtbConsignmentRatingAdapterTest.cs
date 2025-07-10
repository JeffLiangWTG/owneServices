using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRatingAdapterTest : TestCaseWithFactory
	{
		#region IAutoRating Members

		#region TestIAutoRating_ChargeCodeGroups

		public void TestIAutoRating_ChargeCodeGroups()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);
			AssertContainsExactElementsInAnyOrder(new[] { ChargeCodeGroupList.Codes.TransportBooking }, iAutoRating.ChargeCodeGroups);

			if (SetupAndTestConsolCosts(consignment))
			{
				var iAutoRatingWithConsolCosts = GetIAutoRating(consignment);
				AssertContainsExactElementsInAnyOrder(new[] { ChargeCodeGroupList.Codes.TransportBooking }, iAutoRatingWithConsolCosts.ChargeCodeGroups);
				AssertEquals(ChargeCodeFilter.AutorateNonConsolLevelOnly, iAutoRatingWithConsolCosts.ChargeCodeGroups.CostChargesFilter);
			}
		}

		bool SetupAndTestConsolCosts(DtbConsignment consignment)
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			var fromOrganization = Helper.CreateOrganisation("FORM");
			var toOrganization = Helper.CreateOrganisation("TO");
			var fromAddress = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypeList.Codes.PickUp);
			fromAddress.Address.OrganisationPK = fromOrganization.PK;
			var toAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			toAddress.Address.OrganisationPK = toOrganization.PK;

			Helper.CreateRunSheetInstruction(runSheet, new[] { consignment.PickupAddress.PickupAction });

			var cost = (BusinessObject)Factory.New<IJobConsolCost>();
			cost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			cost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				cost[JobConsolCostSchema.E6_ParentID] = runSheet.PK;
				cost[JobConsolCostSchema.E6_ParentTableCode] = runSheet.TablePrefix;
			}
			finally
			{
				cost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			return true;
		}

		#endregion

		#region TestIAutoRating_ConsumerType

		public void TestIAutoRating_ConsumerType()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);
			AssertEquals(JobInvoicingConsumerTypes.TransportConsignment, iAutoRating.ConsumerType);
		}

		#endregion

		#region TestIAutoRating_Properties

		public void TestIAutoRating_Properties()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);
			AssertEquals(MergeChargeOptions.WithinAdapter, iAutoRating.MergeCharges);
			AssertEquals(RateType.TransportBookings, iAutoRating.RateTypeToUse);
			AssertEquals(15, iAutoRating.JobServices.Count);
		}

		#endregion

		#region TestIAutoRating_StatusInformation

		public void TestIAutoRating_StatusInformation()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);
			var expectedErrorMessage = ZString.Format("{0} doesn't have at least 2 Addresses. Cannot continue.", consignment.HumanReadableName);

			var info1 = iAutoRating.StatusInformation;
			AssertEquals("Precondition", false, info1.CanExecute);
			AssertEquals("Precondition", expectedErrorMessage, info1.Message);

			var fromAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var info2 = iAutoRating.StatusInformation;
			AssertEquals(false, info2.CanExecute);
			AssertEquals(expectedErrorMessage, info2.Message);

			var toAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			iAutoRating = GetIAutoRating(consignment);
			var info3 = iAutoRating.StatusInformation;
			AssertEquals(true, info3.CanExecute);
			AssertEquals("", info3.Message);
		}

		#endregion

		#region TestIAutoRating_Time

		public void TestIAutoRating_Time()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = Env.CurrentCompanyPK;
			chargeCode.AC_Desc = "Consignment Fumigation";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.TransportBooking;
			chargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.Fumigation;

			var service = consignment.Services.AddNew();
			service.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			service.ES_Completed = ZDateTime.Today;
			service.ES_Duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2);

			AssertEquals(2d, iAutoRating.JobServices.Time(chargeCode).Span.TotalHours);
		}

		#endregion

		public void TestAdapterTypeAndID()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var adapter = GetIAutoRating(consignment);

			AssertEquals(AdapterType.TransportConsignment, adapter.AdapterType);
			AssertEquals(consignment.LTC_JobID, adapter.OperationalJobCode);
		}

		#endregion

		#region IAutoRatingLocations Members

		#region TestIAutoRatingLocations_Origin

		public void TestIAutoRatingLocations_Origin()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);
			AssertNull("Precondition", iAutoRating.Origin);

			var fromAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			iAutoRating = GetIAutoRating(consignment);
			AssertNull(iAutoRating.Origin);

			var address = Factory.New<OrgAddress>();
			fromAddress.Address.E2_OA_Address = address.PK;
			AssertNull(iAutoRating.Origin);

			address.OA_RL_NKRelatedPortCode = "AUSYD";
			AssertEquals("AUSYD", iAutoRating.Origin.Code);

			fromAddress.Address.E2_AddressOverride = true;
			fromAddress.Address.E2_RN_NKCountryCode = "NZ";
			AssertEquals("NZ", iAutoRating.Origin.Code);
		}

		#endregion

		#region TestIAutoRatingLocations_Properties

		public void TestIAutoRatingLocations_Properties()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);
			AssertNull(iAutoRating.Destination);
			AssertNull(iAutoRating.GetVia(CostSell.Cost));
			AssertNull(iAutoRating.GetVia(CostSell.Revenue));
		}

		#endregion

		#endregion

		#region IImportExport Members

		public void TestIImportExport_IsImport()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);
			AssertEquals(Directions.Unknown, iAutoRating.JobDirection);
		}

		#endregion

		#region IAutoRatingOrganisations Members

		#region TestAutoRatingOrganisations_Carrier

		public void TestIAutoRatingOrganisations_Carrier()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy, iAutoRating.Carrier);
		}

		#endregion

		#region TestAutoRatingOrganisations_Consignor

		public void TestAutoRatingOrganisations_Consignor()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);
			AssertNull("Precondition", iAutoRating.DebtorOrgs[RatingDebtorOrgTypes.CNR]);
			AssertNull("Precondition", iAutoRating.PickupAddress);
			AssertEquals("Precondition", "", iAutoRating.PickupCartageEquipment);

			var fromAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			iAutoRating = GetIAutoRating(consignment);
			AssertNull(iAutoRating.DebtorOrgs[RatingDebtorOrgTypes.CNR]);
			AssertEquals(fromAddress.Address, iAutoRating.PickupAddress);
			AssertEquals("", iAutoRating.PickupCartageEquipment);

			var address = Factory.New<OrgAddress>();
			fromAddress.Address.E2_OA_Address = address.PK;
			AssertNull(iAutoRating.DebtorOrgs[RatingDebtorOrgTypes.CNR]);
			AssertEquals(fromAddress.Address, iAutoRating.PickupAddress);
			AssertEquals("PSL", iAutoRating.PickupCartageEquipment);

			var orgHeader = Factory.New<OrgHeader>();
			address.OA_OH = orgHeader.PK;
			fromAddress.LTS_DropMode = "ABC";
			AssertEquals(orgHeader, iAutoRating.DebtorOrgs[RatingDebtorOrgTypes.CNR]);
			AssertEquals(fromAddress.Address, iAutoRating.PickupAddress);
			AssertEquals("ABC", iAutoRating.PickupCartageEquipment);
		}

		#endregion

		#region TestAutoRatingOrganisations_Consignee

		public void TestAutoRatingOrganisations_Consignee()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);
			AssertNull("Precondition", iAutoRating.DebtorOrgs[RatingDebtorOrgTypes.CNE]);
			AssertNull("Precondition", iAutoRating.DeliveryAddress);
			AssertEquals("Precondition", "", iAutoRating.DeliveryCartageEquipment);

			var fromAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var toAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			iAutoRating = GetIAutoRating(consignment);
			AssertNull(iAutoRating.DebtorOrgs[RatingDebtorOrgTypes.CNE]);
			AssertEquals(toAddress.Address, iAutoRating.DeliveryAddress);
			AssertEquals("", iAutoRating.DeliveryCartageEquipment);

			var address = Factory.New<OrgAddress>();
			address.OA_LCLEquipmentNeeded = "PSL";
			toAddress.Address.E2_OA_Address = address.PK;
			AssertNull(iAutoRating.DebtorOrgs[RatingDebtorOrgTypes.CNE]);
			AssertEquals(toAddress.Address, iAutoRating.DeliveryAddress);
			AssertEquals("PSL", iAutoRating.DeliveryCartageEquipment);

			var orgHeader = Factory.New<OrgHeader>();
			address.OA_OH = orgHeader.PK;
			toAddress.LTS_DropMode = "ABC";
			AssertEquals(orgHeader, iAutoRating.DebtorOrgs[RatingDebtorOrgTypes.CNE]);
			AssertEquals(toAddress.Address, iAutoRating.DeliveryAddress);
			AssertEquals("ABC", iAutoRating.DeliveryCartageEquipment);
		}

		#endregion

		#region TestIAutoRatingOrganisations_TransportProvidersCore

		public void TestIAutoRatingOrganisations_TransportProvidersCore()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var picAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var dlvAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);

			var picAction = picAddress.Actions.AddNew();
			var dlvAction = dlvAddress.Actions.AddNew();

			IAutoRating iAutoRating = new DtbConsignmentRatingAdapter(consignment, FreightMode.LRO);
			AssertEquals("Precondition", 0, iAutoRating.Creditors.AllOrgs.Count);

			var transportCo1 = Helper.CreateOrganisation("TC1");
			var runSheet1 = Helper.CreateRunSheet(transportCo1);
			Helper.CreateRunSheetInstruction(picAction, runSheet1.PK);

			var runSheet2 = Helper.CreateRunSheet();
			Helper.CreateRunSheetInstruction(dlvAction, runSheet2.PK);
			AssertContainsExactElementsInAnyOrder(new[] { transportCo1 }, iAutoRating.Creditors.AllOrgs);

			var transportCo2 = Helper.CreateOrganisation("TC2");
			runSheet2.KG_OH_TransportCo = transportCo2.PK;
			AssertContainsExactElementsInAnyOrder(new[] { transportCo1, transportCo2 }, iAutoRating.Creditors.AllOrgs);

			runSheet2.KG_OH_TransportCo = transportCo1.PK;
			AssertContainsExactElementsInAnyOrder(new[] { transportCo1 }, iAutoRating.Creditors.AllOrgs);
		}

		#endregion

		#region TestIAutoRatingOrganisations_Properties

		public void TestIAutoRatingOrganisations_Properties()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);
			AssertEquals(FreightMode.LRO, iAutoRating.FreightMode);
			AssertEquals("", iAutoRating.HousebillReleaseType);
			AssertNull(iAutoRating.PaymentTerm);
			AssertNull(iAutoRating.WharfCTOAddress);
		}

		#endregion

		#region TestIAutoRatingOrganisations_ServiceLevel

		public void TestIAutoRatingOrganisations_ServiceLevel()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);
			AssertEquals(2, iAutoRating.ServiceLevel.ServiceLevelData.Length);
			iAutoRating.ServiceLevel.ServiceLevelData.Single(s => s.ServiceLevelType == ServiceLevelType.Client && s.ServiceLevel == "");
			iAutoRating.ServiceLevel.ServiceLevelData.Single(s => s.ServiceLevelType == ServiceLevelType.Carrier && s.ServiceLevel == "");

			consignment.LTC_RS_NKServiceLevel = "BOB";
			AssertEquals(2, iAutoRating.ServiceLevel.ServiceLevelData.Length);
			iAutoRating.ServiceLevel.ServiceLevelData.Single(s => s.ServiceLevelType == ServiceLevelType.Client && s.ServiceLevel == "BOB");
			iAutoRating.ServiceLevel.ServiceLevelData.Single(s => s.ServiceLevelType == ServiceLevelType.Carrier && s.ServiceLevel == "BOB");
		}

		#endregion

		#region TestAutoRatingOrganisations ImportBroker

		public void TestAutoRatingOrganisationsImportBroker()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);
			AssertNull(iAutoRating.ImportBroker);
		}

		#endregion

		#region TestAutoRatingOrganisations ExportBroker

		public void TestAutoRatingOrganisationsExportBroker()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iAutoRating = GetIAutoRating(consignment);
			AssertNull(iAutoRating.ExportBroker);
		}

		#endregion

		#endregion

		#region IAutoRatingFreightInfo Members

		#region Measures

		#region TestNoDivideByZeroErrors

		public void TestNoDivideByZeroErrors()
		{
			// 0x container	 300 KG	  15 M3
			// 0x BOX		  30 KG	   1 M3
			// 0x PLT		  20 KG	0.15 M3
			var consignment = Helper.CreateConsignment("LTC001");
			var commodity = CreateCommodity("AAA");
			var container = CreatePackage(consignment, 0, Constants.PkgUnit.Container, commodity, 300m, Constants.Weight.Kilograms, 15m, Constants.Volume.CubicMetres);
			var box = CreatePackage(consignment, 0, Constants.PkgUnit.Box, commodity, 30m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);
			var pallet = CreatePackage(consignment, 0, Constants.PkgUnit.Pallet, commodity, 20m, Constants.Weight.Kilograms, 0.15m, Constants.Volume.CubicMetres);

			var fromAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var toAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);

			var iAutoRating = GetIAutoRating(consignment);
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			// Packages
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.Package));
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
			var consignment = Helper.CreateConsignment("LTC001");
			var commodity = CreateCommodity("AAA");
			var container = CreatePackage(consignment, 1, Constants.PkgUnit.Container, commodity, 300m, Constants.Weight.Kilograms, 15m, Constants.Volume.CubicMetres);
			var box = CreatePackage(consignment, 3, Constants.PkgUnit.Box, commodity, 30m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);
			var innerBox = CreatePackage(box, consignment, 10, Constants.PkgUnit.Box, commodity, 5m, Constants.Weight.Kilograms, 0.1m, Constants.Volume.CubicMetres);
			var pallet = CreatePackage(consignment, 2, Constants.PkgUnit.Pallet, commodity, 20m, Constants.Weight.Kilograms, 0.15m, Constants.Volume.CubicMetres);

			var fromAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var toAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);

			var iAutoRating = GetIAutoRating(consignment);
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			// Packages
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.Package));
			AssertContainsExactElementsInAnyOrder(new[] { "AAA" }, rateableMeasures.GetPackageUniqueCommodities());
			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Box, Constants.PkgUnit.Pallet }, rateableMeasures.GetPackageUniquePackageTypes());

			// Weight
			AssertEquals(55m, rateableMeasures.GetActual(MeasureType.Weight));
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
			var consignment = Helper.CreateConsignment("LTC001");
			SetupPackages(consignment, hasPallet: false, hasContainer: true);
			var commodity = CreateCommodity("AAA");
			var container = consignment.Containers.First();
			container.KP_PackageID = "CONT1";
			container.KP_RH_NKCommodityCode = "AAA";
			var iAutoRating = GetRatingAdapterFRO(consignment);

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
			var consignment = Helper.CreateConsignment("LTC001");
			SetupPackages(consignment, hasPallet: false, hasContainer: true);
			var iAutoRating = GetRatingAdapterFRO(consignment);
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			var containerInfos = rateableMeasures.GetAllContainers().ToList();
			AssertEquals("Container Count", 1, containerInfos.Count);
			AssertEquals("Container Gross Weight should be added to the MeasureType.Weight.", 1000m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals("Container Volume should be added to the MeasureType.Volume.", 1111m, rateableMeasures.GetActual(MeasureType.Volume));
		}

		public void TestIAutoRatingOrganisations_FRO_WeightAssertion_ContainerAndPallet()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			SetupPackages(consignment, hasPallet: true, hasContainer: true);
			var iAutoRating = GetRatingAdapterFRO(consignment);
			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals("Container Gross Weight should be added to the MeasureType.Weight.", 1020m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals("Container Volume should be added to the MeasureType.Volume.", 1111m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals("Container Count", 1, rateableMeasures.GetAllContainers().Count());
		}

		public void TestIAutoRatingOrganisations_WeightAssertion_PalletOnly()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			SetupPackages(consignment, hasPallet: true, hasContainer: false);
			var iAutoRating = GetRatingAdapterFRO(consignment);
			AssertEquals("Pallet Only weight", 20m, ((RateableMeasureSet)iAutoRating.RateableMeasures).GetActual(MeasureType.Weight));
		}

		#endregion

		#region IAutoRatingOrganisations_Measures_PickupDistance

		public void TestIAutoRatingOrganisations_Measures_PickupDistance()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);

			consignment.LTC_Distance = 100m;
			consignment.LTC_DistanceUnit = Constants.Length.Kilometres;
			var iAutoRating = GetIAutoRating(consignment);

			var rateableMeasures = (RateableMeasureSet)iAutoRating.RateableMeasures;
			AssertEquals(100m, rateableMeasures.GetActual(MeasureType.PickupDistance));
			AssertEquals("KM", rateableMeasures.GetUnit(MeasureType.PickupDistance));
		}

		#endregion
		#region Helper Funcitons

		RefCommodityCode CreateCommodity(ZString code)
		{
			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = code;

			return commodity;
		}

		PkgPackage CreatePackage(DtbConsignment consignment, int quantity, string quantityUQ, RefCommodityCode commodity, decimal weight, string weightUQ, decimal volume, string volumeUQ)
		{
			var package = Factory.New<PkgPackage>();
			return CreatePackage(package, consignment, quantity, quantityUQ, commodity, weight, weightUQ, volume, volumeUQ);
		}

		PkgPackage CreatePackage(PkgPackage package, DtbConsignment consignment, int quantity, string quantityUQ, RefCommodityCode commodity, decimal weight, string weightUQ, decimal volume, string volumeUQ)
		{
			var innerPackage = Factory.New<PkgPackage>();
			if (package != null)
			{
				innerPackage.KP_KP_ParentPackage = package.PK;
			}

			innerPackage.KP_PackageQty = quantity;
			innerPackage.KP_F3_NKPackType = quantityUQ;
			if (innerPackage.IsContainer)
			{
				innerPackage.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			}
			innerPackage.KP_Weight = weight;
			innerPackage.KP_WeightUQ = weightUQ;
			innerPackage.KP_Volume = volume;
			innerPackage.KP_VolumeUQ = volumeUQ;
			innerPackage.KP_RH_NKCommodityCode = commodity != null ? commodity.RH_Code : ZString.Empty;
			innerPackage.KP_KJ_ParentPackageJob = consignment.PackageJob.PK;

			return innerPackage;
		}

		void SetupPackages(DtbConsignment consignment, bool hasPallet, bool hasContainer)
		{
			if (hasContainer)
			{
				CreatePackage(consignment, 1, Constants.PkgUnit.Container, null, 1000m, Constants.Weight.Kilograms, 1111m, Constants.Volume.CubicMetres);
			}

			if (hasPallet)
			{
				var pallet = CreatePackage(consignment, 2, Constants.PkgUnit.Pallet, null, 20m, Constants.Weight.Kilograms, 0.15m, Constants.Volume.CubicMetres);
			}
		}

		#endregion

		#endregion

		#region MonetaryValues

		public void TestMoneyType()
		{
			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.Australia);
			var nzdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.NewZealand);

			var consignment = Helper.CreateConsignment("LTC001");
			consignment.LTC_GoodsValue = 150;
			consignment.LTC_RX_NKGoodsValueCurrency = audCurrency.RX_Code;
			consignment.LTC_InsuranceValue = 160;
			consignment.LTC_RX_NKInsuranceValueCurrency = nzdCurrency.RX_Code;

			var ratingAdapter = GetIAutoRating(consignment);
			var monetaryValues = ratingAdapter.MonetaryValues;

			AssertNotNull("Collections shouldn't be null by default", monetaryValues);
			AssertEquals(1, monetaryValues.Values[MoneyType.ValueType.GoodsValue].Count);
			AssertEquals(1, monetaryValues.Values[MoneyType.ValueType.InsuranceValue].Count);
			AssertEquals(new Money(150, audCurrency), monetaryValues.Values[MoneyType.ValueType.GoodsValue].FirstOrDefault());
			AssertEquals(new Money(160, nzdCurrency), monetaryValues.Values[MoneyType.ValueType.InsuranceValue].FirstOrDefault());
		}

		#endregion

		#endregion

		#region IAutoRatingDescriptionMacroExpander Members

		#region TestCanExpandMacros

		public void TestCanExpandMacros()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iExpander = (IAutoRatingDescriptionMacroExpander)GetIAutoRating(consignment);
			AssertEquals(true, iExpander.CanExpandMacros);
		}

		#endregion

		#region TestExpandMacro_Empty

		public void TestExpandMacro_Empty()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var iExpander = (IAutoRatingDescriptionMacroExpander)GetIAutoRating(consignment);

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
			var consignment = Helper.CreateConsignment("LTC001");
			var fromAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var toAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);

			fromAddress.Address.E2_AddressOverride = true;
			fromAddress.Address.E2_CompanyName = "DoodyCo";
			fromAddress.Address.E2_Postcode = "2000";
			fromAddress.Address.E2_State = "NSW";
			fromAddress.Address.E2_City = "Alexandria";

			toAddress.Address.E2_AddressOverride = true;
			toAddress.Address.E2_CompanyName = "OrionCo";
			toAddress.Address.E2_Postcode = "3000";
			toAddress.Address.E2_State = "QLD";
			toAddress.Address.E2_City = "Rockhampton";

			var iExpander = (IAutoRatingDescriptionMacroExpander)GetIAutoRating(consignment);

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

		#region IAutoRatingFreightConditionsSupportable Members

		public void TestConditionsSupporter()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			AssertType<DtbConsignmentRateLineConditionsSupporter>(((IAutoRatingFreightConditionsSupportable)GetIAutoRating(consignment)).ConditionsSupporter);
		}

		#endregion

		#region IJobNumber Members

		public void TestJobNumber()
		{
			var consignment = Helper.CreateConsignment();
			var iJobNumber = (IJobNumber)GetIAutoRating(consignment);
			consignment.LTC_JobID = "Test";
			AssertEquals("Test", iJobNumber.JobNumber);
		}

		#endregion

		#region Implementation

		IAutoRating GetIAutoRating(DtbConsignment consignment)
		{
			return new DtbConsignmentRatingAdapter(consignment, FreightMode.LRO);
		}

		IAutoRating GetRatingAdapterFRO(DtbConsignment consignment)
		{
			return new DtbConsignmentRatingAdapter(consignment, FreightMode.FRO);
		}

		#region Helper

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		#endregion

		#endregion
	}
}
