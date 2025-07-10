using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using OrgSupplierPartCollection = Enterprise.MasterFiles.Business.OrgSupplierPartCollection;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	/// <summary>
	/// Lookups used by the <see cref="GlobalCommercialInvoiceLine"/> class.
	/// </summary>
	/// <param name="parent">Parent line.</param>
	public class GlobalCommercialInvoiceLineLookups(AutoGlobalCommercialInvoiceLine parent)
		: AutoGlobalCommercialInvoiceLineLookups(parent)
	{
		public GlobalCommercialInvoiceLine InvoiceLine => (GlobalCommercialInvoiceLine)Parent;

		CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public CodeDescriptionPairList NetWeightUQList => WeightUQList;

		public CodeDescriptionPairList GrossWeightUQList => WeightUQList;

		public CodeDescriptionPairList VolumeUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume);

		public CodeDescriptionPairList InvoiceUQList => RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory);

		public ZZRefCusCodeListCombinedCollection TariffCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSCNCodes, ZDateTime.Today);

		public OrgSupplierPartCollection ProductCodeList
		{
			get
			{
				var result = new OrgSupplierPartCollection(Factory);
				if (!InvoiceLine.GIL_Product.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Product Code", "Property", InvoiceLine.GIL_Product));
				}
				return result;
			}
		}
	}
}
