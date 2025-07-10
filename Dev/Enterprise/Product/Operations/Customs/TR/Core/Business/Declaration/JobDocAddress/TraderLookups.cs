using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class TraderLookups : JobDocAddressLookups
	{
		public TraderLookups(AutoJobDocAddress parent) : base(parent)
		{
		}

		public CodeDescriptionPairList TraderTypeList
		{
			get
			{
				if (addressTypeList == null)
				{
					addressTypeList = new CodeDescriptionPairList();
					addressTypeList.AddPair(DocAddressTypes.Codes.BuyerDocumentaryAddress, ResString.GetMultilingualString("14EB67ED-4246-438A-A405-D7B6668333BC", "Buyer"));
					addressTypeList.AddPair(DocAddressTypes.Codes.SellerDocumentaryAddress, ResString.GetMultilingualString("D608AD1E-8A52-4053-93FA-19A2346137E1", "Seller"));
				}

				return addressTypeList;
			}
		}
		CodeDescriptionPairList addressTypeList;
	}
}
