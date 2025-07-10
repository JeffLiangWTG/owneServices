using System;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class WeightUQCalculatorTest : Customs.Business.Testing.WeightUQCalculatorTest
	{
		protected override Type JobComInvoiceHeaderType
		{
			get
			{
				return typeof(JobComInvoiceHeader);
			}
		}

		protected override Type CusEntryHeaderType
		{
			get
			{
				return typeof(CusEntryHeader);
			}
		}
	}
}
