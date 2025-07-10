using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	#region VoteExamSurveyQuestionTypeDecider

	public class VoteExamSurveyQuestionTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(VoteExamSurveyQuestion);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = typeof(VoteExamSurveyQuestion);

			ZGuid campaignPK = new ZGuid(row[VoteExamSurveyQuestionSchema.Constants.HY_G0]);
			if (!campaignPK.IsEmpty)
			{
				GlbCompanyCampaign campaign = factory.Load<GlbCompanyCampaign>(campaignPK);
				if (campaign is Enterprise.Integration.Recruiter.ILearningCentreCampaign)
				{
					result = ObjectFactory.GetType<Enterprise.Integration.Recruiter.ILearningCentreQuestion>();
				}
			}

			return result;
		}

		public override Type GetTypeForNew()
		{
			return typeof(VoteExamSurveyQuestion);
		}
	}

	#endregion

	[ProvideMetaDataProperty("ReadOnly", MetaDataTypes.ReadOnly)]
	public class VoteExamSurveyQuestion : AutoVoteExamSurveyQuestion
	{
		public static readonly VoteExamSurveyQuestionTypeDecider TypeDecider = new VoteExamSurveyQuestionTypeDecider();

		#region Constants & Schema

		public static class Constants
		{
			public const byte DefaultMinNumericRange = 1;
			public const byte DefaultMaxNumericRange = 10;
			public const byte DefaultMinSelectionCount = 1;
			public const byte DefaultMaxSelectionCount = 1;
		}

		public new abstract class Schema : AutoVoteExamSurveyQuestion.Schema
		{
			public const string ExamCorrectAnswerAsBool = "ExamCorrectAnswerAsBool";
			public const string IsActiveForBinding = "IsActiveForBinding";
		}

		#endregion

		public VoteExamSurveyQuestion(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			HY_IsActive = true;
			HY_IsRandomisable = true;
		}

		protected bool GetReadOnly(PropertyDescriptor property)
		{
			bool result = (property.Name != Schema.IsActiveForBinding && !HY_IsActive);
			result = result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return result;
		}

		#region IsActiveForBinding

		public ZBool IsActiveForBinding
		{
			get { return HY_IsActive; }
			set
			{
				if (HY_IsActive != value)
				{
					ReOrderQuestionsOnStatusChangeIfApplicable(value);
					if (!IsSubQuestion)
					{
						VoteExamSurveyQuestion[] subQuestions = SubQuestions.ToArray();
						foreach (VoteExamSurveyQuestion subQuestion in subQuestions)
						{
							subQuestion.HY_IsActive = value;
						}
					}
					HY_IsActive = value;
					ResetSubQuestions();
				}
			}
		}

		public ZPropertyInfo IsActiveForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsActiveForBinding), x => HY_IsActiveInfo); }
		}

		protected bool IsActiveForBinding_ReadOnly
		{
			get { return !IsInDatabase; }
		}

		void ReOrderQuestionsOnStatusChangeIfApplicable(bool isActive)
		{
			bool shouldReOrder = !IsSubQuestion || (Campaign != null && Campaign.IsVoteCampaign);
			if (shouldReOrder)
			{
				VoteExamSurveyQuestionSet questionSet = GetQuestionSet();
				if (questionSet != null)
				{
					ZShort nextOrder = questionSet.ActiveQuestions.GetNextQuestionOrder();
					if (isActive)
					{
						ActualOrderInnerInfo.Value = nextOrder;
					}
					else
					{
						ActualOrder = nextOrder;
						ZShort highestInactiveQuestionOrder = FindHighestInactiveQuestionOrder(questionSet);
						ActualOrderInnerInfo.Value = new ZShort(highestInactiveQuestionOrder + 1);
					}
				}
			}
		}

		ZShort FindHighestInactiveQuestionOrder(VoteExamSurveyQuestionSet questionSet)
		{
			short result = short.MinValue;

			foreach (VoteExamSurveyQuestion question in questionSet.InactiveQuestions)
			{
				ZShort order = (ZShort)question[questionSet.SchemaOrderColumn];
				if (order > result)
				{
					result = order;
				}
			}

			return result;
		}

		#endregion

		#region Question / Sub-Question ordering

		#region ActualOrder

		public ZShort ActualOrder
		{
			get { return (delayedOrderChanging != null) ? tempOrderWhileReOrdering : (ZShort)ActualOrderInnerInfo.Value; }
			set { SetActualOrder(value); }
		}

		public ZString ActualOrderForBinding
		{
			get
			{
				ZShort actualOrder = ActualOrder;
				return (actualOrder < 1) ? "" : actualOrder.ToString();
			}
		}

		public ZPropertyInfo ActualOrderForBindingInfo
		{
			get { return GetZPropertyInfo(nameof(ActualOrderForBinding)); }
		}

		void SetActualOrder(ZShort value)
		{
			SetActualOrder(value, true);
		}

		void SetActualOrder(ZShort value, bool ensureValidNewOrder)
		{
			ZShort currentOrder = (ZShort)ActualOrderInnerInfo.Value;
			if (currentOrder != value)
			{
				VoteExamSurveyQuestionSet questionSet = GetQuestionSet();
				if (questionSet != null)
				{
					if (((INeedRow)this).Row.RowState == DataRowState.Detached)
					{
						delayedOrderChanging = delegate
						{
							HandleOrderChanging(questionSet, value, currentOrder, ensureValidNewOrder);
						};
						tempOrderWhileReOrdering = questionSet.GetValidQuestionOrder(value);
					}
					else
					{
						HandleOrderChanging(questionSet, value, currentOrder, ensureValidNewOrder);
					}
				}
				else
				{
					ActualOrderInnerInfo.Value = value;
				}
			}
		}

		public ZPropertyInfo ActualOrderInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ActualOrder), x => ActualOrderInnerInfo); }
		}

		ZPropertyInfo ActualOrderInnerInfo
		{
			get { return (IsSubQuestion) ? HY_SubQuestionOrderInfo : HY_QuestionOrderInfo; }
		}

		public void RunDelayedHandleOrderChanging()
		{
			if (delayedOrderChanging != null)
			{
				delayedOrderChanging();
				delayedOrderChanging = null;
			}
		}

		void HandleOrderChanging(VoteExamSurveyQuestionSet questionSet, ZShort newOrder, ZShort currentOrder, bool ensureValidNewOrder)
		{
			ActualOrderInnerInfo.Value = (ZShort)unusedQuestionOrder;
			questionSet.HandleOrderChanging(this, newOrder, currentOrder);
			ActualOrderInnerInfo.Value = (ensureValidNewOrder) ? questionSet.GetValidQuestionOrder(newOrder) : newOrder;
		}

		public const short unusedQuestionOrder = short.MinValue;
		delegate void DelayedOrderChangingDelegate();
		DelayedOrderChangingDelegate delayedOrderChanging;
		ZShort tempOrderWhileReOrdering;

		#endregion

		public override ZShort HY_QuestionOrder
		{
			get { return base.HY_QuestionOrder; }
			set
			{
				if (base.HY_QuestionOrder != value)
				{
					if (!isSettingSubQuestionsOrderSuspended && !IsSubQuestion && Campaign != null)
					{
						using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
						{
							VoteExamSurveySubQuestionCollection subQuestions = SubQuestions;
							VoteExamSurveyQuestion[] subQuestionsAsArray = SubQuestions.ToArray();
							((IActiveBusinessObjectCollection)subQuestions).Refresh();
							base.HY_QuestionOrder = value;
							foreach (VoteExamSurveyQuestion subQuestion in subQuestionsAsArray)
							{
								subQuestion.HY_QuestionOrder = value;
							}
						}
					}
					else
					{
						base.HY_QuestionOrder = value;
					}
				}
			}
		}

		public void SuspendSettingSubQuestionsOrder()
		{
			isSettingSubQuestionsOrderSuspended = true;
		}

		public void ResumeSettingSubQuestionsOrder()
		{
			isSettingSubQuestionsOrderSuspended = false;
		}

		bool isSettingSubQuestionsOrderSuspended;

		#endregion

		#region Voting

		[ReadOnlyMember(nameof(ShouldRankVotingNominees_ReadOnly))]
		public ZBool ShouldRankVotingNominees
		{
			get { return HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.RankedVote; }
			set
			{
				HY_AnswerType = (value)
					? VoteExamSurveyAnswerTypeList.Codes.RankedVote
					: VoteExamSurveyAnswerTypeList.Codes.UnrankedVote;
				ShouldRankVotingNomineesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ShouldRankVotingNomineesInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldRankVotingNominees)); }
		}

		protected bool ShouldRankVotingNominees_ReadOnly
		{
			get { return Campaign != null && Campaign.CampaignsItemsSent.Count > 0; }
		}

		public bool IsVotingItem
		{
			get { return HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.VotingItem || (Campaign != null && Campaign.IsVoteCampaign && IsSubQuestion); }
		}

		public bool IsVotingHeader
		{
			get { return HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.RankedVote || HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.UnrankedVote; }
		}

		#endregion

		#region Survey / Exam

		public ZBool IsNonNumericExamAnswerType
		{
			get { return HY_AnswerType != VoteExamSurveyAnswerTypeList.Codes.NumericScale && HY_AnswerType != VoteExamSurveyAnswerTypeList.Codes.Percentage; }
		}

		public ZPropertyInfo IsNonNumericExamAnswerTypeInfo
		{
			get { return GetZPropertyInfo(nameof(IsNonNumericExamAnswerType)); }
		}

		public ZBool IsMultipleChoiceQuestion
		{
			get { return HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.MultipleChoice; }
		}

		public ZPropertyInfo IsMultipleChoiceQuestionInfo
		{
			get { return GetZPropertyInfo(nameof(IsMultipleChoiceQuestion)); }
		}

		public ZString MinMaxValueCaption
		{
			get
			{
				return (HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.MultipleChoice)
					? Res.GetString("65d3237d-e233-4210-abe4-d5dc3377dcab", "Selection Count:")
					: Res.GetString("fe0beb97-9471-40bb-92bd-ba8c63184a83", "Number Range:");
			}
		}

		public ZPropertyInfo MinMaxValueCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(MinMaxValueCaption)); }
		}

		public ZBool ShouldSpecifyMinMaxValue
		{
			get
			{
				return
					HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.MultipleChoice ||
					HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.NumericScale;
			}
		}

		public ZPropertyInfo ShouldSpecifyMinMaxValueInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldSpecifyMinMaxValue)); }
		}

		#endregion

		public override ZByte HY_Min
		{
			get { return base.HY_Min; }
			set
			{
				if (base.HY_Min != value)
				{
					base.HY_Min = value;
					Validation.ValidateHY_Max();
				}
			}
		}

		public override ZByte HY_Max
		{
			get { return base.HY_Max; }
			set
			{
				if (base.HY_Max != value)
				{
					base.HY_Max = value;
					Validation.ValidateHY_Min();
				}
			}
		}

		public ZString QuestionTextForWeb => WebUtility.HtmlEncode(HY_QuestionLocalized).Replace("\r\n", "<br/>").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");

		public ZPropertyInfo QuestionTextForWebInfo
		{
			get { return GetZPropertyInfo(nameof(QuestionTextForWeb)); }
		}

		public void SetQuestionTextAndEncode(string value)
		{
			HY_Question = WebUtility.HtmlEncode(value);
		}

		#region Delete

		protected override void BeforeSuccessfulDelete()
		{
			if (Campaign != null)
			{
				if (!IsDeleted && !IsRefreshingByDataRefreshBus && !Campaign.IsDeletingRelatedBusinessObjects)
				{
					VoteExamSurveyQuestionSet questionSet = GetQuestionSet();
					if (questionSet != null)
					{
						ActualOrder = questionSet.ActiveQuestions.GetMaxQuestionOrder();
					}
				}

				if (!IsSubQuestion || subQuestions != null)
				{
					foreach (VoteExamSurveyQuestion subQuestion in SubQuestions.ToArray())
					{
						SubQuestions.RemoveFromRelationship(subQuestion);
						subQuestion.Delete();
					}
				}
			}

			base.BeforeSuccessfulDelete();
		}

		#endregion

		#region Sub Question

		public bool IsSubQuestion
		{
			get { return HY_SubQuestionOrder != 0; }
		}

		public override ZShort HY_SubQuestionOrder
		{
			get { return base.HY_SubQuestionOrder; }
			set
			{
				if (base.HY_SubQuestionOrder != value)
				{
					using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
					{
						ActiveBusinessObjectCollection<VoteExamSurveyQuestion>.RefreshAll(Factory);
						base.HY_SubQuestionOrder = value;
					}

					if (value > 0)
					{
						ResetSubQuestions();
					}
				}
			}
		}

		[ChildEditable(true)]
		public VoteExamSurveySubQuestionCollection SubQuestions
		{
			get
			{
				if (!IsSubQuestion && subQuestions == null)
				{
					subQuestions = GetNewSubQuestions(this, HY_IsActive);
					RegisterEditableChildObject(subQuestions);
				}
				return subQuestions;
			}
		}

		public VoteExamSurveySubQuestionCollection InactiveSubQuestions
		{
			get { return (!IsSubQuestion) ? GetNewSubQuestions(this, false) : null; }
		}

		protected virtual VoteExamSurveySubQuestionCollection GetNewSubQuestions(VoteExamSurveyQuestion question, bool isActive)
		{
			return new VoteExamSurveySubQuestionCollection(question, isActive);
		}

		void ResetSubQuestions()
		{
			if (subQuestions != null)
			{
				UnRegisterEditableChildObject(SubQuestions);
				subQuestions = null;
			}
		}

		VoteExamSurveySubQuestionCollection subQuestions;

		void DetachAndDeleteSubQuestions()
		{
			if (SubQuestions != null)
			{
				using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
				{
					foreach (VoteExamSurveyQuestion subQuestion in SubQuestions.ToArray())
					{
						subQuestion.HY_G0 = ZGuid.Empty;
						subQuestion.Delete();
					}
				}
			}
		}

		#endregion

		protected bool HY_IsOptional_ReadOnly
		{
			get { return IsHeader; }
		}

		public GlbCompanyCampaign Campaign
		{
			get { return Factory.Load<GlbCompanyCampaign>(HY_G0); }
		}

		[RelatedBusinessObject("Campaign")]
		public override ZGuid HY_G0
		{
			get { return base.HY_G0; }
			set { base.HY_G0 = value; }
		}

		#region Answer Type

		[ReadOnlyMember(nameof(IsInDatabase))]
		[List("Lookups.AnswerTypes")]
		public override ZString HY_AnswerType
		{
			get { return base.HY_AnswerType; }
			set
			{
				if (base.HY_AnswerType != value)
				{
					HY_ExamCorrectAnswer = "";

					HandleHandleAnswerTypeChanging(value);
					base.HY_AnswerType = value;

					if (IsMultipleChoiceQuestion)
					{
						HY_Min = Constants.DefaultMinSelectionCount;
						HY_Max = Constants.DefaultMaxSelectionCount;
					}
					else if (HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.NumericScale)
					{
						HY_Min = Constants.DefaultMinNumericRange;
						HY_Max = Constants.DefaultMaxNumericRange;
					}
				}
			}
		}

		void HandleHandleAnswerTypeChanging(ZString newValue)
		{
			if (IsMultipleChoiceQuestion && newValue != VoteExamSurveyAnswerTypeList.Codes.MultipleChoice)
			{
				DetachAndDeleteSubQuestions();
			}

			if (IsHeader != (newValue == VoteExamSurveyAnswerTypeList.Codes.Header))
			{
				VoteExamSurveyQuestionSet questionSet = GetQuestionSet();
				if (questionSet != null)
				{
					questionSet.HandleAnswerTypeChanging(this, newValue);
				}
			}
		}

		#endregion

		public ZBool IsHeader
		{
			get { return HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.Header; }
		}

		public ZPropertyInfo IsHeaderInfo
		{
			get { return GetZPropertyInfo(nameof(IsHeader)); }
		}

		[ReadOnlyMember(nameof(IsHeader))]
		public override ZString HY_RN_NKCountryCode
		{
			get { return base.HY_RN_NKCountryCode; }
			set { base.HY_RN_NKCountryCode = value; }
		}

		[ReadOnlyMember(nameof(HY_IsRandomisable_ReadOnly))]
		public override ZBool HY_IsRandomisable
		{
			get { return base.HY_IsRandomisable; }
			set { base.HY_IsRandomisable = value; }
		}

		protected bool HY_IsRandomisable_ReadOnly
		{
			get { return !IsMultipleChoiceQuestion; }
		}

		[ReadOnlyMember(nameof(IsHeader))]
		public override ZString HY_QuestionCategory
		{
			get { return base.HY_QuestionCategory; }
			set { base.HY_QuestionCategory = value; }
		}

		[VoteExamSurveyQuestionTranslatableDataField(Schema.TableName, Schema.HY_Question, Schema.HY_QuestionMaxLength, Schema.HY_Question, Type = typeof(VoteExamSurveyQuestion), Asmid = ResString.AssemblyId)]
		public override ZString HY_Question
		{
			get { return base.HY_Question; }
			set
			{
				base.HY_Question = value;
				if (!IsSubQuestion && Campaign != null)
				{
					SubQuestions.RefreshBinding();
				}
			}
		}

		[MaxLength(Schema.HY_QuestionMaxLength)]
		public MultilingualString HY_QuestionMultilingual
		{
			get { return GetMultilingual(HY_QuestionInfo); }
			set
			{
				HY_Question = value;
				HY_QuestionMultilingualInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo HY_QuestionMultilingualInfo
		{
			get { return GetZPropertyInfo(nameof(HY_QuestionMultilingual)); }
		}

		public ZString HY_QuestionLocalized
		{
			get { return (ZString)HY_QuestionMultilingual.GetLocalizedValue(Res.CurrentLanguage); }
		}

		public VoteExamSurveyQuestion GetParentQuestion()
		{
			VoteExamSurveyQuestion result = null;
			if (IsSubQuestion)
			{
				ZQuery query = new ZQuery(VoteExamSurveyQuestionSchema.HY_SubQuestionOrder, ZShort.Zero);
				query.AddToFilter(VoteExamSurveyQuestionSchema.HY_QuestionOrder, HY_QuestionOrder);
				query.AddToFilter(VoteExamSurveyQuestionSchema.HY_G0, HY_G0);
				query.AddToFilter(VoteExamSurveyQuestionSchema.HY_AnswerType, SQLComparisonOperator.NotEqual, VoteExamSurveyAnswerTypeList.Codes.Header);
				result = Factory.LoadTop1<VoteExamSurveyQuestion>(query);
			}
			return result;
		}

		public VoteExamSurveyQuestionSet GetQuestionSet()
		{
			VoteExamSurveyQuestionSet result = null;

			if (IsSubQuestion)
			{
				VoteExamSurveyQuestion parentQuestion = GetParentQuestion();
				result = (parentQuestion != null) ? parentQuestion.SubQuestions : null;
			}
			else if (Campaign != null)
			{
				result = Campaign.Questions;
			}

			return result;
		}

		public VoteExamSurveySubmittedAnswerCollection SubmittedAnswers
		{
			get
			{
				if (submittedAnswers == null)
				{
					submittedAnswers = GetNewSubmittedAnswerCollection();
					submittedAnswers.Load();
				}
				return submittedAnswers;
			}
		}

		protected virtual VoteExamSurveySubmittedAnswerCollection GetNewSubmittedAnswerCollection()
		{
			return new VoteExamSurveySubmittedAnswerCollection(this);
		}

		VoteExamSurveySubmittedAnswerCollection submittedAnswers;

		public bool IsDeleteAllowed => HY_G0.IsEmpty || Campaign.IsDeletingRelatedBusinessObjects || !IsInDatabase || ForceAllowDeleteForTest;

		protected virtual bool ForceAllowDeleteForTest => false;

		public override void OnSaving()
		{
			if (Campaign.IsInDatabase && (Campaign.IsSurveyCampaign || Campaign.IsVoteCampaign || Campaign.IsExamCampaign))
			{
				Campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().ForEach(x => x.G8_Stage = GlbCompanyCampaignItemLookups.StagesConstants.Reset);
			}

			base.OnSaving();
		}
	}

	public class VoteExamSurveyQuestionDeleteChecker : DeleteChecker
	{
		public override DeleteDetails DeleteDetails(BusinessObject businessObjectToBeDeleted)
		{
			VoteExamSurveyQuestion question = (VoteExamSurveyQuestion)businessObjectToBeDeleted;
			return question.IsDeleteAllowed
				? new DeleteDetails.Allow()
				: new DeleteDetails.Disallow(Res.GetString("b33c7755-833f-4436-826b-555276473f07",
					"Cannot delete an existing question, mark as inactive instead."));
		}
	}
}
