using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeLayout))]
	sealed class ShipmentTypeLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 3;

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.ShipmentTypeControlBag.Instance.EntryStyleDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ServiceLevelCodeFindBox, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.EntrySubStyleDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.EntryDateForDutyDateEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.BankCodeFindBox, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.DutyPaymentTypeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.InspectionClerkTextBox, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.GoodsAtCustomsAreaCheckBox, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.OverTimePaymentCompletedCheckBox, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentTypeLayoutBuilder<Business.Declaration.JobDeclaration>();
	}
}
