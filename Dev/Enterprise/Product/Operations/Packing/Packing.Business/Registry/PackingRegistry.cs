using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business
{
	public sealed class PackingRegistry : RegistryItemSet
	{
		#region Instance

		public static PackingRegistry Instance
		{
			get { return instance ?? (instance = new PackingRegistry()); }
		}

		PackingRegistry()
		{
		}

		[ThreadStatic]
		static PackingRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Packing { get { return ResString.GetMultilingualString("4bb61ad5-3681-4fb3-88cf-7ad5164843c7", "Packing"); } }
			public static MultilingualString Packing_PackingLabels { get { return CombineCategories(Packing, ResString.GetMultilingualString("1dfda121-7aca-4e39-95e9-acdea715f57e", "Packing Labels")); } }
			public static MultilingualString Packing_Pallets { get { return CombineCategories(Packing, ResString.GetMultilingualString("3aeda121-7aca-4e39-95e9-acdea715f57e", "Pallet Management")); } }
		}

		#endregion

		#region WeightUnit

		public CodePairRegistryItem WeightUnit
		{
			get
			{
				CodePairRegistryItem registryItem = GetItem("WeightUnit", delegate
				{
					return new CodePairRegistryItem
					(
						"WeightUnit",
						Categories.Packing,
						ResString.GetMultilingualString("e5ce03f4-1abf-44cd-9b36-6582ccee8df7", "Weight Unit"),
						ResString.GetMultilingualString("462d181f-b737-4f1c-8f70-10eb1c31214d", "The default Weight Unit for new Packages in Packing."),
						OLookUpEditType.Weight,
						RegistryStorageFlags.All,
						Constants.Weight.Kilograms
					);
				});

				return registryItem;
			}
		}

#if DEBUG
		public void SetWeightUnitForTest(string unit)
		{
			WeightUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unit);
		}
#endif

		#endregion

		#region VolumeUnit

		public CodePairRegistryItem VolumeUnit
		{
			get
			{
				CodePairRegistryItem registryItem = GetItem("VolumeUnit", delegate
				{
					return new CodePairRegistryItem
					(
						"VolumeUnit",
						Categories.Packing,
						ResString.GetMultilingualString("65b0a439-4ef8-4450-952a-cb298b5ffa43", "Volume Unit"),
						ResString.GetMultilingualString("8bb0153c-977e-4784-b7f2-42cac8801b07", "The default Volume Unit for new Packages in Packing."),
						OLookUpEditType.Volume,
						RegistryStorageFlags.All,
						Constants.Volume.CubicMetres
					);
				});

				return registryItem;
			}
		}

#if DEBUG
		public void SetVolumeUnitForTest(string unit)
		{
			VolumeUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unit);
		}
#endif

		#endregion

		#region DimensionUnit

		public CodePairRegistryItem DimensionUnit
		{
			get
			{
				CodePairRegistryItem registryItem = GetItem("DimensionUnit", delegate
				{
					return new CodePairRegistryItem
					(
						"DimensionUnit",
						Categories.Packing,
						ResString.GetMultilingualString("245c722c-0320-43f6-be4a-3522ee338858", "Dimension Unit"),
						ResString.GetMultilingualString("55217024-757d-4a64-a6c9-10b312974a57", "The default Dimension Unit for new Packages in Packing."),
						OLookUpEditType.Length,
						RegistryStorageFlags.All,
						Constants.Length.Metres
					);
				});

				return registryItem;
			}
		}

#if DEBUG
		public void SetDimensionUnitForTest(string unit)
		{
			DimensionUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unit);
		}
#endif
		#endregion

		#region OuterPackageUnit

		public CodePairRegistryItem OuterPackageUnit
		{
			get
			{
				CodePairRegistryItem registryItem = GetItem("PackageUnit", delegate
				{
					return new CodePairRegistryItem
					(
						"PackageUnit",
						Categories.Packing,
						ResString.GetMultilingualString("bb30caba-395a-4e29-b010-375d6a83af1c", "Outer Package Unit"),
						ResString.GetMultilingualString("5e14474b-9fe6-4c06-b030-41bda1059d69", "The default Package Unit for new Outer Packages in Packing."),
						new CodeDescriptionPairListProvider(() => new RefPackTypeCollection(new BusinessObjectFactory()).GetAsCodeDescriptionPair()),
						RegistryStorageFlags.All,
						Constants.PkgUnit.Pallet
					);
				});

				return registryItem;
			}
		}

