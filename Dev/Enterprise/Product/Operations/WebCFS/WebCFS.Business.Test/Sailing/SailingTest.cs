using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.WebCFS.Business
{
	[TestedType(typeof(Sailing))]
	public class SailingTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_JV = voyage.PK;
			origin.JA_RL_NKPortOfLoading = new ZString("AUSYD");

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_JV = voyage.PK;
			destination.JB_RL_NKPortOfDischarge = new ZString("USLAX");

			Sailing sailing = Factory.New<Sailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			Factory.Save();

			return sailing;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Not required for business objects based on views", true);
		}
	}
}
