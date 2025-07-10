using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class ATFCollection : DependentCusAddInfoCollection<ATF, BusinessObject>, IPGADataCorrectionCollection
	{
		public ATFCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USATF)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = child as ATF;
			if (newElement != null && !newElement.IsExport)
			{
				newElement.US_MunitionsListCategory = MunitionsCategoryList.Codes.I;
				var invoiceLine = Master as JobComInvoiceLine;

				if (invoiceLine != null && !invoiceLine.CopyLastPGADetailsToNewLine && invoiceLine.JI_CustomsUnitQty.Equals(FDABaseUQList.Codes.NO))
				{
					var sumQuantity = ZDecimal.Zero;
					foreach (ATF atfLine in this)
					{
						sumQuantity += atfLine.US_Quantity;
					}
					var remainingQuantity = invoiceLine.JI_CustomsQuantity - sumQuantity;
					if (remainingQuantity > 0)
					{
						newElement.US_Quantity = remainingQuantity;
					}
				}

				if (invoiceLine != null && invoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
				{
					ATF previousElement = this[Count - 1];

					newElement.US_CategoryCode = previousElement.US_CategoryCode;
					newElement.US_Quantity = previousElement.US_Quantity;
					newElement.US_FFLNumber = previousElement.US_FFLNumber;
					newElement.US_FFLExemptionCode = previousElement.US_FFLExemptionCode;
					newElement.US_FELNumber = previousElement.US_FELNumber;
					newElement.US_FELExemptionCode = previousElement.US_FELExemptionCode;
					newElement.US_PermitNumber = previousElement.US_PermitNumber;
					newElement.US_PermitExemptionCode = previousElement.US_PermitExemptionCode;
					newElement.US_AECANumber = previousElement.US_AECANumber;
					newElement.US_AECAExemptionCode = previousElement.US_AECAExemptionCode;
					newElement.US_Model = previousElement.US_Model;
					newElement.US_CaliberGaugeSize = previousElement.US_CaliberGaugeSize;
					newElement.US_BarrelLength = previousElement.US_BarrelLength;
					newElement.US_OverallLength = previousElement.US_OverallLength;
					newElement.US_ExtendedDescription = previousElement.US_ExtendedDescription;
				}
			}
		}

		protected override bool AllowNewCore
		{
			get { return AllowAddNewPGALines && base.AllowNewCore; }
		}

		public bool AllowAddNewPGALines
		{
			get
			{
				if (!fAllowAddNewPGALines.HasValue)
				{
					var invoiceLine = Master as JobComInvoiceLine;
					fAllowAddNewPGALines = invoiceLine?.AllowAddNewLineToPGACollection() ?? true;
				}
				return fAllowAddNewPGALines.Value;
			}
			set
			{
				fAllowAddNewPGALines = value;
				if (value)
				{
					var invoiceLine = Master as JobComInvoiceLine;
					invoiceLine?.Declaration?.UpdatePGADataReplacementUpdateRequired();
				}
			}
		}
		bool? fAllowAddNewPGALines;

		System.Collections.Generic.IEnumerable<IPGADataCorrection> IPGADataCorrectionCollection.CorrectionItems => this.Cast<IPGADataCorrection>();
	}
}
