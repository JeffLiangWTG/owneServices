using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsManifestsToOpenValidation : Customs.Business.CusSupportingInfoValidation
	{
		public NctsManifestsToOpenValidation(NctsManifestsToOpen parent)
			: base(parent)
		{
		}

		new NctsManifestsToOpen Parent => (NctsManifestsToOpen)base.Parent;

		protected override void CheckCSI_LineNo()
		{
			base.CheckCSI_LineNo();
			if (!Parent.BillLineNoAndQuantityReadOnly)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.CSI_LineNoInfo);
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		}

		protected override void CheckCSI_CustomsOffice()
		{
			base.CheckCSI_CustomsOffice();
			if (Parent.AtWarehouse)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_CustomsOfficeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CustomsOfficeInfo);
		}

		protected override void CheckCSI_Quantity3()
		{
			base.CheckCSI_Quantity3();
			if (!Parent.BillLineNoAndQuantityReadOnly)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.CSI_Quantity3Info);
				MandatoryValidation.CheckNotNegative(Parent.CSI_Quantity3Info);

				if (Parent.CSI_Quantity3 > int.MaxValue)
				{
					Parent.CSI_Quantity3Info.AddError(ZString.Format((NoResString)"The maximum quantity should be {0}.", int.MaxValue));
				}
			}
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumber2Info);
		}
	}
}
