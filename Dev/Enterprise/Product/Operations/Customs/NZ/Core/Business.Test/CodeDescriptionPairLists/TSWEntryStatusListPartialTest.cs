namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	class TSWEntryStatusListTest : TestCaseWithFactory
	{
		public void TestIsStatusClear()
		{
			Assert(!TSWEntryStatusList.IsCompletedStatus(TSWEntryStatusList.Codes.CPC, "", ""));
			Assert(!TSWEntryStatusList.IsCompletedStatus(TSWEntryStatusList.Codes.PCC, "", ""));
			Assert(TSWEntryStatusList.IsCompletedStatus(TSWEntryStatusList.Codes.CCC, "", ""));
			Assert(TSWEntryStatusList.IsCompletedStatus(TSWEntryStatusList.Codes.NCC, "", ""));
			Assert(TSWEntryStatusList.IsCompletedStatus(TSWEntryStatusList.Codes.CC, "", ""));
			Assert(!TSWEntryStatusList.IsCompletedStatus(TSWEntryStatusList.Codes.DCA, "", ""));
			Assert(!TSWEntryStatusList.IsCompletedStatus(TSWEntryStatusList.Codes.CAR, "", ""));
			Assert(!TSWEntryStatusList.IsCompletedStatus(TSWEntryStatusList.Codes.STC, "", ""));
			Assert(TSWEntryStatusList.IsCompletedStatus(TSWEntryStatusList.Codes.CLR, "", ""));
			Assert(!TSWEntryStatusList.IsCompletedStatus(LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, "", ""));
			Assert(TSWEntryStatusList.IsCompletedStatus(LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, TSWEntryStatusList.EntryTypes.WriteOff, ""));
			Assert(TSWEntryStatusList.IsCompletedStatus(LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved, TSWEntryStatusList.EntryTypes.WriteOff, ""));
			Assert(TSWEntryStatusList.IsCompletedStatus(TSWEntryStatusList.Codes.CIC, "", "B06"));
			Assert(!TSWEntryStatusList.IsCompletedStatus(TSWEntryStatusList.Codes.CIC, "", "B05"));
			Assert(!TSWEntryStatusList.IsCompletedStatus(TSWEntryStatusList.Codes.PIC, "", "B06"));
			Assert(!TSWEntryStatusList.IsCompletedStatus(TSWEntryStatusList.Codes.CIP, "", "B06"));
		}

		public void TestIsImpedimentStatus()
		{
			Assert(TSWEntryStatusList.IsImpedimentStatus(TSWEntryStatusList.Codes.CIA));
			Assert(!TSWEntryStatusList.IsImpedimentStatus(TSWEntryStatusList.Codes.DCA));
			Assert(TSWEntryStatusList.IsImpedimentStatus(TSWEntryStatusList.Codes.III));
			Assert(TSWEntryStatusList.IsImpedimentStatus(TSWEntryStatusList.Codes.PIE));
			Assert(!TSWEntryStatusList.IsImpedimentStatus(TSWEntryStatusList.Codes.STC));
			Assert(!TSWEntryStatusList.IsImpedimentStatus(TSWEntryStatusList.Codes.CLR));
			Assert(TSWEntryStatusList.IsImpedimentStatus(TSWEntryStatusList.Codes.NCI));
			Assert(TSWEntryStatusList.IsImpedimentStatus(TSWEntryStatusList.Codes.NIC));
			Assert(!TSWEntryStatusList.IsImpedimentStatus(TSWEntryStatusList.Codes.NCP));
		}
	}
}
