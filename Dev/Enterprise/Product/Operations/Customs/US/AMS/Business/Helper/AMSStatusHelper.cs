using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business
{
	public class AMSStatusHelper : Integration.Customs.US.USAMS.IAMSStatusHelper
	{
		#region IAMSStatusHelper Members
		// Any business logic changes to the below method should also be done against SQL FUNCTION GetConsolAMSBillStatus
		ZString Integration.Customs.US.USAMS.IAMSStatusHelper.GetAMSBillStatus(Integration.Forwarding.IForwardingConsol consolSource)
		{
			var result = ZString.Empty;
			var consol = consolSource as ForwardingConsol;
			if (consol != null && (consol.IsSea || consol.IsRail) && (consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.UnitedStates) || consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.PuertoRico)))
			{
				result = AMSConsolBillCustomsStatusList.Codes.OutOfSync;
				var factory = consol.Factory;
				var query = new ZQuery(CusInBondHeaderSchema.BH_ParentID, consol.PK);
				query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, Enterprise.Customs.Common.CusInBondApplicationCodeList.Codes.AMS);
				query.FetchOnlyFromLocalCache = !consol.IsInDatabase;
				var header = factory.LoadTop1<CusInBondHeader>(query);
				if (header != null)
				{
					var shipments = GetApplicableShipments(new TypedEnumerable<ForwardingShipment>(consol.Shipments));
					var bills = new List<CusInBondBill>(new TypedEnumerable<CusInBondBill>(header.Bills.OrderBy(x => x.B0_IssuerCode + x.B0_MasterBillNumber)));
					if (shipments.Count == bills.Count)
					{
						result = GetAMSBillStatusForShipments(shipments, bills);
					}
				}
			}
			return result;
		}

		ZString GetAMSBillStatusForShipments(List<ForwardingShipment> shipments, List<CusInBondBill> bills)
		{
			ZString result = AMSConsolBillCustomsStatusList.Codes.OutOfSync;
			var hasBillAlreadyOnFile = false;
			var hasBillNotOnFile = false;
			var isOutOfSync = false;
			foreach (var shipment in shipments)
			{
				var shipmentBillNumber = shipment.JS_HouseBill;
				var checkTrimValue = shipmentBillNumber.Length > 4;
				var billNumber = ZString.Empty;
				var issuerCode = ZString.Empty;
				if (checkTrimValue)
				{
					issuerCode = shipmentBillNumber.Left(4);
					billNumber = shipmentBillNumber.SubstringSafe(4);
				}
				var bill = bills.FirstOrDefault(x => x.B0_MasterBillNumber == shipmentBillNumber || (checkTrimValue && x.B0_IssuerCode == issuerCode && x.B0_MasterBillNumber == billNumber));
				if (bill == null)
				{
					isOutOfSync = true;
					break;
				}
				bills.Remove(bill);

				var moveDetail = bill.MovementDetail;
				if (moveDetail != null && moveDetail.IsBillAlreadyOnFile)
				{
					hasBillAlreadyOnFile = true;
				}
				else
				{
					hasBillNotOnFile = true;
				}
			}
			if (!isOutOfSync)
			{
				if (hasBillAlreadyOnFile && hasBillNotOnFile)
				{
					result = AMSConsolBillCustomsStatusList.Codes.Multiple;
				}
				else if (hasBillAlreadyOnFile)
				{
					result = AMSConsolBillCustomsStatusList.Codes.OnFile;
				}
				else if (hasBillNotOnFile)
				{
					result = AMSConsolBillCustomsStatusList.Codes.NotOnFile;
				}
			}
			return result;
		}

		List<ForwardingShipment> GetApplicableShipments(IEnumerable<ForwardingShipment> shipments)
		{
			var result = new List<ForwardingShipment>();
			foreach (var shipment in shipments)
			{
				if (!shipment.IsCoLoadMaster && !shipment.IsBlindCoLoadMaster && !shipment.IsAssemblyMaster)
				{
					var coLoadMasterShipment = shipment.CoLoadMasterShipment;
					if (coLoadMasterShipment == null || (!coLoadMasterShipment.IsCoLoadMaster && !shipment.IsBlindCoLoadMaster && !coLoadMasterShipment.IsAssemblyMaster) || IsShipmentWhichSendingForwarderDoesNotDoDirectAMSReporting(coLoadMasterShipment))
					{
						result.Add(shipment);
					}
				}
			}
			return result;
		}

		bool IsShipmentWhichSendingForwarderDoesNotDoDirectAMSReporting(Freight.Business.CommonShipment coLoadMasterShipment)
		{
			var sendingForwarder = coLoadMasterShipment.Consignor;
			return sendingForwarder == null || sendingForwarder.MiscServ == null || !sendingForwarder.MiscServ.OM_FWDirectAMSReporter;
		}

		#endregion
	}
}
