using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
			parent = invoiceHeader;
		}
		readonly JobComInvoiceHeader parent;

		public override CodeDescriptionPairList JZ_IncoTerm_List
		{
			get { return Factory.GetCachedValue<IncoTermList>(); }
		}

		public CodeDescriptionPairList JZ_ExchangeRateIndicator_List
		{
			get { return Factory.GetCachedValue<ExchangeRateIndicatorList>(); }
		}

		public CodeDescriptionPairList JZ_RelationshipIndicator_List
		{
			get
			{
				bool isTSWDec = false;
				if (parent != null && parent.JobDeclaration != null)
				{
					isTSWDec = parent.JobDeclaration.IsTSWDeclaration;
				}

				return Factory.GetCachedValue<CodeDescriptionPairList>("NZ JobComInvoiceHeaderLookups RelationshipIndicatorList", delegate
				{
					return new RelationshipIndicatorList(isTSWDec);
				});
			}
		}

		public RefCountryCollection CountryList
		{
			get { return new RefCountryCollection(Factory); }
		}

		public CodeDescriptionPairList QualifiesForPreferentialDutyList
		{
			get { return Factory.GetCachedValue<QualifiesForPreferentialDutyList>(); }
		}

		public PreferentialCountryGroupCodeList PreferentialCountryGroupCodeList
		{
			get { return PreferentialCountryGroupCodeList.GetListFor(parent.JZ_RN_NKDefaultOrigin, parent.DateForDutyRate, parent.Factory); }
		}

		public YesNoList YesNoList
		{
			get { return Factory.GetCachedValue<YesNoList>(); }
		}
	}
}
