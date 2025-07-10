using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Transhipment.Module
{
	public class CusInBondHeaderFilterLookups
	{
		public CusInBondHeaderFilterLookups(CusInBondHeaderFilterStripBusinessObject filterBizObj)
		{
			this.filterBizObj = filterBizObj;
		}

		readonly CusInBondHeaderFilterStripBusinessObject filterBizObj;

		public ConsigneeCollection ImporterList => new ConsigneeCollection(filterBizObj.Factory);
	}
}
