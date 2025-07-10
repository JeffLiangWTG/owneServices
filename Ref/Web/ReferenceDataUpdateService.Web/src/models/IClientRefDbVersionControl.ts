export interface IClientRefDbVersionControl {
	CVC_PK: string;
	CVC_DataSet: string;
	CVC_ClientId: string;
	CVC_DataSetTimestamp: Date;
	CVC_DateSetCheckpoint: string;
	CVC_SystemType: string;
	CVC_LastUpdatedTimeUTC: Date;
	CVC_IsInUse: boolean;
}
