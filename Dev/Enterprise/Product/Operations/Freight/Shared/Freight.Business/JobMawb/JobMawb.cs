using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	[CodeProperty(JobMawbSchema.Constants.JM_Airline3DigitPrefix)]
	[System.Diagnostics.DebuggerDisplay("{JM_Airline3DigitPrefix} {JM_MAWB}")]
	public partial class JobMawb : AutoJobMawb, Integration.IJobMAWB, IDocManagerSupport, IJobInvoicingPlugIn, IEDocsProvider, IDocumentSupportable
	{
		#region Schema

		public new class Schema : AutoJobMawb.Schema
		{
			public const string JM_Calc_Airline2LetterCode = "JM_Calc_Airline2LetterCode";
			public const string JM_Calc_ParentJobNumber = "JM_Calc_ParentJobNumber";
			public const string JM_Calc_UsageIndicator = "JM_Calc_UsageIndicator";
			public const string JM_Calc_IsNeutral = "JM_Calc_IsNeutral";
			public const string MAWB_IsPrinted = "MAWB_IsPrinted";
		}

		#endregion

		public JobMawb(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return JM_Airline3DigitPrefix.IsEmpty || JM_MAWB.IsEmpty ? "MAWB" : string.Format((NoResString)"MAWB {0}-{1}", JM_Airline3DigitPrefix, JM_MAWB); }
		}

		#region New Properties

		public bool ShouldValidateAllocation
		{
			get => shouldValidateAllocation;
			set => shouldValidateAllocation = value;
		}
		bool shouldValidateAllocation;

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region CustomLogReferenceSuffix

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				ZString result = ZString.Empty;

				if (JM_IsPrinted && !(ZBool)JM_IsPrintedInfo.OriginalValue)
				{
					result = Res.GetString("0a7b1971-d1b9-4039-8cff-ac0d3b2fe67d", "Printed for job {0}", JM_Calc_ParentJobNumber);
				}
				else if (JM_ParentID != (ZGuid)JM_ParentIDInfo.OriginalValue)
				{
					if (JM_ParentID.IsValid)
					{
						result = Res.GetString("ce33f731-2c19-4c54-b948-2b803b1718d9", "Attached to job {0}", JM_Calc_ParentJobNumber);
					}
					else if (JM_ParentIDInfo.OriginalValue.IsValid)
					{
						result = Res.GetString("9136C6EF-9eba-4229-ab8f-e5b0396ecffb", "Detached from job {0}", GetParentJobNumber((ZGuid)JM_ParentIDInfo.OriginalValue, (ZString)JM_ParentTableCodeInfo.OriginalValue));
					}
				}

				return result;
			}
		}

		#endregion

		#region JM_Calc_Airline2LetterCode

		public ZString JM_Calc_Airline2LetterCode
		{
			get
			{
				RefAirline airline = RefAirline.LoadFromAirlinePrefix(Factory, JM_Airline3DigitPrefix);

				return airline != null && !JM_Airline3DigitPrefix.IsEmpty ? airline.RM_TwoCharacterCode : NoneSelected;
			}
		}

		public ZPropertyInfo JM_Calc_Airline2LetterCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JM_Calc_Airline2LetterCode); }
		}

		#endregion

		#region JM_Calc_ParentJobNumber

		public ZString JM_Calc_ParentJobNumber
		{
			get
			{
				return GetParentJobNumber(JM_ParentID, JM_ParentTableCode);
			}
		}

		public ZString GetParentJobNumber(ZGuid parentID, ZString parentTableCode)
		{
			ZString result = ZString.Empty;

			if (parentID.IsValid)
			{
				if (parentTableCode == JobConsolSchema.Constants.Prefix)
				{
					CommonConsol consol = (CommonConsol)Factory.Load<Enterprise.Integration.Forwarding.IForwardingConsol>(parentID);
					if (consol != null)
					{
						result = consol.JK_UniqueConsignRef;
					}
				}
				else if (parentTableCode == JobShipmentSchema.Constants.Prefix)
				{
					CommonShipment bookingShipment = (CommonShipment)Factory.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(parentID);
					if (bookingShipment != null)
					{
						result = bookingShipment.JS_UniqueConsignRef;
					}
				}
			}

			return result;
		}

		#endregion

		#region JM_Calc_UsageIndicator

		public ZString JM_Calc_UsageIndicator
		{
			get
			{
				if (JM_ParentID.IsValid)
				{
					return Parent != null ? Res.GetString("JobMawb|UsageIndicator|UsedBy", "Used by:") : Res.GetString("JobMawb|UsageIndicator|Allocated", "Allocated to a non-existing job.");
				}
				else if (JM_ParentID.IsEmpty && JM_ParentTableCode.IsEmpty)
				{
					if (JM_IsPrinted)
					{
						return Res.GetString("JobMawb|UsageIndicator|PrintedUnallocated", "Previously printed and un-allocated.");
					}

					return Res.GetString("JobMawb|UsageIndicator|Available", "Available to allocate.");
				}
				else
				{
					return Res.GetString("JobMawb|UsageIndicator|Unavailable", "Unavailable.");
				}
			}
		}

		public ZPropertyInfo JM_Calc_UsageIndicatorInfo
		{
			get { return GetZPropertyInfo(Schema.JM_Calc_UsageIndicator); }
		}

		#endregion

		#region JM_Calc_IsNeutral

		public ZBool JM_Calc_IsNeutral
		{
			get { return !JM_IsPaper; }
			set { JM_IsPaper = !value; }
		}

		public ZPropertyInfo JM_Calc_IsNeutralInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JM_Calc_IsNeutral, x => JM_IsPaperInfo); }
		}

		#endregion

		#region Home Port Text

		public ZString JM_Calc_HomePortText
		{
			get { return (Branch != null && Branch.HomePort != null) ? new ZString(Branch.HomePort.RL_Code + " - " + Branch.HomePort.RL_PortName) : NoneSelected; }
		}

		static ZString NoneSelected
		{
			get { return Res.GetString("JobMawb|b585b193-d1c7-4143-ba93-a8dd7be885a9", "None Selected"); }
		}

		#endregion

		#region Parent

		public IMAWBParent Parent => ParentCore;

		protected virtual IMAWBParent ParentCore
		{
			get
			{
				if (JM_ParentID.IsValid)
				{
					switch (JM_ParentTableCode)
					{
						case JobConsolSchema.Constants.Prefix:
							return (IMAWBParent)Factory.LoadTop1<Enterprise.Integration.Forwarding.IForwardingConsol>(new ZQuery(JobConsolSchema.PK, JM_ParentID));

						case JobShipmentSchema.Constants.Prefix:
							return (IMAWBParent)Factory.Load<IQuotedBooking>(JM_ParentID);

						default:
							throw new NotSupportedException("This type of parent for MAWB stock is not supported.");
					}
				}

				return null;
			}
		}

		#endregion

		#region Deleted Stack Trace

		public string DeletedStackTrace { get; set; }

		public string DeletedByDataRefreshStackTrace { get; private set; }

		#endregion

		#endregion

		#region JobHeader

		public JobHeader Job => new JobHeader.Loader(this).Load();

		#endregion

		#region Property Overrides

		[BusinessObjectTestExclude]
		public ZBool MAWB_IsPrinted
		{
			get { return base.JM_IsPrinted; }
			set
			{
				if (Env.Security.JobMAWBResetPrintedFlag.IsAllowed)
				{
					if (!value)
					{
						if (JM_IsPrinted)
						{
							DeallocateMAWB();
						}
					}
					else
					{
						if (isMawbDeallocated)
						{
							JM_IsPrinted = value;
							isMawbDeallocated = false;
						}
						else
						{
							throw new InvalidOperationException("Printed value should not be changed to true via MAWB_IsPrinted");
						}
					}
				}
				else
				{
					Globals.Message.Show(Env.Security.JobMAWBResetPrintedFlag.ErrorMessageForNotAllowed);
				}
				MAWB_IsPrintedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MAWB_IsPrintedInfo
		{
			get { return GetZPropertyInfo(Schema.MAWB_IsPrinted); }
		}

		ZBool isMawbDeallocated;

		void DeallocateMAWB()
		{
			var args = new CancelEventArgs();

			var handler = UncheckPrintedNeutralMAWB;

			if (handler != null)
			{
				handler(this, args);
			}

			if (!args.Cancel)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, ZString.Format("Returned printed MAWB to Stock"));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				JM_IsPrinted = false;
				isMawbDeallocated = true;
			}
		}

		public event CancelEventHandler UncheckPrintedNeutralMAWB;

		protected bool MAWB_IsPrinted_ReadOnly
		{
			get { return !IsPrintedShouldBeEditable(); }
		}

		ZBool IsPrintedShouldBeEditable()
		{
			return (JM_IsPrinted && JM_ParentID == Guid.Empty) || isMawbDeallocated;
		}

		protected bool JM_OH_AllocatedTo_ReadOnly
		{
			get { return !JM_OA_From.IsEmpty; }
		}

		[List("JM_GB_List")]
		public override ZGuid JM_GB
		{
			get { return base.JM_GB; }
			set
			{
				base.JM_GB = value;

				if (!JM_GBInfo.HasErrors() && JM_GB.IsValid && JM_GC_Company.IsEmpty)
				{
					var companyPk = Factory.Load<GlbBranch>(JM_GB)?.Company?.PK;
					if (companyPk != null && companyPk.Value != ZGuid.Empty)
					{
						using (GetValidationSuspender())
						{
							JM_GC_Company = companyPk.Value;
						}
					}
				}
			}
		}

		#region JM_OA_From

		[OrganisationDefaultProvider()]
		public OrgHeaderCollection BorrowedFromList
		{
			get
			{
				var result = BindingLists.Organisations;
				return result;
			}
		}

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		protected bool JM_OA_From_ReadOnly
		{
			get { return !JM_OH_AllocatedTo.IsEmpty; }
		}

		protected override ZAddress GetNewJM_OA_From_ZAddress()
		{
			var result = base.GetNewJM_OA_From_ZAddress();
			result.DefaultAddressType = AddressType.NoDefault;
			result.GetDefaultAddress = header => GetDefaultBorrowerAddressPK(header as OrgHeader);

			return result;
		}

		ZGuid GetDefaultBorrowerAddressPK(OrgHeader header)
		{
			if (header == null)
			{
				return ZGuid.Empty;
			}

			var headerAddresses = header.Addresses.Cast<OrgAddress>().Where(a => a.OA_IsActive).ToArray();

			OrgAddress defaultAddress = null;

			if (header.MainAddress.OA_IsActive)
			{
				defaultAddress = header.MainAddress;
			}

			if (defaultAddress == null)
			{
				defaultAddress = headerAddresses.FirstOrDefault();
			}

			return defaultAddress != null ? defaultAddress.PK : header.MainAddress.PK;
		}

		#endregion

		[List("NeutralAirWaybillServiceLevels")]
		public override ZString JM_ServiceLevel
		{
			get { return base.JM_ServiceLevel; }
			set { base.JM_ServiceLevel = value; }
		}

		[ReadOnly(true)]
		public override ZString JM_Airline3DigitPrefix
		{
			get { return base.JM_Airline3DigitPrefix; }
			set
			{
				if (JM_Airline3DigitPrefix != value)
				{
					base.JM_Airline3DigitPrefix = value;
					neutralAirWaybillServiceLevels = null;
				}
			}
		}

		#endregion

		#region Method Overrides

		public override void Delete()
		{
			if (IsInDatabase && !CanDelete)
			{
				throw new CannotDeleteException(ReasonForNotAbleToDelete);
			}

			DeletedStackTrace += System.Environment.StackTrace;
			base.Delete();
		}

		protected override void OnDeletedByDataRefresh()
		{
			DeletedByDataRefreshStackTrace += System.Environment.StackTrace;
			base.OnDeletedByDataRefresh();
		}

		#endregion

		#region Lists

		public GlbBranchCollection JM_GB_List
		{
			get
			{
				fJM_GB_List ??= new GlbBranchCollection(Factory);
				fJM_GB_List.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault((ZString)(NoResString)"Company", "Property", JM_GC_Company));
				return fJM_GB_List;
			}
		}

		GlbBranchCollection fJM_GB_List;

		public OrgHeaderCollection ForwarderList
		{
			get
			{
				if (fForwarderList == null)
				{
					fForwarderList = new ForwarderCollection(Factory);
				}
				return fForwarderList;
			}
		}

		OrgHeaderCollection fForwarderList;

		#region Neutral Air Waybill Service Levels

		public BusinessObjectCollection NeutralAirWaybillServiceLevels
		{
			get
			{
				if (neutralAirWaybillServiceLevels == null)
				{
					var serviceLevels = GetNewNeutralAirWaybillServiceLevels();
					serviceLevels.Load();

					var view = new OrgCarrierServiceLevelCollectionView(serviceLevels, ServiceLevelFilter);
					view.Rebuild();

					neutralAirWaybillServiceLevels = view;
				}

				return neutralAirWaybillServiceLevels;
			}
		}

		BusinessObjectCollection neutralAirWaybillServiceLevels;

		#region OrgCarrierServiceLevelCollectionView Class
