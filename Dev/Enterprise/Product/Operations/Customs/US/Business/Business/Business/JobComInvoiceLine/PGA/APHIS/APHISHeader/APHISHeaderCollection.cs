using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class APHISHeaderCollection : DependentCusAddInfoCollection<APHISHeader, BusinessObject>, IPGADataCorrectionCollection
	{
		public APHISHeaderCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USAPHISHeader)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var newLine = child as APHISHeader;
			var invoiceLine = Master as JobComInvoiceLine;

			if (newLine != null && invoiceLine != null)
			{
				if (invoiceLine.Declaration != null)
				{
					newLine.ApplicantOrgPK = invoiceLine.Declaration.IOROrgPK;
				}

				if (!invoiceLine.IsDataImportInProgress)
				{
					if (invoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
					{
						APHISHeader previousLine = this[Count - 1];
						newLine.CopyPersistentValuesFrom(previousLine);
						newLine.US_LineNo = ZInt.Zero;
						newLine.US_IsDocSubmitted = ZBool.False;
						newLine.US_BouquetGroupingNumber = ZString.Empty;

						foreach (APHISRouting routing in previousLine.Routings)
						{
							newLine.Routings.Add((APHISRouting)routing.Clone());
						}
						foreach (APHISSource source in previousLine.Sources)
						{
							newLine.Sources.Add((APHISSource)source.Clone());
						}
					}
					else
					{
						newLine.Inspections.AddNew();
					}
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
