using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AirShippingProviderCollection : ShippingProviderCollection, IValidateForController
	{
		public AirShippingProviderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AirShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public AirShippingProviderCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public AirShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var airLineQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			airLineQuery.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsAirLine, true);
			airLineQuery.AddToFilter(base.CreateAdditionalFilter());

			var miscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			miscServSubQuery.AddToFilter(JoinCondition.Or, OrgMiscServSchema.OM_RM_Airline, null);
			miscServSubQuery.AddToFilter(JoinCondition.Or, OrgMiscServSchema.OM_RM_Airline, ZGuid.Empty);
			var refAirlineSubQuery = new ZDBOnlySubQuery(typeof(RefAirline), RefAirlineSchema.PK);
			miscServSubQuery.AddSubQuery(OrgMiscServSchema.OM_RM_Airline, refAirlineSubQuery, JoinCondition.Or);

			airLineQuery.AddSubQuery(miscServSubQuery, JoinCondition.And);
			return airLineQuery;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((OrgHeader)child).OH_IsAirLine = true;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			base.SetFilterBusinessObjectDefaults();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.Airline));
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			var orgHeader = (OrgHeader)selectedBusinessObject;

			if (orgHeader.OH_IsActive)
			{
				if (!orgHeader.OH_IsAirLine)
				{
					errors.Add(Res.GetString("8c5beb2e-dd88-4abb-88df-c4543748d534", "An Organization selected from here must have Airline selected."));
				}
				else if (!(orgHeader.MiscServ.Airline?.RM_IsActive ?? true))
				{
					errors.Add(Res.GetString("eb53e64c-d34a-406f-9312-11d8787614d5", "This Organization is linked to an airline that is marked as inactive in the airline reference file."));
				}
			}
		}

		#region IValidateForController Members

		public override void ValidateEntityOnSaving(IBusiness entity)
		{
			base.ValidateEntityOnSaving(entity);
			OrgHeader organisation = (OrgHeader)entity;
			organisation.SetOrgTypeAsExpectedAndValidate(organisation.OH_IsAirLineInfo);
		}

		#endregion
	}
}
