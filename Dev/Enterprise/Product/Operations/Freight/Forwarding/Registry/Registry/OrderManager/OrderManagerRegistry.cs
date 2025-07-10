using System;
using CargoWise.Definitions;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Registry
{
	public sealed class OrderManagerRegistry : RegistryItemSet
	{
		#region Construction

		public static OrderManagerRegistry Instance
		{
			get { return instance ?? (instance = new OrderManagerRegistry()); }
		}

		[ThreadStatic] static OrderManagerRegistry instance;

		OrderManagerRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region AllowPlannedCargoInContainerLoadPlan

		public BooleanRegistryItem AllowPlannedCargoInContainerLoadPlan
		{
			get
			{
				var item = GetItem("AllowPlannedCargoInContainerLoadPlan", () => new BooleanRegistryItem(
					"AllowPlannedCargoInContainerLoadPlan",
					OrdersDataRegistry.Categories.Orders_AdvancedOrderManager,
					ResString.GetMultilingualString("bbaa8442-85b7-4489-96b6-8c0ba7f9c83b", "Allow planned cargo in Container Load Plan"),
					ResString.GetMultilingualString("797b054b-27b6-4629-8ca6-0577ac445f3e", @$"By default, planning of cargo received in the CFS is allowed. Change this setting to 'Yes' to also allow planning of cargo that have not yet received in the CFS.

Note: This registry only takes effect if feature code '{LicenceFeatureCodeList.Codes.AdvancedOrderManagerFeatureControl}' is active."),
					RegistryStorageFlags.System | RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
					AdvOrmFeatureHelper.IsEnabled ? RegistryOptions.Default : RegistryOptions.IsHidden,
					false,
					new BooleanRegistryDataType()));

				return item;
			}
		}

		#endregion

		#region SupplierBookingNumberFormat

		public BillCustomisationRegistryItem SupplierBookingNumberFormat
		{
			get
			{
				var datatype = new BillCustomisationRegistryDataType();
				datatype.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.SupplierBooking;
				datatype.FountainPrefix = "SB";
				datatype.GeneratedNumberName = ResString.GetMultilingualString("5459402e-8a74-64bc-4bbb-321a15f04e38", "Supplier Booking Number Format");
				datatype.SequenceNumberName = ResString.GetMultilingualString("ECC6D98B-3E36-451A-A24D-0263152E6716", "Shipment");
				datatype.MaxLength = JobSupplierBookingSchema.JSB_BookingId.MaxLength;

				return GetItem("SupplierBookingNumberFormat", delegate
				{
					return new BillCustomisationRegistryItem("SupplierBookingNumberFormat",
					OrdersDataRegistry.Categories.Orders_AdvancedOrderManager,
					ResString.GetMultilingualString("5459402e-8a74-64bc-4bbb-321a15f04e38", "Supplier Booking Number Format"),
					ResString.GetMultilingualString("10e172f2-b33b-7fa7-4c2d-11bb944c87a5", "Override this value to customize how Supplier Booking numbers are formatted"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					AdvOrmFeatureHelper.IsEnabled ? RegistryOptions.Default : RegistryOptions.IsHidden,
					datatype);
				});
			}
		}

		#endregion

		#region ContainerLoadListNumberFormat

		public BillCustomisationRegistryItem ContainerLoadListNumberFormat
		{
			get
			{
				var datatype = new BillCustomisationRegistryDataType()
				{
					Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.SupplierBooking,
					FountainPrefix = "LL",
					GeneratedNumberName = ResString.GetMultilingualString("1a1ca145-c8ec-424e-bff3-58b53603d6e0", "Container Load List Number Format"),
					SequenceNumberName = ResString.GetMultilingualString("ECC6D98B-3E36-451A-A24D-0263152E6716", "Shipment"),
					MaxLength = ContainerLoadListHeaderSchema.CLH_LoadListId.MaxLength
				};

				return GetItem("ContainerLoadListNumberFormat",
					() => new BillCustomisationRegistryItem(
						"ContainerLoadListNumberFormat",
						OrdersDataRegistry.Categories.Orders_AdvancedOrderManager,
						ResString.GetMultilingualString("da72ed4c-dfde-4769-bbc7-d2a43ecb831d", "Container Load List Number Format"),
						ResString.GetMultilingualString("bcca198e-c2fa-42e8-91b7-7c67b3fee39f", "Override this value to customize how Container Load List numbers are formatted"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AdvOrmFeatureHelper.IsEnabled ? RegistryOptions.Default : RegistryOptions.IsHidden,
						datatype)
				);
			}
		}

		#endregion

		#region Order Manager Request Mapping

		public OrderManagerRequestMappingRegistryItem OrderManagerRequestMapping
		{
			get
			{
				var defaultValues = new OrderManagerRequestMappingCollection();
				defaultValues.AddDefaultValues();
				var item = GetItem("OrderManagerRequestMapping", () =>
					new OrderManagerRequestMappingRegistryItem(
						"OrderManagerRequestMapping",
						OrdersDataRegistry.Categories.Orders_AdvancedOrderManager,
						ResString.GetMultilingualString("388e33d1-3db8-45a5-945f-fb03336ef292", "Requests Mapping"),
						ResString.GetMultilingualString("c4d52dc7-7722-4d5b-842f-426b6d4ae982", "Use this registry to map the existing requests to request type. After mapping, request will be created automatically when the related scenario occurs."),
						RegistryStorageFlags.System,
						AdvOrmFeatureHelper.IsEnabled ? RegistryOptions.Default : RegistryOptions.IsHidden,
						defaultValues));

				return item;
			}
		}

		#endregion

		#region EnableOrderExplorer

		public BooleanRegistryItem EnableOrderExplorer
		{
			get
			{
				var item = GetItem("EnableOrderExplorer", delegate
				{
					return new BooleanRegistryItem(
						"EnableOrderExplorer",
						OrdersDataRegistry.Categories.Orders_AdvancedOrderManager,
						ResString.GetMultilingualString("5fc868cc-5243-442a-9cfe-977187c4693c", "Enable Order Explorer"),
						ResString.GetMultilingualString("0040a1a6-87e3-46c5-bfcf-b8cc35948153", @$"By default the Order Explorer functionality is disabled. Change this setting to Yes to enable Order Explorer functionality.
You cannot enable this registry if the feature code {LicenceFeatureCodeList.Codes.AdvancedOrderManagerFeatureControl} is inactive."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false,
						new BooleanRegistryDataType());
				});

				return item;
			}
		}

		#endregion
	}
}
