using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCountryDataAddressDependentCollection : ActiveBusinessObjectCollection<OrgCountryData>
	{
		public OrgCountryDataAddressDependentCollection(OrgAddress master)
			: base(master.Factory, new DependentRelationship(master, typeof(OrgCountryData), CreateCountryRelationFilter(), OrgCountryDataSchema.OV_OA_ApprovedLocation))
		{
		}

		#region Implementation

		protected override void OnAdded(OrgCountryData orgCountryData)
		{
			base.OnAdded(orgCountryData);
			orgCountryData.AddedThroughCollection = true;
		}

		protected override void OnLoadedIntoCollectionCore(OrgCountryData orgCountryData)
		{
			base.OnLoadedIntoCollectionCore(orgCountryData);
			orgCountryData.AddedThroughCollection = true;
		}

		protected override void SetDefaultsForNewElementCore(OrgCountryData child)
		{
			base.SetDefaultsForNewElementCore(child);
			child.AddedThroughCollection = true;
		}

		static ZQuery CreateCountryRelationFilter()
		{
			var countryOrEUCode = Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(GlbCompany.CurrentCompany.Country.Code)
				? (ZString)Constants.CountryCodes.EuropeanUnion
				: GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			return new ZQuery(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, countryOrEUCode);
		}

		#endregion
	}
}
