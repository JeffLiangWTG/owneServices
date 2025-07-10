import React, { PropsWithChildren } from "react";

interface ClientVersionsLinkProps {
	systemType?: string | null;
	clientId?: string | null;
	lastUpdatedUTCFrom: string;
	dataSet?: string | null;
}

declare var BASENAME: string;

export const ClientVersionsLink = ({
	dataSet,
	lastUpdatedUTCFrom,
	systemType,
	clientId,
	children,
}: PropsWithChildren<ClientVersionsLinkProps>) => {
	return (
		<>
			<a
				href={`${BASENAME}/ClientVersions?lastUpdatedUTCFrom=${lastUpdatedUTCFrom}&isLate=true${
					dataSet ? "&dataSet=" + dataSet : ""
				}${systemType ? "&systemType=" + systemType : ""}${
					clientId ? "&clientId=" + clientId : ""
				}`}
			>
				{children}
			</a>
		</>
	);
};
