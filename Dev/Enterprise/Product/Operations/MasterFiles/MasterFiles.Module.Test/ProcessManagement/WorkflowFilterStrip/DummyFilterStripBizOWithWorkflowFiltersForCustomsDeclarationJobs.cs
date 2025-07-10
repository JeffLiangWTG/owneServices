using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DummyFilterStripBizOWithWorkflowFiltersForCustomsDeclarationJobs : DummyFilterStripBusinessObject
	{
		public DummyFilterStripBizOWithWorkflowFiltersForCustomsDeclarationJobs()
		{
			QueryObjectType = businessObjectType;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(new WorkflowFilterStripsHelperForTestForCustomsDeclarationJobs(businessObjectType, "BRK", Factory));

			return helpers;
		}

		readonly Type businessObjectType = ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>();
	}
}
