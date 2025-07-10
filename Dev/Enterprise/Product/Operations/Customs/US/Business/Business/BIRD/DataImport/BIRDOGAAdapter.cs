using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	public enum OGAType { FDA, FCC, DOT, PGA }

	public class BIRDOGAAdapter
	{
		public void DoImport(JobComInvoiceLine invoiceLine, List<MessageBlock> ogaBlocks, INotifications notifications)
		{
			ZString commercialDesc = ZString.Empty;
			IOGALine ogaLine = null;
			ConstituentElement constituentElement = null;

			foreach (MessageBlock block in ogaBlocks)
			{
				IBIRDOGAStartRecord startRecord = block as IBIRDOGAStartRecord;

				if (startRecord != null)
				{
					commercialDesc = startRecord.CommercialDesc;
					startRecord.SetOGAIndicator(invoiceLine);//OA disclaim
				}
				else
				{
					IBIRDOGALineRecord lineRecord = block as IBIRDOGALineRecord;

					if (lineRecord != null)
					{
						IBIRDOGALineIDRecord lineIDRecord = block as IBIRDOGALineIDRecord;

						if (lineIDRecord != null)
						{
							ogaLine = CreateOGALine(lineIDRecord.OGAType, invoiceLine);
							lineIDRecord.SetOGAIndicator(invoiceLine);//OI declare
						}

						ogaLine.CommercialDesc = commercialDesc;
						constituentElement = null;
						lineRecord.Update(ogaLine, notifications);
					}
					else
					{
						PGA pgaLine = ogaLine as PGA;

						if (pgaLine != null)
						{
							IBIRDPGAConstituentRecord constituentLine = block as IBIRDPGAConstituentRecord;

							if (constituentLine != null)
							{
								constituentElement = pgaLine.PG04ConstituentElements.AddNew();

								constituentLine.Update(constituentElement, notifications);
							}
							else
							{
								IBIRDPGAScientificRecord scientificData = block as IBIRDPGAScientificRecord;
								if (scientificData != null)
								{
									scientificData.Update(constituentElement, notifications);
								}
							}
						}
					}
				}
			}
		}

		IOGALine CreateOGALine(OGAType ogaType, JobComInvoiceLine invoiceLine)
		{
			switch (ogaType)
			{
				case OGAType.FDA:
					return invoiceLine.FDAs.AddNew();
				case OGAType.DOT:
					return invoiceLine.DOTs.AddNew();
				case OGAType.FCC:
					return invoiceLine.FCCs.AddNew();
				case OGAType.PGA:
					return invoiceLine.LaceyActLines.AddNew();

				default:
					return null;
			}
		}
	}
}
