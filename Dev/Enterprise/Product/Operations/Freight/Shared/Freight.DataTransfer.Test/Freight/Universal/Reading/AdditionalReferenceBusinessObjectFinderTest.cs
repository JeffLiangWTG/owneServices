using System;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AdditionalReferenceBusinessObjectFinder))]
	sealed class AdditionalReferenceBusinessObjectFinderTest : MatchingBusinessObjectFinderTest
	{
		public override void TestFind()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new AdditionalReferenceBusinessObjectFinder(null));

			var dataObject = new AdditionalReference();
			dataObject.Type = new EntryType { Code = "XXX" };
			dataObject.ReferenceNumber = "666";

			var cusEntryNumber1 = Factory.New<CusEntryNumber>();
			cusEntryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			cusEntryNumber1.CE_EntryType = "CAR";
			cusEntryNumber1.CE_EntryNum = "111";

			var cusEntryNumber2 = Factory.New<CusEntryNumber>();
			cusEntryNumber2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			cusEntryNumber2.CE_EntryType = "CAR";
			cusEntryNumber2.CE_EntryNum = "222";

			var cusEntryNumber3 = Factory.New<CusEntryNumber>();
			cusEntryNumber3.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			cusEntryNumber3.CE_EntryType = "BKG";
			cusEntryNumber3.CE_EntryNum = "222";

			var cusEntryNumber4 = Factory.New<CusEntryNumber>();
			cusEntryNumber4.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			cusEntryNumber4.CE_EntryType = "ZZZ";
			cusEntryNumber4.CE_EntryNum = "444";

			var nonUniqueNumbers = cusEntryNumber1.Lookups.AdditionalReferenceNumberTypes.NonUnique();

			AssertCollectionContains("prerequisite", "CAR", nonUniqueNumbers);
			AssertCollectionNotContains("prerequisite", "ZZZ", nonUniqueNumbers);

			var finder = new AdditionalReferenceBusinessObjectFinder(dataObject);
			AssertEquals(null, finder.Find(new[] { cusEntryNumber1, cusEntryNumber2, cusEntryNumber3, cusEntryNumber4 }));

			dataObject.Type = new EntryType { Code = "CAR" };

			finder = new AdditionalReferenceBusinessObjectFinder(dataObject);
			AssertEquals(null, finder.Find(new[] { cusEntryNumber1, cusEntryNumber2, cusEntryNumber3, cusEntryNumber4 }));

			dataObject.Type = new EntryType { Code = "CAR" };
			dataObject.ReferenceNumber = "222";

			finder = new AdditionalReferenceBusinessObjectFinder(dataObject);
			AssertEquals(cusEntryNumber2, finder.Find(new[] { cusEntryNumber1, cusEntryNumber2, cusEntryNumber3, cusEntryNumber4 }));

			dataObject.Type = new EntryType { Code = "ZZZ" };
			finder = new AdditionalReferenceBusinessObjectFinder(dataObject);
			AssertEquals(cusEntryNumber4, finder.Find(new[] { cusEntryNumber1, cusEntryNumber2, cusEntryNumber3, cusEntryNumber4 }));
		}

		public void TestFind_SZBNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				var dataObject = new AdditionalReference()
				{
					Type = new EntryType { Code = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber },
					ReferenceNumber = ""
				};

				var entryNumber1 = Factory.New<CusEntryNumber>();
				entryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
				entryNumber1.CE_EntryType = "CON";
				entryNumber1.CE_EntryNum = "111";

				var entryNumber2 = Factory.New<CusEntryNumber>();
				entryNumber2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
				entryNumber2.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
				entryNumber2.CE_EntryIsSystemGenerated = false;
				entryNumber2.CE_EntryNum = "222";

				var finder = new AdditionalReferenceBusinessObjectFinder(dataObject);
				AssertEquals(null, finder.Find(new[] { entryNumber1, entryNumber2 }));

				var entryNumber3 = Factory.New<CusEntryNumber>();
				entryNumber3.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
				entryNumber3.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
				entryNumber3.CE_EntryIsSystemGenerated = true;
				entryNumber3.CE_EntryNum = "333";

				AssertEquals(entryNumber3, finder.Find(new[] { entryNumber1, entryNumber2, entryNumber3 }));
			}
		}
	}
}
