import { shallow } from "enzyme";
import { FilterStrip } from "../FilterStrip";
import React from "react";
import { Filter, FilterOps } from "../Filter";
import { TextFilterModule } from "../TextFilterModule";
import { TextInput } from "../TextInput";
import { CodeInput } from "../CodeInput";
import { ServiceType } from "../EntityManager";
import { DateTimeRangeInput } from "../DateTimeRangeInput";

describe("<FilterStrip />", () => {
	it("render text input", () => {
		let filterModule = new TextFilterModule(
			"Code",
			new Filter("ZZD_Code", FilterOps.Equals, "AA" as any, "string"),
			35
		);
		const wrapper = shallow(
			<FilterStrip
				filterModule={filterModule}
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>
		);
		expect(wrapper.find(TextInput).length).toEqual(1);
	});

	it("renderSelectInput_NoFilterOps", () => {
		let options = [
			{ Status_Value: "ERR", Status_Description: "Error" },
			{ Status_Value: "PRS", Status_Description: "Success" }
		];
		let filterModule = new TextFilterModule(
			"Status",
			new Filter("PRC_Status", FilterOps.Equals, "" as any, "string"),
			35,
			"Status_Value",
			"Status_Description",
			options
		);
		const wrapper = shallow(
			<FilterStrip
				filterModule={filterModule}
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>
		);
		expect(wrapper.find("select").length).toEqual(0);
		expect(wrapper.find(CodeInput).length).toEqual(1);
	});

	it("render select input", () => {
		let filterModule = new TextFilterModule(
			"Code",
			new Filter("ZZD_CodeType", FilterOps.Equals, "AA" as any, "string"),
			35,
			"ZZK_CodeType",
			"ZZK_Description",
			"RefCusCodeType"
		);
		const wrapper = shallow(
			<FilterStrip
				filterModule={filterModule}
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>
		);
		expect(wrapper.find(CodeInput).length).toEqual(1);
	});

	it("render datetime range input", () => {
		let filterModule = new TextFilterModule(
			"Start Time",
			new Filter(
				"SDA_CreatedTime",
				FilterOps.DateRange,
				"2020-01-01 00:00:00 to 2021-01-01 10:00:00" as any,
				"datetime"
			),
			35
		);
		const wrapper = shallow(
			<FilterStrip
				filterModule={filterModule}
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>
		);
		expect(wrapper.find(DateTimeRangeInput).length).toEqual(1);
		expect(wrapper.find("select").at(0).prop("value")).toEqual(
			FilterOps.DateRange
		);
		expect(wrapper.find("select").at(0).find("option")).toHaveLength(7);
		expect(wrapper.find("select").at(0).find("option").at(0).html()).toEqual(
			`<option value="${FilterOps.PlaceholderForSelection}">Select...</option>`
		);
		expect(wrapper.find("select").at(0).find("option").at(1).html()).toEqual(
			`<option value="${FilterOps.DateToday}">Today</option>`
		);
		expect(wrapper.find("select").at(0).find("option").at(2).html()).toEqual(
			`<option value="${FilterOps.DateYesterday}">Yesterday</option>`
		);
		expect(wrapper.find("select").at(0).find("option").at(3).html()).toEqual(
			`<option value="${FilterOps.DateSevenDaysAgo}">Last 7 days</option>`
		);
		expect(wrapper.find("select").at(0).find("option").at(4).html()).toEqual(
			`<option value="${FilterOps.DateForteenDaysAgo}">Last 14 days</option>`
		);
		expect(wrapper.find("select").at(0).find("option").at(5).html()).toEqual(
			`<option value="${FilterOps.DateLastMonth}">Last Month</option>`
		);
		expect(wrapper.find("select").at(0).find("option").at(6).html()).toEqual(
			`<option value="${FilterOps.DateRange}">Date Range</option>`
		);
	});

	it("render operation selector", () => {
		let filterModule = new TextFilterModule(
			"Code",
			new Filter("ZZD_Code", FilterOps.Equals, "AA" as any, "string"),
			35
		);
		const handleOnChange = jest.fn();
		const wrapper = shallow(
			<FilterStrip
				filterModule={filterModule}
				onValueChange={handleOnChange}
				onValueChanged={jest.fn}
			/>
		);
		expect(wrapper.find("select").at(0).prop("value")).toEqual(
			FilterOps.Equals
		);
		expect(wrapper.find("select").at(0).find("option")).toHaveLength(2);
		expect(wrapper.find("select").at(0).find("option").at(0).html()).toEqual(
			'<option value="0" aria-label="Equals">Equals</option>'
		);
		expect(wrapper.find("select").at(0).find("option").at(1).html()).toEqual(
			'<option value="1" aria-label="Contains">Contains</option>'
		);
	});

	it("parsing target value", () => {
		let filterModule = new TextFilterModule(
			"Code",
			new Filter("ZZD_Code", FilterOps.Equals, "AA" as any, "string"),
			35
		);
		const handleOnChange = jest.fn();
		const wrapper = shallow(
			<FilterStrip
				filterModule={filterModule}
				onValueChange={handleOnChange}
				onValueChanged={jest.fn}
			/>
		);
		wrapper
			.find("select")
			.at(0)
			.simulate("change", { target: { value: "1" } });
		expect(handleOnChange).toHaveBeenCalledWith(
			filterModule.filter,
			"operation",
			1
		);
	});

	it("parsing service type", () => {
		let filterModule = new TextFilterModule(
			"Code",
			new Filter("ZZD_CodeType", FilterOps.Equals, "AA" as any, "string"),
			35,
			"ZZK_CodeType",
			"ZZK_Description",
			"RefCusCodeType"
		);
		const wrapper1 = shallow(
			<FilterStrip
				filterModule={filterModule}
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>
		);
		expect(wrapper1.find(CodeInput).at(0).props().serviceType).toEqual(
			ServiceType.Safe
		);

		const wrapper2 = shallow(
			<FilterStrip
				filterModule={filterModule}
				serviceType={ServiceType.Staging}
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
			/>
		);
		expect(wrapper2.find(CodeInput).at(0).props().serviceType).toEqual(
			ServiceType.Staging
		);
	});
});
