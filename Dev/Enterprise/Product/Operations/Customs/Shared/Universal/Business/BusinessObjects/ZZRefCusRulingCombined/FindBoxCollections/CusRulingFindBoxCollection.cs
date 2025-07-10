using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class CusRulingFindBoxCollection : ZZRefCusRulingCombinedCollection
	{
		public CusRulingFindBoxCollection(BusinessObjectFactory factory, ZString rulingNumber)
			: base(factory)
		{
			this.rulingNumber = rulingNumber;
			AdditionalFilter = GetAdditionalFilter();
			SetFilterDefaults();
		}

		public CusRulingFindBoxCollection(BusinessObjectFactory factory, ZString rulingNumber, OrgHeader org, IEnumerable<ZGuid> validOrganizations)
			: this(factory, rulingNumber)
		{
			this.rulingNumber = rulingNumber;
			this.org = org;
			this.validOrganizations = validOrganizations;
			AdditionalFilter = GetAdditionalFilter();
			SetFilterDefaults();
		}

		public CusRulingFindBoxCollection(BusinessObjectFactory factory, ZString rulingType, ZString rulingNumber, OrgHeader org, IEnumerable<ZGuid> validOrganizations)
			: this(factory, rulingNumber, org, validOrganizations)
		{
			this.rulingType = rulingType;
			this.rulingNumber = rulingNumber;
			this.org = org;
			this.validOrganizations = validOrganizations;
			AdditionalFilter = GetAdditionalFilter();
			SetFilterDefaults();
		}

		readonly OrgHeader org;
		readonly ZString rulingType;
		readonly ZString rulingNumber;
		readonly IEnumerable<ZGuid> validOrganizations;

		protected override bool AllowNew => false;

		ZQuery GetAdditionalFilter()
		{
			var result = new ZDBOnlyQuery(typeof(ZZRefCusRulingCombined));
			var orgLookups = validOrganizations ?? (org != null ? new[] { org.PK } : Array.Empty<ZGuid>());

			if (orgLookups.Any())
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, orgLookups);

				result.AddSubQuery(ZZRefCusRulingCombinedSchema.ZZX_OA_AppliesTo, orgAddressQuery, JoinCondition.And);
				result.AddToFilter(JoinCondition.Or, ZZRefCusRulingCombinedSchema.ZZX_OA_AppliesTo, DBNull.Value);
			}
			return result;
		}

		void SetFilterDefaults()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusRulingFilters.AppliesToOrg, "Property", org != null ? org.PK : ZGuid.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusRulingFilters.RulingNumber, "Property", rulingNumber));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusRulingFilters.RulingType, "Property", rulingType, rulingType == ZString.Empty));
		}

		protected override void SetDefaultsForNewElementCore(ZZRefCusRulingCombined refCusRuling)
		{
			base.SetDefaultsForNewElementCore(refCusRuling);
			if (org != null)
			{
				refCusRuling.ZZX_OA_AppliesTo = org.MainAddress.PK;
			}
			else if (validOrganizations != null && validOrganizations.Any())
			{
				var firstValidOrg = Factory.Load<OrgHeader>(validOrganizations.First());
				if (firstValidOrg != null)
				{
					refCusRuling.ZZX_OA_AppliesTo = firstValidOrg.MainAddress.PK;
				}
			}
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			var selectedCusRuling = selectedBusinessObject as ZZRefCusRulingCombined;
			var orgLookups = validOrganizations ?? (org != null ? new[] { org.PK } : Array.Empty<ZGuid>());

			if (selectedCusRuling != null && orgLookups.Any())
			{
				var orgId = selectedCusRuling.AppliesToAddress?.OA_OH;
				if (orgId != null && !orgId.Value.IsEmpty && !orgLookups.Contains(orgId.Value))
				{
					errors.Add(Res.GetString("d06ee1a2-51b8-4538-95ef-c4632fdcb3d0", "This ruling is associated with another organization."));
				}
			}
		}
	}
}
