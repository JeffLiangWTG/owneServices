using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefUnlocoIATACollection))]
	sealed class RefUnlocoIATACollectionTest : ActiveBusinessObjectCollectionTestCase<RefUnlocoIATACollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var loco = Factory.New<RefUnlocoIATA>();
			loco.RL_Code = "XXYYY";
			loco.RL_IATA = "ZZZ";
			return loco;
		}
	}
}
