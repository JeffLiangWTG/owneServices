using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	class ProcedureCodesCodeDescriptionPairProvider : CodeDescriptionPairList,
			Integration.Customs.Shared.IProcedureCodesCodeDescriptionPairProvider,
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var result = new CodeDescriptionPairList();
			var cpcCollection = RefCusProcedureCollection.LoadCustomsProcedureCodesForCountry(new BusinessObjectFactory(), GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today);
			foreach (var cpc in cpcCollection)
			{
				result.AddPairIfNotExist(cpc.ZZ6_ProcedureCode, cpc.ZZ6_Description);
			}
			result.Sort();
			return result;
		}

		#endregion
	}
}
