using System;
using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(ShipmentTypeLayout))]
sealed class ShipmentTypeLayoutTest : LayoutsAbstractTest
{
	public void TestCTStatusIDDropEditVisibility()
	{
		AssertControlVisibility(EU.GUI.ShipmentTypeControlBag.Instance.CTStatusIDDropEdit, _ => true);
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.ShipmentTypeControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
			yield return (ShipmentTypeControlBag.Instance.EntryStyleDropEdit, ControlWidthClass.Long);
			yield return (ShipmentTypeControlBag.Instance.IsHighValueOvrdCheckBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
			yield return (ShipmentTypeControlBag.Instance.BorderTransportMeansDropEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ServiceLevelCodeFindBox, ControlWidthClass.Long);
			yield return (ShipmentTypeControlBag.Instance.CTStatusIDDropEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit, ControlWidthClass.Long);
			yield return (ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
			yield return (ShipmentTypeControlBag.Instance.SecurityDropEdit, ControlWidthClass.Long);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentTypeLayoutBuilder<EU.Business.Declaration.JobDeclaration>();

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
