using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class PreviousBondedCusSupporting : SingleCusSupportingInfo
	{
		public PreviousBondedCusSupporting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new PreviousBondedCusSupportingValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.PreviousBondedEntryNumber;
		}

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;

		[ResourceStringData("Enterprise.Customs.TW.Business.PreviousBondedCusSupporting|CSI_ReferenceNumber", Caption = "Previous Bonded Entry Number ", ShortCaption = "Pre. Bonded Entry NO")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set
			{
				var oldValue = CSI_ReferenceNumber;
				base.CSI_ReferenceNumber = value;
				if (!IsCopying && oldValue != CSI_ReferenceNumber)
				{
					Parent?.MarkAsNeedingValidation();
					Validation.ValidateCSI_LineNo();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.PreviousBondedCusSupporting|CSI_LineNo", Caption = "Previous Bonded Entry Line Number", ShortCaption = "Pre. Bonded Entry LNO")]
		public override ZInt CSI_LineNo
		{
			get => base.CSI_LineNo;
			set
			{
				var oldValue = CSI_LineNo;
				base.CSI_LineNo = value;
				if (!IsCopying && oldValue != CSI_LineNo)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		public override System.Collections.Generic.IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return CSI_ReferenceNumberInfo;
			yield return CSI_LineNoInfo;
		}
	}
}
