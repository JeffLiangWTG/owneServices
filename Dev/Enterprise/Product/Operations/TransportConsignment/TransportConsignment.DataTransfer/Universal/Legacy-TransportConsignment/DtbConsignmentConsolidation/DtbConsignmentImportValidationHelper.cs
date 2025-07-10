using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	public class DtbConsignmentImportValidationHelper : LandTransportImportValidationHelper
	{
		#region ConsignmentInstructionPKSchemaColumn

		public override SchemaPKColumn ConsignmentInstructionPKSchemaColumn
		{
			get { return DtbBookingInstructionSchema.PK; }
		}

		#endregion

		#region InstructionTableName

		public override ZString InstructionTableName
		{
			get { return DtbBookingInstructionSchema.Constants.TableName; }
		}

		#endregion

		#region InstructionTypeDelivery

		public override string InstructionTypeDelivery
		{
			get { return InstructionTypes.Codes.Delivery; }
		}

		#endregion

		#region InstructionTypePickup

		public override string InstructionTypePickup
		{
			get { return InstructionTypes.Codes.PickUp; }
		}

		#endregion

		#region GetQueryToLoadInstruction

		public override ZQuery GetQueryToLoadInstruction(IColumnIndexer consignmentRow, ZString addressType)
		{
			var query = new ZQuery();
			query.AddToFilter(DtbBookingInstructionSchema.KN_KM_BookingMovement, consignmentRow.GetValue(DtbBookingSchema.PK));
			query.AddToFilter(DtbBookingInstructionSchema.KN_InstructionType, addressType);

			return query;
		}

		#endregion

	}
}
