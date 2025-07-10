import { IEntity } from "./IEntity";

interface IQrtzJobDetails extends IEntity {
	JOB_PK: string,
	SCHED_NAME: string,
	JOB_NAME: string,
	JOB_GROUP: string,
	DESCRIPTION: string,
	JOB_CLASS_NAME: string,
	CountryCode: string,
	ProgramArgs: string,
	ProgramExePath: string
}

export default IQrtzJobDetails;
