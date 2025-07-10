using System.Collections.Generic;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(DV1DetailsLayout))]
sealed class DV1DetailsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (DV1DetailsControlBag.Instance.RelationshipDropEdit, ControlWidthClass.Medium);
			yield return (DV1DetailsControlBag.Instance.PriceInfluenceDropEdit, ControlWidthClass.Medium);
			yield return (DV1DetailsControlBag.Instance.CloseApproximationDropEdit, ControlWidthClass.Medium);
			yield return (DV1DetailsControlBag.Instance.RestrictionsDropEdit, ControlWidthClass.Medium);
			yield return (DV1DetailsControlBag.Instance.ConsiderationDropEdit, ControlWidthClass.Medium);
			yield return (DV1DetailsControlBag.Instance.RoyalitiesLicenceDropEdit, ControlWidthClass.Medium);
			yield return (DV1DetailsControlBag.Instance.ResaleDropEdit, ControlWidthClass.Medium);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (DV1DetailsControlBag.Instance.RelationDetailsTextBox, ControlWidthClass.Long);
			yield return (DV1DetailsControlBag.Instance.RestrictionsConsiderationTextBox, ControlWidthClass.Long);
			yield return (DV1DetailsControlBag.Instance.RoyalitiesLicenceDetailsTextBox, ControlWidthClass.Long);
			yield return (DV1DetailsControlBag.Instance.ResaleDetailsTextBox, ControlWidthClass.Long);
			yield return (DV1DetailsControlBag.Instance.CustomsDecisionNumberTextBox, ControlWidthClass.Long);
			yield return (DV1DetailsControlBag.Instance.ContractNumberTextBox, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (DV1DetailsControlBag.Instance.ContractDateDateEdit, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new DV1DetailsLayoutBuilder<JobDeclaration>();
}
