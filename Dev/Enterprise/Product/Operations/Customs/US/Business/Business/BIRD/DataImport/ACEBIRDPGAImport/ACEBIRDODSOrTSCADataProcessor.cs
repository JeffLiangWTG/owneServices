using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business
{
	public abstract class ACEBIRDODSOrTSCADataProcessor : ACEBIRDCommonPGADataProcessor
	{
		protected ACEBIRDODSOrTSCADataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			foreach (var block in pgaBlocks)
			{
				var pg21Block = block as AEPAPG21;
				if (pg21Block != null)
				{
					invoiceLine.US_TSCAODSCertIndividual = PartyTypeList.Codes.Importer;
					invoiceLine.US_FDAContactName = pg21Block.IndividualName;
					invoiceLine.US_FDAContactPhoneNo = pg21Block.TelephoneNumberOfTheIndividual;
					invoiceLine.US_FDAContactEmail = pg21Block.EmailAddressOrFaxNumberForTheIndividual;
				}
				else
				{
					var pg22Block = block as AEPAPG22;
					if (pg22Block != null)
					{
						switch (pg22Block.DeclarationCode)
						{
							case "EP4":
								invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCAPositive;
								break;
							case "EP5":
								invoiceLine.US_TSCACertification = TSCAIndicatorList.Codes.TSCANegative;
								break;
						}
					}
				}
			}
		}
	}

	public class ACEBIRDODSDataProcessor : ACEBIRDODSOrTSCADataProcessor
	{
		public ACEBIRDODSDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_ODSDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_ODSIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "ODS"; }
		}
	}

	public class ACEBIRDTSCADataProcessor : ACEBIRDODSOrTSCADataProcessor
	{
		public ACEBIRDTSCADataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_TSCADisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_TSCAIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "TSCA"; }
		}
	}
}
