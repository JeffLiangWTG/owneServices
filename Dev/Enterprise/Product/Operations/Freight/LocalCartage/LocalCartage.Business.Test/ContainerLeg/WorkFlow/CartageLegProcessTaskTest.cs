using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CartageLegProcessTask))]
	class CartageLegProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonCartageLeg leg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
			return ((IWorkflowProvider)leg).WorkflowItems.AddNew();
		}
	}
}
