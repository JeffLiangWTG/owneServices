using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class CommissionLookupsForTest : CommissionLookups
	{
		protected CommissionLookupsForTest(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static IDisposable TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(bool value)
		{
			var previousNewDelegate = CommissionLookups.OverridableNewDelegate.Value;
			CommissionLookups.OverridableNewDelegate.Value = (factory) => new CommissionLookupsWithOverridableShouldShowServicesAndSubModulesForTest(factory, value);
			return new DisposableAction(() =>
			{
				CommissionLookups.OverridableNewDelegate.Value = previousNewDelegate;
			});
		}

		public static IDisposable TemporarilyOverrideListsForTesting(string[] products, string[] services, string[] subModules)
		{
			var previousNewDelegate = CommissionLookups.OverridableNewDelegate.Value;
			var productList = new CodeDescriptionPairList();
			foreach (var product in products)
			{
				productList.AddPair(product, product);
			}

			var serviceList = new CodeDescriptionPairList();
			foreach (var service in services)
			{
				serviceList.AddPair(service, service);
			}

			var subModuleList = new CodeDescriptionPairList();
			foreach (var subModule in subModules)
			{
				subModuleList.AddPair(subModule, subModule);
			}

			CommissionLookups.OverridableNewDelegate.Value = (factory) => new CommissionLookupsWithOverridableListsForTest(factory, productList, serviceList, subModuleList);

			return new DisposableAction(() =>
			{
				CommissionLookups.OverridableNewDelegate.Value = previousNewDelegate;
			});
		}
	}
}
