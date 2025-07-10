using System;

namespace Enterprise.Customs._CustomsTemplate_.Business.Testing
{
	class WeightUQCalculatorTest : Customs.Business.Testing.WeightUQCalculatorTest
	{
		protected override Type JobComInvoiceHeaderType => typeof(JobComInvoiceHeader);

		protected override Type CusEntryHeaderType => typeof(CusEntryHeader);
	}
}
