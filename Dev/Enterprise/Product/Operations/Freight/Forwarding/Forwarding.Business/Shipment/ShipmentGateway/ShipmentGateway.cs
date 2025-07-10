using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ShipmentGateway : AutoJobShipmentGateway
	{
		public ShipmentGateway(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoJobShipmentGateway.Schema
		{
			public const string ForwarderPK = "ForwarderPK";
		}

		#endregion

		#region Properties

		#region JSG_JS_Shipment

		[RelatedBusinessObject(nameof(Shipment))]
		public override ZGuid JSG_JS_Shipment
		{
			get => base.JSG_JS_Shipment;
			set => base.JSG_JS_Shipment = value;
		}

		#endregion

		#region JSG_OA_ForwarderAddress

		public override ZGuid JSG_OA_ForwarderAddress
		{
			get => base.JSG_OA_ForwarderAddress;
			set
			{
				if (value != base.JSG_OA_ForwarderAddress)
				{
					base.JSG_OA_ForwarderAddress = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateForwarderPK();
					}
				}
			}
		}

		[List(nameof(ForwarderList))]
		public ZGuid ForwarderPK
		{
			get { return JSG_OA_ForwarderAddress_ZAddress.OrgPK; }
			set
			{
				JSG_OA_ForwarderAddress_ZAddress.OrgPK = value;
				ForwarderPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ForwarderPKInfo
		{
			get { return GetZPropertyInfo(Schema.ForwarderPK); }
		}

		public OrgHeader Forwarder
		{
			get { return ForwarderAddress != null ? Factory.Load<OrgHeader>(ForwarderAddress.OA_OH) : null; }
		}

		protected override ZAddress GetNewJSG_OA_ForwarderAddress_ZAddress()
		{
			ZAddress result = base.GetNewJSG_OA_ForwarderAddress_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			result.GetDefaultAddress = header => GetMainAddressPK(header as OrgHeader);

			return result;
		}

		ZGuid GetMainAddressPK(OrgHeader header)
		{
			return header != null ? header.MainAddress.PK : ZGuid.Empty;
		}

		public ForwarderCollection ForwarderList
		{
			get { return forwarderList ?? (forwarderList = new ForwarderCollection(Factory)); }
		}
		ForwarderCollection forwarderList;

		#endregion

		#region JSG_Sequence

		public bool JSG_Sequence_ReadOnly => true;

		#endregion

		#endregion

		#region Related Business Objects

		public ForwardingShipment Shipment => Factory.Load<ForwardingShipment>(JSG_JS_Shipment);

		#endregion

		#region OnUpdatedByDataRefresh

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			JSG_OA_ForwarderAddressInfo.RefreshBinding();
		}

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new ShipmentGatewayUniqueIndexFailureHandler(this)); }
		}
		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		public class ShipmentGatewayUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public ShipmentGatewayUniqueIndexFailureHandler(ShipmentGateway gateway)
			{
				shipmentGateway = gateway;
			}

			readonly ShipmentGateway shipmentGateway;

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get
				{
					yield return JobShipmentGatewaySchema.Constants.Indexes.FK_UC__JSG_JS_Shipment_JSG_Sequence;
					yield return JobShipmentGatewaySchema.Constants.Indexes.FK_UX__JSG_OA_ForwarderAddress_JSG_Sequence_JSG_JS_Shipment;
				}
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var message = Res.GetString("6848fb49-0f7a-4bed-92c4-ed3857d2662e", "{0} cannot be saved because its Gateways have been modified by another user. Please close and re-open the Shipment to continue.", shipmentGateway.Shipment?.HumanReadableName);
				notifier.ReportError(message, Res.GetString("66071229-feba-4b3d-b4e6-1fe36fbb8dcc", "Information"));
			}
		}

		#endregion

		#region Validation

		protected override JobShipmentGatewayValidation GetNewValidation()
		{
			return new ShipmentGatewayValidation(this);
		}

		public new ShipmentGatewayValidation Validation => (ShipmentGatewayValidation)base.Validation;

		#endregion
	}
}
