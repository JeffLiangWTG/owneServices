using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgWhsClientAccountAssociation : AutoOrgWhsClientAccountAssociation
	{
		public OrgWhsClientAccountAssociation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region OWC_WW_Warehouse

		[List("Lookups.Warehouses")]
		public override ZGuid OWC_WW_Warehouse
		{
			get => base.OWC_WW_Warehouse;
			set => base.OWC_WW_Warehouse = value;
		}

		#endregion

		#region OWC_WSH_SalesChannel

		[List("Lookups.SalesChannels")]
		public override ZGuid OWC_WSH_SalesChannel
		{
			get => base.OWC_WSH_SalesChannel;
			set => base.OWC_WSH_SalesChannel = value;
		}

		#endregion

		#region OWC_OAN_CarrierAccount

		[List("Lookups.CarrierAccounts")]
		public override ZGuid OWC_OAN_CarrierAccount
		{
			get => base.OWC_OAN_CarrierAccount;
			set
			{
				var prevValue = base.OWC_OAN_CarrierAccount;
				base.OWC_OAN_CarrierAccount = value;

				if (prevValue != value)
				{
					var carrierAccount = CarrierAccount;
					if (carrierAccount != null)
					{
						ResetOtherCarrierAccountIfNecessary(carrierAccount, BillToCarrierAccount, () => OWC_OAN_BillToCarrierAccount = ZGuid.Empty);
						ResetOtherCarrierAccountIfNecessary(carrierAccount, DutyBillToCarrierAccount, () => OWC_OAN_DutyBillToCarrierAccount = ZGuid.Empty);
					}
				}
			}
		}

		static void ResetOtherCarrierAccountIfNecessary(OrgCarrierAccount carrierAccount, OrgCarrierAccount otherCarrierAccount, Action resetOtherCarrierAccount)
		{
			if (otherCarrierAccount != null && otherCarrierAccount.OAN_OH_Carrier != carrierAccount.OAN_OH_Carrier)
			{
				resetOtherCarrierAccount();
			}
		}

		#endregion

		#region CarrierCode

		public ZString CarrierCode => CarrierAccount?.Carrier?.OH_Code ?? ZString.Empty;
		public ZPropertyInfo CarrierCodeInfo => GetZPropertyInfo(nameof(CarrierCode));

		#endregion

		#region CarrierName

		[MaxLength(50)]
		public ZString CarrierName => CarrierAccount?.Carrier?.OH_FullName ?? ZString.Empty;
		public ZPropertyInfo CarrierNameInfo => GetZPropertyInfo(nameof(CarrierName));

		#endregion

		#region OWC_BillingType

		[List("Lookups.BillingTypesList")]
		public override ZString OWC_BillingType
		{
			get => base.OWC_BillingType;
			set
			{
				base.OWC_BillingType = value;
				if (value == CarrierBillingType.BillSender)
				{
					OWC_OAN_BillToCarrierAccount = ZGuid.Empty;
				}
			}
		}

		#endregion

		#region OWC_OAN_BillToCarrierAccount

		[List("Lookups.BillToCarrierAccounts")]
		[ReadOnlyMember(nameof(BillToCarrierAccountReadOnly))]
		public override ZGuid OWC_OAN_BillToCarrierAccount
		{
			get => base.OWC_OAN_BillToCarrierAccount;
			set => base.OWC_OAN_BillToCarrierAccount = value;
		}

		public bool BillToCarrierAccountReadOnly => OWC_BillingType == CarrierBillingType.BillSender;

		public ZString BillToCarrierCode => BillToCarrierAccount?.BillToParty?.OH_Code ?? ZString.Empty;
		public ZPropertyInfo BillToCarrierCodeInfo => GetZPropertyInfo(nameof(BillToCarrierCode));

		[MaxLength(50)]
		public ZString BillToCarrierName => BillToCarrierAccount?.BillToParty?.OH_FullName ?? ZString.Empty;
		public ZPropertyInfo BillToCarrierNameInfo => GetZPropertyInfo(nameof(BillToCarrierName));

		#endregion

		#region OWC_OAN_DutyBillToCarrierAccount

		[List("Lookups.DutyBillToCarrierAccounts")]
		public override ZGuid OWC_OAN_DutyBillToCarrierAccount
		{
			get => base.OWC_OAN_DutyBillToCarrierAccount;
			set => base.OWC_OAN_DutyBillToCarrierAccount = value;
		}

		public ZString DutyBillToCarrierCode => DutyBillToCarrierAccount?.BillToParty?.OH_Code ?? ZString.Empty;
		public ZPropertyInfo DutyBillToCarrierCodeInfo => GetZPropertyInfo(nameof(DutyBillToCarrierCode));

		[MaxLength(50)]
		public ZString DutyBillToCarrierName => DutyBillToCarrierAccount?.BillToParty?.OH_FullName ?? ZString.Empty;
		public ZPropertyInfo DutyBillToCarrierNameInfo => GetZPropertyInfo(nameof(DutyBillToCarrierName));

		#endregion

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return (Client != null && !Client.SecurityProvider.HasModifyWarehouseSecurity) || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region GetCarrierAccountNumber

		public static OrgCarrierAccount GetCarrierAccountNumber(OrgHeader carrier, OrgHeader client, IWhsWarehouse warehouse, IWhsSalesChannel salesChannel = null)
		{
			return GetWhsClientAccountAssociation(carrier, client, warehouse, salesChannel)?.CarrierAccount;
		}

		public static OrgWhsClientAccountAssociation GetWhsClientAccountAssociation(OrgHeader carrier, OrgHeader client, IWhsWarehouse warehouse, IWhsSalesChannel salesChannel)
		{
			OrgWhsClientAccountAssociation whsCAN = null;
			if (carrier != null && client != null)
			{
				var whsCANCarrierCollection = client.OrgWhsClientAccountAssociations.Where(wcan => wcan.CarrierCode == carrier.OH_Code);
				if (whsCANCarrierCollection.Any())
				{
					var ranker = new ColumnValueRanker();
					ranker.Add(OrgWhsClientAccountAssociationSchema.OWC_WW_Warehouse, warehouse?.PK ?? ZGuid.Empty, ZGuid.Empty);
					ranker.Add(OrgWhsClientAccountAssociationSchema.OWC_WSH_SalesChannel, salesChannel?.PK ?? ZGuid.Empty, ZGuid.Empty);

					whsCAN = ranker.GetBestMatch(whsCANCarrierCollection)?.FirstOrDefault();
				}
			}

			return whsCAN;
		}

		#endregion
	}
}
