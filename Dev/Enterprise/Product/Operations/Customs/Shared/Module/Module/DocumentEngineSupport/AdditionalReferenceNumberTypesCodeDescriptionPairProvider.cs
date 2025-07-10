using CargoWise.Integration;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	class AdditionalReferenceNumberTypesCodeDescriptionPairProvider : CodeDescriptionPairList,
			Integration.Customs.Shared.IAdditionalReferenceNumberTypesCodeDescriptionPairProvider,
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var result = new CodeDescriptionPairList();
			foreach (ICodeDescription item in CusEntryNumLookups.GetAdditionalReferenceNumberTypes(GlbCompany.CurrentCompany.Country.Code))
			{
				result.AddPairIfNotExist(item.Code, item.Description);
			}
			return result;
		}

		#endregion
	}
}
