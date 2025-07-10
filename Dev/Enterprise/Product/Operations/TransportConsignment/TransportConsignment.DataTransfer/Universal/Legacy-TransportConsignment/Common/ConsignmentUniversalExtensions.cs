using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	static class ConsignmentUniversalExtensions
	{
		public static IColumnIndexer GetConfirmation(this UniversalObjectFactory factory, IColumnIndexer instructionRow, string confirmationType)
		{
			var query = new ZQuery();
			query.AddToFilter(DtbBookingConfirmationSchema.KK_KN_BookingInstruction, instructionRow.GetValue(DtbBookingInstructionSchema.PK));
			query.AddToFilter(DtbBookingConfirmationSchema.KK_ConfirmationType, confirmationType);

			var confirmation = factory.RowFactory.Load(DtbBookingConfirmationSchema.Constants.TableName, query).Single(); // only one Confirmation of each type should exist.
			return DataObjectReader.GetColumnIndexerFromRow(confirmation);
		}

		public static IColumnIndexer GetInstruction(this UniversalObjectFactory factory, IColumnIndexer consignmentRow, string instructionType)
		{
			var query = new ZQuery();
			query.AddToFilter(DtbBookingInstructionSchema.KN_KM_BookingMovement, consignmentRow.GetValue(DtbBookingSchema.PK));
			query.AddToFilter(DtbBookingInstructionSchema.KN_InstructionType, instructionType);

			return DataObjectReader.GetColumnIndexerFromRow(factory.RowFactory.Load(DtbBookingInstructionSchema.Constants.TableName, query).Single()); // only one Instruction of each type should exist.
		}
	}
}


