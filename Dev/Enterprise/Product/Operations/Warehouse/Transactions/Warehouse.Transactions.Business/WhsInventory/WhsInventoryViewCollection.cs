using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsInventoryViewCollection : BusinessObjectCollection<WhsInventoryView>
	{
		#region Constructors

		public WhsInventoryViewCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsInventoryViewCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public WhsInventoryViewCollection(BusinessObjectFactory factory, WhsReceive receive)
			: this(factory, new ZQuery(WhsInventoryViewSchema.WI_WD, receive.PK))
		{
			Receive = receive;
		}

		public WhsInventoryViewCollection(BusinessObjectFactory factory, WhsDocketLine docketLine)
			: this(factory, new ZQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, docketLine.PK))
		{
			DocketLine = docketLine;
		}

		readonly WhsReceive Receive;
		readonly WhsDocketLine DocketLine;

		#endregion

		#region Business Object Collection Overrides

		/// <summary>
		/// This is needed so that when Inventory is suspended, so to is DocketLines. Active collections
		/// do not have this requirement because they share the Index meaning all collections are
		/// effectively suspended.
		/// </summary>
		protected override DisposableList GetAdditionalListChangedSuspenders()
		{
			return Receive != null
				? new DisposableList(new[] { ((IBusinessObjectCollection)Receive.Lines).SuspendListChanged() })
				: base.GetAdditionalListChangedSuspenders();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			var inventory = (WhsInventoryView)child;
			if (Receive != null)
			{
				inventory.WI_WD = Receive.PK;
				inventory.WI_OH_Client = Receive.WD_OH_Client;
				inventory.WI_ArrivalDate = Receive.WD_ArrivalDate;
				if (!Receive.WD_ArrivalDate.IsEmpty)
				{
					inventory.OriginalInventoryStatus = InventoryStatus.Codes.Arrived;
				}
			}
			else if (DocketLine != null)
			{
				var docket = DocketLine.Docket;
				if (docket != null)
				{
					inventory.WI_WD = DocketLine.WE_WD;
					inventory.WI_InDocketLineType = docket.WD_DocketType;
					inventory.WI_WE_InDocketLine = DocketLine.PK;
				}
			}
		}

		protected override bool AllowNewCore
		{
			get
			{
				bool result = false;

				if (Receive != null)
				{
					result = Receive.WD_OH_Client.IsValid && Receive.WD_WW_Whs.IsValid && !Receive.IsFinalisedOrCancelled;
					if (result)
					{
						var receive = Receive;
						if (receive != null && receive.IsCreatedFromWorkOrder)
						{
							result = false;
						}
					}
				}
				return result;
			}
		}

		#endregion
		#region Manual Filter

		public void FilterWarehouse(WhsWarehouse whs)
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				WhsInventoryView inv = this[i];
				if (inv.Location == null || (whs != null && inv.Location.Row.Warehouse.PK != whs.PK))
				{
					this.Remove(inv);
				}
			}
		}

		#endregion

		#region FilterBusinessObjectDefaults

		public void AddWarehouseClientProductFilterDefaults(ZGuid whsPK, ZGuid clientPK, ZGuid productPK)
		{
			if (whsPK.IsValid)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Warehouse", "Property", whsPK));
			}

			if (clientPK.IsValid)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Client", "Property", clientPK));
			}

			if (productPK.IsValid)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Product", "Property", productPK));
			}
		}

		public void AddAttributeFilterDefaults(ILineAttributes attributes)
		{
			if (attributes == null)
			{
				throw new ArgumentNullException(nameof(attributes));
			}

			if (!attributes.ExpiryDate.IsEmpty)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Expiry Date", "Property1", attributes.ExpiryDate));
			}

			if (!attributes.ExpiryDate.IsEmpty)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Expiry Date", "Property2", attributes.ExpiryDate));
			}

			if (!attributes.PackingDate.IsEmpty)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Packing Date", "Property1", attributes.PackingDate));
			}

			if (!attributes.PackingDate.IsEmpty)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Packing Date", "Property2", attributes.PackingDate));
			}

			if (!attributes.PartAttrib1.IsEmpty)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Part Attribute 1", "Property", attributes.PartAttrib1));
			}

			if (!attributes.PartAttrib2.IsEmpty)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Part Attribute 2", "Property", attributes.PartAttrib2));
			}

			if (!attributes.PartAttrib3.IsEmpty)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Part Attribute 3", "Property", attributes.PartAttrib3));
			}
		}

		public void AddLocationFilterDefaults(WhsLocation location)
		{
			if (location == null)
			{
				throw new ArgumentNullException(nameof(location));
			}

			if (location.Row != null)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Row", "Property", location.Row.WR_Name));
			}

			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Column", "Property", new ZString(location.WLV_Column.ToString())));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Level", "Property", new ZString(location.WLV_Level.ToString())));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Tray", "Property", new ZString(location.WLV_Tray.ToString())));
		}

		#endregion

		#region FindBoxListProvider

		public string CodePropertyName
		{
			get { return fCodePropertyName; }
			set { fCodePropertyName = value; }
		}
		string fCodePropertyName;

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new InventoryFindBoxListProvider(this); }
		}

		class InventoryFindBoxListProvider : FindBoxListProvider
		{
			public InventoryFindBoxListProvider(WhsInventoryViewCollection collection)
				: base(collection)
			{
				Inventory = collection;
			}

			protected override string GetCodePropertyName(ZGuid pK)
			{
				return Inventory.CodePropertyName;
			}

			protected override string GetDescriptionPropertyName(Type typeOfElements)
			{
				return Inventory.CodePropertyName;
			}

			protected override void AddCodeEqualsFilter(ZQuery query, string code)
			{
			}

			protected override void AddCodeStartsWithFilter(ZQuery query, string code)
			{
			}

			public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
			{
				return (code, false);
			}

			readonly WhsInventoryViewCollection Inventory;
		}

		#endregion

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			switch (property.Name)
			{
				case nameof(WhsInventoryView.WI_WL):
				case nameof(WhsInventoryView.LocationString):
					return new LocationComparer<WhsInventoryView>(property, direction, inventoryView => inventoryView.Location);
				case nameof(WhsInventoryView.CurrentLocationString):
					return new LocationComparer<WhsInventoryView>(property, direction, inventoryView => inventoryView.CurrentLocation);
				default:
					return base.GetComparerForSort(property, direction);
			}
		}
	}
}
