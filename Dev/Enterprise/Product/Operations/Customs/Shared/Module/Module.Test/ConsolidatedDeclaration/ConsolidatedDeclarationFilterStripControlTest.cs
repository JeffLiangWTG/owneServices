using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	sealed class ConsolidatedDeclarationFilterStripControlTest : TestCaseWithFactory
	{
		public void TestWorkflowFilterStripIsInherited()
		{
			AssertEquals("Must inherit ZFilterStripControl<WorkflowFilterStrip> so that workflow filter strips may be selected", true, typeof(JobDeclarationFilterStripControl).IsSubclassOf(typeof(ZFilterStripControl<MasterFiles.Module.WorkflowFilterStrip>)));
		}

		public void TestDeclarationGridColumns()
		{
			var consolidatedDeclarations = new ConsolidatedDeclarationCollection<ConsolidatedDeclaration>(Factory, ConsolidatedDeclaration.ApplicationCodes.TSW);
			var filterBO = new ConsolidatedDeclarationFilterBusinessObject();
			using (ZForm form = new ZForm())
			{
				using (var userControl = new ConsolidatedDeclarationFilterStripControl(null, consolidatedDeclarations, filterBO))
				{
					form.Show();
					var grid = userControl.FilteredGrid;
					CombineAssertions(() =>
					{
						AssertNotNull("CRD_JobReferenceNumber", grid.GetColumnStyle("CRD_JobReferenceNumber"));
						AssertNotNull("LeadDeclaration+DeclarationNumber", grid.GetColumnStyle("LeadDeclaration+DeclarationNumber"));
						AssertNotNull("LeadDeclaration+JE_MasterBill", grid.GetColumnStyle("LeadDeclaration+JE_MasterBill"));
						AssertNotNull("LeadDeclaration+JE_RL_NKPortOfLoading", grid.GetColumnStyle("LeadDeclaration+JE_RL_NKPortOfLoading"));
						AssertNotNull("LeadDeclaration+JE_ExportDate", grid.GetColumnStyle("LeadDeclaration+JE_ExportDate"));
						AssertNotNull("LeadDeclaration+JE_RL_NKPortOfArrival", grid.GetColumnStyle("LeadDeclaration+JE_RL_NKPortOfArrival"));
						AssertNotNull("LeadDeclaration+JE_DateOfArrival", grid.GetColumnStyle("LeadDeclaration+JE_DateOfArrival"));
						AssertNotNull("LeadDeclaration+JE_TransportMode", grid.GetColumnStyle("LeadDeclaration+JE_TransportMode"));
						AssertNotNull("LeadDeclaration+JE_VesselName", grid.GetColumnStyle("LeadDeclaration+JE_VesselName"));
						AssertNotNull("LeadDeclaration+JE_VoyageFlightNo", grid.GetColumnStyle("LeadDeclaration+JE_VoyageFlightNo"));
						AssertNotNull("CRD_PeriodTo", grid.GetColumnStyle("CRD_PeriodTo"));
						AssertNotNull("LeadDeclaration+JE_OH_Importer", grid.GetColumnStyle("LeadDeclaration+JE_OH_Importer"));
						AssertNotNull("LeadDeclaration+JE_OH_Supplier", grid.GetColumnStyle("LeadDeclaration+JE_OH_Supplier"));
						AssertNotNull("CRD_MessageStatus", grid.GetColumnStyle("CRD_MessageStatus"));
						AssertNotNull("CRD_CustomsStatus", grid.GetColumnStyle("CRD_CustomsStatus"));
						AssertNotNull("LeadDeclaration+JE_EntrySubmittedDate", grid.GetColumnStyle("LeadDeclaration+JE_EntrySubmittedDate"));
					});
				}
			}
		}

		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			var consolidatedDeclarations = new ConsolidatedDeclarationCollection<ConsolidatedDeclaration>(Factory, ConsolidatedDeclaration.ApplicationCodes.TSW);
			var filterBO = new ConsolidatedDeclarationFilterBusinessObject();
			using (ZForm form = new ZForm())
			{
				var filterControl = new ConsolidatedDeclarationFilterStripControl(null, consolidatedDeclarations, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}
	}
}
