using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.ServiceManager;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbBranchValidation : AutoGlbBranchValidation
	{
		public GlbBranchValidation(AutoGlbBranch parent)
			: base(parent)
		{
			phoneNumberFormatterAndValidatorThunk = new Lazy<PhoneNumberFormatterAndValidator>(() => new PhoneNumberFormatterAndValidator());
		}

		new GlbBranch Parent
		{
			get { return (GlbBranch)base.Parent; }
		}

		AddressValidation addressValidation;
		AddressValidation AddressValidation => addressValidation ?? (addressValidation = new AddressValidation());

		#region CheckGB_Code

		protected override void CheckGB_Code()
		{
			base.CheckGB_Code();
			if (!Parent.GB_CodeInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.GB_CodeInfo);
				if (!Parent.GB_CodeInfo.HasErrors())
				{
					if (Parent.GB_Code.Length < 3)
					{
						Parent.GB_CodeInfo.AddError(Res.GetString("296ac88e-6501-432b-9ec0-beea2568d47b", "The length of Branch Code should be 3 characters."));
					}
					else
					{
						var filter = new ZQuery(GlbBranchSchema.GB_Code, SQLComparisonOperator.Equal, Parent.GB_Code);
						filter.AddToFilter(JoinCondition.And, GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

						if (Parent.Factory.Exists(typeof(GlbBranch), filter))
						{
							Parent.GB_CodeInfo.AddError(Res.GetString("7b60412f-af9d-4cbb-84a4-9a499efaae1c", "This code already exists. Please enter another code."));
						}
					}
				}
			}
		}

		#endregion

		#region CheckGB_IsActive
#if DEBUG
		internal
#endif
		const string ServiceTaskNameSeperator = "\r\n\t+ ";

		static string FormatServiceTasksForDisplay(IEnumerable<IStmScheduleTask> tasks)
		{
			return ServiceTaskNameSeperator + String.Join(ServiceTaskNameSeperator, tasks.Select(task => task.S5_ScheduleDescription).OrderBy(s => s));
		}

		void ValidateGlbStaffForInactiveBranch()
		{
			if (Parent.RelatedStaff.Any())
			{
				var activeStaff = Parent.RelatedStaff.Where(staff => staff.GS_IsActive).ToList();
				if (activeStaff.Count > 0)
				{
					Parent.GB_IsActiveInfo.AddError(
						Res.GetString("85716FF8-FF91-440C-B114-2F397E8D90B0",
						"This branch has {0} active staff. Please amend the staff to a new Home Branch, deactivate them, or delete them before deactivating this branch.",
						activeStaff.Count)
						);
				}

				var inactiveStaff = Parent.RelatedStaff.Where(staff => !staff.GS_IsActive).ToList();
				if (inactiveStaff.Count > 0)
				{
					Parent.GB_IsActiveInfo.AddWarning(
						Res.GetString("520FEF55-48CA-4F7D-89FC-7EEF0F1ADE97",
						"This branch has {0} inactive staff. You may wish to amend the staff to a new Home Branch, or delete them before deactivating this branch.",
						inactiveStaff.Count)
						);
				}
			}
		}

		protected override void CheckGB_IsActive()
		{
			base.CheckGB_IsActive();
			if (!Parent.GB_IsActiveInfo.HasErrors())
			{
				if (Parent.GB_IsActive)
				{
					if (Parent.Company != null && !Parent.Company.GC_IsActive)
					{
						Parent.GB_IsActiveInfo.AddError(Res.GetString("E0AFE0DF-BD1A-4E58-8F70-4F8067EDE266", "Activate the global company before activating company branches"));
					}
				}
				else
				{
					if (Parent.IsLastActiveBranch)
					{
						Parent.GB_IsActiveInfo.AddError(Res.GetString("a9a9b728-ee50-4c78-b010-89fc215l39bf8", "This branch is the last active branch and cannot be deactivated."));
					}

					foreach (var scheduleTasks in Parent.RelatedScheduleTasks.GroupBy(task => task.S5_ParentTableCode))
					{
						string tasksName;

						switch (scheduleTasks.Key)
						{
							case Constants.ServiceTask.ParentTableCode:
								tasksName = Res.GetString("9a120bfc-e910-4935-b05a-a6aed6dab4da", "service tasks");
								break;
							case Constants.ReportSchedule.ParentTableCode:
								tasksName = Res.GetString("49916801-dd59-4ff1-b6fc-1fd7d92f300a", "scheduled reports");
								break;
							case Constants.ArchiveManager.ParentTableCode:
								tasksName = Res.GetString("810a81cb-7280-473f-99fd-de20cdff5cde", "archive manager schedules");
								break;
							case Constants.ScheduleUniversalCopyTask.ParentTableCode:
								tasksName = Res.GetString("cd59a18a-fb27-47bf-bd05-0c3d353cb915", "universal copy schedules");
								break;
							default:
								tasksName = null;
								ErrorReporter.ReportOnce(String.Format("Unknown S5_ParentTableCode '{0}', Names: {1}", scheduleTasks.Key, String.Join(", ", scheduleTasks.Select(task => task.S5_ScheduleDescription))));
								break;
						}

						if (!string.IsNullOrEmpty(tasksName))
						{
							var activeTasks = scheduleTasks.Where(task => task.S5_IsActive).ToList();
							if (activeTasks.Count > 0)
							{
								AddErrorForActiveScheduleTasks(activeTasks, tasksName);
							}

							var inactiveTasks = scheduleTasks.Where(task => !task.S5_IsActive).ToList();
							if (inactiveTasks.Count > 0)
							{
								AddNotificationForInactiveScheduleTasks(inactiveTasks, tasksName);
							}
						}
					}

					ValidateGlbStaffForInactiveBranch();

					if (DataRegistry.Instance.WebBranch == Parent.PK && !DataRegistry.Instance.WebBranch.Equals(RawDataRegistry.Instance.WebBranch.DefaultValue))
					{
						Parent.GB_IsActiveInfo.AddError(Res.GetString("EF50941F-E9E1-42C1-8FB6-F5BEF89A6CD6", "This branch is the web branch and cannot be deactivated. Please change the branch under Registry > Web > Web Branch in order to deactivate this branch."));
					}
				}
			}
		}

		void AddErrorForActiveScheduleTasks(IEnumerable<IStmScheduleTask> scheduleTasks, string tasksName)
		{
			Parent.GB_IsActiveInfo.AddError(Res.GetString("574c1961-54ce-4003-af19-50a59175f9a4",
				"This branch has {0} active {1}. Please amend the {1} to a new branch, deactivate, or delete them before deactivating this branch.\r\nThe active {1} assigned to this branch are {2}",
				scheduleTasks.Count(), tasksName, FormatServiceTasksForDisplay(scheduleTasks)));
		}

		void AddNotificationForInactiveScheduleTasks(IEnumerable<IStmScheduleTask> scheduleTasks, string tasksName)
		{
			Parent.GB_IsActiveInfo.AddWarning(Res.GetString("a9f34a44-81c1-4fa1-9a10-34de7e75ca72",
				"This branch has {0} inactive {1}. You may wish to amend the {1} to a new branch, or delete them before deactivating this branch.\r\nThe inactive {1} assigned to this branch are {2}",
				scheduleTasks.Count(), tasksName, FormatServiceTasksForDisplay(scheduleTasks)));
		}

		#endregion

		#region CheckGB_BranchName

		protected override void CheckGB_BranchName()
		{
			base.CheckGB_BranchName();
			if (!Parent.GB_BranchNameInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.GB_BranchNameInfo);
			}
		}

		#endregion

		#region CheckGB_Address1

		protected override void CheckGB_Address1()
		{
			base.CheckGB_Address1();
			if (!Parent.GB_Address1Info.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.GB_Address1Info);
			}
		}

		#endregion

		#region CheckGB_City

		protected override void CheckGB_City()
		{
			base.CheckGB_City();
			if ((!ShouldValidateAddress() || !AddressValidation.IsWebVerified(Parent.ValidationStatus)) && !Parent.GB_CityInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.GB_CityInfo);
			}
		}

		bool ShouldValidateAddress()
		{
			return
				Parent.GB_IsActive &&
				Parent.BaseCountry != null &&
				OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(Parent.BaseCountry.PK.ToGuid(), Parent.ValidationSection);
		}

		#endregion

		#region CheckGB_PostCode

		protected override void CheckGB_PostCode()
		{
			base.CheckGB_PostCode();
			AddressValidation.CheckPostCode(Parent.GB_PostCodeInfo, Parent.BaseCountry, Parent.ValidationSection, Parent.ValidationStatus);
		}

		#endregion

		#region CheckGB_State

		protected override void CheckGB_State()
		{
			base.CheckGB_State();
			AddressValidation.CheckState(Parent.GB_StateInfo, Parent.BaseCountry, Parent.ValidationSection, Parent.ValidationStatus);
		}

		#endregion

		#region Phone Numbers

		#region GB_Phone_Formatted

		public void ValidateGB_Phone_Formatted()
		{
			ValidateCalculatedProperty(Parent.GB_Phone_FormattedInfo);
		}

		protected virtual void CheckGB_Phone_Formatted()
		{
			if (Parent.GB_IsActive)
			{
				if (!Parent.GB_Phone_FormattedInfo.HasErrors())
				{
					MandatoryValidation.CheckEntered(Parent.GB_Phone_FormattedInfo);
				}
				ValidatePhoneNumber(Parent.GB_Phone_FormattedInfo, Parent.GB_PhoneInfo, Parent.GB_Phone_IsManuallyVerifiedInfo);
			}
		}

		#endregion

		#region GB_Fax_Formatted

		public void ValidateGB_Fax_Formatted()
		{
			ValidateCalculatedProperty(Parent.GB_Fax_FormattedInfo);
		}

		protected virtual void CheckGB_Fax_Formatted()
		{
			ValidatePhoneNumber(Parent.GB_Fax_FormattedInfo, Parent.GB_FaxInfo, Parent.GB_Fax_IsManuallyVerifiedInfo);
		}

		#endregion

		#region Implementations

		void ValidatePhoneNumber(ZPropertyInfo phoneNumberProperty, ZPropertyInfo rawPhoneNumberProperty, ZPropertyInfo phoneNumberIsManuallyVerifiedProperty)
		{
			PhoneNumberFormatterAndValidator.Validate(phoneNumberProperty, rawPhoneNumberProperty, phoneNumberIsManuallyVerifiedProperty, Parent.DefaultCountryCodeForPhoneNumbers);
		}

		protected PhoneNumberFormatterAndValidator PhoneNumberFormatterAndValidator
		{
			get { return phoneNumberFormatterAndValidatorThunk.Value; }
		}

		readonly Lazy<PhoneNumberFormatterAndValidator> phoneNumberFormatterAndValidatorThunk;

		#endregion

		#endregion

		#region CheckGB_RL_NKHomePort

		protected override void CheckGB_RL_NKHomePort()
		{
			base.CheckGB_RL_NKHomePort();
			if (!Parent.GB_RL_NKHomePortInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.GB_RL_NKHomePortInfo);
				ListValidation.ErrorIfInvalidCode(Parent.GB_RL_NKHomePortInfo);
			}

			if (!Parent.GB_RL_NKHomePortInfo.HasErrors())
			{
				if (!Env.Security.BranchModify.IsAllowed && Parent.GB_RL_NKHomePortInfo.HasChanges)
				{
					Parent.GB_RL_NKHomePortInfo.AddError(Env.Security.BranchModify.ErrorMessageForNotAllowed);
				}
			}

			if (!Parent.GB_RL_NKHomePortInfo.HasErrors())
			{
				if (Parent.Company != null && !Parent.GB_RL_NKHomePort.IsEmpty && !Parent.Company.GC_RN_NKCountryCode.IsEmpty && (Parent.HomePort.RL_RN_NKCountryCode != Parent.Company.GC_RN_NKCountryCode))
				{
					string portString = Parent.HomePort.RL_RN_NKCountryCode;
					Parent.GB_RL_NKHomePortInfo.AddWarning(Res.GetString("61BA5F77-1706-4614-92A0-98641C97340E", "The Home Port entered belongs to a different country/region to the country/region code entered on the company specified. Please ensure this is correct. If the home port is correct It is strongly recommend that you create a company for {0} as accounting information will be entered against this company", portString));
				}
			}
		}

		#endregion

		#region CheckGB_Email

		protected override void CheckGB_Email()
		{
			if (!Parent.GB_Email.IsEmpty)
			{
				AddressValidation.CheckEmail(Parent.GB_EmailInfo);
			}
		}

		#endregion

		#region CheckGB_WebAddress

		protected override void CheckGB_WebAddress()
		{
			if (!Parent.GB_WebAddress.IsEmpty && !Parent.GB_WebAddressInfo.HasErrors() && !UrlValidation.IsValidUrl(Parent.GB_WebAddress))
			{
				Parent.GB_WebAddressInfo.AddError(Res.GetString("02093877-0be0-45d7-8262-d169fd19c01c", "Please enter a valid website address (URL).\r\n\r\nA valid address is commonly found in the format \"{0}\" or \"{1}\"", "http://", "www."));
			}
		}

		#endregion

		#region CheckGB_AccountingGroupCode

		protected override void CheckGB_AccountingGroupCode()
		{
			base.CheckGB_AccountingGroupCode();
			if (!Parent.GB_AccountingGroupCodeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.GB_AccountingGroupCodeInfo);
			}
		}

		#endregion

		#region GB_RN_NKCountryCode
		protected override void CheckGB_RN_NKCountryCode()
		{
			base.CheckGB_RN_NKCountryCode();
			var info = Parent.GB_RN_NKCountryCodeInfo;

			if (!info.HasErrors())
			{
				MandatoryValidation.CheckEntered(info);
				ListValidation.ErrorIfInvalidCode(info);
			}
		}
		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateGB_Phone_Formatted();
			ValidateGB_Fax_Formatted();
		}

		protected override void CheckGB_ValidationStatus()
		{
			base.CheckGB_ValidationStatus();

			if (ShouldValidateAddress() && Parent.GB_ValidationStatus == AddressValidationStatus.Invalid && !Parent.IgnoreValidationStatusError)
			{
				var shouldSuppressError = OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled;

				var message = Res.GetString(
					"5b1cccd7-c2fc-4083-9010-4a6e7efb198f",
					"There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

				if (shouldSuppressError)
				{
					Parent.GB_ValidationStatusInfo.AddWarning(message);
				}
				else
				{
					Parent.GB_ValidationStatusInfo.AddError(message);
				}
			}
		}
	}
}
