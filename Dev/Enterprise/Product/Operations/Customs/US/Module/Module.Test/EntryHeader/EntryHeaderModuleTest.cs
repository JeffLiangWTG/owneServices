using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(EntryHeaderModule))]
	sealed class EntryHeaderModuleTest : Customs.Module.Testing.EntryHeaderModuleTest
	{
		[RequiresSTA]
		public override void TestAllGridColumnsCanBeExportedToExcel()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()) as ZFilterModule)
			{
				try
				{
					if (module.ModuleDecisionProvider.AllowExcelExport && module.FilterBusinessObject != null)
					{
						var filterStripControl = module.EmbeddedControl as IFilterControl;
						ZGrid grid = filterStripControl.FilteredGrid;
						foreach (ZGridColumnInfo info in grid.ColumnStyles)
						{
							info.IsVisible = true;
						}

						((IFilterStripCommonControlInternalsForTesting)filterStripControl).Bind();
						grid.RefreshTableStyles();
						foreach (var column in grid.Columns)
						{
							if (column.IsVisible && !(column.ColumnStyle.PropertyDescriptor.PropertyType.IsInterface && column.ColumnStyle.PropertyDescriptor.PropertyType == typeof(IZType)))
							{
								var zInterface = column.ColumnStyle.PropertyDescriptor.PropertyType.GetInterface(nameof(IZType));
								AssertNotNull(string.Format("The property '{0}' of data source bound to ZGrid should implement IZType interface for exporting to excel", column.ColumnName), zInterface);
							}
						}
					}
					else
					{
						Assert("If excel export isn't supported on this module, no problem", true);
					}
				}
				catch (ModuleGuiNotSupportedException)
				{
					Assert("If the module isn't used for gui, no excel export will occur", true);
				}
			}
		}

		protected override Customs.Business.CusEntryHeader GetEntryThatMatchesGridCollectionFilter(Customs.Business.BaseJobDeclaration declaration)
		{
			var result = (CusEntryHeader)base.GetEntryThatMatchesGridCollectionFilter(declaration);
			result.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			return result;
		}
	}
}
