using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Rating;

public abstract class MappedChargeCodeBizo(BusinessObjectFactory factory) : NonPersistentBusinessObject(factory, GetNewRow(factory))
{
	public abstract class Schema
	{
		public const string TableName = MappedChargeCodeSchema.Constants.TableName;
		public const string PK = MappedChargeCodeSchema.Constants.PK;
		public const string UCC_Code = MappedChargeCodeSchema.Constants.UCC_Code;
		public const string UCC_Description = MappedChargeCodeSchema.Constants.UCC_Description;
		public const string UCC_GlobalChargeCode = MappedChargeCodeSchema.Constants.UCC_GlobalChargeCode;
		public const string UCC_GlobalChargeCodeDescription = MappedChargeCodeSchema.Constants.UCC_GlobalChargeCodeDescription;
		public const string UCC_LocalChargeCode = MappedChargeCodeSchema.Constants.UCC_LocalChargeCode;
		public const string UCC_LocalChargeCodeDescription = MappedChargeCodeSchema.Constants.UCC_LocalChargeCodeDescription;
		public const string UCC_RateProvider = MappedChargeCodeSchema.Constants.UCC_RateProvider;
		public const string UCC_Carrier = MappedChargeCodeSchema.Constants.UCC_Carrier;
		public const string UCC_ForeignCode = MappedChargeCodeSchema.Constants.UCC_ForeignCode;
		public const string UCC_ForeignName = MappedChargeCodeSchema.Constants.UCC_ForeignName;

		public const int UCC_CodeMaxLength = AutoAccChargeCodeUniversalCodeMapping.Schema.AUP_CodeMaxLength;
		public const int UCC_GlobalChargeCodeMaxLength = AutoAccChargeCode.Schema.AC_CodeMaxLength;
		public const int UCC_LocalChargeCodeMaxLength = AutoAccChargeCode.Schema.AC_CodeMaxLength;

		// These Max Lengths refer to RatesService.WiseRates.Data.Model.CodeMapping.cs schema properties.
		public const int UCC_ProviderMaxLength = 4;
		public const int UCC_CarrierMaxLength = 4;
		public const int UCC_ForeignCodeMaxLength = 40; // This is eventually saved in AUP_Code, however the only constraint is that AUP_Code has to be at least bigger than this, but it can be bigger if it is used for something else in the future.
	}

	#region Validation

	protected virtual MappedChargeCodeBizoValidation Validation => new (this);

	protected override void RunPreSaveValidationCore()
	{
		Validation.ValidateAll();
		base.RunPreSaveValidationCore();
	}

	#endregion

