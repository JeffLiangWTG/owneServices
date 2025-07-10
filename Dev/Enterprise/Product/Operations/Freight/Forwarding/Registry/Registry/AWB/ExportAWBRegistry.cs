using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry.AWB
{
	public sealed class ExportAWBRegistry : RegistryItemSet
	{
		#region Construction

		public static ExportAWBRegistry Instance
		{
			get { return instance ?? (instance = new ExportAWBRegistry()); }
		}

		[ThreadStatic]
		static ExportAWBRegistry instance;

		ExportAWBRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Freight_AWB_MAWB_OtherCharges { get { return CombineCategories(Freight_AWB_MAWB, ResString.GetMultilingualString("a22c4a8e-7ed4-4446-8082-a560b66da3d5", "Other Charges")); } }
			public static MultilingualString Freight_AWB_HAWB_OtherCharges { get { return CombineCategories(Freight_AWB_HAWB, ResString.GetMultilingualString("ce26bd98-cdfd-4edb-a5bb-544baaeddf5d", "Other Charges")); } }
		}

		#endregion

		#region GroupOtherChargesByIATACode

		public BooleanRegistryItem MAWBGroupOtherChargesByIATACode
		{
			get
			{
				return GetItem("MAWBGroupOtherChargesByIATACode", delegate
				{
					return new BooleanRegistryItem(
						"MAWBGroupOtherChargesByIATACode",
						Categories.Freight_AWB_MAWB_OtherCharges,
						ResString.GetMultilingualString("61235234-1f9f-4a72-b8e4-c3a8717e2ac9", "Group By IATA Code"),
						ResString.GetMultilingualString("ca806f55-6289-423e-bca2-2b68371c8357", "Specifies whether the 'Other Charges' will be grouped by IATA Code on MAWB documents."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem HAWBGroupOtherChargesByIATACode
		{
			get
			{
				return GetItem("HAWBGroupOtherChargesByIATACode", delegate
				{
					return new BooleanRegistryItem(
						"HAWBGroupOtherChargesByIATACode",
						Categories.Freight_AWB_HAWB_OtherCharges,
						ResString.GetMultilingualString("4e3ec460-93b6-4fcc-90c0-abb22318e68e", "Group By IATA Code"),
						ResString.GetMultilingualString("b0c28ef9-cf1f-4182-bd0e-c1b829023cbd", "Specifies whether the 'Other Charges' will be grouped by IATA Code on HAWB documents."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region AWBDisplayOption

		#region HAWB

		public AWBDisplayOptionRegistryItem HAWBPrepaidDisplayOption
		{
			get
			{
				return GetItem("HAWBPrepaidDisplayOption", delegate
				{
					return new AWBDisplayOptionRegistryItem(
						"HAWBPrepaidDisplayOption",
						Categories.Freight_AWB_HAWB_OtherCharges,
						ResString.GetMultilingualString("abe03276-6e35-42bc-a4cd-1ec7eb2bd840", "Prepaid"),
						ResString.GetMultilingualString("a613ca6e-eeaa-4e90-ae9b-1e8ef522749b", "Specify display options for prepaid HAWB."),
						RegistryStorageFlags.Company,
						AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB));
				});
			}
		}

		public AWBDisplayOptionRegistryItem HAWBCollectDisplayOption
		{
			get
			{
				return GetItem("HAWBCollectDisplayOption", delegate
				{
					return new AWBDisplayOptionRegistryItem(
						"HAWBCollectDisplayOption",
						Categories.Freight_AWB_HAWB_OtherCharges,
						ResString.GetMultilingualString("804b72a8-43eb-48ac-858b-cc594a2e56b2", "Collect"),
						ResString.GetMultilingualString("1f12d4f2-211a-4ebe-ac16-6ee7f23a96f2", "Specify display options for collect HAWB."),
						RegistryStorageFlags.Company,
						AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB));
				});
			}
		}

		#endregion

		#region MAWB

		public AWBDisplayOptionRegistryItem MAWBPrepaidDisplayOption
		{
			get
			{
				return GetItem("MAWBPrepaidDisplayOption", delegate
				{
					return new AWBDisplayOptionRegistryItem(
						"MAWBPrepaidDisplayOption",
						Categories.Freight_AWB_MAWB_OtherCharges,
						ResString.GetMultilingualString("34e35493-30d0-448b-ab80-7b67af057bfd", "Prepaid"),
						ResString.GetMultilingualString("3f3dc1d8-5e1d-432b-9152-2263ddc574b6", "Specify display options for prepaid MAWB."),
						RegistryStorageFlags.Company,
						AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB));
				});
			}
		}

		public AWBDisplayOptionRegistryItem MAWBCollectDisplayOption
		{
			get
			{
				return GetItem("MAWBCollectDisplayOption", delegate
				{
					return new AWBDisplayOptionRegistryItem(
						"MAWBCollectDisplayOption",
						Categories.Freight_AWB_MAWB_OtherCharges,
						ResString.GetMultilingualString("b54a1fe1-240f-4b95-8ec6-441c229d71b1", "Collect"),
						ResString.GetMultilingualString("c62927de-469f-4c21-9c7a-6ca04f0fe7f9", "Specify display options for collect MAWB."),
						RegistryStorageFlags.Company,
						AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB));
				});
			}
		}

		#endregion

		#endregion
	}
}
