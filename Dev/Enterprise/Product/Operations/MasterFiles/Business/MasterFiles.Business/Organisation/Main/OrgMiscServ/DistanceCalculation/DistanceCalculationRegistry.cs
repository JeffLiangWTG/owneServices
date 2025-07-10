using System;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public sealed class DistanceCalculationRegistry : RegistryItemSet
	{
		DistanceCalculationRegistry() { }

		#region Instance

		public static DistanceCalculationRegistry Instance
		{
			get { return fInstance ?? (fInstance = new DistanceCalculationRegistry()); }
		}

		[ThreadStatic]
		static DistanceCalculationRegistry fInstance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Freight_DistanceCalculationService { get { return CombineCategories(Freight, ResString.GetMultilingualString("f10d0eb2-009b-433a-992c-137b4e903edb", "Distance Calculation Service")); } }
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		public DistanceCalculationProviderConfigurationRegistryItem DistanceCalculationProviderConfigurationItem
		{
			get
			{
				return GetItem("DistanceCalculationProviderConfigurationItem", delegate
				{
					return new DistanceCalculationProviderConfigurationRegistryItem(
						"DistanceCalculationProviderConfigurationItem",
						Categories.Freight_DistanceCalculationService,
						(NoResString)"Distance Calculation Provider Configuration",
						(NoResString)"Specifies the default provider configuration that the CargoWise Distance Calculation service should use for distance calculation. By default, the CargoWise service will pick the most appropriate configuration(s) to use.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DistanceCalculationProviderConfiguration.GetDefault());
				});
			}
		}

		public StringRegistryItem DistanceCalculationServiceURL
		{
			get
			{
				return GetItem("DistanceCalculationServiceURL", delegate
				{
					return new StringRegistryItem(
						"DistanceCalculationServiceURL",
						Categories.Freight_DistanceCalculationService,
						(NoResString)"Distance Calculation Service URL",
						(NoResString)"The URL of the Distance Calculation Service.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						"https://webservices-ausyd.cargowise.net/DistanceCalculation/DistanceCalculationService.svc");
				});
			}
		}

		#endregion

		public CodePairRegistryItem DefaultDistanceUnit
		{
			get
			{
				var distancesListProvider = new CodeDescriptionPairListProvider(() => new DistanceUnitList());

				return GetItem("DefaultDistanceUnit", delegate
				{
					return new CodePairRegistryItem(
						"DefaultDistanceUnit",
						Categories.Freight_DistanceCalculationService,
						ResString.GetMultilingualString("1a90a964-7252-45a1-b49c-be942a6fb56d", "Default Distance Unit"),
						ResString.GetMultilingualString("60ee8a85-396a-4355-ab5d-3a734d9a9aab", "The default Distance Unit."),
						distancesListProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						Constants.Length.Kilometres);
				});
			}
		}
	}
}
