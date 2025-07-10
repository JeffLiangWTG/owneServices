using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class CPSCHeaderCollection : DependentCusAddInfoCollection<CPSCHeader, BusinessObject>, IPGADataCorrectionCollection
	{
		public CPSCHeaderCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USCPSCHeader)
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
			var newElement = child as CPSCHeader;
			var invoiceLine = Master as JobComInvoiceLine;
			if (newElement != null && invoiceLine != null && invoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
			{
				CPSCHeader previousElement = this[Count - 1];
				newElement.CopyPersistentValuesFrom(previousElement);
				newElement.CloneChildren(previousElement);
			}

			if (newElement != null && newElement.US_OA_ManufacturerAddress.IsEmpty && invoiceLine != null && !invoiceLine.IsDeleted)
			{
				newElement.US_OA_ManufacturerAddress = invoiceLine.JI_OA_ManufacturerAddress;
			}
		}

		System.Collections.Generic.IEnumerable<IPGADataCorrection> IPGADataCorrectionCollection.CorrectionItems => this.Cast<IPGADataCorrection>();
	}
}
