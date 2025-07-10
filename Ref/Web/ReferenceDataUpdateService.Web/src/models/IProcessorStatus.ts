import { IEntity } from "./IEntity";

interface IProcessorStatus extends IEntity {
	PRC_PK: string;
	PRC_JobName: string;
	PRC_JobGroup: string;
	PRC_LastRunTime: Date;
	PRC_LastSuccessRunTime: Date | null;
	PRC_Status: string;
}

export default IProcessorStatus;
