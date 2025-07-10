using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	sealed class ConsignmentHAWB : Consignment, IWriteOffStatus
	{
		internal ConsignmentHAWB(WriteOffOResponseMAWB response, string id, string status, string movementStatus, int msgSequence)
			: base(response, id, status, movementStatus)
		{
			mAWB = response.Mawb;
			if (msgSequence > 0)
			{
				cusHAWB = mAWB.ChildBills
									   .Cast<CusHAWB>()
									   .FirstOrDefault(hawb => hawb.CS_HAWB == id && hawb.CS_ConsignmentNum == msgSequence);
			}

			if (cusHAWB == null)
			{
				cusHAWB = mAWB.ChildBills
									   .Cast<CusHAWB>()
									   .FirstOrDefault(hawb => hawb.CS_HAWB == id);
			}

			this.movementStatus = movementStatus;
		}
		readonly CusMAWB mAWB;
		readonly CusHAWB cusHAWB;
		readonly ZString movementStatus;

		protected override ZString CustomsStatusCore
		{
			get { return cusHAWB != null ? cusHAWB.CS_CustomsStatus : ZString.Empty; }
			set
			{
				if (cusHAWB != null)
				{
					var agency = Response.ResponsibleGovernmentAgency;
					cusHAWB.AgencyMessageBeingProcessed = agency;
					cusHAWB.CS_MsgStatus = LowValueManifestStatusList.Codes.Acknowledgement;
					if (mAWB.IsImport)
					{
						if (value == LowValueConsignmentStatusList.Codes.ConsignmentInError || value == LowValueConsignmentStatusList.Codes.ConsignmentRejected)
						{
							cusHAWB.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.EE;
						}
						else
						{
							cusHAWB.CS_CustomsStatus = TSWStatus.GetCombinedWriteOffStatus;
						}
					}
					else
					{
						cusHAWB.CS_CustomsStatus = value;
					}

					if (!movementStatus.IsEmpty)
					{
						UpdateTranshipmentMovementStatusIfRequired(agency, cusHAWB);
					}
				}
			}
		}

		protected override ZString MessageStatusCore
		{
			get { return cusHAWB != null ? cusHAWB.CS_MsgStatus : ZString.Empty; }
			set
			{
				if (cusHAWB != null)
				{
					cusHAWB.CS_MsgStatus = value;
				}
			}
		}

		protected override ZString HouseBillCore
		{
			get { return cusHAWB != null ? cusHAWB.CS_HAWB : ZString.Empty; }
		}

		protected override string JobNumberCore
		{
			get { return cusHAWB != null ? cusHAWB.ConsignmentReference.ToString() : base.JobNumberCore; }
		}

		#region Write-Off Status

		TSWStatus TSWStatus
		{
			get { return new TSWStatus(this); }
		}

		#region IWriteOffStatus Members

		Declaration.ECIWriteOff.CusEntryHeader IWriteOffStatus.EntryHeader => null;

		ZString IWriteOffStatus.CustomsStatus => cusHAWB.CS_CustomsStatus;

		ZString IWriteOffStatus.GoodsClearanceStatus => Status;

		ZString IWriteOffStatus.CombinedStatus => cusHAWB.CS_CustomsStatus;

		JobDeclaration ITSWStatus.Declaration => null;

		ZString ITSWStatus.ResponseStatus => ZString.Empty;

		ZString ITSWStatus.EnterpriseStatus => Status;

		ZString ITSWStatus.Agency => Response.ResponsibleGovernmentAgency;

		#endregion

		void UpdateTranshipmentMovementStatusIfRequired(string agency, CusHAWB houseBill)
		{
			var hawbTranshipment = houseBill.TranshipmentRequest;
			if (hawbTranshipment != null)
			{
				if (agency == ResponsibleGovernmentAgencyList.Codes.MPIBIO)
				{
					houseBill.CS_BioMovementStatus = movementStatus;
				}
				else if (agency == ResponsibleGovernmentAgencyList.Codes.NZCS)
				{
					houseBill.CS_CustomsMovementStatus = movementStatus;
				}

				hawbTranshipment.C4_Status = TSWStatus.CalculateCombinedMovementStatus(houseBill);
			}
		}

		#endregion
	}
}

// Tested in CREMessageProcessorMAWBTest
// Tested in ICRMessageProcessorMAWBTest
