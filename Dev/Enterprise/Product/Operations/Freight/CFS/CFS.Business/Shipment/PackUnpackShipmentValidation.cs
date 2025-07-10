using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core;

namespace Enterprise.Freight.CFS.Business
{
	public class PackUnpackShipmentValidation : CFSShipmentValidation
	{
		public PackUnpackShipmentValidation(PackUnpackShipment parent) : base(parent)
		{
		}

		protected override void CheckJS_F3_NKPackType()
		{
			base.CheckJS_F3_NKPackType();

			if (!Parent.JS_F3_NKPackTypeInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.JS_F3_NKPackTypeInfo);
			}
		}

		protected override void CheckJS_ActualVolume()
		{
			base.CheckJS_ActualVolume();

			if (!Parent.JS_ActualVolumeInfo.HasErrors() && !Parent.JS_ActualVolumeInfo.HasWarnings() && !Parent.JS_ActualVolumeInfo.HasNotifications())
			{
				if (Parent.ValidateTotalsAgainstPackLines && Parent.OuterPackLines.Count > 0 && Parent.TotalOuterPacksVolume != Parent.JS_ActualVolume)
				{
					Parent.JS_ActualVolumeInfo.AddWarning(Res.GetString("0c9ffe93-d581-42a2-9484-5be621879ce1", "Entered volume does not match total volume of the packlines."));
				}
			}
		}

		protected override void CheckJS_ActualWeight()
		{
			base.CheckJS_ActualWeight();

			if (!Parent.JS_ActualWeightInfo.HasErrors() && !Parent.JS_ActualWeightInfo.HasWarnings() && !Parent.JS_ActualWeightInfo.HasNotifications())
			{
				if (Parent.ValidateTotalsAgainstPackLines && Parent.OuterPackLines.Count > 0 && Parent.TotalOuterPacksWeight != Parent.JS_ActualWeight)
				{
					Parent.JS_ActualWeightInfo.AddWarning(Res.GetString("2fd954f8-23bf-4778-b221-95c97572661a", "Entered weight does not match total weight of the packlines."));
				}
			}
		}

		protected override void CheckJS_HouseBill()
		{
			base.CheckJS_HouseBill();

			if (!Parent.JS_HouseBillInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.JS_HouseBillInfo);
			}
		}

		protected override void CheckJS_OuterPacks()
		{
			base.CheckJS_OuterPacks();

			if (!Parent.JS_OuterPacksInfo.HasErrors())
			{
				if (!Parent.AllowSurplusPacks)
				{
					MandatoryValidation.WarnIfNotEntered(Parent.JS_OuterPacksInfo);
				}

				if (!Parent.JS_OuterPacksInfo.HasErrors())
				{
					CompareValidation.CheckNumberNotNegative(Parent.JS_OuterPacksInfo);
				}

				if (!Parent.JS_OuterPacksInfo.HasNotifications() && Parent.ValidateTotalsAgainstPackLines && Parent.JS_OuterPacks != Parent.TotalOuterPacks)
				{
					Parent.JS_OuterPacksInfo.AddWarning(Res.GetString("fb20bf79-946b-4aa1-8a4d-24c5e2fdf49f", "Entered number of packs does not equal the total number in the Pack Lines."));
				}
			}

			if (Parent.CustomsOutturnListener != null && Parent.CustomsOutturnListener.NumberOfPackages != Parent.JS_OuterPacks)
			{
				Parent.JS_OuterPacksInfo.AddWarning(Res.GetString("00d9dee3-8a35-496c-b1f4-393dddd520a0", "The package count is different from the outturn's that this pack line is linked to. The outturn pack count is {0}.", Parent.CustomsOutturnListener.NumberOfPackages));
			}
		}

		protected override void CheckJS_UnitOfVolume()
		{
			base.CheckJS_UnitOfVolume();

			if (!Parent.JS_UnitOfVolumeInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.JS_UnitOfVolumeInfo);
			}
		}

		protected override void CheckJS_PackingMode()
		{
			if (Parent.JS_PackingMode == Constants.ContainerModes.BuyersConsol)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JS_PackingModeInfo, Parent.Lookups.JS_PackingMode_List);
			}
			else
			{
				base.CheckJS_PackingMode();
			}
		}

		protected override void CheckJS_MarksAndNumbers()
		{
			base.CheckJS_MarksAndNumbers();
			if (!Parent.JS_MarksAndNumbersInfo.HasErrors() && Parent.JS_OuterPacks == 0)
			{
				MandatoryValidation.CheckEntered(Parent.JS_MarksAndNumbersInfo);
			}
		}

		#region Implementation

		public new PackUnpackShipment Parent
		{
			get { return (PackUnpackShipment)base.Parent; }
		}

		#endregion
	}
}