#if DEBUG
		public
#endif
		class OrgCarrierServiceLevelCollectionView : BusinessObjectCollectionView<BusinessObject>
		{
			public OrgCarrierServiceLevelCollectionView(OrgCarrierServiceLevelCollection serviceLevels, Func<OrgCarrierServiceLevel, bool> filter)
				: base(serviceLevels)
			{
				this.filter = filter;
			}

			readonly Func<OrgCarrierServiceLevel, bool> filter;

			protected override bool IsThisPartOfTheCollection(BusinessObject element)
			{
				var serviceLevel = (OrgCarrierServiceLevel)element;

				return filter == null || filter(serviceLevel);
			}
		}

		[BusinessObjectTestExclude]
		public Func<OrgCarrierServiceLevel, bool> ServiceLevelFilter
		{
			get { return serviceLevelFilter; }
			set
			{
				serviceLevelFilter = value;
				neutralAirWaybillServiceLevels = null;
			}
		}

		Func<OrgCarrierServiceLevel, bool> serviceLevelFilter;

		protected OrgCarrierServiceLevelCollection GetNewNeutralAirWaybillServiceLevels()
		{
			OrgHeader carrier = LoadMatchingCarrier();
			return carrier != null
				? new OrgCarrierServiceLevelCollection(carrier.MiscServ, true)
				: new OrgCarrierServiceLevelCollection(Factory, true);
		}

		#endregion

		OrgHeader LoadMatchingCarrier()
		{
			return LoadMatchingCarrier(Factory, Branch, JM_Airline3DigitPrefix);
		}

		public static OrgCarrierServiceLevelCollection GetNeutralAirWaybillServiceLevelsFrom2LetterCode(BusinessObjectFactory factory, GlbBranch branch, ZString airline2LetterCode)
		{
			var airline = RefAirline.LoadFromAirline2LetterCode(factory, airline2LetterCode);
			return airline != null
				? GetNeutralAirWaybillServiceLevels(factory, branch, airline.RM_EagleAddedAirlinePrefixOrAccountingCode)
				: new OrgCarrierServiceLevelCollection(factory, false);
		}

		static OrgCarrierServiceLevelCollection GetNeutralAirWaybillServiceLevels(BusinessObjectFactory factory, GlbBranch branch, ZString airline3DigitPrefix)
		{
			var carrier = LoadMatchingCarrier(factory, branch, airline3DigitPrefix);
			return carrier != null
				? new OrgCarrierServiceLevelCollection(carrier.MiscServ, false)
				: new OrgCarrierServiceLevelCollection(factory, false);
		}

		static OrgHeader LoadMatchingCarrier(BusinessObjectFactory factory, GlbBranch branch, ZString airline3DigitPrefix)
		{
			if (airline3DigitPrefix.IsEmpty)
			{
				return null;
			}

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

			ZDBOnlySubQuery subQueryRefAirline = new ZDBOnlySubQuery(typeof(RefAirline), RefAirlineSchema.PK);
			subQueryRefAirline.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, airline3DigitPrefix);

			ZDBOnlySubQuery subQueryOrgMiscServ = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			subQueryOrgMiscServ.AddSubQuery(OrgMiscServSchema.OM_RM_Airline, subQueryRefAirline, JoinCondition.And);
			query.AddSubQuery(subQueryOrgMiscServ, JoinCondition.And);

			ZDBOnlySubQuery subQueryBranchCode = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQueryBranchCode.AddToFilter(OrgCompanyDataSchema.OB_GB_ControllingBranch, branch?.PK ?? ZGuid.Empty);
			query.AddSubQuery(subQueryBranchCode, JoinCondition.And);

			var carrier = factory.LoadTop1<OrgHeader>(query);

			if (carrier == null && branch != null)
			{
				query = new ZDBOnlyQuery(typeof(OrgHeader));

				ZDBOnlySubQuery subQueryCompanyBranch = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
				subQueryCompanyBranch.AddToFilter(OrgCompanyDataSchema.OB_GB_ControllingBranch, branch.Company.Branches.Select(b => b.PK));

				query.AddSubQuery(subQueryOrgMiscServ, JoinCondition.And);
				query.AddSubQuery(subQueryCompanyBranch, JoinCondition.And);

				carrier = factory.LoadTop1<OrgHeader>(query);
			}

			if (carrier == null)
			{
				ZDBOnlyQuery queryOrgMiscServ = new ZDBOnlyQuery(typeof(OrgMiscServ));
				queryOrgMiscServ.AddSubQuery(OrgMiscServSchema.OM_RM_Airline, subQueryRefAirline, JoinCondition.And);

				OrgMiscServ miscServ = factory.LoadTop1<OrgMiscServ>(queryOrgMiscServ);
				carrier = miscServ?.Header;
			}

			return carrier;
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.NeutralMasters);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IJobHeaderParent Members

		public virtual void OnJobCreating(JobHeader job)
		{
		}

		public virtual void OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		JobMawbInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new JobMawbInvoicingSupporter(this)); }
		}

		#endregion

		#region IJobNumber Members

		public string JobNumber
		{
			get { return "M" + JM_Airline3DigitPrefix + JM_MAWB; }
		}

		public void SetJobNumberFieldOnSaving()
		{
			//not required because master bill number is required for saving
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new JobMawbDocumentSupporter(this); }
		}

		#endregion

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new JobMawbUniqueIndexFailureHandler(this); }
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobMawbFetchStrategy(this);
		}

		#endregion

		#region ICanDelete Members

		public override bool CanDelete => base.CanDelete && !JM_IsPrinted && Parent == null;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;
				if (!string.IsNullOrEmpty(result))
				{
					return result;
				}

				if (JM_IsPrinted)
				{
					return ResString.GetMultilingualString("e63b3c96-09ba-44cb-8749-dc75fca9b262", "{0} cannot be deleted as it has been printed.", HumanReadableName);
				}

				if (Parent != null)
				{
					return ResString.GetMultilingualString("9e12dcf0-a540-4f40-a7f9-a8c40fcb7014",
						"{0} cannot be deleted as it is allocated to {1}.",
						HumanReadableName,
						(Parent as BusinessObject)?.HumanReadableName ?? (NoResString)"parent");
				}

				return result;
			}
		}

		#endregion

		#region OnFactorySaved

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				isMawbDeallocated = false;
			}
		}

		#endregion
	}

	public class JobMawbInvoicingSupporter : JobInvoicingSupporter
	{
		public JobMawbInvoicingSupporter(IJobHeaderParent parent) : base(parent) { }

		public override ZString ConsolType
		{
			get { return Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		public override ZString TransportMode
		{
			get { return Constants.TransportModes.Air; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.MasterAWB; }
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.JobMAWBAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.JobMAWBJobInvoicing;
		}

		public override ZString EditSecurityMessage
		{
			get { return EditSecurityMessageCore; }
		}

		protected virtual ZString EditSecurityMessageCore
		{
			get { return ZString.Empty; }
		}

		public override bool EditSecurityLock
		{
			get { return EditSecurityLockCore; }
		}

		protected virtual bool EditSecurityLockCore
		{
			get { return false; }
		}
	}

	#region Document Supporter

	public class JobMawbDocumentSupporter : DocumentSupporter
	{
		public JobMawbDocumentSupporter(JobMawb jobMawb)
			: base(jobMawb)
		{
		}

		protected JobMawb JobMawb
		{
			get { return (JobMawb)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.JobMAWB; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[] { Core.Constants.DataContext.GenericFreightJob };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, JobMawb);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}
			return null;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			OrgAddress address = null;

			if (contactType == MasterFiles.Business.ContactType.Receivables || contactType == MasterFiles.Business.ContactType.LocalClient)
			{
				JobHeader job = new JobHeader.Loader(JobMawb).Load();
				if (job != null)
				{
					address = Factory.Load<OrgAddress>(job.JH_OA_LocalChargesAddr);
				}
			}

			return new DocumentEngine.OrgHeaderContact(address?.Header, address);
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Core.Constants.DataContext.GenericFreightJob && base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}
	}

	#endregion
}

