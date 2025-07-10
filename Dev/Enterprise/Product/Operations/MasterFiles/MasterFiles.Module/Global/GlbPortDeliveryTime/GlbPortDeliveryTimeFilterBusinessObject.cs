using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Module
{
	public class GlbPortDeliveryTimeFilterBusinessObject : FilterStripBusinessObject
	{
		public GlbPortDeliveryTimeFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddModeFilters(filters);
			AddRelatedItemFilters(filters);
			AddCompanyHiddenFilter(filters);
			return filters;
		}

		#region Mode

		void AddModeFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter freightModeFilter = filters.AddTextFilter("Freight Mode", GlbPortDeliveryTimeSchema.G1_FreightMode, FreightModeList);
			freightModeFilter.Category = FilterCategories.ModesAndTypes;
			freightModeFilter.DefaultProperty = Constants.TransportModes.Air;
			freightModeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbPortDeliveryTimeFilter|FreightMode", "Freight Mode");

			ModuleTextFilter jobModeFilter = filters.AddTextFilter("Job Mode", GlbPortDeliveryTimeSchema.G1_JobMode, JobModeList);
			jobModeFilter.Category = FilterCategories.ModesAndTypes;
			jobModeFilter.DefaultProperty = "FWD";
			jobModeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbPortDeliveryTimeFilter|JobMode", "Job Mode");
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			filters.AddNkFilter("Discharge Port", GlbPortDeliveryTimeSchema.G1_RL_NKDischargePort, ModuleIDs.RefUNLOCO, UNLOCOs).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbPortDeliveryTimeFilter|DischargePort", "Discharge Port");
			filters.AddNkFilter("Destination Port", GlbPortDeliveryTimeSchema.G1_RL_NKDestinationPort, ModuleIDs.RefUNLOCO, UNLOCOs).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbPortDeliveryTimeFilter|DestinationPort", "Destination Port");

			filters.AddGuidFilter("Client", ModuleIDs.Organisation, GlbPortDeliveryTimeSchema.G1_OH_ClientOverride, Consignees).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbPortDeliveryTimeFilter|Client", "Client");
		}

		#endregion

		#region Hidden

		void AddCompanyHiddenFilter(ModuleFilterCollection filters)
		{
			ModuleGuidFilter hiddenGuidFilter = filters.AddGuidFilter("Company", ModuleIDs.GlbCompany, GlbPortDeliveryTimeSchema.G1_GC_Company, Companies);
			hiddenGuidFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			hiddenGuidFilter.Property = Env.CurrentCompany.PK;
			hiddenGuidFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbPortDeliveryTimeFilter|Company", "Company");
		}

		#endregion

		#endregion

		#region Collections

		#region UNLOCOs

		RefUNLOCOCollection UNLOCOs
		{
			get
			{
				if (fUNLOCOs == null)
				{
					fUNLOCOs = new RefUNLOCOCollection(Factory);
				}

				return fUNLOCOs;
			}
		}

		RefUNLOCOCollection fUNLOCOs;

		#endregion

		#region Consignees

		public ConsigneeCollection Consignees
		{
			get
			{
				if (fConsignees == null)
				{
					fConsignees = new ConsigneeCollection(Factory);
				}
				return fConsignees;
			}
		}

		ConsigneeCollection fConsignees;

		#endregion

		#region Companies

		public GlbCompanyCollection Companies
		{
			get
			{
				if (fCompanies == null)
				{
					fCompanies = new GlbCompanyCollection(Factory);
				}
				return fCompanies;
			}
		}

		GlbCompanyCollection fCompanies;

		#endregion

		#endregion

		#region Code Description Pair Lists

		CodeDescriptionPairList fFreightModeList;
		public CodeDescriptionPairList FreightModeList
		{
			get
			{
				if (fFreightModeList == null)
				{
					fFreightModeList = new CodeDescriptionPairList();
					fFreightModeList.AddPair(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air);
					fFreightModeList.AddPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea);
					fFreightModeList.AddPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road);
					fFreightModeList.AddPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail);
					fFreightModeList.AddPair(Constants.ContainerModes.LCL, Constants.TransportModeDescriptions.Sea + " - " + Res.GetString("MasterFiles|ContainerMode|LCL", "Less Container Load"));
					fFreightModeList.AddPair(Constants.ContainerModes.FCL, Constants.TransportModeDescriptions.Sea + " - " + Res.GetString("MasterFiles|ContainerMode|FCL", "Full Container Load"));
				}
				return fFreightModeList;
			}
		}

		CodeDescriptionPairList fJobModeList;
		public CodeDescriptionPairList JobModeList
		{
			get
			{
				if (fJobModeList == null)
				{
					fJobModeList = new CodeDescriptionPairList();
					fJobModeList.AddPair("FWD", Res.GetString("MasterFiles|GlbPortDeliveryTimeFilter|ForwardingOnly", "Forwarding Only"));
					fJobModeList.AddPair("CUS", Res.GetString("MasterFiles|GlbPortDeliveryTimeFilter|CustomsOnly", "Customs Only"));
					fJobModeList.AddPair("ALL", Res.GetString("MasterFiles|GlbPortDeliveryTimeFilter|ForwardingAndCustoms", "Forwarding and Customs"));
				}
				return fJobModeList;
			}
		}

		#endregion
	}
}
