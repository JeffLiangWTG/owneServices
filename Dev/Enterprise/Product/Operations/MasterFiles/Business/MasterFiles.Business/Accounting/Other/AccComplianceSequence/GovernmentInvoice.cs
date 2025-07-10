using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	[CanHaveTriggersDespiteNotImplementingIWorkflowProvider] // HACK. Please fix TestGovtTaxInvoiceSave_TriggerActioned
	public class GovernmentInvoice : AccTransactionHeader, IWorkflowTriggerEventSource, ITransactionHeader
	{
		public GovernmentInvoice(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		protected override AccTransactionHeaderValidation GetNewValidation()
		{
			return new GovernmentInvoiceValidation(this);
		}

		void LogTransactionReferenceChanged()
		{
			if (IsInDatabase)
			{
				if (AH_TransactionReferenceInfo.HasChanges)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format("Compliance Number updated in Compliance Update Form. Old value: '{0}', New value: '{1}'", AH_TransactionReferenceInfo.OriginalValue, AH_TransactionReference));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				if (AH_ComplianceSubTypeInfo.HasChanges)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format("Compliance Sub Type updated in Compliance Update Form. Old value: '{0}', New value: '{1}'", AH_ComplianceSubTypeInfo.OriginalValue, AH_ComplianceSubType));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				if (AH_ComplianceDocumentDateInfo.HasChanges)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format("Compliance Document Date updated in Compliance Update Form. Old value: '{0}', New value: '{1}'", ((ZDate)AH_ComplianceDocumentDateInfo.OriginalValue).ToShortDateString(), AH_ComplianceDocumentDate.ToShortDateString()));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		protected override void OnSavingCore()
		{
			LogTransactionReferenceChanged();

			if (AH_ComplianceSubTypeInfo.HasChanges || AH_TransactionReferenceInfo.HasChanges)
			{
				AH_XD_ComplianceBook = ZGuid.Empty;
			}

			EvaluateEInvoicingEligibilityAndQueue();
			var queuer = ObjectFactory.Get<IGovernmentInvoiceComplianceReportQueueHelper>();
			queuer.QueueForComplianceReports(this);
		}

		#region Overridden Properties

		[List("ComplianceSubTypeInLocalLanguageList")]
		public override ZString AH_ComplianceSubType
		{
			get => base.AH_ComplianceSubType;
			set
			{
				if (value != AH_ComplianceSubType)
				{
					base.AH_ComplianceSubType = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateAH_ComplianceSubType();
					}
				}
			}
		}

		#endregion

		#region IWorkflowTriggerEventSource Members

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get
			{
				return Company;
			}
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				return Array.Empty<IWorkflowProviderCore>();
			}
		}

		#endregion

		#region ReadOnly

		bool AH_ComplianceSubType_ReadOnly => CompanySafe.Country.SupportDocumentSigning && !(AH_ComplianceSubType.IsEmpty && AH_TransactionReference.IsEmpty);

		bool AH_TransactionReference_ReadOnly => CompanySafe.Country.SupportDocumentSigning;

		internal GlbCompany CompanySafe => Company ?? GlbCompany.CurrentCompany;

		#endregion

		#region EInvoicing

		internal IEInvoicingTransaction EInvoicingTransaction
			=> EInvoicingTransactionValue ?? (EInvoicingTransactionValue = ObjectFactory.Get<IEInvoicingTransactionProxyFactory>().GetProxy(this));

		int ITransactionHeader.Multiplier => throw new NotSupportedException($"{nameof(ITransactionHeader.Multiplier)} property is not supported by {nameof(GovernmentInvoice)}.");

		IEInvoicingTransaction EInvoicingTransactionValue;

		void EvaluateEInvoicingEligibilityAndQueue()
		{
			if (IsInDatabase)
			{
				EInvoicingTransaction.EvaluateEligibilityAndQueue();
			}
		}

		#endregion
	}
}
