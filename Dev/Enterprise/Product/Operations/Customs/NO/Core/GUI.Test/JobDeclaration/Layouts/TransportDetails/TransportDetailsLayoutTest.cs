using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing
{
	[TestedType(typeof(TransportDetailsLayout))]
	sealed class TransportDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, ControlWidthClass.Long);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, ControlWidthClass.Medium);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.TransportDetailsFlightUserControl, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.TransportDetailsNationalityUserControl, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.TransportDetailsVoyageUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, ControlWidthClass.Auto);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransportDetailsLayoutBuilder<JobDeclaration>();
	}
}
