using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	sealed class RefCusProcedureUserControlTest : TestCaseWithFactory
	{
		public void TestZZ6_CategoryTextBox()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZTextBox>("ZZ6_CategoryTextBox");
					AssertEquals("Binding", "ZZ6_Category", control.BindTo);
					AssertEquals("Caption", "Category", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestPreviousProcedureCodeTextBox()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZTextBox>("PreviousProcedureCodeTextBox");
					AssertEquals("Binding", "ZZ6_PreviousProcedureCode", control.BindTo);
					AssertEquals("Caption", "Previous Procedure Code", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestCountryOrGroupingCodeTextBox()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZTextBox>("CountryOrGroupingCodeTextBox");
					AssertEquals("Binding", "ZZ6_Group", control.BindTo);
					AssertEquals("Caption", "Group", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestZZ6_DescriptionTextBox()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZTextBox>("ZZ6_DescriptionTextBox");
					AssertEquals("Binding", "ZZ6_Description", control.BindTo);
					AssertEquals("Caption", "Description", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestZZ6_ProcedureCodeTextBox()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZTextBox>("ZZ6_ProcedureCodeTextBox");
					AssertEquals("Binding", "ZZ6_ProcedureCode", control.BindTo);
					AssertEquals("Caption", "Procedure Code", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestConcessionTextBox()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZTextBox>("ConcessionTextBox");
					AssertEquals("Binding", "ZZ6_Concession", control.BindTo);
					AssertEquals("Caption", "Concession", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestDataGroupingTextBox()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZTextBox>("DataGroupingTextBox");
					AssertEquals("Binding", "ZZ6_ZZZ_NKDataGrouping", control.BindTo);
					AssertEquals("Caption", "Data Grouping", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestShipmentTypeTextBox()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZTextBox>("ShipmentTypeTextBox");
					AssertEquals("Binding", "ZZ6_ShipmentType", control.BindTo);
					AssertEquals("Caption", "Shipment Type", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestIntoWarehouseTextBox()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZTextBox>("IntoWarehouseTextBox");
					AssertEquals("Binding", "ZZ6_IntoWarehouse", control.BindTo);
					AssertEquals("Caption", "Into Warehouse", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestOutOfWarehouseTextBox()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZTextBox>("OutOfWarehouseTextBox");
					AssertEquals("Binding", "ZZ6_OutOfWarehouse", control.BindTo);
					AssertEquals("Caption", "Out of Warehouse", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestIntoVATWarehouseTextBox()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZTextBox>("IntoVATWarehouseTextBox");
					AssertEquals("Binding", "ZZ6_IntoVATWarehouse", control.BindTo);
					AssertEquals("Caption", "Into VAT Warehouse", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestOutOfVATWarehouseTextBox()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZTextBox>("OutOfVATWarehouseTextBox");
					AssertEquals("Binding", "ZZ6_OutOfVATWarehouse", control.BindTo);
					AssertEquals("Caption", "Out of VAT Warehouse", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestZZ6_EndDateDateEdit()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZDateEdit>("ZZ6_EndDateDateEdit");
					AssertEquals("Binding", "ZZ6_EndDate_ForDisplay", control.BindTo);
					AssertEquals("Caption", "To", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestZZ6_StartDateDateEdit()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZDateEdit>("ZZ6_StartDateDateEdit");
					AssertEquals("Binding", "ZZ6_StartDate", control.BindTo);
					AssertEquals("Caption", "From", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestCalculateDutyCheckBox()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZCheckBox>("CalculateDutyCheckBox");
					AssertEquals("Binding", "ZZ6_CalculateDuty", control.BindTo);
					AssertEquals("Caption", "Calculate Duty", control.CaptionResourceString.Caption);
				});
			}
		}

		public void TestLandedCostCheckBox()
		{
			using (var userControl = new RefCusProcedureUserControl())
			{
				CombineAssertions(() =>
				{
					var control = userControl.FindSingle<ZCheckBox>("LandedCostCheckBox");
					AssertEquals("Binding", "ZZ6_LandedCost", control.BindTo);
					AssertEquals("Caption", "Landed Cost", control.CaptionResourceString.Caption);
				});
			}
		}
	}
}
