using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyUNDGDataItemLookups : UNDGDataItemLookups
	{
		public AgencyUNDGDataItemLookups(AgencyUNDGDataItem parent) : base(parent)
		{
			if (parent != null)
			{
				var packingLine = Factory.Load<PackLine>(parent.DI_ParentID);
				var parentShipment = packingLine?.Shipment;
				cfrShouldBeDefaulted = AgencyUNDGHelper.CFRShouldBeDefaulted(parentShipment);
			}
		}

		readonly bool cfrShouldBeDefaulted;

		public override UNDGSubstanceCollection UNDGSubstances
		{
			get
			{
				return AddSeaFilter(new AgencyUNDGSubstanceCollection(Factory, cfrShouldBeDefaulted));
			}
		}

		public AgencyUNDGSubstanceCollection AddSeaFilter(AgencyUNDGSubstanceCollection collection)
		{
			if (cfrShouldBeDefaulted)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Standard", "Property", (ZString)UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR, true));
			}
			else
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Standard", "Property", (ZString)UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, true));
			}

			var cfrQuery = new ZQuery(UNDGSubstanceSchema.DG_Standard, SQLComparisonOperator.Equal, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR);
			var filterQuery = new ZQuery(UNDGSubstanceSchema.DG_Standard, SQLComparisonOperator.Equal, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			filterQuery.AddToFilter(cfrQuery, JoinCondition.Or);

			collection.AdditionalFilter = filterQuery;
			return collection;
		}
	}
}
