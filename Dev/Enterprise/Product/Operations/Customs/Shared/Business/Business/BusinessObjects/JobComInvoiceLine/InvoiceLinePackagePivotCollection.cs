using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class InvoiceLinePackagePivotCollection : BasePackagePivotCollection<InvoiceLinePackagePivot, BaseJobComInvoiceLine>
	{
		public InvoiceLinePackagePivotCollection(BaseJobComInvoiceLine line)
			: base(line)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusHouseContPackInvoiceLinePivotSchema.CHC_JI;

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(InvoiceLinePackagePivot);

		public ZString MergeKey
		{
			get
			{
				if (!mergeKeyCached.HasValue)
				{
					mergeKeyCached = string.Join("|", this.Cast<InvoiceLinePackagePivot>().Select(x => x.Package).Where(x => x != null && !x.IsDeleted).OrderBy(x => x.CW_SystemCreateTimeUtc).ThenBy(x => x.PK).Select(x => x.PK.ToStringKey()));
				}
				return mergeKeyCached.Value;
			}
		}
		ZString? mergeKeyCached;
		protected override IDependentBusinessObjectCollection GetPivotCollection(BasePackage package)
		{
			return package.IsPivotCollectionLoaded(PivotLevel.InvoiceLine) ? package.InvoiceLinePivotCollection : null;
		}

		internal void RefreshMergeKey()
		{
			mergeKeyCached = null;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			RefreshMergeKey();
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			RefreshMergeKey();
		}
	}
}
