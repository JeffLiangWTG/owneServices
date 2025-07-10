using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.RtfConverter;

namespace Enterprise.MarketingManager.Business
{
	public class OpportunityCreationTemplate : NonPersistentBusinessObject
	{
		public static class Schema
		{
			public const string PackageType = "PackageType";
			public const string OpportunityType = "OpportunityType";
			public const string OpportunityDescription = "OpportunityDescription";
			public const string OpportunityStatus = "OpportunityStatus";
			public const string OpportunityStage = "OpportunityStage";
			public const string Source = "Source";
			public const string SourceDetails = "SourceDetails";
			public const string OpportunityAssignments = "OpportunityAssignments";
			public const string OpportunityNotes = "OpportunityNotes";
		}

		public OpportunityCreationTemplate(GlbCompanyCampaign campaign) : base(campaign?.Factory)
		{
			this.Campaign = campaign;
			SetOpportunityCreationTemplateValues();
		}

		public readonly GlbCompanyCampaign Campaign;

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (Campaign != null && !Campaign.IsDeleted)
			{
				if (!Campaign.IsOpportunityCreationCampaign)
				{
					ClearAllValues();
				}

				Campaign.G0_OpportunityContent = OpportunityContent;
			}
		}

		void SetOpportunityCreationTemplateValues()
		{
			ParseOpportunityContentString(Campaign.G0_OpportunityContent);
		}

		internal void ParseOpportunityContentString(string templateString)
		{
			if (!string.IsNullOrEmpty(templateString))
			{
				var parts = templateString.Split(Separator);

				PackageType = parts[0];
				OpportunityType = parts[1];
				OpportunityDescription = parts[2];
				OpportunityStatus = parts[3];
				OpportunityStage = parts[4];
				Source = parts[5];
				ActiveSourceDetails = parts[6];
				OpportunityAssignment = parts[7];
				SalesPerson = parts[8];
				StaffAssignment = parts[9];
				OpportunityNotes = ZBlob.FromUTF8(parts[10]);
			}
		}

