using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NL.NCTS.Business
{
	public class NctsDepartureHeaderContainer : EU.NCTS.Business.NctsDepartureHeaderContainer
	{
		public NctsDepartureHeaderContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NctsHeader Header => (NctsHeader)base.Header;

		public override ZString BC_Mode
		{
			get => base.BC_Mode;
			set
			{
				var oldValue = BC_Mode;
				base.BC_Mode = value;

				if (!IsCopying && BC_Mode != oldValue)
				{
					Header.Bills.ForEach(x => x.GoodsItems.ForEach(i => i.NotifyChangeOfContainerMode(oldValue, BC_Mode)));
				}
			}
		}
	}
}
