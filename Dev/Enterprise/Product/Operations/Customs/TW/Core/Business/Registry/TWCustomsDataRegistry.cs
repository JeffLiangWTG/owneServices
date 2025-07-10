using System;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business
{
	public sealed class TWCustomsDataRegistry : RegistryItemSet, Integration.Customs.TW.ITWCustomsRegistry
	{
		#region Construction
		public static TWCustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new TWCustomsDataRegistry()); }
		}
		[ThreadStatic]
		static TWCustomsDataRegistry instance;

		public TWCustomsDataRegistry()
		{
		}
		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Taiwan { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("12659B39-6B8E-482A-B220-FABFB2850325", "Taiwan")); } }
		}

		#endregion

		#region Enable Brief Customs Declaration
		public BooleanRegistryItem EnableBriefCustomsDeclaration
		{
			get
			{
				return GetItem("TWENABLEBRIEFCUSDECL", delegate
				{
					var result = new BooleanRegistryItem(
						"TWENABLEBRIEFCUSDECL",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("AF87AC1D-852A-47BE-9611-1F61A0C49A09", "Enable Brief Customs Declaration"),
						ResString.GetMultilingualString("575DCD81-BF9D-4012-A0F2-AF0A91F2A5A4", "Set this value to 'Yes' to enable Taiwan Brief Customs Declaration related functionalities."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}
		#endregion

		#region Always Calculate Pack Quantity From Pack Number
		public BooleanRegistryItem AlwaysCalculatePackQtyFromPackNumber
		{
			get
			{
				return GetItem("ALWAYSCALCPACKQTYFROMPACKNO", delegate
				{
					var result = new BooleanRegistryItem(
						"ALWAYSCALCPACKQTYFROMPACKNO",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("DF12B0E2-2860-4C65-9293-FE238A101705", "Always Calculate Pack Quantity From Pack Number"),
						ResString.GetMultilingualString("515C15CE-EC3A-4872-8BF6-72A121DB0B2C", "When the value is 'Yes', the 'Calculate Pack Qty from Pack #' menu item under Packing List > Action will be clicked by default."),
						RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						false);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}
		#endregion

		#region Enable Customs Declaration Packing List
		public BooleanRegistryItem EnableCustomsDeclarationPackingList
		{
			get
			{
				return GetItem("EnableCustomsDeclarationPackingList", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableCustomsDeclarationPackingList",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("d26d5163-7405-4e16-a610-c35cf1898b0a", "Enable Customs Declaration Packing List"),
						ResString.GetMultilingualString("c01290a3-daa6-4984-968d-a6c37699b1d4", "Set this to 'Yes' to show the menu item under Customs Declaration > Brokerage > Create Packing List, and hide the tabs under Customs Declaration > Packing and Customs Declaration > Invoice Lines > Packages in the supported countries."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						true);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}
		#endregion

		public BooleanRegistryItem TWIsTestMode
		{
			get
			{
				return GetItem("TWIsTestMode", delegate
				{
					var result = new BooleanRegistryItem(
						"TWIsTestMode",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("37C38D77-6BE6-4176-985E-97AE19DAEFE8", "Is Test Mode"),
						ResString.GetMultilingualString("42D0339D-EF1B-4117-9337-E172F178DAB0", "Should TW customs messages (for Declaration and Transhipment Declaration) be sent in testing mode?"),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						IsTestLicence);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}

		public CodeDescriptionBoolDisallowNewRegistryItem TranshipmentDescriptionCustomization
		{
			get
			{
				return GetItem("TranshipmentDescriptionCustomization", delegate
				{
					var result = new CodeDescriptionBoolDisallowNewRegistryItem(
						"TranshipmentDescriptionCustomization",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("73D138D1-FF7A-4BF8-8A04-9343550D8DD4", "Transhipment Description Customization"),
						ResString.GetMultilingualString("3B8D72B2-17D3-420C-B4E6-83344066C30F",
							"Select one or more of the following check boxes to define what reference information will be used to describe an transhipment when it appears in the Favorites or Recent Items areas on the main {0} form and the Recent Items on the Transhipment module.",
							Core.Constants.ProductName),
						RegistryStorageFlags.Company,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("4B926792-339F-4235-B7DA-2FF24DDC50D4", "Select the data you wish to use to describe an transhipment"), true, true),
						new CodeDescriptionBoolDisallowNewCollection() {
						{ "JNO", ResString.GetMultilingualString("TranshipmentDescriptionCustomization|JobNumber", "Job Number"), true },
						{ "MBL", ResString.GetMultilingualString("TranshipmentDescriptionCustomization|MasterBill", "Master Bill"), true },
						{ "HBL", ResString.GetMultilingualString("TranshipmentDescriptionCustomization|HouseBill", "House Bill"), true },
						{ "ENT", ResString.GetMultilingualString("TranshipmentDescriptionCustomization|EntryNumber", "Entry Number"), false },
						{ "IMP", ResString.GetMultilingualString("TranshipmentDescriptionCustomization|ImporterCode", "Importer Code"), false }
						});
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}

		public CusBrokerageBoxNumberRegistryItem CusBrokerageBoxNumber =>
			GetItem("CusBrokerageBoxNumber",
				() => new CusBrokerageBoxNumberRegistryItem(
					"CusBrokerageBoxNumber",
					Categories.Customs_Taiwan,
						ResString.GetMultilingualString("3CD7E029-D39B-4753-A174-6B73F9E8A23B", "Default Brokerage Box Number"),
						ResString.GetMultilingualString("52EC0539-249E-412D-8BC8-5F30BCDF200C", "The customs Brokerage Box Number."),
					RegistryStorageFlags.Company
				)
				{
					CountryFilterPKs = CountryFilterPKs.Taiwan
				}
				);

		public CusGoodsLocationRegistryItem CusGoodsLocation =>
			GetItem("CusGoodsLocation",
				() => new CusGoodsLocationRegistryItem(
				"CusGoodsLocation",
				Categories.Customs_Taiwan,
					ResString.GetMultilingualString("E5A49FF8-C934-4098-8A8D-5A372712CEE1", "Default Goods Location"),
					ResString.GetMultilingualString("AC868EF1-81C8-4C76-AFC2-C4A8A5763269", "Default Goods Location."),
					RegistryStorageFlags.Company
				)
				{
					CountryFilterPKs = CountryFilterPKs.Taiwan
				}
			);

		public BooleanRegistryItem DefaultPrintingGoodsLocationDescription
		{
			get
			{
				return GetItem("DefaultPrintingGoodsLocationDescription", delegate
				{
					var result = new BooleanRegistryItem(
						"DefaultPrintingGoodsLocationDescription",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("89DBBB60-D599-4104-ACE6-388B081AFBAD", "Default Printing Goods Location Description"),
						ResString.GetMultilingualString("160A28BE-BED6-4496-9F05-95A92FB3693F", "Set to 'Yes' to print Goods Location description on Customs Declaration documents."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}

		public static bool IsTestMode => Instance.TWIsTestMode.Value;

		public bool IsTestLicence => (RegKey?.DatabaseType ?? string.Empty) != DatabaseTypes.Codes.Production;

		IProductRegistrationKey RegKey => ObjectFactory.Get<IProductRegistration>()?.Key;

		public TWNCATKClientSettingRegistryItem TWNCATKClientSetting =>
			GetItem("TWNCATKClientSetting",
				() => new TWNCATKClientSettingRegistryItem(
					"TWNCATKClientSetting",
					Categories.Customs_Taiwan,
					ResString.GetMultilingualString("F6B2865C-FF37-4D71-AA0B-2705177C15B9", "NCATK Message Sending Configuration"),
					ResString.GetMultilingualString("E81B7692-94DA-460B-93A1-832C601DCDB7", "The settings are for TW NCATK Client Application which is a standalone tool installed on the client’s local machine. The tool sends and receives messages through the Remote Printing Client software."),
					RegistryStorageFlags.Company
				)
				{
					CountryFilterPKs = CountryFilterPKs.Taiwan
				}
			);

		public CusBrokerStaffRegistryItem CusBrokerStaff =>
			GetItem("CusBrokerStaff",
				() => new CusBrokerStaffRegistryItem(
				"CusBrokerStaff",
				Categories.Customs_Taiwan,
					ResString.GetMultilingualString("B43AC1E3-FB8F-41A4-BF02-BAB5495D8F38", "Default Broker Staff"),
					ResString.GetMultilingualString("69D30E3F-B847-4586-9A5F-8BA35BC71C18", "Default Broker Staff."),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment
				)
				{
					CountryFilterPKs = CountryFilterPKs.Taiwan
				}
			);

		public CusCustomsOfficeRegistryItem CusCustomsOffice =>
			GetItem("CusCustomsOffice",
				() => new CusCustomsOfficeRegistryItem(
				"CusCustomsOffice",
				Categories.Customs_Taiwan,
					ResString.GetMultilingualString("38A2454C-C280-41E6-943D-D4EFDE8B1FB7", "Default Customs Office"),
					ResString.GetMultilingualString("DC34A5D5-9C31-47D5-9290-892576AE6835", "Default Customs Office."),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment
				)
				{
					CountryFilterPKs = CountryFilterPKs.Taiwan
				}
			);

		#region Enable NXM
		public BooleanRegistryItem EnableNX201_01
		{
			get
			{
				return GetItem("EnableNX201_01", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableNX201_01",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("0E327342-ED80-4F52-8894-2331499518DE", "Enable NX201_01"),
						ResString.GetMultilingualString("BFBE530C-3964-4402-8E6B-BCA7C0503F18", "Set this value to 'Yes' to enable NX201_01 related functionalities."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						true);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableNX201_07
		{
			get
			{
				return GetItem("EnableNX201_07", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableNX201_07",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("9513392E-2A3E-4900-BB18-D0AFB911224F", "Enable NX201_07"),
						ResString.GetMultilingualString("303A5CFE-2140-47D6-9BB2-350643D6C958", "Set this value to 'Yes' to enable NX201_07 related functionalities."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						true);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableNX301
		{
			get
			{
				return GetItem("EnableNX301", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableNX301",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("33F4925F-B193-4741-968D-C55609374F85", "Enable NX301"),
						ResString.GetMultilingualString("F10F13D1-2179-4732-9DCE-2E764DAE60F0", "Set this value to 'Yes' to enable NX301 related functionalities."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						true);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableNX301_AX
		{
			get
			{
				return GetItem("EnableNX301_AX", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableNX301_AX",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("3EE8F860-DF2F-4EAD-A3A9-36D4CB336077", "Enable NX301_AX"),
						ResString.GetMultilingualString("B12133AF-6AAD-4B4D-895A-82256538BDDD", "Set this value to 'Yes' to enable NX301_AX related functionalities."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						true);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableNX301_DN
		{
			get
			{
				return GetItem("EnableNX301_DN", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableNX301_DN",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("F2144F15-11CE-4D2E-A341-D876E99E9755", "Enable NX301_DN"),
						ResString.GetMultilingualString("10C57542-68C8-4637-A1D1-962CF5858801", "Set this value to 'Yes' to enable NX301_DN related functionalities."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						true);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableNX401
		{
			get
			{
				return GetItem("EnableNX401", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableNX401",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("2DFE2A20-2DB0-4728-970B-40DB514639F0", "Enable NX401"),
						ResString.GetMultilingualString("CD7F9BF9-9EC3-471E-B232-68236FC77CE0", "Set this value to 'Yes' to enable NX401 related functionalities."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						true);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableNX601
		{
			get
			{
				return GetItem("EnableNX601", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableNX601",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("83A07519-2C48-4516-924B-A6754ADEF25B", "Enable NX601"),
						ResString.GetMultilingualString("01FFAC8F-F278-4A41-A6EC-EA64882B671F", "Set this value to 'Yes' to enable NX601 related functionalities."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						true);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableNX603
		{
			get
			{
				return GetItem("EnableNX603", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableNX603",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("5BD417DB-C016-45C3-8356-F134A8ADB9A9", "Enable NX603"),
						ResString.GetMultilingualString("970D7F8F-C962-4AD1-BEA3-731872742E70", "Set this value to 'Yes' to enable NX603 related functionalities."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						true);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}
		#endregion

		public BooleanRegistryItem AlwaysCalculateAdditionalTax
		{
			get
			{
				return GetItem("AlwaysCalculateAdditionalTax", delegate
				{
					var result = new BooleanRegistryItem(
						"AlwaysCalculateAdditionalTax",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("1075152C-CE17-4F34-8513-21BB5D2E8E6C", "Always Calculate Additional Tax"),
						ResString.GetMultilingualString("4AFC3BF6-1CA2-4C49-A394-EBB4F0C4BAE9", "Set to 'Yes' to allow automatically calculating the additional taxes when the selected tariff has Customs Regulations T, T*, B, B*, L*, C. In other words, if the goods do not need to declare the corresponding additional taxes, they need to be deleted manually."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}

		public BooleanRegistryItem ValidateEntryNumber
		{
			get
			{
				return GetItem("ValidateEntryNumber", delegate
				{
					var result = new BooleanRegistryItem(
						"ValidateEntryNumber",
						Categories.Customs_Taiwan,
						ResString.GetMultilingualString("328FD0A1-FCD1-4C78-9877-A38CDB1F88D1", "Validate Entry Number"),
						ResString.GetMultilingualString("51DBE273-9073-4BA2-9295-3DB7B3AD7206", "Display customs error when Entry number is not allocated."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
					result.CountryFilterPKs = CountryFilterPKs.Taiwan;
					return result;
				});
			}
		}

		IRegistryItem Integration.Customs.TW.ITWCustomsRegistry.CustomsPackingListEnable => EnableCustomsDeclarationPackingList;

		IRegistryItem Integration.Customs.TW.ITWCustomsRegistry.EnableBriefCustomsDeclaration => EnableBriefCustomsDeclaration;

		IRegistryItem Integration.Customs.TW.ITWCustomsRegistry.EnableNX201_01 => EnableNX201_01;

		IRegistryItem Integration.Customs.TW.ITWCustomsRegistry.EnableNX201_07 => EnableNX201_07;

		IRegistryItem Integration.Customs.TW.ITWCustomsRegistry.EnableNX301 => EnableNX301;

		IRegistryItem Integration.Customs.TW.ITWCustomsRegistry.EnableNX301_AX => EnableNX301_AX;

		IRegistryItem Integration.Customs.TW.ITWCustomsRegistry.EnableNX301_DN => EnableNX301_DN;

		IRegistryItem Integration.Customs.TW.ITWCustomsRegistry.EnableNX401 => EnableNX401;

		IRegistryItem Integration.Customs.TW.ITWCustomsRegistry.EnableNX601 => EnableNX601;

		IRegistryItem Integration.Customs.TW.ITWCustomsRegistry.EnableNX603 => EnableNX603;
	}
}
