import React, { useContext, useEffect, useState } from "react";
import { FilterStrip } from "./FilterStrip";
import { IEntityManager, ServiceType } from "./EntityManager";
import { TextFilterModule } from "./TextFilterModule";
import { Filter, FilterOps, IFilter } from "./Filter";
import { Link } from "react-router-dom";
import { CheckBox } from "./CheckBox";
import { IValidationResults } from "./ValidationResults";
import { ValidationHelper } from "./ValidationHelper";
import { ValidationServiceHelper } from "./ValidationServiceHelper";
import Button from "./Button";
import { FilterContext } from "./FilterContext";
import { RefStlScriptUserView } from "./models/RefStlScriptUserView";

interface IRefStlScriptUserViewSearchFormProps {
	entityManager: IEntityManager;
}

const RefStlScriptUserViewSearchForm = ({
	entityManager,
}: IRefStlScriptUserViewSearchFormProps) => {
	const { filters, setFilters } = useContext(FilterContext);
	const [results, setResults] = useState<RefStlScriptUserView[]>([]);
	const [validationResults, setValidationResults] = useState<{
		[propertyName: string]: IValidationResults;
	}>({});
	const [message, setMessage] = useState("");

	useEffect(() => {
		if (filters.length == 0) {
			setFilters([
				new TextFilterModule(
					"Feature Code",
					new Filter("STL_FeatureCode", FilterOps.Equals, "" as any, "string"),
					RefStlScriptUserView.STL_FeatureCode_MaxLength
				),
				new TextFilterModule(
					"Role Name",
					new Filter("STL_RoleName", FilterOps.Contains, "" as any, "string"),
					RefStlScriptUserView.STL_RoleName_MaxLength
				)
			]);
		} else if (filters.map((f) => f.filter).filter((f) => f.value).length > 0) {
			(async () => {
				await find();
			})();
		}
	}, []);

	const validationService =
		ValidationServiceHelper.getRefStlScriptUserViewValidationService(
			entityManager
		);

	const onValueChange = (entity: any, name: string, value: object) => {
		let filter = entity as IFilter;
		if (filter) {
			let index = filters.findIndex(
				(x) => x.filter.propertyName == filter.propertyName
			);
			filters[index].filter[name] = value;
			setFilters([...filters]);
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

	const find = async () => {
		if (
			!ValidationHelper.hasErrors(
				Object.getOwnPropertyNames(validationResults).map(
					(p) => validationResults[p]
				)
			)
		) {
			var data = await entityManager.getAsync<RefStlScriptUserView>(
				"RefStlScriptUserView",
				[ServiceType.Safe],
				filters.map((f) => f.filter).filter((f) => f.value),
				false
			);
			if (data.length == 0) {
				setMessage("Search returned 0 result.");
			} else if (data.length > 1000) {
				data = data.slice(0, 1000);
				setMessage(
					"More than 1000 results found. Only showing the first 1000 results."
				);
			} else {
				setMessage(`${data.length} results`);
			}
			setResults(data);
		} else {
			alert("Please fix all errors before searching.");
		}
	};

	return (
		<div>
			{filters.length > 0 &&
				filters.map((f) => (
					<FilterStrip
						key={f.name}
						filterModule={f}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						entityManager={entityManager}
						validationResults={validationResults[f.name]}
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
						to="/RefStlScriptUserViewDetailsForm/"
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
			<table className="table table-striped" id="result-table">
				<thead>
					<tr>
						<th scope="col">Feature Code</th>
						<th scope="col">Role Name</th>
						<th scope="col">Min CW Version</th>
						<th scope="col">Max CW Version</th>
						<th scope="col">Active On</th>
						<th scope="col">Published</th>
						<th scope="col"></th>
					</tr>
				</thead>
				<tbody>
					{results.map((r) => (
						<tr key={r.STL_PK}>
							<td>{r.STL_FeatureCode}</td>
							<td>{r.STL_RoleName}</td>
							<td>{r.STL_MinCW1Version}</td>
							<td>{r.STL_MaxCW1Version}</td>
							<td>{r.STL_ActiveOn}</td>
							<td>
								<CheckBox propertyName="STL_IsPublished" entity={r} />
							</td>
							<td>
								<Link to={"/RefStlScriptUserViewDetailsForm/" + r.STL_PK}>
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

export default RefStlScriptUserViewSearchForm;