		internal void ClearAllValues()
		{
			PackageTypeInfo.ClearValue();
			OpportunityTypeInfo.ClearValue();
			OpportunityDescriptionInfo.ClearValue();
			OpportunityStatusInfo.ClearValue();
			OpportunityStageInfo.ClearValue();
			SourceInfo.ClearValue();
			ActiveSourceDetailsInfo.ClearValue();
			OpportunityAssignmentInfo.ClearValue();
			SalesPersonInfo.ClearValue();
			StaffAssignmentInfo.ClearValue();
			OpportunityNotesInfo.ClearValue();
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public char Separator = '¦';

		#region Properties

		#region OpportunityTemplate

		ZString OpportunityContent => PackageType + Separator + OpportunityType + Separator + OpportunityDescription + Separator + OpportunityStatus + Separator + OpportunityStage + Separator + Source + Separator + ActiveSourceDetails + Separator + OpportunityAssignment + Separator + SalesPerson + Separator + StaffAssignment + Separator + OpportunityNotes.ToUTF8();

		#endregion

		#region PackageType

		[List("Lookups.PackageTypeList")]
		public ZString PackageType
		{
			get => packageType;
			set => SetNonPersistentPropertyValue(PackageTypeInfo, ref packageType, value);
		}
		ZString packageType;

		public ZPropertyInfo PackageTypeInfo => GetZPropertyInfo(nameof(PackageType));

		#endregion

		#region OpportunityType

		[List("Lookups.OpportunityTypeList")]
		public ZString OpportunityType
		{
			get => opportunityType;
			set => SetNonPersistentPropertyValue(OpportunityTypeInfo, ref opportunityType, value);
		}
		ZString opportunityType;

		public ZPropertyInfo OpportunityTypeInfo => GetZPropertyInfo(nameof(OpportunityType));

		#endregion

		#region OpportunityDescription

		public ZString OpportunityDescription
		{
			get => opportunityDescription;
			set => SetNonPersistentPropertyValue(OpportunityDescriptionInfo, ref opportunityDescription, value);
		}
		ZString opportunityDescription;

		public ZPropertyInfo OpportunityDescriptionInfo => GetZPropertyInfo(nameof(OpportunityDescription));

		#endregion

		#region OpportunityStatus

		[List("Lookups.OpportunityStatusList")]
		public ZString OpportunityStatus
		{
			get => opportunityStatus;
			set => SetNonPersistentPropertyValue(OpportunityStatusInfo, ref opportunityStatus, value);
		}
		ZString opportunityStatus;

		public ZPropertyInfo OpportunityStatusInfo => GetZPropertyInfo(nameof(OpportunityStatus));

		#endregion

		#region OpportunityStage

		[List("Lookups.OpportunityStageList")]
		public ZString OpportunityStage
		{
			get => opportunityStage;
			set => SetNonPersistentPropertyValue(OpportunityStageInfo, ref opportunityStage, value);
		}
		ZString opportunityStage;

		public ZPropertyInfo OpportunityStageInfo => GetZPropertyInfo(nameof(OpportunityStage));

		#endregion

		#region Source

		[List("Lookups.ActiveSourcesList")]
		public ZString Source
		{
			get => source;
			set => SetNonPersistentPropertyValue(SourceInfo, ref source, value);
		}
		ZString source;

		public ZPropertyInfo SourceInfo => GetZPropertyInfo(nameof(Source));

		#endregion

		#region ActiveSourceDetails

		[List("Lookups.ActiveSourceDetailsList")]
		public ZString ActiveSourceDetails
		{
			get => activeSourceDetails;
			set => SetNonPersistentPropertyValue(ActiveSourceDetailsInfo, ref activeSourceDetails, value);
		}
		ZString activeSourceDetails;

		public ZPropertyInfo ActiveSourceDetailsInfo => GetZPropertyInfo(nameof(ActiveSourceDetails));

		#endregion

		#region SourceDetails

		[List("Lookups.SourceDetailsList")]
		public ZString SourceDetails
		{
			get => sourceDetails;
			set => SetNonPersistentPropertyValue(SourceDetailsInfo, ref sourceDetails, value);
		}
		ZString sourceDetails;

		public ZPropertyInfo SourceDetailsInfo => GetZPropertyInfo(nameof(SourceDetails));

		#endregion

		#region UseCampaignName

		public ZBool UseCampaignName
		{
			get => useCampaignName;
			set => SetNonPersistentPropertyValue(UseCampaignNameInfo, ref useCampaignName, value);
		}
		ZBool useCampaignName;

		public ZPropertyInfo UseCampaignNameInfo => GetZPropertyInfo(nameof(UseCampaignName));

		#endregion

		#region OpportunityAssignment

		[List("Lookups.OpportunityAssignmentList")]
		public ZString OpportunityAssignment
		{
			get => opportunityAssignment;

			set
			{
				SetNonPersistentPropertyValue(OpportunityAssignmentInfo, ref opportunityAssignment, value);

				StaffAssignmentInfo.ClearValue();
				SalesPersonInfo.ClearValue();
			}
		}
		ZString opportunityAssignment;

		public ZPropertyInfo OpportunityAssignmentInfo => GetZPropertyInfo(nameof(OpportunityAssignment));

		#endregion

		#region SalesPerson

		[List("Lookups.SalesPersonList")]
		public ZString SalesPerson
		{
			get => salesPerson;
			set => SetNonPersistentPropertyValue(SalesPersonInfo, ref salesPerson, value);
		}
		ZString salesPerson;

		public ZPropertyInfo SalesPersonInfo => GetZPropertyInfo(nameof(SalesPerson));

		#endregion

		#region StaffAssignment

		[List("Lookups.StaffAssignmentList")]
		public ZString StaffAssignment
		{
			get => staffAssignment;
			set => SetNonPersistentPropertyValue(StaffAssignmentInfo, ref staffAssignment, value);
		}
		ZString staffAssignment;

		public ZPropertyInfo StaffAssignmentInfo => GetZPropertyInfo(nameof(StaffAssignment));

		#endregion

		#region OpportunityNotes

		public ZBlob OpportunityNotes
		{
			get => opportunityNotes;
			set => SetNonPersistentPropertyValue(OpportunityNotesInfo, ref opportunityNotes, value);
		}
		ZBlob opportunityNotes;

		public ZPropertyInfo OpportunityNotesInfo => GetZPropertyInfo(nameof(OpportunityNotes));

		public ZBlob OpportunityNotes_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(OpportunityNotes);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				OpportunityNotes = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}
		#endregion

		#region OverallDispositionDescription

		public ZString OverallDisposition => IsClosed ? OrgOpportunityOverallDispositionList.Codes.Closed : OrgOpportunityOverallDispositionList.Codes.Open;

		public ZString OverallDispositionDescription => Lookups.OverallDispositionList.GetDescriptionFromCode(OverallDisposition);

		#endregion

		#region IsClosed

		public ZBool IsClosed => OrganisationsDataRegistry.Instance.OpportunityStatus.Value.GetBoolFromCode(OpportunityStatus);

		#endregion

		#endregion

		#region Lookups

		public OpportunityCreationTemplateLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = GetNewLookups();
				}

				return fLookups;
			}
		}

		protected virtual OpportunityCreationTemplateLookups GetNewLookups()
		{
			return new OpportunityCreationTemplateLookups(this);
		}

		OpportunityCreationTemplateLookups fLookups;

		#endregion

		public OpportunityCreationTemplateValidation Validation => new OpportunityCreationTemplateValidation(this);
	}
}
