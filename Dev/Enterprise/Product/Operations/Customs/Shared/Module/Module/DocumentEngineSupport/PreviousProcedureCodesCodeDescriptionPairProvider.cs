using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	class PreviousProcedureCodesCodeDescriptionPairProvider : CodeDescriptionPairList,
			Integration.Customs.Shared.IPreviousProcedureCodesCodeDescriptionPairProvider,
			DocumentEngine.RuntimeOptions.IDependenceCodeDescriptionPairListProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return GetCodeDescriptionPairListByProceduresCode(string.Empty);
		}

		#endregion

		#region IDependenceCodeDescriptionPairListProvider Members

		public ReadOnlyCodeDescriptionPairList GetDependenceCodeDescriptionPairList(string proceduresCode)
		{
			return GetCodeDescriptionPairListByProceduresCode(proceduresCode);
		}

		#endregion

		#region Implementation

		readonly Dictionary<string, ReadOnlyCodeDescriptionPairList> fCodeDescriptionPairList = new Dictionary<string, ReadOnlyCodeDescriptionPairList>();

		ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairListByProceduresCode(string proceduresCode)
		{
			proceduresCode = proceduresCode ?? string.Empty;

			if (!fCodeDescriptionPairList.ContainsKey(proceduresCode))
			{
				var list = new CodeDescriptionPairList();
				var collection = proceduresCode.Length == 0 ? PreviousProcedureCodes : PreviousProcedureCodes.Where(x => x.ZZ6_ProcedureCode == proceduresCode);
				foreach (var cpc in collection)
				{
					list.AddPair(cpc.ZZ6_PreviousProcedureCode, cpc.ZZ6_Description);
				}
				list.Sort();
				fCodeDescriptionPairList[proceduresCode] = list;
			}

			return fCodeDescriptionPairList[proceduresCode];
		}

		RefCusProcedure[] PreviousProcedureCodes
		{
			get
			{
				if (fPreviousProcedureCodes == null)
				{
					var query = RefCusProcedure.Loader.GetFilter(GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today);
					query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, SQLComparisonOperator.NotEqual, string.Empty);
					fPreviousProcedureCodes = new BusinessObjectFactory().Load<RefCusProcedure>(query);
				}
				return fPreviousProcedureCodes;
			}
		}
		RefCusProcedure[] fPreviousProcedureCodes;

		#endregion
	}
}
