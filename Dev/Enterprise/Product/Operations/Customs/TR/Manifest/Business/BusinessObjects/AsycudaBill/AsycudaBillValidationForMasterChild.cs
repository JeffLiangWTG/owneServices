using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidationForMasterChild
	{
		public AsycudaBillValidationForMasterChild(AsycudaBill parent)
			: base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected override void CheckABL_RL_NKPortOfDischarge()
		{
			var portInfo = Parent.ABL_RL_NKPortOfDischargeInfo;
			CheckValidUNLOCOCode(portInfo, Parent.PortOfDischarge, ManifestValidationRuleCodes.IATAPortOfDischarge);
			base.CheckABL_RL_NKPortOfDischarge();
		}

		protected override void CheckABL_RL_NKPortOfLoading()
		{
			var portInfo = Parent.ABL_RL_NKPortOfLoadingInfo;
			CheckValidUNLOCOCode(portInfo, Parent.PortOfLoading, ManifestValidationRuleCodes.IATAPortOfLoading);
			base.CheckABL_RL_NKPortOfLoading();
		}

		protected override bool IsPortOfLoadingRequired => Parent.Header.AMA_ManifestType != TRManifestTypes.Codes.GRUPAJ;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String comparison")]
		void CheckValidUNLOCOCode(ZPropertyInfo portCodeInfo, RefUNLOCO refUNLOCO, string validationRule)
		{
			ListValidation.ErrorIfInvalidCode(portCodeInfo);
			if (refUNLOCO != null)
			{
				if (Header.IsRoad && !refUNLOCO.RL_HasRoad ||
					Header.IsAir && !refUNLOCO.RL_HasAirport ||
					Header.IsSea && !refUNLOCO.RL_HasSeaport ||
					Header.IsMail && !refUNLOCO.RL_HasPost)
				{
					portCodeInfo.AddError(string.Format(CultureInfo.InvariantCulture, "Selected {0} cannot be used for {1} transport", portCodeInfo.HumanReadableName, Header.AMA_TransportMode));
				}

				if (Parent.IsAir && refUNLOCO.RL_IATA.IsEmpty)
				{
					ZZValidationHeaderHelper.CheckIsMandatoryForValidationRuleWhenTheRelatedValueIsEmpty(portCodeInfo, validationRule);
				}
			}
		}

		protected override bool IsABL_E_DEPRequired
		{
			get
			{
				switch (Parent.Header.AMA_ManifestType)
				{
					case TRManifestTypes.Codes.DENITH:
					case TRManifestTypes.Codes.DENIHR:
					case TRManifestTypes.Codes.HAVITH:
					case TRManifestTypes.Codes.GRUPAJ:
					case TRManifestTypes.Codes.VARONC:
					case TRManifestTypes.Codes.CIKONC:
					case TRManifestTypes.Codes.HAVIHR:
					case TRManifestTypes.Codes.TESLIM:
						return false;
					default:
						return true;
				}
			}
		}

		protected override void CheckMandatoryABL_E_ARV()
		{
		}
	}
}
