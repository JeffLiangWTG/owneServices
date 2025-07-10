using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GovernmentAgencyIDListTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsGovernmentAgencyIDBetweenR9901AndR9920()
		{
			var governmentAgencyIDPrefix = "R99";
			var governmentAgencyID = ZString.Empty;
			for (int i = 1; i <= 20; i++)
			{
				governmentAgencyID = governmentAgencyIDPrefix + i.ToString().PadLeft(2, '0');
				NUnit.Framework.Assert.That(GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9901AndR9920(governmentAgencyID), NUnit.Framework.Is.EqualTo(true), $"the result is true when government gency ID is {governmentAgencyID}");
			}

			governmentAgencyID = "R9900";
			NUnit.Framework.Assert.That(GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9901AndR9920(governmentAgencyID), NUnit.Framework.Is.EqualTo(false), $"the result is false when government gency ID is {governmentAgencyID}");
		}

		[ExpectNoExceptions]
		public void TestGovernmentAgencyDescription()
		{
			NUnit.Framework.Assert.That(new GovernmentAgencyIDList().GetDescriptionFromCode("R9974"), NUnit.Framework.Is.EqualTo("臺北關快遞機放組（聯邦快遞一般貨棧）"), $"Originally belonged to the 備用 code group");
		}

		[ExpectNoExceptions]
		public void TestIsGovernmentAgencyIDBetweenR9921AndR9950()
		{
			var governmentAgencyIDPrefix = "R99";
			var governmentAgencyID = ZString.Empty;
			for (int i = 21; i <= 50; i++)
			{
				governmentAgencyID = governmentAgencyIDPrefix + i.ToString();
				NUnit.Framework.Assert.That(GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9921AndR9950(governmentAgencyID), NUnit.Framework.Is.EqualTo(true), $"the result is true when government gency ID is {governmentAgencyID}");
			}

			governmentAgencyID = "R9900";
			NUnit.Framework.Assert.That(GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9921AndR9950(governmentAgencyID), NUnit.Framework.Is.EqualTo(false), $"the result is false when government gency ID is {governmentAgencyID}");
		}

		[ExpectNoExceptions]
		public void TestIsGovernmentAgencyIDBetweenR9951AndR9980()
		{
			var governmentAgencyIDPrefix = "R99";
			var governmentAgencyID = ZString.Empty;
			for (int i = 51; i <= 80; i++)
			{
				governmentAgencyID = governmentAgencyIDPrefix + i.ToString();
				NUnit.Framework.Assert.That(GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9951AndR9980(governmentAgencyID), NUnit.Framework.Is.EqualTo(true), $"the result is true when government gency ID is {governmentAgencyID}");
			}

			governmentAgencyID = "R9900";
			NUnit.Framework.Assert.That(GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9951AndR9980(governmentAgencyID), NUnit.Framework.Is.EqualTo(false), $"the result is false when government gency ID is {governmentAgencyID}");
		}

		[ExpectNoExceptions]
		public void TestIsGovernmentAgencyIDBetweenR9981AndR9999()
		{
			var governmentAgencyIDPrefix = "R99";
			var governmentAgencyID = ZString.Empty;
			for (int i = 81; i <= 99; i++)
			{
				governmentAgencyID = governmentAgencyIDPrefix + i.ToString();
				NUnit.Framework.Assert.That(GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9981AndR9999(governmentAgencyID), NUnit.Framework.Is.EqualTo(true), $"the result is true when government gency ID is {governmentAgencyID}");
			}

			governmentAgencyID = "R9900";
			NUnit.Framework.Assert.That(GovernmentAgencyIDList.IsGovernmentAgencyIDBetweenR9981AndR9999(governmentAgencyID), NUnit.Framework.Is.EqualTo(false), $"the result is false when government gency ID is {governmentAgencyID}");
		}
	}
}
