using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class JobMessageSubTypeForImportList : CodeDescriptionPairList
	{
		public JobMessageSubTypeForImportList(bool isTSWDeclaration)
		{
			AddPair(JobMessageSubTypeList.Codes.Normal, (JobMessageSubTypeList.Descriptions.Normal));
			AddPair(JobMessageSubTypeList.Codes.Simplified, (JobMessageSubTypeList.Descriptions.Simplified));
			AddPair(JobMessageSubTypeList.Codes.Temporary, (JobMessageSubTypeList.Descriptions.Temporary));
			AddPair(JobMessageSubTypeList.Codes.Sight, (JobMessageSubTypeList.Descriptions.Sight));
			AddPair(JobMessageSubTypeList.Codes.Periodic, (JobMessageSubTypeList.Descriptions.Periodic));
			AddPair(JobMessageSubTypeList.Codes.Completion, (JobMessageSubTypeList.Descriptions.Completion));
			AddPair(JobMessageSubTypeList.Codes.WriteOff, (JobMessageSubTypeList.Descriptions.WriteOff));
			if (isTSWDeclaration)
			{
				AddPair(MessageSubTypeCombinedList.Codes.IPI, MessageSubTypeCombinedList.Descriptions.IPI);
			}
		}
	}
}
