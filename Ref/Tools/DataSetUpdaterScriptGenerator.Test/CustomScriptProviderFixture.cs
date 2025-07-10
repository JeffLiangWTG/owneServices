using NUnit.Framework;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator.Test;

[TestFixture]
class CustomScriptProviderFixture
{
	[Test]
	public void Get_RefCusTaxOrFee()
	{
		var customScriptProvider = new CustomScriptProvider();
		var updaterInfo = new RefCusTaxOrFeeTypeUpdaterInfo<IRefCusTaxOrFeeType>();
		var customScript = customScriptProvider.Get(updaterInfo);
		Assert.That(customScript, Is.EqualTo(taxOrFeeScript));
	}

	const string taxOrFeeScript = @"IF EXISTS ( SELECT 1 FROM #TempRefCusTaxOrFee WHERE ZZF_ZZZ_NKDataGrouping = 'X!' AND ZZF_Value <> 0.15)
BEGIN
	RAISERROR('CW requires upgrade to the latest version.', 16, 1)
	RETURN
END

DELETE FROM #TempRefCusTaxOrFee WHERE ZZF_ZZZ_NKDataGrouping = 'X!'
DELETE FROM #TempRefCusTaxOrFeeType WHERE ZX0_Description = 'Dummy RefCusTaxOrFeeType'
";

	[Test]
	public void Get_RefCusQuota()
	{
		var customScriptProvider = new CustomScriptProvider();
		var updaterInfo = new OneTableDataSetUpdaterInfo<IRefCusQuota>(false);
		var customScript = customScriptProvider.Get(updaterInfo);
		Assert.That(customScript, Is.EqualTo(quotaScript));
	}

	const string quotaScript = @"IF EXISTS ( SELECT 1 FROM #TempRefCusQuota WHERE ZXQ_ZZZ_NKDataGrouping = 'X!' AND ZXQ_Balance <> 0.15)
BEGIN
	RAISERROR('CW requires upgrade to the latest version.', 16, 1)
	RETURN
END

DELETE FROM #TempRefCusQuota WHERE ZXQ_ZZZ_NKDataGrouping = 'X!'
";

	[Test]
	public void Get_ReExchangeRateZZ()
	{
		var customScriptProvider = new CustomScriptProvider();
		var updaterInfo = new OneTableDataSetUpdaterInfo<IRefExchangeRateZZ>(true);
		var customScript = customScriptProvider.Get(updaterInfo);
		Assert.That(customScript, Is.EqualTo(exchangeRateZZScript));
	}

	const string exchangeRateZZScript = @"IF EXISTS ( SELECT 1 FROM #TempRefExchangeRateZZ WHERE ZZN_RN_NKCountry = 'X!' AND ZZN_Rate <> 0.15)
BEGIN
	RAISERROR('CW requires upgrade to the latest version.', 16, 1)
	RETURN
END

DELETE FROM #TempRefExchangeRateZZ WHERE ZZN_RN_NKCountry = 'X!'
";

	[Test]
	public void Get_RefCusMap()
	{
		var customScriptProvider = new CustomScriptProvider();
		var updaterInfo = new OneTableDataSetUpdaterInfo<IRefCusMap>(false);
		var customScript = customScriptProvider.Get(updaterInfo);
		Assert.That(customScript, Is.EqualTo(string.Empty));
	}
}
