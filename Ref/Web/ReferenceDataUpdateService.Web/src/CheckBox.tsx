import React, { ChangeEvent } from "react";
import { IEntity } from "./models/IEntity";
import { IValidationResults } from "./ValidationResults";
import { ValidationHelper } from "./ValidationHelper";
import { EntityHelper } from "./EntityHelper";
import { IParentProps } from "./IParentProps";

interface ICheckBoxProps {
	entity: IEntity | null;
	label?: string;
	propertyName: string;
	inlineLabel?: boolean;
	isReadOnly?: boolean;
	isHidden?: boolean;
	validationResults?: IValidationResults;
	onValueChange?: (entity: any, propertyName: string, value: object) => void;
	onValueChanged?: (entity: any, propertyName: string) => void;
	parentProps?: IParentProps;
}

export class CheckBox extends React.Component<ICheckBoxProps> {
	constructor(props: ICheckBoxProps) {
		super(props);
		this.getValueSafe = this.getValueSafe.bind(this);
		this.onValueChange = this.onValueChange.bind(this);
		this.onValueChanged = this.onValueChanged.bind(this);
	}

	valueChanged: boolean = false;

	readOnly(): boolean {
		return (
			(this.props.parentProps !== undefined &&
				this.props.parentProps.readOnly === true) ||
			(this.props.entity != null &&
				!EntityHelper.isEditable(this.props.entity)) ||
			this.props.isReadOnly === true
		);
	}

	getValueSafe(): boolean {
		if (this.props.entity) {
			let propertyValue = this.props.entity[this.props.propertyName];
			if (propertyValue) {
				return Boolean(JSON.parse(propertyValue));
			}
		}
		return false;
	}

	onValueChange(e: ChangeEvent<HTMLInputElement>): void {
		let isReadOnly = this.props.onValueChange === undefined || this.readOnly();
		if (isReadOnly) {
			return;
		} else {
			this.props.onValueChange!(
				this.props.entity,
				this.props.propertyName,
				e.target.checked as any
			);
			this.valueChanged = true;
		}
	}

	onValueChanged(): void {
		if (this.valueChanged && this.props.onValueChanged != undefined) {
			this.props.onValueChanged(this.props.entity, this.props.propertyName);
		}
		this.valueChanged = false;
	}

	render() {
		let input = (
			<input
				className={
					"form-check-input " +
					(ValidationHelper.getValidationResults(
						this.props.propertyName,
						this.props.validationResults
					).length > 0
						? "is-invalid"
						: "")
				}
				type="checkbox"
				checked={this.getValueSafe()}
				onChange={this.onValueChange}
				onBlur={this.onValueChanged}
				hidden={this.props.isHidden}
			/>
		);
		if (this.props.label) {
			if (this.props.inlineLabel) {
				return (
					<div className="form-check">
						{input}
						<label
							className={
								"form-check-label " +
								(ValidationHelper.getValidationResults(
									this.props.propertyName,
									this.props.validationResults
								).length > 0
									? "text-danger"
									: "")
							}
							htmlFor={this.props.propertyName}
						>
							{this.props.label}
						</label>
					</div>
				);
			} else {
				return (
					<div className="form-group row">
						<label
							className={
								"col-sm-4 col-form-label " +
								(ValidationHelper.getValidationResults(
									this.props.propertyName,
									this.props.validationResults
								).length > 0
									? "text-danger"
									: "")
							}
						>
							{this.props.label}
						</label>
						<div className="col-sm-8">{input}</div>
					</div>
				);
			}
		} else {
			return <div>{input}</div>;
		}
	}
}
