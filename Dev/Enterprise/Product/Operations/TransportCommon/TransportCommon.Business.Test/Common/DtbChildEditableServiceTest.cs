using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportCommon.Business.Testing
{
	class DtbChildEditableServiceTest : TestCaseWithFactory
	{
		public void TestChildEditableServiceStates()
		{
			var service = new DtbChildEditableService();
			AssertEquals("GetState, default behaviour", DtbChildEditableServiceState.None, DtbChildEditableService.GetState(Factory));

			foreach (DtbChildEditableServiceState state in Enum.GetValues(typeof(DtbChildEditableServiceState)))
			{
				DtbChildEditableService.SetState(Factory, state);
				AssertEquals("GetState", state, DtbChildEditableService.GetState(Factory));
			}
		}
	}
}
