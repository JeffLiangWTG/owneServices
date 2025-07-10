using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using LayoutPreviousDocumentsUserControl = Enterprise.Customs.PL.GUI.PlugIn.LayoutPreviousDocumentsUserControl;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(LayoutPreviousDocumentsUserControl))]
sealed class LayoutPreviousDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestInvoiceLine_Import() => AssertPreviousDocumentsUserControl(new InvoiceLineTestData_Import(Factory.New<JobDeclaration>()));

	public void TestInvoiceLine_Export() => AssertPreviousDocumentsUserControl(new InvoiceLineTestData_Export(Factory.New<JobDeclaration>()));

	public void TestInvoiceHeader_Import() => AssertPreviousDocumentsUserControl(new InvoiceHeaderTestData_Import(Factory.New<JobDeclaration>()));

	public void TestInvoiceHeader_Export() => AssertPreviousDocumentsUserControl(new InvoiceHeaderTestData_Export(Factory.New<JobDeclaration>()));

	public void TestJobDeclaration_Import() => AssertPreviousDocumentsUserControl(new JobDeclarationTestData_Import(Factory.New<JobDeclaration>()));

	public void TestJobDeclaration_Export() => AssertPreviousDocumentsUserControl(new JobDeclarationTestData_Export(Factory.New<JobDeclaration>()));

	public void TestEntryInstruction_Import() => AssertPreviousDocumentsUserControl(new EntryInstructionTestData_Import(Factory.New<JobDeclaration>()));

	public void TestEntryInstruction_Export() => AssertPreviousDocumentsUserControl(new EntryInstructionTestData_Export(Factory.New<JobDeclaration>()));

	void AssertPreviousDocumentsUserControl(TestData testData)
	{
		var declaration = testData.Parent as JobDeclaration;
		declaration.JE_MessageType = testData.MessageType;

		using (var form = new ZForm(declaration))
		using (var control = new LayoutPreviousDocumentsUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			form.SetDataBinding(null, null);
			new ControlRebinder().Rebind(control, nameof(JobDeclaration.FilteredInvoiceLines), testData.BindingString);
			form.SetDataBinding(declaration, "");

			CombineAssertions(() =>
			{
				var grid = control.FindSingle<ZGrid>(nameof(LayoutPreviousDocumentsUserControl.PreviousDocumentsGrid));
				AssertGridColumns(testData, grid);
				testData.AddNewPreviousDocument();

				var detailsLayoutUserControl = control.FindSingle<PreviousDocumentsDetailsLayoutControl>(nameof(LayoutPreviousDocumentsUserControl.DetailsLayoutControl));
				AssertLayoutControls(testData, detailsLayoutUserControl.DetailsPanel);
			});
		}
	}

	void AssertLayoutControls(TestData testData, DynamicLayoutPanel dynamicLayoutPanel)
	{
		var layoutPanel = testData.LayoutPanel;

		var additionalColumnAmmount = dynamicLayoutPanel.Controls.Cast<Control>().Count(x => x.GetType() == typeof(ZPanel));
		AssertEquals("Control count", layoutPanel.IncludedControls.Count, dynamicLayoutPanel.Controls.Count - additionalColumnAmmount);

		foreach (var controlName in layoutPanel.IncludedControls.Select(x => x.ControlName))
		{
			Assert($"Control {controlName} should exist in layout", dynamicLayoutPanel.Controls.ContainsKey(controlName));
			AssertEquals($"Control {controlName} should be visible", testData.VisibleControls.Contains(controlName), dynamicLayoutPanel.FindSingle<Control>(controlName).Visible);
		}
	}

	void AssertGridColumns(TestData testData, ZGrid grid)
	{
		if (testData.OrderedColumnDetails != null)
		{
			AssertSequencesEqual(testData.OrderedColumnDetails, grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
		}
	}

	abstract class TestData
	{
		public TestData(BusinessObject parent)
		{
			Parent = parent;
		}
		public abstract string BindingString { get; }

		public BusinessObject Parent { get; }

		public virtual string MessageType => JobMessageTypeList.Codes.Import;

		public virtual string[] OrderedColumnDetails => new[]
		{
			PreviousDocument.Schema.CSI_Code,
			PreviousDocument.Schema.CSI_SubType,
			PreviousDocument.Schema.CSI_ReferenceNumber,
			PreviousDocument.Schema.CSI_LineNo
		};

		public virtual PanelLayout LayoutPanel => new PreviousDocumentsFieldsLayout().Layout;

		public virtual string[] VisibleControls => new []
		{
			nameof(PreviousDocumentsFieldsControlBag.CodeDropEdit),
			nameof(PreviousDocumentsFieldsControlBag.SubTypeDropEdit),
			nameof(PreviousDocumentsFieldsControlBag.ReferenceTextBox),
			nameof(PreviousDocumentsFieldsControlBag.LineNoCalcEdit)
		};

		public abstract void AddNewPreviousDocument();
	}

	class InvoiceLineTestData_Import : TestData
	{
		public InvoiceLineTestData_Import(BusinessObject parent) : base(parent)
		{
		}

		public override string BindingString => nameof(JobDeclaration.FilteredInvoiceLines);

		public override void AddNewPreviousDocument() => (Parent as JobDeclaration).Invoices.AddNew().InvoiceLines.AddNew().PreviousDocuments.AddNew();
	}

	sealed class InvoiceLineTestData_Export : InvoiceLineTestData_Import
	{
		public InvoiceLineTestData_Export(BusinessObject parent) : base(parent)
		{
		}

		public override string MessageType => JobMessageTypeList.Codes.Export;

		public override string[] OrderedColumnDetails => new[]
		{
			PreviousDocument.Schema.CSI_Code,
			PreviousDocument.Schema.CSI_ReferenceNumber,
			PreviousDocument.Schema.CSI_LineNo,
			PreviousDocument.Schema.CSI_UnitOfQuantity,
			PreviousDocument.Schema.CSI_Quantity,
			PreviousDocument.Schema.CSI_PackType,
			PreviousDocument.Schema.CSI_PackQty,
			PreviousDocument.Schema.CSI_ReferenceNumber2,
		};

		public override string[] VisibleControls => new[]
		{
			nameof(PreviousDocumentsFieldsControlBag.CodeDropEdit),
			nameof(PreviousDocumentsFieldsControlBag.ReferenceTextBox),
			nameof(PreviousDocumentsFieldsControlBag.LineNoCalcEdit),
			nameof(PreviousDocumentsFieldsControlBag.QuantityCalcDropEdit),
			nameof(PreviousDocumentsFieldsControlBag.PackageQuantityCalcDropEdit),
			nameof(PreviousDocumentsFieldsControlBag.Quantity2CalcDropEdit),
			nameof(PreviousDocumentsFieldsControlBag.Reference2TextBox)
		};
	}

	class InvoiceHeaderTestData_Import : TestData
	{
		public InvoiceHeaderTestData_Import(BusinessObject parent) : base(parent)
		{
		}

		public override string BindingString => nameof(JobDeclaration.Invoices);

		public override void AddNewPreviousDocument() => (Parent as JobDeclaration).Invoices.AddNew().PreviousDocuments.AddNew();
	}

	sealed class InvoiceHeaderTestData_Export : InvoiceHeaderTestData_Import
	{
		public InvoiceHeaderTestData_Export(BusinessObject parent) : base(parent)
		{
		}

		public override string MessageType => JobMessageTypeList.Codes.Export;

		public override string[] OrderedColumnDetails => new[]
		{
			PreviousDocument.Schema.CSI_Code,
			PreviousDocument.Schema.CSI_ReferenceNumber,
			PreviousDocument.Schema.CSI_LineNo
		};

		public override string[] VisibleControls => new[]
		{
			nameof(PreviousDocumentsFieldsControlBag.CodeDropEdit),
			nameof(PreviousDocumentsFieldsControlBag.ReferenceTextBox),
			nameof(PreviousDocumentsFieldsControlBag.LineNoCalcEdit)
		};
	}

	class JobDeclarationTestData_Import : TestData
	{
		public JobDeclarationTestData_Import(BusinessObject parent) : base(parent)
		{
		}

		public override string BindingString => string.Empty;

		public override void AddNewPreviousDocument() => (Parent as JobDeclaration).PreviousDocuments.AddNew();
	}

	sealed class JobDeclarationTestData_Export : JobDeclarationTestData_Import
	{
		public JobDeclarationTestData_Export(BusinessObject parent) : base(parent)
		{
		}

		public override string MessageType => JobMessageTypeList.Codes.Export;

		public override string[] OrderedColumnDetails => new[]
		{
			PreviousDocument.Schema.CSI_Code,
			PreviousDocument.Schema.CSI_ReferenceNumber
		};

		public override string[] VisibleControls => new[]
		{
			nameof(PreviousDocumentsFieldsControlBag.CodeDropEdit),
			nameof(PreviousDocumentsFieldsControlBag.ReferenceTextBox)
		};
	}

	class EntryInstructionTestData_Import : TestData
	{
		public EntryInstructionTestData_Import(BusinessObject parent) : base(parent)
		{
		}

		public override string BindingString => nameof(JobDeclaration.CustomsEntryInstructions);

		public override void AddNewPreviousDocument() => (Parent as JobDeclaration).CustomsEntryInstructions.AddNew().PreviousDocuments.AddNew();
	}

	sealed class EntryInstructionTestData_Export : EntryInstructionTestData_Import
	{
		public EntryInstructionTestData_Export(BusinessObject parent) : base(parent)
		{
		}

		public override string MessageType => JobMessageTypeList.Codes.Export;

		public override string[] OrderedColumnDetails => new[]
		{
			PreviousDocument.Schema.CSI_Code,
			PreviousDocument.Schema.CSI_ReferenceNumber
		};

		public override string[] VisibleControls => new[]
		{
			nameof(PreviousDocumentsFieldsControlBag.CodeDropEdit),
			nameof(PreviousDocumentsFieldsControlBag.ReferenceTextBox)
		};
	}
}
