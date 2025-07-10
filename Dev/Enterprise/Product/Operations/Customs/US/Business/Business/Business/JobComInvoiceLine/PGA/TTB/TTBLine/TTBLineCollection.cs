using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class TTBLineCollection : DependentCusAddInfoCollection<TTBLine, BusinessObject>, IPGADataCorrectionCollection
	{
		public TTBLineCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USTTBLine)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newLine = child as TTBLine;
			var invoiceLine = Master as JobComInvoiceLine;
			if (newLine != null && invoiceLine != null)
			{
				if (invoiceLine.IsImport)
				{
					newLine.DefaultPermitNumberFromIOR(invoiceLine);

					if (invoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
					{
						var previousTTBLine = this[Count - 1];
						newLine.CopyPersistentValuesFrom(previousTTBLine);
						newLine.US_QuantityInPCS = CargoWise.Types.ZDecimal.Zero;

						foreach (var cigar in previousTTBLine.Cigars)
						{
							newLine.Cigars.Add((TTBCigar)cigar.Clone());
						}
						foreach (var permit in previousTTBLine.COLAAndCertificates)
						{
							newLine.COLAAndCertificates.Add((TTBCOLAAndCertificate)permit.Clone());
						}
					}
				}
				else if (invoiceLine.IsExport)
				{
					if (newLine.US_NumberForIRC.IsEmpty)
					{
						var header = invoiceLine.InvoiceHeader;
						if (header != null)
						{
							var org = header.US_USPPI.Organisation;
							if (org != null)
							{
								var tteRegistrationNumber = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.TTEPermitNumber, Core.Constants.CountryCodes.UnitedStates);
								if (tteRegistrationNumber.IsValid)
								{
									newLine.US_NumberForIRC = tteRegistrationNumber;
								}
							}
						}
					}

					var dateOfExport = invoiceLine.US_DateOfExport;
					if (dateOfExport.IsValid && newLine.US_Date.IsEmpty)
					{
						newLine.US_Date = dateOfExport;
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
