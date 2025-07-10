using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public class DocumentImportCargoLabel : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string IncludeConsignee = "IncludeConsignee";
			public const string IncludeConsignor = "IncludeConsignor";
			public const string IncludeHouseBill = "IncludeHouseBill";
			public const string IncludeCFSName = "IncludeCFSName";
			public const string NoOfLabelsToPrint = "NoOfLabelsToPrint";
		}

		#endregion

		public DocumentImportCargoLabel(ForwardingConsol consol)
			: base(consol.Factory)
		{
			Consol = consol;

			defaultNumberOfLabelsToPrint = (ZInt)consol.JK_TotalShipmentQuantity;
			fNoOfLabelsToPrint = defaultNumberOfLabelsToPrint;
		}

		public DocumentImportCargoLabel(ForwardingShipment shipment)
			: base(shipment.Factory)
		{
			Shipment = shipment;

			defaultNumberOfLabelsToPrint = shipment.JS_OuterPacks;
			fNoOfLabelsToPrint = defaultNumberOfLabelsToPrint;
		}

		readonly ZInt defaultNumberOfLabelsToPrint;

		public readonly ForwardingConsol Consol;
		public readonly ForwardingShipment Shipment;

		#region Properties for binding

		#region Include Consignee

		public ZBool IncludeConsignee
		{
			get { return fIncludeConsignee; }
			set
			{
				fIncludeConsignee = value;
				SetNoOfLabelsToPrint();
				IncludeConsigneeInfo.RefreshBinding();
			}
		}
		ZBool fIncludeConsignee;

		public ZPropertyInfo IncludeConsigneeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IncludeConsignee); }
		}

		#endregion

		#region Include Consignor

		public ZBool IncludeConsignor
		{
			get { return fIncludeConsignor; }
			set
			{
				fIncludeConsignor = value;
				SetNoOfLabelsToPrint();
				IncludeConsignorInfo.RefreshBinding();
			}
		}
		ZBool fIncludeConsignor;

		public ZPropertyInfo IncludeConsignorInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IncludeConsignor); }
		}

		#endregion

		#region Include HouseBill

		public ZBool IncludeHouseBill
		{
			get { return fIncludeHouseBill; }
			set
			{
				fIncludeHouseBill = value;
				SetNoOfLabelsToPrint();
				IncludeHouseBillInfo.RefreshBinding();
			}
		}
		ZBool fIncludeHouseBill;

		public ZPropertyInfo IncludeHouseBillInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IncludeHouseBill); }
		}

		#endregion

		#region Include CFSName

		public ZBool IncludeCFSName
		{
			get { return fIncludeCFSName; }
			set
			{
				fIncludeCFSName = value;
				IncludeCFSNameInfo.RefreshBinding();
			}
		}
		ZBool fIncludeCFSName;

		public ZPropertyInfo IncludeCFSNameInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IncludeCFSName); }
		}

		#endregion

		#region No of Labels to Print

		protected bool NoOfLabelsToPrint_ReadOnly
		{
			get { return IncludeConsignor || IncludeConsignee || IncludeHouseBill; }
		}

		void SetNoOfLabelsToPrint()
		{
			NoOfLabelsToPrint = defaultNumberOfLabelsToPrint;
		}

		public ZInt NoOfLabelsToPrint
		{
			get { return fNoOfLabelsToPrint; }
			set
			{
				fNoOfLabelsToPrint = value;
				if (!IsValidationSuspended)
				{
					ValidateNoOfLabelsToPrint();
				}
				NoOfLabelsToPrintInfo.RefreshBinding();
			}
		}
		ZInt fNoOfLabelsToPrint;

		protected void ValidateNoOfLabelsToPrint()
		{
			NoOfLabelsToPrintInfo.ClearAllNotifications();

			if (NoOfLabelsToPrint < 1)
			{
				NoOfLabelsToPrintInfo.AddError(Res.GetString("a8955a21-b60e-450f-b27b-565cd8fee1d8", "Number of labels to print needs to be greater or equal to 1"));
			}
			else if (NoOfLabelsToPrint != defaultNumberOfLabelsToPrint)
			{
				NoOfLabelsToPrintInfo.AddWarning(Res.GetString("9471cc99-818f-4cb2-bc92-33f585b4b3c3", "Number of labels to print is different to the number of outer packs: {0}", defaultNumberOfLabelsToPrint));
			}
		}

		public ZPropertyInfo NoOfLabelsToPrintInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.NoOfLabelsToPrint); }
		}

		#endregion

		#endregion
	}
}
