import React, { createContext, useContext, useEffect, useState } from "react";
import { FilterStrip } from "./FilterStrip";
import { IEntityManager, ServiceType } from "./EntityManager";
import { TextFilterModule } from "./TextFilterModule";
import { Filter, FilterOps, IFilter } from "./Filter";
import { RefCusCodeListUserView } from "./models/RefCusCodeListUserView";
import { Link } from "react-router-dom";
import { CheckBox } from "./CheckBox";
import { PopupForm } from "./PopupForm";
import { IValidationResults } from "./ValidationResults";
import { ValidationHelper } from "./ValidationHelper";
import { ValidationServiceHelper } from "./ValidationServiceHelper";
import Button from "./Button";
import { FilterContext } from "./FilterContext";

interface IRefCusCodeListUserViewSearchFormProps {
	entityManager: IEntityManager;
}

const RefCusCodeListUserViewSearchForm = ({
	entityManager,
}: IRefCusCodeListUserViewSearchFormProps) => {
	const { filters, setFilters } = useContext(FilterContext);
	const [results, setResults] = useState<RefCusCodeListUserView[]>([]);
	const [validationResults, setValidationResults] = useState<{
		[propertyName: string]: IValidationResults;
	}>({});
	const [message, setMessage] = useState("");

	useEffect(() => {
		if (filters.length == 0) {
			setFilters([
				new TextFilterModule(
					"Code",
					new Filter("ZZD_Code", FilterOps.Equals, "" as any, "string"),
					RefCusCodeListUserView.ZZD_Code_MaxLength
				),
				new TextFilterModule(
					"Description",
					new Filter("ZZD_Description", FilterOps.Equals, "" as any, "string"),
					RefCusCodeListUserView.ZZD_Description_MaxLength
				),
				new TextFilterModule(
					"List Type",
					new Filter("ZZD_CodeType", FilterOps.Equals, "" as any, "string"),
					RefCusCodeListUserView.ZZD_CodeType_MaxLength,
					"ZZK_CodeType",
					"ZZK_Description",
					"RefCusCodeType"
				),
				new TextFilterModule(
					"Country or Grouping",
					new Filter(
						"ZZD_CountryOrGrouping",
						FilterOps.Equals,
						"" as any,
						"string"
					),
					RefCusCodeListUserView.ZZD_CountryOrGrouping_MaxLength,
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

	const validationService =
		ValidationServiceHelper.getRefCusCodeListFilterValidationService(
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
			var data = await entityManager.getAsync<RefCusCodeListUserView>(
				"RefCusCodeListUserView",
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
						to="/RefCusCodeListUserViewDetailsForm/"
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
						<th scope="col">List Type</th>
						<th scope="col">Code</th>
						<th scope="col">Description</th>
						<th scope="col">Country</th>
						<th scope="col">System</th>
						<th scope="col"></th>
					</tr>
				</thead>
				<tbody>
					{results.map((r) => (
						<tr key={r.ZZD_PK}>
							<td>{r.ZZD_CodeType}</td>
							<td>{r.ZZD_Code}</td>
							<td>{r.ZZD_Description}</td>
							<td>{r.ZZD_CountryOrGrouping}</td>
							<td>
								<CheckBox propertyName="ZZD_IsSystem" entity={r} />
							</td>
							<td>
								<Link to={"/RefCusCodeListUserViewDetailsForm/" + r.ZZD_PK}>
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

export default RefCusCodeListUserViewSearchForm;
