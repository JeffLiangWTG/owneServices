//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgARTermsLookups
//
//    This class should be used for overriding collections in AutoOrgARTermsLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgARTermsLookups : AutoOrgARTermsLookups
	{
		public OrgARTermsLookups(AutoOrgARTerms parent)
				: base(parent)
		{
		}

		OrgARTerms ParentOrgARTerms
		{
			get { return (OrgARTerms)Parent; }
		}

		#region InvoiceTypeList

		public CodeDescriptionPairList InvoiceTypeList
		{
			get
			{
				CodeDescriptionPairList invoiceTypeList = new CodeDescriptionPairList();
				invoiceTypeList.AddRange(ParentOrgARTerms.JobTypeDirectionAndTransportListProvider.InvoiceTypeList);
				AddAdditionalInvoiceTypes(invoiceTypeList);
				return invoiceTypeList;
			}
		}

		void AddAdditionalInvoiceTypes(CodeDescriptionPairList invoiceTypeList)
		{
			invoiceTypeList.Insert(0, InvoiceTypes.All);
			invoiceTypeList.Insert(1, InvoiceTypes.DSB);
		}

		public static class InvoiceTypes
		{
			public static CodeDescriptionPair All
			{
				get { return new CodeDescriptionPair("ALL", ResString.GetMultilingualString("8a86c2a4-50bf-4e2b-a392-dd3c62091107", "Any Invoice Type")); }
			}

			public static CodeDescriptionPair DSB
			{
				get { return new CodeDescriptionPair("DSB", ResString.GetMultilingualString("a6a575fb-45ef-4f85-9c6b-f2a23a6d00c0", "Any Disbursement Type")); }
			}
		}

		#endregion

		public ReadOnlyCodeDescriptionPairList AgreedPaymentMethodList
		{
			get
			{
				return OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetCodeDescriptionPairList();
			}
		}

		public CodeDescriptionPairList InvoiceTermList
		{
			get
			{
				CachedProperty<CodeDescriptionPairList> invoiceTermList_cachedValue = new CachedProperty<CodeDescriptionPairList>(Factory, delegate
						{
							CodeDescriptionPairList invoiceTermsList = new CodeDescriptionPairList(InvoiceTermListWithoutDefaultValue);
							string settlementGroupTerm = string.Empty;
							if (ParentOrgARTerms.CompanyData != null && ParentOrgARTerms.CompanyData.Header != null && ParentOrgARTerms.Validation.CanDefaultARTermBeSet)
							{
								string arTermAsString = ParentOrgARTerms.CompanyData.Header.ARSettlementGroup.CompanyData.GetARTermWithoutFallback(ParentOrgARTerms.JobTypeDirectionAndTransportListProvider.GetJobTypeDetail(), ParentOrgARTerms.PY_Direction, ParentOrgARTerms.PY_TransportMode, ParentOrgARTerms.PY_GB_Branch, ParentOrgARTerms.PY_GE_Department, ParentOrgARTerms.PY_InvoiceClass).ToString();
								if (!string.IsNullOrEmpty(arTermAsString))
								{
									settlementGroupTerm = string.Format(" ({0})", arTermAsString);
								}
							}
							invoiceTermsList.Insert(0, new CodeDescriptionPair(DefaultInvoiceTerm.Code, DefaultInvoiceTerm.Description + settlementGroupTerm));
							return invoiceTermsList;
						});

				return invoiceTermList_cachedValue.Value;
			}
		}

		public CodeDescriptionPairList InvoiceTermListNonCompanySpecific
		{
			get
			{
				var result = InvoiceTermListWithoutDefaultValue;
				result.Insert(0, DefaultInvoiceTerm);
				return result;
			}
		}

		public CodeDescriptionPairList InvoiceTermListWithoutDefaultValue
		{
			get { return new ARInvoiceTermsList(); }
		}

		public CodeDescriptionPairList JobTypeList
		{
			get
			{
				return ParentOrgARTerms.JobTypeDirectionAndTransportListProvider.JobTypeList;
			}
		}

		public CodeDescriptionPairList DirectionList
		{
			get
			{
				return ParentOrgARTerms.JobTypeDirectionAndTransportListProvider.DirectionList;
			}
		}

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				return ParentOrgARTerms.JobTypeDirectionAndTransportListProvider.TransportModeList;
			}
		}

		public static readonly CodeDescriptionPair DefaultInvoiceTerm = new CodeDescriptionPair("DEF", ResString.GetMultilingualString("e1f14f19-6400-4c90-92c7-5ae5ab8749c9", "Default from AR Settlement Group"));
	}
}
