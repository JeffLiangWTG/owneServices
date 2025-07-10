using System.Collections.Generic;
using Enterprise.Customs.NO.Registry;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(FTPSettingsEmmaDocLayout))]
sealed class FTPSettingsEmmaDocLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
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
			yield return (FTPSettingsControlBag.Instance.UserNameTextBox, ControlWidthClass.Long);
			yield return (FTPSettingsControlBag.Instance.PasswordTextBox, ControlWidthClass.Long);
			yield return (FTPSettingsControlBag.Instance.ViewButton, ControlWidthClass.Long);
			yield return (FTPSettingsControlBag.Instance.UrlAddressTextBox, ControlWidthClass.Long);
			yield return (FTPSettingsControlBag.Instance.PortTextBox, ControlWidthClass.Long);
		}
	}
	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new FTPSettingsEmmaDocLayoutBuilder<FTPSettingsRegistry>();
}
