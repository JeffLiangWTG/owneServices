using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileQuestion : AutoRefCusProfileQuestion
	{
		public RefCusProfileQuestion(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(ProfileType))]
		public override ZGuid XQ2_XXX_ProfileType { get => base.XQ2_XXX_ProfileType; set => base.XQ2_XXX_ProfileType = value; }

		public RefCusProfileType ProfileType => Factory.Load<RefCusProfileType>(XQ2_XXX_ProfileType);

		[ChildEditable(true)]
		public RefCusProfileQuestionAnswerListCollection Answers
		{
			get
			{
				if (fAnswers == null)
				{
					fAnswers = new RefCusProfileQuestionAnswerListCollection(this);
					RegisterEditableChildObject(fAnswers);
				}
				return fAnswers;
			}
		}
		RefCusProfileQuestionAnswerListCollection fAnswers;

		[ChildEditable(true)]
		public RefCusProfileQuestionAttributeCollection Attributes
		{
			get
			{
				if (fAttributes == null)
				{
					fAttributes = new RefCusProfileQuestionAttributeCollection(this);
					RegisterEditableChildObject(fAttributes);
				}
				return fAttributes;
			}
		}
		RefCusProfileQuestionAttributeCollection fAttributes;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(RefCusProfileQuestion question)
				: base(question)
			{
			}

			protected new RefCusProfileQuestion BusinessObject
			{
				get { return (RefCusProfileQuestion)base.BusinessObject; }
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(RefCusProfileQuestionAttributeSchema.XQ3_XQ2_Question, BusinessObject.PK);
				Factory.AddFetchHint(RefCusProfileQuestionAnswerListSchema.XQ4_XQ2_Question, BusinessObject.PK);
			}
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public RefCusProfileQuestion[] Load(RefCusProfile[] profiles, ZString dataGrouping, ZDateTime date)
			{
				var profilesWithQuestionCode = profiles.Where(r => !r.XX0_QuestionCode.IsEmpty).ToArray();

				var effectiveDate = date.IsValid ? date.Date : ZDateTime.Today;

				var questionQuery = GetFilter(profilesWithQuestionCode, dataGrouping, effectiveDate);
				questionQuery.FetchOnlyFromLocalCache = true;

				var questions = Factory.Load<RefCusProfileQuestion>(questionQuery).ToList();
				var nonCachedProfiles = profilesWithQuestionCode.Where(p => !questions.Any(q => q.XQ2_XXX_ProfileType == p.XX0_XXX_ProfileType && q.XQ2_Code == p.XX0_QuestionCode)).ToArray();
				if (nonCachedProfiles.Length > 0)
				{
					questions.AddRange(Factory.Load<RefCusProfileQuestion>(GetFilter(nonCachedProfiles, dataGrouping, effectiveDate)));
				}

				return questions.ToArray();
			}

			static ZQuery GetFilter(RefCusProfile[] profiles, ZString dataGrouping, ZDateTime effectiveDate)
			{
				var questionQuery = new ZQuery();

				if (profiles == null || profiles.Length == 0 || dataGrouping.IsEmpty || !effectiveDate.IsValid)
				{
					questionQuery.IsNoResultQuery = true;
				}
				else
				{
					questionQuery.AddToFilter(RefCusProfileQuestionSchema.XQ2_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
					questionQuery.AddToFilter(RefCusProfileQuestionSchema.XQ2_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
					questionQuery.AddToFilter(RefCusProfileQuestionSchema.XQ2_ZZZ_NKDataGrouping, dataGrouping);

					var codesQuery = new ZQuery { DefaultJoinCondition = JoinCondition.Or };
					foreach (var groupByType in profiles.Where(x => !x.XX0_QuestionCode.IsEmpty).GroupBy(x => x.XX0_XXX_ProfileType))
					{
						codesQuery.AddToFilter(new ZQuery(RefCusProfileQuestionSchema.XQ2_XXX_ProfileType, groupByType.Key)
							.AddToFilter(RefCusProfileQuestionSchema.XQ2_Code, groupByType.Select(x => x.XX0_QuestionCode)));
					}
					questionQuery.AddToFilter(codesQuery);
				}

				return questionQuery;
			}

			public RefCusProfileQuestion[] Load(ZString profileType, ZString dataGrouping, ZDateTime date)
			{
				var effectiveDate = date.IsValid ? date.Date : ZDateTime.Today;
				return Factory.Load<RefCusProfileQuestion>(GetFilter(profileType, dataGrouping, effectiveDate));
			}

			static ZQuery GetFilter(ZString profileType, ZString dataGrouping, ZDateTime effectiveDate)
			{
				if (profileType.IsEmpty || dataGrouping.IsEmpty || !effectiveDate.IsValid)
				{
					return ZQuery.NoResultQuery;
				}

				var dbQuery = new ZDBOnlyQuery(typeof(RefCusProfileQuestion));

				var subQuery = new ZDBOnlySubQuery(typeof(RefCusProfileType), RefCusProfileTypeSchema.PK);
				subQuery.AddToFilter(RefCusProfileTypeSchema.XXX_ProfileType, profileType);
				subQuery.AddToFilter(RefCusProfileTypeSchema.XXX_ZZZ_NKDataGrouping, dataGrouping);

				dbQuery.AddSubQuery(RefCusProfileQuestionSchema.XQ2_XXX_ProfileType, subQuery, JoinCondition.And);
				dbQuery.AddToFilter(RefCusProfileQuestionSchema.XQ2_ZZZ_NKDataGrouping, dataGrouping);
				dbQuery.AddToFilter(RefCusProfileQuestionSchema.XQ2_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
				dbQuery.AddToFilter(RefCusProfileQuestionSchema.XQ2_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);

				return dbQuery;
			}

			public RefCusProfileQuestion[] Load(ZString profileType, ZGuid tariffTypePK, ZString tariffCode, ZString dataGrouping, ZDateTime date, bool includingEffectiveInFuture = false
				, IEnumerable<(string Name, string[] Values)> profileAttributes = null
				, IEnumerable<(string Name, string Value)> questionAttributes = null)
			{
				var effectiveDate = date.IsValid ? date.Date : ZDateTime.Today;

				var questionQuery = new ZDBOnlyQuery(typeof(RefCusProfileQuestion));

				var profileTypePK = new RefCusProfileType.Loader(Factory).Load(profileType, tariffTypePK, dataGrouping)?.PK ?? ZGuid.Empty;
				var profileSubQuery = RefCusProfile.Loader.GetFilter(profileTypePK, tariffCode, dataGrouping, effectiveDate, profileAttributes);
				if (!profileSubQuery.IsNoResultQuery)
				{
					var profileQuery = new ZDBOnlySubQuery(typeof(RefCusProfile), RefCusProfileSchema.XX0_QuestionCode);
					profileQuery.AddToFilter(profileSubQuery);

					questionQuery.AddSubQuery(RefCusProfileQuestionSchema.XQ2_Code, profileQuery, JoinCondition.And);
					questionQuery.AddToFilter(GetFilter(profileTypePK, dataGrouping, effectiveDate, includingEffectiveInFuture, questionAttributes));
				}
				else
				{
					questionQuery.IsNoResultQuery = true;
				}

				return Factory.Load<RefCusProfileQuestion>(questionQuery);
			}

			static ZQuery GetFilter(ZGuid profileTypePK, ZString dataGrouping, ZDateTime effectiveDate, bool includingEffectiveInFuture = false, IEnumerable<(string Name, string Value)> attributes = null)
			{
				if (!profileTypePK.IsValid || dataGrouping.IsEmpty || !effectiveDate.IsValid)
				{
					return ZQuery.NoResultQuery;
				}

				var questionQuery = new ZDBOnlyQuery(typeof(RefCusProfileQuestion));
				questionQuery.AddToFilter(RefCusProfileQuestionSchema.XQ2_XXX_ProfileType, profileTypePK);
				questionQuery.AddToFilter(RefCusProfileQuestionSchema.XQ2_ZZZ_NKDataGrouping, dataGrouping);
				questionQuery.AddToFilter(RefCusProfileQuestionSchema.XQ2_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);

				if (!includingEffectiveInFuture)
				{
					questionQuery.AddToFilter(RefCusProfileQuestionSchema.XQ2_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
				}

				if (attributes?.Any() ?? false)
				{
					foreach (var (name, value) in attributes)
					{
						var attributeQuery = new ZDBOnlySubQuery(typeof(RefCusProfileQuestionAttribute), RefCusProfileQuestionAttributeSchema.XQ3_XQ2_Question, RefCusProfileQuestionSchema.PK);
						attributeQuery.AddToFilter(RefCusProfileQuestionAttributeSchema.XQ3_Name, name);
						attributeQuery.AddToFilter(RefCusProfileQuestionAttributeSchema.XQ3_Value, value);
						questionQuery.AddSubQuery(attributeQuery, JoinCondition.And);
					}
				}

				return questionQuery;
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(RefCusProfileQuestion);
		}
	}
}
