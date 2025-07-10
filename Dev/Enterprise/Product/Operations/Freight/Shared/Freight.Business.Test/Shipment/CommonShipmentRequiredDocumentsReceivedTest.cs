using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonShipmentRequiredDocumentsReceivedTest : JobRequiredDocumentsReceivedTest<CommonShipment>
	{
		protected override void CreateNewParent(out ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent)
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "SGSIN";

			pk = shipment.PK;
			requiredDocumentsParent = shipment.DocsAndCartage;
			logsParent = shipment;
		}

		protected override void ReloadParent(BusinessObjectFactory factory, ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent)
		{
			var shipment = factory.Load<CommonShipment>(pk);
			requiredDocumentsParent = shipment.DocsAndCartage;
			logsParent = shipment;
		}
		protected override void ModifyParent(CommonShipment parent)
		{
			parent.JS_AdditionalTerms = "aaaa";
		}
	}
}
