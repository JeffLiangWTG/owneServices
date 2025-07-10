using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WarehouseAdjustmentDocketDataTest : WhsTestCaseWithFactory
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(WhsAdjustment), AssemblyData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			AssertNotNull(AssemblyData.GetBusinessObjectCollection(Factory));
			AssertEquals(typeof(WhsAdjustmentCollection), AssemblyData.GetBusinessObjectCollection(Factory).GetType());
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.WhsAdjustment, AssemblyData.ModuleID);
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, AssemblyData.ReferenceType);
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals(true, AssemblyData.IsAllowedForUnallocatedeDocs);
		}

		#endregion

		#region TestGetQuery

		public void TestGetQuery()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var otherClient = Helper.CreateClient("OTHER");
			var oldDate = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.Today).AddMonths(-1);
			var emptyDateOffset = ZDateTimeOffset.Empty;
			var adjustment1 =
				CreateWhsAdjustmentWithLineAndFinaliseAssertion(data.Org1, data.Whs1, "AD1", data.Part1, emptyDateOffset);
			var adjustment2 =
				CreateWhsAdjustmentWithLineAndFinaliseAssertion(data.Org1, data.Whs1, "AD2", data.Part1,
					oldDate.AddDays(1));
			var adjustment3 =
				CreateWhsAdjustmentWithLineAndFinaliseAssertion(data.Org1, data.Whs1, "AD3", data.Part1,
					oldDate.AddDays(2));
			var adjustment4 =
				CreateWhsAdjustmentWithLineAndFinaliseAssertion(data.Org1, data.Whs1, "AD4", data.Part1,
					oldDate.AddDays(3));
			var adjustment5 =
				CreateWhsAdjustmentWithLineAndFinaliseAssertion(data.Org1, data.Whs1, "AD5", data.Part1,
					oldDate.AddDays(4));
			var adjustment6 =
				CreateWhsAdjustmentWithLineAndFinaliseAssertion(otherClient, data.Whs1, "AD6", data.Part1,
					emptyDateOffset); // Different Client
			Factory.Save();

			var emptyDate = ZDateTime.Empty;
			// Is Consignee - should have no impact on Adjustment Jobs
			var query1 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, emptyDate, data.Org1.PK));
			AssertCorrectAdjustmentListLoaded(query1,
				new WhsAdjustment[] { adjustment1, adjustment2, adjustment3, adjustment4, adjustment5 });
			var query2 = AssemblyData.GetQuery(new AssemblyDataParams(false, true, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, emptyDate, data.Org1.PK));
			AssertCorrectAdjustmentListLoaded(query2,
				new WhsAdjustment[] { adjustment1, adjustment2, adjustment3, adjustment4, adjustment5 });
			// Is Client/Consignor - should decide all Adjustment Jobs for selected Organisation
			var query3 = AssemblyData.GetQuery(new AssemblyDataParams(true, false, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, emptyDate, data.Org1.PK));
			AssertCorrectAdjustmentListLoaded(query3, Array.Empty<WhsAdjustment>());
			var query4 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, emptyDate, data.Org1.PK));
			AssertCorrectAdjustmentListLoaded(query4,
				new WhsAdjustment[] { adjustment1, adjustment2, adjustment3, adjustment4, adjustment5 });

			// Finalised Dates
			var query5 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, oldDate.ToLocalZDateTime().AddDays(2), emptyDate, data.Org1.PK)); // Job Closed From
			AssertCorrectAdjustmentListLoaded(query5, new WhsAdjustment[] { adjustment3, adjustment4, adjustment5 });
			var query6 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, oldDate.ToLocalZDateTime().AddDays(3), data.Org1.PK)); // Job Closed To
			AssertCorrectAdjustmentListLoaded(query6, new WhsAdjustment[] { adjustment2, adjustment3, adjustment4 });
			var query7 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, oldDate.ToLocalZDateTime().AddDays(2), oldDate.ToLocalZDateTime().AddDays(3), data.Org1.PK)); // Job Closed Range
			AssertCorrectAdjustmentListLoaded(query7, new WhsAdjustment[] { adjustment3, adjustment4 });
			// Different Client
			var query8 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, emptyDate, otherClient.PK));
			AssertCorrectAdjustmentListLoaded(query8, new WhsAdjustment[] { adjustment6 });
		}

		WhsAdjustment CreateWhsAdjustmentWithLineAndFinaliseAssertion(OrgHeader client, WhsWarehouse whs,
			ZString externalReference, OrgSupplierPart part, ZDateTimeOffset finaliseDate)
		{
			WhsAdjustment adjustment = Helper.CreateWhsAdjustment(client, whs, externalReference, Notify);
			WhsAdjustmentLine adjustmentLine =
				Helper.CreateWhsAdjustmentLine(adjustment, part, 10m, whs.DefaultLocation);
			if (!finaliseDate.IsEmpty)
			{
				adjustment.FinaliseDocket();
				AssertEquals(true, adjustment.IsFinalised);
				adjustment.WD_FinalisedDate = finaliseDate;
			}

			return adjustment;
		}

		void AssertCorrectAdjustmentListLoaded(ZQuery query, WhsAdjustment[] expectedAdjustmentList)
		{
			var adjustmentList = new WhsAdjustmentCollection(Factory);
			adjustmentList.AdditionalFilter = query;
			AssertEquals("Incorrect List of Adjustments was loaded", expectedAdjustmentList.Length,
				adjustmentList.Count);
			foreach (WhsAdjustment adjustment in expectedAdjustmentList)
			{
				AssertEquals("Expected Adjustment has been not loaded: " + adjustment.WD_ExternalReference, true,
					adjustmentList.Contains(adjustment));
			}
		}

		#endregion

		#region Implementation

		WarehouseAdjustmentAssemblyData AssemblyData
		{
			get
			{
				return assemblyData ?? (assemblyData = new WarehouseAdjustmentAssemblyData());
			}
		}

		WarehouseAdjustmentAssemblyData assemblyData;

		#endregion
	}
}
