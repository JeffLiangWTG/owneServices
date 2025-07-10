using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Testing
{
	[TestedType(typeof(ProhibitedCodeCollection))]
	public class ProhibitedCodeCollectionTest : CodeDataPairCollectionTest<ProhibitedCodeCollection>
	{
		protected override ProhibitedCodeCollection GetCollectionToTest()
		{
			return new ProhibitedCodeCollection(Factory, Classification.CC_ProhibitedCodesInfo);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ProhibitedCode(Factory, null);
		}

		protected override CodeDataPairCollection CollectionAgainstDeclaration
		{
			get { return null; }
		}

		protected override CodeDataPairCollection CollectionAgainstInvoiceLine
		{
			get { return InvoiceLine.ProhibitedCodes; }
		}
	}
}
