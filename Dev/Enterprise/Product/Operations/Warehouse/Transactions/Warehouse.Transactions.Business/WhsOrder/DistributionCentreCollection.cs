using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class DistributionCentreCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public DistributionCentreCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region CreateAdditionalFilter

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			if (!AllowOtherOrgTypes)
			{
				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderSchema.OH_IsMiscFreightServices, true);
				query.AddToFilter(OrgHeaderSchema.OH_IsDistributionCentre, true);

				result.AddToFilter(query);
			}
			return result;
		}

		#endregion

		#region SetFilterBusinessObjectDefaults

		protected override void SetFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.DistributionCentre));
		}

		#endregion

		#region SetDefaultsForNewChild

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var orgHeader = (OrgHeader)child;
			orgHeader.OH_IsMiscFreightServices = true;
			orgHeader.OH_IsDistributionCentre = true;
		}

		#endregion

		#region AddNotificationWhenAdditionalFilterNotMet

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			var orgHeader = ((OrgHeader)selectedBusinessObject);
			if (!(orgHeader.OH_IsMiscFreightServices && orgHeader.OH_IsDistributionCentre))
			{
				errors.Add(Res.GetString("7832afb6-2147-4d6c-a489-2b04927a33d0",
					"An Organization selected from here must have an Organization type of Distribution Center selected under Services tab."));
			}
		}

		#endregion
	}
}
