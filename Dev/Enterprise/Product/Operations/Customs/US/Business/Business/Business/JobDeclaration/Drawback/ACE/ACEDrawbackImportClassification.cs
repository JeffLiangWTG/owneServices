using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class ACEDrawbackImportClassification : IACEDrawbackImportClassification
	{
		public ACEDrawbackImportClassification(ZString tariffNumber, ZString description, ZDecimal quantity, ZString unitOfMeasure, ZDecimal allowableQTY, ZDecimal goodsValuePerUnit, ZDecimal substitutedValuePerUnit)
		{
			this.tariffNumber = tariffNumber;
			this.description = description;
			this.quantity = quantity;
			this.unitOfMeasure = unitOfMeasure;
			this.allowableQTY = allowableQTY;
			this.goodsValuePerUnit = goodsValuePerUnit;
			this.substitutedValuePerUnit = substitutedValuePerUnit;
		}
		readonly ZString tariffNumber;
		readonly ZString description;
		readonly ZDecimal quantity;
		readonly ZString unitOfMeasure;
		readonly ZDecimal allowableQTY;
		readonly ZDecimal goodsValuePerUnit;
		readonly ZDecimal substitutedValuePerUnit;

		#region IACEDrawbackImportClassification Members

		ZString IACEDrawbackImportClassification.HTSNumber
		{
			get { return tariffNumber; }
		}

		ZString IACEDrawbackImportClassification.DescriptionText
		{
			get { return description.Left(50); }
		}

		IACEDrawbackExportQuantityAndUnit IACEDrawbackImportClassification.ExportQuantityAndUnit
		{
			get
			{
				return new ACEDrawbackExportQuantityAndUnit(quantity, unitOfMeasure, allowableQTY, goodsValuePerUnit, substitutedValuePerUnit);
			}
		}

		#endregion

		class ACEDrawbackExportQuantityAndUnit : IACEDrawbackExportQuantityAndUnit
		{
			public ACEDrawbackExportQuantityAndUnit(ZDecimal quantity, ZString unitOfMeasure, ZDecimal allowableQTY, ZDecimal goodsValuePerUnit, ZDecimal substitutedValuePerUnit)
			{
				this.quantity = quantity;
				this.unitOfMeasure = unitOfMeasure;
				this.allowableQTY = allowableQTY;
				this.goodsValuePerUnit = goodsValuePerUnit;
				this.substitutedValuePerUnit = substitutedValuePerUnit;
			}
			readonly ZDecimal quantity;
			readonly ZString unitOfMeasure;
			readonly ZDecimal allowableQTY;
			readonly ZDecimal goodsValuePerUnit;
			readonly ZDecimal substitutedValuePerUnit;

			ZDecimal IACEDrawbackExportQuantityAndUnit.Quantity
			{
				get { return quantity; }
			}

			ZString IACEDrawbackExportQuantityAndUnit.UnitOfMeasure
			{
				get { return unitOfMeasure; }
			}

			ZDecimal IACEDrawbackExportQuantityAndUnit.AllowableQuantity
			{
				get { return allowableQTY; }
			}

			ZDecimal IACEDrawbackExportQuantityAndUnit.GoodsValuePerUnit
			{
				get { return goodsValuePerUnit; }
			}

			ZDecimal IACEDrawbackExportQuantityAndUnit.SubstitutedValuePerUnit
			{
				get { return substitutedValuePerUnit; }
			}
		}
	}
}
