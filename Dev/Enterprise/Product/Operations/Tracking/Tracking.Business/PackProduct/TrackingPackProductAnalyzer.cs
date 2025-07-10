using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingPackProductAnalyzer : WebBusinessObjectAnalyzer
	{
		#region Overrides

		protected override bool IsNewAndDefaultCore(BusinessObject bO)
		{
			if (bO is PackProduct)
			{
				PackProduct packProductBO = (PackProduct)bO;
				return (!(bO.IsInDatabase || bO.IsDeleted) && packProductBO.D2_ProductCode.IsEmpty && packProductBO.D2_ProductQuantity.IsEmpty && packProductBO.OrderLine == null);
			}
			return base.IsNewAndDefaultCore(bO);
		}

		#endregion
	}
}
