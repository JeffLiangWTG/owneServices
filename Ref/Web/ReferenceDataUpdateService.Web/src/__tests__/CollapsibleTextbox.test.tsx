import { IEntity } from "../models/IEntity";
import { shallow } from "enzyme";
import React from "react";
import { CollapsibleTextbox } from "../CollapsibleTextbox";

describe("CollapsibleTextbox", () => {
	it("render with label", async () => {
		let codeList: IEntity = { ZZD_Description: "Hello" };
		let wrapper = shallow(
			<CollapsibleTextbox
				entity={codeList}
				label="Description"
				propertyName="ZZD_Description"
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
				maxLength={2000}
			/>
		);

		expect(wrapper.find("label").at(0).text()).toEqual("Description");
	});

	it("render without label", async () => {
		let codeList: IEntity = { ZZD_Description: "Hello" };
		let wrapper = shallow(
			<CollapsibleTextbox
				entity={codeList}
				propertyName="ZZD_Description"
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
				maxLength={2000}
			/>
		);

		expect(wrapper.find("label").length).toEqual(0);
	});

	it("render when length < 100", async () => {
		let codeList: IEntity = { ZZD_Description: "Hello" };
		let wrapper = shallow(
			<CollapsibleTextbox
				entity={codeList}
				label="Description"
				propertyName="ZZD_Description"
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
				maxLength={2000}
			/>
		);

		expect(wrapper.find("textarea").at(0).props().value).toEqual("Hello");
	});

	it("render when length > 100", async () => {
		let codeList: IEntity = { ZZD_Description: description };
		let wrapper = shallow(
			<CollapsibleTextbox
				entity={codeList}
				label="Description"
				propertyName="ZZD_Description"
				onValueChange={jest.fn}
				onValueChanged={jest.fn}
				maxLength={2000}
			/>
		);
		expect(wrapper.find("textarea").props().value).toEqual(
			description.substring(0, 100) + " ..."
		);

		wrapper.find("textarea").simulate("focus");
		expect(wrapper.find("textarea").props().value).toEqual(description);

		wrapper.find("textarea").simulate("blur");
		expect(wrapper.find("textarea").props().value).toEqual(
			description.substring(0, 100) + " ..."
		);
	});

	it("onValueChange when length < 100", () => {
		let codeList: IEntity = { ZZD_Description: "Hello" };
		let onValueChangeMock = jest.fn();
		let wrapper = shallow(
			<CollapsibleTextbox
				entity={codeList}
				label="Description"
				propertyName="ZZD_Description"
				onValueChange={onValueChangeMock}
				onValueChanged={jest.fn}
				maxLength={2000}
			/>
		);

		wrapper.find("textarea").simulate("change", { target: { value: "AA" } });
		expect(onValueChangeMock).toHaveBeenCalledWith(
			codeList,
			"ZZD_Description",
			"AA"
		);
	});

	it("onValueChange when length > 100", () => {
		let codeList: IEntity = {
			ZZD_Description: description,
		};
		let onValueChangeMock = jest.fn();
		let wrapper = shallow(
			<CollapsibleTextbox
				entity={codeList}
				label="Description"
				propertyName="ZZD_Description"
				onValueChange={onValueChangeMock}
				onValueChanged={jest.fn}
				maxLength={2000}
			/>
		);

		wrapper.find("textarea").simulate("focus");
		wrapper.find("textarea").simulate("change", {
			target: {
				value: description + " New",
			},
		});
		expect(onValueChangeMock).toHaveBeenCalledWith(
			codeList,
			"ZZD_Description",
			description + " New"
		);
	});

	it("onValueChanged when length < 100", () => {
		let codeList: IEntity = { ZZD_Description: "Hello" };
		let onValueChangedMock = jest.fn();
		let wrapper = shallow(
			<CollapsibleTextbox
				entity={codeList}
				label="Description"
				propertyName="ZZD_Description"
				onValueChange={jest.fn()}
				onValueChanged={onValueChangedMock}
				maxLength={2000}
			/>
		);
		wrapper.find("textarea").simulate("change", { target: { value: "AA" } });
		wrapper.find("textarea").simulate("blur");
		expect(onValueChangedMock).toHaveBeenCalledWith(
			codeList,
			"ZZD_Description"
		);
	});
	it("onValueChanged when length > 100", () => {
		let codeList: IEntity = {
			ZZD_Description: description,
		};
		let onValueChangedMock = jest.fn();
		let wrapper = shallow(
			<CollapsibleTextbox
				entity={codeList}
				label="Description"
				propertyName="ZZD_Description"
				onValueChange={jest.fn()}
				onValueChanged={onValueChangedMock}
				maxLength={2000}
			/>
		);
		wrapper.find("textarea").simulate("focus");
		wrapper.find("textarea").simulate("change", {
			target: {
				value: description + " New",
			},
		});
		wrapper.find("textarea").simulate("blur");
		expect(onValueChangedMock).toHaveBeenCalledWith(
			codeList,
			"ZZD_Description"
		);
	});

	const description: string =
		"When cursor is moved out of this field it is collapsed into one-liner with a special symbol indicating there are more characters than currently visible";
});
