using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(EntryNumberCollectionReader<>))]
	sealed class EntryNumberCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var shipment = Factory.New<CommonShipment>();

			var cusEntryNumber1 = shipment.CusEntryNumbersForAllCountries.AddNew();
			cusEntryNumber1.CE_ParentTable = "JobShipment";
			cusEntryNumber1.CE_EntryType = "AAA";
			cusEntryNumber1.CE_EntryNum = "111";
			cusEntryNumber1.CE_RN_NKCountryCode = "AU";
			cusEntryNumber1.CE_IssueDate = new ZDateTime(2012, 2, 1);

			var cusEntryNumber2 = shipment.CusEntryNumbersForAllCountries.AddNew();
			cusEntryNumber2.CE_ParentTable = "JobShipment";
			cusEntryNumber2.CE_EntryType = "BBB";
			cusEntryNumber2.CE_EntryNum = "222";
			cusEntryNumber1.CE_RN_NKCountryCode = "NZ";
			cusEntryNumber1.CE_IssueDate = new ZDateTime(2012, 2, 2);

			Factory.SaveForTesting();

			var entryNumberDataObject1 = CreateDataObject("AAA", "222", "AU", new ZDateTime(2012, 3, 3));
			var entryNumberDataObject2 = CreateDataObject("ZZZ", "123", "NZ", new ZDateTime(2012, 3, 5));

			var numbersCollection = shipment.CusEntryNumbersForAllCountries;

			var logger = new TestErrorLogger();
			var reader = new EntryNumberCollectionReader<CusEntryNumber>(new[]
			{
				entryNumberDataObject1, entryNumberDataObject2
			},
			logger, Factory, shipment, numbersCollection);

			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logs", @"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AAA|111|02-Feb-12", "AAA|222|03-Mar-12", "BBB|222|", "ZZZ|123|05-Mar-12"
			},
			FormatEntryNumbers(shipment));
		}

		#region Implementation

		string[] FormatEntryNumbers(CommonShipment shipment)
		{
			return shipment.CusEntryNumbersForAllCountries
				.Cast<CusEntryNumber>()
				.Select(n => string.Format("{0}|{1}|{2}", n.CE_EntryType, n.CE_EntryNum, n.CE_IssueDate.ToShortDateString(), n.CE_RN_NKCountryCode))
				.ToArray();
		}

		EntryNumber CreateDataObject(ZString code, ZString number, ZString countryCode, ZDateTime issueDate)
		{
			var dataObject = new EntryNumber();
			dataObject.Type = new EntryType { Code = code };
			dataObject.Number = number;
			dataObject.CountryOfIssue = new Country { Code = countryCode };
			dataObject.IssueDate = issueDate;
			return dataObject;
		}

		#endregion
	}
}
