using CargoWise.EntityFramework;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondCargoDescValidation : Customs.Business.CusInBondCargoDescValidation
	{
		public CusInBondCargoDescValidation(CusInBondCargoDesc parent)
			: base(parent)
		{
			isAMSHBREffective = ZZCustomsFunctionality.IsAMSHBREffective;
		}
		readonly bool isAMSHBREffective;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
		}

		protected override void CheckBY_HarmonisedTariff()
		{
			var parent = Parent;
			if (parent.BY_HarmonisedTariff.IsEmpty)
			{
				if (isAMSHBREffective && parent.Bill is CusInBondBill bill2 && BillOfLadingStatusIndicatorList.IsAMSHBR(bill2.B0_BillStatus))
				{
					AddYouHaveNotEnteredMessage(parent.BY_HarmonisedTariffInfo);
				}
				else if (parent.Bill is CusInBondBill bill && bill.IsTariffRequired)
				{
					AddYouHaveNotEnteredMessage(parent.BY_HarmonisedTariffInfo);
				}
			}
			else
			{
				ISFTariffValidator.ValidateFormattedHarmonisedNum(parent.Factory, parent.BY_HarmonisedTariffInfo, parent.BY_HarmonisedTariff, 6);
			}
		}

		void AddYouHaveNotEnteredMessage(ZPropertyInfo info)
		{
			info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(info.HumanReadableName));
		}

		protected override void CheckBY_MarksAndNumbers()
		{
			base.CheckBY_MarksAndNumbers();
			if (IsInventoryRecordValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_MarksAndNumbersInfo);
				ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.BY_MarksAndNumbersInfo);
			}
		}

		protected override void CheckBY_Description()
		{
			base.CheckBY_Description();
			if (IsInventoryRecordValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_DescriptionInfo);
				ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.BY_DescriptionInfo);
			}
		}

		protected override void CheckBY_ManifestUnitCode()
		{
			base.CheckBY_ManifestUnitCode();

			ListValidation.MessageErrorIfInvalidCode(Parent.BY_ManifestUnitCodeInfo);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BY_ManifestUnitCodeInfo);
		}

		protected override void CheckBY_MonetaryValue()
		{
			base.CheckBY_MonetaryValue();

			var parent = Parent;
			if (parent.BY_MonetaryValue.IsEmpty && parent.Bill is CusInBondBill bill && bill.IsMoveHeaderFor62or63)
			{
				AddYouHaveNotEnteredMessage(parent.BY_MonetaryValueInfo);
			}

			if (isAMSHBREffective && parent.Bill is CusInBondBill bill1)
			{
				if (BillOfLadingStatusIndicatorList.IsAMSHBR(bill1.B0_BillStatus))
				{
					var deminimusValue = parent.DeminimusValue;
					if (parent.BY_MonetaryValue > deminimusValue)
					{
						var message = string.Format(deminimusMessage, deminimusValue);
						parent.BY_MonetaryValueInfo.AddMessageError(message);
					}
				}
			}
		}

		const string deminimusMessage = "Value is greater than the current de minimus value allowed (${0:0.##})";

		protected override void CheckBY_GrossWeight()
		{
			base.CheckBY_GrossWeight();

			var parent = Parent;
			if (parent.BY_GrossWeight.IsEmpty && parent.Bill is CusInBondBill bill && bill.IsMoveHeaderFor62or63)
			{
				AddYouHaveNotEnteredMessage(parent.BY_GrossWeightInfo);
			}
		}

		protected override void CheckBY_GrossWeightUnit()
		{
			base.CheckBY_GrossWeightUnit();

			var parent = Parent;

			if (parent.BY_GrossWeightUnit.IsEmpty && parent.Bill is CusInBondBill bill && bill.IsMoveHeaderFor62or63)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BY_GrossWeightUnitInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(parent.BY_GrossWeightUnitInfo);
			}
		}

		bool IsInventoryRecordValidationMode
		{
			get { return Parent.IsInventoryRecordValidationMode; }
		}

		protected new CusInBondCargoDesc Parent
		{
			get { return (CusInBondCargoDesc)base.Parent; }
		}
	}
}