#if DEBUG
		public void SetOuterPackageUnitForTest(string unit)
		{
			OuterPackageUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unit);
		}
#endif

		#endregion

		#region InnerPackageUnit

		public CodePairRegistryItem InnerPackageUnit
		{
			get
			{
				CodePairRegistryItem registryItem = GetItem("InnerPackageUnit", delegate
				{
					var packTypesProvider = new CodeDescriptionPairListProvider(() =>
					{
						var packTypes = new RefPackTypeCollection(new BusinessObjectFactory()).GetAsCodeDescriptionPair();
						packTypes.RemoveCode(Constants.PkgUnit.Container);
						return packTypes;
					});

					return new CodePairRegistryItem
					(
						"InnerPackageUnit",
						Categories.Packing,
						ResString.GetMultilingualString("82e64ce1-ec1b-4cfb-8b9d-4bc639c0ef99", "Inner Package Unit"),
						ResString.GetMultilingualString("1a950e06-bd32-4bf6-a80b-5cdf32c34b07", "The default Package Unit for new Inner Packages in Packing."),
						packTypesProvider,
						RegistryStorageFlags.All,
						Constants.PkgUnit.Carton
					);
				});

				return registryItem;
			}
		}

#if DEBUG
		public void SetInnerPackageUnitForTest(string unit)
		{
			InnerPackageUnit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unit);
		}
#endif

		#endregion

		#region PackageIDCustomisation

		public BillCustomisationRegistryItem PackageIDCustomisation
		{
			get
			{
				return GetItem("PackageIDCustomization", delegate
				{
					var dataType = new BillCustomisationRegistryDataType(DefaultPackageIDCustomisation)
					{
						AllowNonAlphanumericCharacters = true,
						EnableMacroInsertion = false,
						FountainPrefix = "",
						GeneratedNumberName = ResString.GetMultilingualString("29ac79da-5381-432a-b880-16b3a6c61e55", "Package ID"),
						SequenceNumberName = ResString.GetMultilingualString("29ac79da-5381-432a-b880-16b3a6c61e55", "Package ID"),
						MaxLength = PkgPackageHeaderSchema.KPH_PackageID.MaxLength,
						Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.PackageID
					};

					return new BillCustomisationRegistryItem(
						"PackageIDCustomization",
						Categories.Packing,
						ResString.GetMultilingualString("d4a0f17a-3003-4bb3-a06c-0413c0ec6f3e", "Package ID Customization"),
						ResString.GetMultilingualString("58a3ad19-f203-4a21-8db7-78920a8eae93", "Override this value to customize how Package IDs are formatted, these settings only apply when an SSCC cannot be generated (in the absence of a Client Prefix)."),
						RegistryStorageFlags.All,
						dataType);
				});
			}
		}

		BillOfLadingNumberCustomisation DefaultPackageIDCustomisation
		{
			get
			{
				var customisation = new BillOfLadingNumberCustomisation();

				customisation.AllowNonAlphanumericCharacters = true;
				customisation.EnableMacroInsertion = false;

				var jobNo = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.JobNo];
				jobNo.Include = true;
				jobNo.Fountain = true;

				var clientCoded1 = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1];
				clientCoded1.Include = true;
				clientCoded1.Detail = "-";

				customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "3";

				return customisation;
			}
		}

		#endregion

		#region PackageIDUniquePeriodInMonths

		public IntRegistryItem PackageIDUniquePeriodInMonths
		{
			get
			{
				return GetItem("PackageIDUniquePeriodInMonths", delegate
				{
					return new IntRegistryItem("PackageIDUniquePeriodInMonths",
						Categories.Packing,
						ResString.GetMultilingualString("71eebf42-e1f7-4584-b12c-20803943998e", "Package ID Unique Period in Months"),
						ResString.GetMultilingualString("b503754b-d5d6-45ad-afaf-0e16f625e5ca", "Specifies the default number of months for package ID unique periods."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,       // this should be changed to 'Default' once all development items done, open to users.
						6, 1, 12);
				});
			}
		}

		#endregion

		#region ProductLabelDateFormat

		public StringRegistryItem ProductLabelDateFormat
		{
			get
			{
				StringRegistryItem registryItem = GetItem("ProductLabelDateFormat", delegate
				{
					return new StringRegistryItem(
						"ProductLabelDateFormat",
						Categories.Packing_PackingLabels,
						ResString.GetMultilingualString("0918b802-be39-4be6-b2ca-00277a9b537e", "Product Label Date Format"),
						ResString.GetMultilingualString("37f681c3-8255-463a-b5fd-657420d39084",
							"The Date Format of Dates on Product Labels. Default: dd.MM.yyyy"),
						RegistryStorageFlags.All,
						RegistryOptions.Default,
						"dd.MM.yyyy"); // default
				});

				return registryItem;
			}
		}

		#endregion

		#region DefaultDocumentToPrintOnClosePackage

		public GuidRegistryItem DefaultDocumentToPrintOnClosePackage
		{
			get
			{
				GuidRegistryItem registryItem = GetItem("DefaultDocumentToPrintOnClosePackage", delegate
				{
					return new GuidRegistryItem(
						"DefaultDocumentToPrintOnClosePackage",
						Categories.Packing_PackingLabels,
						ResString.GetMultilingualString("982a07d0-af68-4d28-b0f8-1b16662aefe9", "Default Label to Print on Close Package when Scan Packing"),
						ResString.GetMultilingualString("982a07d0-af68-4d28-b0f8-1b16662aefe9", "Default Label to Print on Close Package when Scan Packing"),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.PackingDocument),
						RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						DefaultTargetLabelDocument);
				});

				return registryItem;
			}
		}

		internal static readonly Guid DefaultTargetLabelDocument = new Guid("89758b9c-7fc7-4f5f-a095-f5d3ec318cde");
