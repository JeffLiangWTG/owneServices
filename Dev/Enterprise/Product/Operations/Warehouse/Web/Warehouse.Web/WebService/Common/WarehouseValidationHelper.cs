using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Common
{
	public static class WarehouseValidationHelper
	{
		public static string ValidatePalletID(string palletID)
		{
			return ValidateMaxLength(WhsDocketLineSchema.WE_PalletID.Name, palletID, WhsDocketLineSchema.WE_PalletID.MaxLength);
		}

		public static string ValidateDocketExternalReference(string reference, string userFriendlyPropertyName)
		{
			return ValidateMaxLength(userFriendlyPropertyName, reference, WhsDocketSchema.WD_ExternalReference.MaxLength);
		}

		static string ValidateMaxLength(string propertyName, string property, int propertyMaxLength)
		{
			return !string.IsNullOrEmpty(property) && property.Length > propertyMaxLength
				? Res.GetString("861321b9-3610-4aa5-ba7e-e322e8e8dc8f", "{0} exceeds maximum length allowed. The maximum length of this property is {1} characters, but {2} were entered.",
					propertyName, propertyMaxLength, property.Length)
				: string.Empty;
		}
	}
}
