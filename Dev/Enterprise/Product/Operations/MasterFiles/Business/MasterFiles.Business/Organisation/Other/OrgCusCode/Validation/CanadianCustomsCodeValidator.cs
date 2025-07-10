using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class CanadianCustomsCodeValidator
	{
		#region Constants

		public static class Constants
		{
			public static string CarrierCodeRightFormat
			{
				get { return Res.GetString("e73b23bd-e0de-418a-b825-374656adb247", "A valid Carrier Code is composed of 4 alphanumeric characters and '-'(hyphen)."); }
			}

			public static string AuthorizationIDRightFormat
			{
				get { return Res.GetString("9b245c18-836a-41fc-95c4-0201eb4fe47d", "The Authorization ID is composed of 2 alpha/4 numeric digits (AANNNN)."); }
			}

			public static string ExportLicenceNumberCodeRightFormat
			{
				get { return Res.GetString("eeae7fbc-1545-4ca4-a8f6-5accee138239", "An Export License Number is composed of 6 alphanumeric characters."); }
			}

			public static string ServiceProviderAuthorizationIDRightFormat
			{
				get { return Res.GetString("fb321821-4602-45f5-8fc8-43106742c973", "The Service Provider Authorization ID is composed of 2 alpha/4 numeric digits (AANNNN), where the second alpha must be 'A'."); }
			}

			public static string BusinessNumberForPayrollDeductionsRightFormat
			{
				get { return Res.GetString("d2d14014-01e5-487b-b55b-55430d638151", "Business Number for Payroll Deductions should be in the format '999999999RP9999', nine digits to identify the business; and the two letters 'RP' and four digits to identify the account."); }
			}

			public static string BusinessNumberForImportExportFormat
			{
				get { return Res.GetString("7160b73a-5bf2-4df6-8256-3bfe386a4642", "Business Number for Import/Export should be in the format '999999999RM9999', nine digits to identify the business; and the two letters 'RM' and four digits to identify the account."); }
			}

			public static string BusinessNumberCustomsBrokerFormat
			{
				get { return Res.GetString("8530CDB1-03D5-4217-9ACA-2C6FAEF5D4BF", "Business Number for Customs Broker should be in the format '999999999RM9999', nine digits to identify the business; and the two letters 'RM' and four digits to identify the account."); }
			}

			public static string BusinessNumberImporterNonCommercialFormat
			{
				get { return Res.GetString("8D07042B-75F2-46F9-8B5A-13B9E7EE0DC3", "Business Number for Importer Non-Commercial should be in the format '999999999RM9999', nine digits to identify the business; and the two letters 'RM' and four digits to identify the account."); }
			}

			public static string BusinessNumberForExportFormat
			{
				get { return Res.GetString("0c18f432-6348-4ff8-9bc8-48e50c7d5074", "Business Number for Export should be in the format '999999999RM9999', nine digits to identify the business; and the two letters 'RM' and four digits to identify the account."); }
			}

			public static string BusinessNumberForLowValueShipmentsFormat
			{
				get { return Res.GetString("76f9abba-9020-4987-b1f4-6f0680c61de2", "Business Number for Low Value Shipments should be in the format '999999999RM9999', nine digits to identify the business; and the two letters 'RM' and four digits to identify the account."); }
			}

			public static string BusinessNumberForGoodsServicesHarmonizedSalesTaxFormat
			{
				get { return Res.GetString("6bfb736e-9a35-4fe8-b435-dbc646404d1f", "Business Number for Goods and Services Tax/Harmonized Sales Tax should be in the format '999999999RT9999', nine digits to identify the business; and the two letters 'RT' and four digits to identify the account."); }
			}

			public static string BusinessNumberForCorporateIncomeTaxFormat
			{
				get { return Res.GetString("dc03b014-befa-44fd-8bb1-4e66b1afea08", "Business Number for Corporate Income Tax should be in the format '999999999RC9999', nine digits to identify the business; and the two letters 'RC' and four digits to identify the account."); }
			}

			public static string BusinessNumberImporterCommercialFormat
			{
				get { return Res.GetString("ea979b3e-3d5b-4891-a0f8-58dfad2fef75", "A valid Business Number Importer Commercial is composed of 9 alphanumeric characters."); }
			}

			public static string WorldManufacturerIdentifierFormat
			{
				get { return Res.GetString("ac3cb6c0-da66-46b7-91e0-2bda582d43ad", "World Manufacturer Identifier should be no longer than 6 characters."); }
			}
		}

		#endregion

		#region Validation

		public static string GetCarrierCodeError(ZString code)
		{
			return Regex.IsMatch(code, @"^[a-z0-9\-]{4}$", RegexOptions.IgnoreCase) ? string.Empty : Constants.CarrierCodeRightFormat;
		}

		public static string GetAuthorizationIDError(ZString code)
		{
			return Regex.IsMatch(code, @"^[a-z]{2}[0-9]{4}$", RegexOptions.IgnoreCase) ? string.Empty : Constants.AuthorizationIDRightFormat;
		}

		public static string GetExportLicenceNumberError(ZString code)
		{
			return Regex.IsMatch(code, @"^[a-z0-9]{6}$", RegexOptions.IgnoreCase) ? string.Empty : Constants.ExportLicenceNumberCodeRightFormat;
		}

		public static ZString GetServiceProviderAuthorizationIDError(ZString code)
		{
			return Regex.IsMatch(code, @"^[a-z]a[0-9]{4}$", RegexOptions.IgnoreCase) ? string.Empty : Constants.ServiceProviderAuthorizationIDRightFormat;
		}

		public static ZString GetBusinessNumberForPayrollDeductionsError(ZString code)
		{
			return Regex.IsMatch(code, @"^[0-9]{9}RP[0-9]{4}$", RegexOptions.IgnoreCase) ? string.Empty : Constants.BusinessNumberForPayrollDeductionsRightFormat;
		}

		public static ZString GetBusinessNumberForImportExportError(ZString code)
		{
			return Regex.IsMatch(code, @"^[0-9]{9}RM[0-9]{4}$", RegexOptions.IgnoreCase) ? string.Empty : Constants.BusinessNumberForImportExportFormat;
		}

		public static ZString GetBusinessNumberCustomsBrokerError(ZString code)
		{
			return Regex.IsMatch(code, @"^[0-9]{9}RM[0-9]{4}$", RegexOptions.IgnoreCase) ? string.Empty : Constants.BusinessNumberCustomsBrokerFormat;
		}

		public static ZString GetBusinessNumberImporterNonCommercialError(ZString code)
		{
			return Regex.IsMatch(code, @"^[0-9]{9}RM[0-9]{4}$", RegexOptions.IgnoreCase) ? string.Empty : Constants.BusinessNumberImporterNonCommercialFormat;
		}

		public static ZString GetBusinessNumberForExportError(ZString code)
		{
			return Regex.IsMatch(code, @"^[0-9]{9}RM[0-9]{4}$", RegexOptions.IgnoreCase) ? string.Empty : Constants.BusinessNumberForExportFormat;
		}

		public static ZString GetBusinessNumberForLowValueShipmentsError(ZString code)
		{
			return Regex.IsMatch(code, @"^[0-9]{9}RM[0-9]{4}$", RegexOptions.IgnoreCase) ? string.Empty : Constants.BusinessNumberForLowValueShipmentsFormat;
		}

		public static ZString GetBusinessNumberForGoodsServicesHarmonizedSalesTaxError(ZString code)
		{
			return Regex.IsMatch(code, @"^[0-9]{9}RT[0-9]{4}$", RegexOptions.IgnoreCase) ? string.Empty : Constants.BusinessNumberForGoodsServicesHarmonizedSalesTaxFormat;
		}

		public static ZString GetBusinessNumberForCorporateIncomeTaxError(ZString code)
		{
			return Regex.IsMatch(code, @"^[0-9]{9}RC[0-9]{4}$", RegexOptions.IgnoreCase) ? string.Empty : Constants.BusinessNumberForCorporateIncomeTaxFormat;
		}

		public static ZString GetBusinessNumberImporterCommercialError(ZString code)
		{
			return Regex.IsMatch(code, @"^[a-z0-9]{9}$", RegexOptions.IgnoreCase) ? string.Empty : Constants.BusinessNumberImporterCommercialFormat;
		}

		public static ZString GetWorldManufacturerIdentifierError(ZString code)
		{
			return Regex.IsMatch(code, @"^[a-z0-9]{1,6}$", RegexOptions.IgnoreCase) ? string.Empty : Constants.WorldManufacturerIdentifierFormat;
		}

		#endregion
	}
}
