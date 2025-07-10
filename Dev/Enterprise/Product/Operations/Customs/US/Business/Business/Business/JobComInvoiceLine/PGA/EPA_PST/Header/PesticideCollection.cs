using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class PesticideCollection : DependentCusAddInfoCollection<Pesticide, BusinessObject>, IPGADataCorrectionCollection
	{
		public PesticideCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USPesticide)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusAddInfoSchema.B7_ParentID; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newLine = child as Pesticide;
			var invoiceLine = Master as JobComInvoiceLine;

			if (newLine != null && invoiceLine != null)
			{
				if (invoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
				{
					Pesticide previousPSTLine = this[Count - 1];
					newLine.CopyPersistentValuesFrom(previousPSTLine);
					newLine.US_PSTLabelsSent = CargoWise.Types.ZBool.False;

					foreach (var line in previousPSTLine.PesticideLines)
					{
						newLine.PesticideLines.Add((PesticideLine)line.Clone());
					}
				}

				var remainingNetWeight = new ZWeight(invoiceLine.JI_CustomsQuantity, invoiceLine.JI_CustomsUnitQty);
				if (remainingNetWeight.IsValid)
				{
					foreach (Pesticide pSTLine in this)
					{
						var pstLineWeight = new ZWeight(pSTLine.US_NetWeight, pSTLine.US_WeightUQ);
						if (!pstLineWeight.IsValid)
						{
							remainingNetWeight = ZWeight.Invalid;
							break;
						}

						remainingNetWeight -= pstLineWeight;
					}

					newLine.US_NetWeight = System.Math.Max(0, remainingNetWeight.Amount);
					newLine.US_WeightUQ = remainingNetWeight.Unit;
				}

				newLine.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
				newLine.US_NotifyParty = PartyTypeList.Codes.CustomsBroker;
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
