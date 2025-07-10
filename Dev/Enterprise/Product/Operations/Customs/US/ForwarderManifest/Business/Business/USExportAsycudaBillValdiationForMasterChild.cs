using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportAsycudaBillValdiationForMasterChild : AsycudaBillValidationForMasterChild
	{
		public USExportAsycudaBillValdiationForMasterChild(USExportAsycudaBill parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAESITNNumbers();
			ValidateInBondNumbers();
		}

		public void ValidateAESITNNumbers()
		{
			ValidateCalculatedProperty(((USExportAsycudaBill)Parent).AESITNNumbersInfo);
		}

		public void ValidateInBondNumbers()
		{
			ValidateCalculatedProperty(((USExportAsycudaBill)Parent).InBondNumbersInfo);
		}

		protected void CheckAESITNNumbers()
		{
			if (Parent is USExportAsycudaBill parent)
			{
				USExportAsycudaBillValidationHelper.AddNotificationFromCusEntryNumber(parent.AESITNNumberCollection, parent.AESITNNumbersInfo);
				USExportAsycudaBillValidationHelper.CheckITNAndExemptionCodeAndInBondNumber(parent, parent.AESITNNumbersInfo);
			}
		}

		protected void CheckInBondNumbers()
		{
			if (Parent is USExportAsycudaBill parent)
			{
				USExportAsycudaBillValidationHelper.AddNotificationFromCusEntryNumber(parent.InBondNumberCollection, parent.InBondNumbersInfo);
				USExportAsycudaBillValidationHelper.CheckITNAndExemptionCodeAndInBondNumber(parent, parent.InBondNumbersInfo);
			}
		}

		protected override void CheckABL_UCRNumber()
		{
			base.CheckABL_UCRNumber();
			if (Parent is USExportAsycudaBill parent)
			{
				USExportAsycudaBillValidationHelper.CheckITNAndExemptionCodeAndInBondNumber(parent, parent.ABL_UCRNumberInfo);
			}
		}

		protected override void CheckMandatoryABL_E_ARV()
		{ }

		protected override void CheckABL_RL_NKPortOfLoading()
		{ }

		protected override void CheckABL_BillNumberCore()
		{ }

		protected override void CheckABL_BillIssuer()
		{
			base.CheckABL_BillIssuer();
			new IssuerCarrierSCACValidator(Parent.Factory).ValidateSCACCode(Parent.ABL_BillIssuerInfo, Parent.Header?.AMA_TransportMode ?? string.Empty, "Carrier", true, true, false);
		}

		protected override bool ShouldCheckHasAsycudaCountry => false;

		protected override void CheckABL_CustomsLoadPort()
		{
			base.CheckABL_CustomsLoadPort();
			var parent = Parent;
			var schDExportInfo = parent.ABL_CustomsLoadPortInfo;
			var header = (USExportAsycudaManifestHeader)parent.Header;
			var schDExport = parent.ABL_CustomsLoadPort;
			var transportMode = header.AMA_TransportMode;
			if (!schDExport.IsEmpty)
			{
				if (transportMode == RefTransportModeList.Codes.AIR || transportMode == RefTransportModeList.Codes.SEA
					|| transportMode == RefTransportModeList.Codes.ROA || transportMode == RefTransportModeList.Codes.RAI || transportMode == RefTransportModeList.Codes.FIX)
				{
					var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(parent.Factory, schDExport,
					Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today,
					attributeFilters:
					new[]
					{
							new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.ROLE, JoinCondition.And,
							new ZString[] { "EXP" })
					});

					if (port != null && !port.IsTransportModeApplied(transportMode))
					{
						schDExportInfo.AddMessageError(string.Format(InvalidPortForTransportMode, transportMode));
					}
				}
			}
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(schDExportInfo);

			if (schDExport.IsEmpty && header.GetPortOfExportRefLocoMappings(schDExport, transportMode).Count > 1)
			{
				schDExportInfo.AddMessageError(ExportMultipleMatches);
			}
		}

		public string ExportMultipleMatches = "Multiple port code matches have been found for this UNLoco. Please select the appropriate code from list.";
		public string InvalidPortForTransportMode = "Invalid port for transport type. This port is not a valid {0} port.";
	}
}
