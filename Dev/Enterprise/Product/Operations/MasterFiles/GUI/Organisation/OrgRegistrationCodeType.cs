using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public class OrgRegistrationCodeType : NonPersistentBusinessObject
	{
		public OrgRegistrationCodeType(string type, string description, string countryCode, bool isPrimary, string category, bool localBizNumber)
		{
			var yes = Res.GetString("620e5599-6454-4a23-8dc8-7c2cad744ef8", "Yes");
			var no = Res.GetString("a582130f-b5f7-457c-a523-cc389508d2b7", "No");
			Type = type;
			Description = description;
			Country = countryCode;
			Primary = isPrimary ? yes : no;
			Category = category;
			LocalBusinessNumber = localBizNumber ? yes : no;
		}

		public OrgRegistrationCodeType() { }

		public static class Schema
		{
			public const string TableName = "OrgRegistrationCodeType";
			public const string Type = "Type";
			public const string Description = "Description";
			public const string Country = "Country";
			public const string Primary = "Primary";
			public const string Category = "Category";
			public const string LocalBusinessNumber = "Local Business Number";
		}

		public ZString Type { get; }
		public ZString Description { get; }
		public ZString Country { get; }
		public ZString Primary { get; }
		public ZString Category { get; }
		public ZString LocalBusinessNumber { get; }
	}

	class OrgRegistrationCodeTypeExcelExportColumn : ExcelExportColumnBase
	{
		public OrgRegistrationCodeTypeExcelExportColumn(string description, int width)
		{
			Description = description;
			Width = width;
		}

		public override DocumentEngineIntegration.CellFormat GetFormat(IZType value) => new DocumentEngineIntegration.CellFormat();

		protected override string GetDescription() => Description;

		protected override IZType GetValueForExportCore(BusinessObject bizObj)
		{
			var propertyName = Description.Replace(" ", string.Empty);
			var value = typeof(OrgRegistrationCodeType).GetProperty(propertyName).GetValue(bizObj);
			return (ZString)value?.ToString();
		}
	}

	public class OrgRegistrationCodeTypeCollection : NonPersistentBusinessObjectCollection<OrgRegistrationCodeType>
	{
		public OrgRegistrationCodeTypeCollection() : base(new BusinessObjectFactory()) { }

		const int ExcelColumnWidthMultiplyFactor = 48;
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static List<ExcelExportColumnBase> columns;
		public static List<ExcelExportColumnBase> ColumnsForExcel
		{
			get
			{
				if (columns == null)
				{
					columns = new List<ExcelExportColumnBase>();
					columns.Add(new OrgRegistrationCodeTypeExcelExportColumn(OrgRegistrationCodeType.Schema.Type, 100 * ExcelColumnWidthMultiplyFactor));
					columns.Add(new OrgRegistrationCodeTypeExcelExportColumn(OrgRegistrationCodeType.Schema.Description, 300 * ExcelColumnWidthMultiplyFactor));
					columns.Add(new OrgRegistrationCodeTypeExcelExportColumn(OrgRegistrationCodeType.Schema.Country, 50 * ExcelColumnWidthMultiplyFactor));
					columns.Add(new OrgRegistrationCodeTypeExcelExportColumn(OrgRegistrationCodeType.Schema.Primary, 50 * ExcelColumnWidthMultiplyFactor));
					columns.Add(new OrgRegistrationCodeTypeExcelExportColumn(OrgRegistrationCodeType.Schema.Category, 300 * ExcelColumnWidthMultiplyFactor));
					columns.Add(new OrgRegistrationCodeTypeExcelExportColumn(OrgRegistrationCodeType.Schema.LocalBusinessNumber, 300 * ExcelColumnWidthMultiplyFactor));
				}

				return columns;
			}
		}

		protected override bool AllowNewCore => false;

		protected override BusinessObject AddNewCore(Type bizoType) => throw new NotSupportedException();

		protected virtual RefCountry[] GetAllCountries() => Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_IsActive, true) { OrderBy = RefCountrySchema.RN_Code.Name });

		public void InitializeCollection()
		{
			var countries = GetAllCountries();
			var cusCodesList = new OrgCodeLists();
			foreach (var country in countries)
			{
				if (country != null)
				{
					var pairList = cusCodesList.CustomsCodes_List(country);
					var orgNumberTypes = OrgRegistrationNumberTypeList.GetOrganizationalNumberTypes(country);
					var personalNumberTypes = OrgRegistrationNumberTypeList.GetPersonalEffectsNumberTypes(country);

					foreach (CodeDescriptionPair pair in pairList)
					{
						var isPrimary = OrgCusCode.IsCompanyCodeTypePrimary(pair.Code, country);
						var codeType = Res.GetString("8f1baadf-01de-456a-939e-018873780d01", "Miscellaneous Number");
						if (orgNumberTypes != null && orgNumberTypes.Contains(pair.Code))
						{
							codeType = Res.GetString("f3ffdc85-a1a5-46bb-8876-b02b592de808", "Organization Number");
						}
						else if (personalNumberTypes != null && personalNumberTypes.Contains(pair.Code))
						{
							codeType = Res.GetString("7d6c8c72-1e59-48a6-9e79-a0cd62e806b7", "Personal Effects Number");
						}

						var isLocalBizNumber = pair.Code == country.LocalBusinessRegNoCodeType;

						Add(new OrgRegistrationCodeType(pair.Code, pair.Description, country.RN_Code, isPrimary, codeType, isLocalBizNumber));
					}
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new OrgRegistrationCodeType();
	}
	class OrgRegistrationCodeTypeReader : CollectionWrapperBusinessObjectReader
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static OrgRegistrationCodeTypeCollection codeCollection;
		public static IBusinessObjectCollection CodeCollection
		{
			get
			{
#if DEBUG
				if (codeCollection == null && Globals.IsTest)
				{
					codeCollection = new OrgRegistrationCodeTypeCollectionForTest();
					codeCollection.InitializeCollection();
				}
#endif
				if (codeCollection == null)
				{
					codeCollection = new OrgRegistrationCodeTypeCollection();
					codeCollection.InitializeCollection();
				}

				return codeCollection;
			}
		}

		public override bool HasRecords => true;

		public OrgRegistrationCodeTypeReader() : base(CodeCollection) { }
	}

#if DEBUG
	public class OrgRegistrationCodeTypeCollectionForTest : OrgRegistrationCodeTypeCollection
	{
		protected override RefCountry[] GetAllCountries() => new RefCountry[] { RefCountry.LoadFromCountryCode(Factory, "AU"), RefCountry.LoadFromCountryCode(Factory, "US") };
	}
#endif
}
