using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(EntryStartEndDates))]
	public class EntryStartEndDatesTest : NonPersistentBusinessObjectTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestValidation()
		{
			var startEnd = new EntryStartEndDates(ZDate.Today, ZDate.Today.AddMonths(6));

			AssertNoErrors(startEnd.StartInfo);
			AssertNoWarnings(startEnd.StartInfo);
			startEnd.Start = ZDate.Invalid;
			AssertHasError(startEnd.StartInfo, "Enter a valid selection.");
			startEnd.Start = ZDate.Today.AddYears(10);
			AssertHasError(startEnd.StartInfo, string.Format("The date '{0}' is more than 5 years from now and thus is not valid.", startEnd.Start.ToString("dd-MMM-yyyy")));

			AssertNoErrors(startEnd.EndInfo);
			AssertNoWarnings(startEnd.EndInfo);
			startEnd.End = ZDate.Invalid;
			AssertHasError(startEnd.EndInfo, "Enter a valid selection.");
			startEnd.End = ZDate.Today.AddYears(10);
			AssertHasError(startEnd.EndInfo, string.Format("The date '{0}' is more than 5 years from now and thus is not valid.", startEnd.End.ToString("dd-MMM-yyyy")));
		}

		public void TestStartEndReadOnly()
		{
			var startEnd = new EntryStartEndDates(ZDate.Today, ZDate.Today.AddMonths(6));

			Assert(!startEnd.StartInfo.ReadOnly);
			startEnd.UpdateStart = false;
			Assert(startEnd.StartInfo.ReadOnly);
			startEnd.UpdateStart = true;
			Assert(!startEnd.StartInfo.ReadOnly);

			Assert(!startEnd.EndInfo.ReadOnly);
			startEnd.UpdateEnd = false;
			Assert(startEnd.EndInfo.ReadOnly);
			startEnd.UpdateEnd = true;
			Assert(!startEnd.EndInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EntryStartEndDates(ZDate.Today, ZDate.Today.AddMonths(6));
		}
	}
}
