/** TODO: ToBeCleaned : Obsoleted Code #Victor 20160210 Dead Code Reported
using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusEntryLineAmendment
	{
		public CusEntryLineAmendment(ICusEntryLine OriginalLine, ICusEntryLine ModifiedLine)
		{
			fOriginalLine = OriginalLine;
			fModifiedLine = ModifiedLine;
		}

		public ICusEntryLine OriginalLine
		{
			get { return fOriginalLine; }
		}

		public ICusEntryLine ModifiedLine
		{
			get { return fModifiedLine; }
		}

		readonly ICusEntryLine fOriginalLine;
		readonly ICusEntryLine fModifiedLine;
	}
}

#region Test

#if DEBUG

namespace Enterprise.Customs.Business.Testing
{
	using NUnit.Framework;
	using Enterprise.ZArchitecture.Business.Testing;

	public class CusEntryLineAmendmentTest : TestCaseWithFactory
	{
		public void TestConstructionFromCusEntryLine()
		{
			CusEntryLine OriginalLine = Factory.New<CusEntryLine>();
			CusEntryLine ModifiedLine = Factory.New<CusEntryLine>();

			CusEntryLineAmendment Amendment = new CusEntryLineAmendment(OriginalLine, ModifiedLine);

			AssertEquals("Value copied correctly", OriginalLine, Amendment.OriginalLine);
			AssertEquals("Value copied correctly", ModifiedLine, Amendment.ModifiedLine);
		}
	}
}

#endif

#endregion

**/
