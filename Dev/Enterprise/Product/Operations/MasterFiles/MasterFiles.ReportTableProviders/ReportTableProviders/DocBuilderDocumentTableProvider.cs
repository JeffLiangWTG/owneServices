using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.ReportTableProviders
{
	class DocBuilderDocumentTableProvider : TableProvider
	{
		public override DataTable GetDataTable(string tableName, string dataSourceString, Report report, bool needsToAddWhereClause)
		{
			return GetDataTable(tableName, dataSourceString, report, needsToAddWhereClause, -1);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant for Table name, Hard-coded constant for Table column")]
		public override DataTable GetDataTable(string tableName, string dataSourceString, Report report, bool needsToAddWhereClause, int maximumNumberOfRows)
		{
			var table = new DataTable("DocBuilderMenus");
			table.Locale = CultureInfo.InvariantCulture;
			table.Columns.Add(new DataColumn("BusinessContext"));
			table.Columns.Add(new DataColumn("MenuName"));
			table.Columns.Add(new DataColumn("MenuPath"));
			table.Columns.Add(new DataColumn("Description"));
			table.Columns.Add(new DataColumn("Filter"));
			table.Columns.Add(new DataColumn("SecurityPath"));
			table.Columns.Add(new DataColumn("DocGroup"));

			var factory = new ReadOnlyBusinessObjectFactory(false);
			var docBuilderMenus = GetAllDocBuilderDocuments(factory);
			var registeredControllers = new ControllerList().All.Select(controllerInfo => ZControllerFactory.Create(controllerInfo.ID));

			var businessContextToModuleSecurityPath = new Dictionary<string, List<string>>();
			foreach (var controller in registeredControllers)
			{
				if (controller == null)
				{
					continue;
				}

				ZModule module = null;
				try
				{
					module = ZModuleFactory.Instance.Create(controller.ModuleID);
				}
				catch (NotImplementedException) { }
				catch (NotSupportedException) { }

				if (module == null)
				{
					continue;
				}

				using (module)
				{
					var businessContext = GetModuleControllerBusinessContext(factory, module, controller);
					if (businessContext != BusinessContext.INVALID)
					{
						var moduleSecurityPath = module?.SecurityCheckpoint?.DisplayTextPathToSecurityRight.GetUnresolvedString();
						if (!string.IsNullOrEmpty(moduleSecurityPath) && !moduleSecurityPath.Equals(NoneSecurityCheckpoint.NoneCode, StringComparison.OrdinalIgnoreCase))
						{
							var securityPathList = businessContextToModuleSecurityPath.GetOrAdd(businessContext.ToString(), () => { return new List<string>(); });
							if (!securityPathList.Contains(moduleSecurityPath))
							{
								securityPathList.Add(moduleSecurityPath);
							}
						}
					}
				}
			}

			foreach (var menuItem in docBuilderMenus)
			{
				List<string> securityPathList;
				if (!businessContextToModuleSecurityPath.TryGetValue(menuItem.SU_BusinessContext, out securityPathList))
				{
					securityPathList = new List<string>() { menuItem.SU_BusinessContext };
				}

				foreach (var moduleSecurityPath in securityPathList)
				{
					var newRow = table.NewRow();
					newRow[0] = menuItem.SU_BusinessContext;
					newRow[1] = menuItem.SU_MenuName;
					newRow[2] = menuItem.SU_MenuPath;
					newRow[3] = menuItem.SU_Hint;
					newRow[4] = menuItem.SU_FilterList;
					newRow[5] = moduleSecurityPath;
					newRow[6] = menuItem.SU_ContactType;
					table.Rows.Add(newRow);
				}
			}
			var sort = report.SortOrderCollection.SelectedOrder.FieldList;
			if (sort != "NULL")
			{
				table.DefaultView.Sort = sort;
			}
			return table.DefaultView.ToTable();
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		StmMenuItem[] GetAllDocBuilderDocuments(BusinessObjectFactory factory)
		{
			var queryText = string.Format(CultureInfo.InvariantCulture, @"{0} IN (SELECT {0}
FROM  dbo.StmMenuItem
WHERE {1} in ('DOC', 'WEB')
AND {2} = 1
AND {3} not like 'Rep%'
AND dbo.TemplatesForMenuItem(SU_PK) like '%System Document Elements%')", AutoStmMenuItem.Schema.PK, AutoStmMenuItem.Schema.SU_MenuType, AutoStmMenuItem.Schema.SU_IsSystemDefined, AutoStmMenuItem.Schema.SU_BusinessContext);
			var query = new ZDBOnlyQuery(typeof(StmMenuItem));
			query.AddFilterAndZSQLParameterCollection(queryText, new ZSqlParameterCollection());
			return factory.Load<StmMenuItem>(query);
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		BusinessContext GetModuleControllerBusinessContext(BusinessObjectFactory factory, ZModule module, ZController controller)
		{
			BusinessContext businessContext = BusinessContext.INVALID;

			var moduleWithBusinessContext = module as IDocumentBusinessContext;
			if (moduleWithBusinessContext != null)
			{
				businessContext = moduleWithBusinessContext.BusinessContext;
			}
			else
			{
				Type topLevelBusinessObjectType = null;
				try
				{
					topLevelBusinessObjectType = controller.TypeOfTopLevelBusinessObject;
				}
				catch (NotImplementedException) { }
				catch (NotSupportedException) { }

				if (topLevelBusinessObjectType != null && typeof(IDocumentSupportable).IsAssignableFrom(topLevelBusinessObjectType))
				{
					try
					{
						var documentSupportable = factory.New(topLevelBusinessObjectType) as IDocumentSupportable;
						if (documentSupportable != null)
						{
							businessContext = documentSupportable.DocumentSupporter.BusinessContext;
						}
					}
					catch (Exception)
					{
					}
				}
			}
			return businessContext;
		}
	}
}
