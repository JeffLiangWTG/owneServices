import React, { useEffect, useState } from "react";
import { DateTimeInput } from "./DateTimeInput";
import { IEntity } from "./models/IEntity";
import moment from "moment";
import axios from "axios";
import {
	ClientDataSetVersionSummary,
	IClientDataSetVersionSummary,
} from "./models/IClientDataSetVersionSummary";
import uuid from "uuid";
import { ClientVersionsLink } from "./ClientVersionsLink";

interface IDateFilterClientsDataset extends IEntity {
	dateToFilter: string;
}

declare var __SafeAPI__: string;

const DEFAULT_DATE_TO_FILTER = moment().add(-1, "day").format("YYYY-MM-DDTHH:mm:ss");

export const ClientsDataset = () => {
	const safeApi = __SafeAPI__.replace("odata", "api");
	const [recordsMessage, setRecordsMessage] = useState("");
	const axiosConfig = {
		headers: { "Content-Type": "application/json" },
	};

	const [dateObj, setDateObj] = useState<IDateFilterClientsDataset>({
		dateToFilter: DEFAULT_DATE_TO_FILTER,
	});
	const [results, setResults] = useState<IClientDataSetVersionSummary[]>([]);
	const [isLoading, setIsLoading] = useState(false);

	const getClientDataSetVersionSummary = async () => {
		setIsLoading(true);
		let results = await axios.get(
			`${safeApi}ClientDataSetVersionSummary/Get?lastUpdatedUTCFrom=${moment(
				dateObj.dateToFilter
			).toISOString()}`, //transform in UTC to search
			axiosConfig
		);
		let summaryWithPk = (results.data as IClientDataSetVersionSummary[])
			.map((x) => {
				return new ClientDataSetVersionSummary(
					uuid(),
					x.DataSet,
					x.LastDataChangedTime,
					x.NoOfCustomersUpdated,
					x.NoOfCustomersFailed,
					x.NoOfProductionCustomersFailed
				);
			})
			.sort((x, y) => {
				if (y.DataSet < x.DataSet) return 1;
				if (y.DataSet > x.DataSet) return -1;
				return 0;
			});

		if (summaryWithPk.length == 0) {
			setRecordsMessage("Search returned 0 result.");
		} else {
			setRecordsMessage(`${summaryWithPk.length} results`);
		}
		setResults(summaryWithPk);
		setIsLoading(false);
	};

	const onDateFilterValueChange = async (
		entity: any,
		name: string,
		value: object
	) => {
		if (value != null) {
			setDateObj({ dateToFilter: value as any });
		} else {
			setDateObj({ dateToFilter: DEFAULT_DATE_TO_FILTER });
		}
	};

	const onDateFilterValueChanged = async (entity: any, name: string) => {};

	useEffect(() => {
		getClientDataSetVersionSummary();
	}, []);

	return (
		<>
			<div className="row mt-4">
				<div className="col-4">
					<span className="h2">Clients' Data Set</span>
				</div>
			</div>
			<div className="row mt-5">
				<div className="col-6">
					<DateTimeInput
						label="Last Updated Since"
						propertyName="dateToFilter"
						entity={dateObj}
						onValueChange={onDateFilterValueChange}
						onValueChanged={onDateFilterValueChanged}
						format="DD-MMM-yyyy HH:mm:ss A"
						useLocalTime={true}
					/>
				</div>
				<div className="col">
					<button
						type="button"
						className="btn btn-info"
						onClick={getClientDataSetVersionSummary}
						disabled={isLoading}
					>
						{isLoading ? "Loading..." : "Search"}
					</button>
				</div>
			</div>
			<div className="row mt-2">
				<div className="col"></div>
				<div className="col m-auto">
					<span className="text-info">{recordsMessage}</span>
				</div>
			</div>
			<div className="row mt-2">
				<div className="col">
					<table className="table">
						<thead>
							<tr>
								<th>Data Set</th>
								<th>Last Data Changed Time</th>
								<th>No. Of Customers Updated</th>
								<th>No. Of Customers Failed (Production)</th>
							</tr>
						</thead>
						{results.length > 0 && (
							<tbody>
								{results.map((x) => (
									<tr key={x.PK}>
										<td>
											<ClientVersionsLink
												dataSet={x.DataSet}
												lastUpdatedUTCFrom={dateObj.dateToFilter}
											>
												<span>{x.DataSet}</span>
											</ClientVersionsLink>
										</td>
										<td>
											{moment(x.LastDataChangedTime).format(
												"DD-MMM-yyyy HH:mm:ss"
											)}
										</td>
										<td>{x.NoOfCustomersUpdated}</td>
										<td>
											<ClientVersionsLink
												dataSet={x.DataSet}
												lastUpdatedUTCFrom={dateObj.dateToFilter}
											>
												<span>{x.NoOfCustomersFailed}</span>
											</ClientVersionsLink>
											<ClientVersionsLink
												dataSet={x.DataSet}
												lastUpdatedUTCFrom={dateObj.dateToFilter}
												systemType="PRD"
											>
												<span>({x.NoOfProductionCustomersFailed})</span>
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
