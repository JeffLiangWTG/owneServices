using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class JobComInvoiceLineReservedField : ReservedField
	{
		public JobComInvoiceLineReservedField(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceLine));

		public override ZString CY_Code
		{
			get => base.CY_Code;
			set
			{
				var oldValue = CY_Code;
				base.CY_Code = value;
				if (Parent is JobComInvoiceLine invoiceLine && value != oldValue && !IsCopying)
				{
					var reservedFields = invoiceLine.ReservedFields;
					if (reservedFields.Count > 0 && reservedFields[0].PK == PK)
					{
						invoiceLine.ReservedFieldCode1Info.RefreshBinding();
					}
					else if (reservedFields.Count > 1 && reservedFields[1].PK == PK)
					{
						invoiceLine.ReservedFieldCode2Info.RefreshBinding();
					}
				}
			}
		}

		public override ZString CY_Data
		{
			get => base.CY_Data;
			set
			{
				var oldValue = CY_Data;
				base.CY_Data = value;
				if (Parent is JobComInvoiceLine invoiceLine && value != oldValue && !IsCopying)
				{
					var reservedFields = invoiceLine.ReservedFields;
					if (reservedFields.Count > 0 && reservedFields[0].PK == PK)
					{
						invoiceLine.ReservedFieldValue1Info.RefreshBinding();
					}
					else if (reservedFields.Count > 1 && reservedFields[1].PK == PK)
					{
						invoiceLine.ReservedFieldValue2Info.RefreshBinding();
					}
				}
			}
		}
	}
}
