import React, { useEffect, useState } from "react";
import { IValidationResults } from "./ValidationResults";
import { RefAccTaxRateUserView } from "./models/RefAccTaxRateUserView";
import { TextFilterModule } from "./TextFilterModule";
import { IEntityManager, ServiceType } from "./EntityManager";
import { Filter, FilterOps, IFilter } from "./Filter";
import { ValidationServiceHelper } from "./ValidationServiceHelper";
import {
	IValidationService,
	ValidationServiceWrapper,
} from "./ValidationService";
import { ValidationHelper } from "./ValidationHelper";
import uuid from "uuid";
import { FilterStrip } from "./FilterStrip";
import { CheckBox } from "./CheckBox";
import { DateTimeInput } from "./DateTimeInput";
import { TextInput } from "./TextInput";
import { CodeInput } from "./CodeInput";
import { PopupForm } from "./PopupForm";
import Button from "./Button";

interface IRefAccTaxRateUserViewSearchFormProps {
	entityManager: IEntityManager;
}

const RefAccTaxRateUserViewSearchForm = ({
	entityManager,
}: IRefAccTaxRateUserViewSearchFormProps) => {
	const [results, setResults] = useState<RefAccTaxRateUserView[]>([]);
	const [editingPks, setEditingPks] = useState<string[]>([]);
	const [addingPks, setAddingPks] = useState<string[]>([]);
	const [gridTaxRateReferenceTypes, setGridTaxRateReferenceTypes] = useState<
		RefAccTaxRateUserView[]
	>([]);
	const [filterTaxRateReferenceTypes, setFilterTaxRateReferenceTypes] =
		useState<RefAccTaxRateUserView[]>([]);
	const [filters, setFilters] = useState<TextFilterModule[]>([]);
	const [filterValidationResults, setFilterValidationResults] = useState<{
		[propertyName: string]: IValidationResults;
	}>({});
	const [gridValidationResults, setGridValidationResults] = useState<{
		[propertyName: string]: IValidationResults;
	}>({});
	const [saveMessage, setSaveMessage] = useState("");
	const [searchMessage, setSearchMessage] = useState("");
	const [saveButtonDisabled, setSaveButtonDisabled] = useState(false);

	useEffect(() => {
		if (filters.length == 0) {
			setFilters([
				new TextFilterModule(
					"Country",
					new Filter("ZAT_RN_NKCountry", FilterOps.Equals, "" as any, "string"),
					RefAccTaxRateUserView.ZAT_RN_NKCountry_MaxLength,
					"RN_Code",
					"RN_Desc",
					"RefCountry"
				),
				new TextFilterModule(
					"Reference Type",
					new Filter(
						"ZAT_ReferenceRateType",
						FilterOps.Equals,
						"" as any,
						"string"
					),
					RefAccTaxRateUserView.ZAT_ReferenceRateType_MaxLength,
					"ZAT_ReferenceRateType",
					"ZAT_RN_NKCountry",
					"RefAccTaxRateUserView"
				),
			]);
		}
	}, []);

	const updateSaveButtonDisabledProperty = (isValidating: boolean) => {
		setSaveButtonDisabled(isValidating);
	};

	const filterValidationService: IValidationService =
		ValidationServiceHelper.getRefAccTaxRateFilterValidationService(
			entityManager
		);
	const gridValidationService: ValidationServiceWrapper =
		new ValidationServiceWrapper(
			[
				ValidationServiceHelper.getRefAccTaxRateValidationService(
					entityManager
				),
			],
			updateSaveButtonDisabledProperty
		);

	const find = async () => {
		if (
			!ValidationHelper.hasErrors(
				Object.getOwnPropertyNames(filterValidationResults).map(
					(p) => filterValidationResults[p]
				)
			)
		) {
			var data = await entityManager.getAsync<RefAccTaxRateUserView>(
				"RefAccTaxRateUserView",
				[ServiceType.Safe],
				filters.map((f) => f.filter).filter((f) => f.value),
				false
			);
			setResults(data);
			if (results.length == 0) {
				setSearchMessage("Search returned 0 result.");
			} else {
				setSearchMessage(`${results.length} results`);
			}
		} else {
			setSearchMessage("Please fix all errors before searching.");
		}
	};

	const onEditButtonClick = (pk: string) => {
		setEditingPks([...editingPks, pk]);
	};

	const onAddingButtonClick = () => {
		let rate = new RefAccTaxRateUserView();
		rate.ZAT_PK = uuid.v1();
		rate.ZAT_RN_NKCountry = "";
		rate.ZAT_ReferenceRateType = "";
		rate.ZAT_RateDenominator = 1;
		rate.ZAT_RateNumerator = 0;
		rate.ZAT_StartDate = "1900-01-01";
		rate.ZAT_EndDate = "9999-12-31";
		rate.ZAT_IsSystem = true;
		rate.ZAT_IsPublished = true;
		rate.ZAT_IsEditable = true;

		entityManager.add(rate, "RefAccTaxRateUserView");
		setResults([rate, ...results]);
		setEditingPks([rate.ZAT_PK, ...editingPks]);
		setAddingPks([rate.ZAT_PK, ...addingPks]);
		setGridTaxRateReferenceTypes([]);
	};

	const onGridCancellingButtonClick = async (id: string) => {
		let index = results.findIndex((x) => x.ZAT_PK == id);
		let indexForPKarray = editingPks.findIndex((x) => x == id);
		if (index >= 0 && indexForPKarray >= 0) {
			if (entityManager.isInDatabase(id)) {
				let reloadEntity = await entityManager.getAsync<RefAccTaxRateUserView>(
					"RefAccTaxRateUserView",
					[ServiceType.Safe],
					[new Filter("ZAT_PK", FilterOps.Equals, id as any, "guid")],
					true
				);
				if (reloadEntity.length > 0) {
					const resultsCopy = [...results];
					resultsCopy[index] = reloadEntity[0];
					setResults(resultsCopy);
				}
			} else {
				entityManager.remove(results[index]);
				delete gridValidationResults[id];
				setResults(results.filter((x) => x.ZAT_PK != id));
			}
			setEditingPks(editingPks.filter((x) => x != id));
			setAddingPks(addingPks.filter((x) => x != id));
		}
	};

	const onGridReferenceTypes_Focused = async (entity: any) => {
		const code = entity as RefAccTaxRateUserView;
		if (code) {
			setGridTaxRateReferenceTypes(
				await getReferenceTypes(code.ZAT_RN_NKCountry)
			);
		}
	};

	const getReferenceTypes = async (country: string) => {
		if (country == "") {
			return [];
		} else {
			let filter = new Filter(
				"ZAT_RN_NKCountry",
				FilterOps.Equals,
				country as any,
				"string"
			);
			let taxRateReferenceTypes =
				await entityManager.getAsync<RefAccTaxRateUserView>(
					"RefAccTaxRateUserView",
					[ServiceType.Safe],
					[filter],
					false
				);
			return distinctEntities(taxRateReferenceTypes);
		}
	};

	const onGridValueChange = async (
		entity: any,
		name: string,
		value: object
	) => {
		let code = entity as RefAccTaxRateUserView;
		if (code) {
			let index = results.findIndex((x) => x.ZAT_PK == code.ZAT_PK);
			if (index >= 0) {
				const updatedResults = [...results];
				updatedResults[index] = { ...updatedResults[index], [name]: value };
				setResults(updatedResults);
				entityManager.update(updatedResults[index]);
			}
		}
	};

	const onGridValueChanged = async (entity: any, name: string) => {
		let code = entity as RefAccTaxRateUserView;
		if (code) {
			let index = results.findIndex((x) => x.ZAT_PK == code.ZAT_PK);
			if (index >= 0) {
				gridValidationResults[code.ZAT_PK] = {
					[name]: await ValidationHelper.validateProperty(
						gridValidationService,
						code,
						name
					),
				};
				setGridValidationResults({ ...gridValidationResults });
			}
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

			if (
				updatedFilters[index].filter.value != null &&
				updatedFilters[index].name == "Country"
			) {
				let country = updatedFilters[index].filter.value as any;
				let filter = new Filter(
					"ZAT_RN_NKCountry",
					FilterOps.Equals,
					country,
					"string"
				);
				let taxRateReferenceTypes =
					await entityManager.getAsync<RefAccTaxRateUserView>(
						"RefAccTaxRateUserView",
						[ServiceType.Safe],
						[filter],
						false
					);
				setFilterTaxRateReferenceTypes(distinctEntities(taxRateReferenceTypes));
			}
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

	const save = async () => {
		let gridValidationResults: { [pk: string]: IValidationResults } = {};
		await Promise.all(
			results.map((a) =>
				ValidationHelper.validate(gridValidationService, a, 0, true).then(
					(r) => (gridValidationResults[a.ZAT_PK] = r)
				)
			)
		);
		setGridValidationResults(gridValidationResults);
		if (
			!ValidationHelper.hasErrors(
				Object.getOwnPropertyNames(gridValidationResults).map(
					(p) => gridValidationResults[p]
				)
			)
		) {
			let result = await entityManager.saveChanges(ServiceType.Safe);
			setSaveMessage(result.message);
			if (result.success) {
				setEditingPks([]);
				setAddingPks([]);
			}
		} else {
			setSaveMessage("Please fix all errors before saving.");
		}
	};

	const distinctEntities = (
		results: RefAccTaxRateUserView[]
	): RefAccTaxRateUserView[] => {
		return results.filter(
			(elem, index, self) =>
				self.findIndex((t) => {
					return t.ZAT_ReferenceRateType === elem.ZAT_ReferenceRateType;
				}) === index
		);
	};

	const onCancelAllButtonClick = async () => {
		editingPks
			.filter((p) => !entityManager.isInDatabase(p))
			.forEach((p) => {
				let index = results.findIndex((x) => x.ZAT_PK === p);
				if (index >= 0) {
					entityManager.remove(results[index]);
					results.splice(index, 1);
				}
			});
		let filter = new Filter(
			"ZAT_PK",
			FilterOps.In,
			editingPks.filter((pk) => entityManager.isInDatabase(pk)),
			"guid"
		);
		let reloads = await entityManager.getAsync<RefAccTaxRateUserView>(
			"RefAccTaxRateUserView",
			[ServiceType.Safe],
			[filter],
			true
		);
		setResults(
			results.map((r) => {
				let idx = reloads.findIndex((l) => l.ZAT_PK === r.ZAT_PK);
				return idx >= 0 ? reloads[idx] : r;
			})
		);
		setGridValidationResults({});
		setEditingPks([]);
		setAddingPks([]);
	};

	return (
		<div>
			{filters.length > 0 ? (
				<>
					<FilterStrip
						entityManager={entityManager}
						key={filters[0].name}
						filterModule={filters[0]}
						onValueChange={onFilterValueChange}
						onValueChanged={onFilterValueChanged}
						validationResults={filterValidationResults[filters[0].name]}
					/>
					<FilterStrip
						prevEntity={filterTaxRateReferenceTypes}
						key={filters[1].name}
						filterModule={filters[1]}
						onValueChange={onFilterValueChange}
						onValueChanged={onFilterValueChanged}
						validationResults={filterValidationResults[filters[1].name]}
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
				<div className="col-sm-1">
					<Button
						type="button"
						className="btn btn-info"
						onClick={onAddingButtonClick}
					>
						Add
					</Button>
				</div>
				<div className="col-sm-1">
					<Button
						type="button"
						className="btn btn-info"
						disabled={saveButtonDisabled}
						onClick={save}
					>
						Save
					</Button>
				</div>
				{editingPks.length !== 0 ? (
					<div className="col-sm-1">
						<Button
							type="button"
							className="btn btn-info"
							onClick={onCancelAllButtonClick}
						>
							Cancel All
						</Button>
					</div>
				) : (
					""
				)}
				<div className="col m-auto">
					<span className="text-info">{searchMessage}</span>
				</div>
			</div>
			<table className="table table-striped">
				<thead>
					<tr>
						<th scope="col">Country</th>
						<th scope="col">Reference Type</th>
						<th scope="col">Rate Numerator</th>
						<th scope="col">Rate Denominator</th>
						<th scope="col">Start Date</th>
						<th scope="col">End Date</th>
						<th scope="col">System</th>
						<th scope="col">Publish</th>
						<th scope="col">Edit/Cancel</th>
					</tr>
				</thead>
				<tbody>
					{results.map((r) => {
						if (editingPks.includes(r.ZAT_PK)) {
							return (
								<tr key={r.ZAT_PK}>
									<td>
										{
											<CodeInput
												propertyName="ZAT_RN_NKCountry"
												entity={r}
												onValueChange={onGridValueChange}
												onValueChanged={onGridValueChanged}
												listCodePropertyName="RN_Code"
												listDescriptionPropertyName="RN_Desc"
												listEntityTypeName="RefCountry"
												entityManager={entityManager}
												readOnly={!addingPks.includes(r.ZAT_PK)}
												validationResults={gridValidationResults[r.ZAT_PK]}
											/>
										}
									</td>
									<td className="col-sm-2">
										{
											<CodeInput
												propertyName="ZAT_ReferenceRateType"
												entity={r}
												onValueChange={onGridValueChange}
												onValueChanged={onGridValueChanged}
												listCodePropertyName="ZAT_ReferenceRateType"
												listDescriptionPropertyName="*"
												listEntityTypeName="RefAccTaxRateUserView"
												prevEntity={gridTaxRateReferenceTypes}
												onFocusing={onGridReferenceTypes_Focused}
												readOnly={!addingPks.includes(r.ZAT_PK)}
												validationResults={gridValidationResults[r.ZAT_PK]}
											/>
										}
									</td>
									<td>
										<TextInput
											inputType="number"
											maxLength={2}
											propertyName="ZAT_RateNumerator"
											entity={r}
											onValueChange={onGridValueChange}
											onValueChanged={onGridValueChanged}
											validationResults={gridValidationResults[r.ZAT_PK]}
										/>
									</td>
									<td>
										<TextInput
											inputType="number"
											maxLength={2}
											propertyName="ZAT_RateDenominator"
											entity={r}
											onValueChange={onGridValueChange}
											onValueChanged={onGridValueChanged}
											validationResults={gridValidationResults[r.ZAT_PK]}
										/>
									</td>
									<td className="col-sm-2">
										<DateTimeInput
											entity={r}
											propertyName="ZAT_StartDate"
											onValueChange={onGridValueChange}
											onValueChanged={onGridValueChanged}
											readOnly={!addingPks.includes(r.ZAT_PK)}
											validationResult={gridValidationResults[r.ZAT_PK]}
											format="DD/MM/YYYY"
											dateOnly={true}
										/>
									</td>
									<td className="col-sm-2">
										<DateTimeInput
											entity={r}
											propertyName="ZAT_EndDate"
											onValueChange={onGridValueChange}
											onValueChanged={onGridValueChanged}
											validationResult={gridValidationResults[r.ZAT_PK]}
											format="DD/MM/YYYY"
											dateOnly={true}
										/>
									</td>
									<td>
										<CheckBox
											propertyName="ZAT_IsSystem"
											entity={r}
											onValueChange={onGridValueChange}
											isReadOnly={true}
										/>
									</td>
									<td>
										<CheckBox
											propertyName="ZAT_IsPublished"
											entity={r}
											onValueChange={onGridValueChange}
										/>
									</td>
									<td>
										<Button
											type="button"
											className="btn btn-info btn-sm"
											onClick={() => onGridCancellingButtonClick(r.ZAT_PK)}
										>
											Cancel
										</Button>
									</td>
								</tr>
							);
						} else {
							return (
								<tr key={r.ZAT_PK}>
									<td>{r.ZAT_RN_NKCountry}</td>
									<td>{r.ZAT_ReferenceRateType}</td>
									<td>{r.ZAT_RateNumerator}</td>
									<td>{r.ZAT_RateDenominator}</td>
									<td>{r.ZAT_StartDate}</td>
									<td>{r.ZAT_EndDate}</td>
									<td>
										<input
											type="checkbox"
											checked={r.ZAT_IsSystem}
											readOnly
										/>
									</td>
									<td>
										<input
											type="checkbox"
											checked={r.ZAT_IsPublished}
											readOnly
										/>
									</td>
									<td>
										<Button
											type="button"
											className="btn btn-info btn-sm"
											onClick={() => onEditButtonClick(r.ZAT_PK)}
											disabled={!r.ZAT_IsEditable}
										>
											Edit
										</Button>
									</td>
								</tr>
							);
						}
					})}
				</tbody>
			</table>
			<div>
				{saveMessage.length > 0 ? (
					<PopupForm
						title="Information"
						message={saveMessage}
						handleHideModal={() => setSaveMessage("")}
					/>
				) : null}
			</div>
		</div>
	);
};

export default RefAccTaxRateUserViewSearchForm;
