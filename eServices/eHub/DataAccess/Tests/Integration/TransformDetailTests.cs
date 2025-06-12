using System;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.Integration
{
	public class TransformDetailTests
	{
		[Test]
		public void TransformDetail_ConvertsFromSharedTypes()
		{
			Assert.Multiple(() =>
			{
				Assert.DoesNotThrow(() =>
					_ = (CargoWise.eHub.DataAccess.Integration.TransformDetail)
						new eServices.eHubDataAccess.Integration.TransformDetail(
							Guid.NewGuid(),
							"transformType",
							"targetMessageType"
						));

				Assert.DoesNotThrow(() =>
					_ = (CargoWise.eHub.DataAccess.Integration.TransformDetail)
						new eServices.eHubDataAccess.Integration.TransformDetail(
							Guid.Empty,
							null,
							null
						));
			});
		}
	}
}
