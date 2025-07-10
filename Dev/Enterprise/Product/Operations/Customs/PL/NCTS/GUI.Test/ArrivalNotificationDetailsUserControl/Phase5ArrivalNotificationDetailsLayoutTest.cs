using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

[TestedType(typeof(Phase5ArrivalNotificationDetailsLayout))]
sealed class Phase5ArrivalNotificationDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ArrivalNotificationDetailsLayoutBuilder<Business.NctsHeader>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls => new[]
	{
		(ArrivalNotificationDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto),
		(ArrivalNotificationDetailsControlBag.Instance.LocalReferenceNumberTextBox, ControlWidthClass.Long),
		(ArrivalNotificationDetailsControlBag.Instance.MrnTextBox, ControlWidthClass.Long),
		(ArrivalNotificationDetailsControlBag.Instance.DestinationCustomsOfficeCodeCodeFindBox, ControlWidthClass.Long),
		(ArrivalNotificationDetailsControlBag.Instance.ArrivalDateDateTimeOffsetEdit, ControlWidthClass.Medium),
		(ArrivalNotificationDetailsControlBag.Instance.AuthorizationCodeDropEdit, ControlWidthClass.Long),
		(ArrivalNotificationDetailsControlBag.Instance.NumberCodeFindBox, ControlWidthClass.Long),
		(ArrivalNotificationDetailsControlBag.Instance.GoodsLocationFromAuthorizationCodeFindBox, ControlWidthClass.Long),
		(ArrivalNotificationDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Auto),
		(ArrivalNotificationDetailsControlBag.Instance.IncidentFlagDropEdit, ControlWidthClass.Long)
	};

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls => new[]
	{
		(ArrivalNotificationDetailsControlBag.Instance.DestinationTraderDocAddressControl, ControlWidthClass.Auto),
		(Phase5ArrivalNotificationDetailsControlBag.Instance.RepresentativeTraderGuidFindBox, ControlWidthClass.Auto)
	};
}

