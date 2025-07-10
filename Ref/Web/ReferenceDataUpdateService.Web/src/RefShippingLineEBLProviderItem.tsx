import React from "react";
import {
	RefShippingLineEBLProviderName,
	RefShippingLineEBLProviderWrapper,
} from "./models/IRefShippingLineEBLProvider";
import { CheckBox } from "./CheckBox";
import { IValidationResults } from "./ValidationResults";
import { CodeInput } from "./CodeInput";

interface IRefShippingLineEBLProviderItemProps {
	providerItem: RefShippingLineEBLProviderWrapper;
	onValueChange(entity: any, name: string, value: object): void;
	onValueChanged(entity: any, name: string): void;
	handleDeleteEBLProviderItem(providerItemPk: string): void;
	validationResults?: IValidationResults;
	eblProviderDistinctNames: RefShippingLineEBLProviderName[];
}

export const RefShippingLineEBLProviderItem = ({
	providerItem,
	onValueChange,
	onValueChanged,
	handleDeleteEBLProviderItem,
	validationResults,
	eblProviderDistinctNames,
}: IRefShippingLineEBLProviderItemProps) => {
	return (
		<>
			<div className="col-sm-5">
				<CodeInput
					propertyName="RSE_Name"
					entity={providerItem}
					onValueChange={onValueChange}
					onValueChanged={onValueChanged}
					validationResults={validationResults}
					listCodePropertyName="RSE_Name"
					listDescriptionPropertyName="RSE_Name"
					listEntityTypeName={eblProviderDistinctNames}
				/>
			</div>
			<div className="col-sm-3 form-check mt-2" style={{ textAlign: "center" }}>
				<CheckBox
					entity={providerItem}
					propertyName="RSE_IsAvailable"
					onValueChange={onValueChange}
					onValueChanged={onValueChanged}
					validationResults={validationResults}
				/>
			</div>
			<div className="col-sm-3 form-check mt-2" style={{ textAlign: "center" }}>
				<CheckBox
					entity={providerItem}
					propertyName="RSE_IsDefault"
					onValueChange={onValueChange}
					onValueChanged={onValueChanged}
					isHidden={providerItem.IsDefaultHidden}
				/>
			</div>
			<div className="col-sm-1">
				<input
					type="button"
					className="btn btn-info"
					value="-"
					onClick={() => handleDeleteEBLProviderItem(providerItem.RSE_PK)}
				/>
			</div>
		</>
	);
};
