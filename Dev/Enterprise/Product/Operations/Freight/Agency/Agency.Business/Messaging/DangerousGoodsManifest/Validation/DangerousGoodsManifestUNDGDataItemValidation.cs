using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class DangerousGoodsManifestUNDGDataItemValidation : AutoUNDGDataItemValidation
	{
		public DangerousGoodsManifestUNDGDataItemValidation(AutoUNDGDataItem parent) : base(parent)
		{
		}

		new UNDGDataItem Parent => (UNDGDataItem)base.Parent;

		protected void CheckSubstancePK()
		{
			MandatoryValidation.CheckEntered(Parent.SubstancePKInfo);
			if (Parent.UNDGSubstance?.DG_UNNO.IsEmpty ?? false)
			{
				Parent.SubstancePKInfo.AddMessageError(Res.GetString("7BF91D11-E4F7-4641-A970-1FD37BF202AC", "Dangerous Goods UNDG Code is required."));
			}
		}

		#region DI_DG_ClassForBinding

		public void ValidateDI_DG_ClassForBinding()
		{
			ValidateCalculatedProperty(Parent.DI_DG_ClassForBindingInfo);
		}

		protected virtual void CheckDI_DG_ClassForBinding()
		{
			if (Parent.DI_DG_ClassForBinding.IsEmpty)
			{
				Parent.DI_DG_ClassForBindingInfo.AddMessageError(Res.GetString("F0864563-0A36-483D-BC55-2B1DE8E49516", "Dangerous Goods IMO Class is required."));
			}
		}

		#endregion

		protected override void CheckDI_DGFlashPoint()
		{
			base.CheckDI_DGFlashPoint();

			if ((Parent.DI_DG_ClassForBinding == "3" || (Parent.UNDGSubstance?.DG_SubLabel1 ?? ZString.Empty) == "3") && Parent.DI_DGFlashPoint.IsEmpty)
			{
				Parent.DI_DGFlashPointInfo.AddMessageError(Res.GetString("8456ED17-EAB6-4F2A-9401-412C4E25D8E0", "Dangerous Goods Shipment flash point is required for all class 3 and Sub Label 3 cargo with a UNDG Number."));
			}
		}

		protected override void CheckDI_PackageCount()
		{
			base.CheckDI_PackageCount();

			if (Parent.DI_PackageCount.IsEmpty)
			{
				Parent.DI_PackageCountInfo.AddMessageError(Res.GetString("80830DEF-59A0-4270-8331-194D5A74EBE1", "Dangerous Goods package count is required."));
			}
		}

		protected override void CheckDI_DGWeight()
		{
			base.CheckDI_DGWeight();

			if (Parent.DI_DGWeight.IsEmpty)
			{
				Parent.DI_DGWeightInfo.AddMessageError(Res.GetString("89FF640E-C49D-40A8-9DA9-D4D2C1257FD1", "Dangerous Goods weight is required."));
			}
		}

		protected override void CheckDI_TechnicalName()
		{
			base.CheckDI_TechnicalName();

			if (Parent.DI_TechnicalName.IsEmpty)
			{
				if (!Parent.UNDGSubstance?.DG_UNNO.IsEmpty ?? false)
				{
					var code = Parent.UNDGSubstance.DG_UNNO;
					Parent.DI_TechnicalNameInfo.AddMessageError(Res.GetString("F4CADE51-4F05-4F2C-BA1E-5D87AFCC40DF", "Technical Name is required for Dangerous Goods code {0}", code));
				}
				else
				{
					Parent.DI_TechnicalNameInfo.AddMessageError(Res.GetString("E1486183-B12B-4529-B592-C00D541D5122", "Technical Name is required"));
				}
			}
		}
	}
}
