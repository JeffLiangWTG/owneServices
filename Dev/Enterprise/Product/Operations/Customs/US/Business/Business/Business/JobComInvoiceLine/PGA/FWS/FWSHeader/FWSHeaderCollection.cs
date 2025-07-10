using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class FWSHeaderCollection : DependentCusAddInfoCollection<FWSHeader, BusinessObject>,
		IPGADataCorrectionCollection
	{
		public FWSHeaderCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USFWSHeader)
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
			var newElement = (FWSHeader)child;
			var invoiceLine = Master as JobComInvoiceLine;
			if (invoiceLine != null && !invoiceLine.IsDeleted && invoiceLine.IsImport)
			{
				newElement.US_OA_FWSExporterAddress = invoiceLine.JI_OA_ExporterAddress;
				var declaration = invoiceLine.Declaration;
				if (declaration != null)
				{
					newElement.FWSImporterOrgPK = declaration.IOROrgPK;
					newElement.US_FIRMS = declaration.US_InspecFirms;
				}

				using (declaration != null ? declaration.SuspendMarkApportionmentDirty() : null)
				{
					newElement.US_InvCurrPGAValue = TotalFWSValueRemainingIncludingChildLines(invoiceLine);
				}
			}
		}

		public ZDecimal TotalInvCurrPGAValue
		{
			get
			{
				ZDecimal result = 0m;
				foreach (FWSHeader element in this.Where(x => !x.IsDeletedOrBeingDeleted()))
				{
					result += element.US_InvCurrPGAValue;
				}
				return result;
			}
		}

		public ZDecimal TotalUSDValue
		{
			get
			{
				ZDecimal result = 0m;
				foreach (FWSHeader element in this.Where(x => !x.IsDeletedOrBeingDeleted()))
				{
					result += element.US_Value;
				}
				return result;
			}
		}

		ZDecimal TotalFWSValueRemainingIncludingChildLines(JobComInvoiceLine invoiceLine)
		{
			var line = invoiceLine.ParentTariffLine is JobComInvoiceLine parentTariffLine ? parentTariffLine : invoiceLine;
			ZDecimal valueRemaining = line.TotalLinePriceIncludingChildLines - line.TotalFWSValueIncludingChildLines;
			return valueRemaining > ZDecimal.Zero ? valueRemaining : ZDecimal.Zero;
		}

		IEnumerable<IPGADataCorrection> IPGADataCorrectionCollection.CorrectionItems => this.Cast<IPGADataCorrection>();
	}
}
