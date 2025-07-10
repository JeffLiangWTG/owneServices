using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class CommonPickupDeliveryConfirmCollection : ActiveBusinessObjectCollection<CommonPickupDeliveryConfirm>
	{
		public CommonPickupDeliveryConfirmCollection(BusinessObject master)
			: base(master)
		{
			stackTrace = System.Environment.StackTrace;
		}

		public CommonPickupDeliveryConfirmCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			stackTrace = System.Environment.StackTrace;
		}

		public CommonPickupDeliveryConfirmCollection(BusinessObjectFactory factory, CommonPickupDeliveryConfirmRelationship relationship)
			: base(factory, relationship)
		{
			this.packLineType = GetPackLineType();
			stackTrace = System.Environment.StackTrace;
		}

		public CommonPickupDeliveryConfirmCollection(BusinessObjectFactory factory, CommonShipment shipment, string pickupDeliveryType)
			: base(factory, new ShipmentPickupDeliveryConfirmRelationship(shipment, pickupDeliveryType))
		{
			this.packLineType = GetPackLineType();
			this.shipment = shipment;
			this.pickupDeliveryType = pickupDeliveryType;

			stackTrace = string.IsNullOrEmpty(pickupDeliveryType) ? System.Environment.StackTrace : string.Empty;
		}

		public class CommonPickupDeliveryConfirmRelationship : ManyToManyRelationship
		{
			public CommonPickupDeliveryConfirmRelationship(BusinessObject master, Type elementType, Type pivotObjectType, ZQuery filter, SchemaGuidColumn pivotTableFKToMaster, SchemaGuidColumn pivotTableFKToElements) : base(master, elementType, pivotObjectType, filter, pivotTableFKToMaster, pivotTableFKToElements)
			{
			}

			[ThreadStatic]
			internal static string debugLogMessage;

			protected override string DebugLogMessage => debugLogMessage;

			protected override string GetPivotDebugInformation(BusinessObject pivot, IEnumerable<BusinessObject> allPivots)
			{
				return GetDivotDebugInformation(pivot as CommonConfirmDivot, allPivots.Cast<CommonConfirmDivot>());
			}

			internal static string GetDivotDebugInformation(CommonConfirmDivot divot, IEnumerable<CommonConfirmDivot> allDivots)
			{
				Argument.NotNull(divot, nameof(divot));
				Argument.NotNull(allDivots, nameof(allDivots));

				var duplicates = allDivots.Where(d => d.J8_JL == divot.J8_JL && d.J8_EU_PickupDeliverConfirm == divot.J8_EU_PickupDeliverConfirm).ToArray();
				var confirms = duplicates.Select(d => d.Confirm).Distinct().ToArray();

				var debugLogMessage = FormattableString.Invariant($"Duplicate divots ({duplicates.Length}):\r\n");
				foreach (var currentDivot in duplicates)
				{
					debugLogMessage += GetDivotLogString(currentDivot) + "\r\n";
					debugLogMessage += "PackLine: " + GetPackLineLogString(currentDivot.PackLine) + "\r\n";
				}

				debugLogMessage += FormattableString.Invariant($"\r\nConfirms of duplicate divots ({confirms.Length}):\r\n");
				foreach (var confirm in confirms)
				{
					debugLogMessage += GetConfirmLogString(confirm) + "\r\n";
				}

				return debugLogMessage;
			}

			public static string GetDivotLogString(CommonConfirmDivot divot)
			{
				Argument.NotNull(divot, nameof(divot));

				return FormattableString.Invariant($@"Divot PK: '{divot.PK}',
J8_JL: '{divot.J8_JL}',
J8_EU_PickupDeliverConfirm: '{divot.J8_EU_PickupDeliverConfirm}',
J8_PackagesDelivered: '{divot.J8_PackagesDelivered}',
J8_DeliveryWeight: '{divot.J8_DeliveryWeight}',
J8_DeliveryVolume: '{divot.J8_DeliveryVolume}',
IsInDatabase: '{divot.IsInDatabase}',
J8_SystemCreateTimeUtc: '{divot.J8_SystemCreateTimeUtc}',
J8_SystemCreateUser: '{divot.J8_SystemCreateUser}'");
			}

			public static string GetPackLineLogString(PackLine packLine)
			{
				Argument.NotNull(packLine, nameof(packLine));

				return FormattableString.Invariant($@"IsInDatabase: '{packLine.IsInDatabase}', JL_SystemCreateTimeUtc: '{packLine.JL_SystemCreateTimeUtc}', JL_SystemCreateUser: '{packLine.JL_SystemCreateUser}', JL_SystemLastEditTimeUtc: '{packLine.JL_SystemLastEditTimeUtc}', JL_SystemLastEditUser: '{packLine.JL_SystemLastEditUser}'");
			}

			public static string GetConfirmLogString(CommonPickupDeliveryConfirm confirm)
			{
				if (confirm == null)
				{
					return (NoResString)"CommonPickupDeliveryConfirm is null";
				}
				return FormattableString.Invariant($@"IsInDatabase: '{confirm.IsInDatabase}', EU_PickupDeliveryType: '{confirm.EU_PickupDeliveryType}', EU_JS: '{confirm.EU_JS}', EU_D1: '{confirm.EU_D1}', EU_JC: '{confirm.EU_JC}'
EU_SystemCreateTimeUtc: '{confirm.EU_SystemCreateTimeUtc}', EU_SystemCreateUser: '{confirm.EU_SystemCreateUser}', EU_SystemLastEditTimeUtc: '{confirm.EU_SystemLastEditTimeUtc}', EU_SystemLastEditUser: '{confirm.EU_SystemLastEditUser}'");
			}
		}

		readonly Type packLineType;
		readonly CommonShipment shipment;
		readonly string pickupDeliveryType;
		readonly string stackTrace;

		Type GetPackLineType()
		{
			if (Relationship == null)
			{
				return typeof(PackLine);
			}

			var packLine = Relationship.Master as PackLine;
			if (packLine != null)
			{
				return packLine.GetType();
			}

			var parentShipment = Relationship.Master as CommonShipment;
			if (parentShipment != null)
			{
				return parentShipment.OuterPackLines.TypeOfElements;
			}

			return typeof(PackLine);
		}

		#region Overrides

		internal bool AllowsNewItems { get => AllowNew; }

		protected override bool AllowNew => (shipment != null
											&& !shipment.IsCoLoadMaster
											&& !shipment.IsBlindCoLoadMaster
											&& !shipment.IsAssemblyMaster
											&& pickupDeliveryType != Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture)
											&& Relationship.SupportsAddToRelationship();

		protected override void OnLoadingIntoCollectionCore(CommonPickupDeliveryConfirm loadingObject)
		{
			base.OnLoadedIntoCollectionCore(loadingObject);
			loadingObject.PackLineType = packLineType;
		}

		protected override void SetRelationshipDefaultsForElementCore(CommonPickupDeliveryConfirm newElement, bool throwIfRelationshipNotSupported)
		{
			newElement.PackLineType = packLineType;
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
		}

		protected override void OnAddIntoRelationshipCore(BusinessObject businessObject)
		{
			base.OnAddIntoRelationshipCore(businessObject);

			var pickupDelivery = (CommonPickupDeliveryConfirm)businessObject;

			if (shipment == null
				|| string.IsNullOrWhiteSpace(pickupDeliveryType)
				|| pickupDelivery == null
				|| pickupDelivery.EU_JS != shipment.PK)
			{
				if (shipment != null)
				{
					pickupDelivery.DebugLog.AppendLine(FormattableString.Invariant($"OnAddIntoRelationshipCore - Skipping creating divot for Confirm: '{pickupDelivery.PK}', pickupDeliveryType: '{pickupDeliveryType}', pickupDelivery.EU_JS: '{pickupDelivery.EU_JS}', shipment.PK: '{shipment.PK}'"));
				}
				else
				{
					pickupDelivery.DebugLog.AppendLine(FormattableString.Invariant($"OnAddIntoRelationshipCore - Skipping creating divot for Confirm: '{pickupDelivery.PK}', pickupDeliveryType: '{pickupDeliveryType}', pickupDelivery.EU_JS: '{pickupDelivery.EU_JS}', shipment: null"));
				}

				return;
			}

			foreach (var packLine in shipment.OuterPackLines.OfType<PackLine>())
			{
				if (!pickupDelivery.Divots.Any(x => x.PackLine?.PK == packLine.PK))
				{
					CommonPickupDeliveryConfirmRelationship.debugLogMessage += FormattableString.Invariant($"\r\nOnAddIntoRelationshipCore - Packline: '{packLine.PK}'. Confirm is already in Database: '{pickupDelivery.IsInDatabase}'");
					pickupDelivery.CreateDivot(packLine);
				}
				else
				{
					pickupDelivery.DebugLog.AppendLine(FormattableString.Invariant($"OnAddIntoRelationshipCore - Divot already exists for Packline: '{packLine.PK}'"));
				}
			}
		}

		#endregion

		protected override void OnAdded(CommonPickupDeliveryConfirm confirm)
		{
			base.OnAdded(confirm);

			if (confirm == null)
			{
				return;
			}

			if (string.IsNullOrEmpty(pickupDeliveryType))
			{
				confirm.DebugLog.AppendLine(FormattableString.Invariant($"CommonPickupDeliveryConfirmCollection::OnAdded, pickupDeliveryType is null or empty."));
				confirm.DebugLog.AppendLine(stackTrace);
			}

			if (confirm.EU_PickupDeliveryType.IsEmpty && !confirm.EU_JS.IsEmpty)
			{
				confirm.DebugLog.AppendLine(FormattableString.Invariant($"CommonPickupDeliveryConfirmCollection::OnAdded, EU_PickupDeliveryType is empty, EU_JS = {confirm.EU_JS}."));
				confirm.DebugLog.AppendLine(stackTrace);
			}
		}
	}
}
