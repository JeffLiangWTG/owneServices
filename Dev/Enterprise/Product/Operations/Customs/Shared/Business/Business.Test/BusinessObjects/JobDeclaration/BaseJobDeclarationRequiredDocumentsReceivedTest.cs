using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobDeclarationRequiredDocumentsReceivedTest : JobRequiredDocumentsReceivedTest<BaseJobDeclaration>
	{
		protected override void CreateNewParent(out ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent)
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.JE_RL_NKOrigin = "SGSIN";

			pk = declaration.PK;
			requiredDocumentsParent = declaration.DocsAndCartage;
			logsParent = declaration;
		}

		protected override void ReloadParent(BusinessObjectFactory factory, ZGuid pk, out IHaveRequiredDocuments requiredDocumentsParent, out IStmALogParent logsParent)
		{
			var declaration = factory.Load<BaseJobDeclaration>(pk);
			requiredDocumentsParent = declaration.DocsAndCartage;
			logsParent = declaration;
		}

		protected override void ModifyParent(BaseJobDeclaration parent)
		{
			parent.JE_AgentsReference = "aaaa";
		}
	}
}
