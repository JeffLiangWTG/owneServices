import React, { useEffect, useState } from "react";
import { IEntityManager, ServiceType } from "./EntityManager";
import { TextFilterModule } from "./TextFilterModule";
import { Filter, FilterOps } from "./Filter";
import { DateTimeInput } from "./DateTimeInput";
import { IClientRefDbVersionControl } from "./models/IClientRefDbVersionControl";
import moment from "moment";
import { parseBool } from "ts-odatajs/lib/odata/odatautils";
import { ClientVersionsLink } from "./ClientVersionsLink";
import axios from "axios";
import { useHistory, useLocation } from "react-router-dom";

interface ClientVersionsProps {
	entityManager: IEntityManager;
}

interface ClientVersionsQueryParams {
	lastUpdatedUTCFrom: string;
	dataSet: string | null;
	clientId: string | null;
	isLate: boolean | null;
	systemType: string | null;
}

interface DataSetTimeStamps {
	[key: string]: string;
}

declare var __SafeAPI__: string;

export const ClientVersions = ({ entityManager }: ClientVersionsProps) => {
	const safeApi = __SafeAPI__.replace("odata", "api");
	const axiosConfig = {
		headers: { "Content-Type": "application/json" },
	};

	const location = useLocation();
	const history = useHistory();
	const urlSearchParams = new URLSearchParams(location.search);

	const [results, setResults] = useState<IClientRefDbVersionControl[]>([]);
	const [dataSetTimestamp, setDataSetTimestamp] = useState<DataSetTimeStamps>();
	const [params] = useState<ClientVersionsQueryParams>({
		clientId: urlSearchParams.get("clientId"),
		dataSet: urlSearchParams.get("dataSet"),
		isLate: parseBool(urlSearchParams.get("isLate")),
		lastUpdatedUTCFrom: urlSearchParams.get("lastUpdatedUTCFrom") ?? "",
		systemType: urlSearchParams.get("systemType"),
	});

	const getDateRangeToFilter = (value: any) => {
		return `${new Date(
			value
		).toISOString()} to ${new Date().toISOString()}` as any;
	};

	const [filters] = useState([
		new TextFilterModule(
			"CVC_LastUpdatedTimeUTC",
			new Filter(
				"CVC_LastUpdatedTimeUTC",
				FilterOps.DateRange,
				getDateRangeToFilter(params.lastUpdatedUTCFrom),
				"datetime"
			),
			100
		),
		new TextFilterModule(
			"CVC_DataSet",
			new Filter(
				"CVC_DataSet",
				FilterOps.Equals,
				params.dataSet as any,
				"string"
			),
			100
		),
		new TextFilterModule(
			"CVC_ClientId",
			new Filter(
				"CVC_ClientId",
				FilterOps.Equals,
				params.clientId as any,
				"string"
			),
			100
		),
		new TextFilterModule(
			"CVC_SystemType",
			new Filter(
				"CVC_SystemType",
				FilterOps.Equals,
				params.systemType as any,
				"string"
			),
			100
		),
	]);

	const doSearch = async () => {
		const appliedFilters = filters.map((x) => x.filter).filter((x) => x.value);
		const clientDsResults =
			await entityManager.getAsync<IClientRefDbVersionControl>(
				"ClientRefDbVersionControl",
				[ServiceType.Safe],
				appliedFilters,
				false
			);
		const dataSetTimeStamps = await axios.get<DataSetTimeStamps>(
			`${safeApi}ClientDataSetVersionSummary/GetDataSetsTimestamp`,
			axiosConfig
		);

		let results = clientDsResults;
		if (params.isLate && params.dataSet && dataSetTimeStamps.data) {
			results = results.filter(
				(x) =>
					!x.CVC_DataSetTimestamp ||
					!(x.CVC_DataSetTimestamp instanceof Date) ||
					x.CVC_DataSetTimestamp?.getDate() <
						new Date(dataSetTimeStamps.data[params.dataSet!]).getDate()
			);
		}

		setResults(results);
		setDataSetTimestamp(dataSetTimeStamps.data);
	};

	const onDateFilterValueChange = (
		entity: any,
		name: string,
		value: object
	) => {
		params.lastUpdatedUTCFrom = value as any;
		urlSearchParams.set(
			"lastUpdatedUTCFrom",
			moment(value).format("YYYY-MM-DDTHH:mm:ss")
		);
		history.push({
			pathname: location.pathname,
			search: urlSearchParams.toString(),
		});
		filters
			.map((x) => x.filter)
			.filter((x) => x.propertyName == "CVC_LastUpdatedTimeUTC")
			.map((x) => (x.value = getDateRangeToFilter(value)));
	};
	const onDateFilterValueChanged = () => {};

	useEffect(() => {
		doSearch();
	}, []);

	return (
		<>
			<div className="row mt-4">
				<div className="col-4">
					<span className="h3">Clients' Dataset parameters: </span>
				</div>
			</div>
			<div className="row mt-2">
				<div className="col-6">
					<DateTimeInput
						label="Last Updated Since"
						propertyName="lastUpdatedUTCFrom"
						entity={params}
						onValueChange={onDateFilterValueChange}
						onValueChanged={onDateFilterValueChanged}
						format="DD-MMM-yyyy HH:mm:ss A"
						useLocalTime={true}
					/>
				</div>
				<div className="col">
					<button type="button" className="btn btn-info" onClick={doSearch}>
						Update
					</button>
				</div>
			</div>
			<div className="row mt-2">
				<div className="col">
					<table className="table">
						<thead>
							<tr>
								<th>Data Set</th>
								<th>Data Set Timestamp</th>
								<th>Client Id</th>
								<th>Client Timestamp</th>
								<th>System Type</th>
							</tr>
						</thead>
						{results.length > 0 && (
							<tbody>
								{results.map((x) => (
									<tr key={x.CVC_PK}>
										<td>
											<ClientVersionsLink
												dataSet={x.CVC_DataSet}
												lastUpdatedUTCFrom={params.lastUpdatedUTCFrom}
											>
												<span>{x.CVC_DataSet}</span>
											</ClientVersionsLink>
										</td>
										<td>
											{dataSetTimestamp && dataSetTimestamp[x.CVC_DataSet]
												? moment(dataSetTimestamp![x.CVC_DataSet!]).format(
														"DD-MMM-yyyy HH:mm:ss"
												  )
												: ""}
										</td>
										<td>
											<ClientVersionsLink
												clientId={x.CVC_ClientId}
												lastUpdatedUTCFrom={params.lastUpdatedUTCFrom}
											>
												{x.CVC_ClientId}
											</ClientVersionsLink>
										</td>
										<td>
											{x.CVC_DataSetTimestamp
												? moment(x.CVC_DataSetTimestamp).format(
														"DD-MMM-yyyy HH:mm:ss"
												  )
												: ""}
										</td>
										<td>
											<ClientVersionsLink
												lastUpdatedUTCFrom={params.lastUpdatedUTCFrom}
												systemType={x.CVC_SystemType}
											>
												{x.CVC_SystemType}
											</ClientVersionsLink>
										</td>
									</tr>
								))}
							</tbody>
						)}
					</table>
				</div>
			</div>
		</>
	);
};
