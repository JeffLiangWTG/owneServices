using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class DocumentCustomizationToolTest : TestCaseWithFactory
	{
		public void TestShowDocumentCustomizationForm()
		{
			BusinessObject bizObj = Factory.New<DummyBusinessObject>();
			AssertEquals(false, bizObj is IDocumentSupportable);

			using (ZFormWithDocumentCustomizationTool form = new ZFormWithDocumentCustomizationTool(bizObj))
			{
				form.ShowDocumentCustomizationTool();

				AssertEquals(false, Application.OpenForms.Cast<Form>().Any(f => f is IMenuCustomisationForm));
			}

			bizObj = Factory.New<DummyDocumentSupportable>();

			using (ZFormWithDocumentCustomizationTool form = new ZFormWithDocumentCustomizationTool(bizObj))
			{
				form.ShowDocumentCustomizationTool();

				AssertEquals(true, Application.OpenForms.Cast<Form>().Any(f => f is IMenuCustomisationForm));
			}
		}

		class ZFormWithDocumentCustomizationTool : ZForm
		{
			public ZFormWithDocumentCustomizationTool(object bizObj)
				: base(bizObj)
			{
			}

			public void ShowDocumentCustomizationTool()
			{
				IDevTool docCustomizationTool = new DocumentCustomizationTool();
				docCustomizationTool.Show(this);
			}
		}

		class DummyDocumentSupportable : DummyBusinessObject, IDocumentSupportable
		{
			public DummyDocumentSupportable(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DocumentSupporter DocumentSupporter
			{
				get { return documentSupporter ?? (documentSupporter = new DocumentSupporterForTest(this)); }
			}
			DocumentSupporter documentSupporter;
		}

		class DocumentSupporterForTest : DocumentSupporter
		{
			public DocumentSupporterForTest(BusinessObject parentBusinessObject)
				: base(parentBusinessObject)
			{
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.Test; }
			}

			public override ZArchitecture.Modules.ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { throw new System.NotImplementedException(); }
			}

			protected override DocumentEngineCore.DocWrappers.DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, MasterFiles.Integration.IStmMenuItem commandBeingRun)
			{
				throw new System.NotImplementedException();
			}

			protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				return new Core.Constants.DataContext[1];
			}
		}
	}
}
