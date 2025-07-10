//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobComInvoiceHeaderLookups
//
//    This class should be used for overriding collections in AutoJobComInvoiceHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections;
using CargoWise.Common.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class JobComInvoiceHeaderLookups : AutoJobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(AutoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		#region Collection

		public virtual RefCurrencyCollection CurrencyList
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignee)]
		public ConsigneeCollection ImporterList
		{
			get
			{
				if (importerList == null)
				{
					importerList = new ConsigneeCollection(Factory);
				}

				return importerList;
			}
		}
		ConsigneeCollection importerList;

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignor)]
		public virtual ConsignorCollection SupplierList
		{
			get
			{
				if (supplierList == null)
				{
					supplierList = new ConsignorCollection(Factory);
				}
				return supplierList;
			}
		}
		ConsignorCollection supplierList;

		public GlbBranchCollection BranchList
		{
			get { return new GlbBranchCollection(Factory); }
		}

		public GroupHeaderCollection JZ_JZ_GroupInvoiceFK_List
		{
			get
			{
				if (fJZ_JZ_GroupInvoiceFK_List == null)
				{
					fJZ_JZ_GroupInvoiceFK_List = GetNewGroupHeaderCollection();
					fJZ_JZ_GroupInvoiceFK_List.Load();
				}
				return fJZ_JZ_GroupInvoiceFK_List;
			}
		}
		GroupHeaderCollection fJZ_JZ_GroupInvoiceFK_List;

		protected virtual GroupHeaderCollection GetNewGroupHeaderCollection()
		{
			return new GroupHeaderCollection(Invoice);
		}

		public virtual OrgHeaderCollection ShipToParties
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public virtual OrgHeaderCollection SoldToParties
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public OrgHeaderCollection SellerConsignors
		{
			get { return new ConsignorCollection(Factory); }
		}

		public virtual OrgHeaderCollection Distributors
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public virtual OrgHeaderCollection Packagers
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public virtual OrgHeaderCollection Shippers
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public virtual OrgHeaderCollection Exporters
		{
			get { return SupplierList; }
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList Bills
		{
			get { return Invoice.JobDeclaration != null ? Invoice.JobDeclaration.Bills : new CodeDescriptionPairList(); }
		}

		#endregion

		#region List

		public virtual CodeDescriptionPairList MessageStatusList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList MessageTypes
		{
			get { return JobMessageTypeList.GetListWithAdvanceShippingNotice(Factory); }
		}

		public virtual CodeDescriptionPairList JZ_IncoTerm_List
		{
			get { return Factory.GetCachedValue("InvoiceHeader|IncoTermList", () => new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms)); }
		}

		public CodeDescriptionPairList JZ_VolumeUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public CodeDescriptionPairList JZ_WeightUQ_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList NoOfPacksPackType_List
		{
			get { return Invoice.JobDeclaration != null ? Invoice.JobDeclaration.Lookups.JE_TotalNoOfPacksPackType_List : new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList ExchangeRateTypeList
		{
			get { return Factory.GetCachedValue<ChargeExchangeRateTypeList>(); }
		}

		public virtual ICodeDescriptionPairList ValuationCodeList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public CodeDescriptionPairList PaymentMethodList => PaymentMethodListCore;

		protected virtual CodeDescriptionPairList PaymentMethodListCore => new CodeDescriptionPairList();

		public virtual ICodeDescriptionPairList RelatedIndicatorList => Factory.GetCachedValue<RelatedIndicatorList>();

		#endregion

		#region Implementation

		protected BaseJobComInvoiceHeader Invoice
		{
			get { return (BaseJobComInvoiceHeader)Parent; }
		}

		#endregion
	}
}
