using System.Collections;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgCodeMappingForeignModuleFilter : OrgCodeMappingBaseModuleFilter
	{
		#region Construction

		public OrgCodeMappingForeignModuleFilter(ZString description)
			: base(description)
		{
		}

		public OrgCodeMappingForeignModuleFilter(ZString description, GetTextQuery queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
		}

		#endregion

		#region Schema

		protected abstract class Schema : BaseCodeMappingSchema
		{
			public const string ForeignCode = nameof(ForeignCode);
		}

		#endregion

		#region Properties

		#region LocalCode

		public ZString ForeignCode
		{
			get { return foreignCode; }
			set
			{
				InvalidateCachedQuery();
				if (SetNonPersistentPropertyValue(ForeignCodeInfo, ref foreignCode, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateForeignCode();
					}
					ForeignCodeInfo.RefreshBinding();
				}
			}
		}
		ZString foreignCode;

		public ZPropertyInfo ForeignCodeInfo => GetZPropertyInfo(Schema.ForeignCode);

		#endregion

		#endregion

		#region Clear

		protected override void ClearCore()
		{
			base.ClearCore();
			ForeignCode = string.Empty;
		}

		#endregion

		#region Query

		protected override ZQuery GetCodeMappingFilter()
		{
			var query = new ZQuery();
			if (!string.IsNullOrEmpty(ForeignCode))
			{
				query.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, SQLComparisonOperator.StartsWith, ForeignCode);
			}
			return query;
		}

		#endregion

		#region Empty

		protected override bool IsEmptyCore => base.IsEmptyCore && ForeignCode.IsEmpty;

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString(Schema.ForeignCode, ForeignCode);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			ForeignCode = reader.ReadElementString(Schema.ForeignCode);
		}

		#endregion

		#region Validation

		public new OrgCodeMappingForeignModuleFilterValidation Validation => (OrgCodeMappingForeignModuleFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation() => new OrgCodeMappingForeignModuleFilterValidation(this);

		#endregion
	}
}
