using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDynamicWorkOrderDocManagerInfo : DocManagerInfo
	{
		public WhsDynamicWorkOrderDocManagerInfo(WhsDynamicWorkOrder dynamicWorkOrder, ZString docManagerCode)
			: base(dynamicWorkOrder, docManagerCode)
		{
			this.dynamicWorkOrder = dynamicWorkOrder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in WhsDynamicWorkOrderDocManagerInfo")]
		readonly WhsDynamicWorkOrder dynamicWorkOrder;
	}
}
