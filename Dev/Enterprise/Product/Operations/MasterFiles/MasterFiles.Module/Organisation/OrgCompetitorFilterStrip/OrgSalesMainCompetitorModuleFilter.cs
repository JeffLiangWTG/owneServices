using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	public class OrgSalesMainCompetitorModuleFilter : ModuleFilter
	{
		public delegate ZQuery GetSalesCompetitorOnQueryDelegate(ZGuid competitor, ZString competitorType);

		public OrgSalesMainCompetitorModuleFilter(ZString description, GetSalesCompetitorOnQueryDelegate queryDelegate)
			: base(description, queryDelegate)
		{
		}

		protected OrgSalesMainCompetitorModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected override void ClearCore()
		{
			CompetitorType = ZString.Empty;
			Competitor = ZGuid.Empty;
		}

		protected override bool IsEmptyCore => CompetitorType.IsEmpty || Competitor.IsEmpty;

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
					if (!IsValidationSuspended)
					{
						Validation.ValidateCompetitorAndCompetitorType();
					}
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

		#region Competitor

		[List("Competitors")]
		public ZGuid Competitor
		{
			get { return competitor; }
			set
			{
				if (SetNonPersistentPropertyValue(CompetitorInfo, ref competitor, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateCompetitorAndCompetitorType();
					}
					InvalidateCachedQuery();
					CompetitorInfo.RefreshBinding();
				}
			}
		}
		ZGuid competitor;

		public ZPropertyInfo CompetitorInfo
		{
			get { return GetZPropertyInfo(nameof(Competitor)); }
		}

		public OrganisationsFindBoxCollection Competitors
		{
			get
			{
				if (competitors == null)
				{
					competitors = new OrganisationsFindBoxCollection(new BusinessObjectFactory());
				}

				var defaultFilter = competitors.FilterBusinessObjectDefaults;
				defaultFilter.RemoveAll();

				switch (CompetitorType)
				{
					case Registry.Business.CompetitorTypeList.Codes.Customs:
						defaultFilter.Add(new FilterBusinessObjectDefault("Organisation Types", "Property8", ZBool.True));
						break;

					case Registry.Business.CompetitorTypeList.Codes.Forwarding:
						defaultFilter.Add(new FilterBusinessObjectDefault("Organisation Types", "Property5", ZBool.True));
						break;

					case Registry.Business.CompetitorTypeList.Codes.Warehouse:
						defaultFilter.Add(new FilterBusinessObjectDefault("Organisation Types", "Property7", ZBool.True));
						break;

					case Registry.Business.CompetitorTypeList.Codes.LandTransport:
						defaultFilter.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.LocalTransport));
						break;
				}

				return competitors;
			}
		}
		OrganisationsFindBoxCollection competitors;

		#endregion

		#region Validation

		new OrgSalesMainCompetitorModuleFilterValidation Validation => (OrgSalesMainCompetitorModuleFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new OrgSalesMainCompetitorModuleFilterValidation(this);
		}

		#endregion

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { Competitor, CompetitorType }; }
		}

		public override bool IsExpensiveQuery => false;

		protected override FilterCategory DefaultCategory => FilterCategories.Other;

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			writer.WriteElementString("CompetitorType", CompetitorType);
			writer.WriteElementString("Competitor", Competitor.ToString());
		}

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			CompetitorType = reader.ReadElementString("CompetitorType");
			Competitor = new Guid(reader.ReadElementString("Competitor"));
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new OrgSalesMainCompetitorModuleFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (OrgSalesMainCompetitorModuleFilter)filterToCopyFrom;
			CompetitorType = filter.CompetitorType;
			Competitor = filter.Competitor;
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			throw new NotSupportedException();
		}

#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			this.CompetitorType = RandomString(3);
			this.Competitor = ZGuid.NewZGuid();
		}

#endif

		#endregion
	}
}
