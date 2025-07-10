using System;
using System.Collections.Generic;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsPalletWebServiceResponse : WebServiceResponse
	{
		#region Constructors

		public WhsPalletWebServiceResponse()
			: base()
		{
			PalletInfo = new PutawayPalletInfo();
		}

		#endregion

		#region Properties

		#region PalletInfo

		public PutawayPalletInfo PalletInfo
		{
			get;
			set;
		}

		#endregion

		#region PalletID

		public string PalletID
		{
			get => PalletInfo.PalletID;
			set => PalletInfo.PalletID = value;
		}

		#endregion

		#region Inventory

		public WhsInventoryLineInfoCollection Inventory
		{
			get { return inventory ?? (inventory = new WhsInventoryLineInfoCollection()); }
			set { inventory = value; }
		}

		#endregion

		#region ReferencesOfReceiveThatCouldBeAutoFinalised

		public List<string> ReferencesOfReceiveThatCouldBeAutoFinalised
		{
			get;
			set;
		}

		#endregion

		#region AllowToOverrideLocation

		public bool AllowToOverrideLocation
		{
			get;
			set;
		}

		#endregion

		#region ShowStockOnHandWarningOnPutaway

		public bool ShowStockOnHandWarningOnPutaway { get; set; }

		#endregion

		#region TaskPK

		public Guid TaskPK { get; set; }

		#endregion

		#endregion

		#region Implementation

		WhsInventoryLineInfoCollection inventory;

		#endregion
	}
}
