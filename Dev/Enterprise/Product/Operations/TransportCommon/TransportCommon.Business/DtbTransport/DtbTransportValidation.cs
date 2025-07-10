using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportValidation : DtbBookingValidation
	{
		protected DtbTransportValidation(DtbTransport parent)
			: base(parent)
		{
		}

		protected new DtbTransport Parent
		{
			get { return (DtbTransport)base.Parent; }
		}

		#region CheckKM_KT_NKBookingTemplate

		protected override void CheckKM_KT_NKBookingTemplate()
		{
			base.CheckKM_KT_NKBookingTemplate();
			ListValidation.ErrorIfInvalidCode(Parent.KM_KT_NKBookingTemplateInfo);
		}

		#endregion

		#region CheckKM_IsHazardous

		protected override void CheckKM_IsHazardous()
		{
			base.CheckKM_IsHazardous();

			if (!Parent.KM_IsHazardousInfo.HasErrors())
			{
				var isBookingContainsHazardousPackages = Parent.IsAnyPackageHazardous;
				if (Parent.KM_IsHazardous && !isBookingContainsHazardousPackages)
				{
					Parent.KM_IsHazardousInfo.AddError(Res.GetString("DtbTransportValidation|NoPackagesWithDGs", "No packages with Dangerous Goods have been assigned to Instructions."));
				}
				else if (!Parent.KM_IsHazardous && isBookingContainsHazardousPackages)
				{
					Parent.KM_IsHazardousInfo.AddError(Res.GetString("DtbTransportValidation|HavePackagesWithDGs", "There are packages with Dangerous Goods assigned to Instructions. Is Hazardous needs to be checked."));
				}
			}
		}

		#endregion

		#region CheckKM_RequiresRefrigeration

		protected override void CheckKM_RequiresRefrigeration()
		{
			base.CheckKM_RequiresRefrigeration();

			if (!Parent.KM_RequiresRefrigerationInfo.HasErrors())
			{
				var requiresRefrigerationFromPackages = Parent.IsAnyPackageRequiresRefridgeration;
				if (Parent.KM_RequiresRefrigeration && !requiresRefrigerationFromPackages)
				{
					Parent.KM_RequiresRefrigerationInfo.AddError(Res.GetString("DtbTransportValidation|NoPackagesWithRefrigeration", "No packages that Require Refrigeration have been assigned to Instructions."));
				}
				else if (!Parent.KM_RequiresRefrigeration && requiresRefrigerationFromPackages)
				{
					Parent.KM_RequiresRefrigerationInfo.AddError(Res.GetString("DtbTransportValidation|HavePackagesWithRefrigeration", "There are packages that Require Refrigeration assigned to Instructions. Require Refrigeration need to be checked."));
				}
			}
		}

		#endregion

		#region CheckKM_RS_NKServiceLevel

		protected override void CheckKM_RS_NKServiceLevel()
		{
			base.CheckKM_RS_NKServiceLevel();
			ListValidation.ErrorIfInvalidCode(Parent.KM_RS_NKServiceLevelInfo);
		}

		#endregion
	}
}
