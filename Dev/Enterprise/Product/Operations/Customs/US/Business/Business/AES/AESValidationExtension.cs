namespace Enterprise.Customs.US.Business
{
	public static class AESValidationExtension
	{
		public static bool IsCarrierCodeRequired(this JobDeclaration declaration)
		{
			return declaration != null && declaration.IsExport && (declaration.IsSea || declaration.IsAir || declaration.IsBorderWaterBorne || declaration.IsRail || declaration.IsTruck);
		}

		public static bool IsExportingCarrierRequired(this JobDeclaration declaration)
		{
			return declaration != null && declaration.IsExport && (declaration.IsSea || declaration.IsAir || declaration.IsBorderWaterBorne || declaration.IsRail || declaration.IsTruck);
		}

		public static bool IsWeightRequired(this JobComInvoiceLine invoiceLine)
		{
			var declaration = invoiceLine == null ? null : invoiceLine.Declaration;
			return declaration != null && declaration.IsExport && (declaration.IsSea || declaration.IsBorderWaterBorne || declaration.IsRail || declaration.IsTruck || declaration.IsAir || (!declaration.IsFixedTransportInstallations && invoiceLine.US_ExportCode == ExportInformationCodeList.Codes.HH));
		}

		public static bool IsTransportReferenceNumberRequired(this JobDeclaration declaration)
		{
			return
				declaration != null &&
				declaration.IsExport &&
				declaration.IsSea;
		}

		public static bool IsTransportReferenceNumberForbidden(this JobDeclaration declaration)
		{
			return
				declaration != null &&
				declaration.IsExport &&
				!(declaration.IsSea || declaration.IsAir || declaration.IsRail || declaration.IsTruck);
		}
	}
}
