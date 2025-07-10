using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonShipmentCustomsEntryNumberGeneralTest : TestCaseWithFactory
	{
		public void TestEntryTypeForDisplayWhenCountyCodeIsUS()
		{
			var currentBranchHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USLAX";

				var customsEntryNumber = new CommonShipmentCustomsEntryNumber(Factory.New<CommonShipment>());
				var shipment = customsEntryNumber.Shipment;
				AssertEquals(CusEntryNumberTypeList.Codes.ITN, customsEntryNumber.EntryType);

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_PackingMode = Constants.ContainerModes.Loose;
				AssertEquals(CusEntryNumberTypeList.Codes.ITN, customsEntryNumber.EntryType);

				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				AssertEquals(CusEntryNumberTypes.UnitedStates.CRN, customsEntryNumber.EntryType);
			}
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = currentBranchHomePort;
		}
	}
}
