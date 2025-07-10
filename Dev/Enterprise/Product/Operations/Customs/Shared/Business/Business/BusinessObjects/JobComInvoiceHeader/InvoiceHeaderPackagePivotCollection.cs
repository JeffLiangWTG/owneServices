using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class InvoiceHeaderPackagePivotCollection : BasePackagePivotCollection<InvoiceHeaderPackagePivot, BaseJobComInvoiceHeader>
	{
		public InvoiceHeaderPackagePivotCollection(BaseJobComInvoiceHeader header)
			: base(header)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusHouseContPackInvoiceHeaderPivotSchema.CHZ_JZ;

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(InvoiceHeaderPackagePivot);

		protected override IDependentBusinessObjectCollection GetPivotCollection(BasePackage package)
		{
			return package.IsPivotCollectionLoaded(PivotLevel.Invoice) ? package.InvoiceHeaderPivotCollection : null;
		}
	}
}
