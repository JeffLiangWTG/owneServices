import React, { useContext, useEffect, useState } from "react";
import { IEntityManager, ServiceType } from "./EntityManager";
import { ValidationServiceHelper } from "./ValidationServiceHelper";
import { IValidationService } from "./ValidationService";
import { TextFilterModule } from "./TextFilterModule";
import { Filter, FilterOps, IFilter } from "./Filter";
import { ValidationHelper } from "./ValidationHelper";
import { IValidationResults } from "./ValidationResults";
import { RefCusProcedureUserView } from "./models/RefCusProcedureUserView";
import { FilterStrip } from "./FilterStrip";
import { Link } from "react-router-dom";
import { PopupForm } from "./PopupForm";
import Button from "./Button";
import { FilterContext } from "./FilterContext";

interface IRefCusProcedureUserViewSearchFormProps {
	entityManager: IEntityManager;
}

const RefCusProcedureUserViewSearchForm = ({
	entityManager,
}: IRefCusProcedureUserViewSearchFormProps) => {
	const [results, setResults] = useState<RefCusProcedureUserView[]>([]);
	const { filters, setFilters } = useContext(FilterContext);
	const [filterValidationResults, setFilterValidationResults] = useState<{
		[propertyName: string]: IValidationResults;
	}>({});
	const [message, setMessage] = useState("");

	const filterValidationService: IValidationService =
		ValidationServiceHelper.getRefCusProcedureFilterValidationService(
			entityManager
		);

	useEffect(() => {
		if (filters.length == 0) {
			setFilters([
				new TextFilterModule(
					"Procedure Code",
					new Filter(
						"ZZ6_ProcedureCode",
						FilterOps.Equals,
						"" as any,
						"string"
					),
					5
				),
				new TextFilterModule(
					"Previous Procedure Code",
					new Filter(
						"ZZ6_PreviousProcedureCode",
						FilterOps.Equals,
						"" as any,
						"string"
					),
					5
				),
				new TextFilterModule(
					"Concession",
					new Filter("ZZ6_Concession", FilterOps.Equals, "" as any, "string"),
					50
				),
				new TextFilterModule(
					"Shipment Type",
					new Filter("ZZ6_ShipmentType", FilterOps.Equals, "" as any, "string"),
					50
				),
				new TextFilterModule(
					"Country or Grouping",
					new Filter(
						"ZZ6_CountryOrGrouping",
						FilterOps.Equals,
						"" as any,
						"string"
					),
					3,
					"ZZZ_DataGrouping",
					"ZZZ_Description",
					"RefDataGrouping"
				),
			]);
		} else if (filters.map((f) => f.filter).filter((f) => f.value).length > 0) {
			(async () => {
				await find();
			})();
		}
	}, []);

	const find = async () => {
		if (
			!ValidationHelper.hasErrors(
				Object.getOwnPropertyNames(filterValidationResults).map(
					(p) => filterValidationResults[p]
				)
			)
		) {
			var data = await entityManager.getAsync<RefCusProcedureUserView>(
				"RefCusProcedureUserView",
				[ServiceType.Safe],
				filters.map((f) => f.filter).filter((f) => f.value),
				false
			);
			if (data.length == 0) {
				setMessage("Search return 0 results");
			} else {
				setMessage(`${data.length} results`);
			}
			setResults(data);
		} else {
			setMessage("Please fix all errors before searching.");
		}
	};

	const onFilterValueChange = async (
		entity: any,
		name: string,
		value: object
	) => {
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

	const onFilterValueChanged = async (entity: any, name: string) => {
		let filter = entity as IFilter;
		if (filter) {
			let index = filters.findIndex(
				(x) => x.filter.propertyName == filter.propertyName
			);
			if (index >= 0) {
				let filterModule = filters[index];
				filterValidationResults[filterModule.name] = {
					[name]: await ValidationHelper.validateFilterProperty(
						filterValidationService,
						filter,
						name
					),
				};
				setFilterValidationResults({ ...filterValidationResults });
			}
		}
	};

	return (
		<div>
			{filters.map((r) => (
				<FilterStrip
					key={r.name}
					filterModule={r}
					onValueChange={onFilterValueChange}
					onValueChanged={onFilterValueChanged}
					entityManager={entityManager}
					validationResults={filterValidationResults[r.name]}
				/>
			))}
			<div className="form-group row">
				<div className="col-sm-1">
					<Button type="button" className="btn btn-info" onClick={find}>
						Find
					</Button>
				</div>
				<div className="col-sm-1">
					<Link
						to="/RefCusProcedureUserViewDetailsForm/"
						className="btn btn-info"
						role="button"
					>
						New
					</Link>
				</div>
				<div className="col m-auto">
					<span className="text-info">{message}</span>
				</div>
			</div>
			<table className="table table-striped">
				<thead>
					<tr>
						<th scope="col">Procedure Code</th>
						<th scope="col">Previous Procedure Code</th>
						<th scope="col">Concession</th>
						<th scope="col">Description</th>
						<th scope="col">Country or Grouping</th>
						<th scope="col">Shipment Type</th>
						<th scope="col"></th>
					</tr>
				</thead>
				<tbody>
					{results.map((r) => {
						return (
							<tr key={r.ZZ6_PK}>
								<td>{r.ZZ6_ProcedureCode}</td>
								<td>{r.ZZ6_PreviousProcedureCode}</td>
								<td>{r.ZZ6_Concession}</td>
								<td>{r.ZZ6_Description}</td>
								<td>{r.ZZ6_CountryOrGrouping}</td>
								<td>{r.ZZ6_ShipmentType}</td>
								<td>
									<Link to={"/RefCusProcedureUserViewDetailsForm/" + r.ZZ6_PK}>
										Details
									</Link>
								</td>
							</tr>
						);
					})}
				</tbody>
			</table>
		</div>
	);
};

export default RefCusProcedureUserViewSearchForm;
