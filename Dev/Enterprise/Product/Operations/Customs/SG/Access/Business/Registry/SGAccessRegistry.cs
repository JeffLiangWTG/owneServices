using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.Access.Business.Registry
{
	public sealed class SGAccessRegistry : RegistryItemSet
	{
		SGAccessRegistry() { }

		public override bool IsForProductivityWise => false;

		public static SGAccessRegistry Instance
		{
			get { return instance ?? (instance = new SGAccessRegistry()); }
		}

		[ThreadStatic]
		static SGAccessRegistry instance;

		public DateTimeRegistryItem OVRLiveEffectiveDate
		{
			get
			{
				var registryItem = GetItem("EffectiveOVRLiveDate", delegate
				{
					return new DateTimeRegistryItem(
						"EffectiveOVRLiveDate",
						SG.Registry.SGCustomsDataRegistry.Categories.Customs_Singapore_ACCESS,
						ResString.GetMultilingualString("A12240BE-2992-4C29-AF08-DEDAB5723D74", "Effective OVR Live Date"),
						ResString.GetMultilingualString("46963764-923E-431E-A11F-392620673426", "The date that the SG ACCESS requires the OVR SR data in the AIRPCM report."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						new DateTime(2023, 1, 1)
						);
				});
				return registryItem;
			}
		}

		public static ZBool IsOVRLiveEffective => ZDate.Today.CompareTo(Instance.OVRLiveEffectiveDate.Value) >= 0;
	}
}
