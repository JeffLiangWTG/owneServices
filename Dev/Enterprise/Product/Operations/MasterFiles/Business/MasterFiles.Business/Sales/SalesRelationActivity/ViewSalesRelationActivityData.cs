using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ViewSalesRelationActivityData : AutoViewSalesRelationActivityData
	{
		public ViewSalesRelationActivityData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool CanDelete
		{
			get { return false; }
		}
	}
}
