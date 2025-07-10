using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ChildEditableServiceTest : TestCaseWithFactory
	{
		public void TestChildEditableServiceStates()
		{
			IChildEditableService service = new ChildEditableService();
			AssertEquals("GetState, default behaviour", ChildEditableServiceStates.Consol, ChildEditableService.GetState(Factory));
			AssertEquals("GetStateDirectly, default behaviour", ChildEditableServiceStates.Unknown, ChildEditableService.GetStateDirectly(Factory));
			AssertEquals("IChildEditableService.GetStateDirectly, default behaviour", ChildEditableServiceStates.Unknown, service.GetStateDirectly(Factory));

			foreach (ChildEditableServiceStates state in Enum.GetValues(typeof(ChildEditableServiceStates)))
			{
				if (state != ChildEditableServiceStates.Unknown)
				{
					ChildEditableService.SetState(Factory, state);
					AssertEquals("GetState", state, ChildEditableService.GetState(Factory));
					AssertEquals("GetStateDirectly", state, ChildEditableService.GetStateDirectly(Factory));
					AssertEquals("IChildEditableService.GetStateDirectly", state, service.GetStateDirectly(Factory));
				}
			}
		}
	}
}
