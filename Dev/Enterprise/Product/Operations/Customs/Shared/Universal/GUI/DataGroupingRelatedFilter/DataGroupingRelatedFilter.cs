using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal.GUI
{
	public class DataGroupingRelatedFilter : ModuleCodeFilter
	{
		public DataGroupingRelatedFilter(ZString description, GetCodeQuery queryDelegate, BusinessObjectFactory factory,
			FieldType property2FieldType, int property2MaxLength, ResourceStringData property2ResourceString, Func<BusinessObjectFactory, ZString, ICodeDescriptionPairList> property2ListGetter)
			: base(description, queryDelegate, DummyGetList, DummyGetList)
		{
			this.factory = factory;
			Property2FieldType = property2FieldType;
			Property2MaxLength = property2MaxLength;
			Property2ResourceString = property2ResourceString;
			Property2ListGetter = property2ListGetter;
			Property1Validation = ListValidation.WarnIfInvalidCode;
			Property2Validation = ListValidation.WarnIfInvalidCode;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Property1 = Environment.Env.CurrentCompany.Country.Code;
		}

		readonly BusinessObjectFactory factory;

		#region Basic

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException(GetType().Name + " does not support 'Common'.");
		}

		protected override FilterCategory DefaultCategory => FilterCategories.TextSearch;

		protected override bool IsEmptyCore => Property2.IsEmpty;

		static GetList DummyGetList => () => null;

		public bool UseProperty2ListGetterWhenProperty1IsEmpty { get; set; }

		#endregion

		#region Property1 - Country

		[MaxLength(RefDataGrouping.Schema.ZZZ_DataGroupingMaxLength)]
		[List(nameof(GroupingList))]
		public override ZString Property1
		{
			get => base.Property1;
			set
			{
				if (Property1 != value)
				{
					InvalidateCachedQuery();
				}
				base.Property1 = value;
			}
		}

		public RefDataGroupingCollection GroupingList => new RefDataGroupingCollection(factory);

		#endregion

		#region Property2 - Customs Field

		public FieldType Property2FieldType { get; }
		public int Property2MaxLength { get; }
		Func<BusinessObjectFactory, ZString, ICodeDescriptionPairList> Property2ListGetter { get; }
		public ResourceStringData Property2ResourceString { get; }

		[MaxLength(nameof(Property2MaxLength))]
		[List(nameof(Property2List))]
		public override ZString Property2
		{
			get => base.Property2;
			set
			{
				if (Property2 != value)
				{
					InvalidateCachedQuery();
				}
				base.Property2 = value;
			}
		}

		public ICodeDescriptionPairList Property2List
		{
			get
			{
				ICodeDescriptionPairList result;
				if (Property1.IsEmpty && !UseProperty2ListGetterWhenProperty1IsEmpty || Property2ListGetter == null)
				{
					result = new CodeDescriptionPairList();
				}

				else
				{
					result = factory.GetCachedValue(ZString.Format((NoResString)"Enterprise.Customs.Universal.GUI.DataGroupingRelatedFilter.{0}|{1}", Description, Property1), () => Property2ListGetter(factory, Property1));
				}

				return result;
			}
		}

		#endregion
	}
}
