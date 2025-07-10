import React from "react";
import { IEntity } from "./models/IEntity";
import { IValidationResults } from "./ValidationResults";
import { ValidationHelper } from "./ValidationHelper";
import { ValidationStrip } from "./ValidationStrip";
import { EntityHelper } from "./EntityHelper";
import { IParentProps } from "./IParentProps";

interface ITextAreaProps {
	entity: IEntity;
	label: string;
	propertyName: string;
	validationResults?: IValidationResults;
	onValueChange: (entity: any, propertyName: string, value: object) => void;
	onValueChanged: (entity: any, propertyName: string) => void;
	maxLength?: number;
	parentProps?: IParentProps;
}

export class TextArea extends React.Component<ITextAreaProps> {
	constructor(props: ITextAreaProps) {
		super(props);
		this.onValueChange = this.onValueChange.bind(this);
		this.onValueChanged = this.onValueChanged.bind(this);
	}

	valueChanged: boolean = false;

	async onValueChange(value: object) {
		this.props.onValueChange(this.props.entity, this.props.propertyName, value)
		this.valueChanged = true;
	}

	async onValueChanged() {
		if (this.valueChanged) {
			this.props.onValueChanged(this.props.entity, this.props.propertyName);
		}
		this.valueChanged = false;
	}

	readOnly(): boolean {
		return (this.props.parentProps !== undefined && this.props.parentProps.readOnly === true) || !EntityHelper.isEditable(this.props.entity);
	}

	render() {
		return <div className="form-group row">
			<label className={"col-sm-4 col-form-label " + (ValidationHelper.getValidationResults(this.props.propertyName, this.props.validationResults).length > 0 ? "text-danger" : "")}>{this.props.label}</label>
			<div className="col-sm-8">
				<textarea className={"form-control " + (ValidationHelper.getValidationResults(this.props.propertyName, this.props.validationResults).length > 0 ? "is-invalid" : "")} value={this.props.entity[this.props.propertyName]}
					onChange={e => this.onValueChange(e.target.value as any)} onBlur={this.onValueChanged} readOnly={this.readOnly()} maxLength={this.props.maxLength ? this.props.maxLength : Object.getPrototypeOf(this.props.entity).constructor[this.props.propertyName + '_MaxLength']} />
				<ValidationStrip propertyName={this.props.propertyName} validationResults={this.props.validationResults} />
			</div>
		</div>
	}
}
