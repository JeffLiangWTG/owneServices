using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	public class TestOrganisationFilterControl : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestColumnsAsPerRegistrySettings()
		{
			var creditReportItemCollection = new CreditReportItemCollection() {
				CreditReportItemTest.CreateCreditReportItemForTest(Env.CurrentCompany.Country.Code) };
			creditReportItemCollection[0].CountryEnabledForCompany = true;

			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			using (var filterControl = GetNewOrganisationFilterControl())
			{
				var columns = filterControl.Grid.ColumnStyles;

				CombineAssertions(() =>
				{
					AssertColumnAvailable(columns, "MiscServ+OM_CCCreditRating", true);
					AssertColumnAvailable(columns, "MiscServ+OM_CCLatePaymentScore", true);
					AssertColumnAvailable(columns, "MiscServ+OM_CCFailureRiskScore", true);
				});
			}

			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			using (var filterControl = GetNewOrganisationFilterControl())
			{
				var columns = filterControl.Grid.ColumnStyles;

				CombineAssertions(() =>
				{
					AssertColumnAvailable(columns, "MiscServ+OM_CCCreditRating", false);
					AssertColumnAvailable(columns, "MiscServ+OM_CCLatePaymentScore", false);
					AssertColumnAvailable(columns, "MiscServ+OM_CCFailureRiskScore", false);
				});
			}

			creditReportItemCollection[0].CountryEnabledForCompany = false;

			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			using (var filterControl = GetNewOrganisationFilterControl())
			{
				var columns = filterControl.Grid.ColumnStyles;

				CombineAssertions(() =>
				{
					AssertColumnAvailable(columns, "MiscServ+OM_CCCreditRating", false);
					AssertColumnAvailable(columns, "MiscServ+OM_CCLatePaymentScore", false);
					AssertColumnAvailable(columns, "MiscServ+OM_CCFailureRiskScore", false);
				});
			}
		}

		[RequiresSTA]
		public void TestColumnsCanBeDisplayed()
		{
			using (var filterControl = GetNewOrganisationFilterControl())
			{
				var columns = filterControl.Grid.ColumnStyles;

				CombineAssertions(() =>
				{
					AssertHasColumn(columns, "MainAddressCountryCodes");
					AssertHasColumn(columns, "MainAddress+OA_AdditionalAddressInformation");
					AssertHasColumn(columns, "CompanyData+OB_ARClientNumber");
					AssertHasColumn(columns, "MiscServ+OrgSecurityGroup+GG_Code");
					AssertHasColumn(columns, "EmployerIdentificationNumber");
					AssertHasColumn(columns, "PowerOfAttorneyValidToDate");
					AssertHasColumn(columns, "MiscServ+OM_CRCarrierCategory");
				});
			}
		}

		[RequiresSTA]
		public void TestPowerOfAttorneyValidToDateColumnType()
		{
			using (var filterControl = GetNewOrganisationFilterControl())
			{
				var powerOfAttorneyValidToDateColumn = filterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == "PowerOfAttorneyValidToDate");
				CombineAssertions(() =>
				{
					AssertType<ZDateEditColumnStyleInfo>(powerOfAttorneyValidToDateColumn);
					AssertEquals(ZDateTimePickerFormat.Short, ((ZDateEditColumnStyleInfo)powerOfAttorneyValidToDateColumn).DateTimeFormat);
				});
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);
			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}

		void AssertColumnAvailable(ArrayList columns, string nameOfColumn, bool isAvailable)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Single(column => column.ColumnName == nameOfColumn && column.IsUnavailable == !isAvailable);
			AssertNotEquals(isAvailable, anyColumnHasGivenName.IsUnavailable);
		}

		protected virtual OrganisationFilterControl GetNewOrganisationFilterControl() => new OrganisationFilterControl();
	}
}
