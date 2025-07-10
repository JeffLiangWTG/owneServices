using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ICommonInvoiceDataProvider : IInvoicesProviderValueChangedAnnouncerProvider
	{
		ICustomsFileParent CustomsFileParent { get; }
		bool IsDeclarationIntegrated { get; }
		bool ApportionmentDirty { get; set; }
		ZBool IsExWarehouse { get; }
		ZBool IsImport { get; }
		ZBool IsExport { get; }
		ZString CustomsVATTypeCaption { get; }
		bool CopyLastLineDetailsToNewLines { get; set; }
		IEnumerable<BaseJobComInvoiceLine> FilteredInvoiceLines { get; }
		IEnumerable<BaseJobComInvoiceHeader> Invoices { get; }
		ZString JE_MessageType { get; set; }
		ZPropertyInfo JE_MessageTypeInfo { get; }
		ZBool JE_CopyLastInvoiceLineDetailsToNewLines { get; }
		WeightApportionManager WeightApportionManager { get; }
		ZBool JE_AutoWeightApportion { get; set; }
		IDisposable SuspendWeightApportionment();
		bool WeightApportionmentEnabled { get; }
		void ApportionWeightIfNeeded(IWeightHolder weightHolder, IWeightApportionee uncommittedApportionee);
		void ExpandOneBOMProductLine(BaseJobComInvoiceLine invoiceLine);
		void CollapseOneBOMProductLine(BaseJobComInvoiceLine invoiceLine);
	}
}
