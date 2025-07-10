using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgInvoiceTypeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeInclusionType()
		{
			AssertEquals(3, TestInvoiceType.Lookups.ChargeInclusionType.Count);
			AssertEquals(true, TestInvoiceType.Lookups.ChargeInclusionType.ContainsCode("ALL"));
			AssertEquals(true, TestInvoiceType.Lookups.ChargeInclusionType.ContainsCode("INC"));
			AssertEquals(true, TestInvoiceType.Lookups.ChargeInclusionType.ContainsCode("EXC"));
		}

		public void TestModuleList()
		{
			AssertNotNull(TestInvoiceType.Lookups.ModuleList);
			Assert(!TestInvoiceType.Lookups.ModuleList.ContainsCode(InvoiceTypeModuleList.Codes.MSC));
			Assert(!TestInvoiceType.Lookups.ModuleList.ContainsCode(InvoiceTypeModuleList.Codes.ISF));
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			TestInvoiceType = TestHeader.CompanyData.InvoiceTypes.AddNew();
			Assert(TestInvoiceType.Lookups.ModuleList.ContainsCode(InvoiceTypeModuleList.Codes.ISF));
		}

		public void TestBillingInterval()
		{
			AssertNotNull(TestInvoiceType.Lookups.BillingInterval);
		}

		public void TestCommenceOn()
		{
			TestInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.WKY;
			AssertEquals("Should be days of week", 7, TestInvoiceType.Lookups.CommenceOn.Count);
			Assert(!(TestInvoiceType.Lookups.CommenceOn is InvoiceTypeMonthCommencement));

			TestInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			Assert(TestInvoiceType.Lookups.CommenceOn is InvoiceTypeMonthCommencement);

			TestInvoiceType.PI_Interval = ZString.Empty;
			AssertEquals("Should be empty collection", 0, TestInvoiceType.Lookups.CommenceOn.Count);
		}

		public void TestInvoiceLayout()
		{
			AssertNotNull(TestInvoiceType.Lookups.InvoiceLayout);
		}

		public void TestTransportModeList()
		{
			foreach (CodeDescriptionPair jobType in TestInvoiceType.Lookups.JobTypeList)
			{
				((IOrgInvoiceType)TestInvoiceType).JobType = jobType.Code;
				AssertNotNull(TestInvoiceType.Lookups.TransportModeList);
				if (jobType.Code == JobInvoicingConsumerTypes.Brokerage.Code)
				{
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
					AssertEquals("Number of Transport Mode", 12, TestInvoiceType.Lookups.TransportModeList.Count);

					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
					AssertEquals("Number of Transport Mode", 5, TestInvoiceType.Lookups.TransportModeList.Count);
				}
				else
				{
					AssertEquals("Number of Transport Mode", 8, TestInvoiceType.Lookups.TransportModeList.Count);
				}
			}
		}

		public void TestServiceDirectionList()
		{
			foreach (CodeDescriptionPair jobType in TestInvoiceType.Lookups.JobTypeList)
			{
				((IOrgInvoiceType)TestInvoiceType).JobType = jobType.Code;
				AssertNotNull(TestInvoiceType.Lookups.ServiceDirectionList);
				if (jobType.Code == JobInvoicingConsumerTypes.Brokerage.Code)
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
					{
						AssertEquals("Number of Service Direction", 6, TestInvoiceType.Lookups.ServiceDirectionList.Count);
					}

					using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
					{
						AssertEquals("Number of Service Direction", 10, TestInvoiceType.Lookups.ServiceDirectionList.Count);
					}
				}
				else
				{
					AssertEquals("Number of Service Direction", 5, TestInvoiceType.Lookups.ServiceDirectionList.Count);
				}
			}
		}

		public void TestSecondaryInvoiceLayoutList()
		{
			AssertEquals("Count", 2, TestInvoiceType.Lookups.SecondaryInvoiceLayout.Count);
			Assert("Contains INV", TestInvoiceType.Lookups.SecondaryInvoiceLayout.ContainsCode(InvoiceTypeLayoutList.Codes.INV));
			Assert("Contains CHG", TestInvoiceType.Lookups.SecondaryInvoiceLayout.ContainsCode(InvoiceTypeLayoutList.Codes.CHG));
			Assert("Does not contain NON", !TestInvoiceType.Lookups.SecondaryInvoiceLayout.ContainsCode(InvoiceTypeLayoutList.Codes.NON));
		}

		#region Implementation

		OrgInvoiceType TestInvoiceType;
		OrgHeader TestHeader;

		protected override void SetUp()
		{
			TestHeader = OrgHeader.New(Factory);
			TestHeader.FillWithValidTestData();
			TestInvoiceType = TestHeader.CompanyData.InvoiceTypes.AddNew();

			base.SetUp();
		}

		#endregion
	}
}
