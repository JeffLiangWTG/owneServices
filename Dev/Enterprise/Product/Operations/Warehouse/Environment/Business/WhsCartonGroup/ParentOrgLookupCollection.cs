using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class ParentOrgLookupCollection : OrganisationsFindBoxCollection, IValidateForController
	{
		public ParentOrgLookupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region CreateAdditionalFilter

		protected override ZQuery CreateAdditionalFilter()
		{
			var addressWhsSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
			addressWhsSubQuery.AddSubQuery(new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsWarehouseSchema.WW_OA_WarehouseAddress), JoinCondition.And);

			var miscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH, notIn: true);
			miscServSubQuery.AddToFilter(OrgMiscServSchema.OM_WCG_CartonGroup, SQLComparisonOperator.NotEqual, null);

			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsShippingProvider, true);
			query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsConsignee, true);
			query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsWarehouseClient, true);
			query.AddSubQuery(addressWhsSubQuery, JoinCondition.Or);
			query.AddSubQuery(miscServSubQuery, JoinCondition.And);
			query.AddToFilter(base.CreateAdditionalFilter());

			return query;
		}

		#endregion

		#region SetFilterBusinessObjectDefaults

		protected override void SetFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property2", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property4", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property7", ZBool.True));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "AndJoinCondition", ZBool.False));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "OrJoinCondition", ZBool.True));
		}

		#endregion

		#region AddNotificationWhenAdditionalFilterNotMet

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			var orgHeader = ((OrgHeader)selectedBusinessObject);
			if (!orgHeader.OH_IsConsignee && !orgHeader.OH_IsWarehouseClient && !orgHeader.OH_IsShippingProvider && !AttachedToWarehouse(orgHeader))
			{
				errors.Add(Res.GetString("c725f899-8355-4b63-9299-865410448b51",
					"An Organization selected from here must have an Organization type of Carrier, Warehouse Client, Consignee or be linked to a Warehouse record."));
			}

			var cartonGroupPK = orgHeader.MiscServ.OM_WCG_CartonGroup;
			if (!cartonGroupPK.IsEmpty)
			{
				var cartonGroup = Factory.Load<WhsCartonGroup>(cartonGroupPK);
				errors.Add(Res.GetString("ed350b9a-16b9-454b-beb3-f1b18f0ca383",
					"This Organization is already linked to Carton Group {0}.", cartonGroup != null ? cartonGroup.WCG_Code : ZString.Empty));
			}
		}

		bool AttachedToWarehouse(OrgHeader orgHeader)
		{
			return Factory.LoadTop1<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, orgHeader.Addresses.Select(a => a.PK))) != null;
		}

		#endregion
	}
}

