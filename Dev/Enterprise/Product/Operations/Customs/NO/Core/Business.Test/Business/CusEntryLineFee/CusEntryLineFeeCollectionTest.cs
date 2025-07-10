using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(CusEntryLineFeeCollection))]
	sealed class CusEntryLineFeeCollectionTest : CusEntryLineFeeCollectionTest<CusEntryLineFee, CusEntryLine>
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			return entryLine.Fees;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusEntryLineFee>();
	}
}
