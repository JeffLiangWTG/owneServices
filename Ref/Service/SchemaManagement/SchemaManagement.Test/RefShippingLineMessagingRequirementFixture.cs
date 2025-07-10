using System;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	[TransactionedTestCase]
	public class RefShippingLineMessagingRequirementFixture
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
				context.RefShippingLineMessagingRequirementTypes.Add(new RefShippingLineMessagingRequirementType
				{
					RST_PK = Guid.NewGuid(),
					RST_Code = "TP1",
					RST_Description = "Description"
				});
				var messagingRequirement = new RefShippingLineMessagingRequirement
				{
					RSR_PK = Guid.NewGuid(),
					RSR_RSL_ShippingLine = shippingLinePk,
					RSR_IsBookingRequest = true,
					RSR_RST_NKType = "TP1",
					RSR_IsShippingInstruction = true
				};
				context.RefShippingLineMessagingRequirements.Add(messagingRequirement);
				context.SaveChanges();
			}
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(_dbName)))
			{
				var messagingRequirement = new RefShippingLineMessagingRequirement
				{
					RSR_PK = Guid.NewGuid(),
					RSR_RSL_ShippingLine = shippingLinePk,
					RSR_IsBookingRequest = false,
					RSR_RST_NKType = "TP1",
					RSR_IsShippingInstruction = false
				};
				context.RefShippingLineMessagingRequirements.Add(messagingRequirement);
				Assert.That(() => context.SaveChanges(), Throws.Exception);
			}
		}

		[SetUp]
		public void SetUp()
		{
			_dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		}
	}
}
