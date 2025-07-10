using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgInvoiceType))]
	sealed class OrgInvoiceTypeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return TestInvoiceType;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return TestInvoiceType;
		}

		public void TestSetDefaultValues()
		{
			var orgInvoiceType = Factory.New<OrgInvoiceType>();
			AssertEquals(InvoiceTypeChargeInclusionTypeList.Codes.ALL, orgInvoiceType.PI_Calc_IsInclude);
			AssertEquals(InvoiceTypeLayoutList.Codes.INV, orgInvoiceType.PI_Type);
			AssertEquals(InvoiceTypeLayoutList.Codes.INV, orgInvoiceType.PI_SecondaryType);
		}

		public void TestDeferredChargesReadOnly()
		{
			TestInvoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.INC;
			TestInvoiceType.DeferredCharges.AddNew();
			Factory.Save();
			AssertDeferredChargesReadOnly(false);

			TestInvoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.EXC;
			Factory.Save();
			AssertDeferredChargesReadOnly(false);

			TestInvoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.ALL;
			Factory.Save();
			AssertDeferredChargesReadOnly(true);
		}

		void AssertDeferredChargesReadOnly(bool expected)
		{
			var newFactory = new BusinessObjectFactory();
			var invoiceTypeInNewFactory = newFactory.Load<OrgInvoiceType>(TestInvoiceType.PK);
			AssertEquals(expected, invoiceTypeInNewFactory.DeferredCharges.ReadOnly);
		}

		[ExpectNoExceptions]
		public void TestPI_TransportMode()
		{
			TestInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			TestInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			TestInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.LMH;
			TestInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.INV;
			TestInvoiceType.PI_ServiceDirection = "ALL";

			var transportModeList = TestInvoiceType.OrgInvoiceTypeLookupHelper.TransportModeList;
			foreach (CodeDescriptionPair transportMode in transportModeList)
			{
				TestInvoiceType.PI_TransportMode = transportMode.Code;
				try
				{
					Factory.Save();
				}
				catch (Exception ex)
				{
					Fail(string.Format("Saving failed for TransportMode: {0} with Exception:{1}{2}", transportMode, System.Environment.NewLine, ex.Message));
				}
			}
		}

		public void TestJobTypeList()
		{
			var jobTypeList = TestInvoiceType.OrgInvoiceTypeLookupHelper.JobTypeList;
			Assert(!jobTypeList.ContainsCode(JobInvoicingConsumerTypes.TransportBooking));
			Assert(!jobTypeList.ContainsCode(JobInvoicingConsumerTypes.ForwardingConsol));
			Assert(!jobTypeList.ContainsCode(JobInvoicingConsumerTypes.ImporterSecurityFiling));

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var testInvoiceTypeUnitedStates = TestHeader.CompanyData.InvoiceTypes.AddNew();
			jobTypeList = testInvoiceTypeUnitedStates.OrgInvoiceTypeLookupHelper.JobTypeList;
			Assert(jobTypeList.ContainsCode(JobInvoicingConsumerTypes.ImporterSecurityFiling));
		}

		public void TestPI_Calc_IsInclude()
		{
			TestInvoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.ALL;
			AssertEquals(true, TestInvoiceType.DeferredCharges.ReadOnly);
			AssertEquals(true, TestInvoiceType.PI_IsInclude);

			TestInvoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.INC;
			AssertEquals(false, TestInvoiceType.DeferredCharges.ReadOnly);
			AssertEquals(true, TestInvoiceType.PI_IsInclude);

			TestInvoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.EXC;
			AssertEquals(false, TestInvoiceType.DeferredCharges.ReadOnly);
			AssertEquals(false, TestInvoiceType.PI_IsInclude);

			var deferredCharge = TestInvoiceType.DeferredCharges.AddNew();
			deferredCharge.PO_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			TestInvoiceType.PI_IsInclude = false;
			Factory.Save();
			AssertEquals(InvoiceTypeChargeInclusionTypeList.Codes.EXC, TestInvoiceType.PI_Calc_IsInclude);

			TestInvoiceType.PI_IsInclude = true;
			Factory.Save();
			AssertEquals(InvoiceTypeChargeInclusionTypeList.Codes.INC, TestInvoiceType.PI_Calc_IsInclude);

			TestInvoiceType.DeferredCharges.RemoveAndDeleteAll();
			TestInvoiceType.PI_IsInclude = true;
			Factory.Save();
			AssertEquals(InvoiceTypeChargeInclusionTypeList.Codes.ALL, TestInvoiceType.PI_Calc_IsInclude);
		}

		public void TestPI_Module()
		{
			TestInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.INV;
			TestInvoiceType.PI_Module = InvoiceTypeModuleList.Codes.MSC;
			AssertHasErrors("The Layout shoult be revalidated and must be 'CHG'", TestInvoiceType.PI_TypeInfo);
		}

		public void TestInvoiceLayoutDescription()
		{
			AssertEquals("Description is Invoice", InvoiceTypeLayoutList.Descriptions.INV, TestInvoiceType.InvoiceLayoutDescription);
			TestInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
			AssertEquals("Description is Charge", InvoiceTypeLayoutList.Descriptions.CHG, TestInvoiceType.InvoiceLayoutDescription);
		}

		public void TestCommenceOnDescription()
		{
			AssertEquals("Description Empty", ZString.Empty, TestInvoiceType.CommenceOnDescription);

			TestInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			TestInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.M01;
			AssertEquals("Description is 1st Day Of Month", InvoiceTypeMonthCommencement.Descriptions.M01, TestInvoiceType.CommenceOnDescription);

			TestInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.WKY;
			TestInvoiceType.PI_StartDay = "MON";
			AssertEquals("Description is Monday", "Monday", TestInvoiceType.CommenceOnDescription);
		}

		public void TestBillingIntervalDescription()
		{
			AssertEquals("Description Empty", ZString.Empty, TestInvoiceType.BillingIntervalDescription);
			TestInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			AssertEquals("Description is Monthly", InvoiceTypeBillingInterval.Descriptions.MTH, TestInvoiceType.BillingIntervalDescription);
		}

		public void TestModuleDescription()
		{
			AssertEquals("Description Empty", ZString.Empty, TestInvoiceType.ModuleDescription);
			TestInvoiceType.PI_Module = InvoiceTypeModuleList.Codes.CUS;
			AssertEquals("Description is Monthly", InvoiceTypeModuleList.Descriptions.CUS, TestInvoiceType.ModuleDescription);
		}

		public void TestSecondaryInvoiceLayout()
		{
			TestInvoiceType.PI_Type = ZString.Empty;
			TestInvoiceType.PI_SecondaryType = ZString.Empty;
			AssertEquals("PI_Type is Empty so is PI_SecondaryType", ZString.Empty, TestInvoiceType.PI_SecondaryType);
			TestInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
			TestInvoiceType.PI_SecondaryType = InvoiceTypeLayoutList.Codes.INV;
			AssertEquals("PI_SecondaryType is Updated", InvoiceTypeLayoutList.Codes.INV, TestInvoiceType.PI_SecondaryType);
			AssertEquals("PI_Type is still CHG", InvoiceTypeLayoutList.Codes.CHG, TestInvoiceType.PI_Type);
		}

		public void TestSecondaryInvoiceLayoutDescription()
		{
			TestInvoiceType.PI_Type = ZString.Empty;
			TestInvoiceType.PI_SecondaryType = ZString.Empty;
			AssertEquals("Description Empty", ZString.Empty, TestInvoiceType.SecondaryInvoiceLayoutDescription);
			TestInvoiceType.PI_SecondaryType = InvoiceTypeLayoutList.Codes.CHG;
			AssertEquals("Secondary Description is Invoice", InvoiceTypeLayoutList.Descriptions.CHG, TestInvoiceType.SecondaryInvoiceLayoutDescription);
		}

		#region TestHeader

		public OrgHeader TestHeader
		{
			get
			{
				if (fTestHeader == null)
				{
					fTestHeader = OrgHeader.New(Factory);
					fTestHeader.FillWithValidTestData();
				}
				return fTestHeader;
			}
		}
		OrgHeader fTestHeader;

		#endregion

		#region TestInvoiceType

		public OrgInvoiceType TestInvoiceType
		{
			get
			{
				if (fTestInvoiceType == null)
				{
					fTestInvoiceType = TestHeader.CompanyData.InvoiceTypes.AddNew();
				}
				return fTestInvoiceType;
			}
		}
		OrgInvoiceType fTestInvoiceType;

		#endregion

		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldARInvoiceValue = Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed;

			try
			{
				OrgInvoiceType testInvoiceType = OrgInDB.CompanyData.InvoiceTypes.AddNew();

				Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testInvoiceType.PI_IntervalInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testInvoiceType.PI_ModuleInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testInvoiceType.PI_StartDayInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testInvoiceType.PI_TypeInfo.ReadOnly);

				Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testInvoiceType.PI_IntervalInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testInvoiceType.PI_ModuleInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testInvoiceType.PI_StartDayInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testInvoiceType.PI_TypeInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgReceivablesModifyInvoiceBatching.IsAllowed = oldARInvoiceValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion
	}
}
