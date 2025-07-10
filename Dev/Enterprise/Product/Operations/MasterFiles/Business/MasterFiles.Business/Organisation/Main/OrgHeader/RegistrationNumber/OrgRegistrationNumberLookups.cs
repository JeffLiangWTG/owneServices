using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRegistrationNumberLookups : ZLookups
	{
		public OrgRegistrationNumberLookups(OrgRegistrationNumber parent)
			: base(parent)
		{
		}

		protected new OrgRegistrationNumber Parent
		{
			get { return (OrgRegistrationNumber)base.Parent; }
		}

		public OrgRegistrationNumberTypeList NumberTypes
		{
			get
			{
				var key = FormattableString.Invariant($"OrgRegistrationNumberTypeList_{Parent.Organization?.Country?.Code}_{Parent.Organization?.MainAddress?.Country?.Code}");
				return Parent.Factory.GetCachedValue(key, () => new OrgRegistrationNumberTypeList(new[] { Parent.Organization.Country, Parent.Organization?.MainAddress?.Country }));
			}
		}
	}
}
