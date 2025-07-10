using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	internal class ExternalRequestTypesFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestFilteredGridFields()
		{
			var collection = new ExternalRequestTypeCollection(Factory);
			var filter = new ExternalRequestTypesFilterBusinessObject();
			using var filterControl = new ExternalRequestTypesFilterControl(collection, filter);
			using var form = new ZForm();
			form.Controls.Add(filterControl);

			form.Show();

			CombineAssertions(() =>
			{
				AssertEquals(ExternalRequestType.Schema.RQT_Code, "Code", filterControl.FilteredGrid.GetColumnCaption(ExternalRequestType.Schema.RQT_Code));
				AssertEquals(ExternalRequestType.Schema.RQT_Description, "Description", filterControl.FilteredGrid.GetColumnCaption(ExternalRequestType.Schema.RQT_Description));
				AssertEquals(ExternalRequestType.Schema.RQT_JobType, "Job Type", filterControl.FilteredGrid.GetColumnCaption(ExternalRequestType.Schema.RQT_JobType));
				AssertEquals(ExternalRequestType.Schema.RQT_IsActive, "Is Active", filterControl.FilteredGrid.GetColumnCaption(ExternalRequestType.Schema.RQT_IsActive));
				AssertEquals(ExternalRequestType.Schema.RQT_IsSystem, "Is System", filterControl.FilteredGrid.GetColumnCaption(ExternalRequestType.Schema.RQT_IsSystem));
			});
		}
	}
}
