using System;
using System.Diagnostics;
using System.Xml;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	public delegate ZQuery GetReferenceNumberQueryDelegate(SQLComparisonOperator opperator, ZString country, ZString type, ZString number);

	public class ReferenceNumberFilter : ModuleTextBaseFilter
	{
		public ReferenceNumberFilter(ZString description, GetReferenceNumberQueryDelegate queryDelegate, RefCountryCollection countries, CodeDescriptionPairList typesFromModule)
			: base(description, queryDelegate)
		{
			this.countries = countries;
			this.typesFromModule = typesFromModule;
			DefaultCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		public ReferenceNumberFilter(ZString description, GetReferenceNumberQueryDelegate queryDelegate, RefCountryCollection countries)
			: base(description, queryDelegate)
		{
			this.countries = countries;
			DefaultCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		protected ReferenceNumberFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection) { }

		[List("Countries")]
		[MaxLength(2)]
		public ZString Country
		{
			[DebuggerStepThrough]
			get { return country; }
			set
			{
				CheckMaximumLength(CountryInfo, value);
				SetNonPersistentPropertyValue(CountryInfo, ref country, value);
				types = null;

				if (!IsValidationSuspended)
				{
					Validation.ValidateCountry();
				}
			}
		}

		public ZPropertyInfo CountryInfo
		{
			get
			{
				return this.GetZPropertyInfo(nameof(Country));
			}
		}

		public RefCountryCollection Countries
		{
			get { return countries; }
		}

		public ZString DefaultCountry
		{
			[DebuggerStepThrough]
			get { return defaultCountry; }
			set
			{
				Country = value;
				defaultCountry = value;
			}
		}

		[List("Types")]
		[MaxLength(3)]
		public virtual ZString Type
		{
			[DebuggerStepThrough]
			get { return type; }
			set
			{
				CheckMaximumLength(TypeInfo, value);
				SetNonPersistentPropertyValue(TypeInfo, ref type, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateType();
				}
			}
		}

		public virtual ZPropertyInfo TypeInfo
		{
			get { return this.GetZPropertyInfo(nameof(Type)); }
		}

		public CodeDescriptionPairList Types
		{
			get
			{
				return types ?? (types = typesFromModule ?? (CodeDescriptionPairList)ObjectFactory.Get<Enterprise.Integration.Customs.ICusEntryNumHelper>().GetAdditionalReferenceNumberTypes(Country));
			}
		}

		public new ReferenceNumberFilterValidation Validation
		{
			get { return ((ReferenceNumberFilterValidation)(base.Validation)); }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ReferenceNumberFilterValidation(this);
		}

		#region Implementation

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			base.CopyPersistantValuesFromFilter(filterToCopyFrom);
			ReferenceNumberFilter source = ((ReferenceNumberFilter)(filterToCopyFrom));
			source.country = country;
			source.type = type;
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			Country = DefaultCountry;
			Type = "";
		}

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.NumbersAndReferences; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ReferenceNumberFilter(category, parentCollection);
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			throw new NotSupportedException();
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("Country", Country);
			writer.WriteElementString("Type", Type);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			Country = reader.ReadElementString("Country");
			Type = reader.ReadElementString("Type");
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && Type.IsEmpty;

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { SqlComparisonOperator, Country, Type, Property }; }
		}

		ZString country;
		ZString type;
		ZString defaultCountry;

		readonly RefCountryCollection countries;
		CodeDescriptionPairList types;
		readonly CodeDescriptionPairList typesFromModule;

		#endregion
	}
}
