using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using EZC = Enterprise.ZArchitecture.Core;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public static class VGMHelper
	{
		public static ZString GetSCAC(this UniversalShipment dataObject, DocAddressType addressType)
		{
			Argument.NotNull(dataObject, nameof(dataObject));

			var orgAddress = dataObject.OrganizationAddressCollection?.FirstOrDefault(
				org => org.AddressType.GetValueOrDefault() == addressType.ToString());
			var scacRegNumber = orgAddress?.RegistrationNumberCollection?.FirstOrDefault(
				reg => reg.CountryOfIssue.GetCodeAsUpperCase() == Core.Constants.CountryCodes.UnitedStates
				&& (reg.Type?.Code).GetValueOrDefault() == OrgCusCode.CodeTypes.CarrierCode);
			return scacRegNumber?.Value ?? ZString.Empty;
		}

		public static ZString GetC1C(this UniversalShipment dataObject, DocAddressType addressType)
		{
			Argument.NotNull(dataObject, nameof(dataObject));

			var orgAddress = dataObject.OrganizationAddressCollection?.FirstOrDefault(
				org => org.AddressType.GetValueOrDefault() == addressType.ToString());
			var c1cRegNumber = orgAddress?.RegistrationNumberCollection?.FirstOrDefault(
				reg => (reg.Type?.Code).GetValueOrDefault() == OrgCusCode.CodeTypes.CargoWiseOneCarrierCode);
			return c1cRegNumber?.Value ?? ZString.Empty;
		}

		public static bool IsVGM(this UniversalShipment dataObject)
		{
			Argument.NotNull(dataObject, nameof(dataObject));

			return dataObject.DataContext?.RecipientRoleCollection?
				.Any(c => c.Code == RecipientRoleType.FOR && c.ServiceCode == ServiceCodeType.VGM) ?? false;
		}

		public static bool HasVGMSection(this UniversalShipment dataObject)
		{
			Argument.NotNull(dataObject, nameof(dataObject));

			return dataObject.DataContext?.RecipientRoleCollection?
				.Any(c => c.ServiceCode == ServiceCodeType.VGM) ?? false;
		}

		public static void UpdateContainerGrossWeightVerificationDateTime(this Container containerDataObject)
		{
			Argument.NotNull(containerDataObject, nameof(containerDataObject));

			var verifiedDateTime = containerDataObject.GrossWeightVerificationDateTime ?? ZDateTime.Empty;
			if (verifiedDateTime.IsEmpty)
			{
				var verificationType = containerDataObject.GrossWeightVerificationType?.Code ?? ZString.Empty;
				if (!verificationType.IsEmpty
					&& new ZString[]
					{
						Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container,
						Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages,
						Core.Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal,
						Core.Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod
					}.Contains(verificationType))
				{
					containerDataObject.GrossWeightVerificationDateTime = ZDateTime.Now;
				}
			}
		}

		public static bool IsContainerModeApplicableForVGM(ZString containerMode)
		{
			return containerMode == Core.Constants.ContainerModes.FCL
				|| containerMode == Core.Constants.ContainerModes.Groupage
				|| containerMode == Core.Constants.ContainerModes.BuyersConsol;
		}

		public static ZString ValidateVGMProperties(this UniversalShipment dataObject, bool shouldValidateDateTime = false)
		{
			if (dataObject.ContainerCollection == null)
			{
				return ZString.Empty;
			}

			foreach (var container in dataObject.ContainerCollection)
			{
				return ValidateVGMProperties(container, shouldValidateDateTime);
			}

			return ZString.Empty;
		}

		internal static ZString ValidateVGMProperties(this UniversalContainer dataObject, bool shouldValidateDateTime = false)
		{
			var hasWeightRelatedInfo = dataObject.GrossWeightVerificationDateTime != null;
			var isVerificationTypeMissing = string.IsNullOrEmpty(dataObject.GrossWeightVerificationType?.Code);

			if (hasWeightRelatedInfo && isVerificationTypeMissing)
			{
				return Res.GetString("d357ee10-446e-4cb8-8dad-244f6e562ee9", "'{0}' not found in XML, Gross Weight, Unit, Verified By, Verified Date will not be imported.", (EZC.NoResString)"GrossWeightVerificationType");
			}

			if (isVerificationTypeMissing && !hasWeightRelatedInfo)
			{
				return ZString.Empty;
			}

			bool isNotVerified = dataObject.GrossWeightVerificationType.Code.Value == Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			bool isNotRequired = dataObject.GrossWeightVerificationType.Code.Value == Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired;
			bool isOtherType = !isNotVerified && !isNotRequired;

			bool isValidNonVerified = isNotVerified && (!dataObject.GrossWeightVerificationDateTime.IsValid());
			bool isValidVerified = isOtherType && dataObject.GrossWeight > 0 && (!shouldValidateDateTime || dataObject.GrossWeightVerificationDateTime.IsValid());
			bool isValidNotRequired = isNotRequired && dataObject.GrossWeightVerificationDateTime.IsValid() && dataObject.GrossWeight > 0;

			if (!isValidNonVerified && !isValidVerified && !isValidNotRequired)
			{
				return Res.GetString("2e251570-f8d3-4225-8b35-52ec722d51bc",
					"When element '{0}' is 'NON' then element '{1}' will not be imported.\r\nWhen element '{0}' is not 'NON' then '{1}' must be entered and '{2}' must be greater than 0.",
					(EZC.NoResString)"GrossWeightVerificationType",
					(EZC.NoResString)"GrossWeightVerificationDateTime",
					(EZC.NoResString)"GrossWeight");
			}

			return ZString.Empty;
		}

		public static bool IsValid(this ZDateTime? zDateTime)
		{
			return zDateTime != null && zDateTime.HasValue && zDateTime.Value.IsValid;
		}
	}
}
