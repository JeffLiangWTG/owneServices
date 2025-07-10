using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestImportToGenAddOnColumn()
		{
			var boolValue = true;
			var byteValue = 1;
			var dateValue = new ZDate(2020, 7, 20);
			var dateTimeValue = new ZDateTime(2017, 7, 6);
			var guidValue = new ZGuid("9AA64150-35A6-47B0-BD8C-D71274C8031A");
			var decimalValue = 2m;
			var integerValue = 3;
			var shortValue = 20;
			var stringValue = "String";

			var addInfoCollection = new List<AddInfo>();
			addInfoCollection.AddSafe(new AddInfo() { Key = DummyGenAddOnBusinessObject.StringPropertyAddOn, Value = stringValue });
			addInfoCollection.AddSafe(new AddInfo() { Key = DummyGenAddOnBusinessObject.BooleanPropertyAddOn, Value = boolValue.ToString() });
			addInfoCollection.AddSafe(new AddInfo() { Key = DummyGenAddOnBusinessObject.DatePropertyAddOn, Value = dateValue.ToString() });
			addInfoCollection.AddSafe(new AddInfo() { Key = DummyGenAddOnBusinessObject.DateTimePropertyAddOn, Value = dateTimeValue.ToString() });
			addInfoCollection.AddSafe(new AddInfo() { Key = DummyGenAddOnBusinessObject.DecimalPropertyAddOn, Value = decimalValue.ToString() });
			addInfoCollection.AddSafe(new AddInfo() { Key = DummyGenAddOnBusinessObject.GuidPropertyAddOn, Value = guidValue.ToString() });
			addInfoCollection.AddSafe(new AddInfo() { Key = DummyGenAddOnBusinessObject.IntegerPropertyAddOn, Value = integerValue.ToString() });
			addInfoCollection.AddSafe(new AddInfo() { Key = DummyGenAddOnBusinessObject.ShortPropertyAddOn, Value = shortValue.ToString() });
			addInfoCollection.AddSafe(new AddInfo() { Key = DummyGenAddOnBusinessObject.BytePropertyAddOn, Value = byteValue.ToString() });

			var genAddOnDetailList = new List<GenAddOnDetail>()
			{
				new GenAddOnDetail() { PropertyName = nameof(DummyGenAddOnBusinessObject.StringProperty), AddInfoKey = DummyGenAddOnBusinessObject.StringPropertyAddOn, GenAddOnColumnName = DummyGenAddOnBusinessObject.StringPropertyAddOn, TypeCode = AddOnColumnDataType.Codes.String },
				new GenAddOnDetail() { PropertyName = nameof(DummyGenAddOnBusinessObject.BooleanProperty), AddInfoKey = DummyGenAddOnBusinessObject.BooleanPropertyAddOn, GenAddOnColumnName = DummyGenAddOnBusinessObject.BooleanPropertyAddOn, TypeCode = AddOnColumnDataType.Codes.Boolean },
				new GenAddOnDetail() { PropertyName = nameof(DummyGenAddOnBusinessObject.DateProperty), AddInfoKey = DummyGenAddOnBusinessObject.DatePropertyAddOn, GenAddOnColumnName = DummyGenAddOnBusinessObject.DatePropertyAddOn, TypeCode = AddOnColumnDataType.Codes.Date },
				new GenAddOnDetail() { PropertyName = nameof(DummyGenAddOnBusinessObject.DateTimeProperty), AddInfoKey = DummyGenAddOnBusinessObject.DateTimePropertyAddOn, GenAddOnColumnName = DummyGenAddOnBusinessObject.DateTimePropertyAddOn, TypeCode = AddOnColumnDataType.Codes.Datetime },
				new GenAddOnDetail() { PropertyName = nameof(DummyGenAddOnBusinessObject.DecimalProperty), AddInfoKey = DummyGenAddOnBusinessObject.DecimalPropertyAddOn, GenAddOnColumnName = DummyGenAddOnBusinessObject.DecimalPropertyAddOn, TypeCode = AddOnColumnDataType.Codes.Decimal },
				new GenAddOnDetail() { PropertyName = nameof(DummyGenAddOnBusinessObject.GuidProperty), AddInfoKey = DummyGenAddOnBusinessObject.GuidPropertyAddOn, GenAddOnColumnName = DummyGenAddOnBusinessObject.GuidPropertyAddOn, TypeCode = AddOnColumnDataType.Codes.Guid },
				new GenAddOnDetail() { PropertyName = nameof(DummyGenAddOnBusinessObject.IntegerProperty), AddInfoKey = DummyGenAddOnBusinessObject.IntegerPropertyAddOn, GenAddOnColumnName = DummyGenAddOnBusinessObject.IntegerPropertyAddOn, TypeCode = AddOnColumnDataType.Codes.Integer },
				new GenAddOnDetail() { PropertyName = nameof(DummyGenAddOnBusinessObject.ShortProperty), AddInfoKey = DummyGenAddOnBusinessObject.ShortPropertyAddOn, GenAddOnColumnName = DummyGenAddOnBusinessObject.ShortPropertyAddOn, TypeCode = AddOnColumnDataType.Codes.Short },
				new GenAddOnDetail() { PropertyName = nameof(DummyGenAddOnBusinessObject.ByteProperty), AddInfoKey = DummyGenAddOnBusinessObject.BytePropertyAddOn, GenAddOnColumnName = DummyGenAddOnBusinessObject.BytePropertyAddOn, TypeCode = AddOnColumnDataType.Codes.Byte },
			};

			var dummyBO = Factory.New<DummyGenAddOnBusinessObject>();
			new GenAddOnColumnCollectionDataObjectReader(logger).ReadIntoBusinessObject(addInfoCollection, genAddOnDetailList, dummyBO);
			AssertEquals("String", stringValue, dummyBO.StringProperty);
			AssertEquals("Boolean", true, dummyBO.BooleanProperty);
			AssertEquals("Date", dateValue, dummyBO.DateProperty);
			AssertEquals("DateTime", dateTimeValue, dummyBO.DateTimeProperty);
			AssertEquals("Guid", guidValue, dummyBO.GuidProperty);
			AssertEquals("Decimal", decimalValue, dummyBO.DecimalProperty);
			AssertEquals("Integer", integerValue, dummyBO.IntegerProperty);
			AssertEquals("Short", shortValue, dummyBO.ShortProperty);
			AssertEquals("Byte", byteValue, dummyBO.ByteProperty);

			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var dummyBO2 = Factory.New<DummyGenAddOnBusinessObject>();
			new GenAddOnColumnCollectionDataObjectReader(logger).ReadIntoBusinessObject(addInfoCollection, genAddOnDetailList, dummyBO2);
			AssertEquals("String", stringValue, dummyBO2.StringProperty);
			AssertEquals("Boolean", true, dummyBO2.BooleanProperty);
			AssertEquals("Date", dateValue, dummyBO2.DateProperty);
			AssertEquals("DateTime", dateTimeValue, dummyBO2.DateTimeProperty);
			AssertEquals("Guid", guidValue, dummyBO2.GuidProperty);
			AssertEquals("Decimal", decimalValue, dummyBO2.DecimalProperty);
			AssertEquals("Integer", integerValue, dummyBO2.IntegerProperty);
			AssertEquals("Short", shortValue, dummyBO2.ShortProperty);
			AssertEquals("Byte", byteValue, dummyBO2.ByteProperty);
		}

		[SystemDefinedValues]
		public class DummyGenAddOnBusinessObject : DummyBusinessObject
		{
			public const string StringPropertyAddOn = "STP";
			public const string BooleanPropertyAddOn = "BLP";
			public const string DatePropertyAddOn = "DAP";
			public const string DateTimePropertyAddOn = "DTP";
			public const string DecimalPropertyAddOn = "DCP";
			public const string GuidPropertyAddOn = "GDP";
			public const string IntegerPropertyAddOn = "ITP";
			public const string ShortPropertyAddOn = "SHP";
			public const string BytePropertyAddOn = "BTP";

			public DummyGenAddOnBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public ZString StringProperty
			{
				get
				{
					return (GenAddOnForStringProperty != null) ? GenAddOnForStringProperty.XA_Data : ZString.Empty;
				}
				set
				{
					if (!value.IsEmpty)
					{
						if (GenAddOnForStringProperty == null)
						{
							GenAddOnHelper.FindOrMakeNewAddOn(StringPropertyAddOn, this, out genAddOnForStringProperty);
							RegisterEditableChildObject(GenAddOnForStringProperty);
						}
						CheckMaximumLength(StringPropertyInfo, value);
						GenAddOnForStringProperty.XA_Data = value;
					}
					else
					{
						if (GenAddOnForStringProperty != null)
						{
							GenAddOnForStringProperty.XA_Data = ZString.Empty;
							GenAddOnForStringProperty.Delete();
							genAddOnForStringProperty = null;
						}
					}
				}
			}

			public ZPropertyInfo StringPropertyInfo => GetZPropertyInfo(nameof(StringProperty));

			GenAddOnColumn GenAddOnForStringProperty
			{
				get
				{
					if (genAddOnForStringProperty == null || genAddOnForStringProperty.IsDeleted)
					{
						GenAddOnHelper.Find(StringPropertyAddOn, this, out genAddOnForStringProperty);
						if (genAddOnForStringProperty != null)
						{
							RegisterEditableChildObject(genAddOnForStringProperty);
						}
					}
					return genAddOnForStringProperty;
				}
			}
			GenAddOnColumn genAddOnForStringProperty;

			public ZBool BooleanProperty
			{
				get
				{
					return (GenAddOnForBooleanProperty != null) ? new ZBool(GenAddOnForBooleanProperty.XA_Data) : ZBool.False;
				}
				set
				{
					if (!value.IsEmpty)
					{
						if (GenAddOnForBooleanProperty == null)
						{
							GenAddOnHelper.FindOrMakeNewAddOn(BooleanPropertyAddOn, this, out genAddOnForBooleanProperty);
							RegisterEditableChildObject(GenAddOnForBooleanProperty);
						}
						GenAddOnForBooleanProperty.XA_Data = value.ToString();
					}
					else
					{
						if (GenAddOnForBooleanProperty != null)
						{
							GenAddOnForBooleanProperty.XA_Data = ZString.Empty;
							GenAddOnForBooleanProperty.Delete();
							genAddOnForBooleanProperty = null;
						}
					}
				}
			}

			public ZPropertyInfo BooleanPropertyInfo => GetZPropertyInfo(nameof(BooleanProperty));

			GenAddOnColumn GenAddOnForBooleanProperty
			{
				get
				{
					if (genAddOnForBooleanProperty == null || genAddOnForBooleanProperty.IsDeleted)
					{
						GenAddOnHelper.Find(BooleanPropertyAddOn, this, out genAddOnForBooleanProperty);
						if (genAddOnForBooleanProperty != null)
						{
							RegisterEditableChildObject(genAddOnForBooleanProperty);
						}
					}
					return genAddOnForBooleanProperty;
				}
			}
			GenAddOnColumn genAddOnForBooleanProperty;

			public ZDate DateProperty
			{
				get
				{
					return (GenAddOnForDateProperty != null) ? new ZDate(GenAddOnForDateProperty.XA_Data) : ZDate.Empty;
				}
				set
				{
					if (!value.IsEmpty)
					{
						if (GenAddOnForDateProperty == null)
						{
							GenAddOnHelper.FindOrMakeNewAddOn(DatePropertyAddOn, this, out genAddOnForDateProperty);
							RegisterEditableChildObject(GenAddOnForDateProperty);
						}
						GenAddOnForDateProperty.XA_Data = value.ToString();
					}
					else
					{
						if (GenAddOnForDateProperty != null)
						{
							GenAddOnForDateProperty.XA_Data = ZString.Empty;
							GenAddOnForDateProperty.Delete();
							genAddOnForDateProperty = null;
						}
					}
				}
			}

			public ZPropertyInfo DatePropertyInfo => GetZPropertyInfo(nameof(DateProperty));

			GenAddOnColumn GenAddOnForDateProperty
			{
				get
				{
					if (genAddOnForDateProperty == null || genAddOnForDateProperty.IsDeleted)
					{
						GenAddOnHelper.Find(DatePropertyAddOn, this, out genAddOnForDateProperty);
						if (genAddOnForDateProperty != null)
						{
							RegisterEditableChildObject(genAddOnForDateProperty);
						}
					}
					return genAddOnForDateProperty;
				}
			}
			GenAddOnColumn genAddOnForDateProperty;

			public ZDateTime DateTimeProperty
			{
				get
				{
					return (GenAddOnForDateTimeProperty != null) ? new ZDateTime(GenAddOnForDateTimeProperty.XA_Data) : ZDateTime.Empty;
				}
				set
				{
					if (!value.IsEmpty)
					{
						if (GenAddOnForDateTimeProperty == null)
						{
							GenAddOnHelper.FindOrMakeNewAddOn(DateTimePropertyAddOn, this, out genAddOnForDateTimeProperty);
							RegisterEditableChildObject(GenAddOnForDateTimeProperty);
						}
						GenAddOnForDateTimeProperty.XA_Data = value.ToString();
					}
					else
					{
						if (GenAddOnForDateTimeProperty != null)
						{
							GenAddOnForDateTimeProperty.XA_Data = ZString.Empty;
							GenAddOnForDateTimeProperty.Delete();
							genAddOnForDateTimeProperty = null;
						}
					}
				}
			}

			public ZPropertyInfo DateTimePropertyInfo => GetZPropertyInfo(nameof(DateTimeProperty));

			GenAddOnColumn GenAddOnForDateTimeProperty
			{
				get
				{
					if (genAddOnForDateTimeProperty == null || genAddOnForDateTimeProperty.IsDeleted)
					{
						GenAddOnHelper.Find(DateTimePropertyAddOn, this, out genAddOnForDateTimeProperty);
						if (genAddOnForDateTimeProperty != null)
						{
							RegisterEditableChildObject(genAddOnForDateTimeProperty);
						}
					}
					return genAddOnForDateTimeProperty;
				}
			}
			GenAddOnColumn genAddOnForDateTimeProperty;

			public ZDecimal DecimalProperty
			{
				get
				{
					return (GenAddOnForDecimalProperty != null) ? new ZDecimal(GenAddOnForDecimalProperty.XA_Data) : ZDecimal.Zero;
				}
				set
				{
					if (!value.IsEmpty)
					{
						if (GenAddOnForDecimalProperty == null)
						{
							GenAddOnHelper.FindOrMakeNewAddOn(DecimalPropertyAddOn, this, out genAddOnForDecimalProperty);
							RegisterEditableChildObject(GenAddOnForDecimalProperty);
						}
						GenAddOnForDecimalProperty.XA_Data = value.ToString();
					}
					else
					{
						if (GenAddOnForDecimalProperty != null)
						{
							GenAddOnForDecimalProperty.XA_Data = ZString.Empty;
							GenAddOnForDecimalProperty.Delete();
							genAddOnForDecimalProperty = null;
						}
					}
				}
			}

			public ZPropertyInfo DecimalPropertyInfo => GetZPropertyInfo(nameof(DecimalProperty));

			GenAddOnColumn GenAddOnForDecimalProperty
			{
				get
				{
					if (genAddOnForDecimalProperty == null || genAddOnForDecimalProperty.IsDeleted)
					{
						GenAddOnHelper.Find(DecimalPropertyAddOn, this, out genAddOnForDecimalProperty);
						if (genAddOnForDecimalProperty != null)
						{
							RegisterEditableChildObject(genAddOnForDecimalProperty);
						}
					}
					return genAddOnForDecimalProperty;
				}
			}
			GenAddOnColumn genAddOnForDecimalProperty;

			public ZGuid GuidProperty
			{
				get
				{
					return (GenAddOnForGuidProperty != null) ? new ZGuid(GenAddOnForGuidProperty.XA_Data) : ZGuid.Empty;
				}
				set
				{
					if (!value.IsEmpty)
					{
						if (GenAddOnForGuidProperty == null)
						{
							GenAddOnHelper.FindOrMakeNewAddOn(GuidPropertyAddOn, this, out genAddOnForGuidProperty);
							RegisterEditableChildObject(GenAddOnForGuidProperty);
						}
						GenAddOnForGuidProperty.XA_Data = value.ToString();
					}
					else
					{
						if (GenAddOnForGuidProperty != null)
						{
							GenAddOnForGuidProperty.XA_Data = ZString.Empty;
							GenAddOnForGuidProperty.Delete();
							genAddOnForGuidProperty = null;
						}
					}
				}
			}

			public ZPropertyInfo GuidPropertyInfo => GetZPropertyInfo(nameof(GuidProperty));

			GenAddOnColumn GenAddOnForGuidProperty
			{
				get
				{
					if (genAddOnForGuidProperty == null || genAddOnForGuidProperty.IsDeleted)
					{
						GenAddOnHelper.Find(GuidPropertyAddOn, this, out genAddOnForGuidProperty);
						if (genAddOnForGuidProperty != null)
						{
							RegisterEditableChildObject(genAddOnForGuidProperty);
						}
					}
					return genAddOnForGuidProperty;
				}
			}
			GenAddOnColumn genAddOnForGuidProperty;

			public ZInt IntegerProperty
			{
				get
				{
					return (GenAddOnForIntegerProperty != null) ? new ZInt(GenAddOnForIntegerProperty.XA_Data) : ZInt.Zero;
				}
				set
				{
					if (!value.IsEmpty)
					{
						if (GenAddOnForIntegerProperty == null)
						{
							GenAddOnHelper.FindOrMakeNewAddOn(IntegerPropertyAddOn, this, out genAddOnForIntegerProperty);
							RegisterEditableChildObject(GenAddOnForIntegerProperty);
						}
						GenAddOnForIntegerProperty.XA_Data = value.ToString();
					}
					else
					{
						if (GenAddOnForIntegerProperty != null)
						{
							GenAddOnForIntegerProperty.XA_Data = ZString.Empty;
							GenAddOnForIntegerProperty.Delete();
							genAddOnForIntegerProperty = null;
						}
					}
				}
			}

			public ZPropertyInfo IntegerPropertyInfo => GetZPropertyInfo(nameof(IntegerProperty));

			GenAddOnColumn GenAddOnForIntegerProperty
			{
				get
				{
					if (genAddOnForIntegerProperty == null || genAddOnForIntegerProperty.IsDeleted)
					{
						GenAddOnHelper.Find(IntegerPropertyAddOn, this, out genAddOnForIntegerProperty);
						if (genAddOnForIntegerProperty != null)
						{
							RegisterEditableChildObject(genAddOnForIntegerProperty);
						}
					}
					return genAddOnForIntegerProperty;
				}
			}
			GenAddOnColumn genAddOnForIntegerProperty;

			public ZShort ShortProperty
			{
				get
				{
					return (GenAddOnForShortProperty != null) ? new ZShort(GenAddOnForShortProperty.XA_Data) : ZShort.Zero;
				}
				set
				{
					if (!value.IsEmpty)
					{
						if (GenAddOnForShortProperty == null)
						{
							GenAddOnHelper.FindOrMakeNewAddOn(ShortPropertyAddOn, this, out genAddOnForShortProperty);
							RegisterEditableChildObject(GenAddOnForShortProperty);
						}
						GenAddOnForShortProperty.XA_Data = value.ToString();
					}
					else
					{
						if (GenAddOnForShortProperty != null)
						{
							GenAddOnForShortProperty.XA_Data = ZString.Empty;
							GenAddOnForShortProperty.Delete();
							genAddOnForShortProperty = null;
						}
					}
				}
			}

			public ZPropertyInfo ShortPropertyInfo => GetZPropertyInfo(nameof(ShortProperty));

			GenAddOnColumn GenAddOnForShortProperty
			{
				get
				{
					if (genAddOnForShortProperty == null || genAddOnForShortProperty.IsDeleted)
					{
						GenAddOnHelper.Find(ShortPropertyAddOn, this, out genAddOnForShortProperty);
						if (genAddOnForShortProperty != null)
						{
							RegisterEditableChildObject(genAddOnForShortProperty);
						}
					}
					return genAddOnForShortProperty;
				}
			}
			GenAddOnColumn genAddOnForShortProperty;

			public ZByte ByteProperty
			{
				get
				{
					return (GenAddOnForByteProperty != null) ? new ZByte(GenAddOnForByteProperty.XA_Data) : ZByte.Zero;
				}
				set
				{
					if (!value.IsEmpty)
					{
						if (GenAddOnForByteProperty == null)
						{
							GenAddOnHelper.FindOrMakeNewAddOn(BytePropertyAddOn, this, out genAddOnForByteProperty);
							RegisterEditableChildObject(GenAddOnForByteProperty);
						}
						GenAddOnForByteProperty.XA_Data = value.ToString();
					}
					else
					{
						if (GenAddOnForByteProperty != null)
						{
							GenAddOnForByteProperty.XA_Data = ZString.Empty;
							GenAddOnForByteProperty.Delete();
							genAddOnForByteProperty = null;
						}
					}
				}
			}

			public ZPropertyInfo BytePropertyInfo => GetZPropertyInfo(nameof(ByteProperty));

			GenAddOnColumn GenAddOnForByteProperty
			{
				get
				{
					if (genAddOnForByteProperty == null || genAddOnForByteProperty.IsDeleted)
					{
						GenAddOnHelper.Find(BytePropertyAddOn, this, out genAddOnForByteProperty);
						if (genAddOnForByteProperty != null)
						{
							RegisterEditableChildObject(genAddOnForByteProperty);
						}
					}
					return genAddOnForByteProperty;
				}
			}
			GenAddOnColumn genAddOnForByteProperty;
		}
	}
}
