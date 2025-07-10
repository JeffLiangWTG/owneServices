using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USDEAConstituentAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWeightUQList()
		{
			AssertNotNull(DEAConstituent.AddInfoLookups.WeightUQList);
		}

		DEAConstituent DEAConstituent
		{
			get
			{
				if (fDEAConstituent == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
					fDEAConstituent = invoiceLine.DEAHeaders.AddNew().Constituents.AddNew();
				}
				return fDEAConstituent;
			}
		}
		DEAConstituent fDEAConstituent;
	}
}
