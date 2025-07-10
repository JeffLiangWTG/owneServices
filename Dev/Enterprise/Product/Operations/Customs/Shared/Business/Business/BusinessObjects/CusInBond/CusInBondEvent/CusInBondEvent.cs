using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public abstract class CusInBondEvent : AutoCusInBondEvent, Integration.Customs.ICusInBondEvent, ICusGoodsLocationTypeSupporter
	{
		protected CusInBondEvent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly TypeDecider TypeDecider = new CusInBondEventTypeDecider();

		[RelatedBusinessObject("Header")]
		public override ZGuid BN_BH
		{
			get => base.BN_BH;
			set => base.BN_BH = value;
		}

		public CusInBondHeader Header => Factory.Load<CusInBondHeader>(BN_BH);

		#region ICusGoodsLocationTypeSupporter

		Type ICusGoodsLocationTypeSupporter.GoodsLocationType => GoodsLocationTypeCore;

		protected virtual Type GoodsLocationTypeCore => typeof(CusGoodsLocation);

		#endregion
	}
}
