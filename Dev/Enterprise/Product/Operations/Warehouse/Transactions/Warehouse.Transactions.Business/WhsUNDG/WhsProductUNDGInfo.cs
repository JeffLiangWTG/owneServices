using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public record WhsProductUNDGInfo
	{
		public ZGuid ProductPK;
		public ZGuid DG_PK;
		public ZString DG_Code;
		public ZGuid DCR_PK;
		public ZString DCR_Code;
		public ZString UNDGClass;
		public ZDecimal DG_Weight;
		public ZString DG_WeightUQ;
		public ZDecimal DG_Volume;
		public ZString DG_VolumeUQ;

		public WhsProductUNDGInfo(
			ZGuid productPK,
			ZGuid dgPK,
			ZString dgCode,
			ZGuid dcrPK,
			ZString dcrCode,
			ZString undgClass,
			ZDecimal dgWeight,
			ZString dgWeightUq,
			ZDecimal dgVolume,
			ZString dgVolumeUq)
		{
			ProductPK = productPK;
			DG_PK = dgPK;
			DG_Code = dgCode;
			DCR_PK = dcrPK;
			DCR_Code = dcrCode;
			UNDGClass = undgClass;
			DG_Weight = dgWeight;
			DG_WeightUQ = dgWeightUq;
			DG_Volume = dgVolume;
			DG_VolumeUQ = dgVolumeUq;
		}
	}
}
