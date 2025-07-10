using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.Integration.Compliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public class ComplianceSubTypeList : List<IComplianceSubType>, IComplianceSubTypeList
	{
		public ICodeDescriptionPairList GetDescriptionPairList()
		{
			var list = new CodeDescriptionPairList();
			ForEach(x => list.Add(new CodeDescriptionPair(x.Code.ToString(), x.Description().ToString())));

			return list;
		}
		public ICodeDescriptionPairList GetLocalDescriptionPairList()
		{
			var list = new CodeDescriptionPairList();
			ForEach(x => list.Add(new CodeDescriptionPair(x.Code.ToString(), x.LocalDescription())));

			return list;
		}
	}
}