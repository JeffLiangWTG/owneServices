using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class CustomFieldsDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestReadCustomFields()
		{
			var customFieldsData = new List<CustomizedField>
			{
				new CustomizedField { Key = "Description", Value = "kalosz", DataType = DataType.String },
				new CustomizedField { Key = "Decimal", Value = 11m.ToString(), DataType = DataType.Decimal },
				new CustomizedField { Key = "Date", Value = new ZDateTime(2012, 7, 25).ToISO8601String(), DataType = DataType.DateTime },
				new CustomizedField { Key = "DateTimeOffset", Value = new ZDateTimeOffset(2012, 7, 25, 1, 2, 3, 123, TimeSpan.FromHours(8)).ToISO8601String(), DataType = DataType.DateTimeOffset },
				new CustomizedField { Key = "Geography", Value = new ZGeography("-121 48").ToString(), DataType = DataType.Geography },
				new CustomizedField { Key = "Bool", Value = true.ToString(), DataType = DataType.Boolean },
				new CustomizedField { Key = "Code", Value = "XXX", DataType = DataType.String },
				new CustomizedField { Key = "Number", Value = 66.ToString(), DataType = DataType.Integer },
				new CustomizedField { Key = "Short", Value = 5.ToString(), DataType = DataType.Short },
				new CustomizedField { Key = "Time", Value = new ZTime(12, 7).ToString(), DataType = DataType.Time },
				new CustomizedField { Key = "Byte...my shiny metal !@#$", Value = 1.ToString(), DataType = DataType.Byte }
			};

			var customFieldInfos = new List<CustomFieldInfo>
				{
					CustomFieldInfo.New(true, "Description", "", DummyBizoSchema.Z0_Description),
					CustomFieldInfo.New(true, "Decimal", "", DummyBizoSchema.Z0_Decimal),
					CustomFieldInfo.New(true, "Date", "", DummyBizoSchema.Z0_Date),
					CustomFieldInfo.New(true, "DateTimeOffset", "", DummyBizoSchema.Z0_DateTimeOffset),
					CustomFieldInfo.New(true, "Geography", "", DummyBizoSchema.Z0_Geography),
					CustomFieldInfo.New(true, "Bool", "", DummyBizoSchema.Z0_Bool),
					CustomFieldInfo.New(false, "Code", "", DummyBizoSchema.Z0_Code),
					CustomFieldInfo.New(true, "Number", "", DummyBizoSchema.Z0_Number),
					CustomFieldInfo.New(true, "Short", "", DummyBizoSchema.Z0_Short),
					CustomFieldInfo.New(true, "Time", "", DummyBizoSchema.Z0_Time),
					CustomFieldInfo.New(true, "Byte...my shiny metal !@#$", "", DummyBizoSchema.Z0_Byte)
				};

			var customizedFieldDescriptor = new DummyCustomFieldsDescriptor(customFieldInfos);
			var dummy = Factory.New<DummyBusinessObject>();

			var reader = new CustomFieldsDataObjectReader<DummyBusinessObject>(new TestErrorLogger(), dummy, customizedFieldDescriptor);
			reader.ReadCustomFields(customFieldsData);

			AssertEquals("Z0_Description", "kalosz", dummy.Z0_Description);
			AssertEquals("Z0_Decimal", 11m, dummy.Z0_Decimal);
			AssertEquals("Z0_Date", new ZDateTime(2012, 7, 25), dummy.Z0_Date);
			AssertEquals("Z0_DateTimeOffset", new ZDateTimeOffset(2012, 7, 25, 1, 2, 3, 123, TimeSpan.FromHours(8)), dummy.Z0_DateTimeOffset);
			AssertEquals("Z0_Geography", new ZGeography("-121 48"), dummy.Z0_Geography);
			AssertEquals("Z0_Bool", true, dummy.Z0_Bool);
			AssertEquals("Z0_Code", "XXX", dummy.Z0_Code);
			AssertEquals("Z0_Number", 66, dummy.Z0_Number);
			AssertEquals("Z0_Short", (short)5, dummy.Z0_Short);
			AssertEquals("Z0_Time", new ZTime(12, 7), dummy.Z0_Time);
			AssertEquals("Z0_Byte", (byte)1, dummy.Z0_Byte);
		}

		public void TestReadCustomFields_ReadInactive()
		{
			var customFieldsData = new List<CustomizedField>
			{
				new CustomizedField { Key = "Description", Value = "kalosz", DataType = DataType.String }
			};

			var customizedFieldDescriptor = new DummyCustomFieldsDescriptor(new[]
			{
				CustomFieldInfo.New(false, "Description", "", DummyBizoSchema.Z0_Description)
			});

			var dummy = Factory.New<DummyBusinessObject>();

			var reader = new CustomFieldsDataObjectReader<DummyBusinessObject>(new TestErrorLogger(), dummy, customizedFieldDescriptor);
			reader.ReadCustomFields(customFieldsData);

			AssertEquals("Z0_Description", "kalosz", dummy.Z0_Description);
		}

		public void TestReadCustomFields_ElementWithInvalidKey()
		{
			var customFieldsData = new List<CustomizedField>
			{
				new CustomizedField { Key = "ZOOM!", Value = "kalosz", DataType = DataType.String }
			};

			var customizedFieldDescriptor = new DummyCustomFieldsDescriptor(new[]
			{
				CustomFieldInfo.New(true, "Description", "", DummyBizoSchema.Z0_Description)
			});

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = ZString.Empty;

			var reader = new CustomFieldsDataObjectReader<DummyBusinessObject>(new TestErrorLogger(), dummy, customizedFieldDescriptor);
			reader.ReadCustomFields(customFieldsData);

			AssertEquals("Z0_Description", ZString.Empty, dummy.Z0_Description);
		}

		public void TestReadCustomFields_DateTimeIgnoresTimezone()
		{
			var customFieldsData = new List<CustomizedField>
			{
				new CustomizedField { Key = "Date", Value = "2021-04-18T00:00:00Z", DataType = DataType.DateTime }
			};

			var customizedFieldDescriptor = new DummyCustomFieldsDescriptor(new[]
			{
				CustomFieldInfo.New(true, "Date", "", DummyBizoSchema.Z0_Date)
			});

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Date = ZDateTime.Empty;

			var reader = new CustomFieldsDataObjectReader<DummyBusinessObject>(new TestErrorLogger(), dummy, customizedFieldDescriptor);
			reader.ReadCustomFields(customFieldsData);

			AssertEquals("Z0_Date", new ZDateTime(new DateTime(2021, 04, 18, 0, 0, 0)), dummy.Z0_Date);
		}

		public void TestReadCustomFields_ElementWithInvalidDataType()
		{
			var customFieldsData = new List<CustomizedField>
			{
				new CustomizedField { Key = "Decimal", Value = 24.ToString(), DataType = DataType.Boolean }
			};

			var customizedFieldDescriptor = new DummyCustomFieldsDescriptor(new[]
			{
				CustomFieldInfo.New(true, "Decimal", "", DummyBizoSchema.Z0_Decimal)
			});

			var dummy = Factory.New<DummyBusinessObject>();

			var reader = new CustomFieldsDataObjectReader<DummyBusinessObject>(new TestErrorLogger(), dummy, customizedFieldDescriptor);
			reader.ReadCustomFields(customFieldsData);

			AssertEquals("Z0_Decimal", 24m, dummy.Z0_Decimal);
		}

		public void TestReadCustomFields_MismatchedData()
		{
			var customFieldsData = new List<CustomizedField>
			{
				new CustomizedField { Key = "Decimal", Value = true.ToString(), DataType = DataType.Boolean }
			};

			var customizedFieldDescriptor = new DummyCustomFieldsDescriptor(new[]
			{
				CustomFieldInfo.New(true, "Decimal", "", DummyBizoSchema.Z0_Decimal)
			});

			var dummy = Factory.New<DummyBusinessObject>();

			var logger = new TestErrorLogger();
			var reader = new CustomFieldsDataObjectReader<DummyBusinessObject>(logger, dummy, customizedFieldDescriptor);
			reader.ReadCustomFields(customFieldsData);

			AssertEquals("Z0_Decimal", ZDecimal.Zero, dummy.Z0_Decimal);
			AssertMultilineASCIIEquals("log",
			@"Warning - Custom Fields - Invalid value [True]. Value must be a valid Decimal.",
			logger.Logs);
		}

		public void TestReadCustomFields_DuplicatedElement()
		{
			var customFieldsData = new List<CustomizedField>
			{
				new CustomizedField { Key = "Decimal", Value = 11m.ToString(), DataType = DataType.Decimal },
				new CustomizedField { Key = "Decimal", Value = 22m.ToString(), DataType = DataType.Decimal }
			};

			var customizedFieldDescriptor = new DummyCustomFieldsDescriptor(new[]
			{
				CustomFieldInfo.New(true, "Decimal", "", DummyBizoSchema.Z0_Decimal)
			});

			var dummy = Factory.New<DummyBusinessObject>();

			var logger = new TestErrorLogger();
			var reader = new CustomFieldsDataObjectReader<DummyBusinessObject>(logger, dummy, customizedFieldDescriptor);
			reader.ReadCustomFields(customFieldsData);

			AssertEquals("Z0_Decimal", 11m, dummy.Z0_Decimal);
			AssertMultilineASCIIEquals("log",
			@"Warning - Element Decimal has duplicates. Only the value from the first element one will be used.",
			logger.Logs);
		}

		public void TestReadCustomFields_DataWithOverflowingData()
		{
			var customFieldsData = new List<CustomizedField>
			{
				new CustomizedField { Key = "Description", Value = new string('a', DummyBizoSchema.Z0_Description.MaxLength + 1), DataType = DataType.String },
				new CustomizedField { Key = "Decimal", Value = "999999999999999999999999999999999999999999999999999999", DataType = DataType.Decimal }
			};

			var customizedFieldDescriptor = new DummyCustomFieldsDescriptor(new[]
			{
				CustomFieldInfo.New(true, "Description", "", DummyBizoSchema.Z0_Description),
				CustomFieldInfo.New(true, "Decimal", "", DummyBizoSchema.Z0_Decimal)
			});

			var dummy = Factory.New<DummyBusinessObject>();

			var logger = new TestErrorLogger();
			var reader = new CustomFieldsDataObjectReader<DummyBusinessObject>(logger, dummy, customizedFieldDescriptor);
			reader.ReadCustomFields(customFieldsData);

			AssertEquals("Description has been trimmed", new string('a', DummyBizoSchema.Z0_Description.MaxLength), dummy.Z0_Description);
			AssertEquals("Decimal has been trimmed", ZDecimal.Zero, dummy.Z0_Decimal);
			AssertMultilineASCIIEquals("log",
@"Warning - Attempted to insert 101 characters into Field [Z0_Description] which has a maximum length of 100 characters. Field was truncated.
Warning - Custom Fields - Invalid value [999999999999999999999999999999999999999999999999999999]. Value must be a valid Decimal.",
			logger.Logs);
		}

		public void TestReadCustomFields_KeyIsCaseInsensitive()
		{
			var customFieldsData = new List<CustomizedField>
			{
				new CustomizedField { Key = "DeSCRipTIOn", Value = "cat food", DataType = DataType.String },
			};

			var customizedFieldDescriptor = new DummyCustomFieldsDescriptor(new[]
			{
				CustomFieldInfo.New(true, "Description", "", DummyBizoSchema.Z0_Description)
			});

			var dummy = Factory.New<DummyBusinessObject>();

			var logger = new TestErrorLogger();
			var reader = new CustomFieldsDataObjectReader<DummyBusinessObject>(logger, dummy, customizedFieldDescriptor);
			reader.ReadCustomFields(customFieldsData);

			AssertEquals("should import data", "cat food", dummy.Z0_Description);
		}
	}
}
