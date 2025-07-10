using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class LoadListDeliveryCartageTypeTest : LoadListCartageTypeTest
	{
		protected override LoadListCartageType GetLoadListCartageType(CFSLoadListConsol loadList)
		{
			return new LoadListDeliveryCartageType(loadList);
		}

		#region TestCartageJobType

		public void TestCartageJobType()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			loadList.JK_RL_NKLoadPort = "USLAX";
			loadList.JK_RL_NKDischargePort = "AUSYD";
			var cartageType = new LoadListDeliveryCartageType(loadList);
			AssertEquals(Constants.CartageJobType.NEW_FCLCTOtoCFS, cartageType.CartageJobType);

			loadList.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals(Constants.CartageJobType.NEW_AirImportLooseCTOtoCFS, cartageType.CartageJobType);

			loadList.Containers.AddNew();
			AssertEquals(Constants.CartageJobType.NEW_AirImportULDCTOtoCFS, cartageType.CartageJobType);

			loadList.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals(Constants.CartageJobType.NEW_FCLImportUnpack, cartageType.CartageJobType);
		}

		#endregion

		public void TestGetMatchingDirectionCodes()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			var cartageType = new LoadListDeliveryCartageType(loadList);
			AssertArrayEqualsByElements("GetMatchingDirectionCodes() returned the wrong values", new ZString[] { "IMP", "DST" }, cartageType.GetMatchingDirectionCodes().ToArray());
		}
	}
}
