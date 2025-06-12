using System;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.SQL
{
	internal class ExceptionsAccessorTests
	{
		[Test]
		public void SubmitError_CallsSharedDataAccess()
		{
			var sharedExceptionsAccessor = new Mock<eServices.eHubDataAccess.Integration.IExceptionsAccessor>();
			var eHubExceptionsAccessor = new CargoWise.eHub.DataAccess.Sql.ExceptionsAccessor(sharedExceptionsAccessor.Object);
			eHubExceptionsAccessor
				.SubmitError(Guid.Empty, "source", "errorType", "description");
			sharedExceptionsAccessor.Verify(x => x
				.SubmitError(Guid.Empty, "source", "errorType", "description"), Times.Once);
		}

		[Test]
		public void SubmitError_EIPK_OIPK_CallsSharedDataAccess()
		{
			var sharedExceptionsAccessor = new Mock<eServices.eHubDataAccess.Integration.IExceptionsAccessor>();
			var eHubExceptionsAccessor = new CargoWise.eHub.DataAccess.Sql.ExceptionsAccessor(sharedExceptionsAccessor.Object);
			eHubExceptionsAccessor
				.SubmitError(Guid.Empty, "source", "errorType", "description", Guid.Empty, Guid.Empty);
			sharedExceptionsAccessor.Verify(x => x
				.SubmitError(Guid.Empty, "source", "errorType", "description", Guid.Empty, Guid.Empty), Times.Once);
		}

		[Test]
		public void SubmitErrorAndUpdateStatus_CallsSharedDataAccess()
		{
			var sharedExceptionsAccessor = new Mock<eServices.eHubDataAccess.Integration.IExceptionsAccessor>();
			var eHubExceptionsAccessor = new CargoWise.eHub.DataAccess.Sql.ExceptionsAccessor(sharedExceptionsAccessor.Object);
			eHubExceptionsAccessor
				.SubmitErrorAndUpdateStatus(Guid.Empty, "source", "errorType", "description", Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, (SqlConnection)null);
			sharedExceptionsAccessor.Verify(x => x
				.SubmitErrorAndUpdateStatus(Guid.Empty, "source", "errorType", "description", Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, (SqlConnection)null), Times.Once);
		}

		[Test]
		public void SubmitErrorAndUpdateStatus_Alerted_CallsSharedDataAccess()
		{
			var sharedExceptionsAccessor = new Mock<eServices.eHubDataAccess.Integration.IExceptionsAccessor>();
			var eHubExceptionsAccessor = new CargoWise.eHub.DataAccess.Sql.ExceptionsAccessor(sharedExceptionsAccessor.Object);
			eHubExceptionsAccessor
				.SubmitErrorAndUpdateStatus(Guid.Empty, "source", "errorType", "description", Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, (SqlConnection)null, true);
			sharedExceptionsAccessor.Verify(x => x
				.SubmitErrorAndUpdateStatus(Guid.Empty, "source", "errorType", "description", Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty, (SqlConnection)null, true), Times.Once);
		}
	}
}
