using System;
using System.Collections;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	public class ShipmentsOnConsolLimitHelper : CollectionLimitHelperForPotentialHVLV
	{
		public ShipmentsOnConsolLimitHelper(CommonConsol consol)
			: base(consol)
		{
			this.consol = consol;
		}

		readonly CommonConsol consol;

		protected override IList Collection
		{
			get { return consol.Shipments; }
		}

		protected override int Limit
		{
			get { return FreightDataRegistry.Instance.ShipmentsPerConsolLimit.Value; }
		}

		protected override DateTime LimitIntroductionTimeUtc
		{
			get { return FreightDataRegistry.Instance.ShipmentsPerConsolLimitIntroductionTimeUTC.Value; }
		}

		protected override ZDateTime ParentCreationTimeUtc
		{
			get { return consol.JK_SystemCreateTimeUtc; }
		}

		protected override string ChildPlural
		{
			get { return Res.GetString("ab6c63c0-077e-49f2-ad2b-e2fc9c4c29d0", "Shipments"); }
		}

		protected override string ChildSingular
		{
			get { return Res.GetString("de9481ab-63a2-4bdc-8be7-ea72014ec081", "Shipment"); }
		}

		protected override string ParentSingular
		{
			get { return Res.GetString("edf2d64e-5aa4-4b84-ac10-a0678df8c215", "Consol"); }
		}
	}
}
