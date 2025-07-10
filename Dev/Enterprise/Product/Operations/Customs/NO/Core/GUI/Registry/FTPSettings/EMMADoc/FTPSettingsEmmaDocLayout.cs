using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;
using FTPSettingsRegistry = Enterprise.Customs.NO.Registry.FTPSettingsRegistry;

namespace Enterprise.Customs.NO.GUI;

[CodeAlive("This class will be used in the next workflow")]
sealed class FTPSettingsEmmaDocLayout : IPanelLayoutProviderWithExtensions
{
	PanelLayout IPanelLayoutProvider.Layout => layout.Value;

	readonly Lazy<PanelLayout> layout = new(CreateFTPSettingsEmmaDocLayout);

	IReadOnlyCollection<ILayoutExtension> IPanelLayoutProviderWithExtensions.Extensions => new[]
	{
		new PasswordViewerLayoutExtension()
	};

	static PanelLayout CreateFTPSettingsEmmaDocLayout()
	{
		var builder = new FTPSettingsEmmaDocLayoutBuilder<FTPSettingsRegistry>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.UserNameTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.PasswordTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.ViewButton, ControlWidthClass.Long);
		builder.Add(commonBag.UrlAddressTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.PortTextBox, ControlWidthClass.Long);

		return builder.Build();
	}
}
