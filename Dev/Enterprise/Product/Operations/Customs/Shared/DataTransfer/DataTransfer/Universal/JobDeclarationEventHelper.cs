using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public static class JobDeclarationEventHelper
	{
		public static ZQuery GetJobDeclarationQueryFor(BusinessObjectFactory factory, ZString declarationReference, ZString companyCode, ZString countryCode)
		{
			var declaration = GetJobDeclarationFor(factory, declarationReference, companyCode, countryCode);
			return declaration == null ? null : new ZQuery(JobDeclarationSchema.PK, declaration.PK);
		}

		public static BaseJobDeclaration GetJobDeclarationFor(BusinessObjectFactory factory, ZString declarationReference, ZString companyCode, ZString countryCode)
		{
			BaseJobDeclaration result = null;
			if (!declarationReference.IsEmpty && factory != null)
			{
				var declarations = factory.Load<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, declarationReference));
				if (declarations.Length == 1)
				{
					result = declarations[0];
				}
				else if (declarations.Length > 1)
				{
					result = GetDeclarationForCompany(companyCode, declarations);
					if (result == null && !countryCode.IsEmpty)
					{
						result = GetDeclarationForCountry(countryCode, declarations);
					}
					if (result == null && companyCode != GlbCompany.CurrentCompany.GC_Code)
					{
						result = GetDeclarationForCompany(GlbCompany.CurrentCompany.GC_Code, declarations);
					}
				}
			}
			return result;
		}

		static BaseJobDeclaration GetDeclarationForCompany(ZString companyCode, BaseJobDeclaration[] declarations)
		{
			return companyCode.IsEmpty ? null : GetDeclarationForCompany(x => (x?.GC_Code ?? ZString.Empty) == companyCode, declarations);
		}

		static BaseJobDeclaration GetDeclarationForCountry(ZString countryCode, BaseJobDeclaration[] declarations)
		{
			return countryCode.IsEmpty ? null : GetDeclarationForCompany(x => (x?.GC_RN_NKCountryCode ?? ZString.Empty) == countryCode, declarations);
		}

		static BaseJobDeclaration GetDeclarationForCompany(Func<GlbCompany, bool> matchCompany, BaseJobDeclaration[] declarations)
		{
			BaseJobDeclaration result = null;
			if (matchCompany != null)
			{
				var matchedCompanyDeclartions = declarations.Where(x => matchCompany(x.Company)).Take(2).ToArray();
				if (matchedCompanyDeclartions.Length == 1)
				{
					result = matchedCompanyDeclartions[0];
				}
			}
			return result;
		}
	}
}
