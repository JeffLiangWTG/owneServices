using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.Integration
{
	public class TransformSetTests
	{
		[Test]
		public void TransformSet_ConvertsFromSharedTypes()
		{
			Assert.Multiple(() =>
			{
				Assert.DoesNotThrow(() =>
					_ = (CargoWise.eHub.DataAccess.Integration.TransformSet)
						new eServices.eHubDataAccess.Integration.TransformSet(
							new[]
							{
								new eServices.eHubDataAccess.Integration.TransformDetail(
									Guid.NewGuid(),
									"transformType",
									"targetMessageType"
								)
							}.ToList(),
							"XpathPredicate"
						));

				Assert.DoesNotThrow(() =>
					_ = (CargoWise.eHub.DataAccess.Integration.TransformSet)
						new eServices.eHubDataAccess.Integration.TransformSet(
							null,
							null
						));
			});
		}
	}
}
