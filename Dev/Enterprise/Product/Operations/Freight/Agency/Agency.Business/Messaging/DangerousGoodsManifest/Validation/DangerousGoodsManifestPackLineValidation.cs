using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class DangerousGoodsManifestPackLineValidation : JobPackLinesValidation
	{
		public DangerousGoodsManifestPackLineValidation(AutoJobPackLines parent) : base(parent)
		{
		}

		protected override void CheckJL_DetailedDescription()
		{
			base.CheckJL_DetailedDescription();

			if (Parent.JL_DetailedDescription.IsEmpty)
			{
				Parent.JL_DetailedDescriptionInfo.AddMessageError(Res.GetString("1D345132-3E99-4CE7-9973-9014C293AFF3", "Detailed Description is required."));
			}
		}

		protected override void CheckJL_PackageCount()
		{
			base.CheckJL_PackageCount();

			if (Parent.JL_PackageCount == 0)
			{
				Parent.JL_PackageCountInfo.AddMessageError(Res.GetString("BEA7FD3A-458E-4803-8DB8-5B93D21CBD94", "Number of packages are required."));
			}
		}

		protected override void CheckJL_ActualWeight()
		{
			base.CheckJL_ActualWeight();

			if (Parent.JL_ActualWeight.IsEmpty)
			{
				Parent.JL_ActualWeightInfo.AddMessageError(Res.GetString("7182FE77-A7C3-444F-AE71-F1F651E486D3", "Weight is required."));
			}
		}

		protected override void CheckJL_F3_NKPackType()
		{
			base.CheckJL_F3_NKPackType();

			if (Parent.JL_F3_NKPackType.IsEmpty)
			{
				Parent.JL_F3_NKPackTypeInfo.AddMessageError(Res.GetString("E170122B-A9CC-455E-927F-AC77FF8CF386", "Package Type is required."));
			}
		}
	}
}
