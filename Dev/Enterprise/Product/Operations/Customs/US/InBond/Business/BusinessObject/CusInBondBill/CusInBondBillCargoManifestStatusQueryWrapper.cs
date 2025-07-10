using System;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondBillCargoManifestStatusQueryWrapper : ICargoManifestStatusQueryData
	{
		public CusInBondBillCargoManifestStatusQueryWrapper(CusInBondBill bill, bool isHouseBillLevel, bool isAMSHBREffective)
		{
			this.bill = bill;
			this.isHouseBillLevel = isHouseBillLevel;
			this.isAMSHBREffective = isAMSHBREffective;
		}
		readonly CusInBondBill bill;
		readonly bool isHouseBillLevel;
		readonly bool isAMSHBREffective;

		ZString ICargoManifestStatusQueryData.HumanFriendlyReference
		{
			get { return isHouseBillLevel || !isAMSHBREffective || !bill.IsSea ? bill.HumanReadableName : GetSeaMasterBillOnlyName(); }
		}

		ZString GetSeaMasterBillOnlyName()
		{
			var result = new ZStringBuilder();
			result.Append("Bill Of Lading ");
			if (bill.Header?.BH_FTZMove ?? false)
			{
				result.AppendIfNotEmpty(bill.B0_MasterBillNumber);
			}
			else
			{
				result.AppendIfNotEmpty(bill.IssuerCodeAndMasterBillNumber);
			}
			return result.ToString();
		}

		ZString ICargoManifestStatusQueryData.JobReferenceNumber
		{
			get { return bill.Header != null ? bill.Header.BH_JobReference : ZString.Empty; }
		}

		ZString ICargoManifestStatusQueryData.EntryOrInBondNumber
		{
			get { return ZString.Empty; }
		}

		ZString ICargoManifestStatusQueryData.MasterAirWayBillNumber
		{
			get { return bill.B0_MasterBillNumber; }
		}

		ZString ICargoManifestStatusQueryData.HouseAirWayBillNumber
		{
			get { return bill.B0_HouseBillNumber; }
		}

		ZString ICargoManifestStatusQueryData.BillIssuerCode
		{
			get { return isHouseBillLevel && bill.IsSea && !bill.B0_HouseBillIssuerCode.IsEmpty ? bill.B0_HouseBillIssuerCode : bill.B0_IssuerCode; }
		}

		ZString ICargoManifestStatusQueryData.BillNumber
		{
			get { return isHouseBillLevel && bill.IsSea && !bill.B0_HouseBillNumber.IsEmpty ? bill.B0_HouseBillNumber : bill.B0_MasterBillNumber; }
		}

		bool ICargoManifestStatusQueryData.HasPGAData
		{
			get { return false; }
		}

		string ICargoManifestStatusQueryData.TableCode
		{
			get { return JobDeclarationSchema.Constants.Prefix; }
		}

		ZGuid ICargoManifestStatusQueryData.MessageAttacheePK
		{
			get { return bill.PK; }
		}

		CargoManifestQueryActionType ICargoManifestStatusQueryData.QueryActionType
		{
			get { return bill.IsAir ? CargoManifestQueryActionType.MAWB : CargoManifestQueryActionType.BillOfLading; }
		}

		public bool IsInDatabase => bill.IsInDatabase;

		public ControllerID ControllerID => ((IControllerIDProvider)bill).ControllerID;

		public Guid BusinessObjectPK => ((IControllerIDProvider)bill).BusinessObjectPK;

		void ICargoManifestStatusQueryData.LinkToMessage(EDIMessage message)
		{
			if (bill?.MovementDetail?.MoveHeader is CusInBondMoveHeader moveHeader)
			{
				moveHeader.Messages.Add(message);
			}
		}

		ZBool ICargoManifestStatusQueryData.IsRelevantFor(ZString actionCode)
		{
			var isRelevantForAirHouseBill = actionCode == CargoManifestStatusQueryActionList.Codes.HAWB && bill.IsAir && !bill.B0_HouseBillNumber.IsEmpty;

			return actionCode.IsEmpty || isRelevantForAirHouseBill ||
				actionCode == CargoManifestStatusQueryActionList.Codes.MAWB || actionCode == CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;
		}
	}
}
