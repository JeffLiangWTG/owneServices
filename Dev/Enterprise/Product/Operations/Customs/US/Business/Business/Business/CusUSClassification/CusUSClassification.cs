using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CusUSClassification : AutoCusUSClassification, Integration.Customs.US.ICusUSClassification
	{
		public CusUSClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CusClassPartPivot Parent
		{
			get { return fParent ?? (fParent = Factory.Load<CusClassPartPivot>(this.CD_ParentID)); }
		}
		CusClassPartPivot fParent;

		public override ZString CD_DDTCIndicator
		{
			get { return base.CD_DDTCIndicator; }
			set
			{
				var oldValue = base.CD_DDTCIndicator;
				base.CD_DDTCIndicator = value;
				if (!IsCopying && oldValue != value)
				{
					ClearDDTCDataIfNeeded();
				}
			}
		}

		public override ZString CD_ADDCaseNo
		{
			get => base.CD_ADDCaseNo;
			set
			{
				var oldValue = base.CD_ADDCaseNo;
				if (oldValue != value)
				{
					base.CD_ADDCaseNo = value;
					if (!IsCopying)
					{
						Parent?.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString CD_CVDCaseNo
		{
			get => base.CD_CVDCaseNo;
			set
			{
				var oldValue = base.CD_CVDCaseNo;
				if (oldValue != value)
				{
					base.CD_CVDCaseNo = value;
					if (!IsCopying)
					{
						Parent?.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString CD_ADDDepositRateInd
		{
			get => base.CD_ADDDepositRateInd;
			set
			{
				var oldValue = base.CD_ADDDepositRateInd;
				if (oldValue != value)
				{
					base.CD_ADDDepositRateInd = value;
					if (!IsCopying)
					{
						Parent?.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString CD_CVDDepositRateInd
		{
			get => base.CD_CVDDepositRateInd;
			set
			{
				var oldValue = base.CD_CVDDepositRateInd;
				if (oldValue != value)
				{
					base.CD_CVDDepositRateInd = value;
					if (!IsCopying)
					{
						Parent?.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString CD_SPI
		{
			get { return base.CD_SPI; }
			set
			{
				var oldSpi = CD_SPI;
				base.CD_SPI = value;
				Parent?.LogIfPropertyValueChange("SPI", oldSpi, CD_SPI);
			}
		}

		void ClearDDTCDataIfNeeded()
		{
			if (CD_DDTCIndicator != OGAIndicatorList.Codes.Declared)
			{
				CD_ITARExemptionNo = ZString.Empty;
				CD_DDTCLicenceType = ZString.Empty;
				CD_DDTCRegoNo = ZString.Empty;
				CD_DDTCLicenceNo = ZString.Empty;
				CD_DDTCJurisdictionNumber = ZString.Empty;
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override CusUSClassificationValidation GetNewValidation()
		{
			return (Parent?.CI_ChildType ?? ZString.Empty) == ClassificationTypeList.Codes.HTI ? new HTICusUSClassificationValidation(this) : base.GetNewValidation();
		}
	}
}
