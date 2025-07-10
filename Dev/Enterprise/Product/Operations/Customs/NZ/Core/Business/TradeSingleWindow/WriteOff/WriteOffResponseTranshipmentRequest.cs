using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	sealed class WriteOffResponseTranshipmentRequest : WriteOffResponse, IMovementStatus
	{
		internal WriteOffResponseTranshipmentRequest(BaseTSWResponse response)
			: base(response)
		{
			this.response = response;
		}
		readonly BaseTSWResponse response;

		public TranshipmentRequest TranshipmentRequest
		{
			get { return (TranshipmentRequest)LinkedObject; }
		}

		CusMAWB CusMawb => Factory.Load<CusMAWB>(TranshipmentRequest.C4_ParentID);

		CusSCAContainer Container => Factory.Load<CusSCAContainer>(TranshipmentRequest.C4_ParentID);

		public bool IsAirCargo => CusMawb != null;

		bool isSeaCargo => Container != null;

		#region Overrides

		public override bool ShouldShowSummaryOnReport => false;

		protected override IEnumerable<ZString> ConsignmentIDs
		{
			get
			{
				var result = Enumerable.Empty<ZString>();
				if (IsAirCargo)
				{
					result = CusMawb.ChildBills.Cast<CusHAWB>()?.Select(hawb => hawb.CS_HAWB);
				}
				else if (isSeaCargo)
				{
					result = Container.PackingLines.Cast<CusSCAPackingLine>().Where(x => x.HouseBill != null).Select(x => x.HouseBill.CA_HouseBill);
				}

				return result;
			}
		}

		protected override ZString CustomsDeliveryInstructionsCore
		{
			get { return CusMawb?.CM_CustomsDeliveryInstructions ?? ZString.Empty; }
			set
			{
				if (IsAirCargo)
				{
					CusMawb.CM_CustomsDeliveryInstructions = value;
				}
			}
		}

		protected override ZString MasterBillCore
		{
			get
			{
				var result = ZString.Empty;

				if (IsAirCargo)
				{
					result = CusMawb.FormattedMasterBill;
				}
				else if (isSeaCargo)
				{
					result = Container.OceanBill.CB_OceanBill;
				}

				return result;
			}
		}

		protected override Consignment CreateConsignment(string id, string status, string movementStatus, int msgSequence)
		{
			return new ConsignmentTranshipment(this, id, status, movementStatus);
		}

		protected override string GetJobID()
		{
			var result = ZString.Empty;
			if (IsAirCargo)
			{
				result = CusMawb.CM_MessageReference;
			}
			else if (isSeaCargo)
			{
				result = Container.OceanBill.CB_MessageReference;
			}

			return result;
		}

		protected override string GetJobName()
		{
			var result = ZString.Empty;
			if (IsAirCargo)
			{
				result = Constants.NZCustoms.ExpressECIName;
			}
			else if (isSeaCargo)
			{
				result = Constants.NZCustoms.ExpressSeaCargoName;
			}

			return result;
		}

		protected override bool GetIsImportEntry()
		{
			var result = ZBool.False;
			if (IsAirCargo)
			{
				result = CusMawb.IsImport;
			}
			else if (isSeaCargo)
			{
				result = Container.OceanBill.IsImport;
			}

			return result;
		}

		public override string MessageParentID => response.SendersReference;

		public override string MessageParentTypeName => NZConstants.ExpressMasterDTRName;

		protected override void SetCustomsStatus(ZString newValue)
		{
			UpdateStatusIfRequired(response.ResponsibleGovernmentAgency, newValue);
		}

		TSWStatus TSWStatus
		{
			get { return new TSWStatus(this); }
		}

		void UpdateStatusIfRequired(string agency, ZString movementStatus)
		{
			if (agency == ResponsibleGovernmentAgencyList.Codes.TSW || movementStatus == CombinedMovementStatus.Codes.CAN)
			{
				TranshipmentRequest.C4_Status = movementStatus;
			}
			else if (agency == ResponsibleGovernmentAgencyList.Codes.MPIBIO)
			{
				TranshipmentRequest.C4_MessageStatus = TSWStatus.CalculateCombinedMovementStatus(TranshipmentRequest);
			}
			else if (agency == ResponsibleGovernmentAgencyList.Codes.NZCS)
			{
				TranshipmentRequest.C4_MessageStatus = TSWStatus.CalculateCombinedMovementStatus(TranshipmentRequest);
			}
		}

		protected override void SetECINumber(ZString newValue)
		{
			if (TranshipmentRequest.TSWEntryNumber == null)
			{
				var entryNumber = Factory.New<CusEntryNumber>();
				entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				entryNumber.CE_ParentID = TranshipmentRequest.PK;
				entryNumber.CE_ParentTable = TranshipmentRequest.TableName;
				entryNumber.CE_EntryType = Common.NZ.CusEntryNumberTypeList.Codes.DomesticTranshipmentRequest;
				entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			}

			TranshipmentRequest.TSWEntryNumber.CE_EntryNum = newValue;
		}

		#endregion // Overrides

		#region IMovementStatus Members

		TranshipmentRequest IMovementStatus.UnderbondMovement => TranshipmentRequest;

		ZString IMovementStatus.CustomsStatus => response.Status;

		ZString IMovementStatus.GoodsClearanceStatus => response.GoodsClearanceStatus;

		ZString IMovementStatus.CombinedStatus => TranshipmentRequest.C4_Status;

		ZString IMovementStatus.Agency => response.ResponsibleGovernmentAgency;

		#endregion
	}
}