#if DEBUG
		public static readonly Guid DefaultTargetLabelDocumentForTesting = DefaultTargetLabelDocument;
#endif

		#endregion

		#region AnonymousPackageCreationEnabled

		public BooleanRegistryItem AnonymousPackageCreationEnabled
		{
			get
			{
				var registryItem = GetItem("AnonymousPackageCreationEnabled", delegate
				{
					return new BooleanRegistryItem(
						"AnonymousPackageCreationEnabled",
						Categories.Packing,
						ResString.GetMultilingualString("4a57dcbc-0b96-47cd-8ad5-65309cee627b", "Package Scanning Auto-Creation"),
						ResString.GetMultilingualString("301d9617-0f9f-4d99-baee-dff9f67a40d5", "If enabled, 'anonymous' packages will automatically be created when scanning barcodes that do not match a package in the system. They can then be allocated to jobs when a matching package ID/barcode is entered."),
						RegistryStorageFlags.System,
						false);
				});

				return registryItem;
			}
		}

		#endregion

		#region AnonymousPackagePurgeTime

		public IntRegistryItem AnonymousPackagePurgeTime
		{
			get
			{
				var registryItem = GetItem("AnonymousPackagePurgeTime", delegate
				{
					return new IntRegistryItem(
						"AnonymousPackagePurgeTime",
						Categories.Packing,
						ResString.GetMultilingualString("8d0f01ee-7993-4efc-b2d7-290244ac4686", "Anonymous Package Purge Time"),
						ResString.GetMultilingualString("fa616a27-8a24-4aad-aff9-c2e6aa740b93", "The time (in days) after which Anonymous Packages will be deleted if they have remained unattached to a Job."),
						RegistryStorageFlags.System,
						14);
				});

				return registryItem;
			}
		}

		#endregion

		#region Pallet Management

		public PalletProviderRegistryItem PalletTypes
		{
			get
			{
				var registryItem = GetItem("PalletTypes", delegate
				{
					return new PalletProviderRegistryItem(
						"PalletTypes",
						Categories.Packing_Pallets,
						ResString.GetMultilingualString("a4f83d0a-7045-4c7c-9756-4e6aa4f1b54b", "Pallet Types"),
						ResString.GetMultilingualString("a2d0ed8e-4132-46a9-84e9-26a4f4812c76", "Available pallet types as well as details used for Pallet management and reconciliation."),
						GetDefaultPalletTypes());
				});

				return registryItem;
			}
		}

		PalletTypeParent GetDefaultPalletTypes()
		{
			var result = new PalletTypeParent();

			// CHEP Equipment
			// http://www.2icsoftware.com/Portals/0/activeforums_Attach/EquipmentCodes.xls
			result.Types.Add(new PalletType { Code = "CHW", Description = ResString.GetMultilingualString("3023e716-afcf-40c3-966a-814aae11d311", "CHEP - Wooden Pallet"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountChep, EquipmentCode = "10001" });
			result.Types.Add(new PalletType { Code = "CCG", Description = ResString.GetMultilingualString("4073e716-afcf-40c3-966a-814aae11d304", "CHEP - Cage"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountChep, EquipmentCode = "10002" });
			result.Types.Add(new PalletType { Code = "CCL", Description = ResString.GetMultilingualString("14620b52-f335-40d6-a595-c9e4577edb9c", "CHEP - Cage Lid"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountChep, EquipmentCode = "10003" });
			result.Types.Add(new PalletType { Code = "CHB", Description = ResString.GetMultilingualString("733e344a-58bc-4967-a907-77d90f2e25a4", "CHEP - Block UK Pallet"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountChep, EquipmentCode = "00001" });
			result.Types.Add(new PalletType { Code = "CHS", Description = ResString.GetMultilingualString("84d5bf22-cb49-4509-bed4-ba4dcbac5a44", "CHEP - Block USA Pallet"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountChep, EquipmentCode = "04055" });
			result.Types.Add(new PalletType { Code = "CHC", Description = ResString.GetMultilingualString("23c4a6c6-8b39-481b-bda7-28b85f5efa8f", "CHEP - Black Folding Crate"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountChep, EquipmentCode = "10700" });

			// LOSCAM Equipment
			// http://www.loscam.com/uploads/file/LOS001-OCT09-Hire%20Equipment%20Reference%20Manual%20October%202009.pdf
			// http://www.loscam.com/sites/default/files/LOS%20HERM%20VER2%20Loscam%20Hire%20Equipment%20Reference%20Manual%20Ver2.pdf
			result.Types.Add(new PalletType { Code = "LWP", Description = ResString.GetMultilingualString("9003feef-1e53-4e48-8e56-f4ab6a7a0f73", "LOSCAM - Wooden Pallet"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "WP" });
			result.Types.Add(new PalletType { Code = "LVP", Description = ResString.GetMultilingualString("1d8add69-0d80-4c4a-bd61-394bf81ca883", "LOSCAM - VICFAM Plastic Pallet"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "VP" });

			result.Types.Add(new PalletType { Code = "LCS", Description = ResString.GetMultilingualString("252b1ef2-0f7a-44aa-82a5-0ca9420df226", "LOSCAM - Collar Surround"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "CS" });
			result.Types.Add(new PalletType { Code = "LC1", Description = ResString.GetMultilingualString("790a6cfd-5970-466b-96e7-3856e9db8df8", "LOSCAM - Collar Lid"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "C1" });

			result.Types.Add(new PalletType { Code = "LN1", Description = ResString.GetMultilingualString("fa2e8758-b47b-48b0-a4f3-52f335881411", "LOSCAM - NALLY Bin 730 mm high"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "N1" });
			result.Types.Add(new PalletType { Code = "LN2", Description = ResString.GetMultilingualString("4060be91-713b-4056-9280-01c9548cf6ef", "LOSCAM - NALLY 780V10 Mega-bin-vented"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "N2" });

			result.Types.Add(new PalletType { Code = "L1F", Description = ResString.GetMultilingualString("349a4628-d4ec-4405-9f75-626f9849af08", "LOSCAM - IBC Full - 1165x1130x1100"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "I6" });
			result.Types.Add(new PalletType { Code = "L1C", Description = ResString.GetMultilingualString("d07a860a-3ded-45ca-9a02-b3463f84b97b", "LOSCAM - IBC Collapsed - 1165x1130x360"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "I6" });

			result.Types.Add(new PalletType { Code = "LG1", Description = ResString.GetMultilingualString("cd13d6e1-a94b-4343-8a06-51ad7bc63669", "LOSCAM - GPAK 1165x1165x1200 mm"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "G1" });
			result.Types.Add(new PalletType { Code = "LG2", Description = ResString.GetMultilingualString("71e896fe-6a59-4ef5-a710-02c28de0a593", "LOSCAM - GPAK 1165x1165x1800 mm"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "G2" });
			result.Types.Add(new PalletType { Code = "LG3", Description = ResString.GetMultilingualString("560df62e-a346-4bfa-87bd-6a7f4310ea48", "LOSCAM - GPAK 2300x1165x1200 mm"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "G3" });
			result.Types.Add(new PalletType { Code = "LG4", Description = ResString.GetMultilingualString("7aa6abb3-0522-485a-abe6-bd3d5c4654e5", "LOSCAM - GPAK 2300x1165x600 mm"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "G4" });
			result.Types.Add(new PalletType { Code = "LG5", Description = ResString.GetMultilingualString("bc89dbff-4bc5-4d93-8bdd-64a8d1555f3c", "LOSCAM - GPAK 2300x1165x2000 mm"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "G5" });
			result.Types.Add(new PalletType { Code = "LG6", Description = ResString.GetMultilingualString("16184973-5bde-41e9-9d5f-0d6fbf899ec7", "LOSCAM - GPAK Full - 1165x1130x1100"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "G6" });

			result.Types.Add(new PalletType { Code = "LD1", Description = ResString.GetMultilingualString("637f2fd4-3f67-4495-b519-e5e6485b5473", "LOSCAM - DOLAV 42-800A solid bin"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "D1" });
			result.Types.Add(new PalletType { Code = "LD2", Description = ResString.GetMultilingualString("83aede0c-fc4d-4ef6-8280-6f8aa4477c7c", "LOSCAM - DOLAV 41-800M vented bin"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "D2" });
			result.Types.Add(new PalletType { Code = "LD3", Description = ResString.GetMultilingualString("bc8eeafc-cc64-44d3-90c2-14fd375dedac", "LOSCAM - DOLAV 22-1000A solid bin"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "D3" });
			result.Types.Add(new PalletType { Code = "LD4", Description = ResString.GetMultilingualString("04ce992b-54e5-493f-93f4-da5b22afdf5b", "LOSCAM - DOLAV 21-1000M vented bin"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "D4" });
			result.Types.Add(new PalletType { Code = "LD5", Description = ResString.GetMultilingualString("69db7c6e-40e0-4863-bd17-4838d8e20f36", "LOSCAM - DOLAV 2-1120A solid bin"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "D5" });
			result.Types.Add(new PalletType { Code = "LD6", Description = ResString.GetMultilingualString("8155dc6e-e4a7-46cb-862f-d0b105cff9cd", "LOSCAM - DOLAV 1-1120M vented bin"), ProviderCode = OrgCusCode.CodeTypes.PalletTradingAccountLoscam, EquipmentCode = "D6" });

			return result;
		}

		#endregion

		#region AutoPrintLabelOnPackageCloseForOverriddenConsigneeAddresses

		public BooleanRegistryItem AutoPrintLabelOnPackageCloseForOverriddenConsigneeAddresses
		{
			get
			{
				BooleanRegistryItem registryItem = GetItem("AutoPrintLabelOnPackageCloseForOverriddenConsigneeAddresses", delegate
				{
					return new BooleanRegistryItem(
						"AutoPrintLabelOnPackageCloseForOverriddenConsigneeAddresses",
						Categories.Packing_PackingLabels,
						ResString.GetMultilingualString("FB9EC6C1-4B5F-4DC9-9850-702A89FB19E1", "Auto-Print Label On Package Close For Overridden Consignee Addresses"),
						ResString.GetMultilingualString("2D1BD3C5-C86B-453D-9F37-B99A52A589F9",
							"Enabling this setting allows label auto-print on package close for overridden consignee addresses."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						false); // default
				});

				return registryItem;
			}
		}

		#endregion

		#region DamagedReasons

		public CodeDescriptionPairListWithDefaultCodeRegistryItem DamagedReasons
		{
			get
			{
				return GetItem("DamagedReasons", delegate
				{
					var defaultList = new ReadOnlyCodeDescriptionPairList();
					defaultList.DefaultCode = string.Empty; // we don't use the default, this is just to get rid of the error

					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						"DamagedReasons",
						Categories.Packing,
						ResString.GetMultilingualString("9cdc17c3-931b-456e-a26f-49c95e947da7", "Damaged Reasons"),
						ResString.GetMultilingualString("13f96455-46c0-4cc3-9561-4b778fe4d899", "The available reasons that may be provided for a damaged package."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultList,
						false,
						false,
						3,
						false);
				});
			}
		}

		#endregion

		#region LastPackingFountainsDeleteTimeUtc

		public DateTimeRegistryItem LastPackingFountainsDeleteTimeUtc
		{
			get
			{
				const string LastPackingFountainsDeleteTimeUtcRegistryName = "LastPackingFountainsDeleteTimeUtc";
				return GetItem(LastPackingFountainsDeleteTimeUtcRegistryName, delegate
				{
					return new DateTimeRegistryItem(
						LastPackingFountainsDeleteTimeUtcRegistryName,
						Categories.Packing,
						(NoResString)"Last Packing Fountains Delete Run", // Hidden registry item does not need translation
						(NoResString)"Time (in UTC) of the last time packing fountains of finalized packing jobs were deleted.", // Hidden registry item does not need translation
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						new DateTime(1900, 1, 1));
				});
			}
		}

		#endregion
	}
}
