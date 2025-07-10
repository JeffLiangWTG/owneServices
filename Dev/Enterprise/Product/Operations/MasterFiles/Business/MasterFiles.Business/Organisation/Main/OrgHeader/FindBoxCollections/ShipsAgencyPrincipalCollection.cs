using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ShipsAgencyPrincipalCollection : OrganisationsFindBoxCollection, IShipsAgencyPrincipalCollection
	{
		public ShipsAgencyPrincipalCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public bool IsValidPrincipal(OrgHeader principal)
		{
			if (principal.OH_IsActive && principal.OH_IsShippingProvider)
			{
				foreach (OrgCompanyData data in principal.CompanyDataCollection)
				{
					if (data.OB_CRIsShipsAgencyPrincipal)
					{
						return true;
					}
				}
			}

			return false;
		}

		#region BusinessObjectCollection Overrides

		protected override ZQuery CreateAdditionalFilter()
		{
			ZDBOnlySubQuery principalQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			principalQuery.AddToFilter(OrgCompanyDataSchema.OB_CRIsShipsAgencyPrincipal, ZBool.True);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgHeader));
			result.AddToFilter(base.CreateAdditionalFilter());
			result.AddToFilter(OrgHeaderSchema.OH_IsShippingProvider, ZBool.True);
			result.AddSubQuery(principalQuery, JoinCondition.And);

			return result;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (!IsValidPrincipal((OrgHeader)selectedBusinessObject))
			{
				errors.Add(Res.GetString("c9c98745-c218-4939-8f35-c0857f8ba525", "A valid organization must be flagged as a principal."));
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			OrgHeader principal = (OrgHeader)child;
			base.SetDefaultsForNewChild(principal);
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject
			base.SetFilterBusinessObjectDefaults();
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.Principal));
		}

		#endregion
	}
}
