using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderRequiredDocumentsReceivedTest : JobRequiredDocumentsReceivedTest<Order>
	{
		protected override void CreateNewParent(out ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent)
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_RL_NKPortOfLoading = "SGSIN";
			order.JD_RL_NKPortOfDischarge = "AUSYD";

			pk = order.PK;
			requiredDocumentsParent = order;
			logsParent = order;
		}

		protected override void ReloadParent(BusinessObjectFactory factory, ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent)
		{
			var order = factory.Load<Order>(pk);
			requiredDocumentsParent = order;
			logsParent = order;
		}
		protected override void ModifyParent(Order parent)
		{
			parent.JD_AdditionalTerms = "aaaa";
		}
	}
}
