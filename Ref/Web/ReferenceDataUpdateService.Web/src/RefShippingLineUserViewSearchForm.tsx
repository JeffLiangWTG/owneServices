import React, { useContext, useEffect, useState } from "react";
import { IEntityManager, ServiceType } from "./EntityManager";
import { IValidationService } from "./ValidationService";
import { ValidationServiceHelper } from "./ValidationServiceHelper";
import { RefShippingLineUserView } from "./models/RefShippingLineUserView";
import { IValidationResults } from "./ValidationResults";
import { TextFilterModule } from "./TextFilterModule";
import { ValidationHelper } from "./ValidationHelper";
import { Filter, FilterOps, IFilter } from "./Filter";
import { FilterStrip } from "./FilterStrip";
import { Link } from "react-router-dom";
import { PopupForm } from "./PopupForm";
import Button from "./Button";
import { FilterContext } from "./FilterContext";

interface IRefShippingLineUserViewSearchFormProps {
	entityManager: IEntityManager;
}

const RefShippingLineUserViewSearchForm = ({
	entityManager,
}: IRefShippingLineUserViewSearchFormProps) => {
	const [results, setResults] = useState<RefShippingLineUserView[]>([]);
	const { filters, setFilters } = useContext(FilterContext);
	const [filterValidationResults, setFilterValidationResults] = useState<{
		[propertyName: string]: IValidationResults;
	}>({});
	const [message, setMessage] = useState("");

	const filterValidationService: IValidationService =
		ValidationServiceHelper.getRefShippingLineFilterValidationService(
			entityManager
		);

	useEffect(() => {
		if (filters.length == 0) {
			setFilters([
				new TextFilterModule(
					"Carrier Name",
					new Filter("RSL_CarrierName", FilterOps.Equals, "" as any, "string"),
					75
				),
				new TextFilterModule(
					"SCAC Code",
					new Filter(
						"RSL_StandardCarrierAlphaCode",
						FilterOps.Equals,
						"" as any,
						"string"
					),
					4
				),
				new TextFilterModule(
					"CW1 Code",
					new Filter(
						"RSL_CargoWiseOneCode",
						FilterOps.Equals,
						"" as any,
						"string"
					),
					4
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
			var data = await entityManager.getAsync<RefShippingLineUserView>(
				"RefShippingLineUserView",
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
						to="/RefShippingLineUserViewDetailsForm/"
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
						<th scope="col">Carrier Name</th>
						<th scope="col">SCAC Code</th>
						<th scope="col">CW1 Code</th>
						<th scope="col">Active</th>
						<th scope="col"></th>
					</tr>
				</thead>
				<tbody>
					{results.map((r) => {
						return (
							<tr key={r.RSL_PK}>
								<td>{r.RSL_CarrierName}</td>
								<td>{r.RSL_StandardCarrierAlphaCode}</td>
								<td>{r.RSL_CargoWiseOneCode}</td>
								<td>
									<input
										type="checkbox"
										checked={r.RSL_IsActive}
										readOnly={true}
									/>
								</td>
								<td>
									<Link to={"/RefShippingLineUserViewDetailsForm/" + r.RSL_PK}>
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

export default RefShippingLineUserViewSearchForm;
