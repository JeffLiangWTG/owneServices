import { IEntity } from "../models/IEntity";
import { shallow, mount } from "enzyme";
import React from "react";
import { DateTimeInput } from "../DateTimeInput";
import moment from "moment";

declare var global: any;

describe("DateTimeInput", () => {
	it("render", async () => {
		const div = global.document.createElement("div");
		global.document.body.appendChild(div);
		let codeList: IEntity = { ZZD_StartDate: "2019-12-31T00:00:00Z" };
		const wrapper = mount(
			<DateTimeInput
				entity={codeList}
				label="Start Date"
				propertyName="ZZD_StartDate"
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>,
			{ attachTo: div }
		);
		expect($("input[type='text']").val()).toEqual("12/31/2019 12:00 AM");
		expect(wrapper.find("input[type='text']").at(0).html()).not.toContain(
			"disable"
		);
		wrapper.detach();
		global.document.body.removeChild(div);
	});

	it("render as readonly", async () => {
		const div = global.document.createElement("div");
		global.document.body.appendChild(div);
		let codeList: IEntity = { ZZD_StartDate: "2019-12-31T00:00:00Z" };
		const wrapper = mount(
			<DateTimeInput
				entity={codeList}
				label="Start Date"
				readOnly={true}
				propertyName="ZZD_StartDate"
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>,
			{ attachTo: div }
		);
		expect(wrapper.find("input[type='text']").at(0).html()).toContain(
			"disable"
		);
		wrapper.detach();
		global.document.body.removeChild(div);
	});

	it("render as non utc", async () => {
		const div = global.document.createElement("div");
		global.document.body.appendChild(div);
		let codeList: IEntity = { ZZD_StartDate: "2019-12-31T21:00:00" };
		const wrapper = mount(
			<DateTimeInput
				useLocalTime={true}
				entity={codeList}
				label="Start Date"
				readOnly={true}
				propertyName="ZZD_StartDate"
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>,
			{ attachTo: div }
		);
		expect($("input[type='text']").val()).toEqual("12/31/2019 9:00 PM");
		wrapper.detach();
		global.document.body.removeChild(div);
	});

	it("render as date only", async () => {
		const div = global.document.createElement("div");
		global.document.body.appendChild(div);
		let codeList: IEntity = { ZZD_StartDate: "2024-12-31" };
		const wrapper = mount(
			<DateTimeInput
				entity={codeList}
				label="Start Date"
				propertyName="ZZD_StartDate"
				dateOnly={true}
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>,
			{ attachTo: div }
		);
		expect($("input[type='text']").val()).toEqual("12/31/2024 12:00 AM");
		wrapper.detach();
		global.document.body.removeChild(div);
	});

	it("format date", () => {
		let wrapper = shallow<DateTimeInput>(
			<DateTimeInput onValueChange={jest.fn} onValueChanged={jest.fn} />
		);
		let date = moment(new Date("2024-12-31 10:00:00"));
		let formattedDate = wrapper.instance().formatDate(date);
		expect(formattedDate).toEqual("2024-12-31T10:00:00Z");

		wrapper = shallow<DateTimeInput>(
			<DateTimeInput
				dateOnly={true}
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>
		);
		date = moment(new Date("2024-12-31"));
		formattedDate = wrapper.instance().formatDate(date);
		expect(formattedDate).toEqual("2024-12-31");
		date = moment(new Date("2024-12-31 10:00:00"));
		formattedDate = wrapper.instance().formatDate(date);
		expect(formattedDate).toEqual("2024-12-31");
	});

	it("parse date", () => {
		let wrapper = shallow<DateTimeInput>(
			<DateTimeInput
				dateOnly={true}
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>
		);
		let dateStr: string = "2024-12-31";
		let date = wrapper.instance().parseDate(dateStr);
		expect(date.format("YYYY-MM-DD")).toEqual(dateStr);
		dateStr = "2024-12-31T10:00:00Z";
		date = wrapper.instance().parseDate(dateStr);
		expect(date.format("YYYY-MM-DDTHH:mm:ss")).toEqual("2024-12-31T00:00:00");

		wrapper = shallow<DateTimeInput>(
			<DateTimeInput
				useLocalTime={true}
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>
		);
		dateStr = "2024-12-31T10:00:00";
		date = wrapper.instance().parseDate(dateStr);
		expect(date.format("YYYY-MM-DDTHH:mm:ss")).toEqual(dateStr);
		dateStr = "2024-12-31T10:00:00Z";
		date = wrapper.instance().parseDate(dateStr);
		expect(date.format("YYYY-MM-DDTHH:mm:ss")).toEqual(dateStr.slice(0, -1));
	});

	it("calls datetimepicker with correct date on componentDidUpdate", () => {
		const codeList1 = { ZZD_StartDate: "2024-12-31" };
		const codeList2 = { ZZD_StartDate: "2025-01-01" };

		let wrapper = shallow<DateTimeInput>(
			<DateTimeInput
				entity={codeList1}
				label="Start Date"
				propertyName="ZZD_StartDate"
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>
		);

		const datetimepickerSpy = jest.spyOn($.fn, "datetimepicker" as any);
		wrapper.setProps({ entity: codeList2 });
		expect(datetimepickerSpy).toHaveBeenCalledWith(
			"date",
			expect.anything()
		);

		datetimepickerSpy.mockRestore();
	});

	it("componentDidUpdate sets date to null when entity property is missing", () => {
		const codeList1 = { ZZD_StartDate: "2024-12-31" };
		const codeList2 = {};

		let wrapper = shallow<DateTimeInput>(
			<DateTimeInput
				entity={codeList1}
				label="Start Date"
				propertyName="ZZD_StartDate"
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>
		);

		const datetimepickerSpy = jest.spyOn($.fn, "datetimepicker" as any);
		wrapper.setProps({ entity: codeList2 });

		expect(datetimepickerSpy).toHaveBeenCalledWith("date", null);

		datetimepickerSpy.mockRestore();
	});

	it("onValueChanged", async () => {
		const div = global.document.createElement("div");
		global.document.body.appendChild(div);
		let codeList: IEntity = { ZZD_StartDate: "2019-12-31T21:00:00" };
		let mockOnValueChanged = jest.fn();
		let wrapper = shallow<DateTimeInput>(
			<DateTimeInput
				entity={codeList}
				label="Start Date"
				propertyName="ZZD_StartDate"
				onValueChange={jest.fn}
				onValueChanged={mockOnValueChanged}
			/>
		);
		wrapper.instance().onValueChanged();
		expect(mockOnValueChanged).toHaveBeenCalledWith(codeList, "ZZD_StartDate");
	});

	it("handleInputBlur calls onValueChange with formatted date for valid input", () => {
		const mockOnValueChange = jest.fn();
		const mockOnValueChanged = jest.fn();
		const entity = { ZZD_StartDate: "2024-01-01T00:00:00Z" };
		const format = "YYYY-MM-DDTHH:mm:ss[Z]";
		const wrapper = shallow<DateTimeInput>(
			<DateTimeInput
				entity={entity}
				propertyName="ZZD_StartDate"
				onValueChange={mockOnValueChange}
				onValueChanged={mockOnValueChanged}
				format={format}
			/>
		);

		const event = {
			target: { value: "2025-02-03T10:20:30Z" }
		} as React.FocusEvent<HTMLInputElement>;

		wrapper.instance().handleInputBlur(event);

		expect(mockOnValueChange).toHaveBeenCalledWith(
			entity,
			"ZZD_StartDate",
			"2025-02-03T10:20:30Z"
		);
		expect(mockOnValueChanged).toHaveBeenCalledWith(entity, "ZZD_StartDate");
	});

	it("handleInputBlur calls onValueChange with null for empty input", () => {
		const mockOnValueChange = jest.fn();
		const mockOnValueChanged = jest.fn();
		const entity = { ZZD_StartDate: "2024-01-01T00:00:00Z" };
		const format = "YYYY-MM-DDTHH:mm:ss[Z]";
		const wrapper = shallow<DateTimeInput>(
			<DateTimeInput
				entity={entity}
				propertyName="ZZD_StartDate"
				onValueChange={mockOnValueChange}
				onValueChanged={mockOnValueChanged}
				format={format}
			/>
		);

		const event = {
			target: { value: "" }
		} as React.FocusEvent<HTMLInputElement>;

		wrapper.instance().handleInputBlur(event);

		expect(mockOnValueChange).toHaveBeenCalledWith(
			entity,
			"ZZD_StartDate",
			null
		);
		expect(mockOnValueChanged).toHaveBeenCalledWith(entity, "ZZD_StartDate");
	});

	it("handleInputBlur does not call onValueChange for invalid input", () => {
		const mockOnValueChange = jest.fn();
		const mockOnValueChanged = jest.fn();
		const entity = { ZZD_StartDate: "2024-01-01T00:00:00Z" };
		const format = "YYYY-MM-DDTHH:mm:ss[Z]";
		const wrapper = shallow<DateTimeInput>(
			<DateTimeInput
				entity={entity}
				propertyName="ZZD_StartDate"
				onValueChange={mockOnValueChange}
				onValueChanged={mockOnValueChanged}
				format={format}
			/>
		);

		const event = {
			target: { value: "not-a-date" }
		} as React.FocusEvent<HTMLInputElement>;

		wrapper.instance().handleInputBlur(event);

		expect(mockOnValueChange).not.toHaveBeenCalled();
		expect(mockOnValueChanged).toHaveBeenCalledWith(entity, "ZZD_StartDate");
	});
});
