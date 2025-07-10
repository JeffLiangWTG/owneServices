using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CustomsTemporaryStorageConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		public void TestBizOType()
		{
			var consumer = GetJobInvoicingConsumerType();
			var type = consumer.BizoType;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.EU.ITemporaryStorageHeader>(), type);
			var ts = Factory.New(type);
			AssertEquals(AsycudaManifestHeaderSchema.Constants.TableName, ts.TableName);
		}

		public void TestApplicationCode()
		{
			var consumer = GetJobInvoicingConsumerType();
			AssertEquals("STO", consumer.Code);
		}

		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType() => JobInvoicingConsumerTypes.CustomsTemporaryStorage;

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceCustoms;

		protected override ControllerID ExpectedControllerID => ControllerIDs.Customs.EU.UCC6TemporaryStorage;
	}
}
