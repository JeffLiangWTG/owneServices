import { IEntity } from "../models/IEntity";
import { shallow } from "enzyme";
import React from "react";
import { TextArea } from "../TextArea";

describe("TextArea", () => {
	it("render", async () => {
		let codeList: IEntity = { ZZD_Description: "Hello" };
		let wrapper = shallow<TextArea>(<TextArea entity={codeList} label="Description" propertyName="ZZD_Description" onValueChange={jest.fn} onValueChanged={jest.fn} maxLength={2000} />);
		expect(wrapper.find("textarea").at(0).html()).toContain("Hello");
	});

	it("onValueChange", () => {
		let codeList: IEntity = { ZZD_Description: "Hello" };
		let wrapper = shallow<TextArea>(<TextArea entity={codeList} label="Description" propertyName="ZZD_Description" onValueChange={jest.fn} onValueChanged={jest.fn} maxLength={2000} />);
		wrapper.instance().onValueChange("AA" as any);
		expect(wrapper.instance().valueChanged).toEqual(true);
	});

	it("onValueChanged", () => {
		let codeList: IEntity = { ZZD_Description: "Hello" };
		let wrapper = shallow<TextArea>(<TextArea entity={codeList} label="Description" propertyName="ZZD_Description" onValueChange={jest.fn} onValueChanged={jest.fn} maxLength={2000} />);
		wrapper.instance().valueChanged = true;
		wrapper.instance().onValueChanged();
		expect(wrapper.instance().valueChanged).toEqual(false);
	});
});
