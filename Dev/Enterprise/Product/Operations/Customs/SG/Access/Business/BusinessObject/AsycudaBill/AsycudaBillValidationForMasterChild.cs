using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.Access.Business
{
	public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidationForMasterChild
	{
		public AsycudaBillValidationForMasterChild(AsycudaBill parent)
			: base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;
		protected new AsycudaManifestHeader Header => Parent.Header;

		protected override void CheckABL_RL_NKPortOfDischarge()
		{
			base.CheckABL_RL_NKPortOfDischarge();
			if (!Parent.ABL_RL_NKPortOfDischarge.IsEmpty)
			{
				var portInfo = Parent.ABL_RL_NKPortOfDischargeInfo;
				if (!portInfo.HasErrors())
				{
					var header = Header;
					if (header != null)
					{
						if (header.IsExport)
						{
							if (Parent.ABL_RL_NKPortOfDischarge.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Singapore)
							{
								portInfo.AddMessageError(ValidationConstants.PortCannotSingaporePortForExport("Discharge port"));
							}
						}
						else if (header.IsImport)
						{
							if (Parent.ABL_RL_NKPortOfDischarge.SubstringSafe(0, 2) != Core.Constants.CountryCodes.Singapore)
							{
								portInfo.AddMessageError(ValidationConstants.PortMustBeSingaporePortForImport("Discharge port"));
							}
						}

						if (header.IsRoad)
						{
							if (Parent.ABL_RL_NKPortOfDischarge.SubstringSafe(0, 2) != Core.Constants.CountryCodes.Malaysia &&
								Parent.ABL_RL_NKPortOfLoading.SubstringSafe(0, 2) != Core.Constants.CountryCodes.Malaysia)
							{
								portInfo.AddMessageError(ValidationConstants.SGRoadManifestValidOnlyForMalaysia);
							}
						}
					}
				}
			}
		}

		protected override void CheckABL_RL_NKPortOfLoading()
		{
			base.CheckABL_RL_NKPortOfLoading();
			if (!Parent.ABL_RL_NKPortOfLoading.IsEmpty)
			{
				var portInfo = Parent.ABL_RL_NKPortOfLoadingInfo;
				if (!portInfo.HasErrors())
				{
					var header = Header;
					if (header != null)
					{
						if (header.IsExport)
						{
							if (Parent.ABL_RL_NKPortOfLoading.SubstringSafe(0, 2) != Core.Constants.CountryCodes.Singapore)
							{
								portInfo.AddMessageError(ValidationConstants.PortMustBeSingaporePortForExport("Load Port"));
							}
						}
						else if (header.IsImport)
						{
							if (Parent.ABL_RL_NKPortOfLoading.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Singapore)
							{
								portInfo.AddMessageError(ValidationConstants.PortCannotBeSingaporePortForImport("Load Port"));
							}
						}

						if (header.IsRoad)
						{
							if (Parent.ABL_RL_NKPortOfDischarge.SubstringSafe(0, 2) != Core.Constants.CountryCodes.Malaysia &&
								Parent.ABL_RL_NKPortOfLoading.SubstringSafe(0, 2) != Core.Constants.CountryCodes.Malaysia)
							{
								portInfo.AddMessageError(ValidationConstants.SGRoadManifestValidOnlyForMalaysia);
							}
						}
					}
				}
			}
		}

		protected override void CheckABL_CustomsDischargePort()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_CustomsDischargePortInfo);
		}

		protected override void CheckABL_CustomsLoadPort()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_CustomsLoadPortInfo);
		}
	}
}
