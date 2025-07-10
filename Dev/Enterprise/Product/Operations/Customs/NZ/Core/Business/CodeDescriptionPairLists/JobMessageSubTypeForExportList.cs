using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class JobMessageSubTypeForExportList : CodeDescriptionPairList
	{
		public JobMessageSubTypeForExportList()
		{
			AddPair(JobMessageSubTypeList.Codes.Normal, (JobMessageSubTypeList.Descriptions.Normal));
			AddPair(JobMessageSubTypeList.Codes.Drawback, (JobMessageSubTypeList.Descriptions.Drawback));
			AddPair(JobMessageSubTypeList.Codes.Completion, (JobMessageSubTypeList.Descriptions.Completion));
			AddPair(JobMessageSubTypeList.Codes.WriteOff, (JobMessageSubTypeList.Descriptions.WriteOff));
		}
	}
}
