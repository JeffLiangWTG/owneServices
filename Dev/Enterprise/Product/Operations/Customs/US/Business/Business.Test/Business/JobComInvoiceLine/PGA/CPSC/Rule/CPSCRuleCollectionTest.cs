using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CPSCRuleCollection))]
	public class CPSCRuleCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNewCore()
		{
			var invoiceLine = Declaration.InvoiceLines.AddNew();
			var header = invoiceLine.CPSCHeaders.AddNew();

			header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			Assert(header.RuleAndLabs.AllowNew);

			header.US_ProcessingCode = CPSCProcessingCodeList.Codes.REF;
			Assert(!header.RuleAndLabs.AllowNew);

			header.US_ProcessingCode = CPSCProcessingCodeList.Codes.FGC;
			Assert(header.RuleAndLabs.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var invoiceLine = Declaration.InvoiceLines.AddNew();
			var header = invoiceLine.CPSCHeaders.AddNew();
			return new CPSCRuleCollection(header);
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
