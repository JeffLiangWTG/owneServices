using System;
using System.Reflection;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common.Models;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class DataSetHelperTest : TransactionedTestCase
	{
		public void TestGetServerData()
		{
			var dataTable = DataSetHelper.CreateDataTable<IDummyStorage>(false, new ValueConverter());
			var serverData = new DummyStorage { D1_Property1 = "AAA", D1_Property2 = 4, D1_IsActive = true };
			var data = DataSetHelper.GetServerData(dataTable, serverData, new ValueConverter(), Tuple.Create(nameof(IDummyStorage.D1_Property2), (object)5));
			AssertEquals(data.Item1, data.Item2[0]);
			AssertEquals("AAA", data.Item2[1]);
			AssertEquals(5, data.Item2[2]);
			dataTable = DataSetHelper.CreateDataTable<IUserDummyStorage>(false, new ValueConverter());
			data = DataSetHelper.GetServerData(dataTable, serverData, new ValueConverter());
			AssertEquals(data.Item1, data.Item2[0]);
			AssertEquals("AAA", data.Item2[1]);
			AssertEquals(4, data.Item2[2]);
			AssertEquals(true, data.Item2[4]);
			AssertEquals(true, data.Item2[5]);
		}

		public void TestGetServerData_WithSqlGeographyColumn()
		{
			var dataTable = DataSetHelper.CreateDataTable<IDummyStorageWithSqlGeographyColumn>(false, new ValueConverterForTest());
			var serverData = new DummyStorageWithSqlGeographyColumn { D1_Property1 = "AAA", D1_Property2 = 4, D1_IsActive = true, D1_GeographyProperty = "POINT(50 30)" };
			var data = DataSetHelper.GetServerData(dataTable, serverData, new ValueConverter(), Tuple.Create(nameof(IDummyStorageWithSqlGeographyColumn.D1_GeographyProperty), (object)"POINT (55 35)"));
			AssertEquals(data.Item1, data.Item2[0]);
			AssertEquals("AAA", data.Item2[1]);
			var expectedSqlGeography = SqlGeography.Parse("POINT (55 35)");
			var actualSqlGeography = data.Item2[4] as SqlGeography;
			AssertNotNull(actualSqlGeography);
			AssertEquals(expectedSqlGeography.Lat, actualSqlGeography.Lat);
			AssertEquals(expectedSqlGeography.Long, actualSqlGeography.Long);
		}

		public void TestCreateDataTable_WithSqlGeographyColumn()
		{
			var dataTable = DataSetHelper.CreateDataTable<IDummyStorageWithSqlGeographyColumn>(true, new ValueConverterForTest());
			AssertEquals(6, dataTable.Columns.Count);
			AssertEquals(dataTable.Columns[nameof(IDummyStorageWithSqlGeographyColumn.D1_PK)].DataType, typeof(Guid));
			AssertEquals(dataTable.Columns[nameof(IDummyStorageWithSqlGeographyColumn.D1_Property1)].DataType, typeof(string));
			AssertEquals(dataTable.Columns[nameof(IDummyStorageWithSqlGeographyColumn.D1_Property2)].DataType, typeof(int));
			AssertEquals(dataTable.Columns[nameof(IDummyStorageWithSqlGeographyColumn.D1_Property3)].DataType, typeof(DateTime));
			AssertEquals(dataTable.Columns[nameof(IDummyStorageWithSqlGeographyColumn.D1_GeographyProperty)].DataType, typeof(SqlGeography));
			AssertEquals(dataTable.Columns[nameof(RefDataSet.Deleted)].DataType, typeof(bool));
			AssertEquals(dataTable.TableName, "DummyStorageWithSqlGeographyColumn");
		}

		public void TestCreateDataTable()
		{
			var dataTable = DataSetHelper.CreateDataTable<IDummyStorage>(true, new ValueConverter());
			AssertEquals(5, dataTable.Columns.Count);
			AssertEquals(dataTable.Columns[nameof(IDummyStorage.D1_PK)].DataType, typeof(Guid));
			AssertEquals(dataTable.Columns[nameof(IDummyStorage.D1_Property1)].DataType, typeof(string));
			AssertEquals(dataTable.Columns[nameof(IDummyStorage.D1_Property2)].DataType, typeof(int));
			AssertEquals(dataTable.Columns[nameof(IDummyStorage.D1_Property3)].DataType, typeof(DateTime));
			AssertEquals(dataTable.Columns[nameof(RefDataSet.Deleted)].DataType, typeof(bool));
			AssertEquals(dataTable.TableName, "DummyStorage");
		}

		protected override void SetUp()
		{
			base.SetUp();
		}

		class ValueConverterForTest : IValueConverter
		{
			public Type GetPropertyType(PropertyInfo property)
			{
				var result = new ValueConverter().GetPropertyType(property);
				if (property.Name == nameof(IDummyStorageWithSqlGeographyColumn.D1_GeographyProperty))
				{
					result = typeof(SqlGeography);
				}
				return result;
			}

			public object GetValue(Type type, object value)
			{
				return new ValueConverter().GetValue(type, value);
			}
		}
	}
}
