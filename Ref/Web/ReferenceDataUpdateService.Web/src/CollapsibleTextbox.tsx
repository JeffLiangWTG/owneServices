import React, { useState, useRef, useEffect } from "react";
import { IEntity } from "./models/IEntity";
import { IValidationResults } from "./ValidationResults";
import { ValidationHelper } from "./ValidationHelper";
import { ValidationStrip } from "./ValidationStrip";
import { EntityHelper } from "./EntityHelper";
import { IParentProps } from "./IParentProps";

interface ICollapsibleTextboxProps {
	entity: IEntity;
	label?: string;
	propertyName: string;
	validationResults?: IValidationResults;
	onValueChange: (entity: any, propertyName: string, value: object) => void;
	onValueChanged: (entity: any, propertyName: string) => void;
	maxLength?: number;
	parentProps?: IParentProps;
}

export const CollapsibleTextbox = ({
	entity,
	label,
	propertyName,
	validationResults,
	onValueChange,
	onValueChanged,
	maxLength,
	parentProps,
}: ICollapsibleTextboxProps) => {
	const [isExpanded, setIsExpanded] = useState(false);
	const [valueChanged, setValueChanged] = useState(false);
	const textAreaRef = useRef<HTMLTextAreaElement>(null);

	const readOnly = (): boolean => {
		return (
			(parentProps !== undefined && parentProps.readOnly === true) ||
			!EntityHelper.isEditable(entity)
		);
	};

	const handleValueChange = async (value: object) => {
		onValueChange(entity, propertyName, value);
		setValueChanged(true);
	};

	const handleValueChanged = async () => {
		if (valueChanged) {
			onValueChanged(entity, propertyName);
		}
		setValueChanged(false);
	};

	const handleBlur = () => {
		setIsExpanded(false);
		handleValueChanged();
	};

	const handleFocus = () => {
		setIsExpanded(true);
	};

	const renderContent = () => {
		const value = entity[propertyName];
		if (isExpanded) {
			return value ?? "";
		} else {
			return value && value.length > 100
				? value.substring(0, 100) + " ..."
				: value ?? "";
		}
	};

	const adjustTextareaHeight = () => {
		if (textAreaRef.current) {
			textAreaRef.current.style.height = "auto";
			textAreaRef.current.style.height =
				textAreaRef.current.scrollHeight + "px";
		}
	};

	useEffect(() => {
		adjustTextareaHeight();
	}, [isExpanded]);

	const renderBare = () => {
		return (
			<div>
				<div className="d-flex">
					<textarea
						ref={textAreaRef}
						className={
							"form-control " +
							(ValidationHelper.getValidationResults(
								propertyName,
								validationResults
							).length > 0
								? "is-invalid"
								: "")
						}
						value={renderContent()}
						onChange={(e) => {
							handleValueChange(e.target.value as any);
							adjustTextareaHeight();
						}}
						onBlur={handleBlur}
						onFocus={handleFocus}
						readOnly={readOnly()}
						rows={1}
						maxLength={
							maxLength
								? maxLength
								: Object.getPrototypeOf(entity).constructor[
								propertyName + "_MaxLength"
								]
						}
						style={{ overflow: "hidden", resize: "none" }}
					/>
				</div>
				<ValidationStrip
					propertyName={propertyName}
					validationResults={validationResults}
				/>
			</div>
		);
	}

	const render = () => {
		if (label) {
			return (
				<div className="form-group row">
					<label
						className={
							"col-sm-4 col-form-label " +
							(ValidationHelper.getValidationResults(
								propertyName,
								validationResults
							).length > 0
								? "text-danger"
								: "")
						}
					>
						{label}
					</label>
					<div className="col-sm-8">{renderBare()}</div>
				</div>
			);
		} else {
			return renderBare();
		}
	};

	return render();
};
