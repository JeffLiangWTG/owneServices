using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.PE.Manifest.Business
{
	public sealed class PECustomsDataRegistry : RegistryItemSet, Integration.Customs.PE.IPECustomsDataRegistry
	{
		#region Construction

		public static PECustomsDataRegistry Instance => instance ?? (instance = new PECustomsDataRegistry());

		[ThreadStatic]
		static PECustomsDataRegistry instance;

		public PECustomsDataRegistry()
		{
		}

		#endregion

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Peru { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("93640DF1-3914-44A6-8311-8F0BA1697C42", "Peru")); } }
			public static MultilingualString Customs_Peru_Manifest { get { return CombineCategories(Customs_Peru, ResString.GetMultilingualString("B656A6DF-3488-4F85-BC11-280190FAAF12", "Manifest")); } }
		}

		#endregion

		#region IPECustomsDataRegistry Members

		IRegistryItem Integration.Customs.PE.IPECustomsDataRegistry.EnablePEManifests => EnablePEManifests;

		#endregion

		public override bool IsForProductivityWise => false;

		public BooleanRegistryItem EnablePEManifests
		{
			get
			{
				return GetItem("EnablePEManifests", delegate
				{
					var result = new BooleanRegistryItem(
						"EnablePEManifests",
						Categories.Customs_Peru,
						ResString.GetMultilingualString("899BCC12-DF7E-4E98-A6D9-417E218A4A5D", "Enable Peru Manifest"),
						ResString.GetMultilingualString("78AFF97E-093B-40E3-B506-8D2964131A6B", "Enable Peru Manifest?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}
	}
}
