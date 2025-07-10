import React from "react";
import { IEntity } from "./models/IEntity";
import { IValidationResults } from "./ValidationResults";
import { ValidationHelper } from "./ValidationHelper";
import { ValidationStrip } from "./ValidationStrip";
import { EntityHelper } from "./EntityHelper";
import { IParentProps } from "./IParentProps";

interface ITextInputProps {
	entity: IEntity;
	label?: string;
	propertyName: string;
	readOnly?: boolean;
	validationResults?: IValidationResults;
	maxLength?: number;
	renderBare?: boolean;
	inputType: string;
	parentProps?: IParentProps;
	onValueChange: (entity: any, propertyName: string, value: object) => void;
	onValueChanged: (entity: any, propertyName: string) => void;
	ariaLabel?: string;
}

export class TextInput extends React.Component<ITextInputProps> {
	constructor(props: ITextInputProps) {
		super(props);
		this.onValueChange = this.onValueChange.bind(this);
		this.onValueChanged = this.onValueChanged.bind(this);
		this.renderBare = this.renderBare.bind(this);
	}

	valueChanged: boolean = false;

	async onValueChange(value: object) {
		this.props.onValueChange(this.props.entity, this.props.propertyName, value);
		this.valueChanged = true;
	}

	async onValueChanged() {
		if (this.valueChanged && this.props.onValueChanged) {
			this.props.onValueChanged(this.props.entity, this.props.propertyName);
		}
		this.valueChanged = false;
	}

	readOnly(): boolean {
		return (
			(this.props.parentProps !== undefined &&
				this.props.parentProps.readOnly === true) ||
			!EntityHelper.isEditable(this.props.entity) ||
			this.props.readOnly === true
		);
	}

	renderBare() {
		return (
			<div>
				<input
					type={this.props.inputType}
					className={
						"form-control " +
						(ValidationHelper.getValidationResults(
							this.props.propertyName,
							this.props.validationResults
						).length > 0
							? "is-invalid  text-danger"
							: "")
					}
					value={this.props.entity[this.props.propertyName]}
					onChange={(e) => this.onValueChange(e.target.value as any)}
					onBlur={this.onValueChanged}
					maxLength={
						this.props.maxLength
							? this.props.maxLength
							: Object.getPrototypeOf(this.props.entity).constructor[
							this.props.propertyName + "_MaxLength"
							]
					}
					readOnly={this.readOnly()}
					aria-label={this.props.ariaLabel}
				/>
				<ValidationStrip
					propertyName={this.props.propertyName}
					validationResults={this.props.validationResults}
				/>
			</div>
		);
	}

	render() {
		if (this.props.label) {
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
					<div className="col-sm-8">{this.renderBare()}</div>
				</div>
			);
		} else {
			return this.renderBare();
		}
	}
}
