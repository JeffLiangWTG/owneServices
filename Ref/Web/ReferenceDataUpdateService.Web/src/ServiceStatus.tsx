import axios from "axios";
import React, { useEffect, useState } from "react";

type ServiceStatusesToShow = "On" | "Off";

interface ServiceStatusProps {
	serviceUrl: string;
	serviceName: string;
	updateIntervalInMs?: number;
}

export const ServiceStatus = ({
	serviceName,
	serviceUrl,
	updateIntervalInMs,
}: ServiceStatusProps) => {
	const [serviceStatus, setServiceStatus] = useState<ServiceStatusesToShow>();

	useEffect(() => {
		if (serviceName && serviceUrl) {
			const healthCheck = async () => {
				try {
					const response = await axios.get(serviceUrl);
					if (
						response.status == 200 &&
						(response.data as string).toLocaleUpperCase().includes("IS OK")
					) {
						setServiceStatus("On");
					} else {
						setServiceStatus("Off");
					}
				} catch {
					setServiceStatus("Off");
				}
			};
			healthCheck();
			const healthCheckInterval = setInterval(
				healthCheck,
				updateIntervalInMs ?? 60000 //interval is set or default to 1min
			);

			return () => {
				clearInterval(healthCheckInterval);
			};
		}
	}, []);

	return (
		<div className="card m-1 bg-light" style={{ maxWidth: "7rem" }}>
			<div className="card-body">
				<h6>{serviceName}</h6>
				<span
					className={`badge badge-${
						serviceStatus == "On" ? "success" : "danger"
					}`}
				>
					{serviceStatus}
				</span>
			</div>
		</div>
	);
};
