using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	internal class ExternalRequestInfoTemplateFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestFilteredGridFields()
		{
			var collection = new ExternalRequestInfoTemplateCollection(Factory);
			var filter = new ExternalRequestInfoTemplateFilterBusinessObject();
			using var filterControl = new ExternalRequestInfoTemplateFilterControl(collection, filter);
			using var form = new ZForm();
			form.Controls.Add(filterControl);

			form.Show();

			CombineAssertions(() =>
			{
				AssertEquals(ExternalRequestInfoTemplate.Schema.RIT_Code, "Code", filterControl.FilteredGrid.GetColumnCaption(ExternalRequestInfoTemplate.Schema.RIT_Code));
				AssertEquals(ExternalRequestInfoTemplate.Schema.RIT_Description, "Description", filterControl.FilteredGrid.GetColumnCaption(ExternalRequestInfoTemplate.Schema.RIT_Description));
				AssertEquals(ExternalRequestInfoTemplate.Schema.RIT_JobType, "Job Type", filterControl.FilteredGrid.GetColumnCaption(ExternalRequestInfoTemplate.Schema.RIT_JobType));
				AssertEquals(ExternalRequestInfoTemplate.Schema.RIT_IsActive, "Is Active", filterControl.FilteredGrid.GetColumnCaption(ExternalRequestInfoTemplate.Schema.RIT_IsActive));
				AssertEquals(ExternalRequestInfoTemplate.Schema.RIT_IsSystem, "Is System", filterControl.FilteredGrid.GetColumnCaption(ExternalRequestInfoTemplate.Schema.RIT_IsSystem));
			});
		}
	}
}