#region Test
#if DEBUG

#region Test Methods

namespace Enterprise.Freight.Business
{
	partial class JobMawb
	{
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			JM_ParentTableCode = "";
			FillPropertyWithUniqueString(JM_MAWBInfo);
			FillPropertyWithUniqueString(JM_Airline3DigitPrefixInfo);
		}

		static void FillPropertyWithUniqueString(ZPropertyInfo propertyInfo)
		{
			if (propertyInfo.Value.IsEmpty)
			{
				propertyInfo.Value = GetUniqueStringForPropertyWithSeed(propertyInfo, 1);
			}
		}

		static ZString GetUniqueStringForPropertyWithSeed(ZPropertyInfo propertyInfo, int initialSeed)
		{
			ZString result = "";
			Random random = new Random(initialSeed);
			for (int i = 0; i < propertyInfo.MaxLength; i++)
			{
				result += CharactersToUse[random.Next(CharactersToUse.Length - 1)];
			}

			// if its not unique, try again
			foreach (DataRow row in ((INeedTable)propertyInfo.BizObj).Table.Rows)
			{
				if (row.RowState != DataRowState.Deleted && row[propertyInfo.Name].ToString() == result)
				{
					result = GetUniqueStringForPropertyWithSeed(propertyInfo, initialSeed + 1);
					break;
				}
			}

			return result;
		}

		const string CharactersToUse = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
	}
}

#endregion

#endif
#endregion
