using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbCompanyExtensionsTest : TestCaseWithFactory
	{
		public void TestGetLocalDecimals_ReturnsZeroFromRefCurrency()
		{
			var currency = CreateCurrency("ZZ0", 0);
			var company = CreateCompany("ZZ0", currency);

			var actual = company.GetLocalDecimals();
			AssertEquals(0, actual);
		}

		public void TestGetLocalDecimals_ReturnsOneFromRefCurrency()
		{
			var currency = CreateCurrency("ZZ1", 1);
			var company = CreateCompany("ZZ1", currency);

			var actual = company.GetLocalDecimals();
			AssertEquals(1, actual);
		}

		public void TestGetLocalDecimals_ReturnsTwoFromRefCurrency()
		{
			var currency = CreateCurrency("ZZ2", 2);
			var company = CreateCompany("ZZ2", currency);

			var actual = company.GetLocalDecimals();
			AssertEquals(2, actual);
		}

		public void TestGetLocalDecimals_ReturnsThreeFromRefCurrency()
		{
			var currency = CreateCurrency("ZZ3", 3);
			var company = CreateCompany("ZZ3", currency);

			var actual = company.GetLocalDecimals();
			AssertEquals(3, actual);
		}

		public void TestGetLocalDecimals_UsesCurrentCompanyWhenPassedNull()
		{
			var currency = CreateCurrency("ZZ1", 1);
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency("ZZ1"))
			{
				GlbCompany nullCompany = null;

				var actual = nullCompany.GetLocalDecimals();
				AssertEquals(1, actual);
			}
		}

		public void TestGetLocalDecimals_ThrowsWhenLocalCurrenyIsBlank_AndCompanyIsActive()
		{
			var currency = CreateCurrency("ZZ3", 3);
			var company = CreateCompany("ZZ3", localCurrency: null, isActive: true);
			company.GC_Name = "Invalid Company Pty Ltd";
			Factory.Save();

			AssertExceptionThrown<ApplicationException>(
				"An active company with blank GC_RX_NKLocalCurrency should be prevented by bizo validation rules, and user should fix this case if it arises",
				"Unable to get number of local decimals for active company 'ZZ3' (Invalid Company Pty Ltd): GC_RX_NKLocalCurrency is ''. Please set a Local Currency for this company.",
				() => company.GetLocalDecimals()
			);
		}

		public void TestGetLocalDecimals_ReturnsTwoAndRaisesDeveloperError_WhenActiveCompanyHasNullLocalCurrency()
		{
			var company = CreateCompany("ZZ0", localCurrency: null, isActive: true);
			company.GC_Name = "Invalid Company Pty Ltd";
			company.GC_RX_NKLocalCurrency = "ZZD";
			Factory.Save();

			ErrorReporter.Clear();
			var actual = company.GetLocalDecimals();
			AssertEquals(2, actual);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("GetLocalDecimalsCannotLoadLocalCurrency", ErrorReporter.LastKeyReported);
			var expectedErrorMessage = $@"Call to GlbCompany.GetLocalDecimals() when GlbCompany.LocalCurrency is null.
GlbCompany.IsInDatabase = True
GlbCompany.IsDeleted = False
GlbCompany.IsDeleting = False
GlbCompany.GC_Code = ZZ0
GlbCompany.GC_Name = Invalid Company Pty Ltd
GlbCompany.GC_RX_NKLocalCurrency = ZZD
GlbCompany.GC_IsActive = Y
GlbCompany.Factory = {Factory._Instance} '{Factory.NameForDebugging}'
Is GlbCompany.CurrentCompany = False
Have cleared RefCurrency table from UberFactory cache, and GlbCompany.LocalCurrency remains null.
Returned 2 Local Decimals as fallback.";
			AssertMultilineASCIIEquals(expectedErrorMessage, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			actual = company.GetLocalDecimals();
			AssertEquals(2, actual);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals("Second call should not log another error", "", ErrorReporter.LastKeyReported);
			AssertEquals("Second call should not log another error", "", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestGetLocalDecimals_AddLocalCurrencyCachedMessage_WhenCurrencyIsNull()
		{
			var company = GlbCompany.CurrentCompany;
			CreateCurrency("ZZ3", 3);
			company.SetCurrency("ZZ3");
			Factory.Save();

			AssertNotNull(company.LocalCurrency);

			company.GC_RX_NKLocalCurrency = "ZZ1";
			company.GetLocalDecimals();
			AssertEquals("GetLocalDecimalsCannotLoadLocalCurrency", ErrorReporter.LastKeyReported);
			var expectedErrorMessage = $@"Call to GlbCompany.GetLocalDecimals() when GlbCompany.LocalCurrency is null.
GlbCompany.IsInDatabase = True
GlbCompany.IsDeleted = False
GlbCompany.IsDeleting = False
GlbCompany.GC_Code = {company.GC_Code}
GlbCompany.GC_Name = {company.GC_Name}
GlbCompany.GC_RX_NKLocalCurrency = ZZ1
GlbCompany.GC_IsActive = Y
GlbCompany.Factory = {company.Factory._Instance} '{company.Factory.NameForDebugging}'
Is GlbCompany.CurrentCompany = True
localCurrencyCached.IsDeleted = False
localCurrencyCached.RX_Code = ZZ3
Have cleared RefCurrency table from UberFactory cache, and GlbCompany.LocalCurrency remains null.
Returned 2 Local Decimals as fallback.";
			AssertMultilineASCIIEquals(expectedErrorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetLocalDecimals_ReturnsOneWithNoDeveloperError_WhenRefreshingUberCacheWorks()
		{
			var company = CreateCompany("ZZ1", localCurrency: null);
			company.GC_RX_NKLocalCurrency = "ZZ1";
			company.GC_Name = "Invalid Company Pty Ltd";
			AssertNull("Precondition: LocalCurrency is null, and is now cached in Uber Factory", company.LocalCurrency);
			Factory.Save();

			// Direct DB query to bypass all bizo caches
			// Expected outcome: ZZ1 is in the database, but cached in Uber Cache as being null
			Db.Connection.ExecuteNonQuery(@"
INSERT INTO dbo.RefCurrency ([RX_PK], [RX_Code], [RX_Symbol], [RX_Desc], [RX_UnitName], [RX_SubUnitName], [RX_SubUnitRatio], [RX_IsActive], [RX_IsSystem], [RX_ISOSubUnitRatio], [RX_AutoVersion], [RX_SystemCreateTimeUtc], [RX_SystemCreateUser], [RX_SystemLastEditTimeUtc], [RX_SystemLastEditUser])
VALUES (NEWID(), 'ZZ1', N'☺', 'ZZ1 Currency', 'ZZ1', 'ZZ1', 10, 1, 1, 10, 1, SYSDATETIME(), '~UK', SYSDATETIME(), '~UK')");

			AssertNull("Precondition: LocalCurrency remains null, Uber Factory is doing its job", company.LocalCurrency);

			var actual = company.GetLocalDecimals();
			AssertEquals("The Uber Factory should be cleared and correct currency loaded", 1, actual);

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals("No errors logged", "", ErrorReporter.LastKeyReported);
			AssertEquals("No errors logged", "", ErrorReporter.LastMessageReported);
		}

		public void TestGetLocalDecimals_ReturnsTwo_WhenLocalCurrencyIsBlank_AndCompanyIsNotSaved()
		{
			var company = CreateCompany("ZZ3", localCurrency: null, isActive: true);

			var actual = company.GetLocalDecimals();
			AssertEquals("Until a company is saved, a blank local currency should return 2 decimal places as a fallback.", 2, actual);
		}

		public void TestGetLocalDecimals_ReturnsTwo_WhenLocalCurrencyIsBlank_AndCompanyIsInactive()
		{
			var company = CreateCompany("ZZ3", localCurrency: null, isActive: false);
			Factory.Save();

			var actual = company.GetLocalDecimals();
			AssertEquals("An inactive company with a blank local currency should return 2 decimal places, because it is no longer in use and fixing old bad data is tricky.", 2, actual);
		}

		RefCurrency CreateCurrency(string code, int decimalPlacesCount)
		{
			var currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = code;
			currency.RX_SubUnitRatio = (int)Math.Pow(10, decimalPlacesCount);
			currency.RX_ISOSubUnitRatio = (int)Math.Pow(10, decimalPlacesCount);
			return currency;
		}

		GlbCompany CreateCompany(string code, RefCurrency localCurrency, bool isActive = true)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = code;
			company.GC_RX_NKLocalCurrency = localCurrency?.RX_Code ?? ZString.Empty;
			company.GC_IsActive = isActive;
			return company;
		}
	}
}
