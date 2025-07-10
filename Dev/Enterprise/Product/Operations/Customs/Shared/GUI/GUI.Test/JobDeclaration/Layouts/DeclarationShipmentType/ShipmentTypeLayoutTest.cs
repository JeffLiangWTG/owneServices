using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeLayouts))]
	sealed class ShipmentTypeLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentTypeLayoutBuilder<BaseJobDeclaration>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (ShipmentTypeControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.ServiceLevelCodeFindBox, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit, ControlWidthClass.Long);
			}
		}
	}
}
