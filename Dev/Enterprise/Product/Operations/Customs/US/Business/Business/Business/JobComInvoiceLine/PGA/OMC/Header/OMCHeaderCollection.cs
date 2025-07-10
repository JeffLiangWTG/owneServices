using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class OMCHeaderCollection : DependentCusAddInfoCollection<OMCHeader, BusinessObject>, IPGADataCorrectionCollection
	{
		public OMCHeaderCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USOMCHeader)
		{
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

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = child as OMCHeader;
			var invoiceLine = Master as JobComInvoiceLine;
			if (newElement != null && invoiceLine != null && invoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
			{
				OMCHeader previousElement = this[Count - 1];
				newElement.CopyPersistentValuesFrom(previousElement);
				newElement.US_TrackingStatus = ZString.Empty;
				newElement.CloneChildren(previousElement);
			}

			if (newElement != null && invoiceLine != null && !invoiceLine.IsDeleted)
			{
				if (newElement.US_SourceCountry.IsEmpty)
				{
					newElement.US_SourceCountry = invoiceLine.US_UC_NKCountryOfOrigin;
				}

				if (newElement.US_DepartureDate.IsEmpty)
				{
					newElement.US_DepartureDate = invoiceLine.US_DateOfExport;
				}

				if (newElement.US_OA_Exporter.IsEmpty)
				{
					newElement.US_OA_Exporter = invoiceLine.JI_OA_ExporterAddress;
				}

				if (newElement.US_NetWeight.IsEmpty && invoiceLine.JI_CustomsUnitQty == ABIUnitOfMeasureList.Codes.Kilograms)
				{
					var sumNetWeight = this.Cast<OMCHeader>().Sum(x => x.US_NetWeight);
					var residualValue = invoiceLine.JI_CustomsQuantity - sumNetWeight;
					if (residualValue > 0)
					{
						newElement.US_NetWeight = residualValue;
					}
				}
			}
		}

		System.Collections.Generic.IEnumerable<IPGADataCorrection> IPGADataCorrectionCollection.CorrectionItems => this.Cast<IPGADataCorrection>();
	}
}
