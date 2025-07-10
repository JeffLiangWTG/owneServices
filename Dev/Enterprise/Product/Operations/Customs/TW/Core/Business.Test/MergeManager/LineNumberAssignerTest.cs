using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class LineNumberAssignerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestTWEntryLineComparer()
		{
			CreateEntryLine("AAA");
			CreateEntryLine("AA");
			CreateEntryLine(ZString.Empty);
			CreateEntryLine("AAA");
			CreateEntryLine(ZString.Empty);
			CreateEntryLine("aA");
			var comparer = new LineNumberAssignerForTest(entryHeader).GetComparer();
			entryHeader.MergedLines.Sort(comparer);
			NUnit.Framework.Assert.That(entryHeader.MergedLines.Count, NUnit.Framework.Is.EqualTo(6));
			NUnit.Framework.Assert.That(entryHeader.MergedLines[0].CL_Grouping, NUnit.Framework.Is.EqualTo("AAA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryHeader.MergedLines[1].CL_Grouping, NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryHeader.MergedLines[2].CL_Grouping, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(entryHeader.MergedLines[3].CL_Grouping, NUnit.Framework.Is.EqualTo("AAA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(entryHeader.MergedLines[4].CL_Grouping, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(entryHeader.MergedLines[5].CL_Grouping, NUnit.Framework.Is.EqualTo("aA").Using(CustomComparers.TypeComparison));
		}

		CusEntryLine CreateEntryLine(ZString grouping)
		{
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Group = grouping;
			invoiceLine.JI_CL = entryLine.PK;
			return entryLine;
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		CusEntryHeader entryHeader;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
		}

		class LineNumberAssignerForTest : Customs.Business.LineNumberAssigner
		{
			public LineNumberAssignerForTest(CusEntryHeader entryHeader) : base(entryHeader)
			{
			}

			public IComparer<Customs.Business.CusEntryLine> GetComparer()
			{
				return GetEntryLineComparerBeforeLineNumbering(EntryHeader);
			}
		}
	}
}
