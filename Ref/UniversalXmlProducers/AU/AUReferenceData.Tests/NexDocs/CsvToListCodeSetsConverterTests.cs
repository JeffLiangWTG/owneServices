using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CsvHelper;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	[TestFixture]
	class CsvToListCodeSetsConverterBaseOnlyTests
	{
		[Test]
		public void TestInvalidHeaderData()
		{
			ConvertAndAssert(@"Invalid Data
sdf", @"Header Validation Error (Index:0, Data:BooleanField, RawRecord:Invalid Data)
Header Validation Error (Index:0, Data:DateTimeField, RawRecord:Invalid Data)
Header Validation Error (Index:0, Data:DecimalField, RawRecord:Invalid Data)
Header Validation Error (Index:0, Data:DoubleField, RawRecord:Invalid Data)
Header Validation Error (Index:0, Data:FloatField, RawRecord:Invalid Data)
Header Validation Error (Index:0, Data:IntegerField, RawRecord:Invalid Data)
Header Validation Error (Index:0, Data:StringField, RawRecord:Invalid Data)
Missing Field Found (Row: 2, Index:0, Data:BooleanField, RawRecord:sdf)
Missing Field Found (Row: 2, Index:0, Data:DateTimeField, RawRecord:sdf)
Missing Field Found (Row: 2, Index:0, Data:DecimalField, RawRecord:sdf)
Missing Field Found (Row: 2, Index:0, Data:DoubleField, RawRecord:sdf)
Missing Field Found (Row: 2, Index:0, Data:FloatField, RawRecord:sdf)
Missing Field Found (Row: 2, Index:0, Data:IntegerField, RawRecord:sdf)
Missing Field Found (Row: 2, Index:0, Data:StringField, RawRecord:sdf)
Error Parsing Row (Row:2, CurrentIndex:-1, RawRecord:sdf):
No members are mapped for type 'CargoWise.RefDbRepo.AUReferenceData.Tests.CsvToListCodeSetsConverterBaseOnlyTests+CsvToItemCodeSetsConverterTestHelper'.", null);
		}

		[Test]
		public void TestMissingHeaderData()
		{
			ConvertAndAssert(@"BooleanField,DateTimeField,DecimalField,FloatField,IntegerField2,StringField
1,2019-12-2,3,4,5,6", @"Header Validation Error (Index:0, Data:DoubleField, RawRecord:BooleanField,DateTimeField,DecimalField,FloatField,IntegerField2,StringField)
Header Validation Error (Index:0, Data:IntegerField, RawRecord:BooleanField,DateTimeField,DecimalField,FloatField,IntegerField2,StringField)
Missing Field Found (Row: 2, Index:0, Data:DoubleField, RawRecord:1,2019-12-2,3,4,5,6)
Missing Field Found (Row: 2, Index:0, Data:IntegerField, RawRecord:1,2019-12-2,3,4,5,6)", null);
		}

		[Test]
		public void TestBadData()
		{
			ConvertAndAssert<CsvToItemCodeSetsConverterTestHelper2>(@"BooleanField,DateTimeField,DecimalField,FloatField,IntegerField,DoubleField,StringField,StringField2
""1"",""2019-12-2"",""3"",""4"",""5"",""6"",HI WORLD,BYE WORLD""", @"Bad Data Found (Row: 2, CurrentIndex:-1, RawRecord:""1"",""2019-12-2"",""3"",""4"",""5"",""6"",HI WORLD,BYE WORLD"")", null);
		}

		[Test]
		public void TestExtraHeaderData()
		{
			ConvertAndAssert(@"BooleanField,DateTimeField,DecimalField,FloatField,IntegerField,DoubleField,StringField,ExtraField
1,2019-12-2,3,4,5,6,7,8", @"", null);
		}

		[Test]
		public void TestInvalidBooleanConvertion()
		{
			ConvertAndAssert(@"BooleanField,DateTimeField,DecimalField,FloatField,IntegerField,DoubleField,StringField
,2019-12-2,3,4,5,6,7,8", @"Error Parsing Row (Row:2, CurrentIndex:0, RawRecord:,2019-12-2,3,4,5,6,7,8):
The conversion cannot be performed.
    Text: ''
    MemberType: System.Boolean
    TypeConverter: 'CsvHelper.TypeConversion.BooleanConverter'
", null);
		}

		[Test]
		public void TestInvalidDateTimeConvertion()
		{
			ConvertAndAssert(@"BooleanField,DateTimeField,DecimalField,FloatField,IntegerField,DoubleField,StringField
1,sdf,3,4,5,6,7,8", @"Error Parsing Row (Row:2, CurrentIndex:1, RawRecord:1,sdf,3,4,5,6,7,8):
Error converting DateTime for DateTimeField: Text:'sdf'
The string 'sdf' was not recognized as a valid DateTime. There is an unknown word starting at index '0'.", null);
		}

		[Test]
		public void TestEmptyDateTimeConvertion()
		{
			ConvertAndAssert(@"BooleanField,DateTimeField,DecimalField,FloatField,IntegerField,DoubleField,StringField
1,,3,4,5,6,7,8", "", null);
		}

		[Test]
		public void TestInvalidDecimalConvertion()
		{
			ConvertAndAssert(@"BooleanField,DateTimeField,DecimalField,FloatField,IntegerField,DoubleField,StringField
1,2019-12-2,a,4,5,6,7,8", @"Error Parsing Row (Row:2, CurrentIndex:2, RawRecord:1,2019-12-2,a,4,5,6,7,8):
The conversion cannot be performed.
    Text: 'a'
    MemberType: System.Decimal
    TypeConverter: 'CsvHelper.TypeConversion.DecimalConverter'", null);
		}

		[Test]
		public void TestInvalidFloatConvertion()
		{
			ConvertAndAssert(@"BooleanField,DateTimeField,DecimalField,FloatField,IntegerField,DoubleField,StringField
1,2019-12-2,3,a,5,6,7,8", @"Error Parsing Row (Row:2, CurrentIndex:3, RawRecord:1,2019-12-2,3,a,5,6,7,8):
The conversion cannot be performed.
    Text: 'a'
    MemberType: System.Single
    TypeConverter: 'CsvHelper.TypeConversion.SingleConverter'", null);
		}

		[Test]
		public void TestInvalidIntegerConvertion()
		{
			ConvertAndAssert(@"BooleanField,DateTimeField,DecimalField,FloatField,IntegerField,DoubleField,StringField
1,2019-12-2,3,4,a,6,7,8", @"Error Parsing Row (Row:2, CurrentIndex:4, RawRecord:1,2019-12-2,3,4,a,6,7,8):
The conversion cannot be performed.
    Text: 'a'
    MemberType: System.Int32
    TypeConverter: 'CsvHelper.TypeConversion.Int32Converter'", null);
		}

		[Test]
		public void TestInvalidDoubleConvertion()
		{
			ConvertAndAssert(@"BooleanField,DateTimeField,DecimalField,FloatField,IntegerField,DoubleField,StringField
1,2019-12-2,3,4,5,a,7,8", @"Error Parsing Row (Row:2, CurrentIndex:5, RawRecord:1,2019-12-2,3,4,5,a,7,8):
The conversion cannot be performed.
    Text: 'a'
    MemberType: System.Double
    TypeConverter: 'CsvHelper.TypeConversion.DoubleConverter'", null);
		}

		void ConvertAndAssert(string csvData, string errorNotification, Action<IListCodeSet[]> assertListCodeSets)
		{
			ConvertAndAssert<CsvToItemCodeSetsConverterTestHelper>(csvData, errorNotification, assertListCodeSets);
		}

		void ConvertAndAssert<T>(string csvData, string errorNotification, Action<IListCodeSet[]> assertListCodeSets)
			where T : CsvToItemCodeSetsConverterTestHelper
		{
			using (var textReader = new StringReader(csvData))
			using (var csvReader = new CsvReader(textReader))
			{
				var converter = new CsvToListCodeSetsConverter<T>();
				var listCodeSets = converter.Convert(csvReader).ToArray();
				Assert.AreEqual(errorNotification.Trim(), converter.ErrorNotification.Trim(), "ErrorNotification");
				assertListCodeSets?.Invoke(listCodeSets);
			}
		}

		class CsvToItemCodeSetsConverterTestHelper2 : CsvToItemCodeSetsConverterTestHelper
		{
			[CodeSetName("StringField2", CodeSetValueType.@string)]
			public string StringField2 { get; set; }
		}

		class CsvToItemCodeSetsConverterTestHelper : CsvToItemCodeSetsConverter
		{
			[CodeSetName("BooleanField", CodeSetValueType.boolean)]
			public bool BooleanField { get; set; }

			[CodeSetName("DateTimeField", CodeSetValueType.dateTime)]
			public DateTime DateTimeField { get; set; }

			[CodeSetName("DecimalField", CodeSetValueType.@decimal)]
			public decimal DecimalField { get; set; }

			[CodeSetName("DoubleField", CodeSetValueType.@double)]
			public double DoubleField { get; set; }

			[CodeSetName("FloatField", CodeSetValueType.@float)]
			public float FloatField { get; set; }

			[CodeSetName("IntegerField", CodeSetValueType.integer)]
			public int IntegerField { get; set; }

			[CodeSetName("StringField", CodeSetValueType.@string)]
			public string StringField { get; set; }
		}
	}
}
