using CargoWise.EntityFramework;
using Enterprise.Customs.Common.SG;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class MultiManifestBillSenderLookups : ZLookups
	{
		public MultiManifestBillSenderLookups(MultiManifestBillSender parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList CycleNumbers => Factory.GetCycleNumbers();

		protected new MultiManifestBillSender Parent => (MultiManifestBillSender)base.Parent;
	}
}
