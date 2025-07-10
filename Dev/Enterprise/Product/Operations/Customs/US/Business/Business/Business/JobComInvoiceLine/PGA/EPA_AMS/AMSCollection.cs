using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class AMSCollection : DependentCusAddInfoCollection<AMS, BusinessObject>, IPGADataCorrectionCollection
	{
		public AMSCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USAMS)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newLine = child as AMS;
			var invoiceLine = Master as JobComInvoiceLine;
			if (newLine != null)
			{
				if (invoiceLine != null)
				{
					newLine.US_CommercialDescription = invoiceLine.JI_Description.Left(newLine.AddInfo.US_CommercialDescriptionInfo.MaxLength);
					if (invoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
					{
						AMS previousAMSLine = this[Count - 1];
						newLine.CopyPersistentValuesFrom(previousAMSLine);

						foreach (AMSLine amsLine in previousAMSLine.AMSLines)
						{
							var amsLineCloned = (AMSLine)amsLine.Clone();
							newLine.AMSLines.Add(amsLineCloned);
						}
					}
				}

				if (newLine.US_Program.IsEmpty)
				{
					USCTariff tariff = null;
					var effectiveDate = ZDateTime.Empty;

					if (invoiceLine != null)
					{
						tariff = invoiceLine.ImportTariffForPGA;
						effectiveDate = invoiceLine.EffectiveDateForDutyRate;
					}
					else if (Master is CusClassPartPivot pivot)
					{
						tariff = pivot.ImportTariff;
						effectiveDate = pivot.EffectiveDate;
					}

					if (tariff != null && effectiveDate.IsValid)
					{
						var ruleCodes = USRefTariffDataLoader.GetEffectiveTariffRuleCodes(Factory, tariff.UE_Tariff, effectiveDate);
						var applicableProgramCode = ruleCodes.Intersect(newLine.AddInfoLookups.ProgramList.GetAllCodesZString()).ToArray();
						if (applicableProgramCode.Length == 1)
						{
							newLine.US_Program = applicableProgramCode[0];
						}
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
