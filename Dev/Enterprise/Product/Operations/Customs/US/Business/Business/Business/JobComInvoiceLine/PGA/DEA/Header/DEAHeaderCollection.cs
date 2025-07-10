using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class DEAHeaderCollection : DependentCusAddInfoCollection<DEAHeader, BusinessObject>, IPGADataCorrectionCollection
	{
		public DEAHeaderCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USDEAHeader)
		{
			var invoiceLine = master as JobComInvoiceLine;
			if (invoiceLine != null && invoiceLine.IsExport)
			{
				MaxCountValidationEnable(3);
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

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = child as DEAHeader;
			var invoiceLine = Master as JobComInvoiceLine;
			if (newElement != null && invoiceLine != null && invoiceLine.IsImport)
			{
				if (invoiceLine.CopyLastPGADetailsToNewLine && Count > 0)
				{
					DEAHeader previousElement = this[Count - 1];
					newElement.CopyPersistentValuesFrom(previousElement);
					newElement.CloneChildren(previousElement);
				}
				else
				{
					DefaultValuesFromInvoiceLine(newElement);
				}
				Factory.AddFetchHint(Enterprise.ZArchitecture.Schema.CusAddInfoSchema.PK, newElement.PK);
			}
		}

		internal void DefaultValuesFromInvoiceLine(DEAHeader newElement)
		{
			var invoiceLine = Master as JobComInvoiceLine;
			var importerOfRecord = invoiceLine?.Declaration?.IOR;
			if (importerOfRecord != null)
			{
				newElement.US_RegistrationNumber = importerOfRecord.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.DEA, Core.Constants.CountryCodes.UnitedStates).Left(newElement.US_RegistrationNumberInfo.MaxLength);
			}

			if (invoiceLine != null)
			{
				newElement.US_CountryOfShipment = invoiceLine.US_UC_NKCountryOfExport;
			}
		}

		System.Collections.Generic.IEnumerable<IPGADataCorrection> IPGADataCorrectionCollection.CorrectionItems => this.Cast<IPGADataCorrection>();
	}
}
