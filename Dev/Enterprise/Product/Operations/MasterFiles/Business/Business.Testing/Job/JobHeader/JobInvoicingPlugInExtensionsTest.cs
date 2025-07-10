using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobInvoicingPlugInExtensionsTest : TestCaseWithFactory
	{
		public void TestIsCreditLimitCheckRequired()
		{
			var items = new OrgsEvaluatedForCreditControlCollection();
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "SHP", DirectionCode = "ALL", Mode = "ALL", INCOTerm = "ALL", FreightPaymentTerm = "ALL", OrganizationType = "LOC" });

			using (AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, items))
			{
				var shipment = CreateJobWithParent(typeof(Enterprise.Integration.Forwarding.IForwardingShipment), JobShipmentSchema.Constants.Prefix);
				var isCreditLimitCheckRequired = (shipment as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.LocalClient);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);

				var consol = CreateJobWithParent(typeof(Enterprise.Integration.Forwarding.IForwardingConsol), JobConsolSchema.Constants.Prefix);
				isCreditLimitCheckRequired = (consol as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.SendingAgent);
				Assert("CreditLimt Check is required", isCreditLimitCheckRequired);
			}
		}

		public void TestIsCreditLimitCheckRequired_INCOTerm()
		{
			var items = new OrgsEvaluatedForCreditControlCollection();
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "SHP", DirectionCode = "EXP", Mode = "ALL", INCOTerm = "EXW", FreightPaymentTerm = "ALL", OrganizationType = "ALL" });
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "SHP", DirectionCode = "DOM", Mode = "ALL", INCOTerm = "C3P", FreightPaymentTerm = "ALL", OrganizationType = "ALL" });
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "QSH", DirectionCode = "IMP", Mode = "ALL", INCOTerm = "FOB", FreightPaymentTerm = "ALL", OrganizationType = "ALL" });
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "QSH", DirectionCode = "DOM", Mode = "ALL", INCOTerm = "PPD", FreightPaymentTerm = "ALL", OrganizationType = "ALL" });
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "BRK", DirectionCode = "", Mode = "", INCOTerm = "DAP", FreightPaymentTerm = "ALL", OrganizationType = "ALL" });
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "AGS", DirectionCode = "ALL", Mode = "", INCOTerm = "", FreightPaymentTerm = "ALL", OrganizationType = "ALL" });
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "FCN", DirectionCode = "ALL", Mode = "ALL", INCOTerm = "", FreightPaymentTerm = "ALL", OrganizationType = "ALL" });

			using (AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, items))
			{
				//Shipment
				var shipment1 = CreateJobWithParent(typeof(Enterprise.Integration.Forwarding.IForwardingShipment), JobShipmentSchema.Constants.Prefix);
				shipment1[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
				shipment1[JobShipmentSchema.JS_RL_NKDestination] = "NZAKL";
				shipment1[JobShipmentSchema.JS_INCO] = "EXW";
				var isCreditLimitCheckRequired = (shipment1 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);

				var shipment2 = CreateJobWithParent(typeof(Enterprise.Integration.Forwarding.IForwardingShipment), JobShipmentSchema.Constants.Prefix);
				shipment2[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
				shipment2[JobShipmentSchema.JS_RL_NKDestination] = "AUMEL";
				shipment2[JobShipmentSchema.JS_INCO] = "C3P";
				isCreditLimitCheckRequired = (shipment2 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);

				var shipment3 = CreateJobWithParent(typeof(Enterprise.Integration.Forwarding.IForwardingShipment), JobShipmentSchema.Constants.Prefix);
				shipment3[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
				shipment3[JobShipmentSchema.JS_RL_NKDestination] = "NZAKL";
				shipment3[JobShipmentSchema.JS_INCO] = "DES";
				isCreditLimitCheckRequired = (shipment3 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is required", isCreditLimitCheckRequired);

				//QuotedBooking
				var booking1 = CreateJobWithQuotedBooking(RatingHeaderSchema.Constants.Prefix);
				booking1["Origin"] = "NZAKL";
				booking1["Destination"] = "AUSYD";
				booking1["PaymentTerms"] = "FOB";
				isCreditLimitCheckRequired = (booking1 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);

				var booking2 = CreateJobWithQuotedBooking(RatingHeaderSchema.Constants.Prefix);
				booking2["Origin"] = "NZAKL";
				booking2["Destination"] = "NZAKL";
				booking2["PaymentTerms"] = "PPD";
				isCreditLimitCheckRequired = (booking2 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);

				var booking3 = CreateJobWithQuotedBooking(RatingHeaderSchema.Constants.Prefix);
				booking3["Origin"] = "NZAKL";
				booking3["Destination"] = "AUSYD";
				booking3["PaymentTerms"] = "DES";
				isCreditLimitCheckRequired = (booking3 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is required", isCreditLimitCheckRequired);

				//Declaration
				var declaration1 = CreateJobWithParent(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.Constants.Prefix);
				declaration1[JobDeclarationSchema.JE_ShipmentIncoTerm] = "DAP";
				isCreditLimitCheckRequired = (declaration1 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);

				var declaration2 = CreateJobWithParent(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.Constants.Prefix);
				declaration2[JobDeclarationSchema.JE_ShipmentIncoTerm] = "DES";
				isCreditLimitCheckRequired = (declaration2 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is required", isCreditLimitCheckRequired);

				//BillOfLading
				var billOfLading = CreateJobWithParent(typeof(Freight.Integration.Agency.IBillOfLading), JobShipmentSchema.Constants.Prefix);
				billOfLading[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
				billOfLading[JobShipmentSchema.JS_RL_NKDestination] = "NZAKL";
				billOfLading[JobShipmentSchema.JS_INCO] = "CLT";
				isCreditLimitCheckRequired = (billOfLading as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);

				//Consol
				var consol = CreateJobWithParent(typeof(Enterprise.Integration.Forwarding.IForwardingConsol), JobConsolSchema.Constants.Prefix);
				consol[JobConsolSchema.JK_PrepaidCollect] = "CCX";
				isCreditLimitCheckRequired = (consol as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.SendingAgent);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);
			}
		}

		public void TestIsCreditLimitCheckRequired_PaymentTerm()
		{
			var items = new OrgsEvaluatedForCreditControlCollection();
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "SHP", DirectionCode = "EXP", Mode = "ALL", INCOTerm = "ALL", FreightPaymentTerm = "CCX", OrganizationType = "ALL" });
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "SHP", DirectionCode = "DOM", Mode = "ALL", INCOTerm = "ALL", FreightPaymentTerm = "CCX", OrganizationType = "ALL" });
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "QSH", DirectionCode = "IMP", Mode = "ALL", INCOTerm = "ALL", FreightPaymentTerm = "CCX", OrganizationType = "ALL" });
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "QSH", DirectionCode = "DOM", Mode = "ALL", INCOTerm = "ALL", FreightPaymentTerm = "CCX", OrganizationType = "ALL" });
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "BRK", DirectionCode = "", Mode = "", INCOTerm = "ALL", FreightPaymentTerm = "CCX", OrganizationType = "ALL" });
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "AGS", DirectionCode = "ALL", Mode = "", INCOTerm = "ALL", FreightPaymentTerm = "PPD", OrganizationType = "ALL" });
			items.Add(new OrgsEvaluatedForCreditControl() { JobType = "FCN", DirectionCode = "ALL", Mode = "ALL", INCOTerm = "ALL", FreightPaymentTerm = "PPD", OrganizationType = "ALL" });

			using (AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, items))
			{
				//Shipment
				var shipment1 = CreateJobWithParent(typeof(Enterprise.Integration.Forwarding.IForwardingShipment), JobShipmentSchema.Constants.Prefix);
				shipment1[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
				shipment1[JobShipmentSchema.JS_RL_NKDestination] = "NZAKL";
				shipment1[JobShipmentSchema.JS_INCO] = "FCA";
				var isCreditLimitCheckRequired = (shipment1 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);

				var shipment2 = CreateJobWithParent(typeof(Enterprise.Integration.Forwarding.IForwardingShipment), JobShipmentSchema.Constants.Prefix);
				shipment2[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
				shipment2[JobShipmentSchema.JS_RL_NKDestination] = "AUMEL";
				shipment2[JobShipmentSchema.JS_INCO] = "C3P";
				isCreditLimitCheckRequired = (shipment2 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);

				var shipment3 = CreateJobWithParent(typeof(Enterprise.Integration.Forwarding.IForwardingShipment), JobShipmentSchema.Constants.Prefix);
				shipment3[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
				shipment3[JobShipmentSchema.JS_RL_NKDestination] = "NZAKL";
				shipment3[JobShipmentSchema.JS_INCO] = "DES";
				isCreditLimitCheckRequired = (shipment3 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is required", isCreditLimitCheckRequired);

				//QuotedBooking
				var booking1 = CreateJobWithQuotedBooking(RatingHeaderSchema.Constants.Prefix);
				booking1["Origin"] = "NZAKL";
				booking1["Destination"] = "AUSYD";
				booking1["PaymentTerms"] = "FCA";
				isCreditLimitCheckRequired = (booking1 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);

				var booking2 = CreateJobWithQuotedBooking(RatingHeaderSchema.Constants.Prefix);
				booking2["Origin"] = "NZAKL";
				booking2["Destination"] = "NZAKL";
				booking2["PaymentTerms"] = "FCD";
				isCreditLimitCheckRequired = (booking2 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);

				var booking3 = CreateJobWithQuotedBooking(RatingHeaderSchema.Constants.Prefix);
				booking3["Origin"] = "NZAKL";
				booking3["Destination"] = "AUSYD";
				booking3["PaymentTerms"] = "DES";
				isCreditLimitCheckRequired = (booking3 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is required", isCreditLimitCheckRequired);

				//Declaration
				var declaration1 = CreateJobWithParent(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.Constants.Prefix);
				declaration1[JobDeclarationSchema.JE_ShipmentIncoTerm] = "FCA";
				isCreditLimitCheckRequired = (declaration1 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);

				var declaration2 = CreateJobWithParent(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.Constants.Prefix);
				declaration2[JobDeclarationSchema.JE_ShipmentIncoTerm] = "DES";
				isCreditLimitCheckRequired = (declaration2 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is required", isCreditLimitCheckRequired);

				//BillOfLading
				var billOfLading1 = CreateJobWithParent(typeof(Freight.Integration.Agency.IBillOfLading), JobShipmentSchema.Constants.Prefix);
				billOfLading1[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
				billOfLading1[JobShipmentSchema.JS_RL_NKDestination] = "NZAKL";
				billOfLading1[JobShipmentSchema.JS_INCO] = "PPD";
				isCreditLimitCheckRequired = (billOfLading1 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);

				var billOfLading2 = CreateJobWithParent(typeof(Freight.Integration.Agency.IBillOfLading), JobShipmentSchema.Constants.Prefix);
				billOfLading2[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
				billOfLading2[JobShipmentSchema.JS_RL_NKDestination] = "NZAKL";
				billOfLading2[JobShipmentSchema.JS_INCO] = "CLT";
				isCreditLimitCheckRequired = (billOfLading2 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.AllDebtors);
				Assert("CreditLimt Check is required", isCreditLimitCheckRequired);

				//Consol
				var consol1 = CreateJobWithParent(typeof(Enterprise.Integration.Forwarding.IForwardingConsol), JobConsolSchema.Constants.Prefix);
				consol1[JobConsolSchema.JK_PrepaidCollect] = "PPD";
				isCreditLimitCheckRequired = (consol1 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.SendingAgent);
				Assert("CreditLimt Check is not required", !isCreditLimitCheckRequired);

				var consol2 = CreateJobWithParent(typeof(Enterprise.Integration.Forwarding.IForwardingConsol), JobConsolSchema.Constants.Prefix);
				consol2[JobConsolSchema.JK_PrepaidCollect] = "CCX";
				isCreditLimitCheckRequired = (consol2 as IJobInvoicingPlugIn).IsCreditLimitCheckRequired(OrgCodes.SendingAgent);
				Assert("CreditLimt Check is required", isCreditLimitCheckRequired);
			}
		}

		public void TestIncoTermCodeAndDescription()
		{
			//Shipment
			var shipment = CreateJobWithParent(typeof(Enterprise.Integration.Forwarding.IForwardingShipment), JobShipmentSchema.Constants.Prefix);
			shipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			shipment[JobShipmentSchema.JS_RL_NKDestination] = "NZAKL";
			shipment[JobShipmentSchema.JS_INCO] = Core.Constants.IncoTerms.FreeCarrier;
			AssertEquals(Core.Constants.IncoTerms.FreeCarrier, (shipment as IJobInvoicingPlugIn).GetINCOTermCode());
			AssertEquals("FCA - Free Carrier (seller is responsible for origin, buyer for loading)", (shipment as IJobInvoicingPlugIn).GetINCOTermDescription(Factory));

			var domesticShipment = CreateJobWithParent(typeof(Enterprise.Integration.Forwarding.IForwardingShipment), JobShipmentSchema.Constants.Prefix);
			domesticShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			domesticShipment[JobShipmentSchema.JS_RL_NKDestination] = "AUMEL";
			domesticShipment[JobShipmentSchema.JS_INCO] = Core.Constants.DomesticPaymentTerms.CollectThirdParty;
			AssertEquals(Core.Constants.DomesticPaymentTerms.CollectThirdParty, (domesticShipment as IJobInvoicingPlugIn).GetINCOTermCode());
			AssertEquals("Collect 3rd Party", (domesticShipment as IJobInvoicingPlugIn).GetINCOTermDescription(Factory));

			//QuotedBooking
			var quotedBooking = CreateJobWithQuotedBooking(RatingHeaderSchema.Constants.Prefix);
			quotedBooking["Origin"] = "NZAKL";
			quotedBooking["Destination"] = "AUSYD";
			quotedBooking["PaymentTerms"] = Core.Constants.IncoTerms.FreeCarrier;
			AssertEquals(Core.Constants.IncoTerms.FreeCarrier, (quotedBooking as IJobInvoicingPlugIn).GetINCOTermCode());
			AssertEquals("FCA - Free Carrier (seller is responsible for origin, buyer for loading)", (quotedBooking as IJobInvoicingPlugIn).GetINCOTermDescription(Factory));

			//Declaration
			var jobDeclaration = CreateJobWithParent(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.Constants.Prefix);
			jobDeclaration[JobDeclarationSchema.JE_ShipmentIncoTerm] = Core.Constants.IncoTerms.FreeCarrier;
			AssertEquals(Core.Constants.IncoTerms.FreeCarrier, (jobDeclaration as IJobInvoicingPlugIn).GetINCOTermCode());
			AssertEquals("FCA - Free Carrier (seller is responsible for origin, buyer for loading)", (jobDeclaration as IJobInvoicingPlugIn).GetINCOTermDescription(Factory));

			//BillOfLading
			var billOfLading = CreateJobWithParent(typeof(Freight.Integration.Agency.IBillOfLading), JobShipmentSchema.Constants.Prefix);
			billOfLading[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			billOfLading[JobShipmentSchema.JS_RL_NKDestination] = "NZAKL";
			billOfLading[JobShipmentSchema.JS_INCO] = Core.Constants.PaymentType.Prepaid;
			AssertEquals(Core.Constants.PaymentType.Prepaid, (billOfLading as IJobInvoicingPlugIn).GetINCOTermCode());
			AssertEquals("Prepaid", (billOfLading as IJobInvoicingPlugIn).GetINCOTermDescription(Factory));

			//Consol
			var consol = CreateJobWithParent(typeof(Enterprise.Integration.Forwarding.IForwardingConsol), JobConsolSchema.Constants.Prefix);
			consol[JobConsolSchema.JK_PrepaidCollect] = Core.Constants.PaymentType.Prepaid;
			AssertEquals(Core.Constants.PaymentType.Prepaid, (consol as IJobInvoicingPlugIn).GetINCOTermCode());
			AssertEquals("Prepaid", (consol as IJobInvoicingPlugIn).GetINCOTermDescription(Factory));
		}

		BusinessObject CreateJobWithParent(Type type, string tableCode)
		{
			var parent = Factory.NewWithValidTestData(ObjectFactory.GetType(type));
			CreateJobForParent(parent, tableCode);
			Factory.Save();
			return parent;
		}

		BusinessObject CreateJobWithQuotedBooking(string tableCode)
		{
			var parent = ObjectFactory.Get<Freight.Integration.QuotedBooking.IQuotedBookingBuilder>().CreateNew(Freight.Integration.QuoteBookingType.QuickBooking, Factory) as BusinessObject;
			CreateJobForParent(parent, tableCode);
			Factory.Save();
			return parent;
		}

		void CreateJobForParent(BusinessObject parent, string tableCode)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_ParentID = parent.PK;
			job.JH_ParentTableCode = tableCode;
			job.IsManuallyCreated = true;
		}
	}
}
