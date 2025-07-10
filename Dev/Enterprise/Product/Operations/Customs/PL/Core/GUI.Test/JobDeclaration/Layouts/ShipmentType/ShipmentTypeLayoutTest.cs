using System;
using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.Customs.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ShipmentTypeLayout))]
sealed class ShipmentTypeLayoutTest : LayoutsAbstractTest
{
	public void TestBehaviour()
	{
		CombineAssertions(() =>
		{
			AssertEquals("MessageTypeDropEdit", true, layout.HasBehaviourByBehaviourType(ShipmentTypeControlBag.Instance.MessageTypeDropEdit, typeof(ZDropEditSizeBehaviour)));
			AssertEquals("EntryStyleDropEdit", true, layout.HasBehaviourByBehaviourType(EU.GUI.ShipmentTypeControlBag.Instance.EntryStyleDropEdit, typeof(ZDropEditSizeBehaviour)));
			AssertEquals("TransportModeDropEdit", true, layout.HasBehaviourByBehaviourType(ShipmentTypeControlBag.Instance.TransportModeDropEdit, typeof(ZDropEditSizeBehaviour)));
			AssertEquals("BorderTransportMeansDropEdit", true, layout.HasBehaviourByBehaviourType(EU.GUI.ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, typeof(ZDropEditSizeBehaviour)));
			AssertEquals("ContainerModeDropEdit", true, layout.HasBehaviourByBehaviourType(ShipmentTypeControlBag.Instance.ContainerModeDropEdit, typeof(ZDropEditSizeBehaviour)));
			AssertEquals("CTStatusIDDropEdit", true, layout.HasBehaviourByBehaviourType(EU.GUI.ShipmentTypeControlBag.Instance.CTStatusIDDropEdit, typeof(ZDropEditSizeBehaviour)));
			AssertEquals("SpecificCircumstanceDropEdit", true, layout.HasBehaviourByBehaviourType(EU.GUI.ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, typeof(ZDropEditSizeBehaviour)));
			AssertEquals("ApplicationCodeDropEdit", true, layout.HasBehaviourByBehaviourType(ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit, typeof(ZDropEditSizeBehaviour)));
		});
	}

	public void TestCTStatusIDDropEditVisibility()
	{
		AssertControlVisibility(EU.GUI.ShipmentTypeControlBag.Instance.CTStatusIDDropEdit, _ => true);
	}

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => new[] { FirstColumnControls };

	protected override int ControlBagCount => 2;

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls => new[]
	{
		(Customs.GUI.ShipmentTypeControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long),
		(EU.GUI.ShipmentTypeControlBag.Instance.EntryStyleDropEdit, ControlWidthClass.Long),
		(EU.GUI.ShipmentTypeControlBag.Instance.IsHighValueOvrdCheckBox, ControlWidthClass.Auto),
		(Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long),
		(EU.GUI.ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, ControlWidthClass.Long),
		(Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long),
		(Customs.GUI.ShipmentTypeControlBag.Instance.ServiceLevelCodeFindBox, ControlWidthClass.Long),
		(EU.GUI.ShipmentTypeControlBag.Instance.CTStatusIDDropEdit, ControlWidthClass.Long),
		(EU.GUI.ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, ControlWidthClass.Long),
		(Customs.GUI.ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit, ControlWidthClass.Long)
	};

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentTypeLayoutBuilder();

	void AssertControlVisibility(ControlReference control, Func<JobDeclaration, bool> isVisible)
	{
		CombineAssertions(() =>
		{
			foreach (ICodeDescription msgType in declaration.Lookups.MessageTypeList)
			{
				declaration.JE_MessageType = msgType.Code;
				AssertEquals($"When Declaration is {msgType.Code}, {control.ControlName} visibility",
					isVisible.Invoke(declaration),
					layout.IsVisible(control, declaration));
			}
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		layout = new ShipmentTypeLayout().Layout;
	}

	PanelLayout layout;
	JobDeclaration declaration;
}
