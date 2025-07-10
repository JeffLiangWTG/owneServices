using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;
using OrgDescs = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeDescriptions;

namespace Enterprise.MasterFiles.Business
{
	public class OrgsEvaluatedForCreditControlLookups : JobConfigurationSelectorLookups
	{
		public OrgsEvaluatedForCreditControlLookups(OrgsEvaluatedForCreditControl parent)
			: base(parent)
		{
			Parent = parent;
		}
		new readonly OrgsEvaluatedForCreditControl Parent;

		#region JobTypeList

		protected override CodeDescriptionPairList GetJobTypeList()
		{
			var result = base.GetJobTypeList();
			result.RemoveCode(JobInvoicingConsumerTypes.AgentBookingCode);
			result.RemoveCode(JobInvoicingConsumerTypes.TransportBookingWithAgentCode);
			result.RemoveCode(JobInvoicingConsumerTypes.TransportBookingCode);
			return result;
		}

		public static CodeDescriptionPairList GetJobTypeListWithoutTypeAll()
		{
			var result = new OrgsEvaluatedForCreditControlLookups(null).GetJobTypeList();
			result.RemoveCode(JobTypeAdditionalCodes.All);
			return result;
		}

		#endregion

		#region Orgnization Type List

		public CodeDescriptionPairList OrganizationTypeList
		{
			get
			{
				if (Parent.JobType == JobInvoicingConsumerTypes.ForwardingConsol.Code)
				{
					return OrganizationTypePermittedForConsolList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.Shipment.Code)
				{
					return OrganizationTypePermittedForShipmentsList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.Brokerage.Code)
				{
					return OrganizationTypePermittedForDeclarationsList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.QuotedBooking.Code)
				{
					return OrganizationTypePermittedForBookingsList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.CFSShipment.Code || Parent.JobType == JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
				{
					return OrganizationTypePermittedForCFSsList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.MasterAWB.Code
					|| Parent.JobType == JobInvoicingConsumerTypes.CFSLoadList.Code
					|| Parent.JobType == JobInvoicingConsumerTypes.FCLStorage.Code
					|| Parent.JobType == JobInvoicingConsumerTypes.LocalCartage.Code)
				{
					return OrganizationTypePermittedForOthersList;
				}
				return AllOrganizationTypeList;
			}
		}

		internal bool HasOrganisationTypes()
		{
			return true;
		}

		internal bool HasAllDebtors()
		{
			return Parent.JobType != JobInvoicingConsumerTypes.ForwardingConsol.Code;
		}

		public CodeDescriptionPairList CompleteOrganizationTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Insert(0, new CodeDescriptionPair(OrgCodes.All, OrgDescs.All));
				result.AddPair(OrgCodes.LocalClient, OrgDescs.LocalClient);
				result.AddPair(OrgCodes.OverseasAgent, OrgDescs.OverseasAgent);
				result.AddPair(OrgCodes.Consignee, OrgDescs.Consignee);
				result.AddPair(OrgCodes.Consignor, OrgDescs.Consignor);
				result.AddPair(OrgCodes.SendingAgent, OrgDescs.SendingAgent);
				result.AddPair(OrgCodes.ReceivingAgent, OrgDescs.ReceivingAgent);
				result.AddPair(OrgCodes.Importer, OrgDescs.Importer);
				result.AddPair(OrgCodes.Supplier, OrgDescs.Supplier);
				result.AddPair(OrgCodes.AllDebtors, OrgDescs.AllDebtors);
				result.AddPair(OrgCodes.ControllingAgent, OrgDescs.ControllingAgent);
				result.AddPair(OrgCodes.ControllingCustomer, OrgDescs.ControllingCustomer);
				return result;
			}
		}

		public CodeDescriptionPairList OrganizationTypePermittedForShipmentsList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Insert(0, new CodeDescriptionPair(OrgCodes.All, OrgDescs.All));
				result.AddPair(OrgCodes.LocalClient, OrgDescs.LocalClient);
				
