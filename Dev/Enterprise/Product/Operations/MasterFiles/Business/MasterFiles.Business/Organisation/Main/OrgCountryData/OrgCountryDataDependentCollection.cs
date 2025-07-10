using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCountryDataDependentCollection : DependentBusinessObjectCollection<OrgCountryData, OrgHeader>
	{
		public OrgCountryDataDependentCollection(OrgHeader parent)
			: base(parent)
		{
		}

		#region Implementation

		protected override void OnLoaded()
		{
			base.OnLoaded();
			foreach (OrgCountryData item in this)
			{
				item.AddedThroughCollection = true;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((OrgCountryData)child).AddedThroughCollection = true;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return OrgCountryDataSchema.OV_OH_OrgHeader; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			ZString countryOrEUCode = Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(GlbCompany.CurrentCompany.Country.Code)
				? (ZString)Constants.CountryCodes.EuropeanUnion
				: GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			query.AddToFilter(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, countryOrEUCode);

			if (Master != null && Master.SupplyChainSecurityConfiguration.IsAddressLevelScheme)
			{
				query.AddToFilter(OrgCountryDataSchema.OV_OA_ApprovedLocation, SQLComparisonOperator.NotEqual, null);
			}

			return query;
		}

		protected override bool EnableRemovingDependentWithoutDeletingErrorReport { get { return true; } }

		#endregion
	}
}
