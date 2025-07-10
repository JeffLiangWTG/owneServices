using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	class PGADisclaimReasonListTest : TestCaseWithFactory
	{
		public void TestGetDisclaimReasonList()
		{
			var list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.AQ1, Factory, false, string.Empty);
			Assert(list.Count == 3);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.D));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.DT1, Factory, false, string.Empty);
			Assert(list.Count == 1);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FD1, Factory, true, EntryTypeList.Codes.Warehouse);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.F));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FD1, Factory, true, EntryTypeList.Codes.ConsumptionFreeDutiable);
			Assert(list.Count == 1);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FD1, Factory, false, EntryTypeList.Codes.Warehouse);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.F));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FD2, Factory, true, EntryTypeList.Codes.ConsumptionFreeDutiable);
			Assert(list.Count == 1);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FD2, Factory, true, EntryTypeList.Codes.Warehouse);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.F));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FD2, Factory, false, EntryTypeList.Codes.Warehouse);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.F));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FD3, Factory, true, EntryTypeList.Codes.ConsumptionFreeDutiable);
			Assert(list.Count == 1);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FD3, Factory, true, EntryTypeList.Codes.Warehouse);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.F));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FD3, Factory, false, EntryTypeList.Codes.Warehouse);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.F));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FD4, Factory, true, EntryTypeList.Codes.ConsumptionFreeDutiable);
			Assert(list.Count == 1);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FD4, Factory, true, EntryTypeList.Codes.Warehouse);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.F));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FD4, Factory, false, EntryTypeList.Codes.Warehouse);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.F));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.DE1, Factory, false, string.Empty);
			Assert(list.Count == 1);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.OM1, Factory, false, string.Empty);
			Assert(list.Count == 1);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.NM1, Factory, false, string.Empty);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.NM3, Factory, false, string.Empty);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.NM5, Factory, false, string.Empty);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.AM1, Factory, false, string.Empty);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.AM3, Factory, false, string.Empty);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.AM7, Factory, false, string.Empty);
			Assert(list.Count == 1);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.NM1, Factory, false, string.Empty);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.NM3, Factory, false, string.Empty);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.NM5, Factory, false, string.Empty);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.CP1, Factory, false, string.Empty);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.CP2, Factory, false, string.Empty);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FW1, Factory, false, string.Empty);
			Assert(list.Count == 3);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.C));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.D));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.E));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FW2, Factory, false, string.Empty);
			Assert(list.Count == 0);

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.FW3, Factory, false, string.Empty);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.C));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.D));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.EP5, Factory, false, string.Empty);
			Assert(list.Count == 3);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.C));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.D));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.TB1, Factory, false, string.Empty);
			Assert(list.Count == 1);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.C));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.TB3, Factory, false, string.Empty);
			Assert(list.Count == 2);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.C));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.AL1, Factory, false, string.Empty);
			Assert(list.Count == 5);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.C));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.D));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.G));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.AL2, Factory, false, string.Empty);
			Assert(list.Count == 4);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.C));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.D));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.G));

			list = PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(OGARequirementList.Codes.AL1, Factory, false, string.Empty);
			Assert(list.Count == 3);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.G));

			list = PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(OGARequirementList.Codes.AL2, Factory, false, string.Empty);
			Assert(list.Count == 3);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.C));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.D));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.G));

			list = PGADisclaimReasonList.GetDisclaimReasonList(ZString.Empty, Factory, false, string.Empty, GovernmentAgencyProgramCodeList.Codes.FWS);
			Assert(list.Count == 3);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.C));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.D));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.E));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.EH1, Factory, false, string.Empty);
			Assert(list.Count == 1);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));

			list = PGADisclaimReasonList.GetDisclaimReasonList(OGARequirementList.Codes.EH2, Factory, false, string.Empty);
			Assert(list.Count == 4);
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.A));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.B));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.C));
			Assert(list.ContainsCode(PGADisclaimReasonList.Codes.D));
		}
	}
}
