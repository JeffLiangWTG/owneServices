using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class JobComInvLineComponentInventory : AutoJobComInvLineComponentInventory, IClusterKeyWorker, WarehouseExtensions.IWarehouseProductLine
	{
		#region Constants

		public new class Schema : AutoJobComInvLineComponentInventory.Schema
		{
			public const string ComponentDescription = "ComponentDescription";
			public const string CustomsEntryKey = "CustomsEntryKey";
			public const string QuantityOnHand = "QuantityOnHand";
			public const string PackType = "PackType";
			public const string ArrivalDate = "ArrivalDate";
		}

		#endregion

		public JobComInvLineComponentInventory(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(InvoiceLine))]
		public override ZGuid JIV_JI
		{
			get { return base.JIV_JI; }
			set { base.JIV_JI = value; }
		}

		public BaseJobComInvoiceLine InvoiceLine => Factory.Load<BaseJobComInvoiceLine>(JIV_JI);

		public IWhsInventoryView Inventory
		{
			get
			{
				if (fInventory == null && !CustomsEntryKey.IsEmpty)
				{
					fInventory = Factory.LoadTop1<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_AllocationKey, SQLComparisonOperator.Equal, CustomsEntryKey)) ??
								 Factory.LoadTop1<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_BondedEntryKey, SQLComparisonOperator.Equal, CustomsEntryKey));
				}

				return fInventory;
			}
		}
		IWhsInventoryView fInventory;

		[BusinessObjectMaxLengthTestExclude]
		[ResourceStringData("Enterprise.Customs.Business.JobComInvLineComponentInventory|CustomsEntryKey", Caption = "Customs Entry Key")]
		public ZString CustomsEntryKey
		{
			get => JIV_AllocationKey;
			set
			{
				var oldValue = CustomsEntryKey;
				if (oldValue != value)
				{
					JIV_AllocationKey = value;

					CustomsEntryKeyInfo.RefreshBinding(oldValue);

					fInventory = null;
				}
			}
		}

		public ZPropertyInfo CustomsEntryKeyInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsEntryKey); }
		}

		[ResourceStringData("Enterprise.Customs.Business.JobComInvLineComponentInventory|QuantityOnHand", Caption = "Quantity on Hand")]
		public ZDecimal QuantityOnHand
		{
			get
			{
				return Inventory?.WI_TotalUnits ?? ZDecimal.Zero;
			}
		}

		public ZPropertyInfo QuantityOnHandInfo
		{
			get { return GetZPropertyInfo(Schema.QuantityOnHand); }
		}

		[ResourceStringData("Enterprise.Customs.Business.JobComInvLineComponentInventory|JIV_QuantityToDraw", Caption = "Quantity to Draw")]
		public override ZDecimal JIV_QuantityToDraw { get => base.JIV_QuantityToDraw; set => base.JIV_QuantityToDraw = value; }

		[ResourceStringData("Enterprise.Customs.Business.JobComInvLineComponentInventory|PackType", Caption = "Pack Type")]
		public ZString PackType
		{
			get
			{
				return Inventory?.WI_F3_NKPackType ?? ZString.Empty;
			}
		}

		public ZPropertyInfo PackTypeInfo
		{
			get { return GetZPropertyInfo(Schema.PackType); }
		}

		[ResourceStringData("Enterprise.Customs.Business.JobComInvLineComponentInventory|ArrivalDate", Caption = "Arrival Date")]
		public ZDateTime ArrivalDate
		{
			get
			{
				return Inventory?.WI_ArrivalDate.ToZDateTime() ?? ZDateTime.Empty;
			}
		}

		public ZPropertyInfo ArrivalDateInfo
		{
			get { return GetZPropertyInfo(Schema.ArrivalDate); }
		}

		[MaxLength(JobComInvLineComponentInventory.Schema.JIV_AllocationKeyMaxLength)]
		public override ZString JIV_AllocationKey
		{
			get => base.JIV_AllocationKey;
			set
			{
				if (base.JIV_AllocationKey != value)
				{
					base.JIV_AllocationKey = value;
					fInventory = null;
				}
			}
		}

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)JIV_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobComInvoiceLine);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)JIV_JIInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobComInvLineComponentInventoryFetchStrategy(this);
	}
}
