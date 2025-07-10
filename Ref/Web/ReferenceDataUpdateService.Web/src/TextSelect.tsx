import React, { ChangeEvent } from "react";
import { EntityHelper } from "./EntityHelper";
import { IParentProps } from "./IParentProps";
import { IEntity } from "./models/IEntity";
import { ValidationHelper } from "./ValidationHelper";
import { IValidationResults } from "./ValidationResults";
import { ValidationStrip } from "./ValidationStrip";

interface ITextSelectProps {
    entity: IEntity,
    label?: string,
    propertyName: string,
    readOnly?: boolean,
    validationResults?: IValidationResults,
    options: string[],
    values: string[],
    parentProps?: IParentProps,
    onValueChange: (entity: any, propertyName: string, value: object) => void,
    onValueChanged: (entity: any, propertyName: string) => void
}

export class TextSelect extends React.Component<ITextSelectProps> {
    constructor(props: ITextSelectProps) {
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
        if (this.valueChanged) {
			this.props.onValueChanged(this.props.entity, this.props.propertyName);
		}
		this.valueChanged = false;
    }

    readOnly() {
		return (this.props.parentProps !== undefined && this.props.parentProps.readOnly === true) || !EntityHelper.isEditable(this.props.entity) || this.props.readOnly === true;
	}

    renderBare() {
        return (
            <div>
                <select className={"form-control " + (ValidationHelper.getValidationResults(this.props.propertyName, this.props.validationResults).length > 0 ? "is-invalid text-danger" : "")} disabled={this.readOnly()}
                value={this.props.entity[this.props.propertyName]} onChange={e => this.onValueChange(e.target.value as any)} onBlur={this.onValueChanged}>
                    {
                        this.props.options.map((v, i) => <option key={v} value={this.props.values[i]}>{v}</option>)
                    }
                </select>
            </div>
        );
    }

    render() {
        if (this.props.label) {
            return (
                <div className="form-group row">
                    <label className={"col-sm-4 col-form-label" + (ValidationHelper.getValidationResults(this.props.propertyName, this.props.validationResults).length > 0 ? "is-invalid text-danger" : "")}>{this.props.label}</label>
                    <div className="col-sm-8">{this.renderBare()}</div>
                    <div className="col-sm-12">
                        <ValidationStrip propertyName={this.props.propertyName} validationResults={this.props.validationResults} />
                    </div>
                </div>
            );
        }
        else {
            return this.renderBare();
        }
    }
}