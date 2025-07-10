import React, { useContext, useEffect, useState } from "react";
import { IValidationResults } from "./ValidationResults";
import { IEntityManager, ServiceType } from "./EntityManager";
import { TextFilterModule } from "./TextFilterModule";
import IQrtzJobDetails from "./models/IQrtzJobDetails";
import { ValidationHelper } from "./ValidationHelper";
import { Filter, FilterOps, IFilter } from "./Filter";
import { IValidationService } from "./ValidationService";
import { ValidationServiceHelper } from "./ValidationServiceHelper";
import { FilterStrip } from "./FilterStrip";
import { Link } from "react-router-dom";
import { PopupForm } from "./PopupForm";
import Button from "./Button";
import { FilterContext } from "./FilterContext";

interface IRefApplicationConfigSearchFormProps {
	entityManager: IEntityManager;
}

const RefApplicationConfigSearchForm = ({
	entityManager,
}: IRefApplicationConfigSearchFormProps) => {
	const [results, setResults] = useState<IQrtzJobDetails[]>([]);
	const [jobsWithDistinctCountry, setJobsWithDistinctCountry] = useState<
		IQrtzJobDetails[]
	>([]);
	const { filters, setFilters } = useContext(FilterContext);
	const [validationResults, setValidationResults] = useState<{
		[propertyName: string]: IValidationResults;
	}>({});
	const [message, setMessage] = useState("");

	const validationService: IValidationService =
		ValidationServiceHelper.getRefApplicationFilterValidationService(
			entityManager
		);

	useEffect(() => {
		if (filters.length == 0) {
			setFilters([
				new TextFilterModule(
					"Job Name",
					new Filter("JOB_NAME", FilterOps.Equals, "" as any, "string"),
					150
				),
				new TextFilterModule(
					"Country",
					new Filter("CountryCode", FilterOps.Equals, "" as any, "string"),
					3,
					"CountryCode",
					"CountryCode",
					"QRTZ_JOB_DETAILS"
				),
			]);
		} else if (filters.map((f) => f.filter).filter((f) => f.value).length > 0) {
			(async () => {
				await find();
			})();
		}
		(async () => {
			await loadDistinctQrtzJobs();
		})();
	}, []);

	const loadDistinctQrtzJobs = async () => {
		if (
			jobsWithDistinctCountry == null ||
			jobsWithDistinctCountry.length == 0
		) {
			let data = await entityManager.getAsync<IQrtzJobDetails>(
				"QRTZ_JOB_DETAILS",
				[ServiceType.Staging],
				[],
				false
			);
			if (data) {
				let filterData = data.filter(
					(elem, index, self) =>
						self.findIndex((t) => {
							return t.CountryCode === elem.CountryCode;
						}) === index
				);
				setJobsWithDistinctCountry(filterData);
			}
		}
	};

	const find = async () => {
		if (
			!ValidationHelper.hasErrors(
				Object.getOwnPropertyNames(validationResults).map(
					(p) => validationResults[p]
				)
			)
		) {
			let appliedFilters: IFilter[] = filters
				.map((f) => f.filter)
				.filter((f) => f.value && f.propertyName != "CountryCode");
			let data = await entityManager.getAsync<IQrtzJobDetails>(
				"QRTZ_JOB_DETAILS",
				[ServiceType.Staging],
				appliedFilters,
				false
			);
			if (data.length == 0) {
				setMessage("Search returned 0 result.");
			} else if (results.length > 1000) {
				data = filterByCountry(data);
				data.sort((a, b) => a.JOB_NAME.localeCompare(b.JOB_NAME));
				if (data.length > 1000) {
					data = data.slice(0, 1000);
					setMessage(
						"More than 1000 results found. Only showing the first 1000 results."
					);
				}
			} else {
				setMessage(`${results.length} results`);
			}
			setResults(data);
		} else {
			alert("Please fix all errors before searching.");
		}
	};

	const filterByCountry = (data: IQrtzJobDetails[]): IQrtzJobDetails[] => {
		data = data.filter(
			(x) => x.ProgramExePath != "" && x.ProgramExePath != null
		);
		let filter: IFilter = filters
			.map((f) => f.filter)
			.filter((f) => f.propertyName == "CountryCode")[0];
		if (filter && filter.value != undefined && filter.value.toString() != "") {
			data = data.filter((x) => x.CountryCode != null);
			data = entityManager.filterEntities(data, [filter]);
		}
		return data;
	};

	const onValueChange = async (entity: any, name: string, value: object) => {
		let filter = entity as IFilter;
		if (filter) {
			const updatedFilters = [...filters];
			const index = updatedFilters.findIndex(
				(x) => x.filter.propertyName == filter.propertyName
			);
			updatedFilters[index].filter[name] = value;
			setFilters(updatedFilters);
		}
	};

	const onValueChanged = async (entity: any, name: string) => {
		let filter = entity as IFilter;
		if (filter) {
			let index = filters.findIndex(
				(x) => x.filter.propertyName == filter.propertyName
			);
			if (index >= 0) {
				let filterModule = filters[index];
				validationResults[filterModule.name] = {
					[name]: await ValidationHelper.validateFilterProperty(
						validationService,
						filter,
						name
					),
				};
				setValidationResults({ ...validationResults });
			}
		}
	};

	const getProgramArgs = (args: string): string => {
		if (args != undefined && args != null) {
			if (args.length > 30) {
				args = args.substring(0, 30) + "...";
			}
		}
		return args;
	};

	const getProgramExePath = (path: string): string => {
		if (path != undefined && path != null) {
			path = path.substring(path.lastIndexOf("\\") + 1);
			if (path.startsWith("CargoWise.RefDbRepo")) {
				path = path.substring(20);
			}
		}
		return path;
	};

	return (
		<div>
			{filters.length > 0 ? (
				<>
					<FilterStrip
						entityManager={entityManager}
						key={filters[0].name}
						filterModule={filters[0]}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						serviceType={ServiceType.Staging}
						validationResults={validationResults[filters[0].name]}
					/>
					<FilterStrip
						prevEntity={jobsWithDistinctCountry}
						key={filters[1].name}
						filterModule={filters[1]}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						serviceType={ServiceType.Staging}
						validationResults={validationResults[filters[1].name]}
					/>
				</>
			) : (
				""
			)}
			<div className="form-group row">
				<div className="col-sm-1">
					<Button type="button" className="btn btn-info" onClick={find}>
						Find
					</Button>
				</div>
				<div className="col m-auto">
					<span className="text-info">{message}</span>
				</div>
			</div>
			<table className="table table-striped">
				<thead>
					<tr>
						<th scope="col">Job Name</th>
						<th scope="col">Country</th>
						<th scope="col">Program Args</th>
						<th scope="col">Program</th>
						<th scope="col"></th>
					</tr>
				</thead>
				<tbody>
					{results.map((r) => (
						<tr key={r.JOB_PK}>
							<td>{r.JOB_NAME}</td>
							<td>{r.CountryCode}</td>
							<td>{getProgramArgs(r.ProgramArgs)}</td>
							<td>{getProgramExePath(r.ProgramExePath)}</td>
							<td>
								<Link to={"/RefApplicationConfigDetailsForm/" + r.JOB_PK}>
									Details
								</Link>
							</td>
						</tr>
					))}
				</tbody>
			</table>
		</div>
	);
};

export default RefApplicationConfigSearchForm;
