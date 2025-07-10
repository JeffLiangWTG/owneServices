using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class JobMessageSubTypeForExciseList : CodeDescriptionPairList
	{
		public JobMessageSubTypeForExciseList()
		{
			AddPair(JobMessageSubTypeList.Codes.Excise, (JobMessageSubTypeList.Descriptions.Excise));
		}
	}
}
