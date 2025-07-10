using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class LoadListPickupCartageTypeTest : LoadListCartageTypeTest
	{
		protected override LoadListCartageType GetLoadListCartageType(CFSLoadListConsol loadList)
		{
			return new LoadListPickupCartageType(loadList);
		}

		#region TestCartageJobType

		public void TestCartageJobType()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			loadList.JK_RL_NKLoadPort = "AUSYD";
			loadList.JK_RL_NKDischargePort = "USLAX";
			var cartageType = new LoadListPickupCartageType(loadList);
			AssertEquals(Constants.CartageJobType.NEW_FCLCFStoCTO, cartageType.CartageJobType);

			loadList.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals(Constants.CartageJobType.NEW_AirExportLooseCFStoCTO, cartageType.CartageJobType);

			loadList.Containers.AddNew();
			AssertEquals(Constants.CartageJobType.NEW_AirExportULDCFStoCTO, cartageType.CartageJobType);

			loadList.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals(Constants.CartageJobType.NEW_FCLExportPack, cartageType.CartageJobType);
		}

		#endregion

		public void TestGetMatchingDirectionCodes()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			var cartageType = new LoadListPickupCartageType(loadList);
			AssertArrayEqualsByElements("GetMatchingDirectionCodes() returned the wrong values", new ZString[] { "EXP", "ORG" }, cartageType.GetMatchingDirectionCodes().ToArray());
		}
	}
}
