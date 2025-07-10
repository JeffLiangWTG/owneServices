using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	[TestedType(typeof(GoodsItemPreviousDocumentLayoutWithGrid))]
	sealed class GoodsItemPreviousDocumentLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(EU.NCTS.GUI.GoodsItemPreviousDocumentsGridUserControl);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.PreviousDocumentLayoutBuilder<Business.NctsPreviousDocument>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.TypeCodeFindBox, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ItemNumberCalcEdit, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.QuantityDropEdit, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.NumOfPackagesDropEdit, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.PreviousDocumentControlBag.Instance.ComplementTextBox, ControlWidthClass.Auto);
				yield return (PreviousDocumentsControlBag.Instance.AmountCalcDropEdit, ControlWidthClass.Auto);
				yield return (PreviousDocumentsControlBag.Instance.CountryCodeFindBox, ControlWidthClass.Auto);
				yield return (PreviousDocumentsControlBag.Instance.PrevDocsTypeDropEdit, ControlWidthClass.Auto);
				yield return (PreviousDocumentsControlBag.Instance.PaymentTypeDropEdit, ControlWidthClass.Auto);
				yield return (PreviousDocumentsControlBag.Instance.NatureOfBussinessDropEdit, ControlWidthClass.Auto);
			}
		}
	}
}
