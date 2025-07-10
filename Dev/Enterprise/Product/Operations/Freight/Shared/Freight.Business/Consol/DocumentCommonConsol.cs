using CargoWise.EntityFramework;

using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class DocumentCommonConsol : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string IncludeConsignee = "IncludeConsignee";
			public const string IncludeConsignor = "IncludeConsignor";
			public const string IncludeCustomsBroker = "IncludeCustomsBroker";
			public const string IncludeAllShipments = "IncludeAllShipments";
			public const string IncludePacked = "IncludePacked";
			public const string IncludeUnPacked = "IncludeUnPacked";
			public const string ContainerToPrint = "ContainerToPrint";
		}

		#endregion

		public DocumentCommonConsol(CommonConsol commonConsol, Core.Constants.DataContext dataContext) : base(commonConsol.Factory)
		{
			fConsol = commonConsol;
			this.DataContext = dataContext;
		}

		public CommonConsol Consol
		{
			get { return fConsol; }
		}
		protected CommonConsol fConsol;

		public readonly Core.Constants.DataContext DataContext;

		public void SetDefaultsFromDataContext(Core.Constants.DataContext dataContext)
		{
			switch (dataContext)
			{
				case Core.Constants.DataContext.LoadListDocument:
					IncludeConsignor = ZBool.True;
					IncludeConsignee = ZBool.True;
					IncludeAllShipments = ZBool.True;
					break;
			}
		}

		#region Properties for binding

		#region Include Consignee

		public ZBool IncludeConsignee
		{
			get { return fIncludeConsignee; }
			set
			{
				fIncludeConsignee = value;
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

		#region Include Customs Broker

		public ZBool IncludeCustomsBroker
		{
			get { return fIncludeCustomsBroker; }
			set
			{
				fIncludeCustomsBroker = value;
				IncludeCustomsBrokerInfo.RefreshBinding();
			}
		}
		ZBool fIncludeCustomsBroker;

		public ZPropertyInfo IncludeCustomsBrokerInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IncludeCustomsBroker); }
		}

		#endregion

		#region Include AllShipments

		public ZBool IncludeAllShipments
		{
			get { return fIncludeAllShipments; }
			set
			{
				fIncludeAllShipments = value;
				IncludeAllShipmentsInfo.RefreshBinding();
			}
		}
		ZBool fIncludeAllShipments;

		public ZPropertyInfo IncludeAllShipmentsInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IncludeAllShipments); }
		}

		#endregion

		#region Include Packed

		public ZBool IncludePacked
		{
			get { return fIncludePacked; }
			set
			{
				fIncludePacked = value;
				IncludePackedInfo.RefreshBinding();
			}
		}
		ZBool fIncludePacked;

		public ZPropertyInfo IncludePackedInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IncludePacked); }
		}

		#endregion

		#region Include UnPacked

		public ZBool IncludeUnPacked
		{
			get { return fIncludeUnPacked; }
			set
			{
				fIncludeUnPacked = value;
				IncludeUnPackedInfo.RefreshBinding();
			}
		}
		ZBool fIncludeUnPacked;

		public ZPropertyInfo IncludeUnPackedInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IncludeUnPacked); }
		}

		#endregion

		#region ContainerToPrint

		public CommonContainer ContainerToPrint { get; set; }

		#endregion

		#endregion
	}
}
