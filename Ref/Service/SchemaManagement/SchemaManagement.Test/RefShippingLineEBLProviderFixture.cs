using System;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test;

[TestFixture]
[TransactionedTestCase]
public class RRefShippingLineEBLProviderFixture
{
	string _dbName;

	[Test]
	public void UniqueConstraints()
	{
		var shippingLinePk = Guid.NewGuid();
		using (var context = new SafeDbContext(TestConnectionString.GetAdmin(_dbName)))
		{
			var shippingLineView = new RefShippingLine
			{
				RSL_PK = shippingLinePk,
				RSL_CarrierName = "Carrier Name",
				RSL_CargoWiseOneCode = "C1AA",
				RSL_StandardCarrierAlphaCode = "C1BB",
				RSL_IsActive = true,
				RSL_IsNVO = false,
				RSL_OceanCarrierMessagingAvailable = false,
				RSL_GlobalSailingScheduleAvailable = false,
				RSL_ContainerAutomationAvailable = false,
				RSL_CargoSphereRatesAvailable = false,
				RSL_InvoiceAvailable = false,
				RSL_IsCW1User = false,
				RSL_EHubIds = string.Empty,
			};
			context.RefShippingLines.Add(shippingLineView);
			context.RefShippingLineEBLProviders.Add(new RefShippingLineEBLProvider
			{
				RSE_PK = Guid.NewGuid(),
				RSE_RSL_ShippingLine = shippingLinePk,
				RSE_Name = "AAA",
				RSE_IsAvailable = true,
				RSE_IsDefault = false,
			});
			var eblProvider = new RefShippingLineEBLProvider
			{
				RSE_PK = Guid.NewGuid(),
				RSE_RSL_ShippingLine = shippingLinePk,
				RSE_Name = "BBB",
				RSE_IsAvailable = true,
				RSE_IsDefault = false
			};
			context.RefShippingLineEBLProviders.Add(eblProvider);
			context.SaveChanges();
		}
		using (var context = new SafeDbContext(TestConnectionString.GetAdmin(_dbName)))
		{
			var eblProvider = new RefShippingLineEBLProvider
			{
				RSE_PK = Guid.NewGuid(),
				RSE_RSL_ShippingLine = shippingLinePk,
				RSE_Name = "AAA",
				RSE_IsAvailable = true,
				RSE_IsDefault = false
			};
			context.RefShippingLineEBLProviders.Add(eblProvider);
			Assert.That(() => context.SaveChanges(), Throws.Exception);
		}
	}

	[SetUp]
	public void SetUp()
	{
		_dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
	}
}
