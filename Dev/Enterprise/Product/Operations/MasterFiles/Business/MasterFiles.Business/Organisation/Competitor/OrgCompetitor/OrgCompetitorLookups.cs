//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCompetitorLookups
//
//    This class should be used for overriding collections in AutoOrgCompetitorLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCompetitorLookups : AutoOrgCompetitorLookups
	{
		public OrgCompetitorLookups(AutoOrgCompetitor parent) : base(parent)
		{
		}

		public ReadOnlyCodeDescriptionPairList ActiveCompetitorTypes => OrganisationsDataRegistry.Instance.CompetitorType.Value.GetActiveCodeDescriptionPairList();

		public ReadOnlyCodeDescriptionPairList AllCompetitorTypes => OrganisationsDataRegistry.Instance.CompetitorType.Value.GetCodeDescriptionPairList();

		public OrganisationsFindBoxCollection Organisations
		{
			get
			{
				var competitorType = ((OrgCompetitor)Parent).OCP_Type;
				if (!lookupsDictionary.ContainsKey(competitorType))
				{
					ZQuery filter;
					Action<OrganisationsFindBoxCollection> setDefaults;

					switch (competitorType)
					{
						case CompetitorTypeList.Codes.Customs:
							filter = CustomsFilter();
							setDefaults = SetFindBoxDefaultsForCustoms;
							break;

						case CompetitorTypeList.Codes.Forwarding:
							filter = ForwarderFilter();
							setDefaults = SetFindBoxDefaultsForForwarder;
							break;

						case CompetitorTypeList.Codes.LandTransport:
							filter = LocalTransportFilter();
							setDefaults = SetFindBoxDefaultsForLocalTransport;
							break;

						case CompetitorTypeList.Codes.Warehouse:
							filter = WarehouseFilter();
							setDefaults = SetFindBoxDefaultsForWarehouse;
							break;

						default:
							filter = DefaultFilter();
							setDefaults = SetNoFindBoxDefaults;
							break;
					}

					lookupsDictionary[competitorType] = GetCollection(filter);
					setDefaults(lookupsDictionary[competitorType]);
				}

				return lookupsDictionary[competitorType];
			}
		}

		readonly Dictionary<ZString, OrganisationsFindBoxCollection> lookupsDictionary = new Dictionary<ZString, OrganisationsFindBoxCollection>();

		protected virtual OrganisationsFindBoxCollection GetCollection(ZQuery filter)
		{
			return new OrganisationsFindBoxCollection(Factory, filter);
		}

		protected virtual ZQuery CustomsFilter()
		{
			return new ZQuery(OrgHeaderSchema.OH_IsBroker, true);
		}

		protected virtual void SetFindBoxDefaultsForCustoms(OrganisationsFindBoxCollection collection)
		{
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property8", ZBool.True));
			collection.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("ce47b83b-d80c-4f96-b64c-17adaf15e656", "An Organization selected from here must have an Organization Type of Broker selected."));
		}

		protected virtual ZQuery ForwarderFilter()
		{
			return new ZQuery(OrgHeaderSchema.OH_IsForwarder, ZBool.True);
		}

		protected virtual void SetFindBoxDefaultsForForwarder(OrganisationsFindBoxCollection collection)
		{
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property5", ZBool.True));
			collection.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("1ea39bc8-df05-4a3f-a866-ade14cfb7187", "An Organization selected from here must have an Organization Type of Forwarder selected."));
		}

		protected virtual ZQuery LocalTransportFilter()
		{
			return new ZQuery(OrgHeaderSchema.OH_IsLocalTransport, true);
		}

		protected virtual void SetFindBoxDefaultsForLocalTransport(OrganisationsFindBoxCollection collection)
		{
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.LocalTransport));
			collection.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("fac9b575-8dfe-45f4-b9cf-2e93c4e17f6d", "An Organization selected from here must have an Organization Type of Road Transport selected."));
		}

		protected virtual ZQuery WarehouseFilter()
		{
			return new ZQuery(OrgHeaderSchema.OH_IsWarehouseClient, true);
		}

		protected virtual void SetFindBoxDefaultsForWarehouse(OrganisationsFindBoxCollection collection)
		{
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property7", ZBool.True));
			collection.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("8DDD85CB-1EBC-446D-9700-E66CBC99D413", "An Organization selected from here must have an Organization Type of Warehouse selected."));
		}

		protected virtual ZQuery DefaultFilter()
		{
			return new ZQuery();
		}

		protected virtual void SetNoFindBoxDefaults(OrganisationsFindBoxCollection collection)
		{
		}

		public CompanyLevelList CompanyLevelList
		{
			get { return companyLevelList ?? (companyLevelList = new CompanyLevelList()); }
		}
		CompanyLevelList companyLevelList;
	}
}

