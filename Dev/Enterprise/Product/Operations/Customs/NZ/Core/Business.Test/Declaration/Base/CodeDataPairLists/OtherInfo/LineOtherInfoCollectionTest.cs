using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Testing
{
	[TestedType(typeof(LineOtherInfoCollection))]
	public class LineOtherInfoCollectionTest : OtherInfoCollectionTest<LineOtherInfoCollection>
	{
		protected override LineOtherInfoCollection GetCollectionToTest()
		{
			return new LineOtherInfoCollection(Factory, Classification.CC_OtherInfosInfo);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new LineOtherInfo(Factory, null);
		}

		protected override CodeDataPairCollection CollectionAgainstDeclaration
		{
			get { return null; }
		}

		protected override CodeDataPairCollection CollectionAgainstInvoiceLine
		{
			get { return InvoiceLine.OtherInfos; }
		}
	}
}
