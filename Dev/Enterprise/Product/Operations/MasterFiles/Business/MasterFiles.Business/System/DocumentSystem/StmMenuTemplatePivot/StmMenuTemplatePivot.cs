using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ExcelTemplates.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty("DocumentTitle"), DescriptionProperty("DocumentIndex")]
	public class StmMenuTemplatePivot : AutoStmMenuTemplatePivot, IStmMenuTemplatePivot
	{
		public StmMenuTemplatePivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString DocumentTitle
		{
			get { return SI_DocumentTitle; }
		}

		public ZString DocumentIndex
		{
			get
			{
				return ResString.GetMultilingualString("0a5fc6d5-6a87-4b9a-9552-103af47fe13a", "Template index: {0}", SI_Index);
			}
		}

		IRefDocType IStmMenuTemplatePivot.DocType => DocType;
		IStmMenuItem IStmMenuTemplatePivot.MenuItem => MenuItem;
		IStmTemplate IStmMenuTemplatePivot.Template => Template;

		internal static StmMenuTemplatePivot FindDocumentPivot(BusinessObjectFactory factory, StmTemplate clientTemplate, StmMenuItem menuItem)
		{
			ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(StmMenuTemplatePivot));
			dBOnlyQuery.AddToFilter(StmMenuTemplatePivotSchema.SI_IsSystemDefined, clientTemplate.SO_IsSystemDefined);
			dBOnlyQuery.AddToFilter(StmMenuTemplatePivotSchema.SI_IsClientSpecific, clientTemplate.SO_IsClientSpecific);
			dBOnlyQuery.AddToFilter(StmMenuTemplatePivotSchema.SI_SO, clientTemplate.PK);
			dBOnlyQuery.AddToFilter(StmMenuTemplatePivotSchema.SI_SU, menuItem.PK);
			return (StmMenuTemplatePivot)factory.LoadTop1(typeof(StmMenuTemplatePivot), dBOnlyQuery);
		}

		internal static StmMenuTemplatePivot CreateDocumentPivot(BusinessObjectFactory newFactory, StmTemplate clientTemplate, StmMenuItem menuItem, ZString copyFromMenuName)
		{
			ZDBOnlyQuery pivotQuery = new ZDBOnlyQuery(typeof(StmMenuTemplatePivot));
			pivotQuery.AddToFilter(StmMenuTemplatePivotSchema.SI_IsSystemDefined, true);
			pivotQuery.AddToFilter(StmMenuTemplatePivotSchema.SI_IsClientSpecific, false);
			pivotQuery.AddToFilter(StmMenuTemplatePivotSchema.SI_DocumentTitle, copyFromMenuName);

			ZDBOnlySubQuery menuItemSubQuery = new ZDBOnlySubQuery(typeof(StmMenuItem), StmMenuTemplatePivotSchema.SI_SU);
			menuItemSubQuery.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
			menuItemSubQuery.AddToFilter(StmMenuItemSchema.SU_IsClientSpecific, false);
			menuItemSubQuery.AddToFilter(StmMenuItemSchema.SU_MenuName, copyFromMenuName);
			menuItemSubQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, menuItem.SU_BusinessContext);
			pivotQuery.AddSubQuery(menuItemSubQuery, JoinCondition.And);

			StmMenuTemplatePivot stmMenuTemplatePivot = (StmMenuTemplatePivot)newFactory.LoadTop1(typeof(StmMenuTemplatePivot), pivotQuery);

			StmMenuTemplatePivot newStmMenuTemplatePivot = newFactory.New<StmMenuTemplatePivot>();
			newStmMenuTemplatePivot.CopyPersistentValuesFrom(stmMenuTemplatePivot);
			newStmMenuTemplatePivot.SI_IsSystemDefined = clientTemplate.SO_IsSystemDefined;
			newStmMenuTemplatePivot.SI_IsClientSpecific = clientTemplate.SO_IsClientSpecific;
			newStmMenuTemplatePivot.SI_SU = menuItem.PK;
			newStmMenuTemplatePivot.SI_SO = clientTemplate.PK;
			newStmMenuTemplatePivot.SI_DocumentTitle = clientTemplate.SO_Name;
			newStmMenuTemplatePivot.SI_MenuTemplateFilter = null;   // copy filter can cause problem if there are multiple pivots with different filters, so skip the filter.
			return newStmMenuTemplatePivot;
		}

		#region SI_MenuTemplateFilter
		[BusinessObjectTestExclude]
		public override ZString SI_MenuTemplateFilter
		{
			get { return base.SI_MenuTemplateFilter; }
			set { base.SI_MenuTemplateFilter = value; }
		}
		#endregion
	}
}
