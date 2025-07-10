using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolRequiredDocumentsReceivedTest : JobRequiredDocumentsReceivedTest<ForwardingConsol>
	{
		protected override void CreateNewParent(out ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";

			pk = consol.PK;
			requiredDocumentsParent = consol;
			logsParent = consol;
		}

		protected override void ReloadParent(BusinessObjectFactory factory, ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent)
		{
			var consol = factory.Load<ForwardingConsol>(pk);
			requiredDocumentsParent = consol;
			logsParent = consol;
		}
		protected override void ModifyParent(ForwardingConsol parent)
		{
			parent.JK_MasterBillNum = "aaaa";
		}
	}
}