				if (ShouldContainsCAGCTP)
				{
					result.AddPair(OrgCodes.ControllingAgent, OrgDescs.ControllingAgent);
				}
				result.AddPair(OrgCodes.Consignee, OrgDescs.Consignee);
				result.AddPair(OrgCodes.Consignor, OrgDescs.Consignor);
				if (ShouldContainsCAGCTP)
				{
					result.AddPair(OrgCodes.ControllingCustomer, OrgDescs.ControllingCustomer);
				}
				result.AddPair(OrgCodes.SendingAgent, OrgDescs.SendingAgent);
				result.AddPair(OrgCodes.ReceivingAgent, OrgDescs.ReceivingAgent);
				result.AddPair(OrgCodes.AllDebtors, OrgDescs.AllDebtors);
				return result;
			}
		}

		bool ShouldContainsCAGCTP
		{
			get
			{
				if (Parent.CurrentFallbackLevel == null)
				{
					return true;
				}

				return AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.GetFallBackValueAtAllLevels(Parent.CurrentFallbackLevel.CompanyPK(false), Parent.CurrentFallbackLevel.BranchPK, Parent.CurrentFallbackLevel.DepartmentPK);
			}
		}

		public CodeDescriptionPairList OrganizationTypePermittedForConsolList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Insert(0, new CodeDescriptionPair(OrgCodes.All, OrgDescs.All));
				result.AddPair(OrgCodes.SendingAgent, OrgDescs.SendingAgent);
				result.AddPair(OrgCodes.ReceivingAgent, OrgDescs.ReceivingAgent);
				return result;
			}
		}

		public CodeDescriptionPairList AllOrganizationTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(OrgCodes.AllDebtors, OrgDescs.AllDebtors);
				return result;
			}
		}

		public CodeDescriptionPairList OrganizationTypePermittedForDeclarationsList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Insert(0, new CodeDescriptionPair(OrgCodes.All, OrgDescs.All));
				result.AddPair(OrgCodes.LocalClient, OrgDescs.LocalClient);
				if (ShouldContainsCAGCTP)
				{
					result.AddPair(OrgCodes.ControllingAgent, OrgDescs.ControllingAgent);
				}
				result.AddPair(OrgCodes.Consignee, OrgDescs.Consignee);
				result.AddPair(OrgCodes.Consignor, OrgDescs.Consignor);
				if (ShouldContainsCAGCTP)
				{
					result.AddPair(OrgCodes.ControllingCustomer, OrgDescs.ControllingCustomer);
				}
				result.AddPair(OrgCodes.OverseasAgent, OrgDescs.OverseasAgent);
				result.AddPair(OrgCodes.AllDebtors, OrgDescs.AllDebtors);
				return result;
			}
		}

		public CodeDescriptionPairList OrganizationTypePermittedForBookingsList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Insert(0, new CodeDescriptionPair(OrgCodes.All, OrgDescs.All));
				result.AddPair(OrgCodes.LocalClient, OrgDescs.LocalClient);
				if (ShouldContainsCAGCTP)
				{
					result.AddPair(OrgCodes.ControllingCustomer, OrgDescs.ControllingCustomer);
					result.AddPair(OrgCodes.ControllingAgent, OrgDescs.ControllingAgent);
				}
				result.AddPair(OrgCodes.Importer, OrgDescs.Importer);
				result.AddPair(OrgCodes.Supplier, OrgDescs.Supplier);
				result.AddPair(OrgCodes.OverseasAgent, OrgDescs.OverseasAgent);
				result.AddPair(OrgCodes.AllDebtors, OrgDescs.AllDebtors);
				return result;
			}
		}

		public CodeDescriptionPairList OrganizationTypePermittedForCFSsList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Insert(0, new CodeDescriptionPair(OrgCodes.All, OrgDescs.All));
				result.AddPair(OrgCodes.LocalClient, OrgDescs.LocalClient);
				result.AddPair(OrgCodes.Consignee, OrgDescs.Consignee);
				result.AddPair(OrgCodes.Consignor, OrgDescs.Consignor);
				result.AddPair(OrgCodes.AllDebtors, OrgDescs.AllDebtors);
				return result;
			}
		}

		public CodeDescriptionPairList OrganizationTypePermittedForOthersList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Insert(0, new CodeDescriptionPair(OrgCodes.All, OrgDescs.All));
				result.AddPair(OrgCodes.LocalClient, OrgDescs.LocalClient);
				result.AddPair(OrgCodes.AllDebtors, OrgDescs.AllDebtors);
				return result;
			}
		}

		#endregion

		internal bool HasINCOTerms()
		{
			return Parent.JobType == JobInvoicingConsumerTypes.Brokerage.Code
				|| (Parent.JobType == JobInvoicingConsumerTypes.Shipment.Code && Parent.DirectionList.ContainsCode(Parent.DirectionCode))
				|| (Parent.JobType == JobInvoicingConsumerTypes.QuotedBooking.Code && Parent.DirectionList.ContainsCode(Parent.DirectionCode));
		}

		public CodeDescriptionPairList INCOTermList
		{
			get
			{
				CodeDescriptionPairList result;
				
				if ((Parent.JobType == JobInvoicingConsumerTypes.Shipment.Code && Parent.DirectionCode == Core.Constants.FreightShipmentDirection.Code.Domestic)
				|| (Parent.JobType == JobInvoicingConsumerTypes.QuotedBooking.Code && Parent.DirectionCode == Core.Constants.FreightShipmentDirection.Code.Domestic))
				{
					result = new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms);
				}
				else if ((Parent.JobType == JobInvoicingConsumerTypes.Shipment.Code && Parent.DirectionCode == Core.Constants.FreightShipmentDirection.Code.All)
				|| (Parent.JobType == JobInvoicingConsumerTypes.QuotedBooking.Code && Parent.DirectionCode == Core.Constants.FreightShipmentDirection.Code.All))
				{
					result = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncludingDomesticTerms);
				}
				else
				{
					result = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
				}
				result.Insert(0, new CodeDescriptionPair(INCOTermCodes.All, INCOTermDescriptions.All));

				return result;
			}
		}

		internal bool HasFreightPaymentTerms()
		{
			return Parent.JobType == JobInvoicingConsumerTypes.Brokerage.Code
				|| Parent.JobType == JobInvoicingConsumerTypes.Shipment.Code
				|| Parent.JobType == JobInvoicingConsumerTypes.QuotedBooking.Code
				|| Parent.JobType == JobInvoicingConsumerTypes.AgencyBillOfLading.Code
				|| Parent.JobType == JobInvoicingConsumerTypes.ForwardingConsol.Code;
		}

		public CodeDescriptionPairList FreightPaymentTermList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.PaymentType);
				result.Insert(0, new CodeDescriptionPair(FreightPaymentTermCodes.All, FreightPaymentTermDescriptions.All));

				return result;
			}
		}
	}
}
