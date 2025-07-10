using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class OrgsEvaluatedForCreditControlLookupsTest : JobConfigurationSelectorLookupsTest
	{
		public new void TestJobTypeList()
		{
			base.TestJobTypeList();

			var orgsEvaluatedForCreditControlLookups = new OrgsEvaluatedForCreditControlLookups(BizObj);
			AssertEquals(50, orgsEvaluatedForCreditControlLookups.JobTypeList.Count);
			AssertCollectionNotContains("The JobTypeList should not contain 'ABK'", JobInvoicingConsumerTypes.AgentBookingCode, orgsEvaluatedForCreditControlLookups.JobTypeList);
			AssertCollectionNotContains("The JobTypeList should not contain 'ATB'", JobInvoicingConsumerTypes.TransportBookingWithAgentCode, orgsEvaluatedForCreditControlLookups.JobTypeList);
			AssertCollectionNotContains("The JobTypeList should not contain 'TBM'", JobInvoicingConsumerTypes.TransportBookingCode, orgsEvaluatedForCreditControlLookups.JobTypeList);
		}

		public void TestGetJobTypeListWithoutTypeAll()
		{
			var jobTypeListWithoutTypeAll = OrgsEvaluatedForCreditControlLookups.GetJobTypeListWithoutTypeAll();
			AssertEquals(49, jobTypeListWithoutTypeAll.Count);
			AssertCollectionNotContains("The JobTypeList should not contain 'ABK'", JobInvoicingConsumerTypes.AgentBookingCode, jobTypeListWithoutTypeAll);
			AssertCollectionNotContains("The JobTypeList should not contain 'ATB'", JobInvoicingConsumerTypes.TransportBookingWithAgentCode, jobTypeListWithoutTypeAll);
			AssertCollectionNotContains("The JobTypeList should not contain 'TBM'", JobInvoicingConsumerTypes.TransportBookingCode, jobTypeListWithoutTypeAll);
			AssertCollectionNotContains("The JobTypeList should not contain 'ALL'", JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, jobTypeListWithoutTypeAll);
		}

		public void TestOrganizationTypePermittedForShipmentsList()
		{
			AssertEquals(false, AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(false, AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			BizObj.CurrentFallbackLevel = null;
			AssertOrganizationTypes(OrganizationTypePermittedForShipmentsList, 9);
			
			BizObj.CurrentFallbackLevel = new ZArchitecture.Environment.FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertOrganizationTypes(OrganizationTypePermittedForShipmentsList, 7);
			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertOrganizationTypes(OrganizationTypePermittedForShipmentsList, 9);

			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BizObj.CurrentFallbackLevel = new ZArchitecture.Environment.FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertOrganizationTypes(OrganizationTypePermittedForShipmentsList, 7);
			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertOrganizationTypes(OrganizationTypePermittedForShipmentsList, 9);
			
			void AssertOrganizationTypes(CodeDescriptionPairList list, int count)
			{
				AssertEquals("OrganizationTypePermittedForShipmentsList.Count", count, list.Count);
				AssertEquals("OrganizationTypePermittedForShipmentsList should contain 'Local Client'", true, list.ContainsCode(OrgCodes.LocalClient));
				AssertEquals("OrganizationTypePermittedForShipmentsList should contain 'Consignee'", true, list.ContainsCode(OrgCodes.Consignee));
				AssertEquals("OrganizationTypePermittedForShipmentsList should contain 'Consignor'", true, list.ContainsCode(OrgCodes.Consignor));
				AssertEquals("OrganizationTypePermittedForShipmentsList should contain 'Sending Agent'", true, list.ContainsCode(OrgCodes.SendingAgent));
				AssertEquals("OrganizationTypePermittedForShipmentsList should contain 'Receiving Agent'", true, list.ContainsCode(OrgCodes.ReceivingAgent));
				AssertEquals("OrganizationTypePermittedForShipmentsList should contain 'All Debtors'", true, list.ContainsCode(OrgCodes.AllDebtors));

				if (count == 9)
				{
					AssertEquals("OrganizationTypePermittedForShipmentsList should contain 'Local Client'", true, list.ContainsCode(OrgCodes.LocalClient));
					AssertEquals("OrganizationTypePermittedForShipmentsList should contain 'ControllingCustomer'", true, list.ContainsCode(OrgCodes.ControllingCustomer));
				}
			}
		}

		public void TestOrganizationTypePermittedForDeclarationsList()
		{
			AssertEquals(false, AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(false, AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			BizObj.CurrentFallbackLevel = null;
			AssertOrganizationTypes(OrganizationTypePermittedForDeclarationsList, 8);

			BizObj.CurrentFallbackLevel = new ZArchitecture.Environment.FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertOrganizationTypes(OrganizationTypePermittedForDeclarationsList, 6);
			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertOrganizationTypes(OrganizationTypePermittedForDeclarationsList, 8);

			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BizObj.CurrentFallbackLevel = new ZArchitecture.Environment.FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertOrganizationTypes(OrganizationTypePermittedForDeclarationsList, 6);
			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertOrganizationTypes(OrganizationTypePermittedForDeclarationsList, 8);
			void AssertOrganizationTypes(CodeDescriptionPairList list, int count)
			{
				AssertEquals("OrganizationTypePermittedForDeclarationsList.Count", count, list.Count);
				AssertEquals("OrganizationTypePermittedForDeclarationsList should contain 'Local Client'", true, list.ContainsCode(OrgCodes.LocalClient));
				AssertEquals("OrganizationTypePermittedForDeclarationsList should contain 'Consignee'", true, list.ContainsCode(OrgCodes.Consignee));
				AssertEquals("OrganizationTypePermittedForDeclarationsList should contain 'Consignor'", true, list.ContainsCode(OrgCodes.Consignor));
				AssertEquals("OrganizationTypePermittedForDeclarationsList should contain 'Overseas Agent'", true, list.ContainsCode(OrgCodes.OverseasAgent));
				AssertEquals("OrganizationTypePermittedForDeclarationsList should contain 'All Debtors'", true, list.ContainsCode(OrgCodes.AllDebtors));

				if (count == 8)
				{
					AssertEquals("OrganizationTypePermittedForShipmentsList should contain 'ControllingAgent'", true, OrganizationTypePermittedForShipmentsList.ContainsCode(OrgCodes.ControllingAgent));
					AssertEquals("OrganizationTypePermittedForShipmentsList should contain 'ControllingCustomer'", true, OrganizationTypePermittedForShipmentsList.ContainsCode(OrgCodes.ControllingCustomer));
				}
			}
		}

		public void TestOrganizationTypePermittedForBookingsList()
		{
			AssertEquals(false, AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals(false, AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			BizObj.CurrentFallbackLevel = null;
			AssertOrganizationTypes(OrganizationTypePermittedForBookingsList, 8);

			BizObj.CurrentFallbackLevel = new ZArchitecture.Environment.FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertOrganizationTypes(OrganizationTypePermittedForBookingsList, 6);
			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertOrganizationTypes(OrganizationTypePermittedForBookingsList, 8);

			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BizObj.CurrentFallbackLevel = new ZArchitecture.Environment.FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertOrganizationTypes(OrganizationTypePermittedForBookingsList, 6);
			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertOrganizationTypes(OrganizationTypePermittedForBookingsList, 8);
			void AssertOrganizationTypes(CodeDescriptionPairList list, int count)
			{
				AssertEquals("OrganizationTypePermittedForBookingsList.Count", count, OrganizationTypePermittedForBookingsList.Count);
				AssertEquals("OrganizationTypePermittedForBookingsList should contain 'Local Client'", true, OrganizationTypePermittedForBookingsList.ContainsCode(OrgCodes.LocalClient));
				AssertEquals("OrganizationTypePermittedForBookingsList should contain 'Importer'", true, OrganizationTypePermittedForBookingsList.ContainsCode(OrgCodes.Importer));
				AssertEquals("OrganizationTypePermittedForBookingsList should contain 'Supplier'", true, OrganizationTypePermittedForBookingsList.ContainsCode(OrgCodes.Supplier));
				AssertEquals("OrganizationTypePermittedForBookingsList should not contain 'Overseas Agent'", true, OrganizationTypePermittedForBookingsList.ContainsCode(OrgCodes.OverseasAgent));
				AssertEquals("OrganizationTypePermittedForBookingsList should contain 'All Debtors'", true, OrganizationTypePermittedForBookingsList.ContainsCode(OrgCodes.AllDebtors));

				if (count == 8)
				{
					AssertEquals("OrganizationTypePermittedForShipmentsList should contain 'ControllingAgent'", true, OrganizationTypePermittedForBookingsList.ContainsCode(OrgCodes.ControllingAgent));
					AssertEquals("OrganizationTypePermittedForShipmentsList should contain 'ControllingCustomer'", true, OrganizationTypePermittedForBookingsList.ContainsCode(OrgCodes.ControllingCustomer));
				}
			}
		}

		public void TestOrganizationTypePermittedForCFSsList()
		{
			AssertEquals("OrganizationTypePermittedForCFSsList.Count", 5, OrganizationTypePermittedForCFSsList.Count);
			AssertEquals("OrganizationTypePermittedForCFSsList should contain 'Local Client'", true, OrganizationTypePermittedForCFSsList.ContainsCode(OrgCodes.LocalClient));
			AssertEquals("OrganizationTypePermittedForCFSsList should contain 'Consignee'", true, OrganizationTypePermittedForCFSsList.ContainsCode(OrgCodes.Consignee));
			AssertEquals("OrganizationTypePermittedForCFSsList should contain 'Consignor'", true, OrganizationTypePermittedForCFSsList.ContainsCode(OrgCodes.Consignor));
			AssertEquals("OrganizationTypePermittedForCFSsList should not contain 'All Debtors'", true, OrganizationTypePermittedForCFSsList.ContainsCode(OrgCodes.AllDebtors));
		}

		public void TestOrganizationTypePermittedForOthersList()
		{
			AssertEquals("OrganizationTypePermittedForOthersList.Count", 3, OrganizationTypePermittedForOthersList.Count);
			AssertEquals("OrganizationTypePermittedForOthersList should contain 'Immediate'", true, OrganizationTypePermittedForOthersList.ContainsCode(OrgCodes.LocalClient));
			AssertEquals("OrganizationTypePermittedForOthersList should contain 'Actual Departure Date'", true, OrganizationTypePermittedForOthersList.ContainsCode(OrgCodes.AllDebtors));
		}

		public void TestCorrectOrganizationTypeOptionListUsed()
		{
			BizObj.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertListsAreSame(BizObj.OrganizationTypeList, OrganizationTypePermittedForShipmentsList);

			BizObj.JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			AssertListsAreSame(BizObj.OrganizationTypeList, OrganizationTypePermittedForDeclarationsList);

			BizObj.JobType = JobInvoicingConsumerTypes.QuotedBooking.Code;
			AssertListsAreSame(BizObj.OrganizationTypeList, OrganizationTypePermittedForBookingsList);

			BizObj.JobType = JobInvoicingConsumerTypes.CFSShipment.Code;
			AssertListsAreSame(BizObj.OrganizationTypeList, OrganizationTypePermittedForCFSsList);

			BizObj.JobType = JobInvoicingConsumerTypes.MasterAWB.Code;
			AssertListsAreSame(BizObj.OrganizationTypeList, OrganizationTypePermittedForOthersList);
			BizObj.JobType = JobInvoicingConsumerTypes.CFSLoadList.Code;
			AssertListsAreSame(BizObj.OrganizationTypeList, OrganizationTypePermittedForOthersList);
			BizObj.JobType = JobInvoicingConsumerTypes.FCLStorage.Code;
			AssertListsAreSame(BizObj.OrganizationTypeList, OrganizationTypePermittedForOthersList);
			BizObj.JobType = JobInvoicingConsumerTypes.LocalCartage.Code;
			AssertListsAreSame(BizObj.OrganizationTypeList, OrganizationTypePermittedForOthersList);
		}

		public void TestINCOTermList()
		{
			BizObj.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			BizObj.DirectionCode = Core.Constants.FreightShipmentDirection.Code.Export;
			AssertEquals(15, BizObj.OrgsEvaluatedForCreditControlLookups.INCOTermList.Count);
			var expectedINCOTermList = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
			expectedINCOTermList.AddPair(INCOTermCodes.All, INCOTermDescriptions.All);
			AssertListsAreSame(expectedINCOTermList, BizObj.OrgsEvaluatedForCreditControlLookups.INCOTermList);

			BizObj.JobType = JobInvoicingConsumerTypes.QuotedBooking.Code;
			BizObj.DirectionCode = Core.Constants.FreightShipmentDirection.Code.Domestic;
			AssertEquals(5, BizObj.OrgsEvaluatedForCreditControlLookups.INCOTermList.Count);
			var expectedDomesticINCOTermList = new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms);
			expectedDomesticINCOTermList.AddPair(INCOTermCodes.All, INCOTermDescriptions.All);
			AssertListsAreSame(expectedDomesticINCOTermList, BizObj.OrgsEvaluatedForCreditControlLookups.INCOTermList);

			BizObj.DirectionCode = Core.Constants.FreightShipmentDirection.Code.All;
			var expectedAllINCOTermList = new CodeDescriptionPairList();
			expectedAllINCOTermList.AddRange(expectedINCOTermList);
			expectedAllINCOTermList.AddRange(new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms));
			AssertListsAreSame(expectedAllINCOTermList, BizObj.OrgsEvaluatedForCreditControlLookups.INCOTermList);
		}

		public void TestFreightPaymentTermList()
		{
			AssertEquals("OrganizationTypePermittedForOthersList.Count", 3, BizObj.OrgsEvaluatedForCreditControlLookups.FreightPaymentTermList.Count);
			CodeDescriptionPairList expectedList = new CodeDescriptionPairList(OLookUpEditType.PaymentType);
			expectedList.AddPair(FreightPaymentTermCodes.All, FreightPaymentTermDescriptions.All);
			AssertListsAreSame(expectedList, BizObj.OrgsEvaluatedForCreditControlLookups.FreightPaymentTermList);
		}

		#region Implementation

		protected new OrgsEvaluatedForCreditControl BizObj
		{
			get { return (OrgsEvaluatedForCreditControl)base.BizObj; }
			set { base.BizObj = value; }
		}

		CodeDescriptionPairList OrganizationTypePermittedForShipmentsList
		{
			get { return BizObj.OrgsEvaluatedForCreditControlLookups.OrganizationTypePermittedForShipmentsList; }
		}

		CodeDescriptionPairList OrganizationTypePermittedForDeclarationsList
		{
			get { return BizObj.OrgsEvaluatedForCreditControlLookups.OrganizationTypePermittedForDeclarationsList; }
		}

		CodeDescriptionPairList OrganizationTypePermittedForBookingsList
		{
			get { return BizObj.OrgsEvaluatedForCreditControlLookups.OrganizationTypePermittedForBookingsList; }
		}

		CodeDescriptionPairList OrganizationTypePermittedForCFSsList
		{
			get { return BizObj.OrgsEvaluatedForCreditControlLookups.OrganizationTypePermittedForCFSsList; }
		}

		CodeDescriptionPairList OrganizationTypePermittedForOthersList
		{
			get { return BizObj.OrgsEvaluatedForCreditControlLookups.OrganizationTypePermittedForOthersList; }
		}

		#endregion
	}
}
