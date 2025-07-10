//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgARTermsValidation
//
//    This class should be used for overriding validation in AutoOrgARTermsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgARTermsValidation : AutoOrgARTermsValidation
	{
		public OrgARTermsValidation(AutoOrgARTerms parent)
				: base(parent)
		{
		}

		OrgARTerms ParentOrgARTerms
		{
			get { return Parent as OrgARTerms; }
		}

		protected override void CheckPY_AgreedPaymentMethod()
		{
			base.CheckPY_InvoiceTerm();
			if (Parent.CompanyData != null && Parent.CompanyData.OB_IsDebtor)
			{
				ListValidation.ErrorIfInvalidCode(Parent.PY_AgreedPaymentMethodInfo);

				if (Parent.CompanyData.Organisation != null && !Parent.PY_AgreedPaymentMethod.IsEmpty)
				{
					var warningMessage = ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>()
						.GetFeatureInterface<IPreferredPaymentMethod>(Parent.CompanyData.Organisation.MainAddress.OA_RN_NKCountryCode)?
						.GetPreferredPaymentMethodWarning(Parent.CompanyData.Organisation.OH_Category, Parent.PY_AgreedPaymentMethod);

					if (!string.IsNullOrEmpty(warningMessage))
					{
						Parent.PY_AgreedPaymentMethodInfo.AddWarning(warningMessage);
					}
				}
			}
		}

		protected override void CheckPY_InvoiceClass()
		{
			base.CheckPY_InvoiceClass();
			if (Parent.CompanyData != null && Parent.CompanyData.OB_IsDebtor)
			{
				MandatoryValidation.CheckEntered(Parent.PY_InvoiceClassInfo);
				ListValidation.ErrorIfInvalidCode(Parent.PY_InvoiceClassInfo);

				if (!Parent.PY_InvoiceClassInfo.HasErrors())
				{
					CheckIntegrity();
				}
			}
		}

		#region CheckPY_InvoiceDays

		protected override void CheckPY_InvoiceDays()
		{
			base.CheckPY_InvoiceDays();
			if (Parent.CompanyData != null && Parent.CompanyData.OB_IsDebtor && !Parent.PY_InvoiceDaysInfo.ReadOnly)
			{
				if (Parent.PY_InvoiceTerm == InvoiceTermsList.MonthsFromInvoiceCycleDate.Code)
				{
					if (Parent.PY_InvoiceDays >= 60)
					{
						Parent.PY_InvoiceDaysInfo.AddError(Res.GetString("8a20f93c-f935-4e83-b85c-6f280aa28df8", "For MIC invoice term, the value entered refers to number of term months. {0} months will result in invoice due date of more than 5 years from now and thus is not valid.", Parent.PY_InvoiceDays));
					}
					else if (Parent.PY_InvoiceDays > 3)
					{
						Parent.PY_InvoiceDaysInfo.AddWarning(Res.GetString("93df4c7c-f703-4084-a4ea-058db880c12f", "For MIC invoice term, the value entered refers to number of term months. {0} months will result in invoice due date of more than {1} days from now. ", Parent.PY_InvoiceDays, Parent.PY_InvoiceDays * 30));
					}
				}
				else if (Parent.PY_InvoiceTerm == Constants.InvoiceTerms.FromDeliveryOrPickupDate && (Parent.PY_InvoiceDays < 0 || Parent.PY_InvoiceDays > 99))
				{
					Parent.PY_InvoiceDaysInfo.AddError(Res.GetString("201A22D8-B6BB-11ED-AFA1-0242DC120002", "Days must be between 0 and 99."));
				}
			}
		}

		#endregion

		#region CheckPY_InvoiceTerm

		protected override void CheckPY_InvoiceTerm()
		{
			base.CheckPY_InvoiceTerm();
			if (Parent.CompanyData != null && Parent.CompanyData.OB_IsDebtor)
			{
				MandatoryValidation.CheckEntered(Parent.PY_InvoiceTermInfo);
				ListValidation.ErrorIfInvalidCode(Parent.PY_InvoiceTermInfo);

				if (Parent.PY_InvoiceTerm == OrgARTermsLookups.DefaultInvoiceTerm.Code && !CanDefaultARTermBeSet)
				{
					Parent.PY_InvoiceTermInfo.AddError(CanDefaultARTermBeSetError);
				}

				switch (Parent.PY_InvoiceTerm)
				{
					case Constants.InvoiceTerms.MonthsFromInvoiceCycleDate:
						{
							if (ParentOrgARTerms.ARTermsCycles.Count == 0)
							{
								Parent.PY_InvoiceTermInfo.AddError(Res.GetString("7BC8CD78-4102-470F-A0C7-33CE5266050A", "Invoice Cycle data must be entered."));
							}
							break;
						}
					case Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle:
						{
							if (ParentOrgARTerms.ARPaymentCycles.Count == 0)
							{
								Parent.PY_InvoiceTermInfo.AddError(Res.GetString("B50BF8F6-EC54-4A5D-9B6B-CA4B1D4DE4F9", "Payment Cycle data must be entered."));
							}
							break;
						}
					case Constants.InvoiceTerms.FromInvoiceDate:
						{
							if (ParentOrgARTerms.ARTermsInstallments.Count == 1)
							{
								Parent.PY_InvoiceTermInfo.AddError(Res.GetString("FFC3CFF4-F0DA-4D3A-AE75-CFD6B5E6CE25", "Multiple Installments grid can't have one row only."));
							}
							break;
						}
				}
				if (Constants.InvoiceTerms.FromDeliveryOrPickupDate.Equals(Parent.PY_InvoiceTerm) && !JobInvoicingConsumerTypes.ShipmentCode.Equals(Parent.PY_JobType) && !JobInvoicingConsumerTypes.BrokerageCode.Equals(Parent.PY_JobType) && !JobInvoicingConsumerTypes.LocalCartageCode.Equals(Parent.PY_JobType))
				{
					Parent.PY_InvoiceTermInfo.AddError(Res.GetString("F35D8D2A-B6BA-11ED-AFA1-0242AD120002", "DLP invoice term is only available for Shipment, Brokerage and Port Transport Job Type."));
				}
			}
		}

		public bool CanDefaultARTermBeSet
		{
			get { return string.IsNullOrEmpty(CanDefaultARTermBeSetError); }
		}

		string CanDefaultARTermBeSetError
		{
			get
			{
				string reason = null;
				if (Parent.CompanyData != null && Parent.CompanyData.Header != null)
				{
					ZDBOnlySubQuery arTermsQuery = new ZDBOnlySubQuery(typeof(OrgARTerms), OrgARTermsSchema.PY_OB);
					arTermsQuery.AddToFilter(OrgARTermsSchema.PY_InvoiceTerm, OrgARTermsLookups.DefaultInvoiceTerm.Code);

					ZDBOnlySubQuery companyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					companyDataQuery.AddSubQuery(arTermsQuery, JoinCondition.And);

					ZDBOnlySubQuery headerQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
					headerQuery.AddSubQuery(companyDataQuery, JoinCondition.And);

					ZDBOnlyQuery relatedPartyQuery = new ZDBOnlyQuery(typeof(OrgRelatedParty));
					relatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, SQLComparisonOperator.NotEqual, Parent.CompanyData.Header.PK);
					relatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ARSettlementGroup);
					relatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_FreightDirection, RelatedPartyDirectionList.Codes.AR);
					relatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);
					relatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, Parent.CompanyData.Header.PK);
					relatedPartyQuery.AddSubQuery(OrgRelatedPartySchema.PR_OH_Parent, headerQuery, JoinCondition.And);

					OrgRelatedParty childSettlementRelatedParty = Parent.Factory.LoadTop1<OrgRelatedParty>(relatedPartyQuery);
					if (childSettlementRelatedParty != null)
					{
						reason = Res.GetString("50aa5f14-358b-4985-b250-357458520870", "this organization is settlement group for another organization with '{0}' term", OrgARTermsLookups.DefaultInvoiceTerm.Code);
					}
					else if (Parent.CompanyData.Header.ARSettlementGroup != null)
					{
						bool isSettlementGroupTheSameAsHeader = Parent.CompanyData.Header.ARSettlementGroup.PK == Parent.CompanyData.Header.PK;
						bool isSettelmentGroupHasDefaultInvoiceTerm = Parent.CompanyData.Header.ARSettlementGroup.CompanyData.ARTerms.Find(new ZQuery(OrgARTermsSchema.PY_InvoiceTerm, OrgARTermsLookups.DefaultInvoiceTerm.Code)).Any();

						if (isSettlementGroupTheSameAsHeader)
						{
							reason = Res.GetString("6bed96d2-7b68-4a49-8260-2e750bb67cb8", "AR Settlement Group is set to itself");
						}
						else if (isSettelmentGroupHasDefaultInvoiceTerm)
						{
							reason = Res.GetString("7f30a6c8-1c3e-4116-87f7-b2d1f8269c20", "settlement organization also has '{0}' term", OrgARTermsLookups.DefaultInvoiceTerm.Code);
						}
					}
					else
					{
						reason = Res.GetString("6e57674c-3fe4-4b41-9e70-22e450c43581", "AR Settlement Group is empty");
					}
				}

				return string.IsNullOrEmpty(reason) ? string.Empty : Res.GetString("d610ed91-00d5-48a4-860b-447981b8781d", "'{0}' term can't be set if {1}.", OrgARTermsLookups.DefaultInvoiceTerm.Code, reason);
			}
		}

		#endregion

		#region CheckPY_JobType

		protected override void CheckPY_JobType()
		{
			base.CheckPY_JobType();
			if (Parent.CompanyData != null && Parent.CompanyData.OB_IsDebtor)
			{
				ParentOrgARTerms.JobTypeDirectionAndTransportListProvider.ValidateJobType();
				if (!ParentOrgARTerms.PY_JobTypeInfo.HasErrors())
				{
					CheckIntegrity();
				}
			}
		}

		#endregion

		#region CheckPY_Direction

		protected override void CheckPY_Direction()
		{
			base.CheckPY_Direction();
			if (Parent.CompanyData != null && Parent.CompanyData.OB_IsDebtor)
			{
				ParentOrgARTerms.JobTypeDirectionAndTransportListProvider.ValidateDirection();
				if (!ParentOrgARTerms.PY_DirectionInfo.HasErrors())
				{
					CheckIntegrity();
				}
			}
		}

		#endregion

		#region CheckPY_TransportMode

		protected override void CheckPY_TransportMode()
		{
			base.CheckPY_TransportMode();
			if (Parent.CompanyData != null && Parent.CompanyData.OB_IsDebtor)
			{
				ParentOrgARTerms.JobTypeDirectionAndTransportListProvider.ValidateTransportMode();
				if (!ParentOrgARTerms.PY_TransportModeInfo.HasErrors())
				{
					CheckIntegrity();
				}
			}
		}

		#endregion

		#region CheckPY_GB

		protected override void CheckPY_GB_Branch()
		{
			base.CheckPY_GB_Branch();
			if (Parent.CompanyData != null && Parent.CompanyData.OB_IsDebtor)
			{
				ListValidation.ErrorIfInvalidPK(Parent.PY_GB_BranchInfo);
				if (!ParentOrgARTerms.PY_GB_BranchInfo.HasErrors())
				{
					CheckIntegrity();
				}
			}
		}

		#endregion

		#region CheckPY_GE

		protected override void CheckPY_GE_Department()
		{
			base.CheckPY_GE_Department();
			if (Parent.CompanyData != null && Parent.CompanyData.OB_IsDebtor)
			{
				ListValidation.ErrorIfInvalidPK(Parent.PY_GE_DepartmentInfo);
				if (!ParentOrgARTerms.PY_GE_DepartmentInfo.HasErrors())
				{
					CheckIntegrity();
				}
			}
		}

		#endregion

		public void CheckIntegrity()
		{
			if (!(Parent as OrgARTerms).RowValidationSuspender.IsSuspended && Parent.CompanyData != null)
			{
				CheckDuplicateRow();
				CheckDefaultRow();
			}
		}

		void CheckDuplicateRow()
		{
			foreach (OrgARTerms arTerms in Parent.CompanyData.ARTerms)
			{
				var duplicateRowErrorMsg = Res.GetString("01287e7f-8a0e-46b5-83b1-6a620653a533", @"Term settings for following already exists -
Job Type: {0}, Direction: {1}, Transport Mode: {2}, Branch: {3}, Dept.: {4}, Invoice type: {5}", arTerms.PY_JobType, (arTerms.PY_Direction.IsEmpty ? (ZString)(NoResString)"<Empty>" : arTerms.PY_Direction), (arTerms.PY_TransportMode.IsEmpty ? (ZString)(NoResString)"<Empty>" : arTerms.PY_TransportMode), (!arTerms.PY_GB_Branch.IsValid ? (ZString)"ALL" : arTerms.Branch.GB_Code), (!arTerms.PY_GE_Department.IsValid ? (ZString)"ALL" : arTerms.Department.GE_Code), arTerms.PY_InvoiceClass);

				arTerms.ClearRowNotifications();
				if (Parent.CompanyData.ARTerms.Any(x => x.PY_InvoiceClass == arTerms.PY_InvoiceClass
						&& x.PY_JobType == arTerms.PY_JobType
						&& x.PY_TransportMode == arTerms.PY_TransportMode
						&& x.PY_Direction == arTerms.PY_Direction
						&& x.PY_GB_Branch == arTerms.PY_GB_Branch
						&& x.PY_GE_Department == arTerms.PY_GE_Department
						&& x.PK != arTerms.PK))
				{
					arTerms.AddRowError(duplicateRowErrorMsg);
				}
			}
		}

		void CheckDefaultRow()
		{
			if (!Parent.CompanyData.ARTerms.Any(x => x.PY_InvoiceClass == OrgARTermsLookups.InvoiceTypes.All.Code
						 && x.PY_JobType == JobTypeDirectionAndTransportInfoProvider.All
						 && x.PY_TransportMode == JobTypeDirectionAndTransportInfoProvider.All
						 && x.PY_Direction == JobTypeDirectionAndTransportInfoProvider.All
						 && x.PY_GB_Branch == ZGuid.Empty
						 && x.PY_GE_Department == ZGuid.Empty))
			{
				var defaultRowErrorMsg = Res.GetString("b08bd715-3c7f-4c21-b385-a8077b806200", @"Term settings for Job Type: {0}, Invoice type: {1} is mandatory.", JobTypeDirectionAndTransportInfoProvider.All, OrgARTermsLookups.InvoiceTypes.All.Code);
				Parent.AddRowError(defaultRowErrorMsg);
			}
		}
	}
}
