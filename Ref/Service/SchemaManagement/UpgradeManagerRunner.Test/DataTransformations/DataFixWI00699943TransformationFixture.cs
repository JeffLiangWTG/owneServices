using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.UpgradeManagerRunner.DataTransformations;
using NUnit.Framework;
using Moq;
using System.Data;
using Moq.Protected;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test;

class DataFixWI00699943TransformationFixture : TransformationFixture
{
	[Test]
	[TransactionedTestCase]
	public void IsDuplicateDataExists()
	{
		try
		{
			Connection = new SqlConnection(TestConnectionString.GetAdmin(TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe)));
			Connection.Open();
			Transaction = Connection.BeginTransaction();

			Assert.That(DataFixWI00699943TransformationForTest.IsDuplicateDataExists_Exposed(Transaction), Is.False, "[PreCondition]");

			PrepareTestData();

			Assert.That(DataFixWI00699943TransformationForTest.IsDuplicateDataExists_Exposed(Transaction), Is.True);
		}
		finally
		{
			try
			{
				if (Transaction != null)
				{
					Transaction.Rollback();
					Transaction.Dispose();
				}
			}
			finally
			{
				if (Connection != null)
				{
					Connection.Dispose();
				}
			}
		}
	}

	[Test]
	[TransactionedTestCase]
	public void Run_ShouldRetryMax10Times()
	{
		var transformationMock = new Mock<DataFixWI00699943Transformation>(MockBehavior.Default, 0) { CallBase = true };
		transformationMock.Protected().Setup("RunCore", false, ItExpr.IsAny<IDbTransaction>());

		var task = transformationMock.Object;

		try
		{
			Connection = new SqlConnection(TestConnectionString.GetAdmin(TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe)));
			Connection.Open();
			Transaction = Connection.BeginTransaction();

			PrepareTestData();

			Assert.That(DataFixWI00699943TransformationForTest.IsDuplicateDataExists_Exposed(Transaction), Is.True);

			task.Run(Transaction);

			transformationMock.Protected().Verify("RunCore", Times.Exactly(10), ItExpr.IsAny<IDbTransaction>());
		}
		finally
		{
			try
			{
				if (Transaction != null)
				{
					Transaction.Rollback();
					Transaction.Dispose();
				}
			}
			finally
			{
				if (Connection != null)
				{
					Connection.Dispose();
				}
			}
		}
	}

	protected override void PrepareTestData()
	{
		using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = @"IF EXISTS (SELECT *
										FROM sys.indexes
										WHERE [name] = 'IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeType_ZXE_ColumnCaption')
											DROP INDEX [IX_RefCusCodeListAttributeName_ZXE_ZZZ_NKDataGrouping_ZXE_ZZK_NKCodeType_ZXE_ColumnCaption] 
												ON [dbo].[RefCusCodeListAttributeName];

									INSERT INTO RefDataGrouping
										(ZZZ_DataGrouping, ZZZ_Description)
									VALUES
										('ZZZ', 'ZZZ Test Grouping');

									INSERT INTO RefCusCodeType
										([ZZK_CodeType],
										[ZZK_Description],
										[ZZK_IsReadonly],
										[ZZK_MaxLength],
										[ZZK_ZZZ_NKDataGrouping])
									VALUES
										('FAC', 'FAC', 1, 20, 'ZZZ'),
										('EU15', 'EU15', 1, 20, 'ZZZ'),
										('CO15', 'CO15', 1, 20, 'ZZZ');


									INSERT INTO dbo.RefCusCodeListAttributeName
										(ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_ColumnCaption)
									VALUES
										(N'City', N'City', N'FAC', N'ZZZ', N'City'),
										(N'Direction A', N'Direction of the goods Import/Export', N'EU15', N'ZZZ', N'Direction'),
										(N'Direction B', N'Direction of the goods Import/Export', N'CO15', N'ZZZ', N'Direction'),
										(N'Direction C', N'Direction of the goods Import/Export', N'CO15', N'ZZZ', N'Direction'),
										(N'Direction D', N'Direction of the goods Import/Export', N'CO15', N'ZZZ', N'Direction'),
										(N'Direction E', N'Direction of the goods Import/Export', N'CO15', N'ZZZ', N'Direction(3)'),
										(N'Direction F', N'Direction of the goods Import/Export', N'CO15', N'ZZZ', N'SomeVeryVeryVeryLongColumnCaption A'),
										(N'Direction G', N'Direction of the goods Import/Export', N'CO15', N'ZZZ', N'SomeVeryVeryVeryLongColumnCaption A');";
			cmd.Transaction = Transaction;
			cmd.ExecuteNonQuery();
		}
	}

	protected override void AssertTransformationResults()
	{
		Assert.That(DataFixWI00699943TransformationForTest.IsDuplicateDataExists_Exposed(Transaction), Is.False);

		using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = @"SELECT COUNT(*)
									FROM [dbo].[RefCusCodeListAttributeName]
									WHERE [ZXE_ColumnCaption] = 'SomeVeryVeryVeryLongColumnCaption A';";
			cmd.Transaction = Transaction;
			Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0), "Should not have duplicate caption");
		}

		using (var cmd = Connection.CreateCommand())
		{
			cmd.CommandText = @"SELECT COUNT(*)
									FROM [dbo].[RefCusCodeListAttributeName]
									WHERE [ZXE_ColumnCaption] = 'SomeVeryVeryVeryLongColumnCaptio(1)';";
			cmd.Transaction = Transaction;
			Assert.That((int)cmd.ExecuteScalar(), Is.EqualTo(1), "Should truncate the original caption");
		}
	}

	protected override IDataTransformationTask GetTask() => new DataFixWI00699943Transformation(0);

	internal class DataFixWI00699943TransformationForTest : DataFixWI00699943Transformation
	{
		public DataFixWI00699943TransformationForTest(int version) : base(version)
		{
		}

		internal static bool IsDuplicateDataExists_Exposed(IDbTransaction trans) => IsDuplicateDataExists(trans);
	}
}
