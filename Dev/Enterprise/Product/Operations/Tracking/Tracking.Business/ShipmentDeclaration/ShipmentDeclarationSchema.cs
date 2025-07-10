using System;
using System.Data;
using CargoWise.Schema;

namespace Enterprise.Tracking.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Baseline")]
	public sealed class ShipmentDeclarationSchema : Schema, ITableSchema
	{
		public static class Constants
		{
			public const string PersistentBizOPK = "PersistentBizOPK";

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
			public const string Number = "Number";
			public const string HouseBill = "HouseBill";
			public const string MasterBill = "MasterBill";

			public const string ConsignorPK = "ConsignorPK";
			public const string ConsignorName = "ConsignorName";
			public const string ConsignorFullAddress = "ConsignorFullAddress";
			public const string ConsignorAddress = "ConsignorAddress";
			public const string ConsignorCity = "ConsignorCity";
			public const string ConsignorState = "ConsignorState";
			public const string ConsignorPostCode = "ConsignorPostCode";

			public const string ConsigneePK = "ConsigneePK";
			public const string ConsigneeName = "ConsigneeName";
			public const string ConsigneeFullAddress = "ConsigneeFullAddress";
			public const string ConsigneeAddress = "ConsigneeAddress";
			public const string ConsigneeCity = "ConsigneeCity";
			public const string ConsigneeState = "ConsigneeState";
			public const string ConsigneePostCode = "ConsigneePostCode";

			public const string OriginPortCode = "OriginPortCode";
			public const string DestinationPortCode = "DestinationPortCode";
			public const string CurrentLoadPort = "CurrentLoadPort";
			public const string CurrentDischargePort = "CurrentDischargePort";
			public const string MainLoadPort = "MainLoadPort";
			public const string MainDischargePort = "MainDischargePort";

			public const string ETA = "ETA";
			public const string ETDWithSuppression = "ETDWithSuppression";
			public const string ETAWithSuppression = "ETAWithSuppression";

			public const string MainVessel = "MainVessel";
			public const string MainVoyageWithSuppression = "MainVoyageWithSuppression";
			public const string CurrentVessel = "CurrentVessel";
			public const string CurrentVoyageWithSuppression = "CurrentVoyageWithSuppression";

			public const string BookingReference = "BookingReference";
			public const string OwnerReference = "OwnerReference";
			public const string TransportMode = "TransportMode";

			public const string PacksWithUnits = "PacksWithUnits";
			public const string VolumeWithUnits = "VolumeWithUnits";
			public const string WeightWithUnits = "WeightWithUnits";

			public const string GoodsValue = "GoodsValue";
			public const string GoodsValueCurrency = "GoodsValueCurrency";
			public const string GoodsDescription = "GoodsDescription";

			public const string EstimatedPickupDate = "EstimatedPickupDate";
			public const string PickupDateRequiredBy = "PickupDateRequiredBy";
			public const string EstimatedDeliveryDate = "EstimatedDeliveryDate";
			public const string DeliveryDateRequiredBy = "DeliveryDateRequiredBy";
			public const string DeliveryDate = "DeliveryDate";
			public const string ActualPickupDate = "ActualPickupDate";

			public const string ServiceLevelCode = "ServiceLevelCode";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
			public const string Charges = "Charges";

			public const string ReceivedDate = "ReceivedDate";
			public const string ReceivedBy = "ReceivedBy";
			public const string PiecesReceived = "PiecesReceived";

			public const string ShipmentType = "ShipmentType";

			public const string BookedOnline = "BookedOnline";

			public const string Top3Containers = "Top3Containers";
			public const string OrderReference = "OrderReference";

			public const string SendingForwarderPK = "SendingForwarderPK";
			public const string ReceivingForwarderPK = "ReceivingForwarderPK";

			public const string LoadingMeters = "LoadingMeters";

			public const string ContainerMode = "ContainerMode";

			public const string ChargesApply = "ChargesApply";
			public const string ReleaseType = "ReleaseType";
			public const string OnBoard = "OnBoard";

			public const string AdditionalTerms = "AdditionalTerms";
			public const string InspectionTypeCode = "InspectionTypeCode";
			public const string PaymentTerm = "PaymentTerm";

			public const string DeliveryAgentFullName = "DeliveryAgentFullName";
			public const string PickupAgentFullName = "PickupAgentFullName";
			public const string StorageDate = "StorageDate";
			public const string TEUCount = "TEUCount";
			public const string Top3JobNotes = "Top3JobNotes";

			public const string FirstLegLoadETD = "FirstLegLoadETD";
			public const string FirstLegLoadATD = "FirstLegLoadATD";
			public const string LastLegDischargeETA = "LastLegDischargeETA";
			public const string LastLegDischargeATA = "LastLegDischargeATA";
		}
		public static readonly ShipmentDeclarationSchema Instance = new ShipmentDeclarationSchema();
		public readonly static SchemaPKColumn PersistentBizOPK = new SchemaPKColumn(Instance, Constants.PersistentBizOPK);
		public readonly static SchemaDateTimeColumn ETA = new SchemaDateTimeColumn(Instance, Constants.ETA, 0, SqlDbType.SmallDateTime, DBNull.Value, IsNullable);

		#region ITableSchema Members

		SchemaColumnCollection ITableSchema.All
		{
			get
			{
				return new SchemaColumnCollection(PersistentBizOPK, new SchemaColumn[] {
					ETA,
					//DestinationPortCode,
					//OriginPortCode,
					//LoadPortCode,
					//DischargePortCode
				});
			}
		}

		SchemaColumn ITableSchema.GetSchemaColumn(string columnName)
		{
			return (((ITableSchema)this).All)[columnName];
		}

		SchemaPKColumn ITableSchema.PK
		{
			get { return PersistentBizOPK; }
		}

		string ITableSchema.PkIndexName => null;

		string ITableSchema.SqlSchemaName
		{
			get { return string.Empty; }
		}

		string ITableSchema.TableName
		{
			get { return string.Empty; }
		}

		#endregion

	}
}
