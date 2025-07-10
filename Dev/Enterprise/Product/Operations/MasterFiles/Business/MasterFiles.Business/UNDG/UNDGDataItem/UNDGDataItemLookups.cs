using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGDataItemLookups : AutoUNDGDataItemLookups
	{
		public UNDGDataItemLookups(AutoUNDGDataItem parent)
			: base(parent)
		{
		}

		#region Constants

		public static class Constants
		{
			public static MultilingualString UnknownSubstance
			{
				get { return ResString.GetMultilingualString("8F5A8DEF-E91A-49D8-B18C-FD019F1FC868", "UNKNOWN SUBSTANCE"); }
			}
		}

		#endregion

		#region Contacts

		public OrgContactCollection Contacts
		{
			get { return new OrgContactCollection(Factory); }
		}

		#endregion

		#region Volume Units

		public CodeDescriptionPairList VolumeUnits
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#region Weight Units

		public CodeDescriptionPairList WeightUnits
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		public CodeDescriptionPairList MarinePollutantList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair(UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant_Code, UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant);
				list.AddPair(UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant_Code, UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant);
				list.AddPair("", "");
				return list;
			}
		}

		#region DGClassList

		public CodeDescriptionPairList DGClassList
		{
			get { return GetDGClassList(Factory); }
		}

		#region SuppressResourceStringsCheckRegion

		public static CodeDescriptionPairList GetDGClassList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("DGClassList", delegate
			{
				var collection = new[] { "1", "1.1A", "1.1B", "1.1C", "1.1D", "1.1E", "1.1F", "1.1G", "1.1J", "1.1L",
					"1.2B", "1.2C", "1.2D", "1.2E", "1.2F", "1.2G", "1.2H", "1.2J", "1.2K", "1.2L",
					"1.3C", "1.3G", "1.3H", "1.3J", "1.3K", "1.3L", "1.4B", "1.4C", "1.4D", "1.4E",
					"1.4F", "1.4G", "1.4S", "1.5D", "1.6N", "2", "2.1", "2.2", "2.3", "3", "4", "4.1",
					"4.2", "4.3", "5.1", "5.2", "6", "6.1", "6.2", "7", "8", "9" };

				var list = new CodeDescriptionPairList();

				foreach (var item in collection)
				{
					list.AddPair(item, Res.GetString("42e9bf16-a9be-4eee-bf81-9b1920e71a7f", "Class {0}", item));
				}
				list.AddPair("COMB", Res.GetString("0713A2DE-4DBD-4F4B-9C2C-A14A2A50EDB1", "Combustible liquid"));

				return list;
			});
		}

		#endregion

		#region UNDGSubstances

		public virtual UNDGSubstanceCollection UNDGSubstances
		{
			get { return new UNDGSubstanceCollection(Factory); }
		}

		#endregion

		#endregion

		#region CodeDescriptionPairList

		public virtual CodeDescriptionPairList PackingInstructionSectionList => Factory.GetCachedValue("UNDGDataItemLookups|PackingInstructionSectionList", () => new CodeDescriptionPairList());

		#endregion

		#region Radioactive Label Categories

		public virtual CodeDescriptionPairList RadioactiveLabelCategoryList => new CodeDescriptionPairList();

		#endregion

		#region Radionuclide Elements

		public virtual CodeDescriptionPairList RadionuclideElementList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList RadionuclideElementSuffixList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList RadioactiveMaximumActivityUnitList => new CodeDescriptionPairList();

		#endregion

		#region ApprovalCertificateTypeList

		public static CodeDescriptionPairList GetApprovalCertificateTypeList(BusinessObjectFactory factory) => factory.GetCachedValue("UNDGDataItemLookups|ApprovalCertificateTypeList", () => new ApprovalCertificateTypeList());

		public CodeDescriptionPairList ApprovalCertificateTypeList => GetApprovalCertificateTypeList(Factory);

		#endregion

		#region UNDGDataItemQuantityClasses

		public static class UNDGDataItemQuantityClasses
		{
			public static class Code
			{
				public const string LIM = "LIM";
				public const string REG = "REG";
				public const string EXC = "EXC";
			}

			public static class Description
			{
				public static MultilingualString LIM => ResString.GetMultilingualString("a1b2c3d4-e5f6-7890-abcd-ef1234567890", "Limited Quantity");
				public static MultilingualString REG => ResString.GetMultilingualString("b2c3d4e5-f678-90ab-cdef-234567890123", "Regular Quantity");
				public static MultilingualString EXC => ResString.GetMultilingualString("c3d4e5f6-7890-abcd-ef12-345678901234", "Excepted Quantity");
			}
		}

		public static CodeDescriptionPairList GetUNDGDataItemQuantityClassesList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("UNDGDataItemQuantityClassesList", () =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(UNDGDataItemQuantityClasses.Code.LIM, UNDGDataItemQuantityClasses.Description.LIM);
				list.AddPair(UNDGDataItemQuantityClasses.Code.REG, UNDGDataItemQuantityClasses.Description.REG);
				list.AddPair(UNDGDataItemQuantityClasses.Code.EXC, UNDGDataItemQuantityClasses.Description.EXC);

				return list;
			});
		}

		#endregion
	}
}
