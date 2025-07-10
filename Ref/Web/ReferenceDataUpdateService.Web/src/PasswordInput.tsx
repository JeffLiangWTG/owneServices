import React from "react";
import { IEntity } from "./models/IEntity";
import { IValidationResults } from "./ValidationResults";
import { ValidationHelper } from "./ValidationHelper";
import { ValidationStrip } from "./ValidationStrip";
import { EntityHelper } from "./EntityHelper";
import { IParentProps } from "./IParentProps";

interface IPasswordInputState {
	passwordVisible: boolean;
}

interface IPasswordInputProps {
	entity: IEntity;
	propertyName: string;
	readOnly?: boolean;
	validationResults?: IValidationResults;
	maxLength?: number;
	parentProps?: IParentProps;
	onValueChange: (entity: any, propertyName: string, value: object) => void;
	onValueChanged: (entity: any, propertyName: string) => void;
}

export class PasswordInput extends React.Component<IPasswordInputProps, IPasswordInputState> {

	constructor(props: IPasswordInputProps) {
		super(props);
		this.state = {
			passwordVisible: false,
		};
		this.onValueChange = this.onValueChange.bind(this);
		this.onValueChanged = this.onValueChanged.bind(this);
		this.togglePasswordVisibility = this.togglePasswordVisibility.bind(this);
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

	readOnly(): boolean {
		return (
			(this.props.parentProps !== undefined &&
				this.props.parentProps.readOnly === true) ||
			!EntityHelper.isEditable(this.props.entity) ||
			this.props.readOnly === true
		);
	}

	togglePasswordVisibility = () => {
		this.setState((prevState) => ({
			passwordVisible: !prevState.passwordVisible,
		}));
	};

	render() {
		return (
			<div className="input-group">
				<input
					type={this.state.passwordVisible ? 'text' : 'password'}
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
				/>
				<div className="input-group-append row">
					<span className="input-group-text toggle-password" onClick={this.togglePasswordVisibility}>
						{this.state.passwordVisible ?
						(
						  <i className="fa fa-eye"></i>
						) :
						(
						  <i className="fa fa-eye-slash"></i>
						)}
					</span>
				</div>
				<ValidationStrip
					propertyName={this.props.propertyName}
					validationResults={this.props.validationResults}
				/>
			</div>
		);
	}
}
