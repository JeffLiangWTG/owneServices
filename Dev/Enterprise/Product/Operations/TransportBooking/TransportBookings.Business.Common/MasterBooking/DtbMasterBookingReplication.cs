using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Shared
{
	public static class DtbMasterBookingReplication
	{
		public static IEnumerable<SchemaColumn> DtbBookingConsolidationReplicatedColumns
		{
			get
			{
				yield return DtbBookingConsolidationSchema.KB_JobDirection;
			}
		}

		public static IEnumerable<SchemaColumn> DtbBookingReplicatedColumns
		{
			get
			{
				yield return DtbBookingSchema.KM_KT_NKBookingTemplate;
				yield return DtbBookingSchema.KM_IsActive;
				yield return DtbBookingSchema.KM_Status;
				yield return DtbBookingSchema.KM_Description;
				yield return DtbBookingSchema.KM_Direction;
				yield return DtbBookingSchema.KM_RS_NKServiceLevel;
				yield return DtbBookingSchema.KM_PL_NKCarrierServiceLevel;
				yield return DtbBookingSchema.KM_RatingFreightMode;
				yield return DtbBookingSchema.KM_Distance;
				yield return DtbBookingSchema.KM_DistanceUnit;
				yield return DtbBookingSchema.KM_OAN_CarrierAccount;
				yield return DtbBookingSchema.KM_GB_Branch;
				yield return DtbBookingSchema.KM_BookingOfTransportRequestedDate;
				yield return DtbBookingSchema.KM_IsAgentBooking;
				yield return DtbBookingSchema.KM_TransportReference;
				yield return DtbBookingSchema.KM_TransportMode;
			}
		}

		public static IEnumerable<SchemaColumn> DtbBookingInstructionReplicatedColumns
		{
			get
			{
				yield return DtbBookingInstructionSchema.KN_Sequence;
				yield return DtbBookingInstructionSchema.KN_InstructionType;
				yield return DtbBookingInstructionSchema.KN_DropMode;
				yield return DtbBookingInstructionSchema.KN_ServiceInstruction;
				yield return DtbBookingInstructionSchema.KN_Status;
				yield return DtbBookingInstructionSchema.KN_RQ_Equipment;
				yield return DtbBookingInstructionSchema.KN_IsContainerRateable;
				yield return DtbBookingInstructionSchema.KN_IsLooseRateable;
				yield return DtbBookingInstructionSchema.KN_TZ_DomesticZone;
				yield return DtbBookingInstructionSchema.KN_IsAuthorisedToLeave;
			}
		}

		public static IEnumerable<SchemaColumn> DtbBookingConfirmationReplicatedColumns
		{
			get
			{
				yield return DtbBookingConfirmationSchema.KK_ConfirmationType;
				yield return DtbBookingConfirmationSchema.KK_Estimated;
				yield return DtbBookingConfirmationSchema.KK_EstimatedUtc;
				yield return DtbBookingConfirmationSchema.KK_Actual;
				yield return DtbBookingConfirmationSchema.KK_RequiredFrom;
				yield return DtbBookingConfirmationSchema.KK_RequiredFromUtc;
				yield return DtbBookingConfirmationSchema.KK_RequiredTo;
				yield return DtbBookingConfirmationSchema.KK_RequiredToUtc;
				yield return DtbBookingConfirmationSchema.KK_ReferenceNum;
				yield return DtbBookingConfirmationSchema.KK_ReceivedBySignature;
				yield return DtbBookingConfirmationSchema.KK_ReceivedBy;
				yield return DtbBookingConfirmationSchema.KK_SlotDateTime;
				yield return DtbBookingConfirmationSchema.KK_SlotReference;
				yield return DtbBookingConfirmationSchema.KK_OC_Driver;
				yield return DtbBookingConfirmationSchema.KK_VehicleRegistration;
				yield return DtbBookingConfirmationSchema.KK_DocumentID;
				yield return DtbBookingConfirmationSchema.KK_DocumentType;
				yield return DtbBookingConfirmationSchema.KK_DocumentIssuer;
				// currently not replicating KK_KD_BookingInstructionPkgDivot or KK_Quantity - we are not yet supporting partial confirmations
			}
		}

		public static IEnumerable<ZPropertyInfo> GetPropertyInfosFromListOfColumns(BusinessObject businessObject, IEnumerable<SchemaColumn> columns)
		{
			return columns.Select(c => businessObject.FindPropertyInfo(c.Name));
		}

		public static IEnumerable<SchemaColumn> GetReplicatedColumnsForTable(string tableName)
		{
			switch (tableName)
			{
				case DtbBookingConsolidationSchema.Constants.TableName:
					return DtbBookingConsolidationReplicatedColumns;

				case DtbBookingSchema.Constants.TableName:
					return DtbBookingReplicatedColumns;

				case DtbBookingInstructionSchema.Constants.TableName:
					return DtbBookingInstructionReplicatedColumns;

				case DtbBookingConfirmationSchema.Constants.TableName:
					return DtbBookingConfirmationReplicatedColumns;

				default:
					return Enumerable.Empty<SchemaColumn>();
			}
		}
	}
}
