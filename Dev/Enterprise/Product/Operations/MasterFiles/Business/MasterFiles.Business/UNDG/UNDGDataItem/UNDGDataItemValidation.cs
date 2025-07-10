using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGDataItemValidation : AutoUNDGDataItemValidation
	{
		public UNDGDataItemValidation(AutoUNDGDataItem parent)
			: base(parent)
		{
		}

		public new UNDGDataItem Parent
		{
			get { return (UNDGDataItem)base.Parent; }
		}

		#region Properties

		protected virtual bool ShouldValidateVolumeAndWeightWhenQuantityIsLimited => true;

		#endregion

		#region DI_DG

		protected virtual bool UNDGSubstanceIsRequired => true;

		public void ValidateSubstancePK()
		{
			ValidateCalculatedProperty(Parent.SubstancePKInfo);
		}

		protected virtual void CheckSubstancePK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.SubstancePKInfo, GetOverridenInvalidSubstanceError());
			var substance = Parent?.Substance;
			if (substance == null && UNDGSubstanceIsRequired && !Parent.IsEmptyItem)
			{
				Parent.SubstancePKInfo.AddError(Res.GetString("62d6f555-a081-404a-a311-b0495add3881", "Please enter a DG substance."));
			}
		}

		protected IMultilingualString GetOverridenInvalidSubstanceError()
		{
			return ListValidation.GetNotificationMessage(Parent.SubstancePKInfo);
		}

		#endregion

		#region DI_DG_ClassForBinding

		public void ValidateDI_DG_ClassForBinding()
		{
			ValidateCalculatedProperty(Parent.DI_DG_ClassForBindingInfo);
		}

		protected virtual void CheckDI_DG_ClassForBinding()
		{
		}

		#endregion

		#region DI_IMOClass

		protected override void CheckDI_IMOClass()
		{
			base.CheckDI_IMOClass();
			if (!Parent.IsAutoAddedItem)
			{
				ValidateDI_DG_DI_DG_NKSubsOrDI_IMOClassIsEntered(Parent.DI_IMOClassInfo);
				ListValidation.ErrorIfInvalidCode(Parent.DI_IMOClassInfo);
				if (!Parent.DI_IMOClass.IsEmpty && Parent.Substance != null && Parent.DI_IMOClass != Parent.Substance.DG_Class)
				{
					Parent.DI_IMOClassInfo.AddError(Res.GetString("f9e9df4d-5222-4db9-b83b-e9572faa4ae8", "DG Substance's UN Number does not match the DG Class of this substance"));
				}
			}
		}

		#endregion

		#region DI_OC_DGContact

		protected override void CheckDI_OC_DGContact()
		{
			base.CheckDI_OC_DGContact();
			ListValidation.ErrorIfInvalidPK(Parent.DI_OC_DGContactInfo);
		}

		#endregion

		#region DI_UnitOfVolume

		protected override void CheckDI_UnitOfVolume()
		{
			base.CheckDI_UnitOfVolume();
			if (Parent.DI_DGVolume > 0)
			{
				MandatoryValidation.CheckEntered(Parent.DI_UnitOfVolumeInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.DI_UnitOfVolumeInfo);
			ValidateDI_IsLimitedQuantity();
		}

		#endregion

		#region DI_UnitOfWeight

		protected override void CheckDI_UnitOfWeight()
		{
			base.CheckDI_UnitOfWeight();
			if (Parent.DI_DGWeight > 0)
			{
				MandatoryValidation.CheckEntered(Parent.DI_UnitOfWeightInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.DI_UnitOfWeightInfo);
			ValidateDI_IsLimitedQuantity();
		}

		#endregion

		#region DI_DGWeight

		protected override void CheckDI_DGWeight()
		{
			base.CheckDI_DGWeight();
			ValidateDI_IsLimitedQuantity();

			if (ShouldValidateVolumeAndWeightWhenQuantityIsLimited)
			{
				var isWeightLimited = Core.Constants.Weight.ContainsCode(Parent?.UNDGSubstance?.DG_LQMaxAmtUQ.ToUpperInvariant() ?? ZString.Empty);

				if (Parent.DI_IsLimitedQuantity && isWeightLimited)
				{
					var weightType = Parent?.UNDGSubstance?.DG_LQMaxAmtType ?? ZString.Empty;
					var isNetWeight = weightType == UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode;
					var isGrossWeight = weightType == UNDGSubstanceLookups.LimitedQuantityTypes.GLMCode;

					if (isNetWeight && Parent.DI_DGWeight.IsEmpty)
					{
						Parent.DI_DGWeightInfo.AddWarning(Res.GetString("01001712-c213-16a4-4bd1-c1e9674d19a2", "When transported in limited quantities, the weight is required for this substance by IATA DGR."));
					}
					else if (isGrossWeight)
					{
						Parent.DI_DGWeightInfo.AddWarning(Res.GetString("15d02c1b-d693-bb8f-4df0-164aad6e4bd9", "When transported in limited quantities, the gross weight is required for this substance by IATA DGR."));
					}
				}
			}
		}

		#endregion

		#region DI_DGVolume

		protected override void CheckDI_DGVolume()
		{
			base.CheckDI_DGVolume();
			ValidateDI_IsLimitedQuantity();

			if (ShouldValidateVolumeAndWeightWhenQuantityIsLimited)
			{
				var isVolumeLimited = Core.Constants.Volume.ContainsCode(Parent?.UNDGSubstance?.DG_LQMaxAmtUQ.ToUpperInvariant() ?? ZString.Empty);
				var weightType = Parent?.UNDGSubstance?.DG_LQMaxAmtType ?? ZString.Empty;
				var isNetWeight = weightType == UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode;

				if (Parent.DI_IsLimitedQuantity && isVolumeLimited && Parent.DI_DGVolume.IsEmpty && isNetWeight)
				{
					Parent.DI_DGVolumeInfo.AddWarning(Res.GetString("ca829ab3-82dd-eb8c-44f3-0c91dc552856", "When transported in limited quantities, the volume is required for this substance by IATA DGR."));
				}
			}
		}

		#endregion

		#region DI_IsLimitedQuantity

		protected override void CheckDI_IsLimitedQuantity()
		{
			base.CheckDI_IsLimitedQuantity();

			if (Parent.Substance != null)
			{
				if (Parent.Substance.DG_LQMaxAmt > 0)
				{
					if (Parent.DI_DGWeight > 0 && Core.Constants.Weight.ContainsCode(Parent.Substance.DG_LQMaxAmtUQ) && !Core.Constants.Weight.ContainsCode(Parent.DI_UnitOfWeight))
					{
						Parent.DI_IsLimitedQuantityInfo.AddWarning(Res.GetString("3650dfeb-e0a4-4657-9694-6ce008833777", "Substance {0} has a Limited Quantity Weight specified of {1} {2}, however the weight units entered on this line are not valid. Please specify weight units, or manually specify whether this line contains a Limited Quantity.", Parent.Substance.DG_Code, Parent.Substance.DG_LQMaxAmt, Parent.Substance.DG_LQMaxAmtUQ));
					}
					else if (Parent.DI_DGVolume > 0 && Core.Constants.Volume.ContainsCode(Parent.Substance.DG_LQMaxAmtUQ) && !Core.Constants.Volume.ContainsCode(Parent.DI_UnitOfVolume))
					{
						Parent.DI_IsLimitedQuantityInfo.AddWarning(Res.GetString("53e01df1-75e2-4c88-a004-c8ed940d940e", "Substance {0} has a Limited Quantity Volume specified of {1} {2}, however the volume units entered on this line are valid. Please specify volume units, or manually specify whether this line contains a Limited Quantity.", Parent.Substance.DG_Code, Parent.Substance.DG_LQMaxAmt, Parent.Substance.DG_LQMaxAmtUQ));
					}
				}

				if (!Parent.Substance.DG_LQSpecProvIndex.IsEmpty)
				{
					Parent.DI_IsLimitedQuantityInfo.AddWarning(Res.GetString("62c3de62-36f7-4bd0-9a08-eda6b9377697", "Substance {0} has a Special Provision specified for the handling of Limited Quantities. Please review this provision and manually specify whether this line contains a Limited Quantity. The provision number is {1} and states:\r\n\r\n{2}", Parent.Substance.DG_Code, Parent.Substance.DG_LQSpecProvIndex, Parent.Substance.LQSpecProvData));
				}
			}
		}

		#endregion

		#region DI_ApprovalCertificateType

		protected override void CheckDI_ApprovalCertificateType()
		{
			base.CheckDI_ApprovalCertificateType();

			if (!Parent.DI_ApprovalCertificateIDMark.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.DI_ApprovalCertificateTypeInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.DI_ApprovalCertificateTypeInfo);
		}

		#endregion

		#region DI_ApprovalCertificateIDMark

		protected override void CheckDI_ApprovalCertificateIDMark()
		{
			base.CheckDI_ApprovalCertificateIDMark();

			if (!Parent.DI_ApprovalCertificateType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.DI_ApprovalCertificateIDMarkInfo);
			}
		}

		#endregion

		#region DI_NECWeight

		protected override void CheckDI_NECWeight()
		{
			base.CheckDI_NECWeight();

			if (Parent.Substance != null)
			{
				if (Parent.IsClassOneDgSubstance)
				{
					var kgNECWeight = Core.Constants.Weight.Convert(Parent.DI_NECWeight, Parent.DI_NECWeightUQ, Core.Constants.Weight.Kilograms);
					var kgDGWeight = Core.Constants.Weight.Convert(Parent.DI_DGWeight, Parent.DI_UnitOfWeight, Core.Constants.Weight.Kilograms);
					if (Parent.DI_NECWeight.IsEmpty)
					{
						Parent.DI_NECWeightInfo.AddWarning(Res.GetString("625B28EB-C589-48E5-AED1-19E3C4FADB5A", "Net explosive content is required for Class 1 substances"));
					}
					if (kgNECWeight > kgDGWeight)
					{
						Parent.DI_NECWeightInfo.AddError(Res.GetString("D6FD88F8-32DC-4780-B4E8-BEA8CD923E8D", "Net Explosive Content must be less than, or equal to, the net mass of the Class 1 UN substance"));
					}
				}
			}
		}

		#endregion

		#region DI_NECWeightUQ

		protected override void CheckDI_NECWeightUQ()
		{
			base.CheckDI_NECWeightUQ();
			if (Parent.Substance != null)
			{
				if (Parent.IsClassOneDgSubstance)
				{
					MandatoryValidation.CheckUnitEntered(Parent.DI_NECWeightUQInfo, Parent.DI_NECWeightInfo);
				}
			}
		}

		#endregion

		#region CheckDI_F3_NKPackType

		protected override void CheckDI_F3_NKPackType()
		{
			base.CheckDI_F3_NKPackType();
			ListValidation.ErrorIfInvalidCode(Parent.DI_F3_NKPackTypeInfo);

			if (Parent.DI_PackageCount > 0)
			{
				MandatoryValidation.CheckEntered(Parent.DI_F3_NKPackTypeInfo);
			}
		}

		#endregion

		#region CheckDI_PackageCount

		protected override void CheckDI_PackageCount()
		{
			base.CheckDI_PackageCount();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.DI_PackageCountInfo, 0);
		}

		#endregion

		#region Implementation

		void ValidateDI_DG_DI_DG_NKSubsOrDI_IMOClassIsEntered(ZPropertyInfo info)
		{
			if (UNDGSubstanceIsRequired && Parent.DI_DG_NKSubs.IsEmpty && Parent.DI_DG.IsEmpty && Parent.SubstanceCode.IsEmpty && Parent.DI_IMOClass.IsEmpty)
			{
				info.AddError(Res.GetString("18c166ad-e539-4e2c-ae0d-73f83bb668ce", "At least one of {0}, {1} must be entered.", Parent.DI_DG_NKSubsInfo.HumanReadableName, Parent.DI_IMOClassInfo.HumanReadableName));
			}
		}

		#endregion
	}
}
