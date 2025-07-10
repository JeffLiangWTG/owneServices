using System;
using System.Collections;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class PermitRuleModuleFilter : ModuleCodeFilter
	{
		public delegate ZQuery GetPermitRuleQuery(ZString value0, ZString value1, ZString value2);

		public PermitRuleModuleFilter(ZString description, GetPermitRuleQuery queryDelegate, GetList getCountries)
			: base(description, DummyDelegate, DummyGetList, DummyGetList)
		{
			Property0Validation = ListValidation.WarnIfInvalidCode;
			Property1Validation = ListValidation.WarnIfInvalidCode;
			this.getCountries = getCountries;
			QueryDelegate = queryDelegate;
		}

		readonly GetList getCountries;

		public readonly SchemaColumn FilterColumn0;

		static ZQuery DummyDelegate(ZString param1, ZString param2)
		{
			return new ZQuery();
		}

		#region CopyPersistantValuesFromFilter

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			PermitTypeModuleFilter filter = (PermitTypeModuleFilter)filterToCopyFrom;
			Property0 = filter.Property0;
			base.CopyPersistantValuesFromFilter(filterToCopyFrom);
		}

		#endregion

		#region Clear / IsEmpty / Default

		protected override void ClearCore()
		{
			Property0 = DefaultProperty0;
			base.ClearCore();
		}

		protected override bool IsEmptyCore => Property0.IsEmpty && base.IsEmptyCore;

		public ZString DefaultProperty0
		{
			get { return fDefaultProperty0; }
			set
			{
				using (this.GetValidationSuspender())
				{
					fDefaultProperty0 = value;
					Property0 = value;
					InvalidateCachedQuery();
				}
			}
		}

		ZString fDefaultProperty0;

		#endregion

		#region New Properties

		#region Property0

		[BusinessObjectTestExclude] // don't need a maxlength
		[List("Countries")]
		public virtual ZString Property0
		{
			get { return fProperty0; }
			set
			{
				if (fProperty0 != value)
				{
					fProperty0 = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty0();
					}
					Property0Info.RefreshBinding();
					countrySpecificInstruction = null;
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property0Info
		{
			get { return GetZPropertyInfo(nameof(Property0)); }
		}

		ZString fProperty0;

		#endregion

		#region Property0Validation

		public Validation Property0Validation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property0Validation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { property0Validation = value; }
		}
		Validation property0Validation;

		#endregion

		public PermitCountrySpecificInstruction CountrySpecificInstruction
		{
			get
			{
				if (countrySpecificInstruction == null)
				{
					var country = Property0;
					if (!country.IsEmpty)
					{
						countrySpecificInstruction = PermitCountrySpecificInstruction.GetByCountryCode(Factory, country);
					}
				}
				return countrySpecificInstruction;
			}
		}
		PermitCountrySpecificInstruction countrySpecificInstruction;

		#endregion

		#region Override Properties

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException(GetType().Name + " does not support 'Common'.");
		}

		public new BusinessObjectFactory Factory => factory ?? (factory = CreateNewFactory());
		BusinessObjectFactory factory;

		[MaxLength(3)]
		[List("PermitRuleCodes")]
		public override ZString Property1
		{
			get { return base.Property1; }
			set
			{
				var hasChanged = Property1 != value;
				base.Property1 = value;
				if (hasChanged)
				{
					Property2 = ZString.Empty;
				}
			}
		}

		[MaxLength(15)]
		[ReadOnlyMember(nameof(Property2_ReadOnly))]
		[List("LookupList")]
		public override ZString Property2
		{
			get { return base.Property2; }
			set { base.Property2 = value; }
		}

		public bool Property2_ReadOnly => Property1.IsEmpty;

		public virtual ZString Property2_FieldType => CountrySpecificInstruction?.GetValueFromFieldType(Property1) ?? nameof(ZArchitecture.FieldType.Text);

		#endregion

		#region Validation

		public new PermitRuleModuleFilterValidation Validation
		{
			get { return (PermitRuleModuleFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new PermitRuleModuleFilterValidation(this);
		}

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { Property0, Property1, Property2 }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			ZQuery result = new ZQuery();
			if (!Property0.IsEmpty)
			{
				result.AddToFilter(FilterColumn0, Property0);
			}

			if (!Property1.IsEmpty)
			{
				result.AddToFilter(FilterColumn1, Property1);
			}

			if (!Property2.IsEmpty)
			{
				result.AddToFilter(FilterColumn2, Property2);
			}

			return result;
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("Property0", Property0);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == "Property0")
			{
				Property0 = reader.ReadElementString("Property0");
			}
			base.DeserializePropertiesFromXml(reader);
		}

		#endregion

		#region List

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public IList Countries => getCountries();

		public PermitRuleCodeList PermitRuleCodes => CountrySpecificInstruction?.GetRuleCodeListForModule() ?? new PermitRuleCodeList();

		public ICollection LookupList => CountrySpecificInstruction?.GetLookupList(null, Property1) ?? new CodeDescriptionPairList();

		#endregion

		internal static GetList DummyGetList
		{
			get
			{
				return () => null;
			}
		}
	}

	#region class Validation

	public class PermitRuleModuleFilterValidation : ModuleCodeFilterValidation
	{
		public PermitRuleModuleFilterValidation(PermitRuleModuleFilter parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		#region ValidateProperty0

		public void ValidateProperty0()
		{
			ValidateCalculatedProperty(Parent.Property0Info);
		}

		protected void CheckProperty0()
		{
			if (Parent.Property0Validation != null)
			{
				Parent.Property0Validation(Parent.Property0Info);
			}
		}

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			ValidateProperty0();
			base.ValidateAll();
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}

		protected new readonly PermitRuleModuleFilter Parent;

		#endregion
	}

	#endregion

}
