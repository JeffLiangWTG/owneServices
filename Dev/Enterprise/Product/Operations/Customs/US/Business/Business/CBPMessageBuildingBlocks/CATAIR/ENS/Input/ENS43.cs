using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS43 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS43, IBIRDLineRecord
	{
		#region IBIRDLineRecord Members

		void IBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			if (PreImportationReviewProgramPIRPRulingsNumber != "INVREQ")
			{
				if (TypeIndicator == PIRPRulingTypeList.Codes.CommercialDescription)
				{
					int lengthAddedToJI_Description = 0;

					if (!invoiceLine.JI_Description.Contains(CommercialDescription, StringComparison.OrdinalIgnoreCase))
					{
						lengthAddedToJI_Description = JobComInvoiceLineSchema.JI_Description.MaxLength - invoiceLine.JI_Description.Length;

						//if there is room for CommercialDescription in JI_Description
						if (lengthAddedToJI_Description > 0)
						{
							invoiceLine.JI_Description += CommercialDescription.SubstringSafe(0, lengthAddedToJI_Description);
						}
					}

					if (!invoiceLine.JI_Description.Contains(CommercialDescription, StringComparison.OrdinalIgnoreCase))
					{
						invoiceLine.JI_ExtraInfoForClassification += CommercialDescription.SubstringSafe(lengthAddedToJI_Description);
					}
				}
				else
				{
					invoiceLine.US_PIRPRulingNo = PreImportationReviewProgramPIRPRulingsNumber;
					invoiceLine.US_PIRPRulingType = TypeIndicator;
				}
			}
		}

		ZString IBIRDTariffRecord.Tariff
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
