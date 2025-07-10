using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	public class LandTransportConsignmentImportValidationHelper : LandTransportImportValidationHelper
	{
		#region ConsignmentInstructionPKSchemaColumn

		public override SchemaPKColumn ConsignmentInstructionPKSchemaColumn
		{
			get { return DtbConsignmentAddressSchema.PK; }
		}

		#endregion

		#region InstructionTableName

		public override ZString InstructionTableName
		{
			get { return DtbConsignmentAddressSchema.Constants.TableName; }
		}

		#endregion

		#region InstructionTypeDelivery

		public override string InstructionTypeDelivery
		{
			get { return ConsignmentAddressTypes.Codes.Delivery; }
		}

		#endregion

		#region InstructionTypePickup

		public override string InstructionTypePickup
		{
			get { return ConsignmentAddressTypes.Codes.PickUp; }
		}

		#endregion

		#region GetQueryToLoadInstruction

		public override ZQuery GetQueryToLoadInstruction(IColumnIndexer consignmentRow, ZString addressType)
		{
			var query = new ZQuery();
			query.AddToFilter(DtbConsignmentAddressSchema.LTS_LTC_Consignment, consignmentRow.GetValue(DtbConsignmentSchema.PK));
			query.AddToFilter(DtbConsignmentAddressSchema.LTS_InstructionType, addressType);

			return query;
		}

		#endregion
	}
}
