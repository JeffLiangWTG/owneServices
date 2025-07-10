using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Module.Testing
{
	public sealed class JobDeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestFilteredGridFields()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new JobDeclarationFilterBusinessObject();
			using (var filterControl = new JobDeclarationFilterStripControl(null, declarations, filterBO))
			{
				filterControl.Show();
				var messageStatusColumn = filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_MessageStatus);
				Assert("Should have GroupName setting", !messageStatusColumn.GroupName.IsEmpty());
				var messageStatusDescriptionColumn = filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_MessageStatusDescription);
				Assert("Should have GroupName setting", !messageStatusDescriptionColumn.GroupName.IsEmpty());
				Assert("Should visible by default", messageStatusDescriptionColumn.IsVisible);
				var declarationDateColumn = filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.DeclarationDate);
				Assert("DeclarationDate should visible by default", declarationDateColumn.IsVisible);
				var clearanceStatusColumn = filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.ClearanceStatus);
				Assert("ClearanceStatus should visible by default", clearanceStatusColumn.IsVisible);
				var importerNameColumn = filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.ImporterName);
				Assert("ImporterName should visible by default", importerNameColumn.IsVisible);
				var supplierNameColumn = filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.SupplierName);
				Assert("SupplierName should visible by default", supplierNameColumn.IsVisible);
				var importerChineseNameColumn = filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.ImporterChineseName);
				Assert("ImporterChineseName should visible by default", importerChineseNameColumn.IsVisible);
				var supplierChineseNameColumn = filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.SupplierChineseName);
				Assert("SupplierChineseName should visible by default", supplierChineseNameColumn.IsVisible);
				var declarationType = filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.DeclarationType);
				Assert("DeclarationType should visible by default", declarationType.IsVisible);
				var entryReleaseDate = filterControl.FilteredGrid.GetColumnStyle(BaseJobDeclaration.Schema.EntryReleaseDate);
				Assert("EntryReleaseDate should visible by default", entryReleaseDate.IsVisible);
			}
		}
	}
}
