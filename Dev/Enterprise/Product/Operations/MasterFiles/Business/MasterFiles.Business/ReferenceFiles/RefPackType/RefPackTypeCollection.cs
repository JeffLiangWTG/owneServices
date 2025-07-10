using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Units = CargoWise.Definitions.RefPackTypeStandardUnits;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefPackType)]
	public class RefPackTypeCollection : ActiveBusinessObjectCollection<RefPackType>, IRefPackTypeCollection, IHaveCodeDescriptionPairList
	{
		public RefPackTypeCollection(BusinessObjectFactory factory)
			: this(factory, true)
		{
		}

		public RefPackTypeCollection(BusinessObjectFactory factory, bool excludeCNT)
			: base(factory)
		{
			if (excludeCNT)
			{
				this.AdditionalFilter = new ZQuery(RefPackTypeSchema.F3_Code, SQLComparisonOperator.NotEqual, ReservedContainerType);
			}
			this.ApplySort(RefPackType.Schema.F3_Code, ListSortDirection.Ascending);
		}

		public const string ReservedContainerType = "CNT";

		public bool ContainsCode(string code)
		{
			var packType = (from p in this where p.F3_Code == code select p).FirstOrDefault();

			return packType != null;
		}

		public MultilingualString GetDescriptionFromCode(string code)
		{
			var description = (from p in this where p.F3_Code == code select p.F3_DescriptionMultilingual).FirstOrDefault();

			return description ?? (NoResString)string.Empty;
		}

		public CodeDescriptionPairList GetAsCodeDescriptionPair()
		{
			var list = new CodeDescriptionPairList();

			foreach (var packType in this)
			{
				list.AddPair(packType.F3_Code, packType.F3_DescriptionMultilingual);
			}

			return list;
		}

		public static CodeDescriptionPairList GetStandardUnitsAsCodeDescriptionPairs()
		{
			var list = new CodeDescriptionPairList();
			list.AddPairIfNotExist(Units.Codes.Centimeters, ResString.GetMultilingualString("d1a2c1fc-8992-469e-8d17-a9b45ea84657", Units.Descriptions.Centimeters));
			list.AddPairIfNotExist(Units.Codes.Feet, ResString.GetMultilingualString("7b54ddde-cc9c-48ce-a2ba-d950c1fec3dd", Units.Descriptions.Feet));
			list.AddPairIfNotExist(Units.Codes.Inches, ResString.GetMultilingualString("748a3410-9e9a-4e93-a7d6-249a23a8a9c3", Units.Descriptions.Inches));
			list.AddPairIfNotExist(Units.Codes.Kilometers, ResString.GetMultilingualString("f597049c-703c-438a-80f3-7c69f35716e4", Units.Descriptions.Kilometers));
			list.AddPairIfNotExist(Units.Codes.Meters, ResString.GetMultilingualString("6dd22401-61e1-4ce7-93bd-2023f89d1531", Units.Descriptions.Meters));
			list.AddPairIfNotExist(Units.Codes.Miles, ResString.GetMultilingualString("61edc90f-f21a-49d1-8827-29f8db1c5239", Units.Descriptions.Miles));
			list.AddPairIfNotExist(Units.Codes.Millimeters, ResString.GetMultilingualString("a9839f7d-8085-4daf-bd11-89f406be9a65", Units.Descriptions.Millimeters));
			list.AddPairIfNotExist(Units.Codes.Yards, ResString.GetMultilingualString("ad80aab4-e71f-4832-895b-91979c3b451f", Units.Descriptions.Yards));

			list.AddPairIfNotExist(Units.Codes.Decitons, ResString.GetMultilingualString("c0a71cb3-71e1-4c36-b948-e14354a9923e", Units.Descriptions.Decitons));
			list.AddPairIfNotExist(Units.Codes.Grams, ResString.GetMultilingualString("61f0d526-7fd8-4ab2-bd11-e6735b82db10", Units.Descriptions.Grams));
			list.AddPairIfNotExist(Units.Codes.Hectograms, ResString.GetMultilingualString("b13adf77-db39-4774-8b8f-0e28f3640c3c", Units.Descriptions.Hectograms));
			list.AddPairIfNotExist(Units.Codes.Kilograms, ResString.GetMultilingualString("9653ffdd-075a-4f6a-8f4d-bac422ce2d7c", Units.Descriptions.Kilograms));
			list.AddPairIfNotExist(Units.Codes.Kilotons, ResString.GetMultilingualString("a0281eeb-85e8-4771-9fd7-0c48539bf6bd", Units.Descriptions.Kilotons));
			list.AddPairIfNotExist(Units.Codes.Pounds, ResString.GetMultilingualString("520c5856-f2ef-460f-90ae-edd7a9e66e4d", Units.Descriptions.Pounds));
			list.AddPairIfNotExist(Units.Codes.PoundsTroy, ResString.GetMultilingualString("fb1a4f7a-8a2a-4e75-bbc5-35cdf05c8939", Units.Descriptions.PoundsTroy));
			list.AddPairIfNotExist(Units.Codes.MetricCarat, ResString.GetMultilingualString("41acf894-da28-4412-871d-f52f5a52cd5a", Units.Descriptions.MetricCarat));
			list.AddPairIfNotExist(Units.Codes.Milligrams, ResString.GetMultilingualString("176f6b6e-d4f2-4f1f-9d7b-7a96add343dd", Units.Descriptions.Milligrams));
			list.AddPairIfNotExist(Units.Codes.OuncesTroy, ResString.GetMultilingualString("895a93a6-d154-4dd0-8149-c09280169e74", Units.Descriptions.OuncesTroy));
			list.AddPairIfNotExist(Units.Codes.Ounces, ResString.GetMultilingualString("9f54aa6a-e5b4-4b1e-b10c-79c4508e0b6d", Units.Descriptions.Ounces));
			list.AddPairIfNotExist(Units.Codes.Tonnes, ResString.GetMultilingualString("61e272aa-a193-41ad-878a-b0450210dc10", Units.Descriptions.Tonnes));
			list.AddPairIfNotExist(Units.Codes.LongTons, ResString.GetMultilingualString("b5162dc7-da06-4026-be4f-55e207d1247b", Units.Descriptions.LongTons));
			list.AddPairIfNotExist(Units.Codes.ShortTons, ResString.GetMultilingualString("cb560a93-f0e0-4ed8-ac6e-6df608c95848", Units.Descriptions.ShortTons));

			list.AddPairIfNotExist(Units.Codes.CubicFeet, ResString.GetMultilingualString("56d82448-ba43-4b22-bc42-4012f209e00d", Units.Descriptions.CubicFeet));
			list.AddPairIfNotExist(Units.Codes.CubicInches, ResString.GetMultilingualString("7ff47b1d-a67f-4038-9a5d-11beeb2b0945", Units.Descriptions.CubicInches));
			list.AddPairIfNotExist(Units.Codes.CubicYards, ResString.GetMultilingualString("51fcf6e2-6144-4d0a-8237-596ce8b39683", Units.Descriptions.CubicYards));
			list.AddPairIfNotExist(Units.Codes.CubicDecimeters, ResString.GetMultilingualString("4b150982-5d56-428d-a03c-5e6b2892152b", Units.Descriptions.CubicDecimeters));
			list.AddPairIfNotExist(Units.Codes.CubicCentimeters, ResString.GetMultilingualString("9afb1b40-dbf7-4f5d-af6b-8533703617f7", Units.Descriptions.CubicCentimeters));
			list.AddPairIfNotExist(Units.Codes.Liter, ResString.GetMultilingualString("b0f8c804-8ed3-446d-aea3-df58c3676ac7", Units.Descriptions.Liter));
			list.AddPairIfNotExist(Units.Codes.CubicMeters, ResString.GetMultilingualString("67bceea2-4f67-41bd-83b1-9e800d16a25c", Units.Descriptions.CubicMeters));
			list.AddPairIfNotExist(Units.Codes.MegaLiter, ResString.GetMultilingualString("4fb6e11b-168b-47a0-8bdf-1c88d6369d74", Units.Descriptions.MegaLiter));
			list.AddPairIfNotExist(Units.Codes.TeaChest, ResString.GetMultilingualString("5f5fd1f0-c112-4e2c-94ca-3c2c163e4da9", Units.Descriptions.TeaChest));

			list.AddPairIfNotExist(Units.Codes.SquareCentimeter, ResString.GetMultilingualString("6eb9ef86-bf06-4591-bdd2-715c8ecf4e8d", Units.Descriptions.SquareCentimeter));
			list.AddPairIfNotExist(Units.Codes.SquareFoot, ResString.GetMultilingualString("cce6083f-e3eb-439a-bbdc-f3302d80cf93", Units.Descriptions.SquareFoot));
			list.AddPairIfNotExist(Units.Codes.SquareInch, ResString.GetMultilingualString("7683f358-d092-4aa3-8b75-140a78012df6", Units.Descriptions.SquareInch));
			list.AddPairIfNotExist(Units.Codes.SquareMeter, ResString.GetMultilingualString("cf300df1-3063-4dc8-a023-3aec0cca69a1", Units.Descriptions.SquareMeter));
			list.AddPairIfNotExist(Units.Codes.SquareMillimeter, ResString.GetMultilingualString("76b1b6cd-b796-4b39-923c-42965d34e2f6", Units.Descriptions.SquareMillimeter));
			list.AddPairIfNotExist(Units.Codes.SquareYard, ResString.GetMultilingualString("d73e411c-27ed-4178-a194-c0bf90e9adf0", Units.Descriptions.SquareYard));

			list.AddPairIfNotExist(Units.Codes.Number, ResString.GetMultilingualString("7C9C5528-E206-4A8D-8838-46AA39B786F9", Units.Descriptions.Number));
			list.AddPairIfNotExist(Units.Codes.NumberAlternate, ResString.GetMultilingualString("A77560EE-1B55-48A7-8092-8412FDAA7D94", Units.Descriptions.NumberAlternate));
			list.AddPairIfNotExist(Units.Codes.LitersOfAlcohol, ResString.GetMultilingualString("8CCE3A51-BFC6-4082-A557-5C840B109833", Units.Descriptions.LitersOfAlcohol));

			list.AddPairIfNotExist(Units.Codes.USGallons, ResString.GetMultilingualString("1f00a4d4-8f91-4d81-8205-ab0f11092990", Units.Descriptions.USGallons));
			list.AddPairIfNotExist(Units.Codes.ImperialGallons, ResString.GetMultilingualString("41af6dcb-2dfd-4ecd-a964-6454c8a016ee", Units.Descriptions.ImperialGallons));

			return list;
		}

		public CodeDescriptionPairList GetAsCodeDescriptionPairWithStandardUnits()
		{
			var list = GetAsCodeDescriptionPair();
			list.AddRange(GetStandardUnitsAsCodeDescriptionPairs());
			list.Sort();
			return list;
		}

		ReadOnlyCodeDescriptionPairList IHaveCodeDescriptionPairList.CodeDescriptionPairList
		{
			get
			{
				return GetAsCodeDescriptionPairWithStandardUnits();
			}
		}

		public static CodeDescriptionPairList GetAsCodeDescriptionPairWithStandardUnits(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("RefPackListWithStandardUnits", () => new RefPackTypeCollection(factory).GetAsCodeDescriptionPairWithStandardUnits());
		}

		#region IRefPackTypeCollection Members

		ICodeDescriptionPairList IRefPackTypeCollection.GetAsCodeDescriptionPair()
		{
			return GetAsCodeDescriptionPair();
		}

		public ICodeDescriptionPairList GetAsCodeDescriptionPairFull()
		{
			var list = new CodeDescriptionPairList();
			foreach (var packType in new RefPackTypeCollection(Factory, false))
			{
				list.AddPair(packType.F3_Code, packType.F3_DescriptionMultilingual);
			}
			return list;
		}

		#endregion
	}
}
