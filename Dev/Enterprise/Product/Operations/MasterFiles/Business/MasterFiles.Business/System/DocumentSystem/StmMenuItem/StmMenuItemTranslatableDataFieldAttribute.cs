using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
#pragma warning disable CA1813
	class StmMenuItemTranslatableDataFieldAttribute : TranslatableDataFieldAttribute, ICustomizableDataCaptionSource
#pragma warning restore CA1813
	{
		public StmMenuItemTranslatableDataFieldAttribute(string columnName)
			: base(StmMenuItem.Schema.TableName, columnName, DataXmlFilePaths.Documents)
		{
			Type = typeof(StmMenuItem);
		}

		public override ZQuery Filter
		{
			get
			{
				var filter = new ZQuery(StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.NotContains, "HideThisDocument");
				filter.AddToFilter(StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.DoesNotStartWith, "CTY=");
				filter.AddToFilter(StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.NotContains, "MYDO=Y");
				filter.AddToFilter(StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.NotContains, "MYPENSRR=Y");
				filter.AddToFilter(StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.NotContains, "@env.CurrentUser.IsDeveloper");
				filter.AddToFilter(JoinCondition.Or, StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.StartsWith, "CTY=TW");
				filter.AddToFilter(JoinCondition.Or, StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.StartsWith, "CTY=IT");
				return filter;
			}
		}
	}
}
