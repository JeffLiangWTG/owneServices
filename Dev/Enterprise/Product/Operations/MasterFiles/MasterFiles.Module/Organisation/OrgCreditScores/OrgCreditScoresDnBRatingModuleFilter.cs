using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgCreditScoresDnBRatingModuleFilter : ModuleTextFilter
	{
		#region Construction

		public OrgCreditScoresDnBRatingModuleFilter(ZString description)
			: base(description, (comparisonOperator, text) => new ZQuery())
		{
		}

		#endregion

		#region Properties

		[List("FinancialStrengthList")]
		[MaxLength(3)]
		public ZString FinancialStrength
		{
			get { return financialStrength; }
			set
			{
				if (value != financialStrength)
				{
					InvalidateCachedQuery();
					if (SetNonPersistentPropertyValue(FinancialStrengthInfo, ref financialStrength, value))
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateFinancialStrength();
						}
						FinancialStrengthInfo.RefreshBinding();
					}
				}
			}
		}
		ZString financialStrength;

		public ZPropertyInfo FinancialStrengthInfo => GetZPropertyInfo(Schema.FinancialStrength);

		public CodeDescriptionPairList FinancialStrengthList => financialStrengthList ?? (financialStrengthList = new FinancialStrengthList());
		CodeDescriptionPairList financialStrengthList;

		[List("CreditAppraisalList")]
		[MaxLength(3)]
		public ZString CreditAppraisal
		{
			get { return creditAppraisal; }
			set
			{
				if (value != creditAppraisal)
				{
					InvalidateCachedQuery();
					if (SetNonPersistentPropertyValue(CreditAppraisalInfo, ref creditAppraisal, value))
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateCreditAppraisal();
						}
						CreditAppraisalInfo.RefreshBinding();
					}
				}
			}
		}
		ZString creditAppraisal;

		public ZPropertyInfo CreditAppraisalInfo => GetZPropertyInfo(Schema.CreditAppraisal);

		public CodeDescriptionPairList CreditAppraisalList => creditAppraisalList ?? (creditAppraisalList = new CreditAppraisalList());
		CodeDescriptionPairList creditAppraisalList;

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			var query = new ZQuery();

			if (!IsEmpty)
			{
				if (!FinancialStrength.IsEmpty && !CreditAppraisal.IsEmpty)
				{
					query.AddToFilter(OrgMiscServSchema.OM_CCCreditRating, FinancialStrength + CreditAppraisal);
				}
				else if (!FinancialStrength.IsEmpty)
				{
					query.AddToFilter(OrgMiscServSchema.OM_CCCreditRating, SQLComparisonOperator.Like, FinancialStrength + "_");
				}
				else if (!CreditAppraisal.IsEmpty)
				{
					query.AddToFilter(OrgMiscServSchema.OM_CCCreditRating, SQLComparisonOperator.EndsWith, CreditAppraisal);
				}
			}

			return query;
		}

		#endregion

		#region Validation

		public new OrgCreditScoresDnBRatingModuleFilterValidation Validation => (OrgCreditScoresDnBRatingModuleFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation() => new OrgCreditScoresDnBRatingModuleFilterValidation(this);

		#endregion

		#region Schema

		protected static class Schema
		{
			public const string FinancialStrength = nameof(FinancialStrength);
			public const string CreditAppraisal = nameof(CreditAppraisal);
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString(Schema.FinancialStrength, FinancialStrength);
			writer.WriteElementString(Schema.CreditAppraisal, CreditAppraisal);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			FinancialStrength = reader.ReadElementString(Schema.FinancialStrength);
			CreditAppraisal = reader.ReadElementString(Schema.CreditAppraisal);
		}

		#endregion

		#region Empty

		protected override bool IsEmptyCore => FinancialStrength.IsEmpty && CreditAppraisal.IsEmpty;

		#endregion

		#region Clear

		protected override void ClearCore()
		{
			base.ClearCore();
			FinancialStrength = string.Empty;
			CreditAppraisal = string.Empty;
		}

		#endregion
	}
}
