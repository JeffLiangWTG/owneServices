using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	class CusEntryLineBusinessObjectTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		/***
		 * !!! Test to be removed in production code !!!
		 * Please remove the whole method TestTypeDecider from the production code once the following test case passes.
		 * It is only meant as an initial completeness check immdiately after the country project is set up.
		 ***/
		public void TestTypeDecider()
		{
			AssertType<CusEntryLine>("Update Customs.Business.CusEntryLine to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryLine)));
		}

		// to be overridden once the Tariff is setup for a new country
		protected override ZString ExpectedFallbackEntrylineDescription => "TARIFF_DESCRIPTION";

		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);
	}
}

