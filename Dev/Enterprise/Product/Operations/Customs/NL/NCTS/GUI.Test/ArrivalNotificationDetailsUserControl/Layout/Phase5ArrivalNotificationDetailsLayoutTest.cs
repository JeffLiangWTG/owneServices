using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing;

[TestedType(typeof(Phase5ArrivalNotificationDetailsLayout))]
sealed class Phase5ArrivalNotificationDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (ArrivalNotificationDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
			yield return (ArrivalNotificationDetailsControlBag.Instance.LocalReferenceNumberTextBox, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.MrnTextBox, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.DestinationCustomsOfficeCodeCodeFindBox, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.ArrivalDateDateTimeOffsetEdit, ControlWidthClass.Medium);
			yield return (ArrivalNotificationDetailsControlBag.Instance.AuthorizationCodeDropEdit, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.NumberCodeFindBox, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.OwnerZGuidFindBox, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Auto);
			yield return (ArrivalNotificationDetailsControlBag.Instance.IncidentFlagDropEdit, ControlWidthClass.Long);
			yield return (ArrivalNotificationDetailsControlBag.Instance.NationalInfoSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (ArrivalNotificationDetailsControlBag.Instance.CommunicationLanguageDropEdit, ControlWidthClass.Long);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (ArrivalNotificationDetailsControlBag.Instance.DestinationTraderDocAddressControl, ControlWidthClass.Auto);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ArrivalNotificationDetailsLayoutBuilder<NctsHeader>();
}
