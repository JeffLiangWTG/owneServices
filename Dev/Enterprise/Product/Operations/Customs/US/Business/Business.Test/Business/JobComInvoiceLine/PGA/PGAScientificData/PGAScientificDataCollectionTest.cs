using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PGAScientificDataCollection))]
	public class PGAScientificDataCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestType()
		{
			AssertEquals(typeof(ScientificData), PGAScientificData.AddNew().GetType());
		}

		public void TestDefaults()
		{
			ScientificData data = PGAScientificData.AddNew();
			AssertEquals("DE", data.US_PGACountryCode);
			AssertEquals("", data.US_PGAScientificGenusName);
			AssertEquals("", data.US_PGAScientificSpeciesName);

			data.US_PGAScientificGenusName = "Genus Name";
			data.US_PGAScientificSpeciesName = "Species Name";
			data = PGAScientificData.AddNew();
			AssertEquals("DE", data.US_PGACountryCode);
			AssertEquals("Genus Name", data.US_PGAScientificGenusName);
			AssertEquals("Species Name", data.US_PGAScientificSpeciesName);
		}

		public void TestUpdateToCountryIfEmptyOrCreate()
		{
			ScientificData data = PGAScientificData.AddNew();
			data.US_PGAScientificGenusName = "Genus Name";
			data.US_PGAScientificSpeciesName = "Species Name";
			data.US_PGACountryCode = "";

			PGAScientificData.UpdateToCountryIfEmptyOrCreate("DE");
			AssertEquals("Still one element", 1, PGAScientificData.Count);
			AssertEquals("Country is updated", "DE", data.US_PGACountryCode);

			PGAScientificData.UpdateToCountryIfEmptyOrCreate("FR");
			AssertEquals("Two elements", 2, PGAScientificData.Count);

			AssertEquals("Data.Genus", "Genus Name", PGAScientificData[0].US_PGAScientificGenusName);
			AssertEquals("Data.Species", "Species Name", PGAScientificData[0].US_PGAScientificSpeciesName);
			AssertEquals("Data.CountryCode", "DE", PGAScientificData[0].US_PGACountryCode);

			AssertEquals("The second data.Genus", "Genus Name", PGAScientificData[1].US_PGAScientificGenusName);
			AssertEquals("The second data.Species", "Species Name", PGAScientificData[1].US_PGAScientificSpeciesName);
			AssertEquals("The second data.CountryCode", "FR", PGAScientificData[1].US_PGACountryCode);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return PGAScientificData;
		}

		PGAScientificDataCollection PGAScientificData
		{
			get { return elements ?? (elements = ConstituentElement.ScientificDataCollection); }
		}
		PGAScientificDataCollection elements;

		ConstituentElement ConstituentElement
		{
			get { return constituentElement ?? (constituentElement = PGA.PG04ConstituentElements.AddNew()); }
		}
		ConstituentElement constituentElement;

		PGA PGA
		{
			get
			{
				if (pga == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
					JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					invoiceLine.US_UC_NKCountryOfOrigin = "DE";
					pga = invoiceLine.LaceyActLines.AddNew();
				}

				return pga;
			}
		}
		PGA pga;

		#endregion
	}
}
