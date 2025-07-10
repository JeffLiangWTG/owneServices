using NUnit.Framework;
namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusClassificationValidationTest : Customs.Business.Testing.CusClassificationValidationTest
	{
		[ExpectNoExceptions]
		public void TestParent()
		{
			CusClassification parent = Factory.New<CusClassification>();
			NUnit.Framework.Assert.That(parent, NUnit.Framework.Is.EqualTo(parent.Validation.Parent));
		}
	}
}
