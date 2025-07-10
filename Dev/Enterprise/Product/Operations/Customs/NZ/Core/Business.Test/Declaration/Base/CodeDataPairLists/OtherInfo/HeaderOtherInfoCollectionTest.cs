using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Testing
{
	[TestedType(typeof(HeaderOtherInfoCollection))]
	public class HeaderOtherInfoCollectionTest : OtherInfoCollectionTest<HeaderOtherInfoCollection>
	{
		protected override HeaderOtherInfoCollection GetCollectionToTest()
		{
			return new HeaderOtherInfoCollection(Factory, Classification.CC_AddInfoInfo);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new HeaderOtherInfo(Factory, null);
		}

		protected override CodeDataPairCollection CollectionAgainstDeclaration
		{
			get { return Declaration.OtherInfos; }
		}

		protected override CodeDataPairCollection CollectionAgainstInvoiceLine
		{
			get { return null; }
		}
	}
}
