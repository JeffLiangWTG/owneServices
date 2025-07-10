using System.Collections.Generic;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(NOManifestLayout))]
sealed class NOManifestLayoutTest : LayoutsAbstractTest
{
	public void TestVehicleRegistrationAndNationalityUserControlCaption()
	{
		var behaviours = Layout.GetControlBehaviours(NOBag.VehicleRegistrationAndNationalityUserControl);
		AssertEquals("Number of Behaviours", 1, behaviours.Count);
		var behaviorContainer = behaviours.First().Value;
		AssertNotNull("Behavior Container", behaviorContainer);
		AssertNotNull("Behavior", behaviorContainer.ControlBehaviour);
		var updateRegistrationLabelBehavior = behaviorContainer.ControlBehaviour;
		AssertContains("Behavior Type Name", "InternalCustomizableControlBehaviour", updateRegistrationLabelBehavior.GetType().Name);

		var manifestHeader = Factory.New<AsycudaManifestHeader>();

		CombineAssertions(() =>
		{
			using var control = new VehicleRegistrationAndNationalityUserControl();
			control.SetDataBinding(manifestHeader, "");
			var registrationTextBox = control.VehicleRegistrationTextBox;

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Road;
			updateRegistrationLabelBehavior.UpdateBehaviour(control, manifestHeader);
			AssertEquals("When AMA_TransportMode = Road", "Transport ID", GetCaptionForVehicleRegistrationTextBox());

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Rail;
			updateRegistrationLabelBehavior.UpdateBehaviour(control, manifestHeader);
			AssertEquals("When AMA_TransportMode = Rail", "Train No.", GetCaptionForVehicleRegistrationTextBox());

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
			updateRegistrationLabelBehavior.UpdateBehaviour(control, manifestHeader);
			AssertEquals("When AMA_TransportMode = Sea", "IMO Ship No.", GetCaptionForVehicleRegistrationTextBox());

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Air;
			updateRegistrationLabelBehavior.UpdateBehaviour(control, manifestHeader);
			AssertEquals("When AMA_TransportMode = Air", "Aircraft Reg. No.", GetCaptionForVehicleRegistrationTextBox());

			string GetCaptionForVehicleRegistrationTextBox() => registrationTextBox.GetExtension<ILabelCaptionRenderer>().Caption;
		});
	}

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonBag.CountryTextBox, ControlWidthClass.Medium);
			yield return (CommonBag.RegistrationDateEdit, ControlWidthClass.Medium);
			yield return (CommonBag.ManifestTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.NatureDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.AgentTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.TransportModeDropEdit, ControlWidthClass.Long);
			yield return (NOBag.TransportMeansCodeFindBox, ControlWidthClass.Long);
			yield return (NOBag.VehicleRegistrationAndNationalityUserControl, ControlWidthClass.LongNoCaption);
			yield return (NOBag.DriverNameTextBox, ControlWidthClass.Long);
			yield return (NOBag.DriverCommunicationIdTextBox, ControlWidthClass.Long);
			yield return (CommonBag.ContainerModeDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.BuyersConsolidationCheckBox, ControlWidthClass.Long);
			yield return (CommonBag.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
			yield return (CommonBag.EstDepartureDateEdit, ControlWidthClass.Auto);
			yield return (CommonBag.PortOfFirstArrivalCodeFindBox, ControlWidthClass.Long);
			yield return (CommonBag.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
			yield return (CommonBag.EstArrivalDateEdit, ControlWidthClass.Auto);
			yield return (CommonBag.CustomsOfficeDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.DateAtCustomsOfficeDateEdit, ControlWidthClass.Auto);
			yield return (NOBag.ScheduledDateOfArrCustOfficeDateEdit, ControlWidthClass.Medium);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonBag.JobReferenceTextBox, ControlWidthClass.Medium);
			yield return (CommonBag.MessageStatusTextBox, ControlWidthClass.Medium);
			yield return (CommonBag.MessageStatusDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.CustomsStatusDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
			yield return (CommonBag.MasterBOLTextBox, ControlWidthClass.Medium);
			yield return (CommonBag.CarrierAddressControl, ControlWidthClass.Long);
			yield return (NOBag.RepresentativeAddressControl, ControlWidthClass.Long);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (NOBag.MasterBillGroupBox, ControlWidthClass.LongControl);
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ManifestLayoutBuilder<AsycudaManifestHeader>();

	static CommonManifestControlBag CommonBag => CommonManifestControlBag.Instance;
	static NOManifestControlBag NOBag => NOManifestControlBag.Instance;

	PanelLayout Layout => layout ??= ((IPanelLayoutProvider)new NOManifestLayout()).Layout;
	PanelLayout layout;
}
