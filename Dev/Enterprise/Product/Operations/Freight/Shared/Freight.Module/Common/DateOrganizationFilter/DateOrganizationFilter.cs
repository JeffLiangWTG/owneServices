using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Module
{
	public class DateOrganizationFilter : ModuleDateFilter
	{
		#region Construction

		public DateOrganizationFilter(
			ZString description,
			SchemaDateTimeColumn dateTimeColumn,
			GetGuidQueryWithOperator organizationQueryDelegate,
			IBusinessObjectCollection organizationList,
			ResourceStringData organizationLabel
		) : base(description, dateTimeColumn)
		{
			Argument.NotNull(organizationList, nameof(organizationList));

			OrganizationList = organizationList;
			OrganizationLabel = organizationLabel;
			OrganizationQueryDelegate = organizationQueryDelegate;
		}

		#endregion

		#region Properties

		protected override FilterCategory DefaultCategory => FilterCategories.Dates;

		public ResourceStringData OrganizationLabel { get; }

		public IBusinessObjectCollection OrganizationList { get; }

		GetGuidQueryWithOperator OrganizationQueryDelegate { get; }

		public ZGuid OrganizationPK
		{
			get => organizationPK;
			set
			{
				if (organizationPK != value)
				{
					organizationPK = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateOrganizationPK();
					}

					OrganizationPKInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		protected bool OrganizationPK_ReadOnly
		{
			get => OrganizationComparisonOperator == ModuleFilterWithListAndComparisonOperators<ZGuid>.ComparisonConstants.IsBlank
				|| OrganizationComparisonOperator == ModuleFilterWithListAndComparisonOperators<ZGuid>.ComparisonConstants.IsNotBlank;
		}

		public ZPropertyInfo OrganizationPKInfo
		{
			get => GetZPropertyInfo(nameof(OrganizationPK));
		}
		ZGuid organizationPK;

		public Validation OrganizationPKValidation
		{
			[DebuggerStepThrough]
			get => organizationPKValidation;
			[DebuggerStepThrough]
			set { organizationPKValidation = value; }
		}
		Validation organizationPKValidation;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual ZString OrganizationComparisonOperator
		{
			get => fComparisonOperator;
			set
			{
				if (fComparisonOperator != value)
				{
					fComparisonOperator = value;

					if (value == ModuleFilterWithListAndComparisonOperators<ZGuid>.ComparisonConstants.IsBlank || value == ModuleFilterWithListAndComparisonOperators<ZGuid>.ComparisonConstants.IsNotBlank)
					{
						OrganizationPK = ZGuid.Empty;
					}

					ComparisonOperatorInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		ZString fComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;

		public SQLComparisonOperator SqlComparisonOperator
		{
			get => ModuleTextFilter.GetSqlComparisonOperator(OrganizationComparisonOperator, SQLComparisonOperator.Equal);
		}

		public ZPropertyInfo ComparisonOperatorInfo
		{
			get => GetZPropertyInfo(nameof(OrganizationComparisonOperator));
		}

		public CodeDescriptionPairList ComparisonOperator_List
		{
			get
			{
				if (fComparisonOperator_List == null)
				{
					var operatorList = new CodeDescriptionPairList();
					operatorList.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
					operatorList.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
					operatorList.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
					operatorList.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));

					fComparisonOperator_List = operatorList;
				}

				return fComparisonOperator_List;
			}
		}

		CodeDescriptionPairList fComparisonOperator_List;

		#endregion

		#region GetQuery

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var result = base.GetQueryUsingFilterColumns();

			var emptyOrganisationWithEqualityFilter = OrganizationPK == ZGuid.Empty &&
				(SqlComparisonOperator == SQLComparisonOperator.Equal || SqlComparisonOperator == SQLComparisonOperator.NotEqual);

			if (OrganizationQueryDelegate != null && !emptyOrganisationWithEqualityFilter)
			{
				result.AddToFilter((ZQuery)OrganizationQueryDelegate.DynamicInvoke(new object[] { SqlComparisonOperator, OrganizationPK }));
			}

			return result;
		}

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			base.ClearCore();
			OrganizationPK = ZGuid.Empty;
			OrganizationComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
		}

		protected override bool IsEmptyCore =>
			base.IsEmptyCore && OrganizationPK.IsEmpty && OrganizationComparisonOperator == ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;

		#endregion

		#region Validation

		public new DateOrganizationFilterValidation Validation
		{
			get => (DateOrganizationFilterValidation)base.Validation;
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new DateOrganizationFilterValidation(this);
		}

		public class DateOrganizationFilterValidation : ModuleFilterDateValidation
		{
			public DateOrganizationFilterValidation(DateOrganizationFilter parent)
				: base(parent)
			{
				Parent = parent;
			}

			public void ValidateOrganizationPK()
			{
				ValidateCalculatedProperty(Parent.OrganizationPKInfo);
			}

			public override void ValidateAll()
			{
				base.ValidateAll();
				ValidateOrganizationPK();
			}

			protected virtual void CheckOrganizationPK()
			{
				ListValidation.ErrorIfInvalidPK(Parent.OrganizationPKInfo, Parent.OrganizationList);

				Parent.OrganizationPKValidation?.Invoke(Parent.OrganizationPKInfo);
			}

			public override Type AutoValidationType => GetType();

			protected new readonly DateOrganizationFilter Parent;
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString(nameof(OrganizationPK), OrganizationPK.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			var organizationPropertyName = nameof(OrganizationPK);

			if (reader.Name == organizationPropertyName && ZGuid.TryParse(reader.ReadElementString(organizationPropertyName), out var parsedOrg))
			{
				OrganizationPK = parsedOrg;
			}
		}

		#endregion
	}
}
