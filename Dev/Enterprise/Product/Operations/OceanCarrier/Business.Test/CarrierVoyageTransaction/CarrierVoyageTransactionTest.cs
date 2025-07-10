using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierVoyageTransaction))]
	sealed class CarrierVoyageTransactionTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var carrierVoyageTransaction = Factory.New<CarrierVoyageTransaction>();
			carrierVoyageTransaction.CVT_Remark = "My Transaction";
			return carrierVoyageTransaction;
		}

		public void TransactionIdIsPopulatedOnSavingWhenNotProvided()
		{
			var carrierVoyageTransaction1 = Factory.New<CarrierVoyageTransaction>();
			carrierVoyageTransaction1.CVT_Remark = "Manual set TransactionId is kept during save";
			carrierVoyageTransaction1.CVT_TransactionId = "MyTransactionId";

			var carrierVoyageTransaction2 = Factory.New<CarrierVoyageTransaction>();
			carrierVoyageTransaction2.CVT_Remark = "Auto generated TransactionId";
			carrierVoyageTransaction2.CVT_TransactionId = "MyTransactionId";

			Factory.Save();

			AssertEquals("Manual set TransactionId is kept during save", "MyTransactionId", carrierVoyageTransaction1.CVT_TransactionId);
			AssertNotNullOrEmpty("Auto generated TransactionId", carrierVoyageTransaction2.CVT_TransactionId);
		}
	}
}
