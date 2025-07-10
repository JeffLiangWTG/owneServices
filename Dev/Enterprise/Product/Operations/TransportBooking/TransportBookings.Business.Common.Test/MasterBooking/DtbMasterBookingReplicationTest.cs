using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Shared.Testing
{
	[TestedType(typeof(DtbMasterBookingReplication))]
	class DtbMasterBookingReplicationTest : TestCaseWithFactory
	{
		public void TestDtbBookingConsolidationReplicatedColumns()
		{
			AssertContainsExactElementsInAnyOrder(
				"DtbBookingConsolidation should have the expected columns as replicated columns",
				new SchemaColumn[]
				{
					DtbBookingConsolidationSchema.KB_JobDirection,
				},
				DtbMasterBookingReplication.DtbBookingConsolidationReplicatedColumns);
		}

		public void TestDtbBookingReplicatedColumns()
		{
			AssertContainsExactElementsInAnyOrder(
				"DtbBooking should have the expected columns as replicated columns",
				new SchemaColumn[]
				{
					DtbBookingSchema.KM_KT_NKBookingTemplate,
					DtbBookingSchema.KM_IsActive,
					DtbBookingSchema.KM_Status,
					DtbBookingSchema.KM_Description,
					DtbBookingSchema.KM_Direction,
					DtbBookingSchema.KM_RS_NKServiceLevel,
					DtbBookingSchema.KM_PL_NKCarrierServiceLevel,
					DtbBookingSchema.KM_RatingFreightMode,
					DtbBookingSchema.KM_Distance,
					DtbBookingSchema.KM_DistanceUnit,
					DtbBookingSchema.KM_OAN_CarrierAccount,
					DtbBookingSchema.KM_GB_Branch,
					DtbBookingSchema.KM_BookingOfTransportRequestedDate,
					DtbBookingSchema.KM_IsAgentBooking,
					DtbBookingSchema.KM_TransportReference,
					DtbBookingSchema.KM_TransportMode,
				},
				DtbMasterBookingReplication.DtbBookingReplicatedColumns);
		}

		public void TestDtbBookingInstructionReplicatedColumns()
		{
			AssertContainsExactElementsInAnyOrder(
				"DtbBookingInstruction should have the expected columns as replicated columns",
				new SchemaColumn[]
				{
					DtbBookingInstructionSchema.KN_Sequence,
					DtbBookingInstructionSchema.KN_InstructionType,
					DtbBookingInstructionSchema.KN_DropMode,
					DtbBookingInstructionSchema.KN_ServiceInstruction,
					DtbBookingInstructionSchema.KN_Status,
					DtbBookingInstructionSchema.KN_RQ_Equipment,
					DtbBookingInstructionSchema.KN_IsContainerRateable,
					DtbBookingInstructionSchema.KN_IsLooseRateable,
					DtbBookingInstructionSchema.KN_TZ_DomesticZone,
					DtbBookingInstructionSchema.KN_IsAuthorisedToLeave,
				},
				DtbMasterBookingReplication.DtbBookingInstructionReplicatedColumns);
		}

		public void TestDtbBookingConfirmationReplicatedColumns()
		{
			AssertContainsExactElementsInAnyOrder(
				"DtbBookingConfirmation should have the expected columns as replicated columns",
				new SchemaColumn[]
				{
					DtbBookingConfirmationSchema.KK_ConfirmationType,
					DtbBookingConfirmationSchema.KK_Estimated,
					DtbBookingConfirmationSchema.KK_EstimatedUtc,
					DtbBookingConfirmationSchema.KK_Actual,
					DtbBookingConfirmationSchema.KK_RequiredFrom,
					DtbBookingConfirmationSchema.KK_RequiredFromUtc,
					DtbBookingConfirmationSchema.KK_RequiredTo,
					DtbBookingConfirmationSchema.KK_RequiredToUtc,
					DtbBookingConfirmationSchema.KK_ReferenceNum,
					DtbBookingConfirmationSchema.KK_ReceivedBySignature,
					DtbBookingConfirmationSchema.KK_ReceivedBy,
					DtbBookingConfirmationSchema.KK_SlotDateTime,
					DtbBookingConfirmationSchema.KK_SlotReference,
					DtbBookingConfirmationSchema.KK_OC_Driver,
					DtbBookingConfirmationSchema.KK_VehicleRegistration,
					DtbBookingConfirmationSchema.KK_DocumentID,
					DtbBookingConfirmationSchema.KK_DocumentType,
					DtbBookingConfirmationSchema.KK_DocumentIssuer
				},
				DtbMasterBookingReplication.DtbBookingConfirmationReplicatedColumns);
		}

		public void TestGetPropertyInfosFromListOfColumns()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var columns = new SchemaColumn[] { DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description, DummyBizoSchema.Z0_Decimal };
			var expectedPropertyInfos = new ZPropertyInfo[] { dummy.Z0_CodeInfo, dummy.Z0_DescriptionInfo, dummy.Z0_DecimalInfo };
			AssertContainsExactElementsInExactOrder("Should return ZPropertyInfo properties for each column in input", expectedPropertyInfos, DtbMasterBookingReplication.GetPropertyInfosFromListOfColumns(dummy, columns));
		}

		public void TestGetReplicatedColumnsForTable()
				{
			AssertContainsExactElementsInExactOrder("GetReplicatedColumnsForTable() for DtbBookingConsolidation should return DtbBookingConsolidationReplicatedColumns", DtbMasterBookingReplication.DtbBookingConsolidationReplicatedColumns, DtbMasterBookingReplication.GetReplicatedColumnsForTable(DtbBookingConsolidationSchema.Constants.TableName));
			AssertContainsExactElementsInExactOrder("GetReplicatedColumnsForTable() for DtbBooking should return DtbBookingReplicatedColumns", DtbMasterBookingReplication.DtbBookingReplicatedColumns, DtbMasterBookingReplication.GetReplicatedColumnsForTable(DtbBookingSchema.Constants.TableName));
			AssertContainsExactElementsInExactOrder("GetReplicatedColumnsForTable() for DtbBookingInstruction should return DtbBookingInstructionReplicatedColumns", DtbMasterBookingReplication.DtbBookingInstructionReplicatedColumns, DtbMasterBookingReplication.GetReplicatedColumnsForTable(DtbBookingInstructionSchema.Constants.TableName));
			AssertContainsExactElementsInExactOrder("GetReplicatedColumnsForTable() for DtbBookingConfirmation should return DtbBookingConfirmationReplicatedColumns", DtbMasterBookingReplication.DtbBookingConfirmationReplicatedColumns, DtbMasterBookingReplication.GetReplicatedColumnsForTable(DtbBookingConfirmationSchema.Constants.TableName));
			AssertEquals("GetReplicatedColumnsForTable() for any other table should return empty list of columns", 0, DtbMasterBookingReplication.GetReplicatedColumnsForTable(DummyBizoSchema.Constants.TableName).Count());
		}
	}
}
