using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class ProductPermitCusSupportingValidation : Customs.Business.CusSupportingInfoValidation
	{
		public ProductPermitCusSupportingValidation(ProductPermitCusSupporting parent)
				: base(parent)
		{
		}

		public new ProductPermitCusSupporting Parent => (ProductPermitCusSupporting)base.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			if (Parent.CSI_ReferenceNumber.IsEmpty)
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.CSI_ReferenceNumberInfo.HumanReadableName));
			}
			else
			{
				if (Parent.CSI_ReferenceNumber.Length != 14)
				{
					Parent.CSI_ReferenceNumberInfo.AddWarning(Res.GetString("94105C1C-7F34-40EB-B449-5F6946746FE2", "{0} must consist 14 alphanumeric characters.", Parent.CSI_ReferenceNumberInfo.HumanReadableName));
				}

				if (Parent.Parent?.ProductPermitCusSupportingCollection?.Cast<ProductPermitCusSupporting>().Any(x => x.CSI_LineNo == Parent.CSI_LineNo && x.CSI_ReferenceNumber == Parent.CSI_ReferenceNumber && x.PK != Parent.PK) ?? false)
				{
					Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("515117C7-38B8-4600-B65E-B35AE19CE23B", "{0} and {1} combinations have been duplicated and must be unique.", Parent.CSI_ReferenceNumberInfo.HumanReadableName, Parent.CSI_LineNoInfo.HumanReadableName));
				}
			}
		}

		protected override void CheckCSI_LineNo()
		{
			base.CheckCSI_LineNo();

			if (!Parent.CSI_LineNo.IsEmpty)
			{
				if (Parent.Parent?.ProductPermitCusSupportingCollection?.Cast<ProductPermitCusSupporting>().Any(x => x.CSI_LineNo == Parent.CSI_LineNo && x.CSI_ReferenceNumber == Parent.CSI_ReferenceNumber && x.PK != Parent.PK) ?? false)
				{
					Parent.CSI_LineNoInfo.AddMessageError(Res.GetString("515117C7-38B8-4600-B65E-B35AE19CE23B", "{0} and {1} combinations have been duplicated and must be unique.", Parent.CSI_ReferenceNumberInfo.HumanReadableName, Parent.CSI_LineNoInfo.HumanReadableName));
				}
			}
		}
	}
}
