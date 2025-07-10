using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Rating
{
	[CodeProperty(UniversalCommodityCodeBizo.Schema.RH_UniversalCommodityGroup)]
	[DescriptionProperty(UniversalCommodityCodeBizo.Schema.RH_UniversalCommodityGroupDescription)]
	public class UniversalCommodityCodeBizo : NonPersistentBusinessObject
	{
		public UniversalCommodityCodeBizo(BusinessObjectFactory factory)
			: base(factory, GetNewRow(factory))
		{
		}

		public abstract class Schema
		{
			public const string TableName = UniversalCommodityCodeSchema.Constants.TableName;
			public const string PK = UniversalCommodityCodeSchema.Constants.PK;
			public const string RH_UniversalCommodityGroup = UniversalCommodityCodeSchema.Constants.RH_UniversalCommodityGroup;
			public const string RH_UniversalCommodityGroupDescription = UniversalCommodityCodeSchema.Constants.RH_UniversalCommodityGroupDescription;
			public const string RH_Code = UniversalCommodityCodeSchema.Constants.RH_Code;
			public const string RH_Description = UniversalCommodityCodeSchema.Constants.RH_Description;

			public const int UniversalGroupMaxLength = RefCommodityCode.Schema.RH_UniversalCommodityGroupMaxLength;
			public const int UniversalGroupDescriptionMaxLength = 2147483647;
			public const int CommodityCodeMaxLength = RefCommodityCode.Schema.RH_CodeMaxLength;
			public const int CommodityDescriptionMaxLength = RefCommodityCode.Schema.RH_DescriptionMaxLength;
		}

		public ZString RH_UniversalCommodityGroup
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(UniversalGroupInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(UniversalGroupInfo, value);
				SetPropertyValue(UniversalGroupInfo, value);
			}
		}

		public ZPropertyInfo UniversalGroupInfo => GetZPropertyInfo(UniversalCommodityCodeBizo.Schema.RH_UniversalCommodityGroup);

		public ZString RH_UniversalCommodityGroupDescription
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(UniversalGroupDescriptionInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(UniversalGroupDescriptionInfo, value);
				SetPropertyValue(UniversalGroupDescriptionInfo, value);
			}
		}

		public ZPropertyInfo UniversalGroupDescriptionInfo => GetZPropertyInfo(UniversalCommodityCodeBizo.Schema.RH_UniversalCommodityGroupDescription);

		public ZString RH_Code
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(CommodityCodeInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(CommodityCodeInfo, value);
				SetPropertyValue(CommodityCodeInfo, value);
			}
		}

		public ZPropertyInfo CommodityCodeInfo => GetZPropertyInfo(UniversalCommodityCodeBizo.Schema.RH_Code);

		public ZString RH_Description
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(CommodityDescriptionInfo)); }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(CommodityDescriptionInfo, value);
				SetPropertyValue(CommodityDescriptionInfo, value);
			}
		}

		public ZPropertyInfo CommodityDescriptionInfo => GetZPropertyInfo(UniversalCommodityCodeBizo.Schema.RH_Description);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			DataRow row = ((IBusinessObjectInternals)this).Row;
			row[UniversalCommodityCodeBizo.Schema.RH_UniversalCommodityGroup] = "";
			row[UniversalCommodityCodeBizo.Schema.RH_UniversalCommodityGroupDescription] = "";
			row[UniversalCommodityCodeBizo.Schema.RH_Code] = "";
			row[UniversalCommodityCodeBizo.Schema.RH_Description] = "";
		}

		public override SchemaGuidColumn PKSchemaColumn
		{
			get { return new SchemaGuidColumn(CargoWise.Schema.Schema.GenericTableSchema, UniversalCommodityCodeBizo.Schema.PK, 0, Guid.Empty, false); }
		}

		static DataRow GetNewRow(BusinessObjectFactory factory)
		{
			DataTable table = ((INeedDataSet)factory).Data.Tables[UniversalCommodityCodeBizo.Schema.TableName];
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
				: base(UniversalCommodityCodeBizo.Schema.TableName)
			{
				DataColumn column;
				column = Columns.Add(UniversalCommodityCodeBizo.Schema.PK, typeof(Guid));
				column.AllowDBNull = false;

				column = Columns.Add(UniversalCommodityCodeBizo.Schema.RH_UniversalCommodityGroup, typeof(System.String));
				column.MaxLength = UniversalCommodityCodeBizo.Schema.UniversalGroupMaxLength;
				column.AllowDBNull = false;

				column = Columns.Add(UniversalCommodityCodeBizo.Schema.RH_UniversalCommodityGroupDescription, typeof(System.String));
				column.MaxLength = UniversalCommodityCodeBizo.Schema.UniversalGroupDescriptionMaxLength;
				column.AllowDBNull = false;

				column = Columns.Add(UniversalCommodityCodeBizo.Schema.RH_Code, typeof(System.String));
				column.MaxLength = UniversalCommodityCodeBizo.Schema.CommodityCodeMaxLength;
				column.AllowDBNull = false;

				column = Columns.Add(UniversalCommodityCodeBizo.Schema.RH_Description, typeof(System.String));
				column.MaxLength = UniversalCommodityCodeBizo.Schema.CommodityDescriptionMaxLength;
				column.AllowDBNull = false;
			}
		}
	}
}