using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	sealed class ConsignmentSeaCargo : Consignment, IWriteOffStatus
	{
		internal ConsignmentSeaCargo(WriteOffResponseSeaCargo response, string id, string status, string movementStatus)
		: base(response, id, status, movementStatus)
		{
			oceanBill = response.OceanBill;

			var isEmptyContainerMode = oceanBill.IsEmptyContainerMode;
			Consignment = oceanBill.HouseBills.Cast<CusSCAHouse>().FirstOrDefault(x => isEmptyContainerMode || x.CA_HouseBill == id);

			this.movementStatus = movementStatus;
		}
		readonly CusSCAOceanBill oceanBill;
		internal readonly CusSCAHouse Consignment;
		readonly ZString movementStatus;

		protected override ZString CustomsStatusCore
		{
			get => Consignment?.CA_ShipmentStatus ?? ZString.Empty;
			set
			{
				if (Consignment != null)
				{
					var agency = Response.ResponsibleGovernmentAgency;
					Consignment.AgencyMessageBeingProcessed = agency;
					Consignment.CA_MessageStatus = LowValueManifestStatusList.Codes.Acknowledgement;
					if (oceanBill.IsImport)
					{
						if (value == LowValueConsignmentStatusList.Codes.ConsignmentInError || value == LowValueConsignmentStatusList.Codes.ConsignmentRejected)
						{
							Consignment.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.EE;
						}
						else
						{
							Consignment.CA_ShipmentStatus = TSWStatus.GetCombinedWriteOffStatus;
						}
					}
					else
					{
						Consignment.CA_ShipmentStatus = value;
					}

					if (!movementStatus.IsEmpty)
					{
						UpdateTranshipmentMovementStatusIfRequired(agency, Consignment);
					}
				}
			}
		}

		protected override ZString MessageStatusCore
		{
			get => Consignment?.CA_MessageStatus ?? ZString.Empty;
			set
			{
				if (Consignment != null)
				{
					Consignment.CA_MessageStatus = value;
				}
			}
		}

		protected override ZString HouseBillCore => Consignment?.CA_HouseBill ?? ZString.Empty;

		protected override string JobNumberCore
		{
			get
			{
				var result = ZString.Empty;
				if (Consignment != null)
				{
					var messageReference = oceanBill.CB_MessageReference;
					if (!messageReference.IsEmpty)
					{
						result = string.Join("-", messageReference, Consignment.CA_ConsignmentNum.ToString());
					}
				}
				return result;
			}
		}

		void UpdateTranshipmentMovementStatusIfRequired(string agency, CusSCAHouse houseBill)
		{
			var housebillTranshipment = houseBill.TranshipmentRequest;
			if (housebillTranshipment != null)
			{
				if (agency == ResponsibleGovernmentAgencyList.Codes.MPIBIO)
				{
					houseBill.CA_BioMovementStatus = movementStatus;
				}
				else if (agency == ResponsibleGovernmentAgencyList.Codes.NZCS)
				{
					houseBill.CA_CustomsMovementStatus = movementStatus;
				}

				housebillTranshipment.C4_Status = TSWStatus.CalculateCombinedMovementStatus(houseBill);
			}
		}

		Declaration.ECIWriteOff.CusEntryHeader IWriteOffStatus.EntryHeader => null;

		ZString IWriteOffStatus.CustomsStatus => Consignment?.CA_ShipmentStatus ?? ZString.Empty;

		ZString IWriteOffStatus.GoodsClearanceStatus => Status;

		ZString IWriteOffStatus.CombinedStatus => Consignment?.CA_ShipmentStatus ?? ZString.Empty;

		JobDeclaration ITSWStatus.Declaration => null;

		ZString ITSWStatus.ResponseStatus => ZString.Empty;

		ZString ITSWStatus.EnterpriseStatus => Status;

		ZString ITSWStatus.Agency => Response.ResponsibleGovernmentAgency;

		TSWStatus TSWStatus => tswStatus ?? (tswStatus = new TSWStatus(this));
		TSWStatus tswStatus;
	}
}
