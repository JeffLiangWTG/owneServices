using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.LinerAndAgency.Testing
{
	interface ILinerAndAgenycBasePageForTest : IDisposable
	{
		void SetupBookedContainersGridForTest();
		ZDataGrid BookedContainersGridForTest { get; }
		void SetupPackLinesGridForTest(string packMode);
		ZDataGrid PacksGridForTest { get; }
	}
}
