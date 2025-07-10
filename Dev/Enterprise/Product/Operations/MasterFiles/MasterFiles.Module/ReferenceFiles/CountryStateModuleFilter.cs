using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	public abstract class CountryStateModuleFilter<T> : ModuleFilter where T : IZType
	{
		#region Construction

		protected CountryStateModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public CountryStateModuleFilter(ZString description, SchemaColumn countryColumn, SchemaColumn stateColumn)
			: base(description)
		{
			fStates = GetStates();
			CountryColumn = countryColumn;
			StateColumn = stateColumn;
		}

		#endregion

		#region State

		RefCountryStatesCollection GetStates()
		{
			var factory = new BusinessObjectFactory();
			RefCountryStatesCollection states = new RefCountryStatesCollection(factory);
			if (!Property1.IsEmpty)
			{
				RefCountry country = RefCountry.LoadFromCountryCode(factory, Property1);
				if (country != null)
				{
					ApplyStatesFilterCore(states, country.RN_Code);
				}
			}
			return states;
		}

		protected virtual void ApplyStatesFilterCore(RefCountryStatesCollection states, ZString countryCode)
		{
			states.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Country", "Property", countryCode));
		}

		#endregion

		#region ComparisonOperator

		public virtual CodeDescriptionPairList ComparisonOperator_List
		{
			get
			{
				if (fComparisonOperator_List == null)
				{
					fComparisonOperator_List = new CodeDescriptionPairList();
					fComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, Res.GetString("35a2e68d-acb1-4ab5-85d8-c631116a28a6", "Search for Cities/Towns inside specified Country/Region/State"));
					fComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual, Res.GetString("5de6b279-ebda-41e7-abe4-ac7173564159", "Search for Cities/Towns outside specified Country/Region/State"));
				}
				return fComparisonOperator_List;
			}
		}
		protected CodeDescriptionPairList fComparisonOperator_List;

		[BusinessObjectTestExclude]
		public ZString ComparisonOperator
		{
			get
			{
				if (fComparisonOperator.IsEmpty)
				{
					fComparisonOperator = ComparisonOperator_List[0].Code;
				}
				return fComparisonOperator;
			}
			set
			{
				if (fComparisonOperator != value)
				{
					fComparisonOperator = value;
					ComparisonOperatorInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZString fComparisonOperator;

		public ZPropertyInfo ComparisonOperatorInfo
		{
			get { return GetZPropertyInfo(nameof(ComparisonOperator)); }
		}

		#endregion

		#region DefaultCategory

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Locations; }
		}

		#endregion

		#region schema columns

		readonly SchemaColumn CountryColumn;
		readonly SchemaColumn StateColumn;

		#endregion

		#region Property1/Property2/Validation

		#region Property1

		[MaxLength(RefCountry.Schema.RN_CodeMaxLength)]
		[BusinessObjectTestExclude]
		public ZString Property1
		{
			get { return fProperty1; }
			set
			{
				if (fProperty1 != value)
				{
					fProperty1 = value;
					fStates = GetStates();
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty1();
					}
					Property1Info.RefreshBinding();
					if (!Property2Info.Value.IsEmpty && !IsValidationSuspended)
					{
						Validation.ValidateProperty2();
					}
					Property2 = default(T);
					Property2Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property1Info
		{
			get { return GetZPropertyInfo(nameof(Property1)); }
		}
		ZString fProperty1;

		#endregion

		#region Property1Validation

		public Validation Property1Validation
		{
			get
			{
				return delegate(ZPropertyInfo info)
				{
					//Implemtent me if needed
				};
			}
		}

		#endregion

		#region Property2

		[BusinessObjectTestExclude] // don't need a maxlength
		public T Property2
		{
			get { return fProperty2; }
			set
			{
				if (!Comparer<T>.Equals(fProperty2, value))
				{
					fProperty2 = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty2();
					}
					Property2Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		T fProperty2;

		public ZPropertyInfo Property2Info
		{
			get { return GetZPropertyInfo(nameof(Property2)); }
		}

		#endregion

		#region Property2Validation

		public Validation Property2Validation
		{
			get
			{
				return delegate(ZPropertyInfo info)
				{
					if (!info.Value.IsEmpty)
					{
						if (Property1.IsEmpty)
						{
							string errorMessage = Res.GetString("140C2918-AB4D-4DBB-B205-C34D74CE69B9", "Enter a country/region first.");
							info.AddError(errorMessage);
						}
						else
						{
							RefCountry country = RefCountry.LoadFromCountryCode(new BusinessObjectFactory(), Property1);
							if (country != null)
							{
								ZQuery stateQuery = new ZQuery(CountryStatesColumnForProperty2Validation, info.Value);
								if (country.States.Find(stateQuery).Length == 0)
								{
									string errorMessage = Res.GetString("080B9F86-9623-418D-87F2-E5C53A902C90", "State is not in country/region '{0}'", Property1);
									info.AddError(errorMessage);
								}

								if (Property1.IsEmpty)
								{
									string errorMessage = Res.GetString("140C2918-AB4D-4DBB-B205-C34D74CE69B9", "Enter a country/region first.");
									info.AddError(errorMessage);
								}
							}
						}
					}
				};
			}
		}

		protected SchemaColumn CountryStatesColumnForProperty2Validation
		{
			get
			{
				if (countryStatesColumnForProperty2Validation == null)
				{
					throw new ArgumentNullException("CountryStatesColumnForProperty2Validation cannot be null");
				}
				return countryStatesColumnForProperty2Validation;
			}
			set
			{
				countryStatesColumnForProperty2Validation = value;
			}
		}
		SchemaColumn countryStatesColumnForProperty2Validation;

		#endregion

		#region Validation

		public new CountryStateFilterValidation<T> Validation
		{
			get { return (CountryStateFilterValidation<T>)base.Validation; }
		}

		#endregion

		#endregion

		#region Countries/States lists

		[SuppressWeaklyTypedCollectionMessage]
		public IList Countries
		{
			get
			{
				if (fCountries == null)
				{
					fCountries = new RefCountryCollection(new BusinessObjectFactory());
				}

				return fCountries;
			}
		}
		IList fCountries;

		[SuppressWeaklyTypedCollectionMessage]
		public IList States
		{
			get
			{
				return fStates;
			}
		}

		IList fStates;

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { Property1, Property2 }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			ZQuery result = new ZQuery();
			if (ComparisonOperator == ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact)
			{
				if (!Property1.IsEmpty)
				{
					result.AddToFilter(CountryColumn, Property1);
				}
				if (!Property2.IsEmpty)
				{
					result.AddToFilter(StateColumn, Property2);
				}
			}
			else if (ComparisonOperator == ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual)
			{
				if (!Property1.IsEmpty)
				{
					result.AddToFilter(CountryColumn, SQLComparisonOperator.NotEqual, Property1);
				}
				if (!Property2.IsEmpty)
				{
					result.AddToFilter(JoinCondition.Or, StateColumn, SQLComparisonOperator.NotEqual, Property2);
				}
			}
			return result;
		}

		#endregion

		#region CopyPersistantValuesFromFilter

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			ModuleCodeFilter filter = (ModuleCodeFilter)filterToCopyFrom;
			Property1 = filter.Property1;
			CopyPersistantValuesFromFilterForProperty2(filter);
		}

		protected abstract void CopyPersistantValuesFromFilterForProperty2(ModuleFilter filter);

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Clear / IsEmpty / Default

		protected override void ClearCore()
		{
			Property1 = DefaultProperty1;
			Property2 = DefaultProperty2;
		}

		protected override bool IsEmptyCore => Property1.IsEmpty && Property2.IsEmpty;

		public ZString DefaultProperty1
		{
			get { return fDefaultProperty1; }
			set
			{
				fDefaultProperty1 = value;
				Property1 = value;
				InvalidateCachedQuery();
			}
		}

		ZString fDefaultProperty1;

		T fDefaultProperty2;

		public T DefaultProperty2
		{
			get { return fDefaultProperty2; }
			set
			{
				fDefaultProperty2 = value;
				Property2 = value;
				InvalidateCachedQuery();
			}
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("Property1", Property1);
			writer.WriteElementString("Property2", Property2.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == "Property1")
			{
				Property1 = reader.ReadElementString("Property1");
			}
			DeserializePropertiesFromXmlForProperty2(reader);
		}

		protected abstract void DeserializePropertiesFromXmlForProperty2(XmlReader reader);

		#endregion

		#region NewValidation
		protected abstract override ModuleFilterValidation GetNewValidation();
		#endregion

		#region GetNewCommonModuleFilter
		protected abstract override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection);
		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property1 = RandomString(MaxLength);
		}

#endif
		#endregion
	}

	#region class Validation

	public abstract class CountryStateFilterValidation<T> : ModuleFilterValidation where T : IZType
	{
		public CountryStateFilterValidation(CountryStateModuleFilter<T> parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		#region ValidateProperty1

		public void ValidateProperty1()
		{
			ValidateCalculatedProperty(Parent.Property1Info);
		}

		protected void CheckProperty1()
		{
			ErrorIfInvalidCode(Parent.Property1Info, Parent.Countries);
			if (Parent.Property1Validation != null)
			{
				Parent.Property1Validation(Parent.Property1Info);
			}
		}

		#endregion

		#region ValidateProperty2

		public void ValidateProperty2()
		{
			ValidateCalculatedProperty(Parent.Property2Info);
		}

		protected void CheckProperty2()
		{
			ErrorIfInvalid(Parent.Property2Info, Parent.States);
			if (Parent.Property2Validation != null)
			{
				Parent.Property2Validation(Parent.Property2Info);
			}
		}

		protected abstract void ErrorIfInvalid(ZPropertyInfo propertyInfo, IList states);

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			ValidateProperty1();
			ValidateProperty2();
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}
		protected readonly CountryStateModuleFilter<T> Parent;

		#endregion
	}

	#endregion
}
