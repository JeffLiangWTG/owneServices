using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondHeaderValidation : Customs.Business.CusInBondHeaderValidation
	{
		public CusInBondHeaderValidation(CusInBondHeader parent)
			: base(parent)
		{
		}

		protected new CusInBondHeader Parent => (CusInBondHeader)base.Parent;

		protected CusInBondHeaderLookups Lookups => Parent.Lookups;

		protected BusinessObjectFactory Factory => Parent.Factory;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateDeconsolidator();
				ValidateBH_GS_NKCusAgent();
				ValidateUnladingOffice();
				ValidateReceiptOffice();
				ValidateTW_BoxNumber();
				ValidateEntryNumber();
			}
		}

		public void ValidateEntryNumber()
		{
			ValidateCalculatedProperty(Parent.EntryNumberInfo);
		}

		public void ValidateBH_GS_NKCusAgent()
		{
			ValidateCalculatedProperty(Parent.BH_GS_NKCusAgentInfo);
		}

		public void ValidateDeconsolidator()
		{
			ValidateCalculatedProperty(Parent.DeconsolidatorInfo);
		}

		public void ValidateUnladingOffice()
		{
			ValidateCalculatedProperty(Parent.UnladingOfficeInfo);
		}

		public void ValidateReceiptOffice()
		{
			ValidateCalculatedProperty(Parent.ReceiptOfficeInfo);
		}

		public void ValidateTW_BoxNumber()
		{
			ValidateCalculatedProperty(Parent.TW_BoxNumberInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
		void CheckBH_GS_NKCusAgent()
		{
			var cusAgent = Parent.BH_GS_NKCusAgent;
			var targetInfo = Parent.BH_GS_NKCusAgentInfo;
			if (!cusAgent.IsEmpty)
			{
				var brokerStaff = Parent.CusAgent;
				if (brokerStaff == null)
				{
					targetInfo.AddMessageError(ListValidation.GetNotificationMessage(targetInfo).ToString());
				}
				else if (brokerStaff.GetValidTWBrokerCertificateNumber(ZDate.Today).IsEmpty)
				{
					targetInfo.AddMessageError(ValidationConstants.CusInBondHeader.BrokerStaffNotHaveValidCertificateNumber);
				}

				EnglishCharactersValidation.ErrorIfNotWesternEuropean(targetInfo);
			}
			else
			{
				targetInfo.AddMessageError(ValidationConstants.CusInBondHeader.BrokerStaffIsNotEmpty);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
		void CheckDeconsolidator()
		{
			if (!Parent.Deconsolidator.IsEmpty)
			{
				var orgHeader = Factory.Load<OrgHeader>(Parent.Deconsolidator);
				if (orgHeader == null)
				{
					Parent.DeconsolidatorInfo.AddMessageError(ListValidation.GetNotificationMessage(Parent.DeconsolidatorInfo).ToString());
				}
			}
		}

		protected override void CheckBH_OA_Importer()
		{
			base.CheckBH_OA_Importer();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_OA_ImporterInfo);
			var importerOrgHeader = Parent.ImporterOrg;
			if (importerOrgHeader != null)
			{
				if (!OrgHeaderHelper.CheckHasCusCode(importerOrgHeader, Core.Constants.CountryCodes.Taiwan, new string[] { OrgCusCode.CodeTypes.VATCode,
						OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.CodeTypes.PassportID }))
				{
					Parent.BH_OA_ImporterInfo.AddMessageError(ValidationConstants.CusInBondHeader.MissingVATorPIDorPAS);
				}
			}
		}

		protected override void CheckBH_VoyageNumber()
		{
			base.CheckBH_VoyageNumber();
			var vesselREG = Parent.BH_VoyageNumber;
			if (!vesselREG.IsEmpty)
			{
				var targetInfo = Parent.BH_VoyageNumberInfo;
				if (vesselREG.Length != 6)
				{
					targetInfo.AddMessageError(Res.GetString("B8875ABE-A233-4257-A845-0A5FB93304BB", "Vessel REG should be 6 digits."));
				}
			}
		}

		protected override void CheckBH_CustomsProfile()
		{
			var targetInfo = Parent.BH_CustomsProfileInfo;
			if (!Parent.MailBoxReadOnly)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo, Parent.MailBoxCollection);
			}
			var credential = Parent.GetCredential();
			if (credential != null)
			{
				var isTesting = credential.IsTesting();
				var isTestMode = TWCustomsDataRegistry.IsTestMode;
				if (isTestMode && !isTesting)
				{
					targetInfo.AddMessageError(ValidationConstants.Declaration.MailboxNotForTesting);
				}
				else if (!isTestMode && isTesting)
				{
					targetInfo.AddMessageError(ValidationConstants.Declaration.MailboxNotForProduction);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
		void CheckEntryNumber()
		{
			if (!CommonHelper.IsMatchEntryNumber(Parent, Parent.EntryNumber))
			{
				Parent.EntryNumberInfo.AddMessageError(ValidationConstants.AllocateNumber.KeyComponentValuesChanged);
			}
		}

		protected override void CheckBH_ImportTransportMode()
		{
			base.CheckBH_ImportTransportMode();
			var targetInfo = Parent.BH_ImportTransportModeInfo;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo, Lookups.TransportModeCodes);
			if (Parent.BH_ImportTransportMode != Parent.DefaultImportTransportModeFromOffice)
			{
				targetInfo.AddMessageError(ValidationConstants.CusInBondHeader.DifferentTransportMode);
			}
		}

		protected void CheckUnladingOffice()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.UnladingOfficeInfo);
		}

		protected void CheckReceiptOffice()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ReceiptOfficeInfo);
			if (!Parent.IsReceiptOfficeReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ReceiptOfficeInfo);
			}
		}

		protected override void CheckBH_UniqueVoyageIdentifier()
		{
			base.CheckBH_UniqueVoyageIdentifier();
			if (Parent.BH_UniqueVoyageIdentifier.IsEmpty && Parent.IsTransportModeAir)
			{
				var targetInfo = Parent.BH_UniqueVoyageIdentifierInfo;
				targetInfo.AddWarning(Res.GetString("4CEE4FC7-9CD2-432A-84F2-C683C97647F5", "System will automatically declare 'NIL' when {0} is empty.", targetInfo.HumanReadableName));
			}
		}

		protected override void CheckBH_RL_NKImportLoadPort()
		{
			base.CheckBH_RL_NKImportLoadPort();
			if (Parent.IsTransportModeAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_RL_NKImportLoadPortInfo);
			}
		}

		protected override void CheckBH_ETA()
		{
			base.CheckBH_ETA();
			if (Parent.IsTransportModeAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_ETAInfo);
			}
		}

		protected void CheckTW_BoxNumber()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.TW_BoxNumberInfo);
		}
	}
}
