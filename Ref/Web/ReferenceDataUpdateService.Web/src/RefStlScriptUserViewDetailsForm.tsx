import React, { useState, useEffect, useCallback } from "react";
import uuid from "uuid";
import { IEntityManager, ServiceType } from "./EntityManager";
import _ from "underscore";
import { Filter, FilterOps } from "./Filter";
import update from "immutability-helper";
import { TextInput } from "./TextInput";
import { CheckBox } from "./CheckBox";
import { TextSelect } from "./TextSelect";
import { CollapsibleTextbox } from "./CollapsibleTextbox";
import { DateTimeInput } from "./DateTimeInput";
import { ValidationServiceWrapper } from "./ValidationService";
import { IValidationResults } from "./ValidationResults";
import { ValidationHelper } from "./ValidationHelper";
import { ValidationServiceHelper } from "./ValidationServiceHelper";
import { PopupForm } from "./PopupForm";
import { RefStlScriptUserView } from "./models/RefStlScriptUserView";
import Button from "./Button";

export interface IRefStlScriptUserViewDetailsFormProps {
	id?: string;
	systemVersion?: string;
	entityManager: IEntityManager;
	onSaved?: () => Promise<void>;
}

export const RefStlScriptUserViewDetailsForm = (props: IRefStlScriptUserViewDetailsFormProps) => {
	const [stlScript, setStlScript] = useState<RefStlScriptUserView>(() => {
		let stlScriptView = new RefStlScriptUserView();
		stlScriptView.STL_PK = uuid.v1();
		stlScriptView.STL_FeatureCode = "";
		stlScriptView.STL_RoleName = "";
		stlScriptView.STL_ModuleName = "";
		stlScriptView.STL_FunctionName = "";
		stlScriptView.STL_FeatureName = "";
		stlScriptView.STL_DataGranularity = "TRN";
		stlScriptView.STL_CompanyCode = "";
		stlScriptView.STL_BranchCode = "";
		stlScriptView.STL_TransactionDateUtc = "";
		stlScriptView.STL_CreatingUserCode = "";
		stlScriptView.STL_GuidReference = "";
		stlScriptView.STL_BillingReference1 = "";
		stlScriptView.STL_BillingReference2 = "";
		stlScriptView.STL_BillingReference3 = "";
		stlScriptView.STL_BillingReference4 = "";
		stlScriptView.STL_AdditionalRefs = "";
		stlScriptView.STL_TransactionCount = "1";
		stlScriptView.STL_PreparationScript = "";
		stlScriptView.STL_FromClause = "";
		stlScriptView.STL_WhereClause = "";
		stlScriptView.STL_WithOptionRecompile = false;
		stlScriptView.STL_UsedInBilling = true;
		stlScriptView.STL_ActiveOn = "ALL";
		stlScriptView.STL_MinCW1Version = "";
		stlScriptView.STL_MaxCW1Version = "";
		stlScriptView.STL_DateType = "DTE";
		stlScriptView.STL_CollectionStartDateUtc = null;
		stlScriptView.STL_IsSystem = true;
		stlScriptView.STL_IsPublished = true;
		stlScriptView.STL_IsEditable = true;
		return stlScriptView;
	});

	const [validationResults, setValidationResults] = useState<IValidationResults>({});
	const [saveMessage, setSaveMessage] = useState<string>("");
	const [readOnly, setReadOnly] = useState<boolean>(false);
	const [saveButtonDisabled, setSaveButtonDisabled] = useState<boolean>(false);
	const isKeyReadOnly = props.entityManager.isInDatabase(stlScript) && stlScript.STL_IsSystem;

	const validationService = new ValidationServiceWrapper(
		[
			ValidationServiceHelper.getRefStlScriptValidationService(
				props.entityManager
			),
		],
		(isValidating: boolean) => setSaveButtonDisabled(isValidating)
	);

	const loadStlScript = useCallback(async () => {
		if (props.id || props.entityManager.isInDatabase(stlScript)) {
			setReadOnly(props.systemVersion !== undefined);
			const dataArray = await Promise.all([
				props.entityManager.getAsync<RefStlScriptUserView>(
					"RefStlScriptUserView",
					[ServiceType.Safe],
					[
						new Filter(
							"STL_PK",
							FilterOps.Equals,
							props.id ? props.id : stlScript.STL_PK as any,
							"guid"
						),
					],
					false,
					props.systemVersion
				),
			]);
			const stlScriptData = _.first(dataArray[0]);
			if (stlScriptData) {
				setStlScript(stlScriptData);
			}
		} else {
			props.entityManager.add(stlScript, "RefStlScriptUserView");
		}
	}, [props.id, props.systemVersion, props.entityManager, stlScript]);

	useEffect(() => {
		props.entityManager.clear();
		loadStlScript();

		return () => {
			Promise.all([
				props.entityManager.reload("RefStlScriptUserView", [ServiceType.Safe], [
						new Filter(
							"STL_PK",
							FilterOps.Equals,
							props.id ? props.id : stlScript.STL_PK as any,
							"guid"
						),
					]),
			]);
		}
	}, [props.systemVersion, props.entityManager]);

	const onValueChange = async (entity: any, name: string, value: object): Promise<void> => {
		const updatedStlScript = update(stlScript, { [name]: { $set: value } });
		setStlScript(updatedStlScript);
		props.entityManager.update(updatedStlScript);
	};

	const onValueChanged = async (entity: any, name: string): Promise<void> => {
		const updatedValidationResults = update(validationResults, {
			[name]: {
				$set: await ValidationHelper.validateProperty(
					validationService,
					stlScript,
					name
				),
			},
		});
		setValidationResults(updatedValidationResults);
	};

	const save = async (): Promise<void> => {
		const stlScriptValidationResults = await ValidationHelper.validate(
			validationService,
			stlScript,
			0,
			true
		);
		setValidationResults(stlScriptValidationResults);

		if (!ValidationHelper.hasErrors([validationResults])) {
			const result = await props.entityManager.saveChanges(ServiceType.Safe);
			setSaveMessage(result.message);
			if (props.onSaved) {
				props.onSaved();
			}
		} else {
			setSaveMessage("Please fix all errors before saving.");
		}
	};

	return (
		<div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Feature Code
				</label>
				<div className="col-sm-4">
					<TextInput
						parentProps={{ readOnly: readOnly }}
						inputType="Text"
						propertyName="STL_FeatureCode"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						readOnly={isKeyReadOnly}
						validationResults={validationResults}
					/>
				</div>
				<div className="col-sm-5">
					<CheckBox
						parentProps={{ readOnly: readOnly }}
						label="Is Published"
						propertyName="STL_IsPublished"
						entity={stlScript}
						onValueChange={onValueChange}
						validationResults={validationResults}
					/>
				</div>
				<label className="col-sm-2 col-form-label">
					Min CW Version
				</label>
				<div className="col-sm-4">
					<TextInput
						parentProps={{ readOnly: readOnly }}
						inputType="Text"
						propertyName="STL_MinCW1Version"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						readOnly={isKeyReadOnly}
						validationResults={validationResults}
					/>
				</div>
				<div className="col-sm-5">
					<CheckBox
						parentProps={{ readOnly: readOnly }}
						label="Recompile Option"
						propertyName="STL_WithOptionRecompile"
						entity={stlScript}
						onValueChange={onValueChange}
						validationResults={validationResults}
					/>
				</div>
				<label className="col-sm-2 col-form-label">
					Max CW Version
				</label>
				<div className="col-sm-4">
					<TextInput
						parentProps={{ readOnly: readOnly }}
						inputType="Text"
						propertyName="STL_MaxCW1Version"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						readOnly={isKeyReadOnly}
						validationResults={validationResults}
					/>
				</div>
				<div className="col-sm-5">
					<CheckBox
						parentProps={{ readOnly: readOnly }}
						label="Used In Billing"
						propertyName="STL_UsedInBilling"
						entity={stlScript}
						onValueChange={onValueChange}
						validationResults={validationResults}
					/>
				</div>
				<label className="col-sm-2 col-form-label">
					Active On
				</label>
				<div className="col-sm-4">
					<TextSelect
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_ActiveOn"
						entity={stlScript}
						options={["ALL", "NON", "PRD", "TST"]}
						values={["ALL", "NON", "PRD", "TST"]}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						readOnly={isKeyReadOnly}
						validationResults={validationResults}
					/>
				</div>
				<div className="col-sm-5">
					<TextSelect
						parentProps={{ readOnly: readOnly }}
						label="Date Type"
						propertyName="STL_DateType"
						entity={stlScript}
						options={["SDT", "DTE", "DTO"]}
						values={["SDT", "DTE", "DTO"]}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
				<label className="col-sm-2 col-form-label">
					Start Date Utc
				</label>
				<div className="col-sm-4">
					<DateTimeInput
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_CollectionStartDateUtc"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						format="DD/MM/YYYY HH:mm:SS"
						validationResult={validationResults}
					/>
				</div>
				<div className="col-sm-5">
					<TextSelect
						parentProps={{ readOnly: readOnly }}
						label="Data Granularity"
						propertyName="STL_DataGranularity"
						entity={stlScript}
						options={["TRN", "MAH", "MCO", "DAY", "SPS"]}
						values={["TRN", "MAH", "MCO", "DAY", "SPS"]}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>

			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Role Name
				</label>
				<div className="col-sm-9">
					<TextInput
						parentProps={{ readOnly: readOnly }}
						inputType="Text"
						propertyName="STL_RoleName"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Module Name
				</label>
				<div className="col-sm-9">
					<TextInput
						parentProps={{ readOnly: readOnly }}
						inputType="Text"
						propertyName="STL_ModuleName"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Function Name
				</label>
				<div className="col-sm-9">
					<TextInput
						parentProps={{ readOnly: readOnly }}
						inputType="Text"
						propertyName="STL_FunctionName"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Feature Name
				</label>
				<div className="col-sm-9">
					<TextInput
						parentProps={{ readOnly: readOnly }}
						inputType="Text"
						propertyName="STL_FeatureName"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Company Code
				</label>
				<div className="col-sm-9">
					<CollapsibleTextbox
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_CompanyCode"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Branch Code
				</label>
				<div className="col-sm-9">
					<CollapsibleTextbox
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_BranchCode"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Transaction Date Utc
				</label>
				<div className="col-sm-9">
					<CollapsibleTextbox
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_TransactionDateUtc"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Creating User Code
				</label>
				<div className="col-sm-9">
					<CollapsibleTextbox
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_CreatingUserCode"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Guid Reference
				</label>
				<div className="col-sm-9">
					<CollapsibleTextbox
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_GuidReference"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Billing Reference 1
				</label>
				<div className="col-sm-9">
					<CollapsibleTextbox
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_BillingReference1"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Billing Reference 2
				</label>
				<div className="col-sm-9">
					<CollapsibleTextbox
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_BillingReference2"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Billing Reference 3
				</label>
				<div className="col-sm-9">
					<CollapsibleTextbox
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_BillingReference3"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Billing Reference 4
				</label>
				<div className="col-sm-9">
					<CollapsibleTextbox
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_BillingReference4"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Additional References
				</label>
				<div className="col-sm-9">
					<CollapsibleTextbox
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_AdditionalRefs"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Transaction Count
				</label>
				<div className="col-sm-9">
					<CollapsibleTextbox
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_TransactionCount"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Preparation Script
				</label>
				<div className="col-sm-9">
					<CollapsibleTextbox
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_PreparationScript"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					From Clause
				</label>
				<div className="col-sm-9">
					<CollapsibleTextbox
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_FromClause"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="row form-group">
				<label className="col-sm-2 col-form-label">
					Where Clause
				</label>
				<div className="col-sm-9">
					<CollapsibleTextbox
						parentProps={{ readOnly: readOnly }}
						propertyName="STL_WhereClause"
						entity={stlScript}
						onValueChange={onValueChange}
						onValueChanged={onValueChanged}
						validationResults={validationResults}
					/>
				</div>
			</div>
			<div className="form-group row">
				<div className="col-sm-1">
					<Button
						type="button"
						className="btn btn-info"
						onClick={save}
						disabled={readOnly || saveButtonDisabled}
					>
						Save
					</Button>
				</div>
			</div>
			{saveMessage.length > 0 ? (
				<PopupForm
					title="Information"
					message={saveMessage}
					handleHideModal={() => setSaveMessage("")}
					validationResults={[validationResults]}
				/>
			) : null}
		</div>
	);
}



