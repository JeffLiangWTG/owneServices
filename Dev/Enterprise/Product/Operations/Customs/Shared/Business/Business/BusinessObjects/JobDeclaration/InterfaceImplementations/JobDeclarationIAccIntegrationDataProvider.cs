using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	public class JobDeclarationIAccIntegrationDataProvider : IAccIntegrationDataProvider
	{
		public JobDeclarationIAccIntegrationDataProvider(BaseJobDeclaration declaration, bool useSeperateFactory = true)
			: this(declaration.GetActionsToTake(), declaration.GetEntryHeaderPKs(), declaration.PK, declaration.IsIntegrationWithAccountingSupported, useSeperateFactory ? new BusinessObjectFactory() : declaration.Factory)
		{
		}

		public JobDeclarationIAccIntegrationDataProvider(ChargePosterBehaviours actions, IEnumerable<ZGuid> entryHeaderPKs, ZGuid declarationPK, bool isIntegrationSupported, BusinessObjectFactory factory)
		{
			this.actions = actions;
			this.entryHeaderPKs = entryHeaderPKs;
			this.billingFactory = factory;
			this.declarationInBillingFactory = billingFactory.Load<BaseJobDeclaration>(declarationPK);
			this.isIntegrationSupported = isIntegrationSupported;
		}

		protected readonly ChargePosterBehaviours actions;
		protected readonly IEnumerable<ZGuid> entryHeaderPKs;
		protected readonly BusinessObjectFactory billingFactory;
		protected readonly BaseJobDeclaration declarationInBillingFactory;
		protected readonly bool isIntegrationSupported;

		#region IControllerIDProvider Members

		IControllerIDProvider ControllerIDProvider
		{
			get { return declarationInBillingFactory; }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.ControllerID : null;
			}
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.BusinessObjectPK : Guid.Empty;
			}
		}

		#endregion

		BusinessObjectFactory IAccIntegrationDataProvider.Factory
		{
			get { return billingFactory; }
		}

		ZGuid IAccIntegrationDataProvider.AutoPostingEmailRecipient
		{
			get
			{
				var result = ((ICustomsJobInfo)declarationInBillingFactory).AutoPostingNotification?.EmailRecipients ?? Array.Empty<ZGuid>();
				return result.Length > 0 ? result[0] : ZGuid.Empty;
			}
		}

		ChargePosterBehaviours IAccIntegrationDataProvider.Action
		{
			get { return actions; }
		}

		ZGuid[] IAccIntegrationDataProvider.DisbursementChargeCodes
		{
			get { return EntryChargeTypeList.GetAllChargeCodePKsOf(declarationInBillingFactory.Branch.GB_GC); }
		}

		ZString IAccIntegrationDataProvider.ReferenceID
		{
			get { return declarationInBillingFactory.JE_DeclarationReference; }
		}

		ZString IAccIntegrationDataProvider.JobType
		{
			get { return Res.GetString("f53f45f8-bc76-4056-af57-90658b708efa", "Declaration"); }
		}

		IAccInvoiceDataProvider[] IAccIntegrationDataProvider.InvDataProviders
		{
			get
			{
				List<IAccInvoiceDataProvider> result = new List<IAccInvoiceDataProvider>();

				foreach (ZGuid entryPK in entryHeaderPKs)
				{
					var entryInBillingFactory = (IAccInvoiceDataProvider)declarationInBillingFactory.CustomsEntryHeaders.FindByPK(entryPK);

					if (entryInBillingFactory != null)
					{
						result.Add(entryInBillingFactory);
					}
				}

				return result.ToArray();
			}
		}

		bool IAccIntegrationDataProvider.SupportIntegration
		{
			get { return isIntegrationSupported; }
		}

		void IAccIntegrationDataProvider.LogPostingResult(string message)
		{
		}

		Action IAccIntegrationDataProvider.OnIntegrated
		{
			get { return OnIntegratedWithAccountingSuccessfully; }
		}

		protected virtual void OnIntegratedWithAccountingSuccessfully()
		{
			declarationInBillingFactory.OnIntegratedWithAccountingSuccessfully();
		}

		public EntryChargeTypeList EntryChargeTypeList => EntryChargeTypeList.GetCachedList(declarationInBillingFactory.Factory, declarationInBillingFactory.CountryCode);

		GlbCompany IAccIntegrationDataProvider.Company
		{
			get { return declarationInBillingFactory.Company; }
		}
	}

	static class IntegrationExtensionMethods
	{
		public static ChargePosterBehaviours GetActionsToTake(this BaseJobDeclaration declaration)
		{
			var options = Enterprise.Customs.DataRegistry.Business.CustomsDataRegistry.Instance.EnableAccountingIntegration.GetValueWithoutFallback(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty);

			var result = options.Actions;

			if (options.PreApprovalBillingJob && !declaration.IsJobReadyForPost)
			{
				result = ChargePosterBehaviours.AutoRateDSB;
			}
			else
			{
				// unless all entries are cleared, system won't post AR
				if (options.PostAR)
				{
					bool? hasAllFormalEntriesCleared = null;

					foreach (IAccInvoiceDataProvider entry in GetFormalEntries(declaration))
					{
						bool cleared = entry.IsEligibleForIntegration;

						if (cleared && !hasAllFormalEntriesCleared.HasValue)
						{
							hasAllFormalEntriesCleared = true;
						}

						hasAllFormalEntriesCleared &= cleared;

						if (!hasAllFormalEntriesCleared.Value)
						{
							break;
						}
					}

					if (!hasAllFormalEntriesCleared.HasValue || !hasAllFormalEntriesCleared.Value)
					{
						if ((result & ChargePosterBehaviours.ARPostDSB) == ChargePosterBehaviours.ARPostDSB)
						{
							result = (ChargePosterBehaviours)(result - ChargePosterBehaviours.ARPostDSB);
						}

						if ((result & ChargePosterBehaviours.ARAPPostNonDSB) == ChargePosterBehaviours.ARAPPostNonDSB)
						{
							result = (ChargePosterBehaviours)(result - ChargePosterBehaviours.ARAPPostNonDSB);
						}
					}
				}
			}

			if (result > 0)
			{
				result |= ChargePosterBehaviours.SendEmail;
			}
			return result;
		}

		public static IEnumerable<ZGuid> GetEntryHeaderPKs(this BaseJobDeclaration declaration)
		{
			foreach (IAccInvoiceDataProvider entry in GetFormalEntries(declaration))
			{
				if (entry.IsEligibleForIntegration)
				{
					yield return ((BusinessObject)entry).PK;
				}
			}
		}

		internal static IEnumerable<CusEntryHeader> GetFormalEntries(this BaseJobDeclaration declaration)
		{
			foreach (CusEntryHeader entryHeader in declaration.ActiveEntryHeaders)
			{
				if (entryHeader.IsFormalEntry)
				{
					yield return entryHeader;
				}
			}
		}
	}
}
