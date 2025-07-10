using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test;

[TestFixture]
[TransactionedTestCase]
class RefShippingLineUserViewFixture
{
	[Test]
	public void View()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
		{
			var shippingLineView = new RefShippingLineUserView
			{
				RSL_PK = Guid.NewGuid(),
				RSL_CarrierName = "Carrier Name",
				RSL_CargoWiseOneCode = "C1AA",
				RSL_StandardCarrierAlphaCode = "C1BB",
				RSL_IsPublished = true,
				RSL_IsSystem = true,
				RSL_IsActive = true,
				RSL_IsNVO = false,
				RSL_OceanCarrierMessagingAvailable = false,
				RSL_GlobalSailingScheduleAvailable = false,
				RSL_ContainerAutomationAvailable = false,
				RSL_CargoSphereRatesAvailable = false,
				RSL_InvoiceAvailable = false,
				RSL_IsCW1User = false,
				RSL_EHubIds = string.Empty,
				RSL_BookingRequestAvailable = false,
				RSL_ShippingInstructionAvailable = false,
				RSL_VerifiedGrossContainerWeightAvailable = false,
				RSL_ShippingOrderAvailable = false,
				RSL_EManifestAvailable = false,
				RSL_ShippingLineLogo = new byte[1020]
			};
			context.RefShippingLineUserViews.Add(shippingLineView);
			context.SaveChanges();
			Assert.That(context.DataSetChangeHistories.Where(x => x.DCH_ParentPK == shippingLineView.RSL_PK).ToArray(), Has.Length.EqualTo(1));
		}
		using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
		{
			var shippingLineView = context.RefShippingLineUserViews.FirstOrDefault();
			Assert.AreEqual("Carrier Name", shippingLineView.RSL_CarrierName);
			Assert.AreEqual("C1AA", shippingLineView.RSL_CargoWiseOneCode);
			Assert.AreEqual("C1BB", shippingLineView.RSL_StandardCarrierAlphaCode);
			Assert.AreEqual(true, shippingLineView.RSL_IsActive);
			Assert.AreEqual(false, shippingLineView.RSL_IsNVO);

			var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == shippingLineView.RSL_PK);
			Assert.AreEqual(false, versionControl.RVC_Deleted);

			shippingLineView.RSL_CarrierName = "Modified Name";
			shippingLineView.RSL_CargoWiseOneCode = "C1MM";
			shippingLineView.RSL_IsNVO = true;
			shippingLineView.RSL_IsPublished = false;
			context.SaveChanges();
		}

		using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
		{
			var shippingLineView = context.RefShippingLineUserViews.FirstOrDefault();
			Assert.AreEqual("Modified Name", shippingLineView.RSL_CarrierName);
			Assert.AreEqual("C1MM", shippingLineView.RSL_CargoWiseOneCode);
			Assert.AreEqual("C1BB", shippingLineView.RSL_StandardCarrierAlphaCode);
			Assert.AreEqual(true, shippingLineView.RSL_IsActive);
			Assert.AreEqual(true, shippingLineView.RSL_IsNVO);

			var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == shippingLineView.RSL_PK);
			Assert.AreEqual(true, versionControl.RVC_Deleted);
		}

		using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
		{
			var shippingLineView = context.RefShippingLineUserViews.FirstOrDefault();
			context.RefShippingLineUserViews.Remove(shippingLineView);
			Assert.Throws<DbUpdateException>(() => context.SaveChanges(), "Should not enable Delete in top-level tables");
		}
	}
}
