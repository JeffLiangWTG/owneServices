using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(OrderManagerRegistry))]
	sealed class OrderManagerRegistryTest : RegistryItemSetTestCaseWithFactory<OrderManagerRegistry>
	{
		#region AllowPlannedCargoInContainerLoadPlan 

		public void TestAllowPlannedCargoInContainerLoadPlan_WhenAdvOrmFeatureIsEnabled()
		{
			TestAllowPlannedCargoInContainerLoadPlan(true, RegistryOptions.Default);
		}

		public void TestAllowPlannedCargoInContainerLoadPlan_WhenAdvOrmFeatureIsNotEnabled()
		{
			TestAllowPlannedCargoInContainerLoadPlan(false, RegistryOptions.IsHidden);
		}

		void TestAllowPlannedCargoInContainerLoadPlan(bool enableAdvOrmFeature, RegistryOptions expectedRegistryOptions)
		{
			AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, () =>
			{
				TestRegistryItem(ItemSet.AllowPlannedCargoInContainerLoadPlan,
				"AllowPlannedCargoInContainerLoadPlan",
				"Orders/Advanced Order Manager",
				"Allow planned cargo in Container Load Plan",
				$"By default, planning of cargo received in the CFS is allowed. Change this setting to 'Yes' to also allow planning of cargo that have not yet received in the CFS.\r\n\r\nNote: This registry only takes effect if feature code '{LicenceFeatureCodeList.Codes.AdvancedOrderManagerFeatureControl}' is active.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				expectedRegistryOptions,
				false);
			});
		}

		#endregion

		#region SupplierBookingNumberFormat

		public void TestSupplierBookingNumberFormat_WhenAdvOrmFeatureIsEnabled()
		{
			TestSupplierBookingNumberFormat(true, RegistryOptions.Default);
		}

		public void TestSupplierBookingNumberFormat_WhenAdvOrmFeatureIsNotEnabled()
		{
			TestSupplierBookingNumberFormat(false, RegistryOptions.IsHidden);
		}

		void TestSupplierBookingNumberFormat(bool enableAdvOrmFeature, RegistryOptions expectedRegistryOptions)
		{
			AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
			{
				Action<BillCustomisationRegistryDataType> assertions = (BillCustomisationRegistryDataType item) =>
				{
					AssertEquals("SB", item.FountainPrefix);
					AssertEquals("Supplier Booking Number Format", item.GeneratedNumberName);
					AssertEquals(20, item.MaxLength);
					AssertEquals(item.DefaultValue.Categories, NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.SupplierBooking);
					AssertEquals("NON", item.DefaultValue.CheckDigitAlgorithm);
				};

				TestRegistryItem(ItemSet.SupplierBookingNumberFormat,
					"SupplierBookingNumberFormat",
					"Orders/Advanced Order Manager",
					"Supplier Booking Number Format",
					"Override this value to customize how Supplier Booking numbers are formatted",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					expectedRegistryOptions,
					assertions);
			});
		}

		#endregion

		#region ContainerLoadListNumberFormat

		public void TestContainerLoadListNumberFormat_WhenAdvOrmFeatureIsEnabled()
		{
			TestContainerLoadListNumberFormat(true, RegistryOptions.Default);
		}

		public void TestContainerLoadListNumberFormat_WhenAdvOrmFeatureIsNotEnabled()
		{
			TestContainerLoadListNumberFormat(false, RegistryOptions.IsHidden);
		}

		void TestContainerLoadListNumberFormat(bool enableAdvOrmFeature, RegistryOptions expectedRegistryOptions)
		{
			AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
			{
				void assertions(BillCustomisationRegistryDataType item)
				{
					AssertEquals("LL", item.FountainPrefix);
					AssertEquals("Container Load List Number Format", item.GeneratedNumberName);
					AssertEquals(ContainerLoadListHeaderSchema.CLH_LoadListId.MaxLength, item.MaxLength);
					AssertEquals(item.DefaultValue.Categories, NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.SupplierBooking);
					AssertEquals(CheckDigitAlgorithmList.Codes.None, item.DefaultValue.CheckDigitAlgorithm);
				}

				TestRegistryItem(ItemSet.ContainerLoadListNumberFormat,
					"ContainerLoadListNumberFormat",
					"Orders/Advanced Order Manager",
					"Container Load List Number Format",
					"Override this value to customize how Container Load List numbers are formatted",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					expectedRegistryOptions,
					assertions);
			});
		}

		#endregion

		#region OrderManagerRequestMapping

		public void TestOrderManagerRequestMapping_WhenAdvOrmFeatureIsEnabled()
		{
			TestOrderManagerRequestMapping(true, RegistryOptions.Default);
		}

		public void TestOrderManagerRequestMapping_WhenAdvOrmFeatureIsNotEnabled()
		{
			TestOrderManagerRequestMapping(false, RegistryOptions.IsHidden);
		}

		void TestOrderManagerRequestMapping(bool enableAdvOrmFeature, RegistryOptions expectedRegistryOptions)
		{
			AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
			{
				TestGenericRegistryItem(ItemSet.OrderManagerRequestMapping,
				"OrderManagerRequestMapping",
				"Orders/Advanced Order Manager",
				"Requests Mapping",
				"Use this registry to map the existing requests to request type. After mapping, request will be created automatically when the related scenario occurs.",
				RegistryStorageFlags.System,
				expectedRegistryOptions);

				var defaultValue = GetNewItemSet().OrderManagerRequestMapping.DefaultValue;
				AssertEquals("DefaultValue.Count", 16, defaultValue.Count);
				AssertOrderManagerRequestMappingProperties(defaultValue[0], RequestMappingCodes.CargoDateVsShipmentWindow);
				AssertOrderManagerRequestMappingProperties(defaultValue[1], RequestMappingCodes.CargoDateVsExWorksDate);
				AssertOrderManagerRequestMappingProperties(defaultValue[2], RequestMappingCodes.CargoDateVsRequiredInStoreDate);
				AssertOrderManagerRequestMappingProperties(defaultValue[3], RequestMappingCodes.BookedQuantity);
				AssertOrderManagerRequestMappingProperties(defaultValue[4], RequestMappingCodes.TransportMode);
				AssertOrderManagerRequestMappingProperties(defaultValue[5], RequestMappingCodes.PortOfLoading);
				AssertOrderManagerRequestMappingProperties(defaultValue[6], RequestMappingCodes.Overweight);
				AssertOrderManagerRequestMappingProperties(defaultValue[7], RequestMappingCodes.MinimumVolume);
				AssertOrderManagerRequestMappingProperties(defaultValue[8], RequestMappingCodes.MaximumVolume);
				AssertOrderManagerRequestMappingProperties(defaultValue[9], RequestMappingCodes.PartialReceived);
				AssertOrderManagerRequestMappingProperties(defaultValue[10], RequestMappingCodes.FinalizeBookingWithOutstandingLines);
				AssertOrderManagerRequestMappingProperties(defaultValue[11], RequestMappingCodes.DispatchShortageForLooseCargo);
				AssertOrderManagerRequestMappingProperties(defaultValue[12], RequestMappingCodes.PackedQtyExceedsBooked);
				AssertOrderManagerRequestMappingProperties(defaultValue[13], RequestMappingCodes.BookedQuantityAllocatedPartially);
				AssertOrderManagerRequestMappingProperties(defaultValue[14], RequestMappingCodes.UnusedContainersAllocatedToBooking);
				AssertOrderManagerRequestMappingProperties(defaultValue[15], RequestMappingCodes.MinimumRequirementOfSeal);
			});
		}

		void AssertOrderManagerRequestMappingProperties(OrderManagerRequestMapping orderManagerRequestMapping, string request)
		{
			AssertEquals(request, orderManagerRequestMapping.Request);
			AssertEquals("", orderManagerRequestMapping.RequestType);
		}

		#endregion

		#region EnableOrderExplorer

		public void TestEnableOrderExplorer()
		{
			AdvOrmFeatureHelper.RunTestWith(isEnabled: true, () =>
			{
				TestRegistryItem(ItemSet.EnableOrderExplorer,
					"EnableOrderExplorer",
					"Orders/Advanced Order Manager",
					"Enable Order Explorer",
					$"By default the Order Explorer functionality is disabled. Change this setting to Yes to enable Order Explorer functionality.\r\nYou cannot enable this registry if the feature code {LicenceFeatureCodeList.Codes.AdvancedOrderManagerFeatureControl} is inactive.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false);
			});
		}

		#endregion

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "AllowPlannedCargoInContainerLoadPlan";
				yield return "SupplierBookingNumberFormat";
				yield return "ContainerLoadListNumberFormat";
				yield return "OrderManagerRequestMapping";
				yield return "EnableOrderExplorer";
			}
		}
	}
}
