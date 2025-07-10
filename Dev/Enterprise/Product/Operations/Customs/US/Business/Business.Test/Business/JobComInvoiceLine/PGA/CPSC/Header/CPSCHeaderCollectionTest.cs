using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CPSCHeaderCollection))]
	public class CPSCHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var invoiceLine = Declaration.InvoiceLines.AddNew();
			return new CPSCHeaderCollection(invoiceLine);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				fDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				fDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
	}
}
