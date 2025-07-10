using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business
{
	public class ACEBIRDDDTCDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDDDTCDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return null; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_DDTCIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "DDTC"; }
		}

		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			foreach (var block in pgaBlocks)
			{
				var pg14Block = block as AEPAPG14;
				if (pg14Block != null)
				{
					if (pg14Block.LPCOType == "DD1")
					{
						invoiceLine.US_DDTCRegistrationNo = pg14Block.LPCONumberorName;
					}
					else
					{
						invoiceLine.US_DDTCLicenseType = pg14Block.LPCOType;
						invoiceLine.US_DDTCLicenseNo = pg14Block.LPCONumberorName;
						invoiceLine.US_DDTCExemptionCode = pg14Block.ExemptionCode;
					}
				}
				else
				{
					var pg30Block = block as AEPAPG30;
					if (pg30Block != null)
					{
						if (!pg30Block.AnticipatedArrivalDate.IsEmpty)
						{
							var arrivalDate = DateTimeParser.GetDateTimeFromZDateAndStringTime(pg30Block.AnticipatedArrivalDate, pg30Block.ArrivalTime);
							invoiceLine.US_DDTCArrivalDate = arrivalDate.IsValid ? arrivalDate : ZDateTime.Empty;
						}
					}
				}
			}
		}
	}
}
