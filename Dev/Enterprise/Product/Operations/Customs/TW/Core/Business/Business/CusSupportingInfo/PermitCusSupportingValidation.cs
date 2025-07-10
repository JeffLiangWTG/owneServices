using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class PermitCusSupportingValidation : Customs.Business.CusSupportingInfoValidation
	{
		public PermitCusSupportingValidation(PermitCusSupporting parent) : base(parent)
		{
		}

		public new PermitCusSupporting Parent => (PermitCusSupporting)base.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var permitNumber = Parent.CSI_ReferenceNumber;
			var targetInfo = Parent.CSI_ReferenceNumberInfo;
			var lineNo = Parent.CSI_LineNo;

			if (!lineNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}

			if (!permitNumber.IsEmpty)
			{
				if (!permitNumber.IsLettersAndNumbersOnlyOrEmpty || permitNumber.Length != 14)
				{
					targetInfo.AddMessageError(Res.GetString("CDEEC2A2-DDAE-4FAF-A425-C1A1F21DD740", "{0} must consist 14 alphanumeric characters.", targetInfo.HumanReadableName));
				}

				var invoiceLine = Parent.Parent;
				ValidationHelper.CheckPermitNumbersPlusSpecialCodeNumber(invoiceLine, targetInfo);
			}
		}

		protected override void CheckCSI_LineNo()
		{
			base.CheckCSI_LineNo();
			var lineNo = Parent.CSI_LineNo;
			var referenceNo = Parent.CSI_ReferenceNumber;
			var targetInfo = Parent.CSI_LineNoInfo;

			if (!referenceNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}

			if (!lineNo.IsEmpty)
			{
				if (lineNo > 9999)
				{
					targetInfo.AddError(Res.GetString("7A051A69-15FE-4F7F-B2F2-4717045E3D72", "{0} cannot enter value more than 4 length.", targetInfo.HumanReadableName));
				}

				if (Parent.Parent.PermitCusSupportingCollection?.Cast<PermitCusSupporting>().Any(x => x.CSI_LineNo == lineNo && x.CSI_ReferenceNumber == referenceNo && x.PK != Parent.PK) ?? false)
				{
					targetInfo.AddMessageError(Res.GetString("FEDE968E-1448-424D-83CB-153A1CB0734F", "Permit Number and Permit Item Number combinations have been duplicated and must be unique."));
				}
			}
		}
	}
}
