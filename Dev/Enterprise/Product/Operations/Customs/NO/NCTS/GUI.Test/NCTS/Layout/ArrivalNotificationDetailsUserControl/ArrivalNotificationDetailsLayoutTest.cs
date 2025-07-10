using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.GUI.Testing;

[TestedType(typeof(ArrivalNotificationDetailsLayout))]
sealed class ArrivalNotificationDetailsLayoutTest : LayoutsAbstractTest
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

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ArrivalNotificationDetailsLayoutBuilder<EU.NCTS.Business.NctsHeader>();

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.LocalReferenceNumberTextBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.MrnTextBox, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.GoodsRegistrationNumberTextBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.DestinationCustomsOfficeCodeCodeFindBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.ArrivalDateDateTimeOffsetEdit, ControlWidthClass.Medium);
			yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.AuthorizationCodeDropEdit, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.NumberCodeFindBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Auto);
			yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.IncidentFlagDropEdit, ControlWidthClass.Long);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.ArrivalNotificationDetailsControlBag.Instance.DestinationTraderDocAddressControl, ControlWidthClass.Auto);
		}
	}
}
