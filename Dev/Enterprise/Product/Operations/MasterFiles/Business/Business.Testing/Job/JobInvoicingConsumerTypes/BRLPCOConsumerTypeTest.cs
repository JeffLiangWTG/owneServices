using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class BRLPCOConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		public void TestBizOType()
		{
			var consumer = GetJobInvoicingConsumerType();
			var type = consumer.BizoType;
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.BR.ICusLPCOHeader>(), type);
			var permit = Factory.New(type);
			AssertEquals(CusPermitHeaderSchema.Constants.TableName, permit.TableName);
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceCustoms;

		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType() => JobInvoicingConsumerTypes.BRLPCO;

		protected override ControllerID ExpectedControllerID => ControllerIDs.Customs.BR.LPCO;
	}
}
