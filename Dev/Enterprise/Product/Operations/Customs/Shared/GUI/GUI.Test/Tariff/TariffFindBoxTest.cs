using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class TariffFindBoxTest : Common.GUI.Testing.TariffFindBoxTest
	{
		protected override TestFormWithFindBox GetFormWithValidListBounded()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			return new TestFormTakingInvoiceLine(invoiceLine);
		}

		protected override TestFormWithFindBox GetFormWithInvalidListBounded()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			return new TestFormTakingInvoiceLineBadlyBoundList(invoiceLine);
		}

		protected override TestFormWithFindBox GetDummyForm()
		{
			var dummyObject = DummyEnterpriseBusinessObject.New(Factory);

			return new TestFormTakingDummyBO(dummyObject);
		}

		protected abstract string CorrectBindToListFromInvoiceLine { get; }

		protected abstract string IncorrectBindToListFromInvoiceLine { get; }

		protected static TariffFindBoxTest currentInstance;

		protected override void SetUp()
		{
			base.SetUp();
			currentInstance = this;
		}

		protected override void TearDown()
		{
			currentInstance = null;
			base.TearDown();
		}

		#region Types

		protected class TestFormTakingInvoiceLine : TestFormWithFindBox
		{
			public TestFormTakingInvoiceLine(BusinessObject invoiceLine)
				: base(invoiceLine)
			{
			}

			protected override void InitializeComponent()
			{
				FindBox = TariffFindBoxTest.currentInstance.GetConcreteFindBox();
				SuspendLayout();

				FindBox.Name = "FindBox";
				FindBox.BindTo = "JI_Tariff";
				FindBox.BindToList = TariffFindBoxTest.currentInstance.CorrectBindToListFromInvoiceLine;

				Controls.Add(FindBox);
				ResumeLayout(false);
			}
		}

		protected class TestFormTakingInvoiceLineBadlyBoundList : TestFormWithFindBox
		{
			public TestFormTakingInvoiceLineBadlyBoundList(BusinessObject invoiceLine)
				: base(invoiceLine)
			{
			}

			protected override void InitializeComponent()
			{
				FindBox = TariffFindBoxTest.currentInstance.GetConcreteFindBox();
				SuspendLayout();

				FindBox.Name = "FindBox";
				FindBox.BindTo = "JI_Tariff";
				FindBox.BindToList = TariffFindBoxTest.currentInstance.IncorrectBindToListFromInvoiceLine;

				Controls.Add(FindBox);
				ResumeLayout(false);
			}
		}

		protected class TestFormTakingDummyBO : TestFormWithFindBox
		{
			public TestFormTakingDummyBO(BusinessObject bO)
				: base(bO)
			{
			}

			protected override void InitializeComponent()
			{
				FindBox = TariffFindBoxTest.currentInstance.GetConcreteFindBox();
				SuspendLayout();

				FindBox.Name = "FindBox";
				FindBox.BindTo = "Z0_VarCharMax";
				FindBox.BindToList = "Lookups.DummyList";

				Controls.Add(FindBox);
				ResumeLayout(false);
			}
		}

		#endregion
	}
}
