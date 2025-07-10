using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	class SystemDefinedPropertyManager : AbstractPropertyManager<GenAddOnColumn>
	{
		internal SystemDefinedPropertyManager(BusinessObject businessObject)
			: base(businessObject)
		{
		}

		protected override CargoWise.Schema.SchemaColumn Data
		{
			get { return GenAddOnColumnSchema.XA_Data; }
		}

		protected override CargoWise.Schema.SchemaColumn Name
		{
			get { return GenAddOnColumnSchema.XA_Name; }
		}

		protected override CargoWise.Schema.SchemaColumn ParentID
		{
			get { return GenAddOnColumnSchema.XA_ParentID; }
		}

		protected override CargoWise.Schema.SchemaColumn ParentTableCode
		{
			get { return GenAddOnColumnSchema.XA_ParentTableCode; }
		}

		protected override CargoWise.Schema.SchemaColumn DataType
		{
			get { return GenAddOnColumnSchema.XA_Type; }
		}

		protected override Type AttributeType
		{
			get { return typeof(SystemDefinedValuesAttribute); }
		}
	}
}
