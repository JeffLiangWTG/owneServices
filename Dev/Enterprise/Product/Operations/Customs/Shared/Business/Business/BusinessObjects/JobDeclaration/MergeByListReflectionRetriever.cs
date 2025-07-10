using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	public class MergeByListReflectionRetriever : IMergeByListReflectionRetriever
	{
		public ICodeDescriptionPairList GetMergeByListByCountryCode(string countryCode)
		{
			var list = new CodeDescriptionPairList();
			if (!string.IsNullOrEmpty(countryCode))
			{
				var glbCompany = GlbCompany.GetActiveCompanies(company => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(company.GC_RN_NKCountryCode) == countryCode)?.FirstOrDefault();
				AddMergeByListByCompany(list, glbCompany);
			}
			return list;
		}

		public ICodeDescriptionPairList GetMergeByListByCompanyCode(string companyCode)
		{
			var list = new CodeDescriptionPairList();
			if (!string.IsNullOrEmpty(companyCode))
			{
				var glbCompany = GlbCompany.GetActiveCompanies(company => company.GC_Code == companyCode)?.FirstOrDefault();
				AddMergeByListByCompany(list, glbCompany);
			}
			return list;
		}

		void AddMergeByListByCompany(CodeDescriptionPairList list, GlbCompany glbCompany)
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			if (currentCountryCode == (glbCompany?.GC_RN_NKCountryCode ?? ZString.Empty))
			{
				AddMergeByList(list);
			}
			else
			{
				var branch = glbCompany?.FirstActiveBranch;
				if (branch != null)
				{
					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						AddMergeByList(list);
					}
				}
			}
		}

		void AddMergeByList(CodeDescriptionPairList list)
		{
			var declaration = new BusinessObjectFactory().GetNull<BaseJobDeclaration>();
			var mergeByList = declaration.Lookups.MergeByList;
			if (mergeByList.Count > 0)
			{
				list.AddRange(mergeByList);
			}
		}
	}
}
