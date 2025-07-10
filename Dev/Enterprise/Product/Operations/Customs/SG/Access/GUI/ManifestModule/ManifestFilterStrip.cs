using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Module;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class ManifestFilterStrip : AsycudaFilterStrip
	{
		public ManifestFilterStrip()
			: base(false)
		{
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new ManifestFilterStrip();
	}
}
