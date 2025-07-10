import { IEntity } from "../models/IEntity";
import { shallow } from "enzyme";
import React from "react";
import { TextInput } from "../TextInput";

describe("TextInput", () => {
	it("render", async () => {
		let codeList: IEntity = { ZZD_Code: "AA" };
		let wrapper = shallow<TextInput>(<TextInput inputType='text' entity={codeList} label="Code" propertyName="ZZD_Code" onValueChange={jest.fn} onValueChanged={jest.fn} maxLength={35} />);
		expect(wrapper.find("input[type='text']").at(0).html()).toContain('value="AA"');
	});

	it("renderInt", async () => {
		let codeList: IEntity = { ZAT_RateDenominator: "11" };
		let wrapper = shallow<TextInput>(<TextInput inputType='number' entity={codeList} label="Code" propertyName="ZAT_RateDenominator" onValueChange={jest.fn} onValueChanged={jest.fn} maxLength={3} />);
		expect(wrapper.find("input[type='number']").at(0).html()).toContain('value="11"');
	});

	it("onValueChange", () => {
		let codeList: IEntity = { ZAT_RateDenominator: "11" };
		let wrapper = shallow<TextInput>(<TextInput inputType='number' entity={codeList} label="Code" propertyName="ZAT_RateDenominator" onValueChange={jest.fn} onValueChanged={jest.fn} maxLength={3} />);
		wrapper.instance().onValueChange("AA" as any);
		expect(wrapper.instance().valueChanged).toEqual(true);
	});

	it("onValueChanged", () => {
		let codeList: IEntity = { ZAT_RateDenominator: "11" };
		let wrapper = shallow<TextInput>(<TextInput inputType='number' entity={codeList} label="Code" propertyName="ZAT_RateDenominator" onValueChange={jest.fn} onValueChanged={jest.fn} maxLength={3} />);
		wrapper.instance().valueChanged = true;
		wrapper.instance().onValueChanged();
		expect(wrapper.instance().valueChanged).toEqual(false);
	});
});
