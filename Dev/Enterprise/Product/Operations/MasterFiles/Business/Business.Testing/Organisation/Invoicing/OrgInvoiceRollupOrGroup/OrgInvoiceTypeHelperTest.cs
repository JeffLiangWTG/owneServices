using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgInvoiceTypeHelperTest : TestCaseWithFactory
	{
		public void TestTransportModeList()
		{
			var helper = new OrgInvoiceTypeHelper(TestInvoiceType);
			((IOrgInvoiceType)TestInvoiceType).JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertEquals("ServiceDirectionList.Count", 8, helper.TransportModeList.Count);
			((IOrgInvoiceType)TestInvoiceType).JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("ServiceDirectionList.Count", 12, helper.TransportModeList.Count);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AssertEquals("ServiceDirectionList.Count", 5, helper.TransportModeList.Count);
		}

		public void TestServiceDirectionList()
		{
			var helper = new OrgInvoiceTypeHelper(TestInvoiceType);
			((IOrgInvoiceType)TestInvoiceType).JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertEquals("ServiceDirectionList.Count", 5, helper.ServiceDirectionList.Count);
			((IOrgInvoiceType)TestInvoiceType).JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				AssertEquals("ServiceDirectionList.Count", 6, helper.ServiceDirectionList.Count);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("ServiceDirectionList.Count", 10, helper.ServiceDirectionList.Count);
			}
		}

		public void TestServiceLevelList()
		{
			var helper = new OrgInvoiceTypeHelper(TestInvoiceType);
			AssertEquals("ServiceLevelList.Count", 6, helper.ServiceLevelList.Count);

			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "xxx";
			serviceLevel.RS_Description = "Description";
			serviceLevel.RS_IsActive = false;

			var newServiceLevel = Factory.New<RefServiceLevel>();
			newServiceLevel.RS_Code = "yyy";
			newServiceLevel.RS_Description = "Description";
			newServiceLevel.RS_IsActive = true;

			Factory.Save();

			AssertEquals("ServiceLevelList.Count", 7, helper.ServiceLevelList.Count);
			AssertEquals(ZString.Empty, helper.ServiceLevelList[0].Code);
			Assert(helper.ServiceLevelList.Contains(newServiceLevel));
		}

		public void TestServiceLevel_ReadOnly()
		{
			var helper = new OrgInvoiceTypeHelper(TestInvoiceType);
			supportServiceLevelList.ForEach(x =>
				{
					((IOrgInvoiceType)TestInvoiceType).JobType = x;
					AssertEquals(false, helper.ServiceLevel_ReadOnly);
				});
		}

		public void TestOnSettingJobType()
		{
			const string expectedCode = "STD";
			var helper = new OrgInvoiceTypeHelper(TestInvoiceType);

			supportServiceLevelList.ForEach(x =>
				{
					helper.OnSettingJobType(x);
					TestInvoiceType.PI_RS_NKServiceLevel = expectedCode;
					AssertEquals(expectedCode, TestInvoiceType.PI_RS_NKServiceLevel);
				});

			TestInvoiceType.PI_RS_NKServiceLevel = expectedCode;
			helper.OnSettingJobType(JobInvoicingConsumerTypes.WorkItemCode);
			AssertEquals(ZString.Empty, TestInvoiceType.PI_RS_NKServiceLevel);
		}

		OrgInvoiceType TestInvoiceType;
		OrgHeader TestHeader;
		List<string> supportServiceLevelList;

		protected override void SetUp()
		{
			base.SetUp();
			TestHeader = Factory.NewWithValidTestData<OrgHeader>();
			TestInvoiceType = TestHeader.CompanyData.InvoiceTypes.AddNew();
			supportServiceLevelList = new List<string>()
			{
				JobInvoicingConsumerTypes.AgencyBookingCode,
				JobInvoicingConsumerTypes.AgencyBillOfLadingCode,
				JobInvoicingConsumerTypes.CFSShipmentCode,
				JobInvoicingConsumerTypes.CFSShipmentCode,
				JobInvoicingConsumerTypes.BrokerageCode,
				JobInvoicingConsumerTypes.TransportConsignmentCode,
				JobInvoicingConsumerTypes.QuotedBookingCode,
				JobInvoicingConsumerTypes.LocalCartageCode,
				JobInvoicingConsumerTypes.WarehouseInwardsCode,
				JobInvoicingConsumerTypes.WarehouseOutwardsCode
			};
		}
	}
}
