using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.TradeSingleWindow
{
	public static class AttachmentTypeListPerMsg
	{
		public class AttachmentTypeListForCRE : CodeDescriptionPairList
		{
			public AttachmentTypeListForCRE()
			{
				AddPair(AttachmentTypeList.Codes.OTH, AttachmentTypeList.Descriptions.OTH);
			}
		}

		public class AttachmentTypeListForIM1 : CodeDescriptionPairList
		{
			public AttachmentTypeListForIM1()
			{
				AddPair(AttachmentTypeList.Codes.PER, AttachmentTypeList.Descriptions.PER);
				AddPair(AttachmentTypeList.Codes.CER, AttachmentTypeList.Descriptions.CER);
				AddPair(AttachmentTypeList.Codes.INV, AttachmentTypeList.Descriptions.INV);
				AddPair(AttachmentTypeList.Codes.PAC, AttachmentTypeList.Descriptions.PAC);
				AddPair(AttachmentTypeList.Codes.BOL, AttachmentTypeList.Descriptions.BOL);
				AddPair(AttachmentTypeList.Codes.CQD, AttachmentTypeList.Descriptions.CQD);
				AddPair(AttachmentTypeList.Codes.UBD, AttachmentTypeList.Descriptions.UBD);
				AddPair(AttachmentTypeList.Codes.PP, AttachmentTypeList.Descriptions.PP);
				AddPair(AttachmentTypeList.Codes.OTH, AttachmentTypeList.Descriptions.OTH);
			}
		}

		public class AttachmentTypeListForEX1 : CodeDescriptionPairList
		{
			public AttachmentTypeListForEX1()
			{
				AddPair(AttachmentTypeList.Codes.PER, AttachmentTypeList.Descriptions.PER);
				AddPair(AttachmentTypeList.Codes.CER, AttachmentTypeList.Descriptions.CER);
				AddPair(AttachmentTypeList.Codes.INV, AttachmentTypeList.Descriptions.INV);
				AddPair(AttachmentTypeList.Codes.PAC, AttachmentTypeList.Descriptions.PAC);
				AddPair(AttachmentTypeList.Codes.BOL, AttachmentTypeList.Descriptions.BOL);
				AddPair(AttachmentTypeList.Codes.OTH, AttachmentTypeList.Descriptions.OTH);
			}
		}

		public class AttachmentTypeListForOCR : CodeDescriptionPairList
		{
			public AttachmentTypeListForOCR()
			{
				AddPair(AttachmentTypeList.Codes.OTH, AttachmentTypeList.Descriptions.OTH);
			}
		}
	}
}
