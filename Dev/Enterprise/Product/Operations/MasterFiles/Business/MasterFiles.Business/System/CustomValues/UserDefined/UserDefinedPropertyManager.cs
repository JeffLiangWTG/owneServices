using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	class UserDefinedPropertyManager : AbstractPropertyManager<GenCustomAddOnValue>
	{
		internal UserDefinedPropertyManager(BusinessObject businessObject)
			: base(businessObject)
		{
		}

		protected override CargoWise.Schema.SchemaColumn Data
		{
			get { return GenCustomAddOnValueSchema.XV_Data; }
		}

		protected override CargoWise.Schema.SchemaColumn Name
		{
			get { return GenCustomAddOnValueSchema.XV_Name; }
		}

		protected override CargoWise.Schema.SchemaColumn ParentID
		{
			get { return GenCustomAddOnValueSchema.XV_ParentID; }
		}

		protected override CargoWise.Schema.SchemaColumn ParentTableCode
		{
			get { return GenCustomAddOnValueSchema.XV_ParentTableCode; }
		}

		protected override CargoWise.Schema.SchemaColumn DataType
		{
			get { return GenCustomAddOnValueSchema.XV_Type; }
		}

		protected override Type AttributeType
		{
			get { return typeof(UserDefinedValuesAttribute); }
		}

		protected override bool NameIsMandatory { get { return true; } }
		protected override bool DataTypeIsMandatory { get { return true; } }
	}
}
