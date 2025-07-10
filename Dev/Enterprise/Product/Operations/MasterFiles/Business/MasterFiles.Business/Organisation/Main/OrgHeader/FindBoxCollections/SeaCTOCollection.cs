using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class SeaCTOCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		#region Constructors
		public SeaCTOCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public SeaCTOCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public SeaCTOCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public SeaCTOCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}
		#endregion

		#region Properties
		public ZString ClosestPortUNLOCO
		{
			get { return fClosestPortUNLOCO; }
			set { fClosestPortUNLOCO = value; }
		}
		string fClosestPortUNLOCO;
		#endregion

		#region Overrides

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery seaCTO = new ZQuery(OrgHeaderSchema.OH_IsSeaCTO, true);
			ZQuery miscServ = new ZQuery(OrgHeaderSchema.OH_IsMiscFreightServices, true);
			ZQuery filter = new ZQuery(miscServ, JoinCondition.And, seaCTO);
			if (!ClosestPortUNLOCO.IsEmpty)
			{
				filter.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, ClosestPortUNLOCO);
			}

			ZQuery query = new ZQuery(filter);
			query.AddToFilter(base.CreateAdditionalFilter());
			return query;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader orgHeader = (OrgHeader)child;
			orgHeader.OH_IsMiscFreightServices = orgHeader.SecurityProvider.HasNewDetailsOrgTypeFlagSV;
			orgHeader.OH_IsSeaCTO = true;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.SeaCTO));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (((OrgHeader)selectedBusinessObject).OH_IsActive)
			{
				errors.Add(Res.GetString("743acb12-2652-4542-886f-cbfa3b0a072d", "An Organization selected from here must have an Organization type of Services selected and must have Sea CTO selected."));
			}
		}
		#endregion

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsMiscFreightServicesInfo, organisation.OH_IsSeaCTOInfo);
		}

		#endregion

	}
}
