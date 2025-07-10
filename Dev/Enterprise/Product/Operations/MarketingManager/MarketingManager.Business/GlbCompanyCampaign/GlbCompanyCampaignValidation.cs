using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class GlbCompanyCampaignValidation : AutoGlbCompanyCampaignValidation
	{
		public GlbCompanyCampaignValidation(AutoGlbCompanyCampaign parent)
			: base(parent)
		{
		}

		public new GlbCompanyCampaign Parent
		{
			get { return (GlbCompanyCampaign)base.Parent; }
		}

		#region G0_BroadcastVoteSurveyExam

		protected override void CheckG0_BroadcastVoteSurveyExam()
		{
			base.CheckG0_BroadcastVoteSurveyExam();
			MandatoryValidation.CheckEntered(Parent.G0_BroadcastVoteSurveyExamInfo);

			if (Parent.MasterCampaign != null)
			{
				if (Parent.MasterCampaign.IsDripMarketingCampaign)
				{
					ListValidation.ErrorIfInvalidCode(Parent.G0_BroadcastVoteSurveyExamInfo, Parent.Lookups.DripMarketingTouchTypeList);
					return;
				}

				if (Parent.MasterCampaign.IsInsideSalesCampaign)
				{
					ListValidation.ErrorIfInvalidCode(Parent.G0_BroadcastVoteSurveyExamInfo, Parent.Lookups.InsideSalesTouchTypeList);
					return;
				}
			}

			ListValidation.ErrorIfInvalidCode(Parent.G0_BroadcastVoteSurveyExamInfo, Parent.Lookups.CampaignTypeList);
		}

		#endregion

		#region G0_EstimatedStartedDate

		protected override void CheckG0_EstimatedStartedDate()
		{
			base.CheckG0_EstimatedStartedDate();
			MandatoryValidation.CheckEntered(Parent.G0_EstimatedStartedDateInfo);
		}

		#endregion

		#region G0_CampaignName

		protected override void CheckG0_CampaignName()
		{
			base.CheckG0_CampaignName();
			MandatoryValidation.CheckEntered(Parent.G0_CampaignNameInfo);
			TranslatableDataFieldAttribute.Validate(Parent.G0_CampaignNameInfo);
		}

		#endregion

		#region G0_CampaignName

		protected override void CheckG0_CampaignComment()
		{
			base.CheckG0_CampaignComment();
			TranslatableDataFieldAttribute.Validate(Parent.G0_CampaignCommentInfo);
		}

		#endregion

		#region G0_Stage

		protected override void CheckG0_Stage()
		{
			base.CheckG0_Stage();
			ListValidation.ErrorIfInvalidCode(Parent.G0_StageInfo, Parent.Lookups.CampaignStages);
		}

		#endregion

		#region G0_ActualCompletedDate

		protected override void CheckG0_ActualCompletedDate()
		{
			base.CheckG0_ActualCompletedDate();
			if (Parent.G0_ActualStartedDate.IsValid && !Parent.G0_ActualStartedDate.IsEmpty && Parent.G0_ActualStartedDate > Parent.G0_ActualCompletedDate)
			{
				Parent.G0_ActualCompletedDateInfo.AddError(Res.GetString("4cb62688-46ab-4028-b5d3-2c52d5dba53a", "Actual Completed date should be later than Actual Start Date"));
			}
		}

		#endregion

		#region G0_Type

		protected override void CheckG0_Type()
		{
			base.CheckG0_Type();
			MandatoryValidation.CheckEntered(Parent.G0_TypeInfo);

			if (!Parent.IsInDatabase || Parent.G0_TypeInfo.HasChanges || !Parent.Lookups.MediaTypesList.ContainsCode(Parent.G0_Type))
			{
				ListValidation.ErrorIfInvalidCode(Parent.G0_TypeInfo);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.G0_TypeInfo, Parent.Lookups.ActiveMediaTypesList, ListValidation.InactiveCodeMessage);
			}
		}

		#endregion

		#region G0_Category

		protected override void CheckG0_Category()
		{
			base.CheckG0_Category();
			MandatoryValidation.CheckEntered(Parent.G0_CategoryInfo);

			if (!Parent.IsInDatabase || Parent.G0_CategoryInfo.HasChanges || !Parent.Lookups.MediaCategoryList.ContainsCode(Parent.G0_Category))
			{
				ListValidation.ErrorIfInvalidCode(Parent.G0_CategoryInfo);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.G0_CategoryInfo, Parent.Lookups.ActiveMediaCategoryList, ListValidation.InactiveCodeMessage);
			}
		}

		#endregion

		#region G0_BatchCountDefault

		protected override void CheckG0_BatchCountDefault()
		{
			base.CheckG0_BatchCountDefault();
			if (Parent.G0_BatchCountDefault < 1 && !Parent.IsLinkTrackCampaign && !Parent.IsTouchCampaign)
			{
				Parent.G0_BatchCountDefaultInfo.AddError(Res.GetString("fa07a669-ac26-472f-ad24-4c4f6c261de8", "The default number of Campaigns sent in each batch must be larger than 0"));
			}
		}

		#endregion

		#region G0_EmailSubject

		protected override void CheckG0_EmailSubject()
		{
			base.CheckG0_EmailSubject();
			if (Parent.G0_EmailSubject.IsEmpty)
			{
				Parent.G0_EmailSubjectInfo.AddError(Res.GetString("53c3c43d-d328-476b-9bab-f765409c2733", "Email Subject must be entered"));
			}
			else
			{
				try
				{
					var readonlyFactory = Parent.Factory.GetCachedReadOnlyFactory();
					var emailParser = new CampaignDocumentParser(readonlyFactory);
					emailParser.Parse(CampaignDocumentParser.ParseType.PlainText, Parent.SimulationCampaignItem, Parent.G0_EmailSubject, new Dictionary<string, byte[]>());
				}
				catch (DocumentParsingFailedException)
				{
					Parent.G0_EmailSubjectInfo.AddError(EmailSubjectMacrosMalformedErrorText);
				}
			}
		}

		public static ResourceString EmailSubjectMacrosMalformedErrorText => ResString.GetMultilingualString("E84B7262-79AB-4699-AAF3-21503EEA59B1", "The macro(s) in the email subject are malformed. A start tag: '{0}' should always be followed by an end tag: '{1}'.", Core.Constants.DocumentEngine.EmailParsing.StartTag, Core.Constants.DocumentEngine.EmailParsing.EndTag);

		#endregion

		#region G0_EmailSenderOption

		protected override void CheckG0_EmailSenderOption()
		{
			base.CheckG0_EmailSenderOption();
			MandatoryValidation.CheckEntered(Parent.G0_EmailSenderOptionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.G0_EmailSenderOptionInfo);

			if (Parent.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.SPS)
			{
				Parent.SenderPool.ValidateAll();
				if (Parent.SenderPool.IsNullOrEmpty())
				{
					Parent.G0_EmailSenderOptionInfo.AddError(ErrorMessageForEmptyPool);
				}
				if (Parent.SenderPool.HasErrors())
				{
					Parent.G0_EmailSenderOptionInfo.AddError(ErrorMessageForBrokenPool);
				}
			}
		}

		#endregion

		#region G0_EmailSenderRole

		protected override void CheckG0_EmailSenderRole()
		{
			base.CheckG0_EmailSenderRole();
			if (Parent.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.ORG)
			{
				MandatoryValidation.CheckEntered(Parent.G0_EmailSenderRoleInfo);
				ListValidation.ErrorIfInvalidCode(Parent.G0_EmailSenderRoleInfo);
			}
		}

		#endregion

		#region G0_ReplyToEmail

		protected override void CheckG0_ReplyToEmail()
		{
			base.CheckG0_ReplyToEmail();
			if (!Parent.UseEmailSenderAddressAsReplyTo)
			{
				if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(Parent.G0_ReplyToEmail))
				{
					Parent.G0_ReplyToEmailInfo.AddError(Res.GetString("4a022055-94b5-49e1-97fd-15b9978f1340", "The 'reply to' email address is invalid."));
				}
			}
		}

		#endregion

		#region G0_SenderEmail

		protected override void CheckG0_SenderEmail()
		{
			base.CheckG0_SenderEmail();

			if (Parent.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.EML)
			{
				if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(Parent.SenderEmailAddress))
				{
					Parent.G0_SenderEmailInfo.AddError(Res.GetString("234a2ed4-cd82-4a19-8d05-6cb2f84abdfd", "The sender's email address is invalid."));
				}
			}

			if (EmailAddressValidation.IsEmailAddressValidAndNotEmpty(Parent.G0_SenderEmail))
			{
				var address = new MailAddress(Parent.G0_SenderEmail);
				var domain = address.Host;

				var smtpDomain = Env.Registry.SMTPServer;

				if (smtpDomain.IndexOf(domain) == -1)
				{
					Parent.G0_SenderEmailInfo.AddWarning(Res.GetString("a411c79a-52ea-4e9c-8492-c42dfa900ce1", "The email address entered does not match the SMTP Server configured. Emails sent from this address may be rejected."));
				}
			}
		}

		#endregion

		#region G0_GS_NKCampaignManager

		protected override void CheckG0_GS_NKCampaignManager()
		{
			base.CheckG0_GS_NKCampaignManager();
			MandatoryValidation.CheckEntered(Parent.G0_GS_NKCampaignManagerInfo);
			ListValidation.ErrorIfInvalidCode(Parent.G0_GS_NKCampaignManagerInfo, Parent.Lookups.CampaignManagers);
		}

		#endregion

		#region G0_GS_NKCampaignCoordinator

		protected override void CheckG0_GS_NKCampaignCoordinator()
		{
			base.CheckG0_GS_NKCampaignCoordinator();
			MandatoryValidation.CheckEntered(Parent.G0_GS_NKCampaignCoordinatorInfo);
			ListValidation.ErrorIfInvalidCode(Parent.G0_GS_NKCampaignCoordinatorInfo, Parent.Lookups.CampaignCoordinators);

			if (Parent.CampaignCoordinator != null && ShouldCheckForCampaignCoordinatorEmailAddress)
			{
				if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(Parent.CampaignCoordinator.GS_EmailAddress))
				{
					Parent.G0_GS_NKCampaignCoordinatorInfo.AddError(Res.GetString("1edd2677-db02-426d-8eac-728b47722a1f", "The selected campaign coordinator does not have a valid email address.\r\nPlease choose another campaign coordinator or change this staff member's email address"));
				}
				else
				{
					var mainEmailAddress = Parent.CampaignCoordinator.EmailAddresses.FindByEmailAddressType(Core.Constants.EmailFromAddressTypes.Codes.Main);

					if (mainEmailAddress != null && mainEmailAddress.IsNDR)
					{
						Parent.G0_GS_NKCampaignCoordinatorInfo.AddWarning(Res.GetString("D0F8FB5F-3353-4BF3-A4D3-A3410C2F7D77", "The selected campaign coordinator’s email address is marked as an NDR.\r\nThis means they can’t be notified of any campaign delivery failures. Please clear the NDR setting against their email address or use another coordinator."));
					}
				}
			}
		}

		protected virtual bool ShouldCheckForCampaignCoordinatorEmailAddress
		{
			get { return true; }
		}

		#endregion

		#region G0_RX_NKCampaignCurrency

		protected override void CheckG0_RX_NKCampaignCurrency()
		{
			base.CheckG0_RX_NKCampaignCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.G0_RX_NKCampaignCurrencyInfo, Parent.Lookups.CampaignCurrencies);
		}

		#endregion

		#region G0_QuestionsPerWebPage

		protected override void CheckG0_QuestionsPerWebPage()
		{
			base.CheckG0_QuestionsPerWebPage();
			if (Parent.G0_BroadcastVoteSurveyExam != CampaignTypeList.Codes.Broadcast)
			{
				CompareValidation.CheckWithinRange(Parent.G0_QuestionsPerWebPageInfo, 1, 999);
			}
		}

		#endregion

		#region G0_DocumentBlob

		protected override void CheckG0_DocumentBlob()
		{
			base.CheckG0_DocumentBlob();
			if (Parent.IsEmailCampaign)
			{
				if (Parent.IsTouchCampaign && Parent.EmailContentIsNotSet)
				{
					Parent.G0_DocumentBlobInfo.AddError(GlbCompanyCampaignSender.NotificationConstants.EmailContentCannotBeEmpty);
				}
				else if (Parent.EmailContentIsNotSetButRequired && Parent.HasQueuedItems)
				{
					Parent.G0_DocumentBlobInfo.AddError(EmailContentIsNotSetWhenScheduledText);
				}

				if (!Parent.EmailContentIsNotSet)
				{
					try
					{
						var readonlyFactory = Parent.Factory.GetCachedReadOnlyFactory();
						var emailParser = new CampaignDocumentParser(readonlyFactory);
						emailParser.Parse(CampaignDocumentParser.ParseType.HtmlEmail, Parent.SimulationCampaignItem, Parent.HtmlTextToSend, new Dictionary<string, byte[]>());
					}
					catch (DocumentParsingFailedException)
					{
						Parent.G0_DocumentBlobInfo.AddError(EmailContentMacrosMalformedErrorText);
					}
					catch (UriFormatException e)
					{
						Parent.G0_DocumentBlobInfo.AddError(e.Message);
					}
				}
			}
		}

		protected override void CheckG0_DocumentBlobIsValidZBlobSize()
		{
			// block default blob size validation
		}

		public static ResourceString EmailContentMacrosMalformedErrorText => ResString.GetMultilingualString("CCB43618-A1BB-4EFE-81A3-7F9ED467D649", "The macro(s) in the email content are malformed. A start tag: '{0}' should always be followed by an end tag: '{1}'.", Core.Constants.DocumentEngine.EmailParsing.StartTag, Core.Constants.DocumentEngine.EmailParsing.EndTag);

		public static string EmailContentIsNotSetWhenScheduledText => Res.GetString("9c662b54-6a1b-45a6-96f8-beaeb0f45f93", "Email content cannot be empty for campaign with active send schedules.");

		#endregion

		#region G0_RL_NKEmailSenderUNLOCO

		protected override void CheckG0_RL_NKEmailSenderUNLOCO()
		{
			base.CheckG0_RL_NKEmailSenderUNLOCO();
			if (Parent.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.EML)
			{
				MandatoryValidation.CheckEntered(Parent.G0_RL_NKEmailSenderUNLOCOInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.G0_RL_NKEmailSenderUNLOCOInfo, Parent.Lookups.EmailSenderUNLOCOs);
		}

		#endregion

		#region CampaignID

		public void ValidateCampaignID()
		{
			ValidateCalculatedProperty(Parent.CampaignIDInfo);
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		protected virtual void CheckCampaignID()
		{
			if (!Parent.G0_EstimatedStartedDateInfo.HasErrors() && !Parent.G0_CategoryInfo.HasErrors()
				&& !Parent.G0_TypeInfo.HasErrors() && !Parent.G0_CampaignNameInfo.HasErrors())
			{
				ZQuery filter = new ZQuery(GlbCompanyCampaignSchema.G0_EstimatedStartedDate, Parent.G0_EstimatedStartedDate);
				filter.AddToFilter(GlbCompanyCampaignSchema.G0_Category, Parent.G0_Category);
				filter.AddToFilter(GlbCompanyCampaignSchema.G0_Type, Parent.G0_Type);
				filter.AddToFilter(GlbCompanyCampaignSchema.G0_CampaignName, Parent.G0_CampaignName);
				filter.AddToFilter(GlbCompanyCampaignSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				bool existsAlready = Parent.Factory.Exists(typeof(GlbCompanyCampaign), filter);

				if (existsAlready)
				{
					Parent.CampaignIDInfo.AddError(Res.GetString("c8d489aa-a0a2-4f25-a2e4-adc8b4146cdd", "A campaign already exists with the same Name, Estimated Start Date, Media Type and Media Category. Please ensure you do not enter duplicate campaigns."));
				}
			}
		}

		#endregion

		#region DocumentAttachedMessage

		public void ValidateIsDocumentAttached()
		{
			ValidateCalculatedProperty(Parent.IsDocumentAttachedInfo);
		}

		protected void CheckIsDocumentAttached()
		{
			if (Parent.RequiresCampaignURL && (!Parent.EmailContentIsNotSet || Parent.IsTouchCampaign))
			{
				if (IsCampaignURLFieldMissing(out var errorMessage))
				{
					Parent.IsDocumentAttachedInfo.AddError(errorMessage);
				}
			}
		}

		public bool IsCampaignURLFieldMissing(out string errorMessage)
		{
			errorMessage = string.Empty;
			string field = string.Format(CultureInfo.InvariantCulture, "(*{0}*)", GlbCompanyCampaign.CampaignURLDocFieldName);
			if (!Parent.HtmlTextToSend.Contains(field))
			{
				errorMessage = Res.GetString("6b7e6ab8-5ef6-4625-9508-95d81a237c0f", "HTML document should include field {0}", field);
				return true;
			}
			return false;
		}

		#endregion

		public void ValidateCoordinatorEmailAddress()
		{
			ValidateCalculatedProperty(Parent.CoordinatorEmailAddressInfo);
		}

		protected void CheckCoordinatorEmailAddress()
		{
			if (Parent.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.COR)
			{
				var validEmails = Parent.CoordinatorEmailAddressList.ToArray().Select(x => x.Description).ToList();
				if (!validEmails.Contains(Parent.CoordinatorEmailAddress))
				{
					Parent.CoordinatorEmailAddressInfo.AddError(Res.GetString("FBCCFF78-838F-4EE4-A962-20DC9B5BAD06", "Invalid Coordinator Email address."));
				}
			}
		}

		#region ContactDataSource

		string DataSourceLastSenderMessage
		{
			get
			{
				if (dataSourceLastSenderMessage == null)
				{
					dataSourceLastSenderMessage = Res.GetString("7E49EDF4-0C64-4C2F-B0EF-C5F2088F7C82", "Campaign Tracking Data Source should be chosen when Last Sender function is activated.");
				}

				return dataSourceLastSenderMessage;
			}
		}
		string dataSourceLastSenderMessage;

		public void ValidateContactDataSource()
		{
			ValidateCalculatedProperty(Parent.ContactDataSourceInfo);
		}

		protected virtual void CheckContactDataSource()
		{
			if (!Parent.IsValidationOnNonPersistentPropertiesSuspended)
			{
				MandatoryValidation.CheckEntered(Parent.ContactDataSourceInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ContactDataSourceInfo);

				if (!Parent.IsTouchCampaign && Parent.G0_UseLastEmailSenderAddress)
				{
					if (Parent.ContactDataSource != ContactDataSourceList.Codes.CampaignTracking)
					{
						Parent.ContactDataSourceInfo.AddWarning(DataSourceLastSenderMessage);
					}
				}
			}
		}

		#endregion

		#region G0_UseLastEmailSenderAddress

		protected override void CheckG0_UseLastEmailSenderAddress()
		{
			base.CheckG0_UseLastEmailSenderAddress();

			if (!Parent.IsTouchCampaign && Parent.G0_UseLastEmailSenderAddress)
			{
				if (Parent.ContactDataSource != ContactDataSourceList.Codes.CampaignTracking)
				{
					Parent.G0_UseLastEmailSenderAddressInfo.AddWarning(DataSourceLastSenderMessage);
				}
			}
		}

		#endregion

		#region SourceCampaignPK

		public void ValidateSourceCampaignPK()
		{
			ValidateCalculatedProperty(Parent.SourceCampaignPKInfo);
		}

		protected void CheckSourceCampaignPK()
		{
			if (!Parent.IsValidationOnNonPersistentPropertiesSuspended && Parent.IsUsingCampaignTrackingDataSource && !Parent.IsTouchCampaign)
			{
				IMultilingualString propertyDescription = ResString.GetMultilingualString("84CC5643-F095-4D52-95E5-634E5E5A9021", "Source Campaign");
				MandatoryValidation.CheckEntered(Parent.SourceCampaignPKInfo, propertyDescription);
				ListValidation.ErrorIfInvalidPK(Parent.SourceCampaignPKInfo, propertyDescription);

				if (!Parent.SourceCampaignPKInfo.HasErrors())
				{
					if (Parent.PK == Parent.SourceCampaignPK)
					{
						Parent.SourceCampaignPKInfo.AddError(Res.GetString("3F8D2A44-29D2-4A84-9855-193AD533DC59", "Source Campaign cannot be a follow up campaign of itself."));
					}
					else
					{
						if (IsAlreadyAFollowUpCampaign())
						{
							Parent.SourceCampaignPKInfo.AddError(Res.GetString("8B6421F4-6545-47F8-AA48-094A2E1495B8", "Source Campaign is already a follow up campaign of this campaign."));
						}
					}
				}
			}
		}

		public void ValidateTouchSourceCampaignPKsForValidation()
		{
			ValidateCalculatedProperty(Parent.TouchSourceCampaignPKsForValidationInfo);
		}

		protected void CheckTouchSourceCampaignPKsForValidation()
		{
			if (!Parent.IsValidationOnNonPersistentPropertiesSuspended && Parent.IsUsingCampaignTrackingDataSource && Parent.IsTouchCampaign)
			{
				var propertyDescription = ResString.GetMultilingualString("3BD9AD3D-71CD-4878-9502-92CB67546744", "Source Campaigns").ToString();
				if (Parent.TouchSourceCampaignPKs == null || Parent.TouchSourceCampaignPKs.Length == 0)
				{
					Parent.TouchSourceCampaignPKsForValidationInfo.AddError(MandatoryValidation.MustBeEnteredMessage(propertyDescription));
					Parent.AddRowError(MandatoryValidation.MustBeEnteredMessage(propertyDescription));
				}
				else
				{
					Parent.RemoveRowError(MandatoryValidation.MustBeEnteredMessage(propertyDescription));
				}

				bool hadInvalidPks = false;
				foreach (var pk in Parent.TouchSourceCampaignPKs)
				{
					if (!pk.IsValid)
					{
						Parent.AddRowError(ListValidation.GetNotificationMessage(propertyDescription).ToString());
						Parent.TouchSourceCampaignPKsForValidationInfo.AddError(ListValidation.GetNotificationMessage(propertyDescription).ToString());
						hadInvalidPks = true;
						break;
					}
				}

				if (!hadInvalidPks)
				{
					Parent.RemoveRowError(ListValidation.GetNotificationMessage(propertyDescription).ToString());
				}

				var query = new ZQuery(GlbCompanyCampaignSchema.PK, Parent.TouchSourceCampaignPKs);
				var sourceCampaigns = Parent.Factory.Load<GlbCompanyCampaign>(query);

				var sameDripError = Res.GetString("E4701D52-7E21-4093-A203-32774DEE9778", "Should be a followup campaign within same Master Campaign.");
				var sourceIsSelfError = Res.GetString("3F8D2A44-29D2-4A84-9855-193AD533DC59", "Source Campaign cannot be a follow up campaign of itself.");

				bool hadSameDripError = false;
				bool hadSourceIsSelfError = false;

				foreach (var sourceCampaign in sourceCampaigns)
				{
					bool sourceIsMaster = Parent.MasterCampaign.PK == sourceCampaign.PK;
					bool sourceIsTouchWithinSameMaster = sourceCampaign.IsTouchCampaign && Parent.MasterCampaign.PK == sourceCampaign.MasterCampaign.PK;

					if (!sourceIsMaster && !sourceIsTouchWithinSameMaster)
					{
						Parent.TouchSourceCampaignPKsForValidationInfo.AddError(sameDripError);
						Parent.AddRowError(sameDripError);
						hadSameDripError = true;
					}
					else if (Parent.PK == sourceCampaign.PK)
					{
						Parent.TouchSourceCampaignPKsForValidationInfo.AddError(sourceIsSelfError);
						Parent.AddRowError(sourceIsSelfError);
						hadSourceIsSelfError = true;
					}
				}

				if (!hadSameDripError)
				{
					Parent.RemoveRowError(sameDripError);
				}

				if (!hadSourceIsSelfError)
				{
					Parent.RemoveRowError(sourceIsSelfError);
				}
			}
		}

		bool IsAlreadyAFollowUpCampaign()
		{
			bool result = false;
			var sourceCampaign = Parent.Factory.Load<GlbCompanyCampaign>(Parent.SourceCampaignPK);
			if (sourceCampaign != null)
			{
				var parentValidationResult = Parent.RelatedParentActivityPivotCollection.CheckIsValidActivity(sourceCampaign, true);
				result = !parentValidationResult.IsValid;
			}
			return result;
		}

		#endregion

		#region SenderPool Validation

		static MultilingualString ErrorMessageForEmptyPool { get; } = ResString.GetMultilingualString("b10c1bf1-25cc-4835-b375-184e52aa5538", "Sender Pool cannot be Empty.");
		static MultilingualString ErrorMessageForBrokenPool { get; } = ResString.GetMultilingualString("809df960-9f5c-4c13-9642-dea8ed5d9b8c", "Errors in Sender Pool.");

		public void ValidateSenderPool()
		{
			ValidateG0_EmailSenderOption();
		}

		#endregion

		#region G0_GroupRatio

		protected override void CheckG0_GroupRatio()
		{
			base.CheckG0_GroupRatio();

			if (!Parent.IsTouchCampaign)
			{
				return;
			}

			var messageRatio = Res.GetString("E85585B9-2D83-48BA-9AC7-C641C1D462CC", "Ratio should be between 0 and 100 for campaign: {0}", Parent.G0_CampaignNameMultilingual);

			if (!Parent.CurrentGroupColor.IsEmpty && (Parent.G0_GroupRatio < 0 || Parent.G0_GroupRatio > 100))
			{
				Parent.G0_GroupRatioInfo.AddError(messageRatio);
			}
		}

		#endregion

		#region ValidateTouches

		void ValidateTouches()
		{
			if (!Parent.IsMasterCampaign)
			{
				return;
			}

			var titleValidationMessage = Res.GetString("6CE25279-B401-4DFD-BA70-F5A91FD0A3F4", "The following Touch Point(s) have no transition capability:");
			Parent.RemoveRowError(titleValidationMessage, true);

			var validationMessageStringBuilder = new ZStringBuilder();
			foreach (var touch in Parent.AllTouches)
			{
				if (touch.TransitionRulesToThisCampaign.Count == 0 || touch.TransitionRulesToThisCampaign.All(r => r.IsDeleted))
				{
					validationMessageStringBuilder.AppendLine(touch.G0_CampaignNameMultilingual);
				}
			}

			if (validationMessageStringBuilder.Length > 0)
			{
				validationMessageStringBuilder.Prepend(titleValidationMessage);
				Parent.AddRowError(validationMessageStringBuilder.ToStringWithNewLineBetweenAppends());
			}
		}

		#endregion

		#region Validate Send Scheduled Campaign

		public void ValidateSendSchedule()
		{
			base.ValidateAll();
			ValidateCampaignID();
			ValidateIsDocumentAttached();
			ValidateCoordinatorEmailAddress();
			ValidateContactDataSource();
			ValidateSourceCampaignPK();
			ValidateSenderPool();
			ValidateTouches();
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCampaignID();
			ValidateIsDocumentAttached();
			ValidateCoordinatorEmailAddress();
			ValidateContactDataSource();
			ValidateSourceCampaignPK();
			ValidateSenderPool();
			ValidateTouchSourceCampaignPKsForValidation();
			ValidateTouches();

			if (Parent.IsOpportunityCreationCampaign)
			{
				Parent.OpportunityCreationTemplate.Validation.ValidateAll();
			}
		}

		#endregion
	}
}
