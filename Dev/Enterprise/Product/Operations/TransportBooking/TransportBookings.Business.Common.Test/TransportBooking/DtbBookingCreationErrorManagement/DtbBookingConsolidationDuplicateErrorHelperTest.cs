using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Shared.Testing
{
	public class DtbBookingConsolidationDuplicateErrorHelperTest : TestCase
	{
		public void TestIsSqlDuplicateBookingConsolidationException_NestedDuplicateOnBookingConsolidation()
		{
			var innerEx = SqlExceptionBuilder.CreateSqlException(
				errorNumber: DtbBookingConsolidationDuplicateErrorHelper.CannotInsertDuplicateUniqueIndexKeySqlErrorNumber,
				errorMessage: "blah blah " + DtbBookingConsolidationDuplicateErrorHelper.UniqueDtbBookingConsolidationIndexParentIDTableAndDirection);
			var ex = new ZSaveException(new ZDataException(innerEx, null, null), new BusinessObjectFactory());
			AssertEquals(
				"Should return true for nested SqlException with error 2601 mentioning index " + DtbBookingConsolidationDuplicateErrorHelper.UniqueDtbBookingConsolidationIndexParentIDTableAndDirection,
				true,
				DtbBookingConsolidationDuplicateErrorHelper.IsSqlDuplicateBookingConsolidationException(ex));
		}

		public void TestIsSqlDuplicateBookingConsolidationException_DuplicateOnBookingConsolidation()
		{
			var ex = SqlExceptionBuilder.CreateSqlException(
	errorNumber: DtbBookingConsolidationDuplicateErrorHelper.CannotInsertDuplicateUniqueIndexKeySqlErrorNumber,
	errorMessage: "blah blah " + DtbBookingConsolidationDuplicateErrorHelper.UniqueDtbBookingConsolidationIndexParentIDTableAndDirection);
			AssertEquals(
				"Should return true for SqlException with error 2601 mentioning index " + DtbBookingConsolidationDuplicateErrorHelper.UniqueDtbBookingConsolidationIndexParentIDTableAndDirection,
				true,
				DtbBookingConsolidationDuplicateErrorHelper.IsSqlDuplicateBookingConsolidationException(ex));
		}

		public void TestIsSqlDuplicateBookingConsolidationException_DuplicateWithoutMentioningIndex()
		{
			var ex = SqlExceptionBuilder.CreateSqlException(
	errorNumber: DtbBookingConsolidationDuplicateErrorHelper.CannotInsertDuplicateUniqueIndexKeySqlErrorNumber,
	errorMessage: "other error message");
			AssertEquals(
				"Should return false for SqlException with error 2601 but not mentioning index " + DtbBookingConsolidationDuplicateErrorHelper.UniqueDtbBookingConsolidationIndexParentIDTableAndDirection,
				false,
				DtbBookingConsolidationDuplicateErrorHelper.IsSqlDuplicateBookingConsolidationException(ex));
		}

		public void TestIsSqlDuplicateBookingConsolidationException_OtherSqlError()
		{
			var ex = SqlExceptionBuilder.CreateSqlException(
errorNumber: 2702,
errorMessage: "different sql error");
			AssertEquals(
				"Should return false for SqlException with error number not 2601",
				false,
				DtbBookingConsolidationDuplicateErrorHelper.IsSqlDuplicateBookingConsolidationException(ex));
		}

		public void TestIsSqlDuplicateBookingConsolidationException_OtherError()
		{
			var ex = new Exception("some other exception");
			AssertEquals(
				"Should return false for SqlException with error number not 2601",
				false,
				DtbBookingConsolidationDuplicateErrorHelper.IsSqlDuplicateBookingConsolidationException(ex));
		}

		public void TestCreateSqlDuplicateBookingConsolidationException()
		{
			var ex = DtbBookingConsolidationDuplicateErrorHelper.CreateSqlDuplicateBookingConsolidationException(new BusinessObjectFactory());

			AssertEquals("Returned exception should be correct type", typeof(ZSaveException), ex.GetType());
			AssertEquals("Returned exception first level InnerException should be correct type", typeof(ZDataException), ex.InnerException.GetType());
			AssertEquals("Returned exception's second level and innermost exception should be correct type", typeof(SqlException), ex.InnerException.InnerException.GetType());
			AssertContains("Returned exception's second level and innermost exception should contain name of index constraint violated", DtbBookingConsolidationDuplicateErrorHelper.UniqueDtbBookingConsolidationIndexParentIDTableAndDirection, ex.InnerException.InnerException.Message);
		}
	}
}
