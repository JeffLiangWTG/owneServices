using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AsycudaBillValidation))]
	sealed class AsycudaBillValidationTest : TestCaseWithFactory
	{
		public void TestParent()
		{
			var bill = Factory.New<AsycudaBill>();
			var validationMock = new Mock<AsycudaBillValidation>(bill);
			AssertEquals(bill, validationMock.Object.Parent);
		}
	}
}
