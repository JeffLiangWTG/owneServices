using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	#region OrganisationDefaults Classes

	public class OrganisationDefaults : IEnumerable
	{
		public OrganisationDefaults()
		{
			fDefaultList = new Dictionary<string, OrgFieldDefault>();
		}

		#region Add Methods

		public void Add(OrgFieldDefault @default)
		{
			fDefaultList[@default.FieldName] = @default;
		}

		public void Add(ZString fieldName, IZType value)
		{
			Add(fieldName, value, true);
		}

		public void Add(ZString fieldName, IZType value, ZBool includeInValidation)
		{
			Add(new OrgFieldDefault { FieldName = fieldName, Value = value, IncludeInValidation = includeInValidation });
		}

		public void Add(ZString fieldName, ZString value, ZBool includeInValidation)
		{
			Add(new OrgFieldDefault { FieldName = fieldName, Value = value, IncludeInValidation = includeInValidation });
		}

		public void Add(ZString fieldName, ZString value)
		{
			Add(fieldName, value, true);
		}

		public void Add(ZString fieldName, ZBool value)
		{
			Add(fieldName, value, true);
		}

		public void Add(ZString fieldName, ZBool value, ZBool includeInValidation)
		{
			OrgFieldDefault @default = new OrgFieldDefault { FieldName = fieldName, Value = value, IncludeInValidation = includeInValidation };
			Add(@default);
		}

		public void Add(ZString fieldName, ZGuid value)
		{
			Add(fieldName, value, true);
		}

		public void Add(ZString fieldName, ZGuid value, ZBool includeInValidation)
		{
			OrgFieldDefault @default = new OrgFieldDefault { FieldName = fieldName, Value = value, IncludeInValidation = includeInValidation };
			Add(@default);
		}

		#endregion

		#region Reset

		public void RemoveDefaultsFromUnmatchedNote()
		{
			var keys = fDefaultList.Keys.ToArray();
			foreach (var key in keys)
			{
				if (fDefaultList[key].IsAddedFromUnMatchedNote)
				{
					fDefaultList.Remove(key);
				}
			}
		}

		#endregion

		public OrgFieldDefault GetDefaultByFieldName(ZString fieldName)
		{
			OrgFieldDefault result = null;
			fDefaultList.TryGetValue(fieldName, out result);
			return result;
		}

		public int Count
		{
			get { return fDefaultList.Count; }
		}

		#region Implementation

		protected Dictionary<string, OrgFieldDefault> fDefaultList;
		protected bool HaveConditionalOrgDetailDefaults;

		IEnumerator IEnumerable.GetEnumerator()
		{
			return fDefaultList.Values.GetEnumerator();
		}

		#endregion
	}

	public class OrgFieldDefault : IOrgFieldDefault
	{
		public ZString FieldName { get; set; }
		public IZType Value { get; set; }
		public ZBool IncludeInValidation { get; set; }
		public bool IsConditional { get; set; }
		public bool IsAddedFromUnMatchedNote { get; set; }
	}

	#endregion

	[ModuleID(ModuleId.Organisation)]
	public class OrganisationsFindBoxCollection : OrgHeaderCollection, IValidateForController, IOrganisationDefaultProvider
	{
		public OrganisationsFindBoxCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			fDefaultsForNewChild = new OrganisationDefaults();
			SetFilterBusinessObjectDefaults();
		}

		public OrganisationsFindBoxCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
			fDefaultsForNewChild = new OrganisationDefaults();
			SetFilterBusinessObjectDefaults();
		}

		public OrganisationsFindBoxCollection(BusinessObjectFactory factory, OrganisationDefaults defaults)
			: base(factory)
		{
			DefaultsForNewChild = defaults;
			SetFilterBusinessObjectDefaults();
		}

		public OrganisationsFindBoxCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults)
			: base(factory, filter)
		{
			DefaultsForNewChild = defaults;
			SetFilterBusinessObjectDefaults();
		}

		public bool ShouldApplyActiveFilter { get; set; } = true;

		protected virtual void SetFilterBusinessObjectDefaults()
		{
		}

		public List<IOrgFieldDefault> GetConditionalOrgFieldDefaults()
		{
			return new List<IOrgFieldDefault>(DefaultsForNewChild.Cast<OrgFieldDefault>().Where(def => def.IsConditional).ToList().OfType<IOrgFieldDefault>());
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var filter = base.CreateAdditionalFilter();

			if (ShouldApplyActiveFilter)
			{
				filter.AddToFilter(new ZQuery(OrgHeaderSchema.OH_IsActive, true));
			}

			return filter;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (!((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("4cdfbcb4-6855-4d67-85aa-8a3fc47d8644", "An Organization selected from here must be active."));
			}
		}

		public override bool AllowNewTemporaryOrganisations => true;

		public bool AllowOtherOrgTypes { get; set; }

		#region Set Defaults for New Child

		protected OrganisationDefaults fDefaultsForNewChild;
		public OrganisationDefaults DefaultsForNewChild
		{
			get { return fDefaultsForNewChild; }
			set
			{
				ValidateDefaultForDeveloper(value);
				fDefaultsForNewChild = value;
			}
		}

		[Conditional("DEBUG")]
		void ValidateDefaultForDeveloper(OrganisationDefaults value)
		{
			OrgHeader tempOrg = null;
			foreach (OrgFieldDefault @default in value)
			{
				if (tempOrg == null)
				{
					tempOrg = Factory.New<OrgHeader>();
				}

				BusinessObject bizO = null;
				switch (CargoWise.Schema.Schema.GetPrefixFromColumnName(@default.FieldName))
				{
					case OrgHeaderSchema.Constants.Prefix:
						bizO = tempOrg;
						break;
					case OrgMiscServSchema.Constants.Prefix:
						bizO = tempOrg.MiscServ;
						break;
					case OrgCompanyDataSchema.Constants.Prefix:
						bizO = tempOrg.CompanyData;
						break;
					case OrgAppointedAgentPortsSchema.Constants.Prefix:
						bizO = tempOrg.AppointedAgentPorts.AddNew();
						break;
					case OrgAddressSchema.Constants.Prefix:
						bizO = tempOrg.MainAddress;
						break;
					default:
						throw new Exception("Unknown prefix");
				}

				if (!bizO.ZPropertyInfoHash.ContainsKey(@default.FieldName))
				{
					throw new Exception("Invalid field name for defaults - " + @default.FieldName);
				}
				object o = bizO[@default.FieldName];
			}

			if (tempOrg != null)
			{
				tempOrg.Delete();
			}
		}

#if DEBUG
		protected virtual
#endif
 bool ShouldRegenerateCodeWhenOrgTypesChange
		{
			get { return true; }
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			child.ShowWarningIfCancelled = true;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child1)
		{
			base.SetDefaultsForNewChild(child1);
			var child = (OrgHeader)child1;
			(child).RegenerateCodeWhenOrgTypesChange = ShouldRegenerateCodeWhenOrgTypesChange;

			if (fDefaultsForNewChild != null)
			{
				foreach (OrgFieldDefault @default in fDefaultsForNewChild)
				{
					if (!(@default.IsConditional) || ShouldSetValuesFromConditionalDefaults)
					{
						switch (CargoWise.Schema.Schema.GetPrefixFromColumnName(@default.FieldName))
						{
							case OrgHeaderSchema.Constants.Prefix:
								child1[@default.FieldName] = @default.Value;
								break;
							case OrgMiscServSchema.Constants.Prefix:
								(child).MiscServ[@default.FieldName] = @default.Value;
								break;
							case OrgCompanyDataSchema.Constants.Prefix:
								(child).CompanyData[@default.FieldName] = @default.Value;
								break;
							case OrgAppointedAgentPortsSchema.Constants.Prefix:
								break;
							case OrgAddressSchema.Constants.Prefix:
								(child).MainAddress[@default.FieldName] = @default.Value;
								break;
							default:
								throw new Exception("Invalid field name for defaults - " + @default.FieldName);
						}
					}
				}
			}
		}

		#endregion

		#region IValidateForController Members

		public virtual void ValidateEntityOnSaving(IBusiness entity)
		{
			OrgHeader organisation = (OrgHeader)entity;
			foreach (OrgFieldDefault defaultValue in fDefaultsForNewChild)
			{
				if (defaultValue.IncludeInValidation)
				{
					ZPropertyInfo propertyInfo;
					switch (CargoWise.Schema.Schema.GetPrefixFromColumnName(defaultValue.FieldName))
					{
						case OrgHeaderSchema.Constants.Prefix:
							propertyInfo = organisation.ZPropertyInfoHash[defaultValue.FieldName];
							if (!organisation[defaultValue.FieldName].Equals(defaultValue.Value))
							{
								AddChangedDefaultError(propertyInfo);
							}
							break;

						case OrgMiscServSchema.Constants.Prefix:
							propertyInfo = organisation.MiscServ.ZPropertyInfoHash[defaultValue.FieldName];
							if (!organisation.MiscServ[defaultValue.FieldName].Equals(defaultValue.Value))
							{
								AddChangedDefaultError(propertyInfo);
							}
							break;

						case OrgCompanyDataSchema.Constants.Prefix:
							propertyInfo = organisation.CompanyData.ZPropertyInfoHash[defaultValue.FieldName];
							if (!organisation.CompanyData[defaultValue.FieldName].Equals(defaultValue.Value))
							{
								AddChangedDefaultError(propertyInfo);
							}
							break;

						case OrgAppointedAgentPortsSchema.Constants.Prefix:
							break;

						case OrgAddressSchema.Constants.Prefix:
							propertyInfo = organisation.MainAddress.ZPropertyInfoHash[defaultValue.FieldName];
							if (organisation.MainAddress[defaultValue.FieldName].Equals(defaultValue.Value))
							{
								AddChangedDefaultError(propertyInfo);
							}
							break;

						default:
							throw new InvalidOperationException("Invalid field name for defaults - " + defaultValue.FieldName);
					}
				}
			}
		}

		void AddChangedDefaultError(ZPropertyInfo info)
		{
			this.InfoToValidate = info;
			try
			{
				info.AdditionalValidation += new RunValidationInvoker(info_AdditionalValidation);
				((IBusinessObjectInternals)info.BizObj).Validate(info);
			}
			finally
			{
				this.InfoToValidate = null;
				info.AdditionalValidation -= new RunValidationInvoker(info_AdditionalValidation);
			}
		}

		ZPropertyInfo InfoToValidate;

		void info_AdditionalValidation()
		{
			if (InfoToValidate != null)
			{
				InfoToValidate.AddError(Res.GetString("30f2e184-109d-4387-a7b5-546a421a5fb4", "The value of this field has been changed from the required default value."));
			}
		}

		#endregion

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new OrganisationsFindBoxListProvider(this); }
		}

		#region IOrganisationDefaultProvider Members

		public bool ShouldSetValuesFromConditionalDefaults
		{
			get;
			set;
		}

		public OrganisationTypes OrganisationType { get; set; }

		public ZString OrganisationSubType { get; set; }

		public ZString DocAddressType { get; set; }

		List<IOrgFieldDefault> IOrganisationDefaultProvider.ConditionalDefaults
		{
			get { return GetConditionalOrgFieldDefaults(); }
		}

		#endregion

	}
}
