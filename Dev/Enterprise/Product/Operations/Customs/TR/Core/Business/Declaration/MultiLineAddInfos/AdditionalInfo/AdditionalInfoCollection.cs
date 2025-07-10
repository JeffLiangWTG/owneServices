using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class AdditionalInfoCollection : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection
	{
		public AdditionalInfoCollection(BusinessObject parent) : base(parent)
		{
		}

		public new AdditionalInfo this[int index] => (AdditionalInfo)base[index];

		public new AdditionalInfo AddNew() => (AdditionalInfo)base.AddNew();
	}
}
