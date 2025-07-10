using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HCommodity : IN5101HCommodity
	{
		public N5101HCommodity(AsycudaBill bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;
		const string HS = "HS";
		const string SSO = "SSO";
		const string ZZZ = "ZZZ";

		ZString IN5101HCommodity.CargoDescription => bill.GoodsDescription;

		IEnumerable<IClassification> IN5101HCommodity.Classifications
		{
			get
			{
				var top1AsycudaPack = bill.GetPack();
				if (top1AsycudaPack != null)
				{
					var hsCode = top1AsycudaPack.PackedItem?.API_Tariff.KeepAlphanumericCharacters().Left(6) ?? ZString.Empty;
					if (!hsCode.IsEmpty)
					{
						yield return new N5101HClassification(hsCode, HS);
					}

					var dgCode = top1AsycudaPack.UNDGs?.UNDGSubstanceManager.Value.Left(UNDGSubstance.Schema.DG_UNNOMaxLength) ?? ZString.Empty;
					if (!dgCode.IsEmpty && bill.Header is AsycudaManifestHeader header)
					{
						if (header.IsSea)
						{
							yield return new N5101HClassification(dgCode, SSO);
						}
						else if (header.IsAir)
						{
							yield return new N5101HClassification(dgCode, ZZZ);
						}
					}
				}
			}
		}
	}
}
