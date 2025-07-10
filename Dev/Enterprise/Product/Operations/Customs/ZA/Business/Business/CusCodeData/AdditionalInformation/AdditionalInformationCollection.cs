using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class AdditionalInformationCollection : Customs.Business.CusCodeDataCollection<AdditionalInformation>
	{
		public AdditionalInformationCollection(CusEntryLine parent)
			: base(parent, CusCodeDataTypeList.Codes.AdditionalInformation)
		{
		}

		public AdditionalInformation this[ZString code]
		{
			get { return this.OfType<AdditionalInformation>().FirstOrDefault(x => x.CY_Code == code); }
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			foreach (var addInfo in this.OfType<AdditionalInformation>().Where(x => !x.HasMultiplePairs && x.CY_Order > ZShort.Zero))
			{
				addInfo.CY_Order = ZShort.Zero;
			}

			((CusEntryLineValidation)((CusEntryLine)Master)?.Validation)?.ValidateDiamondLevyValueAndAmount();
		}
	}
}
