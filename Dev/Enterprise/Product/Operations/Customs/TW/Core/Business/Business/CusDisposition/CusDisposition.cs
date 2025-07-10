using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TW.Business
{
	public class CusDisposition : Customs.Business.CusDisposition
	{
		public CusDisposition(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusDisposition|CDI_StatusKey", Caption = "Type")]
		public override ZString CDI_StatusKey { get => base.CDI_StatusKey; set => base.CDI_StatusKey = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusDisposition|CDI_Status", Caption = "Status")]
		public override ZString CDI_Status { get => base.CDI_Status; set => base.CDI_Status = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusDisposition|StatusDescription", Caption = "Status Description", MediumCaption = "Status Desc.", ShortCaption = "Desc.")]
		public new ZString StatusDescription
		{
			get
			{
				string description;
				switch (CDI_StatusKey)
				{
					case CusDispositionStatusKeyList.Codes.RFM:
						description = Factory.GetCachedValue<CTP_017_ErrorDocumentsOrRequiredFormalities>().GetDescriptionFromCode(CDI_Status);
						break;
					case CusDispositionStatusKeyList.Codes.ARM:
						description = Factory.GetCachedValue<CPT_016_RejectionReasons>().GetDescriptionFromCode(CDI_Status);
						break;
					case CusDispositionStatusKeyList.Codes.CLR:
						description = Factory.GetCachedValue<CPT_025_ExtraCondition>().GetDescriptionFromCode(CDI_Status);
						break;
					default:
						description = ZString.Empty;
						break;
				}
				return description;
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusDisposition|CDI_StatusDate", Caption = "Status Date", ShortCaption = "Date")]
		public override ZDateTime CDI_StatusDate { get => base.CDI_StatusDate; set => base.CDI_StatusDate = value; }
	}
}
