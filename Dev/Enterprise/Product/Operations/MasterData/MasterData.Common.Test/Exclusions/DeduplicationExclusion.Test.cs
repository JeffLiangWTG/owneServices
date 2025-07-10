using System;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.MasterData.Common.Test
{
	public class DeduplicationExclusionTest : TestCase
	{
		public void TestBuildDisplay()
		{
			var exclusion = new DeduplicationExclusion();

			exclusion["ABCDEF"] = "Excluded by Inactive";
			exclusion["ZEEWEW"] = "Excluded by other countries";
			exclusion["ABCDEF"] = "Excluded by Inactive";

			var displayInfo = exclusion.BuildDisplay();
			var expected = @"
   ABCDEF: Excluded by Inactive    
   ZEEWEW: Excluded by other countries    
 ";

			AssertMultilineASCIIEquals(expected, displayInfo);
			AssertEquals(2, exclusion.Count);
		}

		public void TestBuildDisplay_Sorted()
		{
			var exclusion = new DeduplicationExclusion();

			exclusion["ZEEWEW"] = "Excluded by other countries";
			exclusion["ABCDEF"] = "Excluded by Inactive";

			var displayInfo = exclusion.BuildDisplay();
			var expectedResultIsOrdered = @"
   ABCDEF: Excluded by Inactive    
   ZEEWEW: Excluded by other countries    
 ";

			AssertMultilineASCIIEquals(expectedResultIsOrdered, displayInfo);
			AssertEquals(2, exclusion.Count);
		}

		public void TestBuildDisplay_WithMaxRecordSize()
		{
			var exclusion = new DeduplicationExclusionForTest();

			exclusion.GetMaxRecordToBuild();

			exclusion["ABCDEF"] = "Excluded by Inactive";
			exclusion["ZEEWEW"] = "Excluded by other countries";
			exclusion["ABCDEF"] = "Excluded by Inactive";
			exclusion["BCDERE"] = "Excluded temporary by you";
			exclusion["EFEFEA"] = "Excluded temporary by you";
			exclusion["TTEESE"] = "Excluded temporary for everyone";

			var displayInfo = exclusion.BuildDisplay();
			var expected = $@"
  Showing details of the first {exclusion.maxRecordToBuild} records

   ABCDEF: Excluded by Inactive    
   BCDERE: Excluded temporary by you    
   EFEFEA: Excluded temporary by you    
   TTEESE: Excluded temporary for everyone    
 ";

			AssertMultilineASCIIEquals(expected, displayInfo);
			AssertEquals(5, exclusion.Count);

			var lines = displayInfo.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Length > 2 && !x.Contains("Showing")).ToArray();

			AssertEquals(4, lines.Length);
		}
	}

	#region Implementation

	public class DeduplicationExclusionForTest : DeduplicationExclusion
	{
		public void GetMaxRecordToBuild()
		{
			maxRecordToBuild = 4;
		}
	}

	#endregion
}
