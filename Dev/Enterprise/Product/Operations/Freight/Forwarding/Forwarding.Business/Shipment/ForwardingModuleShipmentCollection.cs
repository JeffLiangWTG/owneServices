using System;
using System.Collections;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingModuleShipmentCollection : ModuleShipmentCollection, IFilterModuleExtraNotificationProvider, IExternalListValidation
	{
		public ForwardingModuleShipmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			ShipmentDomainService.GetInstance(factory).ModuleShipmentCollection = this;
		}

		#region Implementation

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return fetchStrategy ?? (fetchStrategy = new ForwardingModuleShipmentCollectionFetchStrategy(this));
		}
		IBusinessObjectCollectionFetchStrategy fetchStrategy;

		public new ForwardingModuleShipment this[int index]
		{
			get { return (ForwardingModuleShipment)Elements[index]; }
		}

		public new ForwardingModuleShipment AddNew()
		{
			return (ForwardingModuleShipment)base.AddNew();
		}

		#endregion

		#region Cus Entry Numbers

		public ZString GetEntryNum(ForwardingModuleShipment shipment)
		{
			ZString result = "";
			ModuleEntryNum entryNumRecord = GetEntryNumRecord(shipment);

			if (entryNumRecord != null)
			{
				result = entryNumRecord.EntryNum;
			}
			return result;
		}

		public ZString GetEntryStatus(ForwardingModuleShipment shipment)
		{
			ZString result = "";
			ModuleEntryNum entryNumRecord = GetEntryNumRecord(shipment);
			if (entryNumRecord != null)
			{
				result = entryNumRecord.EntryNumStatus;
			}
			return result;
		}

		public ZString GetCMRCustomsStatus(ForwardingModuleShipment shipment)
		{
			ZString result = "";
			ModuleHouseBill houseBillRecord = GetHouseBillRecord(shipment);
			if (houseBillRecord != null)
			{
				result = houseBillRecord.CMRCustomsStatus;
			}
			return result;
		}

		public ZString GetCMRMessageStatus(ForwardingModuleShipment shipment)
		{
			ZString result = "";
			ModuleHouseBill houseBillRecord = GetHouseBillRecord(shipment);
			if (houseBillRecord != null)
			{
				result = houseBillRecord.CMRMessageStatus;
			}
			return result;
		}

		public ZString GetEntryType(ForwardingModuleShipment shipment)
		{
			ZString result = "";
			ModuleEntryNum entryNumRecord = GetEntryNumRecord(shipment);
			if (entryNumRecord != null)
			{
				result = entryNumRecord.EntryNumType;
			}
			return result;
		}

		class ModuleHouseBill
		{
			public ModuleHouseBill(ZString cMRCustomsStatus, ZString cMRMessageStatus)
			{
				this.CMRCustomsStatus = cMRCustomsStatus;
				this.CMRMessageStatus = cMRMessageStatus;
			}

			public readonly ZString CMRCustomsStatus;
			public readonly ZString CMRMessageStatus;
		}

		class ModuleEntryNum
		{
			public ModuleEntryNum(ZString number, ZString type, ZString status)
			{
				EntryNum = number;
				EntryNumType = type;
				EntryNumStatus = status;
			}

			public readonly ZString EntryNum;
			public readonly ZString EntryNumType;
			public readonly ZString EntryNumStatus;
		}

		public void LoadHouseBills(ForwardingModuleShipment shipment)
		{
			ForwardingModuleShipment[] houseBillShipments = GetLookAheadArray(shipment);
			StringBuilder sqlText = new StringBuilder();

			sqlText.Append("SELECT " + JobShipmentSchema.Constants.PK + ", ");
			sqlText.Append(CusHAWBSchema.Constants.CS_CustomsStatus + ", ");
			sqlText.Append(CusHAWBSchema.Constants.CS_MsgStatus + " FROM ");
			sqlText.Append(CusHAWBSchema.Constants.TableName + " JOIN ");
			sqlText.Append(JobShipmentSchema.Constants.TableName + " ON ");
			sqlText.Append(JobShipmentSchema.Constants.PK + " = ");
			sqlText.Append(CusHAWBSchema.Constants.CS_JS + " WHERE ");
			sqlText.AppendLine(JobShipmentSchema.Constants.PK + (NoResString)" IN (SELECT value FROM @Guids)"); // May be a part of SQL expression.

			sqlText.Append((NoResString)"UNION ALL SELECT " + ZArchitecture.Schema.JobShipmentSchema.Constants.PK + (NoResString)", "); // May be a part of SQL expression.
			sqlText.Append(CusSCAHouseSchema.Constants.CA_ShipmentStatus + ", ");
			sqlText.Append(CusSCAHouseSchema.Constants.CA_MessageStatus + " FROM ");
			sqlText.Append(CusSCAHouseSchema.Constants.TableName + " JOIN ");
			sqlText.Append(JobShipmentSchema.Constants.TableName + " ON ");
			sqlText.Append(JobShipmentSchema.Constants.PK + " = ");
			sqlText.Append(CusSCAHouseSchema.Constants.CA_JS + " WHERE ");
			sqlText.Append(JobShipmentSchema.Constants.PK + (NoResString)" IN (SELECT value FROM @Guids)"); // May be a part of SQL expression.

			using (DbCommand command = Db.Connection.Command(sqlText.ToString())) // Can not access the DB for every row it is too slow.  This gets a range of entry numbers for all visible records.
			{
				command.AddTableValuedParameter("@Guids", JobShipmentSchema.PK, houseBillShipments.Select(x => x.PK));
				using (var dr = command.ExecuteReader())
				{
					while (dr.Read())
					{
						ZGuid pK = (Guid)dr[0];
						ZString cMRCustomsStatus = dr[1].ToString();
						ZString cMRMessageStatus = dr[2].ToString();
						HouseBills[pK] = new ModuleHouseBill(cMRCustomsStatus, cMRMessageStatus);
					}
				}
			}

			foreach (ForwardingModuleShipment ensureLoaded in houseBillShipments)
			{
				if (HouseBills[ensureLoaded.PK] == null)
				{
					HouseBills[ensureLoaded.PK] = new ModuleHouseBill("", "");
				}
			}
		}

		public void LoadCusEntryNumbers(ForwardingModuleShipment shipment)
		{
			ForwardingModuleShipment[] cusEntryNumShipments = GetLookAheadArray(shipment);
			ZString entryStatusColumn = JobDeclarationSchema.Constants.JE_EntryStatus;
			StringBuilder sqlText = new StringBuilder();

			entryStatusColumn = CusEntryNumSchema.Constants.CE_EntryStatus;
			sqlText.Append("SELECT " + JobShipmentSchema.Constants.PK + ", ");
			sqlText.Append(CusEntryNumSchema.Constants.CE_EntryNum + ", ");
			sqlText.Append(CusEntryNumSchema.Constants.CE_EntryType + ", ");
			sqlText.Append(entryStatusColumn + " FROM ");
			sqlText.Append(CusEntryNumSchema.Constants.TableName + " JOIN ");
			sqlText.Append(JobShipmentSchema.Constants.TableName + " ON ");
			sqlText.Append(JobShipmentSchema.Constants.PK + " = ");
			sqlText.Append(CusEntryNumSchema.Constants.CE_ParentID + " WHERE ");
			sqlText.AppendLine(JobShipmentSchema.Constants.PK + (NoResString)" IN (SELECT value FROM @Guids)"); // May be a part of SQL expression

			sqlText.Append((NoResString)"UNION ALL SELECT " + JobShipmentSchema.Constants.PK + (NoResString)", "); // May be a part of SQL expression.
			sqlText.Append(CusEntryNumSchema.Constants.CE_EntryNum + ", ");
			sqlText.Append(CusEntryNumSchema.Constants.CE_EntryType + ", ");
			sqlText.Append(entryStatusColumn + " FROM ");
			sqlText.Append(CusEntryNumSchema.Constants.TableName + " JOIN ");
			sqlText.Append(JobDeclarationSchema.Constants.TableName + " ON ");
			sqlText.Append(JobDeclarationSchema.Constants.PK + " = ");
			sqlText.Append(CusEntryNumSchema.Constants.CE_ParentID + " JOIN ");
			sqlText.Append(JobShipmentSchema.Constants.TableName + " ON ");
			sqlText.Append(JobDeclarationSchema.Constants.JE_JS + " = ");
			sqlText.Append(JobShipmentSchema.Constants.PK + " WHERE ");
			sqlText.AppendLine(JobShipmentSchema.Constants.PK + (NoResString)" IN (SELECT value FROM @Guids)"); // May be a part of SQL expression

			sqlText.Append((NoResString)"UNION ALL SELECT " + JobShipmentSchema.Constants.PK + (NoResString)", "); // May be a part of SQL expression.
			sqlText.Append(CusEntryNumSchema.Constants.CE_EntryNum + ", ");
			sqlText.Append(CusEntryNumSchema.Constants.CE_EntryType + ", ");
			sqlText.Append(entryStatusColumn + " FROM ");
			sqlText.Append(CusEntryNumSchema.Constants.TableName + " JOIN ");
			sqlText.Append(CusEntryHeaderSchema.Constants.TableName + " ON ");
			sqlText.Append(CusEntryNumSchema.Constants.CE_ParentID + " = ");
			sqlText.Append(CusEntryHeaderSchema.Constants.PK + " JOIN ");
			sqlText.Append(JobDeclarationSchema.Constants.TableName + " ON ");
			sqlText.Append(JobDeclarationSchema.Constants.PK + " = ");
			sqlText.Append(CusEntryHeaderSchema.Constants.CH_JE + " JOIN ");
			sqlText.Append(JobShipmentSchema.Constants.TableName + " ON ");
			sqlText.Append(JobDeclarationSchema.Constants.JE_JS + " = ");
			sqlText.Append(JobShipmentSchema.Constants.PK + " WHERE ");
			sqlText.Append(JobShipmentSchema.Constants.PK + (NoResString)" IN (SELECT value FROM @Guids)"); // May be a part of SQL expression.

			using (DbCommand command = Db.Connection.Command(sqlText.ToString())) // Can not access the DB for every row it is too slow.  This gets a range of entry numbers for all visible records.
			{
				command.AddTableValuedParameter("@Guids", JobShipmentSchema.PK, cusEntryNumShipments.Select(x => x.PK));

				using (var dr = command.ExecuteReader())
				{
					while (dr.Read())
					{
						ZGuid pK = (Guid)dr[0];
						ZString entryNum = dr[1].ToString();
						ZString entryType = CusEntryNumber.GetEntryNumberTypeForDisplay(dr[2].ToString(), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, false);
						ZString entryStatus = dr[3].ToString();
						CusEntryNums[pK] = new ModuleEntryNum(entryNum, entryType, entryStatus);
					}
				}
			}
			foreach (ForwardingModuleShipment ensureLoaded in cusEntryNumShipments)
			{
				if (CusEntryNums[ensureLoaded.PK] == null)
				{
					CusEntryNums[ensureLoaded.PK] = new ModuleEntryNum("", "", "");
				}
			}
		}

		ModuleHouseBill GetHouseBillRecord(ForwardingModuleShipment shipment)
		{
			ModuleHouseBill result = (ModuleHouseBill)HouseBills[shipment.PK];
			if (result == null)
			{
				LoadHouseBills(shipment);
				result = (ModuleHouseBill)HouseBills[shipment.PK];
			}
			return result;
		}

		ModuleEntryNum GetEntryNumRecord(ForwardingModuleShipment shipment)
		{
			ModuleEntryNum result = (ModuleEntryNum)CusEntryNums[shipment.PK];
			if (result == null)
			{
				LoadCusEntryNumbers(shipment);
				result = (ModuleEntryNum)CusEntryNums[shipment.PK];
			}
			return result;
		}

		protected const int ReadAheadCache = 100;
		protected ForwardingModuleShipment[] GetLookAheadArray(ForwardingModuleShipment shipment)
		{
			ArrayList lookAheadList = new ArrayList(ReadAheadCache);
			lookAheadList.Add(shipment);
			int currentIndex = Elements.IndexOf(shipment);
			if (currentIndex > -1)
			{
				int stopIndex = currentIndex + ReadAheadCache * 2;
				if (stopIndex > Count)
				{
					stopIndex = Count;
				}

				while (currentIndex < stopIndex)
				{
					ForwardingModuleShipment lookAhead = this[currentIndex];
					if (!ShipmentCached(lookAhead))
					{
						lookAheadList.Add(lookAhead);
						if (lookAheadList.Count >= ReadAheadCache)
						{
							break;
						}
					}
					currentIndex++;
				}
			}
			return (ForwardingModuleShipment[])lookAheadList.ToArray(typeof(ForwardingModuleShipment));
		}

		protected bool ShipmentCached(ForwardingModuleShipment shipment)
		{
			return CusEntryNums[shipment.PK] != null;
		}

		Hashtable fHouseBills;
		protected Hashtable HouseBills
		{
			get
			{
				if (fHouseBills == null)
				{
					fHouseBills = new Hashtable();
				}
				return fHouseBills;
			}
		}

		Hashtable fCusEntryNums;
		protected Hashtable CusEntryNums
		{
			get
			{
				if (fCusEntryNums == null)
				{
					fCusEntryNums = new Hashtable();
				}
				return fCusEntryNums;
			}
		}

		#endregion

		#region IFilterModuleExtraNotificationProvider Members

		INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
		{
			ForwardingShipment shipment = businessObject as ForwardingShipment;
			if (shipment != null && ParentConsol != null)
			{
				using
				(
					new DisposableAction(
						() => FreightShipmentVsConsolMessageHelper.Instance.IsGatewayServiceLevelCheckSuspended = true,
						() => FreightShipmentVsConsolMessageHelper.Instance.IsGatewayServiceLevelCheckSuspended = false)
				)
				{
					IShipmentConsolAttachRequest attachRequest = FreightShipmentVsConsolMessageHelper.Instance.IsAllowedToAttachShipment(ParentConsol, shipment);
					if (!attachRequest.Errors.IsEmpty)
					{
						return new Notification(CargoWise.ComponentModel.NotificationType.Error, attachRequest.Errors);
					}
					else if (!attachRequest.Warnings.IsEmpty)
					{
						return new Notification(CargoWise.ComponentModel.NotificationType.Warning, attachRequest.Warnings);
					}
				}
			}

			if (GetExtraNotificationHanlder != null)
			{
				return GetExtraNotificationHanlder(businessObject);
			}

			return null;
		}

		public Func<BusinessObject, INotification> GetExtraNotificationHanlder;

		#endregion

		#region Template Records

		public bool AllowTemplateRecords { get; set; }

		protected override IFindBoxListProvider FindBoxListProvider => AllowTemplateRecords ? new TemplateRecordFindboxListProvider(this) : base.FindBoxListProvider;

		bool IExternalListValidation.IsValidTemplateRecordPK(ZGuid pk)
		{
			if (AllowTemplateRecords)
			{
				var templateRecord = Factory.Load<StmTemplateRecord>(pk);
				return templateRecord != null && templateRecord.STR_ModuleID == nameof(ModuleId.JobShipment);
			}
			{
				return false;
			}
		}

#if DEBUG
		internal IFindBoxListProvider FindBoxListProviderExposedForTest => FindBoxListProvider;
#endif

		#endregion
	}
}
