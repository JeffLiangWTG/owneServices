using System;
using CargoWise.Common;
using CargoWise.Data;
#if DEBUG
using CargoWise.Data.Testing;
#endif
using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Shared
{
	public static class DtbBookingConsolidationDuplicateErrorHelper
	{
		public static bool IsSqlDuplicateBookingConsolidationException(Exception ex)
		{
			if (ex.GetInnermostException() is SqlException sqlEx && new DbErrorMatch(sqlEx).ExceptionType == DbErrorType.CannotInsertDuplicateUniqueIndexKey)
			{
				return sqlEx.Message.Contains(UniqueDtbBookingConsolidationIndexParentIDTableAndDirection);
			}
			else
			{
				return false;
			}
		}

#if DEBUG
		public static ZSaveException CreateSqlDuplicateBookingConsolidationException(BusinessObjectFactory factory)
		{
			return new ZSaveException(
				new ZDataException(
					SqlExceptionBuilder.CreateSqlException(
						errorNumber: CannotInsertDuplicateUniqueIndexKeySqlErrorNumber,
						errorMessage: $@"Unique index [{UniqueDtbBookingConsolidationIndexParentIDTableAndDirection}] violated, cannot insert duplicate booking consolidation."),
					null,
					null),
				factory);
		}
#endif

		public const string UniqueDtbBookingConsolidationIndexParentIDTableAndDirection = "NR_UX__KB_ParentID_KB_ParentTableCode_KB_JobDirection";
		internal const int CannotInsertDuplicateUniqueIndexKeySqlErrorNumber = 2601;
	}
}
