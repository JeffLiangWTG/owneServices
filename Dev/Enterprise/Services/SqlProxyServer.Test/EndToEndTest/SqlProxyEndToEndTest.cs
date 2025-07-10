using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using CargoWise.Data.HttpClient;
using CargoWise.Data.SqlProxy.Interface.Models;
using CargoWise.Data.Utils;
using CargoWise.Database.Abstractions;
using Enterprise.Dat.Implementation;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace CargoWise.Data.SqlProxyServer.Test.EndToEndTest;

[TestRequiresAdministrativePrivileges("Admin privilege is required to run the web server.")]
[SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "BusinessObjectFactory not available from SQL Over Http Server side process")]
class SqlProxyEndToEndTest
{
	[TestCase("SELECT 1")]
	[TestCase("SELECT @@version")]
	[TestCase("SELECT count(*) FROM [StmData]")]
	public void ExecuteScalar(string sql)
	{
		var expected = sqlProxy.ExecuteScalarRequest(sql, TestHelper.LazyDatabaseConnection.Value).AsValue();

		using var httpConnection = NewHttpConnection();
		var actual = httpConnection.ExecuteScalar(sql);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.ExtendedPropertyTestCases))]
	public void ExecuteScalarSqlVariantFromExtendedProperty(string propertyName, object? value)
	{
		var rawDataType = value?.GetType();
		using var disposable = TestHelper.SaveDbExtendedProperty(propertyName, value);
		var sql =
			$"SELECT value FROM {Db.DatabaseName}.sys.extended_properties WITH (NOLOCK) WHERE class = 0 AND name = @propertyName;";
		var sqlParameter = new SqlParameter("@propertyName", SqlDbType.NVarChar, 128) { Value = propertyName };

		var scalarResult = sqlProxy.ExecuteScalarRequest(sql, TestHelper.LazyDatabaseConnection.Value, request =>
		{
			request.Parameters =
			[
				SqlParameterDTO.FromSqlParameter(sqlParameter),
			];
		});
		Assert.That(scalarResult, Is.Not.Null);
		var expected = scalarResult.AsValue();

		using var httpConnection = NewHttpConnection();
		var actual = httpConnection.ExecuteScalar(sql, command =>
		{
			command.Parameters.Add(sqlParameter);
		});

		if (value != null)
		{
			Assert.That(actual, Is.InstanceOf(rawDataType!));
		}

		Assert.That(actual, Is.EqualTo(expected));
	}

	[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.ExecuteScalaWithParameterTestCases))]
	public void ExecuteScalarWithParameters(string sql, SqlParameter sqlParameter)
	{
		var scalarResult = sqlProxy.ExecuteScalarRequest(sql, TestHelper.LazyDatabaseConnection.Value, request =>
		{
			request.Parameters =
			[
				SqlParameterDTO.FromSqlParameter(sqlParameter),
			];
		});
		Assert.That(scalarResult, Is.Not.Null);
		var expected = scalarResult.AsValue();

		using var httpConnection = NewHttpConnection();
		var actual = httpConnection.ExecuteScalar(sql, command =>
		{
			command.Parameters.Add(sqlParameter);
		});

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void ExecuteScalarExtendedPropertyDoeNotExist()
	{
		const string propertyName = "DropDbExtendedPropertyIfExist";
		using var disposable = TestHelper.DropDbExtendedProperty(propertyName);
		var sql = $"SELECT value FROM {Db.DatabaseName}.sys.extended_properties WITH (NOLOCK) WHERE class = 0 AND name = @propertyName;";
		var sqlParameter = new SqlParameter("@propertyName", SqlDbType.NVarChar, 128) { Value = propertyName };

		var scalarResult = sqlProxy.ExecuteScalarRequest(sql, TestHelper.LazyDatabaseConnection.Value, request =>
		{
			request.Parameters =
			[
				SqlParameterDTO.FromSqlParameter(sqlParameter),
			];
		});
		Assert.That(scalarResult, Is.Not.Null);
		var expected = scalarResult.AsValue();

		using var httpConnection = NewHttpConnection();
		var actual = httpConnection.ExecuteScalar(sql, command =>
		{
			command.Parameters.Add(sqlParameter);
		});

		Assert.That(actual, Is.EqualTo(expected));
	}

	[TestCase("RegistryUserUpdateVersion")]
	public void ExecuteScalarDataRegGetValueAsInteger(string name)
	{
		const string sql = "DataRegGetValueNOD";
		var sqlParameter = new SqlParameter("@Name", SqlDbType.VarChar, 300) { Value = name };
		var scalarResult = sqlProxy.ExecuteScalarRequest(sql, TestHelper.LazyDatabaseConnection.Value, request =>
		{
			request.CommandType = CommandType.StoredProcedure;
			request.Parameters =
			[
				SqlParameterDTO.FromSqlParameter(sqlParameter),
			];
		});
		Assert.That(scalarResult, Is.Not.Null);
		var expected = scalarResult.AsValue();

		using var httpConnection = NewHttpConnection();
		var actual = httpConnection.ExecuteScalar(sql, command =>
		{
			command.CommandType = CommandType.StoredProcedure;
			command.Parameters.Add(sqlParameter);
		});

		Assert.That(actual, Is.EqualTo(expected));
	}

	[TestCase("SELECT GS_PK, GS_Code, GS_IsActive From dbo.GlbStaff", new [] { "GS_PK", "GS_Code", "GS_IsActive" })]

	public void ExecuteReaderGetDataSetFromQuery(string sql, string[] expectedColumns)
	{
		using var httpConnection = NewHttpConnection();
		var httpCommand = new HttpCommand(httpConnection) { CommandText = sql };
		var httpDataAdapter = HttpDataAdapter.New(httpCommand);

		// Act
		var dataSet = new DataSet { Locale = CultureInfo.InvariantCulture };
		httpDataAdapter.FillSchema(dataSet, SchemaType.Source);

		// Assert
		Assert.That(dataSet.Tables.Count, Is.EqualTo(1));

		var table = dataSet.Tables[0];
		Assert.That(table.Rows.Count, Is.EqualTo(0));
		Assert.That(table.Columns.Count, Is.EqualTo(expectedColumns.Length));

		var actualColumns = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
		foreach (var expectedColumn in expectedColumns)
		{
			Assert.That(actualColumns, Does.Contain(expectedColumn));
		}
	}

	[Test]
	public void BulkReadRegistry()
	{
		var names = new[]
		{
			"DATABASE_SCHEMA_VERSION",
			"DATABASE_MINOR_SCHEMA_VERSION",
			"DatabaseMajorCoreScriptVersion",
			"DatabaseMinorCoreScriptVersion",
			"DatabaseMajorTransformationVersion",
			"DatabaseMinorTransformationVersion",
			"LockTimeout",
			"SuspendAuditTriggers",
		};

		var sql = @"
SELECT SD_Name, SD_BinaryValue
FROM dbo.StmData
WHERE SD_Name in (" + string.Join(", ", names.Select((_, i) => $"@CW{i}")) + @")
AND SD_Owner is null
AND SD_DepartmentGuid is null
AND SD_BinaryValue is not null;
";
		var sqlParams = new SqlParameter[names.Length];
		for (var i = 0; i < names.Length; i++)
		{
			var sqlParam = new SqlParameter($"@CW{i}", SqlDbType.NVarChar, 128) { Value = names[i] };
			sqlParams[i] = sqlParam;
		}

		using var expected = sqlProxy.ExecuteReaderRequest(sql, TestHelper.LazyDatabaseConnection.Value, request =>
		{
			request.Parameters = sqlParams.Select(SqlParameterDTO.FromSqlParameter).ToArray();
		});

		using var httpConnection = NewHttpConnection();
		using var actual = httpConnection.ExecuteReader(sql, command =>
		{
			command.Parameters.AddRange(sqlParams);
		});

		Assert.That(actual.FieldCount, Is.EqualTo(expected.FieldCount), "Field count mismatch");
		for (var i = 0; i < expected.FieldCount; i++)
		{
			Assert.That(actual.GetName(i), Is.EqualTo(expected.GetName(i)), $"Column name mismatch at index {i}");
			Assert.That(actual.GetFieldType(i), Is.EqualTo(expected.GetFieldType(i)), $"Column type mismatch at index {i}");
		}

		while (expected.Read())
		{
			Assert.That(actual.Read(), Is.True, "Actual reader ended before expected");

			var expectedKey = expected.GetString(0);
			var expectedValue = Encoding.Unicode.GetString(expected.ReadAllBytes(1));
			var actualKey = actual.GetString(0);
			var actualValue = Encoding.Unicode.GetString(actual.ReadAllBytes(1));

			Assert.That(actualKey, Is.EqualTo(expectedKey), "Key mismatch");
			Assert.That(actualValue, Is.EqualTo(expectedValue), $"Value mismatch for key: {actualKey}");

			if (expectedKey != "SuspendAuditTriggers")
			{
				var expectedValueInt = Convert.ToInt32(expectedValue);
				var actualValueInt = Convert.ToInt32(actualValue);

				Assert.That(actualValueInt, Is.EqualTo(expectedValueInt), "Value mismatch");
			}
		}
	}

	[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.ReaderQueriesTestCases))]
	public void ExecuteReader(string sql)
	{
		using var expected = sqlProxy.ExecuteReaderRequest(sql, TestHelper.LazyDatabaseConnection.Value);
		using var httpConnection = NewHttpConnection();
		using var actual = httpConnection.ExecuteReader(sql);

		Assert.That(actual.FieldCount, Is.EqualTo(expected.FieldCount), "Field count mismatch");
		for (var i = 0; i < expected.FieldCount; i++)
		{
			Assert.That(actual.GetName(i), Is.EqualTo(expected.GetName(i)), $"Column name mismatch at index {i}");
			Assert.That(actual.GetFieldType(i), Is.EqualTo(expected.GetFieldType(i)), $"Column type mismatch at index {i}");
		}

		while (expected.Read())
		{
			Assert.That(actual.Read(), Is.True, "Actual reader ended before expected");

			for (var i = 0; i < expected.FieldCount; i++)
			{
				var expectedValue = expected.GetValue(i);
				var actualValue = actual.GetValue(i);
				Assert.That(actualValue, Is.EqualTo(expectedValue), $"Value mismatch at column {i} ({expected.GetName(i)}).");
			}
		}

		Assert.That(actual.Read(), Is.False, "Actual reader contains more rows than expected");
	}

	[TestCase("GlbStaff")]
	public void ExecuteReaderStoredProcHelpConstraintHavingMultipleResultSets(string tableName)
	{
		var columns = new[]
		{
			"constraint_type", "constraint_name", "constraint_keys"
		};

		Assert.DoesNotThrow(() =>
		{
			using var httpConnection = NewHttpConnection();
			using var reader = httpConnection.ExecuteReader($"EXEC sp_helpconstraint '{tableName}'");
			reader.NextResult();
			while (reader.Read())
			{
				foreach (var column in columns)
				{
					_ = reader[column].ToString();
				}
			}
		});
	}

	[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.SqlDbTypesTestCases))]
	public void TestSqlValueConverterExecuteReader(string sql, SqlDbType expectedDataType, Action<object, object> assertEqual)
	{
		// Arrange
		using var expected = sqlProxy.ExecuteReaderRequest(sql, TestHelper.LazyDatabaseConnection.Value);

		// Act
		using var httpConnection = NewHttpConnection();
		using var actual = httpConnection.ExecuteReader(sql);

		Assert.That(actual.FieldCount, Is.EqualTo(expected.FieldCount), "Field count mismatch");
		for (var i = 0; i < expected.FieldCount; i++)
		{
			Assert.That(actual.GetName(i), Is.EqualTo(expected.GetName(i)), $"Column name mismatch at index {i}");
			Assert.That(actual.GetFieldType(i), Is.EqualTo(expected.GetFieldType(i)),
				$"Column type mismatch at index {i}");
		}

		// Assert
		Assert.That(expected.Read(), Is.True, "Expected reader ended before expected");
		Assert.That(actual.Read(), Is.True, "Actual reader ended before expected");

		var expectedValue = expected.GetValue(0);
		var actualValue = actual.GetValue(0);
		assertEqual.Invoke(actualValue, expectedValue);
	}

	[TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.SqlDbTypesTestCases))]
	public void TestSqlValueConverterExecuteScalar(string sql, SqlDbType expectedDataType, Action<object?, object?> assertEqual)
	{
		// Arrange
		var expected = sqlProxy.ExecuteScalarRequest(sql, TestHelper.LazyDatabaseConnection.Value).AsValue();

		// Act
		using var httpConnection = NewHttpConnection();
		var actual = httpConnection.ExecuteScalar(sql);

		assertEqual.Invoke(actual, expected);
	}

	[Test]
	public void TestThrowDatabaseUpgradeExceptionOnOpenConnection()
	{
		const string sql = "--Connection.EnsureIsOpen";
		IDisposable? disposable = null;

		try
		{
			Assert.Throws<DatabaseUpgradedException>(() =>
			{
				using var httpConnection = NewHttpConnection();
				httpConnection.ExecuteNoQuery(sql, _ =>
				{
					var mockedSchemaVersionMajor = CargoWise.Application.ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
					var mockedSchemaVersions = Mock.Of<IDatabaseAspectVersions>(x
						=> x.SchemaVersion == new VersionLabel(mockedSchemaVersionMajor, 0));

					disposable = CargoWise.Application.ObjectFactory.Substitute(mockedSchemaVersions);
				});
			});
		}
		finally
		{
			disposable?.Dispose();
		}
	}

	[Test]
	public void TestCountReturnsInt()
	{
		using var httpConnection = NewHttpConnection();
		var result = httpConnection.ExecuteScalar("SELECT COUNT(*) FROM sys.databases");

		Assert.That(result, Is.TypeOf<int>());
	}

	static HttpConnection NewHttpConnection()
	{
		return (HttpConnection)new HttpDataProviderFactory().OpenNewDbConnection<CargoWise.DataProtection.RestrictedWriterLoginCredentials>
		(
			Db.ServerName, Db.DatabaseName, nameof(SqlProxyEndToEndTest), 30, connectionPooling: false, 0, 100, 0
		);
	}

	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
		if (initializeDatabaseDetails)
		{
			Db.ClearServerDetails();
		}

		HttpLoaderFactory.ResetGlowLoaderService();
	}

	[OneTimeSetUp]
	public void OneTimeSetUp()
	{
		if (!Db.DatabaseNameIsInitialized || !Db.ServerNameIsInitialized)
		{
			Db.InitializeDatabaseDetails(LocalDBConnection.GetServerName(), "Odyssey");
			initializeDatabaseDetails = true;
		}

		_ = TestHelper.LazyDatabaseConnection.Value;

		if (!GlobalServiceProvider.TryGetInstance(out _))
		{
			EnterpriseApplicationConfiguration.ConfigureObjectFactory();
		}

		var startupTimespan = TimeSpan.FromMinutes(2);
		using var cancellationTokenSource = new CancellationTokenSource(startupTimespan);
		_ = SqlProxyClientProvider.StartSqlProxyServerProcess(Db.ServerName, Db.DatabaseName, cancellationTokenSource.Token);
		if (!SqlProxyClientProvider.IsHttpEnabled)
		{
			throw new TimeoutException($"Glow server did not start within the expected time {startupTimespan}.");
		}
	}

	readonly Core.SqlProxy sqlProxy = new();
	bool initializeDatabaseDetails;
}
