//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSOrgImpAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSOrgImpAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USOrgImpAddInfoLookups : AutoUSOrgImpAddInfoLookups
	{
		public USOrgImpAddInfoLookups(AutoUSOrgImpAddInfo parent)
			: base(parent)
		{
		}

		public OrgHeaderCollection NotifyParties
		{
			get { return fNotifyParties ?? (fNotifyParties = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection fNotifyParties;

		public CodeDescriptionPairList ZO_ImporterTypeList => Factory.GetCachedValue<ImporterTypeList>();

		public CodeDescriptionPairList MessageStatusList
		{
			get { return Factory.GetCachedValue<OrgMessageStatusList>(); }
		}

		public CodeDescriptionPairList ZO_YesNoList
		{
			get { return YesNoDefaultList.GetCachedYesNoList(Factory); }
		}

		public PaymentTypeList PaymentTypes
		{
			get { return Factory.GetCachedValue<PaymentTypeList>(); }
		}

		public ProducerFirmTypeList ProducerFirmTypes
		{
			get { return Factory.GetCachedValue<ProducerFirmTypeList>(); }
		}

		public SubmitterFirmTypeList SubmitterFirmTypes
		{
			get { return Factory.GetCachedValue<SubmitterFirmTypeList>(); }
		}

		public TaxDeferIndicatorList TaxDeferredIndicators
		{
			get { return Factory.GetCachedValue<TaxDeferIndicatorList>(); }
		}

		public CodeDescriptionPairList FDAPriorNoticeExemptCodeList
		{
			get { return Factory.GetCachedValue<FDAPriorNoticeExemptCodeList>(); }
		}

		public ReconIssueCodeList OtherReconIssueList
		{
			get { return OtherReconIssueListCreator.CreateOtherReconIssueList(Factory); }
		}

		public GlbBranchCollection Branches
		{
			get { return new GlbBranchCollection(Factory); }
		}

		public ReconciliationImportEntrySourceList ZO_ImportSourceList
		{
			get { return Factory.GetCachedValue<ReconciliationImportEntrySourceList>(); }
		}

		public ZZRefCusCodeListCombinedCollection ReconPorts
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public PaymentTypeList ReconPaymentTypes
		{
			get { return PaymentTypeList.GetCachedReconPaymentTypeList(Factory); }
		}

		public ACHPaymentTypeList PayMethodList
		{
			get { return Factory.GetCachedValue<ACHPaymentTypeList>(); }
		}

		public ZZRefCusCodeListCombinedCollection FIRMSList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today); }
		}

		public CodeDescriptionPairList DefTaxDueDateCalculationOptionList
		{
			get { return Factory.GetCachedValue<DefTaxDueDateCalculationOptionList>(); }
		}

		public ConsigneeCollection Consignees
		{
			get { return new ConsigneeCollection(Factory); }
		}
	}
}
