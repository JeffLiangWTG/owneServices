using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ShipmentAWBActions : AWBActions
	{
		#region Schema

		[SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new sealed class Schema : AWBActions.Schema
		{
			public const string LabelConsol = "LabelConsol";
			public const string ConsolTotalPiecesFromMAWB = "ConsolTotalPiecesFromMAWB";
			public const string ConsolTotalPiecesFromConsol = "ConsolTotalPiecesFromConsol";
			public const string ConsolTotalPiecesSuppress = "ConsolTotalPiecesSuppress";
			public const string ConsolTotalRangeFrom = "ConsolTotalRangeFrom";
			public const string ConsolTotalRangeTo = "ConsolTotalRangeTo";
			public const string ConsolTotalPacks = "ConsolTotalPacks";

			Schema() { }
		}

		#endregion

		#region Ctor

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "All overriding methods verified safe")]
		public ShipmentAWBActions(ForwardingShipment shipment, ActionsModeType actionsMode)
			: base(actionsMode, shipment.Factory)
		{
			this.shipment = shipment;
			SetAdditionalDefaults();
		}

		#endregion

		readonly ForwardingShipment shipment;
		readonly List<ZGuid> consolPKsWithPopulatedAWB = new List<ZGuid>();

		public override string SettingsCacheName
		{
			get { return settingsCacheName; }
		}
		const string settingsCacheName = "ShipmentAWBActionsSettings";

		public override ZString ParentName
		{
			get { return Res.GetString("5e8e2245-4a76-477e-b76f-415d9fcac94b", "Shipment"); }
		}

		protected override ExportAWBHeader AWB
		{
			get { return shipment.AWBHeader; }
		}

		protected override IDocumentSupportable DocumentSupportable
		{
			get { return shipment; }
		}

		#region Properties

		protected override ZInt PackagesNumber
		{
			get { return shipment.JS_OuterPacks; }
		}

		public override ZInt LabelRangeFrom
		{
			get { return base.LabelRangeFrom; }
			set
			{
				if (base.LabelRangeFrom != value)
				{
					base.LabelRangeFrom = value;

					if (!IsValidationSuspended)
					{
						ValidateConsolTotalRangeFrom();
					}
				}
			}
		}

		public override ZInt LabelRangeTo
		{
			get { return base.LabelRangeTo; }
			set
			{
				if (base.LabelRangeTo != value)
				{
					base.LabelRangeTo = value;

					if (!IsValidationSuspended)
					{
						ValidateConsolTotalRangeFrom();
					}
				}
			}
		}

		public override ZInt TotalPacks
		{
			get { return base.TotalPacks; }
			set
			{
				if (base.TotalPacks != value)
				{
					base.TotalPacks = value;

					if (!IsValidationSuspended)
					{
						ValidateConsolTotalRangeFrom();
					}
				}
			}
		}

		[List("ConsolNames")]
		public ZGuid LabelConsol
		{
			get { return labelConsol; }
			set
			{
				if (SetNonPersistentPropertyValue(LabelConsolInfo, ref labelConsol, value))
				{
					if (!IsValidationSuspended)
					{
						ValidateLabelConsol();
					}

					RefreshConsolRangeData();
				}
			}
		}
		ZGuid labelConsol;

		public ZPropertyInfo LabelConsolInfo
		{
			get { return GetZPropertyInfo(Schema.LabelConsol); }
		}

		public virtual void ValidateLabelConsol()
		{
			LabelConsolInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(LabelConsolInfo);
			MandatoryValidation.CheckEntered(LabelConsolInfo);
		}

		public ZBool ConsolTotalPiecesFromMAWB
		{
			get { return consolTotalPiecesFromMAWB; }
			set
			{
				if (SetNonPersistentPropertyValue(ConsolTotalPiecesFromMAWBInfo, ref consolTotalPiecesFromMAWB, value))
				{
					RefreshConsolRangeData();
				}
			}
		}
		ZBool consolTotalPiecesFromMAWB;

		public ZPropertyInfo ConsolTotalPiecesFromMAWBInfo
		{
			get { return GetZPropertyInfo(Schema.ConsolTotalPiecesFromMAWB); }
		}

		public ZBool ConsolTotalPiecesFromConsol
		{
			get { return consolTotalPiecesFromConsol; }
			set
			{
				if (SetNonPersistentPropertyValue(ConsolTotalPiecesFromConsolInfo, ref consolTotalPiecesFromConsol, value))
				{
					RefreshConsolRangeData();
				}
			}
		}
		ZBool consolTotalPiecesFromConsol;

		public ZPropertyInfo ConsolTotalPiecesFromConsolInfo
		{
			get { return GetZPropertyInfo(Schema.ConsolTotalPiecesFromConsol); }
		}

		public ZBool ConsolTotalPiecesSuppress
		{
			get { return consolTotalPiecesSuppress; }
			set
			{
				if (SetNonPersistentPropertyValue(ConsolTotalPiecesSuppressInfo, ref consolTotalPiecesSuppress, value))
				{
					ConsolTotalRangeFromInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						ValidateConsolTotalRangeFrom();
					}
				}
			}
		}
		ZBool consolTotalPiecesSuppress;

		public ZPropertyInfo ConsolTotalPiecesSuppressInfo
		{
			get { return GetZPropertyInfo(Schema.ConsolTotalPiecesSuppress); }
		}

		public ZInt ConsolTotalRangeFrom
		{
			get { return consolTotalRangeFrom; }
			set
			{
				if (SetNonPersistentPropertyValue(ConsolTotalRangeFromInfo, ref consolTotalRangeFrom, value))
				{
					ConsolTotalRangeToInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						ValidateConsolTotalRangeFrom();
					}
				}
			}
		}
		ZInt consolTotalRangeFrom;

		public ZPropertyInfo ConsolTotalRangeFromInfo
		{
			get { return GetZPropertyInfo(Schema.ConsolTotalRangeFrom); }
		}

		public bool ConsolTotalRangeFrom_ReadOnly
		{
			get { return ConsolTotalPiecesSuppress || LabelConsol == ZGuid.Empty; }
		}

		public void ValidateConsolTotalRangeFrom()
		{
			ConsolTotalRangeFromInfo.ClearAllNotifications();

			if (ConsolTotalRangeFrom < 1 && !LabelConsol.IsEmpty && !ConsolTotalPiecesSuppress)
			{
				ConsolTotalRangeFromInfo.AddError(Res.GetString("b76fdd48-a602-49d1-b24e-d3ba16900518", "Start range needs to be greater or equal to 1"));
			}
			else if (ConsolTotalRangeTo > ConsolTotalPacks && !LabelConsol.IsEmpty && !ConsolTotalPiecesSuppress)
			{
				ConsolTotalRangeFromInfo.AddError(Res.GetString("0b3c846e-4007-4778-ba53-ad7c156a6277", "Start range needs to be less or equal to {0}", Math.Abs(ConsolTotalPacks - (LabelRangeTo - LabelRangeFrom))));
			}
		}

		public ZInt ConsolTotalRangeTo
		{
			get { return LabelConsol != ZGuid.Empty ? (ZInt)(ConsolTotalRangeFrom + (LabelRangeTo - LabelRangeFrom)) : ZInt.Zero; }
		}

		public ZPropertyInfo ConsolTotalRangeToInfo
		{
			get { return GetZPropertyInfo(Schema.ConsolTotalRangeTo); }
		}

		[ReadOnly(true)]
		public ZInt ConsolTotalPacks
		{
			get { return consolTotalPacks; }
			set { SetNonPersistentPropertyValue(ConsolTotalPacksInfo, ref consolTotalPacks, value); }
		}
		ZInt consolTotalPacks;

		public ZPropertyInfo ConsolTotalPacksInfo
		{
			get { return GetZPropertyInfo(Schema.ConsolTotalPacks); }
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList ConsolNames
		{
			get { return consolNames ?? (consolNames = GetConsolNames()); }
		}
		CodeDescriptionPairList consolNames;

		#endregion

		#region Xml Serialization

		protected override List<ZPropertyInfo> GetPropertiesToSerializeCore()
		{
			List<ZPropertyInfo> result = base.GetPropertiesToSerializeCore();
			result.Add(ConsolTotalPiecesFromMAWBInfo);
			return result;
		}

		#endregion

		#region Implementation

		void RefreshConsolRangeData()
		{
			var consol = Factory.Load<ForwardingConsol>(LabelConsol);

			if (consol != null)
			{
				if (ConsolTotalPiecesFromMAWB && consol.IsAWBHeaderAccessible)
				{
					if (!consolPKsWithPopulatedAWB.Contains(consol.PK))
					{
						consol.PopulateAWB();
						consolPKsWithPopulatedAWB.Add(consol.PK);
					}

					ConsolTotalPacks = consol.AWBHeader.EH_TotalNoOfPieces;
				}
				else
				{
					ConsolTotalPacks = (ZInt)consol.JK_TotalShipmentQuantity;
				}

				ConsolTotalRangeFrom = 1;
			}
			else
			{
				ConsolTotalRangeFrom = ConsolTotalPacks = 0;
			}
		}

		CodeDescriptionPairList GetConsolNames()
		{
			var consolsCodeDescriptionPairList = new CodeDescriptionPairList();

			if (shipment != null)
			{
				foreach (ForwardingConsol consol in shipment.Consols.Cast<ForwardingConsol>().Where(consol => consol.IsAWBHeaderAccessible))
				{
					consolsCodeDescriptionPairList.AddPair(consol.PK, consol.JK_UniqueConsignRef, string.Format(CultureInfo.InvariantCulture, "{0} -> {1}", consol.JK_RL_NKLoadPort, consol.JK_RL_NKDischargePort));
				}
			}

			return consolsCodeDescriptionPairList;
		}

		public override void PrintAWBBarcodeLabel()
		{
			if (shipment.IsAWBHeaderAccessible)
			{
				var awb = shipment.AWBHeader as ShipmentExportAWBHeader;
				awb.ConsolForAWBLabel = Factory.Load<ForwardingConsol>(LabelConsol);

				if (awb.ConsolForAWBLabel != null && !consolPKsWithPopulatedAWB.Contains(awb.ConsolForAWBLabel.PK))
				{
					awb.ConsolForAWBLabel.PopulateAWB();
				}
			}

			DoPrintBarcodeLabel(DocumentNames.AWBBarcodeLabels);
		}

		protected override void SetDocumentSettingsCore()
		{
			base.SetDocumentSettingsCore();

			AWB.MAWBLabelStartRange = ConsolTotalRangeFrom;
			AWB.MAWBLabelTotalPacks = ConsolTotalPiecesSuppress ? ZInt.Zero : ConsolTotalPacks;
		}

		protected override void SetAdditionalDefaults()
		{
			using (SuspendSettingHasChanges())
			{
				base.SetAdditionalDefaults();

				var airConsol = shipment.CurrentBranchDepartureAirConsol;
				if (airConsol != null && ConsolNames.GetAllCodes().ToList().Contains(airConsol.JK_UniqueConsignRef))
				{
					LabelConsol = airConsol.PK;
				}

				ConsolTotalPiecesSuppress = !FreightDataRegistry.Instance.PrintTotalNumberOfPiecesOnShipmentAWBBarcodeLabel.Value;
				ConsolTotalPiecesFromConsol = !ConsolTotalPiecesFromMAWB;

				ValidateLabelConsol();
				ValidateConsolTotalRangeFrom();
			}
		}

		#endregion
	}
}
