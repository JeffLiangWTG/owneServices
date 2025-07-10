import type ISourceDataUserView from "./models/ISourceDataUserView";
import { IEntity } from "./models/IEntity";
import IProcessorStatus from "./models/IProcessorStatus";
import moment from "moment";
import React from "react";

type KibanaLinkProps = {
	entity: IEntity;
};
declare var __KibanaRootUrl__: string;
declare var __KibanaIndexSearch__: string;

export const KibanaLink = ({ entity }: KibanaLinkProps) => {
	const getDateToSearch = (date: Date) => {
		return moment(date).add(-30, "minutes").format("yyyy-MM-DD HH:mm:ss");
	};

	const isSourceData = "SDA_PK" in entity;
	const isProcessorStatus = "PRC_PK" in entity;

	const shouldRender = () => {
		if (isProcessorStatus) {
			return true;
		} else if (
			(isSourceData && (entity as ISourceDataUserView).SDA_Status == "ERR") ||
			(entity as ISourceDataUserView).SDA_Status == "FIE"
		) {
			return true;
		}

		return false;
	};

	const preSelectedColumns = ['fields.AppLog.Message']

	const renderLinkByEntityType = () => {
		if (isSourceData) {
			const sourceData = entity as ISourceDataUserView;
			return `${__KibanaRootUrl__}/?_g=(filters:!(),refreshInterval:(pause:!t,value:60000),time:(from:'${getDateToSearch(
				sourceData.SDA_CreatedTime
			)}',to:'now'))&_a=(columns:!(${preSelectedColumns.join(',')}),filters:!(),hideChart:!f,index:'${__KibanaIndexSearch__}',interval:auto,query:(language:kuery,query:"${
				sourceData.SDA_PK
			}"),sort:!(!('@timestamp',desc)))`;
		} else if (isProcessorStatus) {
			const processorStatus = entity as IProcessorStatus;
			return `${__KibanaRootUrl__}/?_g=(filters:!(),refreshInterval:(pause:!t,value:60000),time:(from:'${getDateToSearch(
				processorStatus.PRC_LastRunTime
			)}',to:'now'))&_a=(columns:!(${preSelectedColumns.join(',')}),filters:!(('$state':(store:appState),meta:(alias:!n,disabled:!f,field:fields.SourceContext,index:'${__KibanaIndexSearch__}',key:fields.SourceContext,negate:!f,params:(query:'${
				processorStatus.PRC_JobName
			}'),type:phrase),query:(match_phrase:(fields.SourceContext:'${
				processorStatus.PRC_JobName
			}')))),hideChart:!f,index:'${__KibanaIndexSearch__}',interval:auto,query:(language:kuery,query:''),sort:!(!('@timestamp',desc)))`;
		} else {
			throw new Error("Invalid entity type usage for Kibana Link");
		}
	};

	return shouldRender() ? (
		<a target="_blank" href={renderLinkByEntityType()}>
			Log
		</a>
	) : null;
};
