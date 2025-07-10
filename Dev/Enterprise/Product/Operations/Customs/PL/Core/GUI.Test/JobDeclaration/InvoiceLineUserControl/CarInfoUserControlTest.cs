using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class CarInfoUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		AssertEquals(typeof(JobComInvoiceLine), control.DataSourceType);
	}

	public void TestCarInfoGroupBox_Caption()
	{
		var carInfoGroupBox = control.CarInfoGroupBox;
		CombineAssertions(() =>
		{
			AssertEquals("BindingMember", "Car Information", carInfoGroupBox.CaptionResourceString.Caption);
			AssertType<ZGroupBox>("Type", carInfoGroupBox);
		});
	}

	public void TestCarInfoMarkModelTextBox()
	{
		CombineAssertions(() =>
		{
			var carInfoMarkModelTextBox = control.CarInfoMarkModelTextBox;
			AssertEquals("BindingMember", "JI_MarkModel", carInfoMarkModelTextBox.GetBindingMember());
			AssertType<ZTextBox>("Type", carInfoMarkModelTextBox);
		});
	}

	public void TestCarMakeModelLookUpButton()
	{
		CombineAssertions(() =>
		{
			var carMakeModelLookUpButton = control.CarMakeModelLookUpButton;
			AssertEquals("Text", "...", carMakeModelLookUpButton.Text);
			AssertType<ZButton>("Type", carMakeModelLookUpButton);
		});
	}

	public void TestCarInfoVinTextBox()
	{
		var carInfoVinTextBox = control.CarInfoVinTextBox;
		CombineAssertions(() =>
		{
			AssertEquals("BindingMember", "FirstVehicle+CVH_VehicleIdentificationNumber", carInfoVinTextBox.GetBindingMember());
			AssertType<ZTextBox>("Type", carInfoVinTextBox);
		});
	}

	public void TestCarInfoEngineNoTextBox()
	{
		var carInfoEngineNoTextBox = control.CarInfoEngineNoTextBox;
		CombineAssertions(() =>
		{
			AssertEquals("BindingMember", "FirstVehicle+Engine+CEG_EngineNumber", carInfoEngineNoTextBox.GetBindingMember());
			AssertType<ZTextBox>("Type", carInfoEngineNoTextBox);
		});
	}

	public void TestCarInfoCapacityCalcEdit()
	{
		var carInfoCapacityCalcEdit = control.CarInfoCapacityCalcEdit;
		CombineAssertions(() =>
		{
			AssertEquals("BindingMember", "FirstVehicle+Engine+CEG_CapacityCC", carInfoCapacityCalcEdit.GetBindingMember());
			AssertEquals("MaxValue", 99999m, carInfoCapacityCalcEdit.MaxValue);
			AssertType<ZCalcEdit>("Type", carInfoCapacityCalcEdit);
		});
	}

	public void TestCarInfoCapacityCm3Label()
	{
		var capacityCm3Label = control.CarInfoCapacityCm3Label;
		CombineAssertions(() =>
		{
			AssertEquals("Caption of CarInfoCapacityCm3Label is cm 3", "cm 3", capacityCm3Label.CaptionResourceString.Caption);
			AssertType<ZLabel>("Type", capacityCm3Label);
		});
	}

	public void TestCarInfoFuelTypeDropEdit()
	{
		var carInfoFuelTypeDropEdit = control.CarInfoFuelTypeDropEdit;
		CombineAssertions(() =>
		{
			AssertEquals("BindingMember", "FirstVehicle+Engine+CEG_EngineType", carInfoFuelTypeDropEdit.GetBindingMember());
			AssertType<ZDropEdit>("Type", carInfoFuelTypeDropEdit);
		});
	}

	public void TestCarInfoProductionYearTextBox()
	{
		var carInfoProductionYearTextBox = control.CarInfoProductionYearTextBox;
		CombineAssertions(() =>
		{
			AssertEquals("BindingMember", "FirstVehicle+CVH_ModelYear", carInfoProductionYearTextBox.GetBindingMember());
			AssertType<ZTextBox>("Type", carInfoProductionYearTextBox);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new CarInfoUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	CarInfoUserControl control;
}
