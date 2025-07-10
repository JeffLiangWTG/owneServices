using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeCodeExtensionsTest : TestCaseWithFactory
	{
		#region Get Global Charge Code

		public void TestIsGatewayRelated()
		{
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out AccChargeCode normalChargeCode, out AccChargeCode normalChargeCodeLinkedToGlobalChargeCode, out AccChargeCode globalChargeCode);
			AccChargeCode nullChargeCode = null;

			Assert("Registry is empty", AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.GetAsGuidArray().IsNullOrEmpty());
			Assert(normalChargeCode.IsGatewayRelated());
			Assert(globalChargeCode.IsGatewayRelated());
			Assert(normalChargeCodeLinkedToGlobalChargeCode.IsGatewayRelated());
			Assert(nullChargeCode.IsGatewayRelated());

			using (AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToString()))
			{
				Assert(!normalChargeCode.IsGatewayRelated());
				Assert(globalChargeCode.IsGatewayRelated());
				Assert(normalChargeCodeLinkedToGlobalChargeCode.IsGatewayRelated());
				Assert(!nullChargeCode.IsGatewayRelated());
			}

			using (AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, normalChargeCode.PK.ToString()))
			{
				Assert(normalChargeCode.IsGatewayRelated());
				Assert(!globalChargeCode.IsGatewayRelated());
				Assert(!normalChargeCodeLinkedToGlobalChargeCode.IsGatewayRelated());
				Assert(!nullChargeCode.IsGatewayRelated());
			}

			using (AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToString()))
			using (AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, normalChargeCode.PK.ToString()))
			{
				Assert(normalChargeCode.IsGatewayRelated());
				Assert(!globalChargeCode.IsGatewayRelated());
				Assert(!normalChargeCodeLinkedToGlobalChargeCode.IsGatewayRelated());
				Assert(!nullChargeCode.IsGatewayRelated());
			}

			var chargeCodesForRegistry = new ZStringBuilder(normalChargeCode.PK.ToString()).Append(normalChargeCodeLinkedToGlobalChargeCode.PK.ToString()).ToStringWithDelimiterBetweenAppends(",");
			using (AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToString()))
			using (AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodesForRegistry))
			{
				Assert(normalChargeCode.IsGatewayRelated());
				Assert(!globalChargeCode.IsGatewayRelated());
				Assert(normalChargeCodeLinkedToGlobalChargeCode.IsGatewayRelated());
				Assert(!nullChargeCode.IsGatewayRelated());
			}
		}

		public void TestGetGlobalChargeCode()
		{
			var globalChargeCode = CreateGlobalChargeCode("GLBFRT");
			var localChargeCode = globalChargeCode.ChildChargeCodes.SingleOrDefault(c => c.AC_GC == Env.CurrentCompanyPK);
			var localOnlyChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			AccChargeCode nullChargeCode = null;

			AssertEquals("Should return self when charge code is already global", globalChargeCode, globalChargeCode.GetGlobalChargeCode("", null));
			AssertEquals(globalChargeCode, localChargeCode.GetGlobalChargeCode("", null));
			AssertEquals("Should return self when no global charge code is found", globalChargeCode, localChargeCode.GetGlobalChargeCode("", null));
			AssertNull("Expected to return null rather than throw an exception", nullChargeCode.GetLocalChargeCode("", null));

			globalChargeCode.Delete();
			Factory.Save();

			AssertEquals("If Global Charge Code no longer exists, return self", localOnlyChargeCode, localOnlyChargeCode.GetLocalChargeCode("", null));
		}

		public void TestGetGlobalChargeCode_IntercompanyMapping_AccountsPayable()
		{
			AssertGetGlobalChargeCodePrefersIntercompanyMapping(LedgerTypes.AccountsPayable);
		}

		public void TestGetGlobalChargeCode_IntercompanyMapping_AccountsReceivable()
		{
			AssertGetGlobalChargeCodePrefersIntercompanyMapping(LedgerTypes.AccountsReceivable);
		}

		public void AssertGetGlobalChargeCodePrefersIntercompanyMapping(string invoiceType)
		{
			var clientOverrideOrg = Factory.NewWithValidTestData<OrgHeader>();
			var unrelatedOrg = Factory.NewWithValidTestData<OrgHeader>();

			var globalChargeCode1 = CreateGlobalChargeCode("GLBFRT1");
			var globalChargeCode2 = CreateGlobalChargeCode("GLBFRT2");
			var localChargeCode1 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			var localChargeCode2 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "BAF"));

			var globalChargeCodeMapIntercompany = (BusinessObject)Factory.New<Enterprise.Integration.Accounting.IGlobalChargeCodeMapIntercompany>();
			globalChargeCodeMapIntercompany[AccGlobalChargeCodeMapSchema.YG_Code] = globalChargeCode1.AC_Code;
			var genericGlobalChargeCodeMapPivotIntercompany = (BusinessObject)Factory.New<Enterprise.Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany>();
			genericGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany.PK;
			genericGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_AC] = localChargeCode1.PK;
			genericGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride] = null;
			using (genericGlobalChargeCodeMapPivotIntercompany.GetValidationSuspender())
			{
				genericGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = invoiceType;

				var clientGlobalChargeCodeMapIntercompany = (BusinessObject)Factory.New<Enterprise.Integration.Accounting.IGlobalChargeCodeMapIntercompany>();
				clientGlobalChargeCodeMapIntercompany[AccGlobalChargeCodeMapSchema.YG_Code] = globalChargeCode2.AC_Code;
				var clientGlobalChargeCodeMapPivotIntercompany = (BusinessObject)Factory.New<Enterprise.Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany>();
				clientGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_YG] = clientGlobalChargeCodeMapIntercompany.PK;
				clientGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_AC] = localChargeCode2.PK;
				clientGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride] = clientOverrideOrg.PK;
				using (clientGlobalChargeCodeMapPivotIntercompany.GetValidationSuspender())
				{
					clientGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = invoiceType;
					Factory.Save();

					CombineAssertions(() =>
					{
						var message = "Pre-condition: should be different charge codes";
						AssertNotEquals(message, globalChargeCode1.ChildChargeCodes.Single(x => x.AC_GC == Env.CurrentCompanyPK), localChargeCode1);
						AssertNotEquals(message, globalChargeCode2.ChildChargeCodes.Single(x => x.AC_GC == Env.CurrentCompanyPK), localChargeCode2);

						message = "If no intercompany mapping exists for this invoice type, fall back to self";
						AssertEquals(message, globalChargeCode1, localChargeCode1.GetGlobalChargeCode(invoiceType, null));

						var otherInvoiceType = invoiceType == LedgerTypes.AccountsPayable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
						AssertEquals(message, localChargeCode1, localChargeCode1.GetGlobalChargeCode(otherInvoiceType, null));

						message = "If there is no client intercompany mapping, we still fall back to the generic intercompany mapping";
						AssertEquals(message, globalChargeCode1, localChargeCode1.GetGlobalChargeCode(invoiceType, unrelatedOrg.PK));

						message = "If no ledger type is provided fall back to self as ledger type is a mandatory column in the database";
						AssertEquals(message, localChargeCode1, localChargeCode1.GetGlobalChargeCode("", null));

						message = "If a client intercompany mapping exists, it should be prefered to the generic intercompany mapping";
						AssertEquals(message, globalChargeCode2, localChargeCode2.GetGlobalChargeCode(invoiceType, clientOverrideOrg.PK));
					});
				}
			}
		}

		#endregion

		#region Get Local Charge Code

		public void TestGetLocalChargeCode()
		{
			var globalChargeCode = CreateGlobalChargeCode("GLBFRT");
			var localChargeCode = globalChargeCode.ChildChargeCodes.Single(x => x.AC_GC == Env.CurrentCompanyPK);
			AccChargeCode nullChargeCode = null;

			AssertEquals(localChargeCode, globalChargeCode.GetLocalChargeCode("", null));
			AssertEquals("Should return self when charge code is already local", localChargeCode, localChargeCode.GetLocalChargeCode("", null));
			AssertNull("Expected to return null rather than throw an exception", nullChargeCode.GetLocalChargeCode("", null));

			localChargeCode.Delete();
			Factory.Save();

			AssertEquals("If Local Charge Code no longer exists, return self", globalChargeCode, globalChargeCode.GetLocalChargeCode("", null));
		}

		public void TestGetLocalChargeCode_IntercompanyMapping_AccountsPayable()
		{
			AssertGetLocalChargeCodePrefersIntercompanyMapping(LedgerTypes.AccountsPayable);
		}

		public void TestGetLocalChargeCode_IntercompanyMapping_AccountsReceivable()
		{
			AssertGetLocalChargeCodePrefersIntercompanyMapping(LedgerTypes.AccountsReceivable);
		}

		public void AssertGetLocalChargeCodePrefersIntercompanyMapping(string invoiceType)
		{
			var clientOverrideOrg = Factory.NewWithValidTestData<OrgHeader>();
			var unrelatedOrg = Factory.NewWithValidTestData<OrgHeader>();

			var globalChargeCode = CreateGlobalChargeCode("GLBFRT");
			var genericIntercompanyMappedChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			var clientSpecificIntercompanyMappedChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "BAF"));

			var globalChargeCodeMapIntercompany = (BusinessObject)Factory.New<Enterprise.Integration.Accounting.IGlobalChargeCodeMapIntercompany>();
			globalChargeCodeMapIntercompany[AccGlobalChargeCodeMapSchema.YG_Code] = globalChargeCode.AC_Code;

			var genericGlobalChargeCodeMapPivotIntercompany = (BusinessObject)Factory.New<Enterprise.Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany>();
			genericGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany.PK;
			genericGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_AC] = genericIntercompanyMappedChargeCode.PK;
			genericGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride] = null;
			using (genericGlobalChargeCodeMapPivotIntercompany.GetValidationSuspender())
			{
				genericGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = invoiceType;

				var clientGlobalChargeCodeMapPivotIntercompany = (BusinessObject)Factory.New<Enterprise.Integration.Accounting.IGlobalChargeCodeMapPivotIntercompany>();
				clientGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany.PK;
				clientGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_AC] = clientSpecificIntercompanyMappedChargeCode.PK;
				clientGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride] = clientOverrideOrg.PK;
				using (clientGlobalChargeCodeMapPivotIntercompany.GetValidationSuspender())
				{
					clientGlobalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = invoiceType;
					Factory.Save();

					CombineAssertions(() =>
					{
						var localChargeCode = globalChargeCode.ChildChargeCodes.Single(x => x.AC_GC == Env.CurrentCompanyPK);

						var message = "Pre-condition: should be different charge codes";
						AssertNotEquals(message, localChargeCode, genericIntercompanyMappedChargeCode);
						AssertNotEquals(message, localChargeCode, clientSpecificIntercompanyMappedChargeCode);

						message = "If no intercompany mapping exists for this invoice type, fall back to local charge code";
						AssertEquals(message, genericIntercompanyMappedChargeCode, globalChargeCode.GetLocalChargeCode(invoiceType, null));

						message = "If there is no client intercompany mapping, we should fall back to the generic intercompany mapping";
						AssertEquals(message, genericIntercompanyMappedChargeCode, globalChargeCode.GetLocalChargeCode(invoiceType, unrelatedOrg.PK));

						message = "If a client intercompany mapping exists, it should be prefered to the generic intercompany mapping";
						AssertEquals(message, clientSpecificIntercompanyMappedChargeCode, globalChargeCode.GetLocalChargeCode(invoiceType, clientOverrideOrg.PK));

						message = "If no intercompany mapping exists for this invoice type, fall back to local charge code";
						var otherInvoiceType = invoiceType == LedgerTypes.AccountsPayable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
						AssertEquals(message, localChargeCode, globalChargeCode.GetLocalChargeCode(otherInvoiceType, null));

						message = "If no ledger type is provided fall back to local charge code as ledger type is a mandatory column in the database";
						AssertEquals(message, localChargeCode, globalChargeCode.GetLocalChargeCode("", null));
					});
				}
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var singaporeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			singaporeCompany.GC_IsGSTRegistered = false;
			Factory.Save();
		}

		AccChargeCode CreateGlobalChargeCode(string code)
		{
			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = code;
			globalChargeCode.AC_Desc = code + " Global Charge";
			globalChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			globalChargeCode.Factory.Save();

			return globalChargeCode;
		}

		#endregion
	}
}
