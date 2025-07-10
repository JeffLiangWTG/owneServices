using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class AddInfoJobComInvoiceLineLookups : EU.Business.Declaration.AddInfoJobComInvoiceLineLookups
	{
		public AddInfoJobComInvoiceLineLookups(AddInfoJobComInvoiceLine parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CustomsUQList => InvoiceLine.Lookups.CustomsUQList;

		public CodeDescriptionPairList UsedGoodsCodeList => Factory.GetCachedValue<UsedGoodsCodeList>();

		public ZZRefCusCodeListCombinedCollection ReturningGoodsReasonCodeList
		{
			get
			{
				var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyReturningGoodsReason, InvoiceLine.EffectiveAssessmentDate);
				if (!collection.IsLoaded)
				{
					collection.Load();
					collection.Sort(RefCusCodeListSchema.Constants.ZZD_Code);
				}
				return collection;
			}
		}

		public ZZRefCusCodeListCombinedCollection ExportUnionPackageCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyExportUnionPackageCodes, InvoiceLine.EffectiveAssessmentDate);

		public ZZRefCusCodeListCombinedCollection ThreadCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TurkeyThreadCodes, InvoiceLine.EffectiveAssessmentDate);

		public TariffViewCollection ExportUnionAdditionalTariffList => TariffViewCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Turkey, Constants.TariffTypes.ExportUnionAdditional, InvoiceLine.EffectiveAssessmentDate);

		public CodeDescriptionPairList EntryExitPurposeCodeList => Factory.GetCachedValue<EntryExitPurposeCodeList>();

		public CodeDescriptionPairList InvoicePaymentCodeList => Factory.GetCachedValue<InvoicePaymentCodeList>();

		public CodeDescriptionPairList PriceTypeList => Factory.GetCachedValue<PriceTypeList>();

		new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;

		JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Parent.Parent;
	}
}
