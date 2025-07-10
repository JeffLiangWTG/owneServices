using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ReconFlattenedDataLine : AutoReconFlattenedDataLine
	{
		public ReconFlattenedDataLine()
			: base()
		{
		}

		public override ZString EntryNumber
		{
			get { return base.EntryNumber; }
			set
			{
				base.EntryNumber = value.KeepAlphanumericCharacters();
			}
		}

		[BusinessObjectTestExclude]
		public override ZString OriginalTariff
		{
			get { return base.OriginalTariff; }
			set { base.OriginalTariff = TariffFormatter.Format(value); }
		}

		[BusinessObjectTestExclude]
		public override ZString OriginalFormattedTariff
		{
			get { return TariffFormatter.DisplayFormat(OriginalTariff); }
			set { OriginalTariff = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString OriginalSupTariff
		{
			get { return base.OriginalSupTariff; }
			set { base.OriginalSupTariff = TariffFormatter.Format(value); }
		}

		[BusinessObjectTestExclude]
		public override ZString OriginalFormattedSupTariff
		{
			get { return TariffFormatter.DisplayFormat(OriginalSupTariff); }
			set { OriginalSupTariff = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString ReconTariff
		{
			get { return base.ReconTariff; }
			set { base.ReconTariff = TariffFormatter.Format(value); }
		}

		[BusinessObjectTestExclude]
		public override ZString ReconFormattedTariff
		{
			get { return TariffFormatter.DisplayFormat(ReconTariff); }
			set { ReconTariff = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString ReconSupTariff
		{
			get { return base.ReconSupTariff; }
			set { base.ReconSupTariff = TariffFormatter.Format(value); }
		}

		[BusinessObjectTestExclude]
		public override ZString ReconFormattedSupTariff
		{
			get { return TariffFormatter.DisplayFormat(ReconSupTariff); }
			set { ReconSupTariff = value; }
		}

		public ReconFlattenedDataLineLookups Lookups
		{
			get { return lookups ?? (lookups = new ReconFlattenedDataLineLookups(this)); }
		}
		ReconFlattenedDataLineLookups lookups;

		TariffFormatter TariffFormatter
		{
			get
			{
				if (tariffFormatter == null)
				{
					tariffFormatter = new TariffFormatter();
				}
				return tariffFormatter;
			}
		}
		TariffFormatter tariffFormatter;
	}
}
