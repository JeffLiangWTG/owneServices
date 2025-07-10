using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefCommodityCode)]
	public class OrgCarrierServiceLevelCollection : BusinessObjectCollection<OrgCarrierServiceLevel>
	{
		public OrgCarrierServiceLevelCollection(BusinessObjectFactory factory, bool includeAllServiceLevel, bool includeStdServiceLevel = true)
			: base(factory)
		{
			this.IncludeAllServiceLevel = includeAllServiceLevel;
			this.IncludeStdServiceLevel = includeStdServiceLevel;
		}

		public OrgCarrierServiceLevelCollection(BusinessObjectFactory factory)
			: this(factory, false)
		{
		}

		public OrgCarrierServiceLevelCollection(OrgMiscServ carrierOrganisationMiscServ, bool includeAllServiceLevel, bool includeStdServiceLevel = true)
			: this(carrierOrganisationMiscServ.Factory)
		{
			this.Master = carrierOrganisationMiscServ;
			this.IncludeAllServiceLevel = includeAllServiceLevel;
			this.IncludeStdServiceLevel = includeStdServiceLevel;
		}

		public OrgCarrierServiceLevelCollection(OrgMiscServ carrierOrganisationMiscServ)
			: this(carrierOrganisationMiscServ, false)
		{
		}

		public readonly OrgMiscServ Master;

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (Master != null)
			{
				((OrgCarrierServiceLevel)child).PL_OM = Master.PK;
			}
		}

		#endregion

		#region Load

		readonly bool IncludeAllServiceLevel;
		readonly bool IncludeStdServiceLevel;

		public override void Load()
		{
			if (Master != null)
			{
				base.Load();
			}

			if (IncludeStdServiceLevel)
			{
				AddStandardServiceLevel();
			}

			if (IncludeAllServiceLevel)
			{
				AddAllServiceLevel();
			}
		}

		void AddStandardServiceLevel()
		{
			if (!ServiceLevelExists(OrgCarrierServiceLevel.StandardCode))
			{
				try
				{
					StandardSvcLevel = AddNew();
					using (StandardSvcLevel.SuspendSettingHasChanges())
					{
						StandardSvcLevel.PL_Code = OrgCarrierServiceLevel.StandardCode;
						StandardSvcLevel.PL_CarrierServiceLevelDescription = OrgCarrierServiceLevel.StandardDescription.GetUnresolvedString();
					}
				}
				finally
				{
					HasChanges = false;
				}
			}
		}

		OrgCarrierServiceLevel StandardSvcLevel;

		bool ServiceLevelExists(string code)
		{
			return Find(new ZQuery(OrgCarrierServiceLevelSchema.PL_Code, code)).Length > 0;
		}

		void AddAllServiceLevel()
		{
			if (!ServiceLevelExists(OrgCarrierServiceLevel.AllCode))
			{
				try
				{
					AllSvcLevel = AddNew();
					using (AllSvcLevel.SuspendSettingHasChanges())
					{
						AllSvcLevel.PL_Code = OrgCarrierServiceLevel.AllCode;
						AllSvcLevel.PL_CarrierServiceLevelDescription = OrgCarrierServiceLevel.AllDescription.GetUnresolvedString();
					}
				}
				finally
				{
					HasChanges = false;
				}
			}
		}

		OrgCarrierServiceLevel AllSvcLevel;

		#endregion

		#region Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			if (Master != null)
			{
				query.AddToFilter(OrgCarrierServiceLevelSchema.PL_OM, Master.PK);
			}
			else
			{
				if (IncludeStdServiceLevel && StandardSvcLevel != null)
				{
					query.AddToFilter(OrgCarrierServiceLevelSchema.PK, StandardSvcLevel.PK);
				}
				if (IncludeAllServiceLevel && AllSvcLevel != null)
				{
					query.AddToFilter(JoinCondition.Or, OrgCarrierServiceLevelSchema.PK, AllSvcLevel.PK);
				}
			}
			return query;
		}

		#endregion

		#region Get Description From Code

		public ZString GetDescriptionFromCode(ZString code)
		{
			ZString result = ZString.Empty;

			BusinessObject[] arrayListCodes = Find(new ZQuery(OrgCarrierServiceLevelSchema.PL_Code, code));
			if (arrayListCodes.Length > 0)
			{
				result = ((OrgCarrierServiceLevel)arrayListCodes[0]).PL_CarrierServiceLevelDescriptionMultilingual;
			}

			return result;
		}

		#endregion
	}
}
