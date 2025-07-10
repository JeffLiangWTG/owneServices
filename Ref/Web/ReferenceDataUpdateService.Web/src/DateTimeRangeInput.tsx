import React, { useEffect, useState } from "react";
import uuid from "uuid";
import { IFilter } from "./Filter";
import moment from "moment";

interface DateTimeRangeProps {
	format?: string;
	filter: IFilter;

	onValueChange: (entity: any, propertyName: string, value: object) => void;
}

export const DateTimeRangeInput = ({
	format,
	filter,
	onValueChange,
}: DateTimeRangeProps) => {
	const [inputIdDateFrom] = useState(uuid.v1());
	const [inputIdDateTo] = useState(uuid.v1());

	let hasLoaded = false;
	let dateTo = "";
	let dateFrom = "";
	useEffect(() => {
		if (!hasLoaded) {
			registerDatePickerFormat();
			registerOnChangeEventsIntoDatePicker();
			onValueChange(filter, "value", "" as any);
			hasLoaded = true;
		}
	}, []);

	const registerDatePickerFormat = () => {
		($(`#${inputIdDateFrom}`) as any).datetimepicker({
			format,
		});
		($(`#${inputIdDateTo}`) as any).datetimepicker({
			format,
		});
	};

	const registerOnChangeEventsIntoDatePicker = () => {
		($(`#${inputIdDateFrom}`) as any).on(
			"change.datetimepicker",
			(e: { date: moment.Moment }) => {
				if (e.date) {
					onValueChangeDate(
						e.date.startOf("day").format("YYYY-MM-DDTHH:mm:ss") as any,
						0
					);
				}
			}
		);

		($(`#${inputIdDateTo}`) as any).on(
			"change.datetimepicker",
			(e: { date: moment.Moment }) => {
				if (e.date) {
					//format as YYYY-MM-DDTHH:mm:ss and set it to the end of day.
					onValueChangeDate(
						e.date.endOf("day").format("YYYY-MM-DDTHH:mm:ss") as any,
						1
					);
				}
			}
		);
	};

	const onValueChangeDate = (value: any, index: number) => {
		if (index == 0) {
			dateFrom = value;
		} else {
			dateTo = value;
		}
		onValueChange(filter, "value", `${dateFrom} to ${dateTo}` as any);
	};

	return (
		<div className="form-group row">
			<div className="col-sm-6">
				<div className="form-group">
					<div
						className="input-group date"
						id={inputIdDateFrom}
						data-target-input="nearest"
					>
						<input
							type="text"
							className="form-control datetimepicker-input "
							id={inputIdDateFrom}
							data-target={`#${inputIdDateFrom}`}
							data-toggle="datetimepicker"
							placeholder={format}
						/>
						<div
							className="input-group-append"
							data-target={`#${inputIdDateFrom}`}
							data-toggle="datetimepicker"
						>
							<div className="input-group-text">
								<i className="fa fa-calendar"></i>
							</div>
						</div>
					</div>
				</div>
			</div>
			<div className="col-sm-6">
				<div className="form-group">
					<div
						className="input-group date"
						id={inputIdDateTo}
						data-target-input="nearest"
					>
						<input
							type="text"
							className="form-control datetimepicker-input "
							id={inputIdDateTo}
							data-target={`#${inputIdDateTo}`}
							data-toggle="datetimepicker"
							placeholder={format}
						/>
						<div
							className="input-group-append"
							data-target={`#${inputIdDateTo}`}
							data-toggle="datetimepicker"
						>
							<div className="input-group-text">
								<i className="fa fa-calendar"></i>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	);
};
