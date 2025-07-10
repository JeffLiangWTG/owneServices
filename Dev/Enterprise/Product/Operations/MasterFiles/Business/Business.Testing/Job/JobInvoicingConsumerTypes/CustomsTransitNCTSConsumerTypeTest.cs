using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CustomsTransitNCTSConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		public void TestBizOType()
		{
			var consumer = GetJobInvoicingConsumerType();
			var type = consumer.BizoType;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader>(), type);
			var ncts = Factory.New(type);
			AssertEquals(CusInBondHeaderSchema.Constants.TableName, ncts.TableName);
		}
		public void TestApplicationCode()
		{
			var consumer = GetJobInvoicingConsumerType();
			AssertEquals("NCT", consumer.Code);
		}

		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType() => JobInvoicingConsumerTypes.CustomsTransitNCTS;

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceCustoms;

		protected override ControllerID ExpectedControllerID => ControllerIDs.Customs.EU.NctsMovementController;

		public override void TestExcludeFromClientVisibleOption()
		{
			AssertEquals("NCTS should not have ExcludeFromClientVisibleOption available", false, ConsumerType.ExcludeFromClientVisibleOption);
		}
	}
}
