using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class JobDeclarationFilterLookups : CommonFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		protected new JobDeclarationFilterBusinessObject FilterBizObj => (JobDeclarationFilterBusinessObject)base.FilterBizObj;

		#region Findbox Lists

		public RefUNLOCOCollection PortOfFirstArrivalList
		{
			get
			{
				if (fPortOfFirstArrivalList == null)
				{
					fPortOfFirstArrivalList = new RefUNLOCOCollection(Factory);
				}
				return fPortOfFirstArrivalList;
			}
		}
		RefUNLOCOCollection fPortOfFirstArrivalList;

		public GlbDepartmentCollection DepartmentList
		{
			get
			{
				if (fGlbDeaprtmenthCollection == null)
				{
					fGlbDeaprtmenthCollection = new GlbDepartmentCollection(Factory);
				}
				return fGlbDeaprtmenthCollection;
			}
		}
		GlbDepartmentCollection fGlbDeaprtmenthCollection;

		public virtual OrgHeaderCollection ShippingLines
		{
			get { return new ShippingProviderCollection(Factory); }
		}

		public OrgHeaderCollection FilterShippingLines
		{
			get
			{
				return new ShippingProviderCollection(Factory)
				{
					ShouldApplyActiveFilter = false
				};
			}
		}

		public BrokerCollection ExternalBrokers
		{
			get { return externalBrokers ?? (externalBrokers = new BrokerCollection(Factory)); }
		}
		BrokerCollection externalBrokers;

		public BrokerCollection FilterExternalBrokers
		{
			get
			{
				return filterExternalBrokers ?? (filterExternalBrokers = new BrokerCollection(Factory)
				{
					ShouldApplyActiveFilter = false
				});
			}
		}
		BrokerCollection filterExternalBrokers;

		public OrgHeaderCollection Forwarders
		{
			get { return new ForwarderCollection(Factory); }
		}

		public OrgHeaderCollection FilterForwarders
		{
			get
			{
				return new ForwarderCollection(Factory)
				{
					ShouldApplyActiveFilter = false
				};
			}
		}

		#endregion

		#region CodeDescriptionPairLists

		public virtual CodeDescriptionPairList ApplicationCodeList() => Factory.GetCachedValue<DeclarationApplicationCodeList>();

		public virtual CodeDescriptionPairList ContainerModeList => Declaration.Lookups.CargoIdTypeList;

		public virtual CodeDescriptionPairList TransportTypeList => Declaration.Lookups.TransportTypeList;

		public RefServiceLevelCollection ServiceLevelList => new ActiveServiceLevelCollection(Factory);

		public virtual CodeDescriptionPairList MessageTypeList => Declaration.Lookups.MessageTypeList;

		public virtual CodeDescriptionPairList MessageSubTypeList() => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList PaymentPartyList() => new PaymentPartyCodeDescriptionList();

		#region EntryStatusList

		public virtual CodeDescriptionPairList EntryStatusList() => base.EntryStatusList(FilterBizObj.CountryCode);

		#endregion

		public virtual CodeDescriptionPairList EntryTypeList => new CodeDescriptionPairList();

		public CodeDescriptionPairList ServiceTypeList => Service.Lookups.JobServiceType_List;

		#endregion

		#region DropModeList

		public CodeDescriptionPairList DropModeList => Factory.GetCachedValue<CombinedEquipmentNeededList>();

		#endregion

		BaseJobDeclaration Declaration => declaration ?? (declaration = Factory.GetNull<BaseJobDeclaration>());
		BaseJobDeclaration declaration;

		JobService Service => service ?? (service = Factory.GetNull<JobService>());
		JobService service;
	}
}
