using System;
using System.Collections;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgCodeMappingLocalModuleFilter : OrgCodeMappingBaseModuleFilter
	{
		#region Construction

		public OrgCodeMappingLocalModuleFilter(ZString description)
			: base(description)
		{
		}

		public OrgCodeMappingLocalModuleFilter(ZString description, GetTextQuery queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
		}

		#endregion

		#region Schema

		protected abstract class Schema : BaseCodeMappingSchema
		{
			public const string LocalCode = nameof(LocalCode);
		}

		#endregion

		#region Properties

		#region LocalCode

		[List("LocalCodes")]
		public ZGuid LocalCode
		{
			get { return localCode; }
			set
			{
				InvalidateCachedQuery();
				if (SetNonPersistentPropertyValue(LocalCodeInfo, ref localCode, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateLocalCode();
					}
					LocalCodeInfo.RefreshBinding();
				}
			}
		}
		ZGuid localCode;

		public ZPropertyInfo LocalCodeInfo => GetZPropertyInfo(Schema.LocalCode);

		public OrgHeaderCollection LocalCodes => new OrgHeaderCollection(new BusinessObjectFactory());

		#endregion

		#endregion

		#region Clear

		protected override void ClearCore()
		{
			base.ClearCore();
			LocalCode = ZGuid.Empty;
		}

		#endregion

		#region Query

		protected override ZQuery GetCodeMappingFilter()
		{
			var query = new ZQuery();
			if (!LocalCode.IsEmpty)
			{
				query.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalGuid, LocalCode);
			}
			return query;
		}

		#endregion

		#region Empty

		protected override bool IsEmptyCore => base.IsEmptyCore && LocalCode.IsEmpty;

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString(Schema.LocalCode, LocalCode.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			try
			{
				LocalCode = new ZGuid(reader.ReadElementString(Schema.LocalCode));
			}
			catch (FormatException)
			{
				// The guid in the xml was bad. Just ignore it... What's the worst that could happen? ¯\_(ツ)_/¯
			}
		}

		#endregion

		#region Validation

		public new OrgCodeMappingLocalModuleFilterValidation Validation => (OrgCodeMappingLocalModuleFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation() => new OrgCodeMappingLocalModuleFilterValidation(this);

		#endregion

	}
}
