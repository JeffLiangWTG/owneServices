using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyageTransactionPivot : AutoCarrierVoyageTransactionPivot
	{
		public CarrierVoyageTransactionPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CarrierVoyageTransaction CarrierVoyageTransaction => Factory.Load<CarrierVoyageTransaction>(CV2_CVT_Transaction);

		[RelatedBusinessObject(nameof(CarrierVoyageTransaction))]
		public override ZGuid CV2_CVT_Transaction
		{
			get => base.CV2_CVT_Transaction;
			set => base.CV2_CVT_Transaction = value;
		}
	}
}
