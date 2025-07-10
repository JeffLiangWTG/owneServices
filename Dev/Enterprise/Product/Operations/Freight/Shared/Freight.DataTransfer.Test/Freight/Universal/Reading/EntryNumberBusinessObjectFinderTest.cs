using Enterprise.Customs.Common;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(EntryNumberBusinessObjectFinder))]
	sealed class EntryNumberBusinessObjectFinderTest : MatchingBusinessObjectFinderTest
	{
		public override void TestFind()
		{
			var cusEntryNumber1 = Factory.New<CusEntryNumber>();
			cusEntryNumber1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			cusEntryNumber1.CE_EntryType = "AAA";
			cusEntryNumber1.CE_EntryNum = "111";
			cusEntryNumber1.CE_RN_NKCountryCode = "AU";
			cusEntryNumber1.CE_EntryIsSystemGenerated = false;

			var cusEntryNumber2 = Factory.New<CusEntryNumber>();
			cusEntryNumber2.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusEntryNumber2.CE_EntryType = "BBB";
			cusEntryNumber2.CE_EntryNum = "222";
			cusEntryNumber2.CE_RN_NKCountryCode = "AU";
			cusEntryNumber2.CE_EntryIsSystemGenerated = false;

			var cusEntryNumber3 = Factory.New<CusEntryNumber>();
			cusEntryNumber3.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusEntryNumber3.CE_EntryType = "CCC";
			cusEntryNumber3.CE_EntryNum = "333";
			cusEntryNumber3.CE_RN_NKCountryCode = "AU";
			cusEntryNumber3.CE_EntryIsSystemGenerated = false;

			var cusEntryNumber4 = Factory.New<CusEntryNumber>();
			cusEntryNumber4.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusEntryNumber4.CE_EntryType = "DDD";
			cusEntryNumber4.CE_EntryNum = "444";
			cusEntryNumber4.CE_RN_NKCountryCode = "AU";
			cusEntryNumber4.CE_EntryIsSystemGenerated = false;

			var cusEntryNumbers = new[]
			{
				cusEntryNumber1, cusEntryNumber2, cusEntryNumber3, cusEntryNumber4
			};

			var dataObject = new EntryNumber();
			dataObject.Type = new EntryType { Code = "AAA" };
			dataObject.Number = "111";
			dataObject.CountryOfIssue = new Country { Code = "AU", Name = "Australia" };

			var finder = new EntryNumberBusinessObjectFinder(dataObject);
			finder.Find(cusEntryNumbers);
			AssertEquals("Should not match when category is not the same", false, cusEntryNumber1.CE_EntryIsSystemGenerated);

			dataObject.Type = new EntryType { Code = "BBX" };
			dataObject.Number = "222";

			finder = new EntryNumberBusinessObjectFinder(dataObject);
			finder.Find(cusEntryNumbers);
			AssertEquals("Should not match when type is not the same", false, cusEntryNumber2.CE_EntryIsSystemGenerated);

			dataObject.Type = new EntryType { Code = "CCC" };
			dataObject.Number = "333";
			dataObject.CountryOfIssue = new Country { Code = "NZ", Name = "New Zealand" };

			finder = new EntryNumberBusinessObjectFinder(dataObject);
			finder.Find(cusEntryNumbers);
			AssertEquals("Should not match when country is not the same", false, cusEntryNumber3.CE_EntryIsSystemGenerated);

			dataObject.Type = new EntryType { Code = "DDD" };
			dataObject.Number = "444";
			dataObject.CountryOfIssue = new Country { Code = "AU", Name = "Australia" };

			finder = new EntryNumberBusinessObjectFinder(dataObject);
			finder.Find(cusEntryNumbers);
			AssertEquals("Should match and set to readonly when type/category/country are the same", true, cusEntryNumber4.CE_EntryIsSystemGenerated);
		}
	}
}
