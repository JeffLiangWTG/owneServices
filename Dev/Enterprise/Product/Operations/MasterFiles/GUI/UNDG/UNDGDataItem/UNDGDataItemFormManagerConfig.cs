namespace Enterprise.MasterFiles.GUI
{
	public readonly struct UNDGDataItemFormManagerConfig
	{
		UNDGDataItemFormManagerConfig(
			bool isIMOOnly,
			bool shouldHideOverpack,
			bool shouldHideProperShippingName,
			bool shouldHideSubstanceProperties,
			bool shouldHideUNDGProperties = true,
			bool shouldHidePSAGroup = true,
			bool shouldHideLimitedQuantity = true)
		{
			IsIMOOnly = isIMOOnly;
			ShouldHideOverpack = shouldHideOverpack;
			ShouldHideProperShippingName = shouldHideProperShippingName;
			ShouldHideSubstanceProperties = shouldHideSubstanceProperties;
			ShouldHideUNDGProperties = shouldHideUNDGProperties;
			ShouldHidePSAGroup = shouldHidePSAGroup;
			ShouldHideLimitedQuantity = shouldHideLimitedQuantity;
		}

		public bool IsIMOOnly { get; }

		public bool ShouldHideOverpack { get; }

		public bool ShouldHideProperShippingName { get; }

		public bool ShouldHideSubstanceProperties { get; }

		public bool ShouldHideUNDGProperties { get; }

		public bool ShouldHidePSAGroup { get; }

		public bool ShouldHideLimitedQuantity { get; }

		public static UNDGDataItemFormManagerConfig Default() =>
			new UNDGDataItemFormManagerConfig(false, true, false, false);

		public static UNDGDataItemFormManagerConfig IMOHideProperties() =>
			new UNDGDataItemFormManagerConfig(true, true, true, true);

		public static UNDGDataItemFormManagerConfig IMOShowProperties() =>
			new UNDGDataItemFormManagerConfig(true, true, false, false);

		public static UNDGDataItemFormManagerConfig ShowSubstanceProperties() =>
			new UNDGDataItemFormManagerConfig(false, false, true, false);

		public static UNDGDataItemFormManagerConfig ShowPSAGroup() =>
			new UNDGDataItemFormManagerConfig(false, true, false, false, true, false);

		public static UNDGDataItemFormManagerConfig DynamicHidePSAGroup(bool shouldHidePSAGroup) =>
			new UNDGDataItemFormManagerConfig(false, false, true, false, false, shouldHidePSAGroup);

		public static UNDGDataItemFormManagerConfig ShowUNDGDetails() =>
			new UNDGDataItemFormManagerConfig(false, false, false, false, false, true, false);
	}
}
