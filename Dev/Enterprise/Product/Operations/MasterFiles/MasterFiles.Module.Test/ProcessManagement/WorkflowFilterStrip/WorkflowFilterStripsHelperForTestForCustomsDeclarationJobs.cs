using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class WorkflowFilterStripsHelperForTestForCustomsDeclarationJobs : WorkflowFilterStripsHelperForTest
	{
		public WorkflowFilterStripsHelperForTestForCustomsDeclarationJobs(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory)
			: base(businessObjectType, templateCode, factory)
		{
			AlternativeTaskParentColumn = JobDeclarationSchema.JE_JS;
		}
	}
}
