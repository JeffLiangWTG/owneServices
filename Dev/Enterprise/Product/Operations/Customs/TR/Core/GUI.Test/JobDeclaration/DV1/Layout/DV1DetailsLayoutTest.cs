using System.Collections.Generic;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(DV1DetailsLayout))]
	sealed class DV1DetailsLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EU.GUI.DV1DetailsControlBag.Instance.ContractNumberTextBox, ControlWidthClass.Medium);
				yield return (EU.GUI.DV1DetailsControlBag.Instance.ContractDateDateEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.DV1DetailsControlBag.Instance.RelationshipDropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.DV1DetailsControlBag.Instance.PriceInfluenceDropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.DV1DetailsControlBag.Instance.CloseApproximationDropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.DV1DetailsControlBag.Instance.RestrictionsDropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.DV1DetailsControlBag.Instance.ConsiderationDropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.DV1DetailsControlBag.Instance.RoyalitiesLicenceDropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.DV1DetailsControlBag.Instance.ResaleDropEdit, ControlWidthClass.Medium);
				yield return (DV1DetailsControlBag.Instance.PlaceTextBox, ControlWidthClass.Medium);
				yield return (EU.GUI.DV1DetailsControlBag.Instance.CustomsDecisionNumberTextBox, ControlWidthClass.Medium);
				yield return (DV1DetailsControlBag.Instance.CustomsDecisionDateDateEdit, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EU.GUI.DV1DetailsControlBag.Instance.RelationDetailsTextBox, ControlWidthClass.LongControl);
				yield return (EU.GUI.DV1DetailsControlBag.Instance.RestrictionsConsiderationTextBox, ControlWidthClass.LongControl);
				yield return (EU.GUI.DV1DetailsControlBag.Instance.RoyalitiesLicenceDetailsTextBox, ControlWidthClass.LongControl);
				yield return (EU.GUI.DV1DetailsControlBag.Instance.ResaleDetailsTextBox, ControlWidthClass.LongControl);
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new DV1DetailsLayoutBuilder<JobDeclaration>();
	}
}
