namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PartAttribListTest : Customs.Business.Testing.PartAttribListTest<JobComInvoiceLine, OrgSupplierPart, CusClassPartPivot>
	{
		protected override string HTE => ClassificationTypeList.Codes.HTE;

		protected override string HTI => ClassificationTypeList.Codes.HTI;

		protected override string SHB => ClassificationTypeList.Codes.SHB;

		protected override void SetHTI(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
		}

		protected override void SetSHB(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
		}
	}
}
