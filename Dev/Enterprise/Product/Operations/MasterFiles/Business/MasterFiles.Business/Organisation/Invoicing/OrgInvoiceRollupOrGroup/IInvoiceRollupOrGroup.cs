using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface IOrgInvoiceType
	{
		ZGuid PK { get; }
		ZString JobType { get; set; }
		ZPropertyInfo JobTypeInfo { get; }
		ZString ServiceDirection { get; set; }
		ZPropertyInfo ServiceDirectionInfo { get; }
		ZString TransportMode { get; set; }
		ZPropertyInfo TransportModeInfo { get; }
		ZString ServiceLevel { get; set; }
		ZPropertyInfo ServiceLevelInfo { get; }
		CodeDescriptionPairList JobTypeList { get; }
		CodeDescriptionPairList TransportModeList { get; }
		CodeDescriptionPairList ServiceDirectionList { get; }
		CodeDescriptionPairList ServiceLevelList { get; }
		BusinessObjectFactory Factory { get; }
		BusinessObjectCollection ParentCollection { get; }
		string DuplicateRowErrorMessage { get; }
	}

	public interface IInvoiceRollupOrGroup : IOrgInvoiceType
	{
		ZString GroupOrSubTotal { get; set; }
		ZPropertyInfo GroupOrSubTotalInfo { get; }
		ZString GroupOrSubtotalStyle { get; set; }
		ZPropertyInfo GroupOrSubtotalStyleInfo { get; }
		ZString InvoiceLineDisplayOption { get; set; }
		ZPropertyInfo InvoiceLineDisplayOptionInfo { get; }
		ZString InvoicePostingStyle { get; set; }
		ZPropertyInfo InvoicePostingStyleInfo { get; }
		ZString InvoicePostingCurrency { get; set; }
		ZPropertyInfo InvoicePostingCurrencyInfo { get; }
		CodeDescriptionPairList InvoicePostingOptionsList { get; }
		CodeDescriptionPairList InvoiceLineDisplayOptionsList { get; }
		CodeDescriptionPairList GroupOrSubTotalList { get; }
		CodeDescriptionPairList GroupOrSubTotalStyleList { get; }
		RefCurrencyCollection InvoicePostingCurrencies { get; }
	}
}
