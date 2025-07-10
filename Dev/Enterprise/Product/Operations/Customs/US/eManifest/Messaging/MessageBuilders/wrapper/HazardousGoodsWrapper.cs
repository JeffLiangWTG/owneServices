using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders.eManifest;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	internal class HazardousGoodsWrapper : IHazardousGoods
	{
		public HazardousGoodsWrapper(UNDGDataItem hazmat)
		{
			this.hazmat = hazmat;
			contact = hazmat.DGContact;
		}

		#region Implementation of IHazardousGoods

		public ZString HazardousGoodsCode
		{
			get { return hazmat.UNDGSubstance is UNDGSubstance substance ? substance.DG_UNNO : ZString.Empty; }
		}

		public ZString HazardousGoodsSpecialInstructions
		{
			get { return ZString.Empty; } //NOTE: Not required for US.
		}

		public ZString HazardousGoodsContactName
		{
			get { return contact == null ? ZString.Empty : contact.OC_ContactName; }
		}

		public ZString HazardousGoodsContactPhone
		{
			get { return contact == null ? ZString.Empty : contact.OC_Phone.KeepNumericCharacters(); }
		}

		#endregion

		readonly UNDGDataItem hazmat;
		readonly OrgContact contact;
	}
}
