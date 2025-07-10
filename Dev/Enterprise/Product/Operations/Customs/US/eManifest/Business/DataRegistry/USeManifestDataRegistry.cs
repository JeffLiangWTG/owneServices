using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	public sealed class USeManifestDataRegistry : RegistryItemSet
	{
		USeManifestDataRegistry() { }

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_UnitedStatesofAmerica { get { return CombineCategories(Customs_CountryOrRegion, (NoResString)"United States of America"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_eManifest { get { return CombineCategories(Customs_UnitedStatesofAmerica, (NoResString)"e-Manifest"); } }
		}

		#endregion

		#region Number Customisation

		public BillCustomisationRegistryItem NumberCustomisation
		{
			get
			{
				return GetItem(
					"eManifestNumberCustomisation",
					() => new BillCustomisationRegistryItem(
							"eManifestNumberCustomisation",
							Categories.Customs_UnitedStatesofAmerica_eManifest,
							ResString.GetMultilingualString("97492E88-36C7-4078-89F7-74D761D1C524", "e-Manifest Number Customization"),
							ResString.GetMultilingualString("38822F64-EBA6-4022-99E5-E14E67A94D25", "Override this value to customize how e-Manifest numbers are formatted"),
							RegistryStorageFlags.All,
							new TripCustomisationRegistryDataType()));
			}
		}

		#endregion

		#region Maximum shipments to be sent in one message

		public IntRegistryItem MaximumShipmentsToSendInOneMessage
		{
			get
			{
				return GetItem(
					"eManifestMaximumShipmentsToSendInOneMessage",
					() => new IntRegistryItem(
							"eManifestMaximumShipmentsToSendInOneMessage",
							Categories.Customs_UnitedStatesofAmerica_eManifest,
							ResString.GetMultilingualString("11664F8C-B709-42D3-9B9E-5990BC8B01C5", "Maximum Shipments To Send in One Message"),
							ResString.GetMultilingualString("4B1831BE-18D4-460C-83AE-643AFEF384DD", "Override this value to customize the maximum shipments to be sent in one message."),
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							3000,
							1,
							5000));
			}
		}

		#endregion

		#region Implementation

		public static USeManifestDataRegistry Instance
		{
			get { return instance ?? (instance = new USeManifestDataRegistry()); }
		}

		[ThreadStatic]
		static USeManifestDataRegistry instance;

		#endregion
	}
}
