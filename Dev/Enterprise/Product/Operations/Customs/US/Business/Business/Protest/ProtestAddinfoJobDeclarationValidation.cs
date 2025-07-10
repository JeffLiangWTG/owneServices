using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Protest
{
	public class ProtestAddInfoJobDeclarationValidation : AddInfoJobDeclarationValidation
	{
		public ProtestAddInfoJobDeclarationValidation(AddInfoJobDeclaration addInfo)
			: base(addInfo)
		{
			if (!Declaration.IsProtest)
			{
				throw new ArgumentException("Declaration is not a Protest");
			}
		}

		Protest Protest
		{
			get { return ((JobDeclaration)Parent.Parent).Protest; }
		}

		protected override void CheckUS_P_ApplicationFurtherReview()
		{
			base.CheckUS_P_ApplicationFurtherReview();
			if (!Is514Protest && Parent.US_P_ApplicationFurtherReview)
			{
				Parent.US_P_ApplicationFurtherReviewInfo.AddMessageError(ApplicationFurtherReview);
			}
			ValidateUS_P_ApplicationQuestion1();
			ValidateUS_P_ApplicationQuestion2();
			ValidateUS_P_ApplicationQuestion3();
		}
		internal const string ApplicationFurtherReview = "Only 514 Protests are eligible for further review.";

		protected override void CheckUS_P_AcceleratedDispositionInd()
		{
			base.CheckUS_P_AcceleratedDispositionInd();

			if (Parent.US_P_AcceleratedDispositionInd && !Is514Protest)
			{
				Parent.US_P_AcceleratedDispositionIndInfo.AddMessageError(AcceleratedDispositionOnlyFor514);
			}
		}
		internal const string AcceleratedDispositionOnlyFor514 = "Only 514 protests are eligible for accelerated disposition.";

		protected override void CheckUS_P_FaxSent()
		{
			base.CheckUS_P_FaxSent();
			ValidateUS_P_FaxSentDate();
		}

		protected override void CheckUS_P_FaxSentDate()
		{
			base.CheckUS_P_FaxSentDate();
			if (Parent.US_P_FaxSentDate.IsEmpty)
			{
				if (Parent.US_P_FaxSent)
				{
					Parent.US_P_FaxSentDateInfo.AddMessageError(FaxSentDateRequired);
				}
			}
			else if (!Parent.US_P_FaxSent)
			{
				Parent.US_P_FaxSentDateInfo.AddMessageError(FaxSentDateNotRequired);
			}
		}
		internal const string FaxSentDateRequired = "FAX Sent Date is mandatory if FAX Sent Indicator is ticked.";
		internal const string FaxSentDateNotRequired = "FAX Sent Date is not required when FAX Sent Indicator is not selected.";

		protected override void CheckUS_P_HardCopySent()
		{
			base.CheckUS_P_HardCopySent();
			var block11 = Protest.GetP11BlockFromLatestOriginalAcceptedMessage();
			if (block11 != null)
			{
				if (block11.HardcopyIndicator == SampleHardcopyIndicatorsList.Codes.Sent && !Parent.US_P_HardCopySent)
				{
					Parent.US_P_HardCopySentInfo.AddMessageError(IndicatorIsDifferent);
				}
			}
		}
		internal const string IndicatorIsDifferent = "This indicator was set to 'S'(Sent) in initial filing transaction and cannot be changed to 'N'.";

		protected override void CheckUS_P_SampleSent()
		{
			base.CheckUS_P_SampleSent();
			var block11 = Protest.GetP11BlockFromLatestOriginalAcceptedMessage();
			if (block11 != null)
			{
				if (block11.SampleIndicator == SampleHardcopyIndicatorsList.Codes.Sent && !Parent.US_P_SampleSent)
				{
					Parent.US_P_SampleSentInfo.AddMessageError(IndicatorIsDifferent);
				}
			}
		}

		protected override void CheckUS_P_AddressTeam()
		{
			base.CheckUS_P_AddressTeam();
			if (Parent.US_P_AddressTeam.IsEmpty)
			{
				Parent.US_P_AddressTeamInfo.AddMessageError(AddressTeamRequired);
			}
		}
		internal const string AddressTeamRequired = "Address Team is required for Protest.";

		protected override void CheckUS_P_Assoc514ProtestNo()
		{
			base.CheckUS_P_Assoc514ProtestNo();
			if (Is181115Intervention && Parent.US_P_Assoc514ProtestNo.IsEmpty)
			{
				Parent.US_P_Assoc514ProtestNoInfo.AddMessageError(Associated514NumberRequiredWhen181);
			}

			if (!Parent.US_P_Assoc514ProtestNo.IsEmpty && !Parent.US_P_Assoc514ProtestNo.IsNumbersOnlyOrEmpty)
			{
				Parent.US_P_Assoc514ProtestNoInfo.AddMessageError(Associated514NumberFormat);
			}
		}
		internal const string Associated514NumberRequiredWhen181 = "Associated 514 Protest Number is required when filling a 181.115 intervention.";
		internal const string Associated514NumberFormat = "Invalid Associated 514 Protest Number format. Number should be in the following format: 12 numerics. Only numerics will be send in the protest messages.";

		protected override void CheckUS_P_FilingDDPP()
		{
			base.CheckUS_P_FilingDDPP();

			if (Protest.US_P_FilingDDPP.IsEmpty)
			{
				Protest.US_P_FilingDDPPInfo.AddMessageError(FilingDDPPRequired);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_P_FilingDDPPInfo, Protest.Lookups.SchDPortList);
			}
		}
		internal const string FilingDDPPRequired = "Filing District Port is mandatory. Message will be rejected by Customs.";

		protected override void CheckUS_P_PeriodBaseDate()
		{
			base.CheckUS_P_PeriodBaseDate();

			var isDateRequired = Is514NAFTA ||
				(!Is181115Intervention && Protest.HasEntriesNotLiquidated);

			if (isDateRequired && Parent.US_P_PeriodBaseDate.IsEmpty)
			{
				Parent.US_P_PeriodBaseDateInfo.AddMessageError(PeriodBaseDateRequired);
			}

			if (!isDateRequired && !Parent.US_P_PeriodBaseDate.IsEmpty)
			{
				Parent.US_P_PeriodBaseDateInfo.AddMessageError(PeriodBaseDateNotRequired);
			}

			ValidateUS_P_PeriodBaseDateQualifier();
		}
		internal const string PeriodBaseDateNotRequired = "Period Base Date is not applicable.";
		internal const string PeriodBaseDateRequired = "Period Base Date is required.";

		protected override void CheckUS_P_PeriodBaseDateQualifier()
		{
			base.CheckUS_P_PeriodBaseDateQualifier();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_P_PeriodBaseDateQualifierInfo, Protest.Lookups.ProtestPeriodBaseDateQualifiers, (NoResString)PeriodBaseDateQualifierList);

			var isQualifierRequired = !Parent.US_P_PeriodBaseDate.IsEmpty && !Is181115Intervention;

			if (isQualifierRequired && Parent.US_P_PeriodBaseDateQualifier.IsEmpty)
			{
				Parent.US_P_PeriodBaseDateQualifierInfo.AddMessageError(PeriodBaseDateQualifierRequired);
			}

			if (!isQualifierRequired && !Parent.US_P_PeriodBaseDateQualifier.IsEmpty)
			{
				Parent.US_P_PeriodBaseDateQualifierInfo.AddMessageError(PeriodBaseDateQualifierNotRequired);
			}
		}
		internal const string PeriodBaseDateQualifierList = "Period Base Date Qualifier is not in the list.";
		internal const string PeriodBaseDateQualifierNotRequired = "Period Base Date Qualifier is not applicable if Period Base Date is empty or 181.115 Intervention.";
		internal const string PeriodBaseDateQualifierRequired = "Period Base Date Qualifier is required.";

		protected override void CheckUS_P_ApplicationQuestion1()
		{
			base.CheckUS_P_ApplicationQuestion1();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_P_ApplicationQuestion1Info, Protest.Lookups.FurtherReviewAnswers, (NoResString)string.Format(ApplicationQuestionList, "1"));

			if (!Parent.US_P_ApplicationQuestion1.IsEmpty)
			{
				CheckApplicationQuestions(Parent.US_P_ApplicationQuestion1Info);
			}
			else if (Parent.US_P_ApplicationFurtherReview && Is514Protest)
			{
				Parent.US_P_ApplicationQuestion1Info.AddMessageError(ApplicationQuestionRequired);
			}
		}
		internal const string ApplicationQuestionList = "Further Review Application Question #{0} is not in the list.";
		internal const string ApplicationQuestionRequired = "Application Questions should be answered when Application Further Review selected.";
		internal const string OnlyFor514WithFurtherReview = "Further review questions are required only for 514 Protests and if Further Review is requested.";

		void CheckApplicationQuestions(ZPropertyInfo info)
		{
			if (!Is514Protest || !Parent.US_P_ApplicationFurtherReview)
			{
				info.AddMessageError(OnlyFor514WithFurtherReview);
			}
		}

		protected override void CheckUS_P_ApplicationQuestion2()
		{
			base.CheckUS_P_ApplicationQuestion2();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_P_ApplicationQuestion2Info, Protest.Lookups.FurtherReviewAnswers, (NoResString)string.Format(ApplicationQuestionList, "2"));

			if (!Parent.US_P_ApplicationQuestion2.IsEmpty)
			{
				CheckApplicationQuestions(Parent.US_P_ApplicationQuestion2Info);
			}
			else if (Parent.US_P_ApplicationFurtherReview && Is514Protest)
			{
				Parent.US_P_ApplicationQuestion2Info.AddMessageError(ApplicationQuestionRequired);
			}
		}

		protected override void CheckUS_P_ApplicationQuestion3()
		{
			base.CheckUS_P_ApplicationQuestion3();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_P_ApplicationQuestion3Info, Protest.Lookups.FurtherReviewAnswers, (NoResString)string.Format(ApplicationQuestionList, "3"));

			if (!Parent.US_P_ApplicationQuestion3.IsEmpty)
			{
				CheckApplicationQuestions(Parent.US_P_ApplicationQuestion3Info);
			}
			else if (Parent.US_P_ApplicationFurtherReview && Is514Protest)
			{
				Parent.US_P_ApplicationQuestion3Info.AddMessageError(ApplicationQuestionRequired);
			}
		}

		protected override void CheckUS_P_InternalAdviceNo()
		{
			base.CheckUS_P_InternalAdviceNo();
			if (!Is514Protest && !Parent.US_P_InternalAdviceNo.IsEmpty)
			{
				Parent.US_P_InternalAdviceNoInfo.AddMessageError(OnlyFor514Protest);
			}
		}

		protected override void CheckUS_P_TestSummonsNo()
		{
			base.CheckUS_P_TestSummonsNo();
			if (!Parent.US_P_TestSummonsNo.IsEmpty)
			{
				if (!Is514Protest)
				{
					Parent.US_P_TestSummonsNoInfo.AddMessageError(OnlyFor514Protest);
				}

				if (!Parent.US_P_TestSummonsNo.IsNumbersOnlyOrEmpty)
				{
					Parent.US_P_TestSummonsNoInfo.AddMessageError(SummonNoFormat);
				}
			}
		}
		internal const string OnlyFor514Protest = "Only applicable to 514 Protests.";
		internal const string SummonNoFormat = "Invalid Test Summons Number format: should be only numerics.";

		protected override void CheckUS_P_ProtestantType()
		{
			base.CheckUS_P_ProtestantType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_P_ProtestantTypeInfo, Protest.Lookups.ProtestantTypes, (NoResString)InvalidProtestantType);

			if (Parent.US_P_ProtestantType.IsEmpty)
			{
				Parent.US_P_ProtestantTypeInfo.AddMessageError(EmptyProtestantType);
			}
			else
			{
				var protestant = Protest.Protestant;
				var isForeignType = Parent.US_P_ProtestantType == ProtestantTypeList.Codes.ForeignExporterProducer;
				var section181 = Protest.TariffActCitation == TariffActCitationList.Codes.D_Section181 && protestant != null &&
					(protestant.E2_RN_NKCountryCode == Core.Constants.CountryCodes.Mexico || protestant.E2_RN_NKCountryCode == Core.Constants.CountryCodes.Canada);

				var section520d = Protest.TariffActCitation == TariffActCitationList.Codes.C_Section520d && protestant != null &&
					protestant.E2_RN_NKCountryCode == Core.Constants.CountryCodes.Chile;

				var isInvalidProtestantType = isForeignType && !section520d && !section181;
				if (isInvalidProtestantType)
				{
					Parent.US_P_ProtestantTypeInfo.AddMessageError(ProtestantTypeFIsInvalid);
				}

				var lastAcceptedInitialFilingMessage = Protest.LastAcceptedInitialFilingMessage;
				if (lastAcceptedInitialFilingMessage != null)
				{
					var block30 = lastAcceptedInitialFilingMessage.MessageBlock.MessageBlocks.OfType<PROP30PJPJ>().FirstOrDefault();
					if (block30 != null)
					{
						if (Parent.US_P_ProtestantType != block30.ProtestantType)
						{
							Parent.US_P_ProtestantTypeInfo.AddMessageError(ProtestantTypeIsDifferent);
						}
					}
				}
			}
		}
		internal const string ProtestantTypeIsDifferent = "Protestant Type is different from type sent in Initial Filing transaction.";
		internal const string InvalidProtestantType = "Invalid value for Protestant Type.";
		internal const string EmptyProtestantType = "Protestant Type is a required field.";
		internal const string ProtestantTypeFIsInvalid = "Protestant Type 'F' is only valid in the case of 181.115 interventions for Protestant Countries 'MX' and 'CA' or in the case if 520(d) Petition for Protestant Country 'CL'.";

		protected override void CheckUS_P_RefundCOPartyType()
		{
			base.CheckUS_P_RefundCOPartyType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_P_RefundCOPartyTypeInfo, Protest.Lookups.ProtestRefundCOPartyTypes, (NoResString)RefundPartyTypeList);
		}
		internal const string RefundPartyTypeList = "Refund C/O Party Type is not in the list.";

		protected override void CheckUS_P_ProtestedDecision()
		{
			base.CheckUS_P_ProtestedDecision();
			if (Parent.US_P_ProtestedDecision.IsEmpty)
			{
				Parent.US_P_ProtestedDecisionInfo.AddMessageError(ProtestedDecisionRequired);
			}
		}
		internal const string ProtestedDecisionRequired = "Protested Decision is a mandatory field.";

		protected override void CheckUS_P_LeadProtestNo()
		{
			base.CheckUS_P_LeadProtestNo();

			if (!Parent.US_P_LeadProtestNo.IsEmpty && !Parent.US_P_LeadProtestNo.IsNumbersOnlyOrEmpty)
			{
				Parent.US_P_LeadProtestNoInfo.AddMessageError(LeadProtestNoFormat);
			}
		}
		internal const string LeadProtestNoFormat = "Invalid Lead Protest Number format: should be only numerics.";

		protected override void CheckUS_P_MerchandiseDesc()
		{
			base.CheckUS_P_MerchandiseDesc();
			if (Parent.US_P_MerchandiseDesc.IsEmpty)
			{
				Parent.US_P_MerchandiseDescInfo.AddMessageError(MerchandiseDescRequired);
			}
		}
		internal const string MerchandiseDescRequired = "Merchandise Description is a mandatory field.";

		protected override void CheckUS_P_SubstituteDDPP()
		{
			base.CheckUS_P_SubstituteDDPP();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_P_SubstituteDDPPInfo, Protest.Lookups.SchDPortList);
		}

		bool Is514Protest
		{
			get { return Protest.TariffActCitation == TariffActCitationList.Codes.A_Section514; }
		}

		bool Is514NAFTA
		{
			get { return Is514Protest && Protest.US_P_PeriodBaseDateQualifier == ProtestPeriodBaseDateQualifierList.Codes.B_CountryOfOrigin; }
		}

		bool Is181115Intervention
		{
			get { return Protest.Is181115Intervention; }
		}
	}
}
