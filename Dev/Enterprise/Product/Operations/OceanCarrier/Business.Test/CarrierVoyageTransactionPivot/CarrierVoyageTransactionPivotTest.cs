using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierVoyageTransactionPivot))]
	sealed class CarrierVoyageTransactionPivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var transaction = Factory.NewWithValidTestData<CarrierVoyageTransaction>();
			transaction.CVT_Remark = "123";
			var parentVoyage = Factory.NewWithValidTestData<CarrierVoyage>();
			parentVoyage.CVO_IsPublished = ZBool.False;
			var originVoyage = Factory.NewWithValidTestData<CarrierVoyage>();
			originVoyage.CVO_IsPublished = ZBool.True;

			var carrierVoyageTransactionPivot = Factory.New<CarrierVoyageTransactionPivot>();
			carrierVoyageTransactionPivot.CV2_CVT_Transaction = transaction.PK;
			carrierVoyageTransactionPivot.CV2_ParentType = CarrierVoyageSchema.Constants.Prefix;
			carrierVoyageTransactionPivot.CV2_ParentID = parentVoyage.PK;
			carrierVoyageTransactionPivot.CV2_OriginID = originVoyage.PK;

			return carrierVoyageTransactionPivot;
		}
	}
}
