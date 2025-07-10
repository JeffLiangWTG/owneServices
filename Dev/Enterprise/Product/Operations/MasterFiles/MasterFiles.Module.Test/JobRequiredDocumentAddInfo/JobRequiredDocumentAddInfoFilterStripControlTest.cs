using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class JobRequiredDocumentAddInfoFilterStripControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestFilteredGridFields()
		{
			var collection = new JobRequiredDocumentAddInfoCollection(Factory);
			var filterBO = new JobRequiredDocumentAddInfoFilterBusinessObject();

			using (var filterControl = new JobRequiredDocumentAddInfoFilterStripControl(collection, filterBO))
			{
				filterControl.Show();
				AssertNotNull("Reference Number", filterControl.FilteredGrid.GetColumnStyle("EX_ReferenceNumber"));
				AssertNotNull("Application Code", filterControl.FilteredGrid.GetColumnStyle("EX_ApplicationCode"));
				AssertNotNull("Status", filterControl.FilteredGrid.GetColumnStyle("EX_Status"));
				AssertNotNull("DocumentType", filterControl.FilteredGrid.GetColumnStyle("DocType"));
				AssertNotNull("Category", filterControl.FilteredGrid.GetColumnStyle("Category"));
				AssertNotNull("Description", filterControl.FilteredGrid.GetColumnStyle("Description"));
				AssertNotNull("Period", filterControl.FilteredGrid.GetColumnStyle("Period"));
				AssertNotNull("DateReceived", filterControl.FilteredGrid.GetColumnStyle("DateReceived"));
				AssertNotNull("ValidToDate", filterControl.FilteredGrid.GetColumnStyle("ValidToDate"));
				AssertNotNull("DocNumber", filterControl.FilteredGrid.GetColumnStyle("DocNumber"));
				AssertNotNull("DocUsage", filterControl.FilteredGrid.GetColumnStyle("DocUsage"));
				AssertNotNull("Country", filterControl.FilteredGrid.GetColumnStyle("Country"));
				AssertNotNull("OriginalDocRequired", filterControl.FilteredGrid.GetColumnStyle("OriginalDocRequired"));
				AssertNotNull("CreditControlDoc", filterControl.FilteredGrid.GetColumnStyle("CreditControlDoc"));
			}
		}
	}
}
