using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Manifest.GUI;

sealed class MenuBuilder : ASYCUDA.GUI.MenuBuilder
{
	public MenuBuilder(AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
	{
	}

	public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

	public override ResourceString MenuCaption => ResString.GetMultilingualString("FC8CCE23-B8CC-43F0-B088-982BBAC99EB8", "NO Manifest");

	public override ZMenuItem[] BuildMenu() => new ZMenuItem[] { new SendToCustomsMenuItem(Header) };
}
