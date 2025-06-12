using System;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.SQL
{
	internal class SecurityAccessorTests
	{
		[Test]
		public void UpdatePassword_CallsSharedDataAccess()
		{
			var sharedSecurityAccessor = new Mock<eServices.eHubDataAccess.Integration.ISecurityAccessor>();
			var eHubSecurityAccessor = new CargoWise.eHub.DataAccess.Sql.SecurityAccessor(sharedSecurityAccessor.Object);
			eHubSecurityAccessor
				.UpdatePassword("clientID", "password");
			sharedSecurityAccessor.Verify(x => x
				.UpdatePassword("clientID", "password"), Times.Once);
		}

		[Test]
		public void ValidatePassword_CallsSharedDataAccess()
		{
			var sharedSecurityAccessor = new Mock<eServices.eHubDataAccess.Integration.ISecurityAccessor>();
			var eHubSecurityAccessor = new CargoWise.eHub.DataAccess.Sql.SecurityAccessor(sharedSecurityAccessor.Object);
			eHubSecurityAccessor
				.ValidatePassword("clientID", "password");
			sharedSecurityAccessor.Verify(x => x
				.ValidatePassword("clientID", "password"), Times.Once);
		}

		[Test]
		public void CheckAccess_CallsSharedDataAccess()
		{
			var sharedSecurityAccessor = new Mock<eServices.eHubDataAccess.Integration.ISecurityAccessor>();
			var eHubSecurityAccessor = new CargoWise.eHub.DataAccess.Sql.SecurityAccessor(sharedSecurityAccessor.Object);
			eHubSecurityAccessor
				.CheckAccess("clientID", "operation");
			sharedSecurityAccessor.Verify(x => x
				.CheckAccess("clientID", "operation"), Times.Once);
		}

		[Test]
		public void InsertToRegistrationLog_CallsSharedDataAccess()
		{
			var sharedSecurityAccessor = new Mock<eServices.eHubDataAccess.Integration.ISecurityAccessor>();
			var eHubSecurityAccessor = new CargoWise.eHub.DataAccess.Sql.SecurityAccessor(sharedSecurityAccessor.Object);
			eHubSecurityAccessor
				.InsertToRegistrationLog("clientID", "ipAddress", DateTime.MinValue);
			sharedSecurityAccessor.Verify(x => x
				.InsertToRegistrationLog("clientID", "ipAddress", DateTime.MinValue), Times.Once);
		}

		[Test]
		public void CheckClientAuthorisation_CallsSharedDataAccess()
		{
			var sharedSecurityAccessor = new Mock<eServices.eHubDataAccess.Integration.ISecurityAccessor>();
			var eHubSecurityAccessor = new CargoWise.eHub.DataAccess.Sql.SecurityAccessor(sharedSecurityAccessor.Object);
			eHubSecurityAccessor
				.CheckClientAuthorisation("senderID", "recipientID");
			sharedSecurityAccessor.Verify(x => x
				.CheckClientAuthorisation("senderID", "recipientID"), Times.Once);
		}
	}
}
