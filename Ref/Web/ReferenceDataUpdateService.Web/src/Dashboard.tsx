import React, { useEffect, useState } from "react";
import { IEntityManager, ServiceType } from "./EntityManager";
import { TextFilterModule } from "./TextFilterModule";
import { Filter, FilterOps, IFilter } from "./Filter";
import ISourceDataUserView from "./models/ISourceDataUserView";
import { FilterStrip } from "./FilterStrip";
import { KibanaLink } from "./KibanaLink";
import moment from "moment";
import { ServiceStatus } from "./ServiceStatus";

interface DashboardProps {
	entityManager: IEntityManager;
}

declare var __QuartzHealthCheckURL__: string;
declare var __UpdateServiceHealthCheckURL__: string;
declare var __DeliveryServiceHealthCheckURL__: string;

export const Dashboard = ({ entityManager }: DashboardProps) => {
	const [recordsMessage, setRecordsMessage] = useState("");
	const [filters, setFilters] = useState([
		new TextFilterModule(
			"XML Data Source",
			new Filter("SDA_SubSource", FilterOps.Equals, "" as any, "string"),
			100
		),
		new TextFilterModule(
			"File Name",
			new Filter("SDA_Filename", FilterOps.Contains, "" as any, "string"),
			100
		),
		new TextFilterModule(
			"Start Run Time",
			new Filter(
				"SDA_CreatedTime",
				FilterOps.PlaceholderForSelection,
				"" as any,
				"datetime"
			),
			100
		),
	]);

	const [results, setResults] = useState<ISourceDataUserView[]>([]);

	useEffect(() => {
		getSourceData();
	}, []);

	const getSourceData = async () => {
		const appliedFilters = filters.map((x) => x.filter).filter((x) => x.value);
		let results = await entityManager.getAsync<ISourceDataUserView>(
			"SourceDataUserView",
			[ServiceType.Staging],
			appliedFilters,
			false,
			undefined,
			{ orderBy: ["SDA_CreatedTime desc"] }
		);
		if (results.length == 0) {
			setRecordsMessage("Search returned 0 result.");
		} else if (results.length > 1000) {
			results = results.slice(0, 1000);
			setRecordsMessage(
				"More than 1000 results found. Only showing the first 1000 results."
			);
		} else {
			setRecordsMessage(`${results.length} results`);
		}
		setResults(results);
	};

	const onFilterValueChange = async (
		entity: any,
		name: string,
		value: object
	) => {
		const filter = entity as IFilter;
		if (filter) {
			const updatedFilters = [...filters];
			const index = updatedFilters.findIndex(
				(x) => x.filter.propertyName == filter.propertyName
			);
			updatedFilters[index].filter[name] = value;
			setFilters(updatedFilters);
		}
	};

	const onFilterValueChanged = async (entity: any, name: string) => { };

	const getStatusDescription = (status: string) => {
		switch (status) {
			case "QUE":
				return "Queued";
			case "ERR":
				return "Error";
			case "PRS":
				return "Parsed";
			case "MER":
				return "Merged";
			case "DUP":
				return "Duplicated";
			case "FIE":
				return "Finished with error";
			case "FIN":
				return "Purged after successful merge";
		}
	};

	const getProcessingStatusDescription = (status: string, notProcessedUntil: Date | null) => {
		switch (status) {
			case "QUE":
				return "Waiting";
			case "PRS":
				return notProcessedUntil == null ? "" : "Merging";
			case "ERR":
			case "MER":
			case "DUP":
			case "FIE":
			case "FIN":
				return "";
		}
	}

	const doFilter = async () => {
		await getSourceData();
	};

	const getActualFileName = (fileName: string) => {
		return fileName?.substring(fileName.lastIndexOf("\\") + 1);
	};

	return (
		<>
			<div className="row mt-4">
				<div className="col">
					<span className="h1">Dashboard</span>
				</div>
			</div>
			<div className="row mt-5">
				<div className="col col-8">
					{filters.map((r) => (
						<FilterStrip
							key={r.name}
							onValueChange={onFilterValueChange}
							onValueChanged={onFilterValueChanged}
							filterModule={r}
							entityManager={entityManager}
							serviceType={ServiceType.Staging}
						/>
					))}
					<div className="form-group row">
						<div className="col-sm-1">
							<button type="button" className="btn btn-info" onClick={doFilter}>
								Search
							</button>
						</div>
						<div className="col"></div>
						<div className="col m-auto">
							<span className="text-info">{recordsMessage}</span>
						</div>
					</div>
				</div>
				<div className="col">
					<div className="d-flex flex-row" id="serviceStatus">
						<ServiceStatus
							serviceName="Quartz"
							serviceUrl={__QuartzHealthCheckURL__}
						/>
						<ServiceStatus
							serviceName="Delivery Service"
							serviceUrl={__DeliveryServiceHealthCheckURL__}
						/>
						<ServiceStatus
							serviceName="Update Service"
							serviceUrl={__UpdateServiceHealthCheckURL__}
						/>
					</div>
				</div>
			</div>
			<div className="row">
				<div className="col">
					<table className="table">
						<thead>
							<tr>
								<th className="col-md-1">XML Data Source</th>
								<th className="col-md-2">XML Publication Time</th>
								<th className="col-md-1">Source</th>
								<th className="col-md-1">PK</th>
								<th className="col-md-1">File Name</th>
								<th className="col-md-3">Start Run Time</th>
								<th className="col-md-1">XML Status</th>
								<th className="col-md-1">Processing Status</th>
								<th className="col-md-1">Link to Kibana</th>
							</tr>
						</thead>
						<tbody>
							{results.length > 0 &&
								results.map((x) => (
									<tr key={x.SDA_PK}>
										<td>{x.SDA_SubSource}</td>
										<td>
											{moment(x.SDA_SourceTime).format("yyyy-MM-DD HH:mm:ss")}
										</td>
										<td>{x.SDA_Source}</td>
										<td>{x.SDA_PK}</td>
										<td>{getActualFileName(x.SDA_Filename)}</td>
										<td>
											{moment(x.SDA_CreatedTime).format("yyyy-MM-DD HH:mm:ss")}
										</td>
										<td className={`bg-${x.SDA_Status.toLowerCase()}`}>
											{getStatusDescription(x.SDA_Status)}
										</td>
										<td>
											{getProcessingStatusDescription(x.SDA_Status, x.SDA_NotProcessedUntil)}
										</td>
										<td>
											<KibanaLink entity={x} />
										</td>
									</tr>
								))}
						</tbody>
					</table>
				</div>
			</div>
		</>
	);
};
