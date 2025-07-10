using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	[CreateDatabase("B10A6282DFB04115851BA592DB1A2E53", DbSchema.RefDbRepoSafe, ActionTargets.Test)]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class RefCusProcedureUserViewFixture
	{
		[Test]
		public void View()
		{
			var dbName = CreateDatabaseAttribute.DbNamePrefix + "B10A6282DFB04115851BA592DB1A2E53";
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				context.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_PK = Guid.NewGuid(),
					ZZZ_DataGrouping = "GB",
					ZZZ_Description = "Great Britain"
				});
				context.SaveChanges();

				// Insert
				var procedureView = new RefCusProcedureUserView
				{
					ZZ6_PK = Guid.NewGuid(),
					ZZ6_Category = "01",
					ZZ6_ProcedureCode = "53",
					ZZ6_PreviousProcedureCode = "00",
					ZZ6_Concession = "008",
					ZZ6_Description = "Procedure Description",
					ZZ6_CountryOrGrouping = "GB",
					ZZ6_ShipmentType = "IMP",
					ZZ6_CalculateDuty = false,
					ZZ6_Group = "IFD,ISD",
					ZZ6_LandedCost = false,
					ZZ6_IntoWarehouse = "N",
					ZZ6_OutOfWarehouse = "N",
					ZZ6_IntoInwardProcessing = "N",
					ZZ6_OutOfInwardProcessing = "N",
					ZZ6_IntoOutwardProcessing = "N",
					ZZ6_OutofOutwardProcessing = "N",
					ZZ6_IntoTemporaryImport = "N",
					ZZ6_OutOfTemporaryImport = "N",
					ZZ6_IntoTemporaryExport = "N",
					ZZ6_OutOfTemporaryExport = "N",
					ZZ6_StartDate = new DateTime(1900, 01, 01),
					ZZ6_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
					ZZ6_CalculateVAT = true,
					ZZ6_IsGuaranteeConsumed = "N",
					ZZ6_IsGuaranteeReleased = "N",
					ZZ6_IsTransit = "N",
					ZZ6_IsPublished = true
				};
				var procedureAttributeView = new RefCusProcedureAttributeUserView
				{
					ZXB_PK = Guid.NewGuid(),
					ZXB_ZZ6_ProcedureCode = procedureView.ZZ6_PK,
					ZXB_Name = "SSGPaymentMethod",
					ZXB_Value = "CASH",
					ZXB_CountryOrGrouping = "GB"
				};

				context.RefCusProcedureUserViews.Add(procedureView);
				context.SaveChanges();

				context.RefCusProcedureAttributeUserViews.Add(procedureAttributeView);
				context.SaveChanges();

				Assert.That(context.DataSetChangeHistories.Where(x => x.DCH_ParentPK == procedureView.ZZ6_PK).ToArray(), Has.Length.EqualTo(2));

				var procedure = context.RefCusProcedures.FirstOrDefault();
				Assert.AreEqual("01", procedure.ZZ6_Category);
				Assert.AreEqual("53", procedure.ZZ6_ProcedureCode);
				Assert.AreEqual("00", procedure.ZZ6_PreviousProcedureCode);
				Assert.AreEqual("008", procedure.ZZ6_Concession);
				Assert.AreEqual("Procedure Description", procedure.ZZ6_Description);
				Assert.AreEqual("GB", procedure.ZZ6_ZZZ_NKDataGrouping);
				Assert.AreEqual("IMP", procedure.ZZ6_ShipmentType);
				Assert.AreEqual(false, procedure.ZZ6_CalculateDuty);
				Assert.AreEqual("IFD,ISD", procedure.ZZ6_Group);
				Assert.AreEqual(false, procedure.ZZ6_LandedCost);
				Assert.AreEqual(true, procedure.ZZ6_CalculateVAT);
				Assert.AreEqual("N", procedure.ZZ6_IntoWarehouse);
				Assert.AreEqual("N", procedure.ZZ6_OutOfWarehouse);

				var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == procedure.ZZ6_PK);
				Assert.AreEqual(false, versionControl.RVC_Deleted);

				var attr = context.RefCusProcedureAttributes.FirstOrDefault();
				Assert.AreEqual(procedure.ZZ6_PK, attr.ZXB_ZZ6_ProcedureCode);
				Assert.AreEqual("SSGPaymentMethod", attr.ZXB_Name);
				Assert.AreEqual("CASH", attr.ZXB_Value);

				// Update
				procedureView.ZZ6_ProcedureCode = "60";
				procedureView.ZZ6_PreviousProcedureCode = "10";
				procedureView.ZZ6_ShipmentType = "EXP";
				procedureView.ZZ6_LandedCost = true;
				procedureView.ZZ6_IntoWarehouse = "Y";
				procedureView.ZZ6_IsPublished = false;
				procedureAttributeView.ZXB_Name = "COMPaymentMethod";
				context.SaveChanges();
			}

			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var procedure = context.RefCusProcedures.FirstOrDefault();
				Assert.AreEqual("60", procedure.ZZ6_ProcedureCode);
				Assert.AreEqual("10", procedure.ZZ6_PreviousProcedureCode);
				Assert.AreEqual("EXP", procedure.ZZ6_ShipmentType);
				Assert.AreEqual(true, procedure.ZZ6_LandedCost);
				Assert.AreEqual("Y", procedure.ZZ6_IntoWarehouse);

				var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == procedure.ZZ6_PK);
				Assert.AreEqual(true, versionControl.RVC_Deleted);

				var attr = context.RefCusProcedureAttributes.FirstOrDefault();
				Assert.AreEqual("COMPaymentMethod", attr.ZXB_Name);
			}

			// Delete
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(dbName)))
			{
				var attrView = context.RefCusProcedureAttributeUserViews.FirstOrDefault();
				context.RefCusProcedureAttributeUserViews.Remove(attrView);
				context.SaveChanges();
				Assert.Null(context.RefCusProcedureAttributes.FirstOrDefault());
			}
		}
	}
}
