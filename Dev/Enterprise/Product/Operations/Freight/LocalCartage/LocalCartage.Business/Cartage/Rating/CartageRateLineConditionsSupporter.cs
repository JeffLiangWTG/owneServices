using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageRateLineConditionsSupporter : RateLineConditionsSupporter
	{
		public CartageRateLineConditionsSupporter(BusinessObject objectToWrap)
			: base(objectToWrap)
		{
		}

		CommonCartage cartage;
		CommonCartage Cartage
		{
			get { return cartage ?? (cartage = ObjectToWrap as CommonCartage); }
		}

		protected override OrgHeader GetExportBroker()
		{
			return null;
		}

		protected override OrgHeader GetImportBroker()
		{
			return null;
		}

		protected override OrgHeader GetSendingAgent()
		{
			return null;
		}

		protected override OrgHeader GetReceivingAgent()
		{
			return null;
		}

		protected override OrgHeader GetControllingAgent()
		{
			return null;
		}

		protected override OrgHeader GetDepartureCFS()
		{
			return null;
		}

		protected override OrgHeader GetArrivalCFS()
		{
			return null;
		}

		protected override bool GetHasDangerousGoods()
		{
			return Cartage.LooseBookedMoves.Any(move => move.UNDGs.Any()) || Cartage.ContainerBookedMoves.Any(move => move.UNDGs.Any());
		}
	}
}
