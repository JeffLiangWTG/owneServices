using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Enterprise.ZArchitecture.Environment;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Document
{
	public abstract class CIN750NotificationWriter<TNotificationSource> : DataObjectWriter<TNotificationSource, UniversalShipment> where TNotificationSource : CIN750Notification
	{
		public CIN750NotificationWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(TNotificationSource notification)
		{
			var shipment = CreateUniveralShipment(notification);
			PopulateAdditionalReferences(notification, shipment);
			PopulateAddresses(notification, shipment);
			PopulatePackingLines(notification, shipment);
			PopulateMessageType(shipment);
			PopulateAddInfo(notification, shipment);

			return shipment;
		}

		#region PopulateMessageType

		void PopulateMessageType(UniversalShipment shipment)
		{
			var types = new CIN750MessageTypes();
			var codeDescriptionPair = types[types.IndexOfCode(GetMessageCode())];
			shipment.MessageType = new CodeDescriptionPair()
			{
				Code = codeDescriptionPair.Code,
				Description = codeDescriptionPair.Description
			};
		}

		protected abstract ZString GetMessageCode();

		#endregion

		#region PopulateAdditionalReferences

		protected virtual void PopulateAdditionalReferences(TNotificationSource notification, UniversalShipment shipment)
		{
			var additionalReferences = new DataObjectList<AdditionalReference>() {
				new AdditionalReference()
					{
						Type = new EntryType { Code = notification.RefType.Code, Description = notification.RefType.Description },
						ReferenceNumber = notification.RefCode,
						ContextInformation = notification.EnterpriseAndServerCode,
						IssueDate = notification.MovementTime
					}
				};
			shipment.SetAdditionalReferenceCollection(() => additionalReferences);
		}

		#endregion

		#region PopulateAddresses

		protected void PopulateAddresses(TNotificationSource notification, UniversalShipment shipment)
		{
			shipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();
				foreach (var addressSource in GetAddressSources(notification))
				{
					addresses.Add(GetNotificationAddress(addressSource.AddressType, addressSource.Address, addressSource.RegistrationNumber));
				}
				return addresses.Count > 0 ? addresses : null;
			});
		}

		protected OrganizationAddress GetNotificationAddress(string addressType, Address address, RegistrationNumber registrationNumber)
		{
			return address.ToUXmlOrganizationAddress(addressType, writeManager.WriterStrategy, new List<RegistrationNumber>() { registrationNumber });
		}

		protected abstract IEnumerable<(string AddressType, Address Address, RegistrationNumber RegistrationNumber)> GetAddressSources(TNotificationSource notification);

		#endregion

		#region PopulatePackingLines

		protected virtual void PopulatePackingLines(TNotificationSource notification, UniversalShipment uxmlShipment)
		{
			var packinglineCollection = new DataObjectList<PackingLine>();
			foreach (var docPackingLine in notification.Goods)
			{
				packinglineCollection.Add(GetPackingLine(docPackingLine));
			}
			uxmlShipment.SetPackingLineCollection(() => packinglineCollection);
		}

		protected virtual PackingLine GetPackingLine(DocPackingLine docPackingLine)
		{
			var packline = new PackingLine(writeManager.WriterStrategy);
			packline.PackQty = (long)docPackingLine.AmountQuantity;
			packline.Weight = docPackingLine.AmountWeight;
			packline.WeightUnit = new UnitOfWeight()
			{
				Code = Core.Constants.Weight.Kilograms,
				Description = nameof(Core.Constants.Weight.Kilograms)
			};
			packline.GoodsDescription = docPackingLine.Description;
			packline.SetAddInfoCollection(() => GetPackingLineAddInfo(docPackingLine).ToList());
			return packline;
		}

		protected abstract IEnumerable<AddInfo> GetPackingLineAddInfo(DocPackingLine docPackingLine);

		#endregion

		#region CreateUniveralShipment

		UniversalShipment CreateUniveralShipment(TNotificationSource notification)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = notification
				.CreateUXmlDataContext()
				.AddDataProvider()
				.AddUserBranchAndDepartment();

			return shipment;
		}

		#endregion

		#region PopulateAddInfo

		protected virtual void PopulateAddInfo(TNotificationSource notification, UniversalShipment shipment)
			=> shipment.SetAddInfoCollection(() => new List<AddInfo>() {
				new AddInfo() { Key = CIN750AddInfoConstants.Keys.JobID, Value = notification.JobID },
				new AddInfo() { Key = CIN750AddInfoConstants.Keys.MessageID, Value = notification.MessageID },
				new AddInfo() { Key = CIN750AddInfoConstants.Keys.ServerType, Value = GetServerType() }
			});

		#endregion

		#region GetServerType

		protected string GetServerType() => EnvProxy.Instance.IsProductionSystem ? CIN750AddInfoConstants.Values.ProductionServer : CIN750AddInfoConstants.Values.TestingServer;

		#endregion
	}

	public static class CIN750AddInfoConstants
	{
		public static class Keys
		{
			public const string JobID = "JID";
			public const string MessageID = "MID";
			public const string ServerType = "STP";
		}
		public static class Values
		{
			public const string ProductionServer = "PRD";
			public const string TestingServer = "TST";
		}
	}
}
