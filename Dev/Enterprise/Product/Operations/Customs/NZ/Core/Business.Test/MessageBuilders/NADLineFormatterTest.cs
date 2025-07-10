using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.Testing
{
	public class NADLineFormatterTest : TestCaseWithFactory
	{
		public void TestEmptyLineGetsRemoved()
		{
			NADLineFormatter formatter = new NADLineFormatter(Factory
				, "TEST IMPORTER"
				, "104 BOURKE ROAD"
				, ""
				, "SYDNEY"
				, "NSW"
				, "2015", "AU");

			string expectedResult = @"
TEST IMPORTER
104 BOURKE ROAD
SYDNEY NSW 2015
";
			AssertEquals("formatter.ToString()", expectedResult.Trim(), formatter.ToString());
		}

		public void TestGetLine()
		{
			NADLineFormatter formatter = new NADLineFormatter(Factory
				, "TEST IMPORTER"
				, "104 BOURKE ROAD"
				, "ALEXANDRIA"
				, "SYDNEY"
				, "NSW"
				, "2015", "AU");

			AssertEquals("TEST IMPORTER", formatter.GetLine(0));
			AssertEquals("104 BOURKE ROAD", formatter.GetLine(1));
			AssertEquals("ALEXANDRIA", formatter.GetLine(2));
			AssertEquals("SYDNEY NSW 2015", formatter.GetLine(3));
			AssertEquals("", formatter.GetLine(4));
			AssertEquals("", formatter.GetLine(5));
			AssertEquals("", formatter.GetLine(454));
		}

		public void TestShort()
		{
			NADLineFormatter formatter = new NADLineFormatter(Factory
				, "TEST IMPORTER"
				, "104 BOURKE ROAD"
				, "ALEXANDRIA"
				, "SYDNEY"
				, "NSW"
				, "2015", "AU");

			string expectedResult = @"
TEST IMPORTER
104 BOURKE ROAD
ALEXANDRIA
SYDNEY NSW 2015
";
			AssertEquals("formatter.ToString()", expectedResult.Trim(), formatter.ToString());
		}

		public void TestReasonablyLong()
		{
			NADLineFormatter formatter = new NADLineFormatter(Factory
				, "1111 1111 1111 1111 1111 1111 1111"
				, "2222 2222 2222 2222 2222 2222 2222"
				, "3333 3333 3333 3333 3333 3333 3333"
				, "4444 4444 4444 4444 4444"
				, "5555 5555 5555 5555 5555"
				, "6666 6666", "AU");

			string expectedResult = @"
1111 1111 1111 1111 1111 1111 1111
2222 2222 2222 2222 2222 2222 2222
3333 3333 3333 3333 3333 3333 3333
4444 4444 4444 4444 4444 5555 5555
5555 5555 5555 6666 6666
";
			AssertEquals("formatter.ToString()", expectedResult.Trim(), formatter.ToString());
		}

		public void TestReallyLong()
		{
			NADLineFormatter formatter = new NADLineFormatter(Factory
				, "This First Line is long but reasonable."
				, "The second line isn't going to be much different."
				, "Shortish third but long too."
				, "Alexandria"
				, "NSW"
				, "2015", "AU");

			string expectedResult = @"
THIS FIRST LINE IS LONG BUT
REASONABLE., THE SECOND LINE ISNT
GOING TO BE MUCH DIFFERENT.
SHORTISH THIRD BUT LONG TOO.
ALEXANDRIA NSW 2015";
			AssertEquals("formatter.ToString()", expectedResult.Trim(), formatter.ToString());
		}

		public void TestReallyReallyLong()
		{
			NADLineFormatter formatter = new NADLineFormatter(Factory
				, "This First Line is long but reasonable."
				, "The second line isn't going to be much different."
				, "THIRD IS GOING TO BREAK THE BOUNDARIES OF REALITY."
				, "Alexandria"
				, "NSW"
				, "2015", "AU");

			string expectedResult = @"
THIS FIRST LINE IS LONG BUT
REASONABLE., THE SECOND LINE ISNT
GOING TO BE MUCH DIFFERENT. THIRD
IS GOING TO BREAK THE BOUNDARIES OF
REALITY., ALEXANDRIA NSW 2015";
			AssertEquals("formatter.ToString()", expectedResult.Trim(), formatter.ToString());
		}

		public void TestRidiculouslyLong()
		{
			NADLineFormatter formatter = new NADLineFormatter(Factory
				, "This First Line is Going to be quite Long for sure"
				, "The second line isn't going to be much different."
				, "There's not much we can do so we work around it."
				, "At least the City's Short"
				, "And the State is too"
				, "PostCode", "AU");
			string expectedResult = @"
THIS FIRST LINE IS GOING TO BE QUIT
E LONG FOR SURE, THE SECOND LINE IS
NT GOING TO BE MUCH DIFFERENT. THER
ES NOT MUCH WE CAN DO SO WE WORK AR
OUND IT., AT LEAST THE CITYS SHORT 
AND THE STATE IS TOO POSTCODE
";
			AssertEquals("formatter.ToString()", expectedResult.Trim(), formatter.ToString());
		}
	}
}
