using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using IBaseJobComInvoiceLine = Enterprise.Integration.Customs.IBaseJobComInvoiceLine;

namespace Enterprise.Customs.Universal
{
	public class CusRulingConfigCombinedCollection : DependentBusinessObjectCollection<CusRulingConfigCombined, BusinessObject>
	{
		public CusRulingConfigCombinedCollection(ZZRefCusRulingCombined cusRuling)
			: this(cusRuling, CusRulingConfigCombinedSchema.ZZY_ZZX_CusRuling, !cusRuling.IsSystem)
		{
		}

		public CusRulingConfigCombinedCollection(IBaseJobComInvoiceLine invoiceLine)
			: this((BusinessObject)invoiceLine, CusRulingConfigCombinedSchema.ZZY_JI_InvoiceLine, true)
		{
		}

		CusRulingConfigCombinedCollection(BusinessObject parent, SchemaGuidColumn fkSchemaColumn, bool allowNew)
			: base(parent)
		{
			this.fkSchemaColumn = fkSchemaColumn;
			this.allowNew = allowNew;
		}

		readonly SchemaGuidColumn fkSchemaColumn;
		readonly bool allowNew;

		public CusRulingConfigCombined AddNew(ZString category, ZString type, ZDecimal rate, ZString value)
		{
			var result = AddNew();
			result.ZZY_Category = category;
			result.ZZY_Type = type;
			result.ZZY_Rate = rate;
			result.ZZY_Value = value;

			return result;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => fkSchemaColumn;

		protected override bool AllowNewCore => allowNew;
	}
}
