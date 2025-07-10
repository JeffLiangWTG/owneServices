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
	public class WarehouseReceiveDocketDataTest : WhsTestCaseWithFactory
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(WhsReceive), AssemblyData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			AssertNotNull(AssemblyData.GetBusinessObjectCollection(Factory));
			AssertEquals(typeof(WhsReceiveCollection), AssemblyData.GetBusinessObjectCollection(Factory).GetType());
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.WhsReceive, AssemblyData.ModuleID);
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
			var oldDate = ZDateTime.Today.AddMonths(-1);
			var oldDateOffset = oldDate.ToOffset();
			var emptyDate = ZDateTime.Empty;
			var emptyDateOffset = ZDateTimeOffset.Empty;
			var receive1 = CreateWhsReceiveWithInventoryAndFinaliseAssertion(data.Org1, data.Whs1, "R1",
				data.Part1, oldDateOffset.AddDays(1), emptyDateOffset, emptyDateOffset); // ETD DATES
			var receive2 = CreateWhsReceiveWithInventoryAndFinaliseAssertion(data.Org1, data.Whs1, "R2",
				data.Part1, oldDateOffset.AddDays(2), emptyDateOffset, emptyDateOffset);
			var receive3 = CreateWhsReceiveWithInventoryAndFinaliseAssertion(data.Org1, data.Whs1, "R3",
				data.Part1, oldDateOffset.AddDays(3), emptyDateOffset, emptyDateOffset);
			var receive4 = CreateWhsReceiveWithInventoryAndFinaliseAssertion(data.Org1, data.Whs1, "R4",
				data.Part1, oldDateOffset.AddDays(4), emptyDateOffset, oldDateOffset.AddDays(4)); //Finalised
			var receive5 = CreateWhsReceiveWithInventoryAndFinaliseAssertion(data.Org1, data.Whs1, "R5",
				data.Part1, emptyDateOffset, oldDateOffset.AddDays(1), emptyDateOffset); // ETA DATES
			var receive6 = CreateWhsReceiveWithInventoryAndFinaliseAssertion(data.Org1, data.Whs1, "R6",
				data.Part1, emptyDateOffset, oldDateOffset.AddDays(2), emptyDateOffset);
			var receive7 = CreateWhsReceiveWithInventoryAndFinaliseAssertion(data.Org1, data.Whs1, "R7",
				data.Part1, emptyDateOffset, oldDateOffset.AddDays(3), emptyDateOffset);
			var receive8 = CreateWhsReceiveWithInventoryAndFinaliseAssertion(data.Org1, data.Whs1, "R8",
				data.Part1, emptyDateOffset, oldDateOffset.AddDays(4), oldDateOffset.AddDays(4)); // Finalised
			var receive9 = CreateWhsReceiveWithInventoryAndFinaliseAssertion(data.Org1, data.Whs1, "R9",
				data.Part1, emptyDateOffset, emptyDateOffset, oldDateOffset.AddDays(1)); // FINALISE DATES
			var receive10 = CreateWhsReceiveWithInventoryAndFinaliseAssertion(data.Org1, data.Whs1, "R10",
				data.Part1, emptyDateOffset, emptyDateOffset, oldDateOffset.AddDays(2));
			var receive11 = CreateWhsReceiveWithInventoryAndFinaliseAssertion(data.Org1, data.Whs1, "R11",
				data.Part1, emptyDateOffset, emptyDateOffset, oldDateOffset.AddDays(3));
			var receive12 = CreateWhsReceiveWithInventoryAndFinaliseAssertion(otherClient, data.Whs1, "R12",
				data.Part1, emptyDateOffset, emptyDateOffset, emptyDateOffset); // Different Client
			Factory.Save();
			// Is Consignee - should have no impact on Receive Jobs
			ZQuery query1 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, emptyDate, data.Org1.PK));
			AssertCorrectReceiveListLoaded(query1,
				new WhsReceive[]
				{
					receive1, receive2, receive3, receive4, receive5, receive6, receive7, receive8, receive9,
					receive10, receive11
				});
			ZQuery query2 = AssemblyData.GetQuery(new AssemblyDataParams(false, true, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, emptyDate, data.Org1.PK));
			AssertCorrectReceiveListLoaded(query2,
				new WhsReceive[]
				{
					receive1, receive2, receive3, receive4, receive5, receive6, receive7, receive8, receive9,
					receive10, receive11
				});
			// Is Client/Consignor - should decide all Receive Jobs for selected Organisation
			ZQuery query3 = AssemblyData.GetQuery(new AssemblyDataParams(true, false, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, emptyDate, data.Org1.PK));
			AssertCorrectReceiveListLoaded(query3, Array.Empty<WhsReceive>());
			ZQuery query4 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, emptyDate, data.Org1.PK));
			AssertCorrectReceiveListLoaded(query4,
				new WhsReceive[]
				{
					receive1, receive2, receive3, receive4, receive5, receive6, receive7, receive8, receive9,
					receive10, receive11
				});
			// ETD Dates
			ZQuery query5 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, oldDate.AddDays(2), emptyDate,
				emptyDate, emptyDate, emptyDate, emptyDate, data.Org1.PK)); // ETD From
			AssertCorrectReceiveListLoaded(query5, new WhsReceive[] { receive2, receive3, receive4 });
			ZQuery query6 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, oldDate.AddDays(3),
				emptyDate, emptyDate, emptyDate, emptyDate, data.Org1.PK)); // ETD To
			AssertCorrectReceiveListLoaded(query6, new WhsReceive[] { receive1, receive2, receive3 });
			ZQuery query7 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, oldDate.AddDays(2),
				oldDate.AddDays(3), emptyDate, emptyDate, emptyDate, emptyDate, data.Org1.PK)); // ETD Range
			AssertCorrectReceiveListLoaded(query7, new WhsReceive[] { receive2, receive3 });
			// ETA Dates
			ZQuery query8 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate,
				oldDate.AddDays(2), emptyDate, emptyDate, emptyDate, data.Org1.PK)); // ETA From
			AssertCorrectReceiveListLoaded(query8, new WhsReceive[] { receive6, receive7, receive8 });
			ZQuery query9 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				oldDate.AddDays(3), emptyDate, emptyDate, data.Org1.PK)); // ETA To
			AssertCorrectReceiveListLoaded(query9, new WhsReceive[] { receive5, receive6, receive7 });
			ZQuery query10 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate,
				oldDate.AddDays(2), oldDate.AddDays(3), emptyDate, emptyDate, data.Org1.PK)); // ETA Range
			AssertCorrectReceiveListLoaded(query10, new WhsReceive[] { receive6, receive7 });
			// Finalised Dates
			ZQuery query11 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, oldDate.AddDays(2), emptyDate, data.Org1.PK)); // Job Closed From
			AssertCorrectReceiveListLoaded(query11, new WhsReceive[] { receive4, receive8, receive10, receive11 });
			ZQuery query12 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, oldDate.AddDays(3), data.Org1.PK)); // Job Closed To
			AssertCorrectReceiveListLoaded(query12, new WhsReceive[] { receive9, receive10, receive11 });
			ZQuery query13 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, oldDate.AddDays(2), oldDate.AddDays(3), data.Org1.PK)); // Job Closed Range
			AssertCorrectReceiveListLoaded(query13, new WhsReceive[] { receive10, receive11 });
			// Different Client
			ZQuery query14 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, emptyDate, otherClient.PK));
			AssertCorrectReceiveListLoaded(query14, new WhsReceive[] { receive12 });
		}

		WhsReceive CreateWhsReceiveWithInventoryAndFinaliseAssertion(OrgHeader client, WhsWarehouse whs,
			ZString externalReference, OrgSupplierPart part, ZDateTimeOffset etdDate, ZDateTimeOffset etaDate,
			ZDateTimeOffset finaliseDate)
		{
			var receive = Helper.CreateWhsReceive(client, whs, externalReference, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part, 10m);
			receive.AllocateLocationsWithMock();
			if (!etdDate.IsEmpty)
			{
				receive.WD_ETD = etdDate;
			}

			if (!etaDate.IsEmpty)
			{
				receive.WD_ETA = etaDate;
			}

			if (!finaliseDate.IsEmpty)
			{
				receive.FinaliseDocket();
				AssertEquals(true, receive.IsFinalised);
				receive.WD_FinalisedDate = finaliseDate;
			}

			return receive;
		}

		void AssertCorrectReceiveListLoaded(ZQuery query, WhsReceive[] expectedReceiveList)
		{
			WhsReceiveCollection receiveList = new WhsReceiveCollection(Factory);
			receiveList.AdditionalFilter = query;
			AssertEquals("Incorrect List of Receives was loaded", expectedReceiveList.Length, receiveList.Count);
			foreach (var receive in expectedReceiveList)
			{
				AssertEquals("Expected Receive has been not loaded: " + receive.WD_ExternalReference, true,
					receiveList.Contains(receive));
			}
		}

		#endregion

		#region Implementation

		WarehouseReceiveAssemblyData AssemblyData
		{
			get
			{
				return assemblyData ?? (assemblyData = new WarehouseReceiveAssemblyData());
			}
		}

		WarehouseReceiveAssemblyData assemblyData;

		#endregion
	}
}
