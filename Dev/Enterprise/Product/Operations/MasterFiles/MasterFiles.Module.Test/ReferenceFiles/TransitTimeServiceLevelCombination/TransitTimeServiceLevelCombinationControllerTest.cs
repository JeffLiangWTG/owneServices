using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(TransitTimeServiceLevelCombinationController))]
	class TransitTimeServiceLevelCombinationControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var transitTime = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime.RTT_RS_NKServiceLevel = "STD";
			Factory.Save();

			var view = Factory.LoadTop1<TransitTimeServiceLevelCombinationView>(new ZQuery(TransitTimeServiceLevelCombinationViewSchema.TSC_RTT, SQLComparisonOperator.Equal, transitTime.PK));

			return view;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.TransitTimeServiceLevelCombination;
		}
	}
}
