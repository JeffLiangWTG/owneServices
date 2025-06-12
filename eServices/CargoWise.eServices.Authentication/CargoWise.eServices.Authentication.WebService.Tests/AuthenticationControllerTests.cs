using System.Net;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Authentication.WebService.Tests
{
	[TestFixture]
	public class AuthenticationControllerTests
	{
		[Test]
		public void TestValidateSystemIDAndPassword()
		{
			var databaseMock = new Mock<IDatabaseHelper>();
			databaseMock.Setup(_ => _.ValidateSystemIDAndPassword("SYSID", "correct")).Returns(true);
			databaseMock.Setup(_ => _.ValidateSystemIDAndPassword("SYSID", "wrong")).Returns(false);
			var controller = new Mock<AuthenticationController>();
			controller.Setup(_ => _.DatabaseHelper).Returns(databaseMock.Object);

			Assert.AreEqual(HttpStatusCode.OK, controller.Object.ValidateSystemIDAndPassword(new string[2] { "SYSID", "correct" }).StatusCode);
			Assert.AreEqual(HttpStatusCode.Unauthorized, controller.Object.ValidateSystemIDAndPassword(new string[2] { "SYSID", "wrong" }).StatusCode);
		}

		[Test]
		public void TestValidateCodeAndPassword()
		{
			var databaseMock = new Mock<IDatabaseHelper>();
			databaseMock.Setup(_ => _.ValidateCodeAndPassword("ABC", "DEF", "correct")).Returns(true);
			databaseMock.Setup(_ => _.ValidateCodeAndPassword("ABC", "DEF", "wrong")).Returns(false);
			var controller = new Mock<AuthenticationController>();
			controller.Setup(_ => _.DatabaseHelper).Returns(databaseMock.Object);

			Assert.AreEqual(HttpStatusCode.OK, controller.Object.ValidateCodeAndPassword(new string[3] { "ABC", "DEF", "correct" }).StatusCode);
			Assert.AreEqual(HttpStatusCode.Unauthorized, controller.Object.ValidateCodeAndPassword(new string[3] { "ABC", "DEF", "wrong" }).StatusCode);
		}

		[Test]
		public void TestCheckSystemExistence()
		{
			var databaseMock = new Mock<IDatabaseHelper>();
			databaseMock.Setup(_ => _.CheckSystemIDExistence("ABC")).Returns(true);
			databaseMock.Setup(_ => _.CheckSystemIDExistence("DEF")).Returns(false);
			var controller = new Mock<AuthenticationController>();
			controller.Setup(_ => _.DatabaseHelper).Returns(databaseMock.Object);

			Assert.AreEqual(HttpStatusCode.Found, controller.Object.CheckSystemIDExistence("ABC").StatusCode);
			Assert.AreEqual(HttpStatusCode.Unauthorized, controller.Object.CheckSystemIDExistence("DEF").StatusCode);
		}

		[Test]
		public void TestCheckCodeExistence()
		{
			var databaseMock = new Mock<IDatabaseHelper>();
			databaseMock.Setup(_ => _.CheckCodeExistence("ABC", "123")).Returns(true);
			databaseMock.Setup(_ => _.CheckCodeExistence("DEF", "234")).Returns(false);
			var controller = new Mock<AuthenticationController>();
			controller.Setup(_ => _.DatabaseHelper).Returns(databaseMock.Object);

			Assert.AreEqual(HttpStatusCode.Found, controller.Object.CheckCodeExistence(new string[2] { "ABC", "123" }).StatusCode);
			Assert.AreEqual(HttpStatusCode.Unauthorized, controller.Object.CheckCodeExistence(new string[2] { "DEF", "234" }).StatusCode);
		}
	}
}
