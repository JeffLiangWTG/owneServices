using System.Globalization;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public static class N5101HFunctionalReferenceIdGenerator
	{
		const string dateFormat = "yyMMMdd";
		static OrgHeader CurrentCompanyOrgProxy => GlbCompany.CurrentCompany.OrgProxy;
		static ZString VAT => CurrentCompanyOrgProxy?.GetCustomsRegNo(OrgCusCode.CodeTypes.VATCode) ?? ZString.Empty;
		static ZString Date => ZDateTime.Today.ToString(dateFormat, CultureInfo.CurrentCulture).ToUpper();
		static ZString GetSequence(IDbConnected dbConnected) => Env.NumberFountains.GetTWN5101HFunctionalReferenceIdNumberFountain().GetNext(dbConnected).ToString("D5", CultureInfo.InvariantCulture);
		public static ZString GetFunctionalReferenceId(IDbConnected dbConnected) => $"{VAT}{Date}{GetSequence(dbConnected)}";
	}
}
