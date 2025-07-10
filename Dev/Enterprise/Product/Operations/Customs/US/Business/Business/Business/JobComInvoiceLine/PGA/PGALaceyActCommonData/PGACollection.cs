using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class PGACollection : DependentCusAddInfoCollection<PGA, BusinessObject>, IPGADataCorrectionCollection
	{
		public PGACollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USPGACommon)
		{
		}

		public ZDecimal TotalPGAValue
		{
			get
			{
				ZDecimal result = 0m;
				foreach (PGA element in this.Where(x => !x.IsDeletedOrBeingDeleted()))
				{
					result += element.US_PGALineValue;
				}
				return result;
			}
		}

		public ZDecimal TotalInvCurrPGAValue
		{
			get
			{
				ZDecimal result = 0m;
				foreach (PGA element in this.Where(x => !x.IsDeletedOrBeingDeleted()))
				{
					result += element.US_InvCurrPGAValue;
				}
				return result;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = child as PGA;

			var invoiceLine = Master as JobComInvoiceLine;
			if (newElement != null && invoiceLine != null && !Master.IsDeleted)
			{
				using (invoiceLine.Declaration != null ? invoiceLine.Declaration.SuspendMarkApportionmentDirty() : null)
				{
					newElement.US_InvCurrPGAValue = ValueRemaining(invoiceLine);
				}

				if (invoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
				{
					PGA previousElement = this[Count - 1];
					newElement.CopyPersistentValuesFrom(previousElement);
					newElement.US_InvCurrPGAValue = decimal.Zero;

					foreach (var element in previousElement.PG04ConstituentElements)
					{
						newElement.PG04ConstituentElements.Add((ConstituentElement)element.Clone());
					}

					foreach (var element in previousElement.Licenses)
					{
						newElement.Licenses.Add((License)element.Clone());
					}
				}

				if (newElement.US_CertifyingIndividual.IsEmpty)
				{
					newElement.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
				}
			}
		}

		protected override void OnNonCommittedAdded(BusinessObject bizOAdded)
		{
			base.OnNonCommittedAdded(bizOAdded);

			JobComInvoiceLine invoiceLine = Master as JobComInvoiceLine;
			if (invoiceLine != null && invoiceLine.Declaration != null)
			{
				var pgaAdded = (PGA)bizOAdded;
				if (pgaAdded.US_InvCurrPGAValue > 0)
				{
					invoiceLine.Declaration.MarkApportionmentDirty();
				}
			}
		}

		ZDecimal ValueRemaining(JobComInvoiceLine invoiceLine)
		{
			ZDecimal valueRemaining = invoiceLine.JI_LinePrice - TotalInvCurrPGAValue;
			return valueRemaining > ZDecimal.Zero ? valueRemaining : ZDecimal.Zero;
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
