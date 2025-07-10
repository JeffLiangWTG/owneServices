using System;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class GlbAccreditationHighestLevelByProgramFilter : ModuleTextFilter
	{
		public GlbAccreditationHighestLevelByProgramFilter(ZString description) : base(description, EmptyQuery)
		{
		}

		static ZQuery EmptyQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery();
		}

		#region Schema

		public abstract class Schema
		{
			public const string AccreditationGroupDescription = "AccreditationGroupDescription";
			public const string IsCompleted = "IsCompleted";
		}

		#endregion

		#region Properties

		public new BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());

		BusinessObjectFactory factory;

		#region Accreditation Group

		[List("AccreditationGroups")]
		public ZString AccreditationGroupDescription
		{
			get
			{
				return accreditationGroupDescription;
			}
			set
			{
				if (SetNonPersistentPropertyValue(AccreditationGroupDescriptionInfo, ref accreditationGroupDescription, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateAccreditationGroupDescription();
					}
				}
			}
		}
		ZString accreditationGroupDescription;

		public ZPropertyInfo AccreditationGroupDescriptionInfo => GetZPropertyInfo(nameof(AccreditationGroupDescription));

		public ZGuid AccreditationGroupPK
		{
			get
			{
				var group = Factory.LoadTop1<GlbAccreditationGroup>(new ZQuery(GlbAccreditationGroupSchema.HAG_Description, AccreditationGroupDescription));

				return group?.PK ?? ZGuid.Empty;
			}
		}

		public CodeDescriptionPairList AccreditationGroups
		{
			get
			{
				var list = new CodeDescriptionPairList();
				var groups = new GlbAccreditationGroupCollection(Factory);
				groups.ForEach(x => list.AddPair(x.HAG_Description, x.HAG_Description));

				return list;
			}
		}

		#endregion

		#region Is Completed

		public ZBool IsCompleted
		{
			get => isCompleted;
			set => SetNonPersistentPropertyValue(IsCompletedInfo, ref isCompleted, value);
		}

		ZBool isCompleted = true;

		public ZPropertyInfo IsCompletedInfo => GetZPropertyInfo(nameof(IsCompleted));

		public bool IsCompleted_ReadOnly => IsEmpty;

		#endregion

		#endregion

		#region Validation

		public new GlbAccreditationLatestAttemptForGroupFilterValidation Validation
		{
			get { return (GlbAccreditationLatestAttemptForGroupFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new GlbAccreditationLatestAttemptForGroupFilterValidation(this);
		}

		#endregion

		#region Query

		protected override bool IsEmptyCore => AccreditationGroupPK.IsEmpty;

		protected override ZQuery GetQuery()
		{
			return IsEmpty ? new ZQuery() : GetLatestAttemptForGroupFilter();
		}

		ZQuery GetLatestAttemptForGroupFilter()
		{
			var result = new ZDBOnlyQuery(typeof(GlbAccreditationAttempt));
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@HAG_PK", AccreditationGroupPK, GlbAccreditationGroupSchema.PK);
			parameters.Add("@IsCompleted", IsCompleted, CargoWise.Schema.Schema.GenericBitSchemaColumn);

			var sql = FormattableString.Invariant($@"
HAA_PK IN 
(
	SELECT AttemptPk
	FROM dbo.GetHighestLevelAccreditationAttemptForGroup(@HAG_PK, @IsCompleted, null)	
)"); // SQL Query text is untranslatable
			result.AddFilterAndZSQLParameterCollection(sql, parameters);
			return result;
		}

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString(Schema.AccreditationGroupDescription, AccreditationGroupDescription.ToString());
			writer.WriteElementString(Schema.IsCompleted, IsCompleted.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == Schema.AccreditationGroupDescription)
			{
				AccreditationGroupDescription = reader.ReadElementString(Schema.AccreditationGroupDescription);
			}

			if (reader.Name == Schema.IsCompleted)
			{
				IsCompleted = new ZBool(reader.ReadElementString(Schema.IsCompleted));
			}
		}

		#endregion

		#region Clear

		protected override void ClearCore()
		{
			base.ClearCore();
			AccreditationGroupDescription = ZString.Empty;
			IsCompleted = false;
		}

		#endregion
	}
}
