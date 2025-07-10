import { shallow } from "enzyme";
import React, { ChangeEvent } from "react";
import { Mock } from "typemoq";
import { IEntity } from "../models/IEntity";
import { ComboBox } from "../ComboBox";
import { ProducerStatus } from "../ProducerStatus";

describe("ComboBox", () => {
    it("render", () => {
		var testEntity: IEntity = { ErrorReportingCode: "7" };
		let wrapper = shallow<ComboBox>(<ComboBox entity={testEntity} label="Error Reporting Code" propertyName="ErrorReportingCode" onValueChange={jest.fn} onValueChanged={jest.fn} options={new ProducerStatus()} />);
        expect(wrapper.find("input").at(0).html()).toContain('value="7"');
		expect(wrapper.find("input").at(1).html()).toContain('type="checkbox"');
		expect(wrapper.find("label").at(1).html()).toContain('Failure');
		expect(wrapper.find("input").at(2).html()).toContain('type="checkbox"');
		expect(wrapper.find("label").at(2).html()).toContain('ParseFailure');
		expect(wrapper.find("input").at(3).html()).toContain('type="checkbox"');
		expect(wrapper.find("label").at(3).html()).toContain('MergeFailure');
    });

    it("onValueChange", () => {
        var testEntity: IEntity = { Property_Name: "7" };
		let wrapper = shallow<ComboBox>(<ComboBox entity={testEntity} label="Error Reporting Code" propertyName="ErrorReportingCode" onValueChange={jest.fn} onValueChanged={jest.fn} options={new ProducerStatus()} />);
		wrapper.instance().onValueChange("5" as any);
		expect(wrapper.instance().valueChanged).toEqual(true);
    });

    it("onValueChanged", () => {
        var testEntity: IEntity = { Property_Name: "7" };
		let wrapper = shallow<ComboBox>(<ComboBox entity={testEntity} label="Error Reporting Code" propertyName="ErrorReportingCode" onValueChange={jest.fn} onValueChanged={jest.fn} options={new ProducerStatus()} />);
        wrapper.instance().valueChanged = true;
        wrapper.instance().onValueChanged();
        expect(wrapper.instance().valueChanged).toEqual(false);
    });
});
