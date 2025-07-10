// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEntryInstructionLookups
//
//    This class should be used for overriding collections in AutoCusEntryInstructionLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusEntryInstructionLookups : AutoCusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(AutoCusEntryInstruction parent)
			: base(parent)
		{
		}

		protected BaseJobDeclaration JobDeclaration => (Parent as CusEntryInstruction)?.JobDeclaration;

		public virtual CodeDescriptionPairList StyleList
		{
			get
			{
				var dataGrouping = JobDeclaration?.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure) ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var shipmentType = GetShipmentType();
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_RefCusProcedures_ProcedureCode", dataGrouping, shipmentType, ZDateTime.Today);
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var result = new CodeDescriptionPairList();
					var procedures = RefCusProcedureCollection.LoadCustomsProcedureCodesForCountryAndShipmentType(Factory, dataGrouping, shipmentType, ZDateTime.Today);
					foreach (var procedure in procedures)
					{
						result.AddPair(procedure.ZZ6_ProcedureCode, procedure.ZZ6_Description);
					}
					result.Sort();
					return result;
				});
			}
		}

		protected virtual ZString GetShipmentType() => JobDeclaration?.JE_MessageType ?? ZString.Empty;

		public virtual CodeDescriptionPairList MergeByList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.CommercialInvoiceMergeMethod); }
		}

		public virtual CodeDescriptionPairList EntrySubStyleList => new CodeDescriptionPairList();

		public BondedWarehouseCollection BondedWarehouseCollection => new BondedWarehouseCollection(Factory);

		public virtual OrgHeaderCollection Organisations => new OrgHeaderCollection(Factory);

		protected ShippingProviderCollection fCarrierOrganisations;
		public ShippingProviderCollection CarrierOrganisations
		{
			get
			{
				if (fCarrierOrganisations == null)
				{
					fCarrierOrganisations = new ShippingProviderCollection(Factory);
				}
				return fCarrierOrganisations;
			}
		}
	}
}
