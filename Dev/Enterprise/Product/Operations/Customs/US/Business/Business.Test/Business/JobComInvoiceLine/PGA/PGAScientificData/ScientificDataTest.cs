using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ScientificData))]
	public class ScientificDataTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<ScientificData>
	{
		public void TestIPG05_PG15()
		{
			ScientificData.US_PGACountryCode = "AD";
			AssertEquals(ScientificData.US_PGACountryCode, iScientificData.CountryCode);

			ScientificData.US_PGAScientificGenusName = "name 1";
			AssertEquals(ScientificData.US_PGAScientificGenusName, iScientificData.GenusName);

			ScientificData.US_PGAScientificSpeciesName = "name 2";
			AssertEquals(ScientificData.US_PGAScientificSpeciesName, iScientificData.SpeciesName);
		}

		public void TestClone()
		{
			ScientificData.US_PGAScientificGenusName = "Genus Name";
			var newObj = (ScientificData)ScientificData.Clone();
			AssertEquals(ScientificData.US_PGAScientificGenusName, newObj.US_PGAScientificGenusName);
		}

		#region Overrides

		protected override IEnumerable<ScientificData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var laceyAct = invoiceLine.LaceyActLines.AddNew();
			var constituentElement = laceyAct.PG04ConstituentElements.AddNew();
			yield return constituentElement.ScientificDataCollection.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return ScientificData;
		}

		#endregion

		IScientificData iScientificData
		{
			get { return ScientificData; }
		}

		ScientificData ScientificData
		{
			get
			{
				if (scientificData == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var laceyAct = invoiceLine.LaceyActLines.AddNew();
					var constituentElement = laceyAct.PG04ConstituentElements.AddNew();
					scientificData = constituentElement.ScientificDataCollection.AddNew();
				}
				return scientificData;
			}
		}
		ScientificData scientificData;
	}
}
