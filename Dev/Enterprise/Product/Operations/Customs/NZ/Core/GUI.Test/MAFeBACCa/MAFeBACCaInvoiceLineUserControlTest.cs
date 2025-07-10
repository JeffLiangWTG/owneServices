using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.MAFeBACCa.Testing
{
	[TestedType(typeof(ZForm))]
	public class MAFeBACCaInvoiceLineUserControlTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestAllFieldsAreBoundToFilteredInvoiceLinesChildren()
		{
			ZStringBuilder failures = new ZStringBuilder();
			using (MAFeBACCaInvoiceLineUserControl userControl = new MAFeBACCaInvoiceLineUserControl())
			{
				foreach (KeyValuePair<Control, string> bindingMember in userControl.BindingSource.FullBindingMembers)
				{
					string bindTo = bindingMember.Value;
					if (!bindTo.StartsWith("FilteredInvoiceLines."))
					{
						failures.Append("" + bindingMember.Key.Name + " -> " + bindTo);
					}
				}
			}

			Assert("All BindTo values should start with 'FilteredInvoiceLines.'.\r\nFailures:\r\n" + failures.ToStringWithNewLineBetweenAppends(), failures.IsEmpty);
		}

		protected override Form GetFormToBashCore()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0101010101A";
			Factory.Save();
			ZForm form = new ZForm(declaration);
			form.CaptionRenderingEnabled = true;
			form.Size = new System.Drawing.Size(1012, 551);
			MAFeBACCaInvoiceLineUserControl control = new MAFeBACCaInvoiceLineUserControl(declaration);
			control.Dock = DockStyle.Fill;
			form.Controls.Add(control);
			return form;
		}
	}
}
