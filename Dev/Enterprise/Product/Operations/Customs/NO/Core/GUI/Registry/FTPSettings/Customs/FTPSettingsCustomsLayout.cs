using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;
using FTPSettingsCustomsRegistry = Enterprise.Customs.NO.Registry.FTPSettingsCustomsRegistry;

namespace Enterprise.Customs.NO.GUI;

[CodeAlive("This class will be used in the next workflow")]
sealed class FTPSettingsCustomsLayout : IPanelLayoutProviderWithExtensions
{
	PanelLayout IPanelLayoutProvider.Layout => layout.Value;

	readonly Lazy<PanelLayout> layout = new(CreateFTPSettingsCustomsLayout);

	IReadOnlyCollection<ILayoutExtension> IPanelLayoutProviderWithExtensions.Extensions => new[]
	{
		new PasswordViewerLayoutExtension()
	};

	static PanelLayout CreateFTPSettingsCustomsLayout()
	{
		var builder = new FTPSettingsCustomsLayoutBuilder<FTPSettingsCustomsRegistry>();
		var commonBag = builder.CommonBag;
		var noBag = FTPSettingsCustomsControlBag.Instance;
		builder.AddControlBag(noBag);

		builder.AddColumn();
		builder.Add(commonBag.UserNameTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.PasswordTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.ViewButton, ControlWidthClass.Long);
		builder.Add(commonBag.UrlAddressTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.PortTextBox, ControlWidthClass.Long);
		builder.Add(noBag.SendToCustomFolderTextBox, ControlWidthClass.Long);
		builder.Add(noBag.ReceiveFromCustomFolderTextBox, ControlWidthClass.Long);

		return builder.Build();
	}
}
