import React from "react";
import { IEntity } from "./models/IEntity";
import { EntityHelper } from "./EntityHelper";
import { ValidationStrip } from "./ValidationStrip";
import { IValidationResults } from "./ValidationResults";
import { ValidationHelper } from "./ValidationHelper";
import { IParentProps } from "./IParentProps";
import uuid from "uuid";
import $ from "jquery";
import moment from "moment";
import "tempusdominus-bootstrap-4";

interface IDateTimeInputProps {
	entity?: IEntity;
	label?: string;
	propertyName?: string;
	readOnly?: boolean;
	onValueChange: (entity: any, propertyName: string, value: object) => void;
	onValueChanged: (entity: any, propertyName: string) => void;
	validationResult?: IValidationResults;
	parentProps?: IParentProps;
	format?: string;
	useLocalTime?: boolean;
	dateOnly?: boolean;
}

export class DateTimeInput extends React.Component<IDateTimeInputProps> {
	constructor(props: IDateTimeInputProps) {
		super(props);
		this.inputId = uuid.v1();
		this.previousValue = "";
		this.onValueChanged = this.onValueChanged.bind(this);
	}
	inputId: string;
	previousValue: string;
	get useLocalTime(): boolean {
		return this.props.useLocalTime ?? false;
	}
	get dateOnly(): boolean {
		return this.props.dateOnly ?? false;
	}

	async onValueChanged() {
		if (this.props.onValueChanged) {
			this.props.onValueChanged(
				this.props.entity,
				this.props.propertyName ?? ""
			);
		}
	}

	readOnly(): boolean {
		return (
			(this.props.parentProps !== undefined &&
				this.props.parentProps.readOnly === true) ||
			(this.props.entity && !EntityHelper.isEditable(this.props.entity)) ||
			this.props.readOnly === true
		);
	}

	handleInputBlur = (e: React.FocusEvent<HTMLInputElement>) => {
		if (this.props.propertyName && this.props.entity) {
			const inputValue = e.target.value;
			const parsed = moment(inputValue, this.props.format, true);
			if (parsed.isValid()) {
				const formatted = this.formatDate(parsed);
				if (formatted !== this.props.entity[this.props.propertyName]) {
					this.props.onValueChange(this.props.entity, this.props.propertyName, formatted as any);
				}
			} else if (inputValue.trim() === "") {
				this.props.onValueChange(this.props.entity, this.props.propertyName, null as any);
			}
		}
		this.onValueChanged();
	};

	componentDidMount() {
		($("#" + this.inputId) as any).datetimepicker({
			format: this.props.format,
		});
		if (this.props.entity && this.props.propertyName) {
			($("#" + this.inputId) as any).datetimepicker(
				"date",
				this.parseDate(this.props.entity[this.props.propertyName])
			);
		}
		($("#" + this.inputId) as any).on(
			"change.datetimepicker",
			(e: { date: moment.Moment }) => {
				if (e.date && this.props.entity && this.props.propertyName) {
					let formatDate = this.formatDate(e.date);
					if (formatDate !== this.props.entity[this.props.propertyName]) {
						this.props.onValueChange(this.props.entity, this.props.propertyName, formatDate as any);
					}
				}
			}
		);
	}

	componentDidUpdate() {
		let hasDateTime = this.props.entity && this.props.propertyName && this.props.entity[this.props.propertyName];
		if (hasDateTime) {
			($("#" + this.inputId) as any).datetimepicker(
				"date",
				this.parseDate(this.props.entity![this.props.propertyName!])
			);
		} else {
			($("#" + this.inputId) as any).datetimepicker("date", null);
		}
	}

	formatDate(date: moment.Moment): string {
		if (this.dateOnly) {
			return date.format("YYYY-MM-DD");
		}
		return date.format("YYYY-MM-DDTHH:mm:ss") + (!this.useLocalTime ? "Z" : "");
	}

	parseDate(input: string): moment.Moment {
		if (this.dateOnly) {
			input = input.slice(0, 10) + "T00:00:00Z";
		}
		if (this.useLocalTime) {
			input = input.replace("Z", "");
			return moment(input);
		} else {
			return moment.utc(input);
		}
	}

	render() {
		let validationStrip = (
			<ValidationStrip
				propertyName={this.props.propertyName ?? ""}
				validationResults={this.props.validationResult}
			/>
		);
		if (this.props.label != null) {
			return (
				<div className="form-group row">
					<label className="col-sm-4 col-form-label"> {this.props.label}</label>
					<div className="col-sm-8">
						<div className="form-group">
							<div
								className="input-group date"
								id={this.inputId}
								data-target-input="nearest"
							>
								<input
									type="text"
									className={
										"form-control datetimepicker-input " +
										(ValidationHelper.getValidationResults(
											this.props.propertyName ?? "",
											this.props.validationResult
										).length > 0
											? " is-invalid text-danger"
											: "")
									}
									id={this.inputId}
									data-target={"#" + this.inputId}
									disabled={this.readOnly()}
									onBlur={this.handleInputBlur}
									placeholder={this.props.format}
								/>
								<div
									className="input-group-append"
									data-target={"#" + this.inputId}
									data-toggle="datetimepicker"
								>
									<div className="input-group-text">
										<i className="fa fa-calendar"></i>
									</div>
								</div>
							</div>
						</div>
						{validationStrip}
					</div>
				</div>
			);
		} else {
			return (
				<div className="row">
					<div className="col-sm-12">
						<div className="form-group">
							<div
								className="input-group date"
								id={this.inputId}
								data-target-input="nearest"
							>
								<input
									type="text"
									className={
										"form-control datetimepicker-input " +
										(ValidationHelper.getValidationResults(
											this.props.propertyName ?? "",
											this.props.validationResult
										).length > 0
											? " is-invalid text-danger"
											: "")
									}
									id={this.inputId}
									data-target={"#" + this.inputId}
									disabled={this.readOnly()}
									data-toggle="datetimepicker"
									onBlur={this.handleInputBlur}
									placeholder={this.props.format}
								/>
								<div
									className="input-group-append"
									data-target={"#" + this.inputId}
									data-toggle="datetimepicker"
								>
									<div className="input-group-text">
										<i className="fa fa-calendar"></i>
									</div>
								</div>
							</div>
						</div>
						{validationStrip}
					</div>
				</div>
			);
		}
	}
}
