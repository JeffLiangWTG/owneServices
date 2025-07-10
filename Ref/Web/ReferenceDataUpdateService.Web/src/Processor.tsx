import React, { useEffect, useState } from "react";
import { IEntityManager, ServiceType } from "./EntityManager";
import { TextFilterModule } from "./TextFilterModule";
import { Filter, FilterOps, IFilter } from "./Filter";
import IProcessorStatus from "./models/IProcessorStatus";
import { FilterStrip } from "./FilterStrip";
import moment from "moment";
import { KibanaLink } from "./KibanaLink";

interface ProcessorProps {
	entityManager: IEntityManager;
}

export const Processor = ({ entityManager }: ProcessorProps) => {
	const [recordsMessage, setRecordsMessage] = useState("");
	const [filters, setFilters] = useState([
		new TextFilterModule(
			"Quartz Group",
			new Filter("PRC_JobGroup", FilterOps.Equals, "" as any, "string"),
			100
		),
		new TextFilterModule(
			"Name",
			new Filter("PRC_JobName", FilterOps.Equals, "" as any, "string"),
			100
		),
		new TextFilterModule(
			"Last Run Status",
			new Filter("PRC_Status", FilterOps.Equals, "" as any, "string"),
			100,
			"Status_Value",
			"Status_Description",
			[
				{ Status_Value: "ERR", Status_Description: "Error" },
				{ Status_Value: "PRS", Status_Description: "Success" },
			]
		),
	]);

	const [results, setResults] = useState<IProcessorStatus[]>([]);

	useEffect(() => {
		getData();
	}, []);

	const getData = async () => {
		const appliedFilters = filters.map((x) => x.filter).filter((x) => x.value);
		let results = (
			await entityManager.getAsync<IProcessorStatus>(
				"ProcessorStatus",
				[ServiceType.Staging],
				appliedFilters,
				false
			)
		).sort((a, b) => (a.PRC_JobGroup < b.PRC_JobGroup ? -1 : 0));

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

	const getStatusDescription = (status: string) => {
		switch (status) {
			case "PRS":
				return "Success";
			case "ERR":
				return "Error";
		}
	};

	const onFilterValueChanged = async (entity: any, name: string) => {};

	return (
		<>
			<div className="row mt-4">
				<div className="col">
					<span className="h1">Processor</span>
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
							<button type="button" className="btn btn-info" onClick={getData}>
								Search
							</button>
						</div>
						<div className="col"></div>
						<div className="col m-auto">
							<span className="text-info">{recordsMessage}</span>
						</div>
					</div>
				</div>
			</div>
			<div className="row">
				<div className="col">
					<table className="table">
						<thead>
							<tr>
								<th>Quartz Group</th>
								<th>Name</th>
								<th>Last Run Time(Local)</th>
								<th>Last Run Status</th>
								<th>Last Successful Run Time(Local)</th>
								<th>Link to Kibana</th>
							</tr>
						</thead>
						<tbody>
							{results.length > 0 &&
								results.map((x) => (
									<tr key={x.PRC_PK}>
										<td>{x.PRC_JobGroup}</td>
										<td>{x.PRC_JobName}</td>
										<td>
											{moment(x.PRC_LastRunTime).format("yyyy-MM-DD HH:mm:ss")}
										</td>
										<td className={`bg-${x.PRC_Status.toLowerCase()}`}>
											{getStatusDescription(x.PRC_Status)}
										</td>
										<td>
											{x.PRC_LastSuccessRunTime &&
												moment(x.PRC_LastSuccessRunTime).format(
													"yyyy-MM-DD HH:mm:ss"
												)}
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
