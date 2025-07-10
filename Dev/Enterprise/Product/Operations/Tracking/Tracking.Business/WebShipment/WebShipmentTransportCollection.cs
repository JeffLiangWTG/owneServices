using System;

using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public class WebShipmentTransportCollection : DynamicBusinessObjectCollection<WebShipmentTransport>
	{
		public WebShipmentTransportCollection(BusinessObjectFactory factory, ZGuid parentShipmentPK) : base(factory)
		{
			fParentShipmentPK = parentShipmentPK;
		}

		public void Load()
		{
			ZSqlParameter shipmentPK1 = ZSqlParameter.New("@ShipmentPK1", ParentShipmentPK, JobConShipLinkSchema.JN_JS);
			ZSqlParameter shipmentPK2 = ZSqlParameter.New("@ShipmentPK2", ParentShipmentPK, JobConShipLinkSchema.JN_JS);
			ZSqlParameter emptyDateReplacement = ZSqlParameter.New("@EmptyDateReplacement", ZDateTime.MaxSmallDateTime, JobConsolTransportSchema.JW_ATD);
			ZSqlParameter noReplacement = ZSqlParameter.New("@NoReplacement", "N", JobConsolTransportSchema.JW_IsLinked);

			Load(QueryString, new[] { shipmentPK1, shipmentPK2, emptyDateReplacement, noReplacement });
		}

		#region Implementation

		protected ZGuid ParentShipmentPK
		{
			get { return fParentShipmentPK; }
		}
		readonly ZGuid fParentShipmentPK;

		protected string QueryString
		{
			get
			{
				return String.Format(
					@"SELECT  {0}, {1}, {2}, 
CASE {45} WHEN {46} THEN {3} ELSE {41} END AS {3},
CASE {45} WHEN {46} THEN {4} ELSE {42} END AS {4},
CASE {45} WHEN {46} THEN {5} ELSE {43} END AS {5},
CASE {45} WHEN {46} THEN {6} ELSE {44} END AS {6},
ISNULL({7}, '') AS {7},
ISNULL({8}, '') AS {8},
ISNULL({9}, '') AS {9},
{10}, {11},
ISNULL({16}, {12}) AS {16},
ISNULL({17}, {13}) AS {17},
ISNULL({18}, {14}) AS {18},
ISNULL({19}, {15}) AS {19}
FROM {20} 
LEFT OUTER JOIN {21}  ON ({22} = {23} OR {22} = {24})
LEFT OUTER JOIN dbo.JobConsol  ON {25} = {23}
LEFT OUTER JOIN {26}  ON {27} = {28}
LEFT OUTER JOIN {29}  ON {30} = {31}
LEFT OUTER JOIN {32}  ON {33} = {34}
LEFT OUTER JOIN {35}  ON {36} = {37}
WHERE {38} = {39}
ORDER BY ISNULL(ISNULL(CASE {45} WHEN {46} THEN {5} ELSE {43} END, CASE {45} WHEN {46} THEN {3} ELSE {41} END), {40})",
					JobConsolTransportSchema.JW_LegOrder.Name, // 0
					JobConsolTransportSchema.JW_TransportMode.Name, // 1
					JobConsolTransportSchema.JW_TransportType.Name, // 2
					JobConsolTransportSchema.JW_ETD.Name, // 3
					JobConsolTransportSchema.JW_ETA.Name, // 4
					JobConsolTransportSchema.JW_ATD.Name, // 5
					JobConsolTransportSchema.JW_ATA.Name, // 6
					JobConsolSchema.JK_UniqueConsignRef.Name, // 7
					JobConsolSchema.JK_MasterBillNum.Name, // 8
					JobConsolSchema.JK_AgentType.Name, // 9
					JobConsolSchema.JK_OA_SendingForwarderAddress.Name, // 10
					JobConsolSchema.JK_OA_ReceivingForwarderAddress.Name, // 11
					JobConsolTransportSchema.JW_RL_NKLoadPort.Name, // 12
					JobConsolTransportSchema.JW_RL_NKDiscPort.Name, // 13
					JobConsolTransportSchema.JW_Vessel.Name, // 14
					JobConsolTransportSchema.JW_VoyageFlight.Name, // 15
					JobVoyOriginSchema.JA_RL_NKPortOfLoading.Name, // 16
					JobVoyDestinationSchema.JB_RL_NKPortOfDischarge.Name, // 17
					JobVoyageSchema.JV_RV_NKVessel.Name, // 18
					JobVoyageSchema.JV_VoyageFlight.Name, // 19
					JobConShipLinkSchema.PK.TableName, // 20
					JobConsolTransportSchema.PK.TableName, // 21
					JobConsolTransportSchema.JW_ParentGUID.Name, // 22
					JobConShipLinkSchema.JN_JK.Name, // 23
					"@ShipmentPK1", // 24
					JobConsolSchema.PK.Name, // 25
					JobSailingSchema.PK.TableName, // 26
					JobSailingSchema.PK.Name, // 27
					JobConsolTransportSchema.JW_JX.Name, // 28
					JobVoyOriginSchema.PK.TableName, // 29
					JobVoyOriginSchema.PK.Name, // 30
					JobSailingSchema.JX_JA.Name, // 31
					JobVoyDestinationSchema.PK.TableName, // 32
					JobVoyDestinationSchema.PK.Name, // 33
					JobSailingSchema.JX_JB.Name, // 34
					JobVoyageSchema.PK.TableName, // 35
					JobVoyageSchema.PK.Name, // 36
					JobVoyOriginSchema.JA_JV.Name, // 37
					JobConShipLinkSchema.JN_JS.Name, // 38
					"@ShipmentPK2", // 39
					"@EmptyDateReplacement", // 40
					JobVoyOriginSchema.JA_E_DEP.Name, //41
					JobVoyDestinationSchema.JB_E_ARV.Name, //42
					JobVoyOriginSchema.JA_A_DEP.Name, //43
					JobVoyDestinationSchema.JB_A_ARV.Name, //44
					JobConsolTransportSchema.JW_IsLinked.Name,//45
					"@NoReplacement"//46
					);
			}
		}
		#endregion
	}
}
