

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class InwardMessageTypeList : CodeDescriptionPairList
	{
		public InwardMessageTypeList()
		{
			AddPair(MessageTypeCodeList.Codes.INP, MessageTypeCodeList.Descriptions.INP);
			AddPair(MessageTypeCodeList.Codes.IPT, MessageTypeCodeList.Descriptions.IPT);
		}
	}
}
