using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentLodgementPivot : AutoDtbConsignmentLodgementPivot
	{
		public DtbConsignmentLodgementPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
