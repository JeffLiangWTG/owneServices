using System.Collections.Generic;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(SumARegisterDetailsHeaderLayout))]
sealed class SumARegisterDetailsHeaderLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (NoBag.GoodsNumberUserControl, ControlWidthClass.Long);
			yield return (CommonBag.PreviousReferenceTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.StatusDropEdit, ControlWidthClass.Long);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonBag.ArrvialDateEdit, ControlWidthClass.Long);
			yield return (CommonBag.PreviousReferenceNumberTextBox, ControlWidthClass.Long);
			yield return (NoBag.TrasportMeansUserControl, ControlWidthClass.Auto);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (NoBag.UnloadingRemarksLabel, ControlWidthClass.Long);
			yield return (NoBag.UnloadingRemarksTextBox, ControlWidthClass.Long);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new DetailsHeaderCommonLayoutBuilder<CusTempStorageRegHeader>();

	static DetailsHeaderControlBag CommonBag => DetailsHeaderControlBag.Instance;

	static SumARegisterDetailsHeaderControlBag NoBag => SumARegisterDetailsHeaderControlBag.Instance;
}

