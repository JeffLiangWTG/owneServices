import React, { Component } from "react";
import { FilterOps } from "./Filter";
import { TextInput } from "./TextInput";
import { CodeInput } from "./CodeInput";
import { IFilterModule } from "./TextFilterModule";
import { IEntityManager, ServiceType } from "./EntityManager";
import { IValidationResults } from "./ValidationResults";
import { IEntity } from "./models/IEntity";
import moment from "moment";
import { DateTimeRangeInput } from "./DateTimeRangeInput";

interface IFilterStripProps {
	filterModule: IFilterModule;
	onValueChange: (object: any, propertyName: string, value: object) => void;
	onFocus?: (entity: any) => void;
	onValueChanged: (entity: any, propertyName: string) => void;
	entityManager?: IEntityManager;
	validationResults?: IValidationResults;
	prevEntity?: IEntity[];
	serviceType?: ServiceType;
}

export class FilterStrip extends Component<IFilterStripProps, any> {
	constructor(props: IFilterStripProps) {
		super(props);
		this.state = {
			dateTime1: null,
			dateTime2: null,
		};
	}

	render() {
		if (this.props.filterModule.listEntityTypeName) {
			return this.renderSelectInput();
		} else if (this.props.filterModule.filter.type == "datetime") {
			return this.renderDateTimeInput();
		} else {
			return this.renderTextInput();
		}
	}

	renderDateTimeInput() {
		const dateFormatToShow = "DD-MMM-YY";

		const dateToday = moment(new Date());
		const dateYesterday = moment(dateToday).add(-1, "days");
		const dateLastSevenDays = moment(dateToday).add(-6, "days");
		const dateLastForteenDays = moment(dateToday).add(-13, "days");
		const dateLastMonth = moment(dateToday).add(-1, "month");

		const dateTodayToShow = dateToday.format(dateFormatToShow);
		const dateYesterdayToShow = `${dateYesterday.format(
			dateFormatToShow
		)} to ${dateToday.format(dateFormatToShow)}`;
		const dateLastSevenDaysToShow = `${dateLastSevenDays.format(
			dateFormatToShow
		)} to ${dateToday.format(dateFormatToShow)}`;
		const dateLastForteenDaysToShow = `${dateLastForteenDays.format(
			dateFormatToShow
		)} to ${dateToday.format(dateFormatToShow)}`;
		const dateLastMonthToShow = `${dateLastMonth.format(
			dateFormatToShow
		)} to ${dateToday.format(dateFormatToShow)}`;

		return (
			<>
				<div className="form-group row">
					<label className="col-sm-2 col-form-label">
						{this.props.filterModule.name}
					</label>
					<div className="col-sm-2">
						<select
							className="form-control"
							value={this.props.filterModule.filter.operation}
							onChange={({ target }) => {
								this.props.onValueChange(
									this.props.filterModule.filter,
									"operation",
									parseInt(target.value) as any
								);
								if (
									this.props.filterModule.filter.operation !=
									FilterOps.DateRange
								) {
									this.props.onValueChange(
										this.props.filterModule.filter,
										"value",
										(() => {
											switch (this.props.filterModule.filter.operation) {
												case FilterOps.PlaceholderForSelection:
													return "";
												case FilterOps.DateToday:
													return dateTodayToShow;
												case FilterOps.DateYesterday:
													return dateYesterdayToShow;
												case FilterOps.DateSevenDaysAgo:
													return dateLastSevenDaysToShow;
												case FilterOps.DateForteenDaysAgo:
													return dateLastForteenDaysToShow;
												default:
													return dateLastMonthToShow as any;
											}
										})()
									);
								}
							}}
						>
							<option value={FilterOps.PlaceholderForSelection}>
								Select...
							</option>
							<option value={FilterOps.DateToday}>Today</option>
							<option value={FilterOps.DateYesterday}>Yesterday</option>
							<option value={FilterOps.DateSevenDaysAgo}>Last 7 days</option>
							<option value={FilterOps.DateForteenDaysAgo}>Last 14 days</option>
							<option value={FilterOps.DateLastMonth}>Last Month</option>
							<option value={FilterOps.DateRange}>Date Range</option>
						</select>
					</div>
					<div className="col-sm-8">
						{this.props.filterModule.filter.operation == FilterOps.DateRange ? (
							<DateTimeRangeInput
								filter={this.props.filterModule.filter}
								onValueChange={this.props.onValueChange}
								format="DD-MMM-YY"
							/>
						) : (
							<TextInput
								entity={this.props.filterModule.filter}
								propertyName="value"
								inputType="text"
								onValueChange={this.props.onValueChange}
								onValueChanged={this.props.onValueChanged}
								readOnly={true}
							/>
						)}
					</div>
				</div>
			</>
		);
	}

	renderFilterOps() {
		return (
			<div className="col-sm-2">
				<select
					aria-label={`${this.props.filterModule.name} Filter Operation`}
					className="form-control"
					value={this.props.filterModule.filter.operation as any}
					onChange={(e) =>
						this.props.onValueChange(
							this.props.filterModule.filter,
							"operation",
							parseInt(e.target.value) as any
						)
					}
				>
					<option value={FilterOps.Equals} aria-label={FilterOps[FilterOps.Equals]}>
						{FilterOps[FilterOps.Equals]}
					</option>
					<option value={FilterOps.Contains} aria-label={FilterOps[FilterOps.Contains]}>
						{FilterOps[FilterOps.Contains]}
					</option>
				</select>
			</div>
		);
	}

	renderSelectInput() {
		return (
			<div className="form-group row">
				<label className="col-sm-2 col-form-label">
					{this.props.filterModule.name}
				</label>
				{typeof this.props.filterModule.listEntityTypeName === "string" ? this.renderFilterOps() : ""}
				<div className="col-sm-2">
					<CodeInput
						propertyName="value"
						entity={this.props.filterModule.filter}
						onValueChange={this.props.onValueChange}
						maxLength={this.props.filterModule.maxLength}
						listCodePropertyName={
							this.props.filterModule.listCodePropertyName!!
						}
						listDescriptionPropertyName={
							this.props.filterModule.listDescriptionPropertyName!!
						}
						listEntityTypeName={this.props.filterModule.listEntityTypeName!!}
						entityManager={this.props.entityManager!!}
						validationResults={this.props.validationResults}
						prevEntity={this.props.prevEntity}
						onFocusing={this.props.onFocus}
						onValueChanged={this.props.onValueChanged}
						serviceType={
							this.props.serviceType ? this.props.serviceType : ServiceType.Safe
						}
						ariaLabel={`${this.props.filterModule.name} Filter Value`}
					/>
				</div>
			</div>
		);
	}

	renderTextInput() {
		return (
			<div className="form-group row">
				<label className="col-sm-2 col-form-label">
					{this.props.filterModule.name}
				</label>
				{this.renderFilterOps()}
				<div className="col-sm-8">
					<TextInput
						ariaLabel={`${this.props.filterModule.name} Filter Value`}
						inputType="text"
						propertyName="value"
						entity={this.props.filterModule.filter}
						onValueChange={this.props.onValueChange}
						onValueChanged={this.props.onValueChanged}
						maxLength={this.props.filterModule.maxLength}
						validationResults={this.props.validationResults}
					/>
				</div>
			</div>
		);
	}
}
