using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Web.Auth;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	class AuthorizationHelperForTest : AuthorizationHelper
	{
		readonly IEnumerable<RefUserAuthorization> refUserAuthorizations;
		public AuthorizationHelperForTest(IEnumerable<RefUserAuthorization> refUserAuthorizations)
		{
			this.refUserAuthorizations = refUserAuthorizations;
		}

		protected override IEnumerable<RefUserAuthorization> GetAuthorizationData(string user, Type type)
		{
			return refUserAuthorizations.Where(x => x.UA_User == user && (x.UA_TableName == type.Name || x.UA_TableName == "*"));
		}
	}

	class RefCusCodeListUserView
	{
		public Guid ZZD_PK { get; set; }
		public string ZZD_Code { get; set; }
		public string ZZD_CodeType { get; set; }
		public string ZZD_CountryOrGrouping { get; set; }
		public bool ZZD_IsEditable { get; set; }
	}

	class RefExchangeRateZZ
	{
		public Guid ZZN_PK { get; set; }
		public string ZZN_RateType { get; set; }
		public bool ZZN_IsEditable { get; set; }
	}

	class RefCusTariff
	{
		public Guid ZZ1_PK { get; set; }
		public string ZZ1_TariffCode { get; set; }
		public bool ZZ1_IsEditable { get; set; }
	}
}
