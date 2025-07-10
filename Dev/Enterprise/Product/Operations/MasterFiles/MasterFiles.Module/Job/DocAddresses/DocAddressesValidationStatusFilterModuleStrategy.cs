using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class DocAddressesValidationStatusFilterModuleStrategy : FilterModuleStrategy
	{
		protected override IEnumerable<ModuleFilter> FiltersToAdd
		{
			get
			{
				// TODO: support multi module which implement IDocAddresses
				if (typeof(IWhsDocket).IsAssignableFrom(BizoType) && BizoType.Name.Equals(ControllerIDs.WhsOrder.Name))
				{
					var addressValidationStatus = new ModuleTextFilter("Address Validation Status", GetValidationStatusQuery, AddressValidationStatusList.AddressValidationStatuses)
					{
						Category = FilterCategories.StatusAndFlags,
						MultilingualDescription = ResString.GetMultilingualString("59A35446-08AF-4EA5-95C2-62B3DA6C9C3B", "Address Validation Status")
					};

					yield return addressValidationStatus;
				}
			}
		}

		ZQuery GetValidationStatusQuery(ZString value)
		{
			ZQuery query;

			var tableCode = BusinessObjectFactory.GetTableCodeFromType(BizoType);
			if (string.IsNullOrEmpty(tableCode))
			{
				query = new ZQuery();
			}
			else
			{
				var dbOnlyQuery = new ZDBOnlyQuery(BizoType);

				var subQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				subQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, tableCode);

				var queryForOverrideAddress = new ZQuery();
				queryForOverrideAddress.AddToFilter(JobDocAddressSchema.E2_AddressOverride, true);
				queryForOverrideAddress.AddToFilter(JobDocAddressSchema.E2_ValidationStatus, value);

				var queryForNotOverrideAddress = new ZDBOnlyQuery(typeof(JobDocAddress));
				queryForNotOverrideAddress.AddToFilter(JobDocAddressSchema.E2_AddressOverride, false);

				var subQueryForNotOverrideAddress = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
				subQueryForNotOverrideAddress.AddToFilter(OrgAddressSchema.OA_ValidationStatus, value);

				queryForNotOverrideAddress.AddSubQuery(subQueryForNotOverrideAddress, JoinCondition.And);

				queryForOverrideAddress.AddToFilter(queryForNotOverrideAddress, JoinCondition.Or);

				subQuery.AddToFilter(queryForOverrideAddress);

				dbOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
				query = dbOnlyQuery;
			}

			return query;
		}

		public override void RunOnFilterControlInitialisation(IFilterControl control, IBusinessObjectCollection gridCollection)
		{
		}
	}
}
