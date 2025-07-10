using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.NZ.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors
{
	class CompanyFinder
	{
		public CompanyFinder()
		{
			var nzCompanies = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.NewZealand).OrderBy(x => x.GC_Code);
			foreach (GlbCompany company in nzCompanies)
			{
				string brokerageID = NZCustomsDataRegistry.Instance.NZBrokerageID.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (!string.IsNullOrEmpty(brokerageID) && !companies.ContainsKey(brokerageID))
				{
					companies.Add(brokerageID, company);
				}
			}
		}

		readonly Dictionary<string, GlbCompany> companies = new Dictionary<string, GlbCompany>();

		public GlbCompany FindFromBrokerageID(string brokerageID)
		{
			GlbCompany result;
			if (companies.TryGetValue(brokerageID, out result))
			{
				return result;
			}
			return null;
		}
	}
}
