using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierBuyerLinkToleranceLookups : AutoOrgSupplierBuyerLinkToleranceLookups
	{
		public OrgSupplierBuyerLinkToleranceLookups(AutoOrgSupplierBuyerLinkTolerance parent) : base(parent)
		{
		}

		new OrgSupplierBuyerLinkTolerance Parent => (OrgSupplierBuyerLinkTolerance)base.Parent;

		public CodeDescriptionPairList TransportModes
		{
			get => Factory.GetCachedValue("OrgSupplierBuyerLinkToleranceTransportModes", () =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(Constants.TransportModes.All, Constants.TransportModeDescriptions.All);
				list.AddPair(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air);
				list.AddPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea);
				list.AddPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road);
				list.AddPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail);
				return list;
			});
		}

		public OrgSupplierPartCollection SupplierParts => new OrgSupplierBuyerLinkPartCollection(Factory, Parent.SupplierBuyerLink);
	}
}
