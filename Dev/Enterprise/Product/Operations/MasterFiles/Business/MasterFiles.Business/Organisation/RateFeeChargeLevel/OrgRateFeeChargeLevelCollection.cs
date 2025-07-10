using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRateFeeChargeLevelCollection : ActiveBusinessObjectCollection<OrgRateFeeChargeLevel>
	{
		public OrgRateFeeChargeLevelCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgRateFeeChargeLevelCollection(BusinessObjectFactory factory, OrgHeader master)
			: base(factory, master)
		{
			fMaster = master;
		}

		public OrgRateFeeChargeLevelCollection(OrgHeader master)
			: base(master)
		{
			fMaster = master;
		}

		#region Master

		readonly OrgHeader fMaster;

		#endregion

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = new ZQuery();

			if (fMaster != null)
			{
				filter.AddToFilter(JoinCondition.Or, OrgRateFeeChargeLevelSchema.ORF_OH, SQLComparisonOperator.Equal, fMaster.PK);
			}
			return filter;
		}
	}
}
