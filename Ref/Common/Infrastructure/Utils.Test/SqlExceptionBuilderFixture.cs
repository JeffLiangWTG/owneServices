using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class SqlExceptionBuilderFixture
	{
		[Test]
		public void BuildSqlErrorWithErrorNumber()
		{
			var errorNumber = 10;
			var sqlException = new SqlExceptionBuilder().WithErrorNumber(errorNumber).Build();
			Assert.NotNull(sqlException);
			Assert.AreEqual(errorNumber, sqlException.Number);
		}

		[Test]
		public void BuildSqlErrorWithErrorMessage()
		{
			var errorMessage = "An Error";
			var sqlException = new SqlExceptionBuilder().WithErrorMessage(errorMessage).Build();
			Assert.NotNull(sqlException);
			Assert.AreEqual(0, sqlException.Number);
			Assert.AreEqual(errorMessage, sqlException.Message);
		}
	}
}
