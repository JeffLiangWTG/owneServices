using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.NZ.Registry;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	sealed class CusClassificationUserControlTest : TestCaseWithFactory
	{
		public void TestVisibility()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var control = new CusClassificationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.nZCClassificationFindBox.Visible);
				AssertEquals(true, control.cC_PartsOfClassificationNZCClassFindBox.Visible);
				AssertEquals(true, control.cC_ConcessionCodeCodeFindBox.Visible);

				AssertEquals(false, control.tariffNumFindBox.Visible);
				AssertEquals(false, control.partsOfClassificationFindBox.Visible);
				AssertEquals(false, control.concessionDropEdit.Visible);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var control = new CusClassificationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(false, control.nZCClassificationFindBox.Visible);
				AssertEquals(false, control.cC_PartsOfClassificationNZCClassFindBox.Visible);
				AssertEquals(false, control.cC_ConcessionCodeCodeFindBox.Visible);

				AssertEquals(true, control.tariffNumFindBox.Visible);
				AssertEquals(true, control.partsOfClassificationFindBox.Visible);
				AssertEquals(true, control.concessionDropEdit.Visible);
			}
		}

		[TestDate(2023, 6, 1)]
		public void TestTariffFindBoxEffectiveDate()
		{
			var cusClassification = Factory.New<CusClassification>();
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var control = new CusClassificationUserControl())
			{
				form.Controls.Add(control);
				form.SetDataBinding(cusClassification, ".");
				form.Show();

				AssertEquals(new ZDateTime(2023, 6, 1), control.tariffNumFindBox.GetEffectiveDate.Invoke());
				AssertEquals(new ZDateTime(2023, 6, 1), control.partsOfClassificationFindBox.GetEffectiveDate.Invoke());
			}
		}
	}
}
