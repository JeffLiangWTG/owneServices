using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestsSubclassesOf(typeof(LayoutCustomsSupplierHeaderUserControl))]
	public abstract class LayoutCustomsSupplierHeaderUserControlAbstractTest<TControl, TDeclaration> : TestCaseWithFactory
		where TControl : LayoutCustomsSupplierHeaderUserControl
		where TDeclaration : BaseJobDeclaration
	{
		public void TestCommercialInvoiceDetailControls()
		{
			var declaration = Factory.New<TDeclaration>();
			declaration.Invoices.AddNew();

			using (var form = new BaseJobDeclarationForm(declaration))
			using (var control = (LayoutCustomsSupplierHeaderUserControl)Activator.CreateInstance(typeof(TControl)))
			{
				form.Controls.Add(control);
				control.SetDataBinding(declaration, "");
				form.Show();

				var bindingSource = control.BindingSource;

				CombineAssertions("Invoice Detail Controls", () =>
				{
					foreach (var name in ExpectedControlList)
					{
						var controlFound = control.Controls.Find(name, true);
						AssertEquals("Should be able to find " + name, true, controlFound.Any());
					}
				});
			}
		}

		protected abstract IEnumerable<string> ExpectedControlList { get; }

		protected static IEnumerable<string> DefaultControlList
		{
			get
			{
				yield return nameof(CommercialInvoiceDetailsUserControl.GroupInvoiceDropEdit);
				yield return nameof(CommercialInvoiceDetailsIncoTermsUserControl.IncoTermExplainButton);
				yield return nameof(CommercialInvoiceDetailsUserControl.GrossWeightCalcDropEdit);
				yield return nameof(CommercialInvoiceDetailsUserControl.NetWeightCalcDropEdit);
				yield return nameof(CommercialInvoiceDetailsUserControl.NoOfPacksCalcDropEdit);
			}
		}
	}
}
