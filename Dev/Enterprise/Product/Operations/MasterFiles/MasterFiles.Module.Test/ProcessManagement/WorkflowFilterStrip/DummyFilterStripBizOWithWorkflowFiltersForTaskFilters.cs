using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DummyFilterStripBizOWithWorkflowFiltersForTaskFilters : DummyFilterStripBusinessObject
	{
		public DummyFilterStripBizOWithWorkflowFiltersForTaskFilters()
		{
			QueryObjectType = businessObjectType;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(new WorkflowFilterStripsHelperForTest(businessObjectType, ZString.Empty, Factory));

			return helpers;
		}

		readonly Type businessObjectType = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>();
	}
}
