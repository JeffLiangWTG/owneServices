using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	sealed class CommercialInvoiceFilterStripControlTest : TestCaseWithFactory
	{
		public void TestWorkflowFilterStripIsInherited()
		{
			AssertEquals("Must inherit ZFilterStripControl<WorkflowFilterStrip> so that workflow filter strips may be selected", true, typeof(CommercialInvoiceFilterControl).IsSubclassOf(typeof(ZFilterStripControl<CommercialInvoiceFilterStrip>)));
		}

		public void TestWorkflowCIVCustomFieldColums()
		{
			CreateWorkflowWithCustomFields();
			using (var control = new CommercialInvoiceFilterControl(collection, filterBizObj))
			{
				var workflowColumns = control.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>()
					.Where(col => !string.IsNullOrEmpty(col.Caption) && col.Caption.StartsWith("CIV: ", StringComparison.Ordinal))
					.Select(col => col.Caption).ToArray();
				AssertContainsExactElementsInAnyOrder(new[] { "CIV: Custom text", "CIV: Custom int" }, workflowColumns);
			}
		}

		void CreateWorkflowWithCustomFields()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = WorkflowDescriptors.CommericalInvoiceWorkflowDescriptorCode;
			var customField1 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "CIV: Custom text";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;
			var customField2 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "CIV: Custom int";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;
			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			using (ZForm form = new ZForm())
			{
				CommercialInvoiceFilterControl filterControl = new CommercialInvoiceFilterControl(collection, filterBizObj);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}

		InvoiceHeaderWithNoDeclarationCollection collection;
		CommercialInvoiceFilterBusinessObject filterBizObj;
		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new CommercialInvoiceFilterBusinessObject();
			collection = new InvoiceHeaderWithNoDeclarationCollection(Factory);
		}
	}
}
