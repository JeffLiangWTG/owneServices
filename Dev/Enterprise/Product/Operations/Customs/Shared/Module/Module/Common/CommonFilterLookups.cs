using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class CommonFilterLookups
	{
		public CommonFilterLookups(FilterStripBusinessObject filterBizObj)
		{
			this.FilterBizObj = filterBizObj;
		}

		protected readonly FilterStripBusinessObject FilterBizObj;

		protected BusinessObjectFactory Factory
		{
			get { return FilterBizObj.Factory; }
		}

		public virtual CodeDescriptionPairList EntryStatusList(ZString countryCode)
		{
			var filter = (ModuleTextFilter)FilterBizObj["Shipment Type"];
			var messageType = filter?.Property ?? ZString.Empty;

			return (CodeDescriptionPairList)Common.EntryStatusListHelper.EntryStatusList(Factory, countryCode, messageType);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public virtual CodeDescriptionPairList MessageStatusList()
		{
			var result = new CodeDescriptionPairList();
			var listToClone = GetMainMessageStatusList();

			foreach (ICodeDescription pair in listToClone)
			{
				if (pair.Description == "Not Sent" || pair.Code == ZString.Empty)
				{
					result.Add(new CodeDescriptionPair(DeclarationFilterConstants.EntryStatus.NotSentForFilter, pair.Description));
				}
				else
				{
					result.Add(new CodeDescriptionPair(pair.Code, pair.Description));
				}
			}

			return result;
		}

		protected virtual CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<Common.Shared.MessageStatusList>();

		public ControllingAgentCollection ControllingAgents => new ControllingAgentCollection(Factory);

		public ControllingAgentCollection FilterControllingAgents => new ControllingAgentCollection(Factory)
		{
			ShouldApplyActiveFilter = false
		};

		public ControllingCustomerCollection ControllingCustomers => new ControllingCustomerCollection(Factory);

		public ControllingCustomerCollection FilterControllingCustomers => new ControllingCustomerCollection(Factory)
		{
			ShouldApplyActiveFilter = false
		};

		public OrgHeaderCollection Consignors => new ConsignorCollection(Factory);

		public OrgHeaderCollection FilterConsignors => new ConsignorCollection(Factory)
		{
			ShouldApplyActiveFilter = false
		};

		public OrgHeaderCollection Consignees => new ConsigneeCollection(Factory);

		public OrgHeaderCollection FilterConsignees => new ConsigneeCollection(Factory)
		{
			ShouldApplyActiveFilter = false
		};

		public LocalTransportCollection CartageList => new LocalTransportCollection(Factory);

		public RefVesselCollection VesselList => new RefVesselCollection(Factory);

		public LocationCollection LocationList => new LocationCollection(Factory);

		public GlbStaffCollection StaffList => new GlbStaffCollection(Factory);

		public GlbBranchCollection BranchList => new GlbBranchCollection(Factory);
	}
}
