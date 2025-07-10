using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefUNLOCO)]
	public class RefUNLOCOCollection : ActiveBusinessObjectCollection<RefUNLOCO>, Integration.IRefUNLOCOCollection
	{
		public RefUNLOCOCollection(BusinessObjectFactory factory) : this(factory, "")
		{
		}

		public RefUNLOCOCollection(BusinessObjectFactory factory, OrgHeader orgHeader) : this(factory, "")
		{
			if (OrganisationsDataRegistry.Instance.EnablePredefinedUNLOCOFilterInOrganizations.Value)
			{
				FilterBusinessObjectDefaults.SetDynamicDefaultFilters(() =>
				{
					AddUserFilters(orgHeader);
				});
			}
		}

		public RefUNLOCOCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public RefUNLOCOCollection(BusinessObjectFactory factory, ZString locoMapSystemUsage)
			: base(factory)
		{
			this.LocoMapSystemUsage = locoMapSystemUsage;
		}

		public RefUNLOCOCollection(BusinessObjectFactory factory, ZQuery filter) : this(factory, filter, "")
		{
		}

		public RefUNLOCOCollection(BusinessObjectFactory factory, ZQuery filter, ZString locoMapSystemUsage)
			: base(factory, filter)
		{
			this.LocoMapSystemUsage = locoMapSystemUsage;
		}

		public void SetPostCodeDefault(bool isDeleteDefaultFilter)
		{
			if (!FilterBusinessObjectDefaults.ContainsDefaultFor(Res.GetString("b492d869-3e55-44cb-85b4-06d04a77f846", "Servicing Postal Code:Property")))
			{
				if (!isDeleteDefaultFilter && (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates
					|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada))
				{
					FilterBusinessObjectDefault filterBODefault = new FilterBusinessObjectDefault("Servicing Postal Code", "Property", (ZString)"");
					FilterBusinessObjectDefaults.Add(filterBODefault);
				}
			}
			else
			{
				if (isDeleteDefaultFilter)
				{
					FilterBusinessObjectDefaults.Remove(Res.GetString("b492d869-3e55-44cb-85b4-06d04a77f846", "Servicing Postal Code:Property"));
				}
			}
		}

		public readonly ZString LocoMapSystemUsage = "";

		#region UserFilters

		void AddUserFilters(OrgHeader orgHeader)
		{
			if (!OrganisationsDataRegistry.Instance.IgnoreUNLOCODefaultingRules.Value)
			{
				AddUNLOCOIdentifierFilter();
			}

			if (orgHeader != null && !orgHeader.IsDeleted && orgHeader.MainAddress != null && !orgHeader.MainAddress.IsDeleted)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("CountryState", "Property1", orgHeader.MainAddress.OA_RN_NKCountryCode));
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("CountryState", "Property2", orgHeader.MainAddress.RelatedState?.PK ?? ZGuid.Empty));
			}
		}

		void AddUNLOCOIdentifierFilter()
		{
			var identifiers = OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.Value.Cast<CodeDescriptionBoolDisallowNewWithDefaultDisabled>();
			var useAnd = OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.Value;

			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("UNLOCO Identifiers", "AndJoinCondition", (ZBool)useAnd));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("UNLOCO Identifiers", "OrJoinCondition", (ZBool)!useAnd));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("UNLOCO Identifiers", "Property0", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasAirport, identifiers)));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("UNLOCO Identifiers", "Property1", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasRail, identifiers)));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("UNLOCO Identifiers", "Property2", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasRoad, identifiers)));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("UNLOCO Identifiers", "Property3", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasSeaport, identifiers)));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("UNLOCO Identifiers", "Property4", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasTerminal, identifiers)));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("UNLOCO Identifiers", "Property5", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasOutport, identifiers)));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("UNLOCO Identifiers", "Property6", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasDischarge, identifiers)));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("UNLOCO Identifiers", "Property7", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasUnload, identifiers)));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("UNLOCO Identifiers", "Property8", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasStore, identifiers)));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("UNLOCO Identifiers", "Property9", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasPost, identifiers)));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("UNLOCO Identifiers", "Property10", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasCustomsLodge, identifiers)));
		}

		ZBool HelperMethodGetIdentifier(string code, System.Collections.Generic.IEnumerable<CodeDescriptionBoolDisallowNewWithDefaultDisabled> identifiers)
		{
			var result = false;

			var regsetting = identifiers.SingleOrDefault(d => d.Code == code);
			if (regsetting != null)
			{
				result = regsetting.Bool;
			}
			return result;
		}

		#endregion

		#region FindBox List Provider

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new UNLOCOFindBoxListProvider(this); }
		}

		class UNLOCOFindBoxListProvider : FindBoxListProvider
		{
			public UNLOCOFindBoxListProvider(ActiveBusinessObjectCollection<RefUNLOCO> list)
				: base(list)
			{
			}

			public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
			{
				string uNLOCO = null;
				bool success = true;

				if (code.Length == 3)
				{
					RefUNLOCO bizObj = RefUNLOCO.LoadFromIATA(List.Factory, code);
					if (bizObj != null)
					{
						uNLOCO = bizObj.RL_Code;
						success = true;
					}
				}

				if (uNLOCO == null)
				{
					if (explicitAutoComplete)
					{
						(uNLOCO, success) = base.NearestMatchCore(code, explicitAutoComplete);
					}
					else
					{
						uNLOCO = code;
						success = false;
					}
				}

				return (uNLOCO, success);
			}

			public override string DescriptionFromCode(string code)
			{
				RefLocoMap localMapping = null;

				ZString systemUsage = ((RefUNLOCOCollection)List).LocoMapSystemUsage;
				if (!systemUsage.IsEmpty)
				{
					localMapping = RefLocoMap.Load(List.Factory, code, GlbCompany.CurrentCompany.Country.PK, systemUsage);
				}

				string result = base.DescriptionFromCode(code);
				ZString localCode = localMapping != null ? localMapping.RY_LocalPortCode : ZString.Empty;
				if (!localCode.IsEmpty)
				{
					result += " (" + localCode + ")";
				}

				return result;
			}

			public override bool AutoCompleteOnCommit
			{
				get { return true; }
			}
		}

		#endregion
	}
}
