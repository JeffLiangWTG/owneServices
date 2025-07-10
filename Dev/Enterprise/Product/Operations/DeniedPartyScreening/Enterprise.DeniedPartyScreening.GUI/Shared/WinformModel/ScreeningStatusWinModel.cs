using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class ScreeningStatusWinModel : NonPersistentBusinessObject<ScreeningStatusWinModelValidation>, ISupportNotifyPropertyChanged
	{
		readonly ScoreGrades scoreGrade;
		readonly ScreeningParty screeningParty;
		readonly Action notifySavingButtonStatus;
		readonly bool standAlone;

		public ScreeningStatusWinModel(ScoreGrades scoreGrade, ScreeningParty screeningParty, Action notifySavingButtonStatus = null, bool standAlone = false)
		{
			Argument.NotNull(screeningParty, nameof(screeningParty));

			this.scoreGrade = scoreGrade;
			this.screeningParty = screeningParty;
			this.notifySavingButtonStatus = notifySavingButtonStatus;
			this.standAlone = standAlone;
			InitializeStatus();
			InitializeClearingReasonLists();
		}

		public const string Uri = "https://myaccount.cargowise.com/Home/CargoWiseLearning.aspx#item=D4B99507-3D62-45CE-B256-65D149DA53EA&amp;video=11582152,91b0f67ddf8574bcbde03ab13ad54bf9";

		public bool HideScreeningStatusesComboBox => standAlone || ScreeningStatuses.Count == 0;

		public string CredentialOverride { get; set; }

		public string LearnDeniedPartyScreeningText => Res.GetString("A8B3DB6A-FF6B-4716-B7D8-18CEEC71B724", "Learn about Denied Party Screening");

		public ResourceStringData ChangeStatusWaterMark
		{
			get
			{
				if (screeningParty.Country != null)
				{
					return Res.GetData("C4F644EC-0490-4DEB-83A6-9C7597633CF2", "Mark as Sanctioned");
				}
				else
				{
					return Res.GetData("3AFDC5F7-9C75-4B3F-A704-0BC5B2197029", "Change Status");
				}
			}
		}

		public ResourceStringData ClearingReasonWaterMark => Res.GetData("43AC9B62-56B8-4E5E-82F1-D9599F4E12C6", "Clearing Reason");

		public CodeDescriptionPairList ScreeningStatuses { get; private set; }

		ZString screeningStatus;
		[List("ScreeningStatuses")]
		public ZString ScreeningStatus
		{
			get => screeningStatus;
			set
			{
				value = value.Trim();

				if (value == screeningStatus)
				{
					return;
				}

				if (!ScreeningStatuses.ContainsCode(value))
				{
					return;
				}

				var cachedScreeningStatus = screeningStatus;
				screeningStatus = value;

				(bool IsGranted, string CredentialDetails) isGrantedAndCredentialDetails = DpsSecurityRights.IsGrantedUpdateStatus(screeningStatus, scoreGrade);
				if (isGrantedAndCredentialDetails.IsGranted)
				{
					CredentialOverride = isGrantedAndCredentialDetails.CredentialDetails;
				}
				else
				{
					screeningStatus = cachedScreeningStatus;
					return;
				}

				this.NotifyPropertyChanged(nameof(ClearingReasonsVisibility));
				this.NotifyPropertyChanged(nameof(ClearingReasonTextVisibility));
				notifySavingButtonStatus?.Invoke();
			}
		}

		public CodeDescriptionPairList ClearingReasons { get; private set; }

		[List("ClearingReasons")]
		public ZString ClearingReason
		{
			get => clearingReason;
			set
			{
				value = value.Trim();

				if (!ClearingReasons.ContainsCode(value))
				{
					return;
				}

				clearingReason = value;

				SetRegistryValueToClearingReasonTextIfNeeded(clearingReason);

				this.NotifyPropertyChanged(nameof(ClearingReasonTextVisibility));
				this.NotifyPropertyChanged(nameof(ClearingReasonTextEnabled));
				this.NotifyPropertyChanged(nameof(ClearingReasonText));
				notifySavingButtonStatus?.Invoke();

				if (!IsValidationSuspended)
				{
					Validation.ValidateClearingReasonText();
				}
			}
		}
		ZString clearingReason;

		public bool ClearingReasonsVisibility
		{
			get
			{
				if (!ScreeningStatus.IsEmpty)
				{
					return ScreeningStatus != ScreeningStatusesList.Codes.Matched;
				}

				return false;
			}
		}

		public bool ClearingReasonTextVisibility
		{
			get
			{
				if (ClearingReasonsVisibility && !ClearingReason.IsEmpty)
				{
					return true;
				}

				return false;
			}
		}

		public ZString ClearingReasonText
		{
			get => clearingReasonText;
			set
			{
				value = value.Replace("\r", string.Empty).Replace("\n", " ").Replace("\t", " ");

				if (value != clearingReasonText)
				{
					SetNonPersistentPropertyValue(ClearingReasonTextInfo, ref clearingReasonText, value);
					notifySavingButtonStatus?.Invoke();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateClearingReasonText();
				}
			}
		}

		ZString clearingReasonText;

		public ZPropertyInfo ClearingReasonTextInfo => GetZPropertyInfo(nameof(ClearingReasonText));

		public bool ClearingReasonTextEnabled
		{
			get
			{
				var enabled = false;

				if (!ClearingReason.IsEmpty)
				{
					if (RequireReasonForCLRItemForCurrentCode != null)
					{
						enabled = RequireReasonForCLRItemForCurrentCode.IsMandatory;
					}
					else if (ClearingReason == RequireReasonForCLRRegistryConstants.Code.Other)
					{
						enabled = true;
					}
				}

				return enabled;
			}
		}

		bool RequireClearingReasonForClr
		{
			get
			{
				var result = false;
				if (!ScreeningStatus.IsEmpty)
				{
					var status = ScreeningStatus;
					result = OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.Value.RequireReasonForCLR &&
							 (status == ScreeningStatusesList.Codes.Clear || status == ScreeningStatusesList.Codes.PermanentClear) &&
							 scoreGrade >= ScoreGrades.Medium;
				}

				return result;
			}
		}

		public string ClearingReasonTextValidationText
		{
			get
			{
				string result = null;
				if (RequireClearingReasonForClr)
				{
					if (string.IsNullOrWhiteSpace(ClearingReasonText))
					{
						result = RequireReasonForCLRItem.ClearingReasonValidationText;
					}
					else
					{
						result = RequireReasonForCLRItem.GetErrorMessage(ClearingReasonText);
					}
				}

				return result;
			}
		}

		public bool SaveButtonEnabled => !ScreeningStatus.IsEmpty && (ScreeningStatus == ScreeningStatusesList.Codes.Matched || string.IsNullOrEmpty(ClearingReasonTextValidationText));

		RequireReasonForCLRItem RequireReasonForCLRItemForCurrentCode => OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.Value.ItemCollection
			.Cast<RequireReasonForCLRItem>().FirstOrDefault(i => i.Code == ClearingReason);

		public List<NotifyPropertyChanged> NotifyPropertyChanges { get; } = new List<NotifyPropertyChanged>();

		public void ExecuteOpenHyperLinkCommand()
		{
			WebUrlLauncher.Launch(Uri);
		}

		void SetRegistryValueToClearingReasonTextIfNeeded(string code)
		{
			ClearingReasonText = string.Empty;

			if (code != RequireReasonForCLRRegistryConstants.Code.Other)
			{
				var registryPresetReason = OrganisationsDataRegistry.Instance
					.DeniedPartyScreeningRequireReasonForClearing.Value.ItemCollection.Cast<RequireReasonForCLRItem>()
					.FirstOrDefault(i => i.Code == code);

				if (registryPresetReason != null)
				{
					if (string.IsNullOrWhiteSpace(registryPresetReason.ClearingReason) && !registryPresetReason.IsMandatory)
					{
						ClearingReasonText = StmEntityScreeningLogSchema.PJ_ClearedReason.SqlDbDefault.ToString();
					}
					else
					{
						ClearingReasonText = registryPresetReason.ClearingReason;
					}
				}
			}
		}

		void InitializeStatus()
		{
			ScreeningStatuses = new CodeDescriptionPairList();

			if (screeningParty.Country is RefCountry)
			{
				ScreeningStatuses.AddPair(ScreeningStatusesList.Codes.Matched, ResString.GetMultilingualString("84B472CD-2326-479C-8366-5FAE728EF9DE", "Accept"));
			}
			else
			{
				ScreeningStatuses.AddPair(ScreeningStatusesList.Codes.Matched, ScreeningStatusesList.Descriptions.Matched);
				ScreeningStatuses.AddPair(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Descriptions.Clear);
			}
		}

		void InitializeClearingReasonLists()
		{
			ClearingReasons = new CodeDescriptionPairList();
			var itemCollection = OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.Value.ItemCollection;

			foreach (RequireReasonForCLRItem item in itemCollection)
			{
				ClearingReasons.AddPair(item.Code, item.Title);
			}

			if (Env.Security.OrgDeniedPartyScreeningAllowOtherReason.IsAllowed)
			{
				ClearingReasons.AddPair(RequireReasonForCLRRegistryConstants.Code.Other, RequireReasonForCLRRegistryConstants.Description.Other);
			}
		}

		public override ScreeningStatusWinModelValidation GetNewValidation()
		{
			return new ScreeningStatusWinModelValidation(this);
		}
	}
}
