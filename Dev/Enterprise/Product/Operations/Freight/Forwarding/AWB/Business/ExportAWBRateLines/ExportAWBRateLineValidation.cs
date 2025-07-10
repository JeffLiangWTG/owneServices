//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExportAWBRateLineValidation
//
//    This class should be used for overriding validation in AutoExportAWBRateLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBRateLineValidation : AutoExportAWBRateLineValidation
	{
		public ExportAWBRateLineValidation(AutoExportAWBRateLine parent)
			: base(parent)
		{
		}

		public new ExportAWBRateLine Parent
		{
			get { return (ExportAWBRateLine)base.Parent; }
		}

		protected override void CheckER_NatureAndQtyOfGoodsType()
		{
			base.CheckER_NatureAndQtyOfGoodsType();

			MandatoryValidation.CheckEntered(Parent.ER_NatureAndQtyOfGoodsTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ER_NatureAndQtyOfGoodsTypeInfo, Parent.NatureAndQtyOfGoodsTypeList);
		}

		protected override void CheckER_WeightInLBsOrKGs()
		{
			base.CheckER_WeightInLBsOrKGs();
			ListValidation.MessageErrorIfInvalidCode(Parent.ER_WeightInLBsOrKGsInfo, Parent.RateUQList);
			if (Parent.ER_LineCount == 1)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ER_WeightInLBsOrKGsInfo);
			}
		}

		protected override void CheckER_NoOfPiecesOrRCP()
		{
			base.CheckER_NoOfPiecesOrRCP();
			if (!Parent.ER_NoOfPiecesOrRCP.IsNumbersOnlyOrEmpty)
			{
				if (!Parent.ER_NoOfPiecesOrRCP.IsLettersOnlyOrEmpty)
				{
					Parent.ER_NoOfPiecesOrRCPInfo.AddWarning(Res.GetString("650A65A3-8C6F-43CE-AEAC-B4213DE029A3", "RCP should be alphabetical characters only."));
				}

				if (Parent.ER_NoOfPiecesOrRCP.Length != 3)
				{
					Parent.ER_NoOfPiecesOrRCPInfo.AddWarning(Res.GetString("8C5AD5C4-9C95-4401-80E5-909CF50F9837", "RCP should be 3 characters in length."));
				}
			}
		}

		protected override void CheckER_GrossWeight()
		{
			base.CheckER_GrossWeight();
			if (Parent.ER_GrossWeight != Utilities.Round(Parent.ER_GrossWeight, Parent.GetNumberOfDecimals(Parent.ER_GrossWeightInfo)))
			{
				string message = Res.GetString("59127c52-42e6-42d7-a939-bb856636d5de",
					"A new validation has been added to check that gross weight has only 1 decimal value.\r\nThe overridden weight is currently {0} - please re-enter the weight with only 1 decimal.", Parent.ER_GrossWeight);

				Parent.ER_GrossWeightInfo.AddMessageError(message);
			}
		}

		protected override void CheckER_ChargeableWeight()
		{
			base.CheckER_ChargeableWeight();

			ZDecimal fractionalDigits = Parent.ER_ChargeableWeight - Parent.ER_ChargeableWeight.Truncate();
			if (fractionalDigits != 0m && fractionalDigits != 0.5m)
			{
				string awbChargeableRoundingRegistryPath = string.Format(Culture.Invariant, "{0}/{1}", FreightDataRegistry.Instance.AWBRoundings.Category, FreightDataRegistry.Instance.AWBRoundings.Caption);
				string message = Res.GetString("8d32671e-5511-4060-afdf-e5a35a01951d",
					"Chargeable weight should represent the gross weight with 1 decimal value, whereby this decimal is always rounded up to the next 0.5 unit.\r\nPlease refer to registry {0} to set the desired rounding behavior.", awbChargeableRoundingRegistryPath);

				Parent.ER_ChargeableWeightInfo.AddMessageError(message);
			}
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return info.Name != ExportAWBRateLineSchema.Constants.ER_EH && base.ShouldValidateFKToCancelledRecord(info); // remove extra fetch hints
		}
	}
}
