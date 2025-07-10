using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class PrintBrokerSignatureProvider
	{
		public PrintBrokerSignatureProvider(IBrokerSignatureProvider brokerSignature)
		{
			this.brokerSignature = brokerSignature;
		}
		readonly IBrokerSignatureProvider brokerSignature;

		public ZBool ShouldPrintBrokerSignature
		{
			get
			{
				var branchPK = brokerSignature.RegistryBranchPK;
				var companyPK = brokerSignature.RegistryCompanyPK;

				return USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
			}
		}

		public Image BrokerSignature
		{
			get
			{
				if (ShouldPrintBrokerSignature && Signatory != null)
				{
					return Signatory.SignatureImage;
				}

				return null;
			}
		}

		public GlbStaff Signatory
		{
			get
			{
				var result = BrokerOrCurrentUser;

				var branchPK = brokerSignature.RegistryBranchPK;
				var companyPK = brokerSignature.RegistryCompanyPK;

				ZGuid signatoryPK = USCustomsDataRegistry.Instance.PrintSignatureOnEntryDocsBroker.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
				var signatory = signatoryPK.IsValid && brokerSignature.Factory != null ? brokerSignature.Factory.Load<GlbStaff>(signatoryPK) : null;
				if (signatory != null)
				{
					result = signatory;
				}
				return result;
			}
		}

		GlbStaff BrokerOrCurrentUser
		{
			get
			{
				var shouldUseCusAgent = GlbStaff.CurrentUser.GS_IsSystemAccount;

				if (!shouldUseCusAgent)
				{
					shouldUseCusAgent = USCustomsDataRegistry.Instance.EntryDeclarant.GetFallBackValueAtAllLevels(brokerSignature.RegistryCompanyPK, brokerSignature.RegistryBranchPK, Guid.Empty);
				}

				return shouldUseCusAgent ? brokerSignature.CusAgent : GlbStaff.CurrentUser;
			}
		}

		public Image BrokerSignatureForElectronicRelease
		{
			get
			{
				if (brokerSignature.HasCurrentElectronicRelease)
				{
					return BrokerSignature;
				}

				return null;
			}
		}
	}
}