	[MaxLength(Schema.UCC_CodeMaxLength)]
	public ZString UCC_Code
	{
		get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(UCC_CodeInfo)); }
		set
		{
			value = value.TrimEnd(' ');
			CheckMaximumLength(UCC_CodeInfo, value);
			SetPropertyValue(UCC_CodeInfo, value);
		}
	}

	public ZPropertyInfo UCC_CodeInfo => GetZPropertyInfo(Schema.UCC_Code);

	public ZString UCC_Description
	{
		get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(UCC_DescriptionInfo)); }
		set
		{
			value = value.TrimEnd(' ');
			CheckMaximumLength(UCC_DescriptionInfo, value);
			SetPropertyValue(UCC_DescriptionInfo, value);
		}
	}

	public ZPropertyInfo UCC_DescriptionInfo => GetZPropertyInfo(Schema.UCC_Description);

	[MaxLength(Schema.UCC_GlobalChargeCodeMaxLength)]
	public ZString UCC_GlobalChargeCode
	{
		get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(UCC_GlobalChargeCodeInfo)); }
		set
		{
			value = value.TrimEnd(' ');
			CheckMaximumLength(UCC_GlobalChargeCodeInfo, value);
			SetPropertyValue(UCC_GlobalChargeCodeInfo, value);
		}
	}

	public ZPropertyInfo UCC_GlobalChargeCodeInfo => GetZPropertyInfo(Schema.UCC_GlobalChargeCode);

	public ZString UCC_GlobalChargeCodeDescription
	{
		get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(UCC_GlobalChargeCodeDescriptionInfo)); }
		set
		{
			value = value.TrimEnd(' ');
			CheckMaximumLength(UCC_GlobalChargeCodeDescriptionInfo, value);
			SetPropertyValue(UCC_GlobalChargeCodeDescriptionInfo, value);
		}
	}

	public ZPropertyInfo UCC_GlobalChargeCodeDescriptionInfo => GetZPropertyInfo(Schema.UCC_GlobalChargeCodeDescription);

	[MaxLength(Schema.UCC_LocalChargeCodeMaxLength)]
	public ZString UCC_LocalChargeCode
	{
		get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(UCC_LocalChargeCodeInfo)); }
		set
		{
			value = value.TrimEnd(' ');
			CheckMaximumLength(UCC_LocalChargeCodeInfo, value);
			SetPropertyValue(UCC_LocalChargeCodeInfo, value);
		}
	}

	public ZPropertyInfo UCC_LocalChargeCodeInfo => GetZPropertyInfo(Schema.UCC_LocalChargeCode);

	public ZString UCC_LocalChargeCodeDescription
	{
		get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(UCC_LocalChargeCodeDescriptionInfo)); }
		set
		{
			value = value.TrimEnd(' ');
			CheckMaximumLength(UCC_LocalChargeCodeDescriptionInfo, value);
			SetPropertyValue(UCC_LocalChargeCodeDescriptionInfo, value);
		}
	}

	public ZPropertyInfo UCC_LocalChargeCodeDescriptionInfo => GetZPropertyInfo(Schema.UCC_LocalChargeCodeDescription);

	[MaxLength(Schema.UCC_ProviderMaxLength)]
	public ZString UCC_RateProvider
	{
		get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(UCC_RateProviderInfo)); }
		set
		{
			value = value.TrimEnd(' ');
			CheckMaximumLength(UCC_RateProviderInfo, value);
			SetPropertyValue(UCC_RateProviderInfo, value);
		}
	}

	public ZPropertyInfo UCC_RateProviderInfo => GetZPropertyInfo(Schema.UCC_RateProvider);

	[MaxLength(Schema.UCC_CarrierMaxLength)]
	public ZString UCC_Carrier
	{
		get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(UCC_CarrierInfo)); }
		set
		{
			value = value.TrimEnd(' ');
			CheckMaximumLength(UCC_CarrierInfo, value);
			SetPropertyValue(UCC_CarrierInfo, value);
		}
	}

	public ZPropertyInfo UCC_CarrierInfo => GetZPropertyInfo(Schema.UCC_Carrier);

	[MaxLength(Schema.UCC_ForeignCodeMaxLength)]
	public ZString UCC_ForeignCode
	{
		get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(UCC_ForeignCodeInfo)); }
		set
		{
			value = value.TrimEnd(' ');
			CheckMaximumLength(UCC_ForeignCodeInfo, value);
			SetPropertyValue(UCC_ForeignCodeInfo, value);
		}
	}

	public ZPropertyInfo UCC_ForeignCodeInfo => GetZPropertyInfo(Schema.UCC_ForeignCode);

	public ZString UCC_ForeignName
	{
		get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(UCC_ForeignNameInfo)); }
		set
		{
			value = value.TrimEnd(' ');
			CheckMaximumLength(UCC_ForeignNameInfo, value);
			SetPropertyValue(UCC_ForeignNameInfo, value);
		}
	}

	public ZPropertyInfo UCC_ForeignNameInfo => GetZPropertyInfo(Schema.UCC_ForeignName);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		DataRow row = ((IBusinessObjectInternals)this).Row;
		row[Schema.UCC_Code] = "";
		row[Schema.UCC_Description] = "";
		row[Schema.UCC_GlobalChargeCode] = "";
		row[Schema.UCC_GlobalChargeCodeDescription] = "";
		row[Schema.UCC_LocalChargeCode] = "";
		row[Schema.UCC_LocalChargeCodeDescription] = "";
		row[Schema.UCC_RateProvider] = "";
		row[Schema.UCC_Carrier] = "";
		row[Schema.UCC_ForeignCode] = "";
		row[Schema.UCC_ForeignName] = "";
	}

	public override SchemaGuidColumn PKSchemaColumn
	{
		get { return new SchemaGuidColumn(CargoWise.Schema.Schema.GenericTableSchema, Schema.PK, 0, Guid.Empty, false); }
	}

	static DataRow GetNewRow(BusinessObjectFactory factory)
	{
		DataTable table = ((INeedDataSet)factory).Data.Tables[Schema.TableName];
		if (table == null)
		{
			table = new NonPersistentDataTable();
			((INeedDataSet)factory).Data.Tables.Add(table);
		}
		return table.NewRow();
	}

	class NonPersistentDataTable : ZDataTable
	{
		public NonPersistentDataTable()
			: base(Schema.TableName)
		{
			DataColumn column;
			column = Columns.Add(Schema.PK, typeof(Guid));
			column.AllowDBNull = false;

			column = Columns.Add(Schema.UCC_Code, typeof(string));
			column.MaxLength = Schema.UCC_CodeMaxLength;
			column.AllowDBNull = false;

			column = Columns.Add(Schema.UCC_Description, typeof(string));
			column.AllowDBNull = false;

			column = Columns.Add(Schema.UCC_GlobalChargeCode, typeof(string));
			column.MaxLength = Schema.UCC_GlobalChargeCodeMaxLength;
			column.AllowDBNull = false;

			column = Columns.Add(Schema.UCC_GlobalChargeCodeDescription, typeof(string));
			column.AllowDBNull = false;

			column = Columns.Add(Schema.UCC_LocalChargeCode, typeof(string));
			column.MaxLength = Schema.UCC_LocalChargeCodeMaxLength;
			column.AllowDBNull = false;

			column = Columns.Add(Schema.UCC_LocalChargeCodeDescription, typeof(string));
			column.AllowDBNull = false;

			column = Columns.Add(Schema.UCC_RateProvider, typeof(string));
			column.MaxLength = Schema.UCC_ProviderMaxLength;
			column.AllowDBNull = false;

			column = Columns.Add(Schema.UCC_Carrier, typeof(string));
			column.MaxLength = Schema.UCC_CarrierMaxLength;
			column.AllowDBNull = false;

			column = Columns.Add(Schema.UCC_ForeignCode, typeof(string));
			column.AllowDBNull = false;

			column = Columns.Add(Schema.UCC_ForeignName, typeof(string));
			column.AllowDBNull = false;
		}
	}
}
