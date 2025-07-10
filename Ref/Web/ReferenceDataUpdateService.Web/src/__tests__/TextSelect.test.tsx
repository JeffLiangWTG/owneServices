import { shallow } from "enzyme";
import React, { ChangeEvent } from "react";
import { Mock } from "typemoq";
import { IEntity } from "../models/IEntity";
import { TextSelect } from "../TextSelect";

describe("TextSelect", () => {
    it("render", () => {
        var testEntity: IEntity = { Property_Name: "N" };
        let wrapper = shallow<TextSelect>(<TextSelect entity={testEntity} label="Property Name" propertyName="Property_Name" onValueChange={jest.fn} onValueChanged={jest.fn} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} />);
        expect(wrapper.find("select").at(0).childAt(1).html()).toContain('value="N"');
        expect(wrapper.find("select").at(0).childAt(1).html()).toContain('No');
    });

    it("onValueChange", () => {
        var testEntity: IEntity = { Property_Name: "N" };
        let wrapper = shallow<TextSelect>(<TextSelect entity={testEntity} label="Property Name" propertyName="Property_Name" onValueChange={jest.fn} onValueChanged={jest.fn} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} />);
		wrapper.instance().onValueChange("I" as any);
		expect(wrapper.instance().valueChanged).toEqual(true);
    });

    it("onValueChanged", () => {
        var testEntity: IEntity = { Property_Name: "N" };
        let wrapper = shallow<TextSelect>(<TextSelect entity={testEntity} label="Property Name" propertyName="Property_Name" onValueChange={jest.fn} onValueChanged={jest.fn} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} />);
        wrapper.instance().valueChanged = true;
        wrapper.instance().onValueChanged();
        expect(wrapper.instance().valueChanged).toEqual(false);
    });
});