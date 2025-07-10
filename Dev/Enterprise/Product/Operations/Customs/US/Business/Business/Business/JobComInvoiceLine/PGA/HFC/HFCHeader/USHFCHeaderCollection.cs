using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Business
{
	public class USHFCHeaderCollection : DependentCusAddInfoCollection<USHFCHeader, BusinessObject>, IPGADataCorrectionCollection
	{
		public USHFCHeaderCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USHFCHeader)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newLine = child as USHFCHeader;
			var invoiceLine = Master as JobComInvoiceLine;

			if (newLine != null && invoiceLine != null)
			{
				if (invoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
				{
					var previousHFCHeader = this[Count - 1];
					newLine.CopyPersistentValuesFrom(previousHFCHeader);
					newLine.US_HFCImageSent = CargoWise.Types.ZBool.False;

					foreach (var line in previousHFCHeader.USHFCDetails)
					{
						newLine.USHFCDetails.Add((USHFCDetail)line.Clone());
					}
				}

				var remainingNetWeight = new ZWeight(invoiceLine.JI_CustomsQuantity, invoiceLine.JI_CustomsUnitQty);
				if (remainingNetWeight.IsValid)
				{
					foreach (USHFCHeader hfcHeader in this)
					{
						var hfcHeaderWeight = new ZWeight(hfcHeader.US_NetWeight, Core.Constants.Weight.Kilograms);
						if (!hfcHeaderWeight.IsValid)
						{
							remainingNetWeight = ZWeight.Invalid;
							break;
						}

						remainingNetWeight -= hfcHeaderWeight;
					}

					newLine.US_NetWeight = System.Math.Max(0, remainingNetWeight.Amount);
				}

				newLine.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
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
