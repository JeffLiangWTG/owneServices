using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class DrawbackNAFTA : AutoDrawbackNAFTA, IDrawbackNAFTATariff, IACEDrawbackNAFATTariff
	{
		public DrawbackNAFTA(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related

		public JobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<JobComInvoiceLine>(B7_ParentID); }
		}

		#endregion

		#region Override Properties

		[List(nameof(AddInfoLookups) + "." + nameof(USDrawbackNAFTAAddInfoLookups.US_NAFTACountryCodeList))]
		public override ZString US_DRWNAFTACountryOfExport
		{
			get { return base.US_DRWNAFTACountryOfExport; }
			set { base.US_DRWNAFTACountryOfExport = value; }
		}

		public override ZDecimal US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty
		{
			get { return base.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty; }
			set
			{
				var oldValue = US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty;
				base.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty = value;
				if (!IsCopying && (InvoiceLine?.ShouldReCalculateDrawbackData ?? false) && oldValue != US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty)
				{
					if (InvoiceLine is ISupportDataImporting dataImportingSupporter && dataImportingSupporter.IsImportingData)
					{
						InvoiceLine.DrawbackNAFTAs.Reload(true);
					}

					InvoiceLine.Claims.DutyClaim.Default_99ClaimedDutyAndCalculatedAmount();
				}
			}
		}

		#endregion

		#region Overrides

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();
			DrawbackNAFTA result = (DrawbackNAFTA)base.CloneInternal(args);
			return result;
		}

		#endregion

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		#region IDrawbackNAFTATariff Members

		public ZString NAFTACountryImportEntry
		{
			get { return US_DRWNAFTACountryImportEntry; }
		}

		public ZDate NAFTACountryImportEntryDate
		{
			get { return US_DRWNAFTACountryImportEntryDate.Date; }
		}

		public ZString NAFTACountryTariffNumber
		{
			get { return US_DRWNAFTACountryTariffNumber; }
		}

		public ZDecimal NAFTACountryDutyRate
		{
			get { return US_DRWNAFTACountryDutyRate; }
		}

		public ZDecimal NAFTACountryImportDuty
		{
			get { return US_DRWNAFTACountryImportDuty; }
		}

		public ZDecimal EquivalentUSDollarAmountOfNAFTACountryDuty
		{
			get { return US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty; }
		}

		#endregion

		#region IACEDrawbackNAFATTariff Members

		ZString IACEDrawbackNAFATTariff.EntryNumber
		{
			get { return US_DRWNAFTACountryImportEntry; }
		}

		ZDateTime IACEDrawbackNAFATTariff.EntryDate
		{
			get { return US_DRWNAFTACountryImportEntryDate; }
		}

		ZDecimal IACEDrawbackNAFATTariff.DutyPaidToForeignGov
		{
			get { return US_DRWNAFTACountryImportDuty; }
		}

		ZDecimal IACEDrawbackNAFATTariff.ExchangeRate
		{
			get { return US_DRWNAFTACountryDutyRate; }
		}

		ZString IACEDrawbackNAFATTariff.TariffNumber1
		{
			get { return US_DRWNAFTACountryTariffNumber; }
		}

		ZString IACEDrawbackNAFATTariff.TariffNumber2
		{
			get { return US_DRWNAFTACountryTariffNumber2; }
		}

		ZString IACEDrawbackNAFATTariff.TariffNumber3
		{
			get { return US_DRWNAFTACountryTariffNumber3; }
		}

		ZString IACEDrawbackNAFATTariff.CountryOfExport
		{
			get { return US_DRWNAFTACountryOfExport; }
		}

		#endregion
	}
}
