using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.OceanCarrier.Business
{
	[CodeProperty(Schema.CVT_TransactionId), DescriptionProperty(Schema.CVT_Remark)]
	public sealed class CarrierVoyageTransaction : AutoCarrierVoyageTransaction
	{
		public CarrierVoyageTransaction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnSaving()
		{
			PopulateFormattedNumberPropertyIfRequired(CVT_TransactionIdInfo, Env.NumberFountains.CarrierVoyageTransactionId);
			base.OnSaving();
		}
	}
}
