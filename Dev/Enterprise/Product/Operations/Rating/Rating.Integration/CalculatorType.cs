namespace Enterprise.Rating.Integration
{
	/// <summary>
	/// All the calculator types.
	/// </summary>
	public enum CalculatorType
	{
		None = 0,
		Agency, // AGY
		Cartage, // CTG - Transport Calculator
		CartageZoneDistance, // CTZ - Transport Zone Distance Based Calculator
		CostBased, // CST - CompanyTariffOrCostBasedCalculator.CostBased
		Combined, // CMB - Sliding or Per Unit with Base, Minimum, Maximum Calculator
		CombinedWithIncrement, // CBI - Combined Breaks with Increment Calculator
		CompanyTariffBased, // CTB - CompanyTariffOrCostBasedCalculator.CompanyTariffBased
		DisbursementInterest, // DIN
		Equalization, // VED
		EquipmentHire,
		ExcludeCompanyTariffs, // EXL
		FirstPlusAdditional, // FPA
		Flat, // FLT
		FlatPlusPerUnit, // FPU
		FreightInclusive, // FRT
		HighestCharge, // HCC
		HighestRate, // HRC
		HousebillReleaseType, // HRT
		Minimum, // MIN
		MinimumOrPerUnit, // MPU
		Note, // NTE
		PackageCount, // IAT
		Percentage, // PER
		PercentageBreaks, // PEB
		ProfitShareRebate, // PSR
		SplitMonthBilling, // SMB
		Time, // TME
		Unit, // UNT
		ValueRange, // IXC
		WarehouseLocationType, // WLT
		WarehousePack, // WPK

#if DEBUG
		ForTest_CalculatorPropertyAttribute,
		ForTest_CartageCalculatorWithOverride,
		ForTest_CalculationLog,
		ForTest_ConcreteCalculator,
#endif
	}
}
