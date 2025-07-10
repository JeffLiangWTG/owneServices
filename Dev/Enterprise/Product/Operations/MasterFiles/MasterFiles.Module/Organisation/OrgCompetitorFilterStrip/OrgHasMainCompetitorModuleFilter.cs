using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	public class OrgHasMainCompetitorModuleFilter : ModuleFilter
	{
		public delegate ZQuery GetHasMainCompetitorOnQueryDelegate(ZBool hasMainCompetitor, ZString competitorType);

		public OrgHasMainCompetitorModuleFilter(ZString description, GetHasMainCompetitorOnQueryDelegate queryDelegate)
			: base(description, queryDelegate)
		{
		}

		protected OrgHasMainCompetitorModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected override void ClearCore()
		{
			CompetitorType = ZString.Empty;
			HasMainCompetitor = ZBool.False;
		}

		protected override bool IsEmptyCore => CompetitorType.IsEmpty;

		#region Properties

		#region CompetitorTypes

		[BusinessObjectTestExclude]
		[List("CompetitorTypeList")]
		public ZString CompetitorType
		{
			get { return competitorType; }
			set
			{
				if (SetNonPersistentPropertyValue(CompetitorTypeInfo, ref competitorType, value))
				{
					((OrgHasMainCompetitorModuleFilterValidation)Validation).ValidateCompetitorType();
					InvalidateCachedQuery();
					CompetitorTypeInfo.RefreshBinding();
				}
			}
		}
		ZString competitorType;

		public ZPropertyInfo CompetitorTypeInfo
		{
			get { return GetZPropertyInfo(nameof(CompetitorType)); }
		}

		public CodeDescriptionPairList CompetitorTypeList
		{
			get { return competitorTypeList ?? (competitorTypeList = OrganisationsDataRegistry.Instance.CompetitorType.Value.GetCodeDescriptionPairList()); }
		}
		CodeDescriptionPairList competitorTypeList;

		#endregion

		#region HasMainCompetitor

		public ZBool HasMainCompetitor
		{
			get { return hasMainCompetitor; }
			set
			{
				if (HasMainCompetitor != value)
				{
					InvalidateCachedQuery();
				}
				hasMainCompetitor = value;
			}
		}
		ZBool hasMainCompetitor;

		#endregion

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { HasMainCompetitor, CompetitorType }; }
		}

		public override bool IsExpensiveQuery => false;

		protected override FilterCategory DefaultCategory => FilterCategories.Other;

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			writer.WriteElementString("CompetitorType", CompetitorType);
			writer.WriteElementString("HasMainCompetitor", HasMainCompetitor ? "Y" : "N");
		}

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			CompetitorType = reader.ReadElementString("CompetitorType");
			HasMainCompetitor = reader.ReadElementString("HasMainCompetitor") == "Y";
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new OrgHasMainCompetitorModuleFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (OrgHasMainCompetitorModuleFilter)filterToCopyFrom;
			CompetitorType = filter.CompetitorType;
			HasMainCompetitor = filter.HasMainCompetitor;
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new OrgHasMainCompetitorModuleFilterValidation(this);
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			throw new NotSupportedException();
		}

#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			this.CompetitorType = RandomString(3);
			this.HasMainCompetitor = ZBool.True;
		}

#endif

		#endregion
	}
}
