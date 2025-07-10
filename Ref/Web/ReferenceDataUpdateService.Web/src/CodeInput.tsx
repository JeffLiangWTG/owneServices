import React from "react";
import { IEntity } from "./models/IEntity";
import { IValidationResults } from "./ValidationResults";
import { IEntityManager, ServiceType } from "./EntityManager";
import { ValidationHelper } from "./ValidationHelper";
import uuid from "uuid";
import { ValidationStrip } from "./ValidationStrip";
import { EntityHelper } from "./EntityHelper";
import { IParentProps } from "./IParentProps";

interface ICodeInputProps {
	entity: IEntity;
	label?: string;
	propertyName: string;
	readOnly?: boolean;
	validationResults?: IValidationResults;
	onValueChange: (entity: any, propertyName: string, value: object) => void;
	onFocusing?: (entity: any) => void;
	onValueChanged: (entity: any, propertyName: string) => void;
	listEntityTypeName: string | IEntity[];
	listCodePropertyName: string;
	listDescriptionPropertyName: string;
	entityManager?: IEntityManager;
	maxLength?: number;
	prevEntity?: IEntity[];
	parentProps?: IParentProps;
	serviceType?: ServiceType;
	ariaLabel?: string;
}

interface ICodeInputState {
	selectedValues: IEntity[],
}

export class CodeInput extends React.Component<ICodeInputProps, ICodeInputState> {
	constructor(props: ICodeInputProps) {
		super(props);
		this.onValueChange = this.onValueChange.bind(this);
		this.onValueChanged = this.onValueChanged.bind(this);
		this.state = {
			selectedValues: []
		};
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
		return (this.props.parentProps !== undefined && this.props.parentProps.readOnly === true) || !EntityHelper.isEditable(this.props.entity) || this.props.readOnly === true;
	}

	async componentDidMount() {
		if (Array.isArray(this.props.listEntityTypeName)) {
			this.setState({
				selectedValues: this.props.listEntityTypeName
			});
		}
		else if (this.props.entityManager != null && this.props.prevEntity == null) {
			let serviceType = ServiceType.Safe;
			if (this.props.serviceType) {
				serviceType = this.props.serviceType;
			}
			var selectedValues = (await this.props.entityManager.getAsync<IEntity>(this.props.listEntityTypeName, [serviceType], [], false));
			this.setState({
				selectedValues: selectedValues
			});
		}
	}

	renderWithoutLabel() {
		let dataListId = uuid.v1();
		return <div>
			<input className={"form-control " + (ValidationHelper.getValidationResults(this.props.propertyName, this.props.validationResults).length > 0 ? "is-invalid text-danger" : "")} type="text" value={this.props.entity[this.props.propertyName]}
				onChange={e => this.onValueChange(e.target.value as any)} readOnly={this.readOnly()} list={dataListId} maxLength={this.props.maxLength ? this.props.maxLength : Object.getPrototypeOf(this.props.entity).constructor[this.props.propertyName + '_MaxLength']}
				onFocus={this.props.onFocusing != undefined ? (e => this.props.onFocusing!(this.props.entity)) : undefined}
				onBlur={(this.onValueChanged)}
				aria-label={this.props.ariaLabel}
			/>
			<datalist id={dataListId}>
				{(this.props.prevEntity !== undefined ? this.props.prevEntity : this.state.selectedValues)
					.map((v, index) => (
						<option value={v[this.props.listCodePropertyName] as any} key={`${v[this.props.listCodePropertyName]}-${index}`}>
							{v[this.props.listDescriptionPropertyName]}
						</option>
					))}
			</datalist>
			<ValidationStrip propertyName={this.props.propertyName} validationResults={this.props.validationResults} />
		</div>
	}

	render() {
		if (this.props.label) {
			return <div className="form-group row">
				<label className={"col-sm-4 col-form-label " + (ValidationHelper.getValidationResults(this.props.propertyName, this.props.validationResults).length > 0 ? "text-danger" : "")}>{this.props.label}</label>
				<div className="col-sm-8">
					{this.renderWithoutLabel()}
				</div>
			</div>
		}
		else {
			return this.renderWithoutLabel();
		}
	}
}
