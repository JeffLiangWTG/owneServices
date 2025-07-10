using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.VN.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.VN.Manifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm,
			ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new VNManifestLayout();

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
		}
	}
}
