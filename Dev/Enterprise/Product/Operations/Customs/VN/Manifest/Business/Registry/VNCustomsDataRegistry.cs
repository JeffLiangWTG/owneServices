using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.VN.Manifest.Business
{
	public sealed class VNCustomsDataRegistry : RegistryItemSet, Integration.Customs.VN.IVNCustomsDataRegistry
	{
		#region Construction

		public static VNCustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new VNCustomsDataRegistry()); }
		}
		[ThreadStatic]
		static VNCustomsDataRegistry instance;

		#endregion

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Vietnam { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("EB5466A5-D40D-4983-A420-CE66BFEBFE06", "Vietnam")); } }			
		}

		#endregion

		public override bool IsForProductivityWise => false;

		public BooleanRegistryItem EnableVNManifest
		{
			get
			{
				var defaultValue = CustomsFeatureControlHelper.IsVietnamManifestFeatureEnabled;

				return GetItem("EnableVNManifest", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableVNManifest",
						Categories.Customs_Vietnam,
						ResString.GetMultilingualString("F822D5F6-DFCD-4E62-9CC7-3BB7A56246DE", "Enable Vietnam Manifest"),
						ResString.GetMultilingualString("D48B68D3-2DC3-4333-9239-7792B89CD627", "Enable Vietnam Manifest?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						defaultValue);
					return result;
				});
			}
		}

		IRegistryItem Integration.Customs.VN.IVNCustomsDataRegistry.EnableVNManifest => EnableVNManifest;
	}
}
