namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCTariffsPermitsApplyTo))]
	public class NZCTariffsPermitsApplyToTest : EnterpriseBusinessObjectTestCase
	{
		#region TestLoadByTariffCodeRequiringPermitCodes02Digit
		public void TestLoadByTariffCodeRequiringPermitCodes02Digit()
		{
			NZCTariffsPermitsApplyTo permits = NZCTariffsPermitsApplyTo.Load(Factory, "0301.10.00.00B", isImport: true, isExport: false);
			AssertEquals("03", permits.U6_TariffPortion);
		}
		#endregion

		#region TestLoadByTariffCodeRequiringPermitCodes04Digit
		public void TestLoadByTariffCodeRequiringPermitCodes04Digit()
		{
			NZCTariffsPermitsApplyTo permits = NZCTariffsPermitsApplyTo.Load(Factory, "4701.00.00.01D", isImport: true, isExport: false);
			AssertEquals("4701", permits.U6_TariffPortion);
		}
		#endregion

		#region TestLoadByTariffCodeRequiringPermitCodes07Digit
		public void TestLoadByTariffCodeRequiringPermitCodes07Digit()
		{
			NZCTariffsPermitsApplyTo permits = NZCTariffsPermitsApplyTo.Load(Factory, "1605.20.01.00K", isImport: true, isExport: false);
			AssertEquals("16", permits.U6_TariffPortion);
		}
		#endregion

		#region TestLoadByTariffCodeRequiringPermitCodes10Digit
		public void TestLoadByTariffCodeRequiringPermitCodes10Digit()
		{
			NZCTariffsPermitsApplyTo permits = NZCTariffsPermitsApplyTo.Load(Factory, "0307.39.00.01H", isImport: true, isExport: false);
			AssertEquals("0307.39.00", permits.U6_TariffPortion);
		}
		#endregion

		#region TestLoadByTariffCodeRequiringPermitCodes14Digit
		public void TestLoadByTariffCodeRequiringPermitCodes14Digit()
		{
			NZCTariffsPermitsApplyTo permits = NZCTariffsPermitsApplyTo.Load(Factory, "0307.99.19.00D", isImport: true, isExport: false);
			AssertEquals("0307.99.19.00D", permits.U6_TariffPortion);
		}
		#endregion

		#region TestLoadByTariffCodeRequiringNoPermitCodes
		public void TestLoadByTariffCodeRequiringNoPermitCodes()
		{
			NZCTariffsPermitsApplyTo permits = NZCTariffsPermitsApplyTo.Load(Factory, "4801.00.11.11K", isImport: true, isExport: false);
			AssertNull("Permits", permits);
		}
		#endregion

		#region TestLoadByTariffCodeWrongLength
		public void TestLoadByTariffCodeWrongLength()
		{
			NZCTariffsPermitsApplyTo permits = NZCTariffsPermitsApplyTo.Load(Factory, "3602.00.00", isImport: true, isExport: false);
			AssertNull("Permits", permits);
		}
		#endregion

		#region TestLoadByTariffKnowsTheDifferenceBetweenImportAndExport
		public void TestLoadByTariffKnowsTheDifferenceBetweenImportAndExport()
		{
			NZCTariffsPermitsApplyTo permits = NZCTariffsPermitsApplyTo.Load(Factory, "4701.00.00.05G", isImport: false, isExport: true);
			AssertNull("Permits", permits);
			permits = NZCTariffsPermitsApplyTo.Load(Factory, "4701.00.00.05G", isImport: true, isExport: false);
			AssertEquals("4701", permits.U6_TariffPortion);
		}
		#endregion

		#region TestLoadByTariffCanHandleAskingForBothImportAndExport
		public void TestLoadByTariffCanHandleAskingForBothImportAndExport()
		{
			NZCTariffsPermitsApplyTo permits = NZCTariffsPermitsApplyTo.Load(Factory, "4701.00.00.05G", isImport: true, isExport: true);
			AssertEquals("4701", permits.U6_TariffPortion);
		}
		#endregion
	}
}
