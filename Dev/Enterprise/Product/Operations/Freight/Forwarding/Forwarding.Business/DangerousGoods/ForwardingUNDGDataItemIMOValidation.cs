using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.DangerousGoods
{
	public class ForwardingUNDGDataItemIMOValidation : ForwardingUNDGDataItemValidation
	{
		public ForwardingUNDGDataItemIMOValidation(AutoUNDGDataItem parent)
			: base(parent)
		{
		}

		protected override void CheckDI_TechnicalName()
		{
			base.CheckDI_TechnicalName();

			if (!Parent.DI_TechnicalName.IsEmpty)
			{
				return;
			}

			var substance = Parent.Substance;
			if (substance is null)
			{
				return;
			}

			if (!substance.DG_TechName.IsEmpty && substance.DG_MP == "Y")
			{
				Parent.DI_TechnicalNameInfo.AddWarning(Res.GetString("6E5DBBCF-89D3-49E7-A5F2-69B0DA2F44BE",
					"Technical Name is required for substances that are marine pollutants, per the IMO IMDG Code. Enter the recognized chemical name of the constituent which most predominantly contributes to the classification as marine pollutant."));
			}

			var imoSpecialProvisions = new ZString[] { "274", "318" };
			if (substance.SpecialProvisions.Any(data => imoSpecialProvisions.Contains(data.DC_Index)))
			{
				Parent.DI_TechnicalNameInfo.AddMessageError(Res.GetString("0C407413-00F8-45A4-8B2E-9430F61B344F",
					"Technical Name is required for the Ocean Booking for substances of the IMO Standard with Special Provision 274 and/or 318, and substances of the CFR Standard with Special Provision 441."));
			}
		}

		protected override void CheckDI_PackageCount()
		{
			base.CheckDI_PackageCount();
			CheckNetExplosiveContent(Parent.DI_PackageCountInfo);
		}

		protected override void CheckDI_F3_NKPackType()
		{
			base.CheckDI_F3_NKPackType();
			CheckNetExplosiveContent(Parent.DI_F3_NKPackTypeInfo);
		}

		protected override void CheckDI_DGWeight()
		{
			base.CheckDI_DGWeight();
			CheckNetExplosiveContent(Parent.DI_DGWeightInfo);
		}

		protected override void CheckDI_UnitOfWeight()
		{
			base.CheckDI_UnitOfWeight();
			CheckNetExplosiveContent(Parent.DI_UnitOfWeightInfo);
		}

		protected override void CheckDI_DGVolume()
		{
			base.CheckDI_DGVolume();
			CheckNetExplosiveContent(Parent.DI_DGVolumeInfo);
		}

		protected override void CheckDI_UnitOfVolume()
		{
			base.CheckDI_UnitOfVolume();
			CheckNetExplosiveContent(Parent.DI_UnitOfVolumeInfo);
		}

		void CheckNetExplosiveContent(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.Value.IsEmpty)
			{
				return;
			}

			var substance = Parent.Substance;
			if (substance is null)
			{
				return;
			}

			if ((substance.DG_State == UNDGSubstanceLookups.StateTypes.Code.ExplosiveArticle || substance.DG_State == UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance) &&
				(Parent.Shipment?.Consols.Cast<ForwardingConsol>().Any(c => c.IsSea) ?? false) &&
				(Parent.DI_PackageCount.IsEmpty || Parent.DI_F3_NKPackType.IsEmpty || (Parent.DI_DGWeight.IsEmpty || Parent.DI_UnitOfWeight.IsEmpty) && (Parent.DI_DGVolume.IsEmpty || Parent.DI_UnitOfVolume.IsEmpty)))
			{
				propertyInfo.AddMessageError(Res.GetString("CAC6EBA4-71C6-40D7-9A85-08848877C893",
					"For explosives, the packs, pack type, net quantity (weight or volume) and unit of measurement of Explosive Content is required for the Ocean Booking."));
			}
		}
	}
}
