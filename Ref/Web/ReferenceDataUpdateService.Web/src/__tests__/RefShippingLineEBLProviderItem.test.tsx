import { shallow } from "enzyme";
import { RefShippingLineEBLProviderItem } from "../RefShippingLineEBLProviderItem";
import React from "react";
import { RefShippingLineEBLProviderWrapper } from "../models/IRefShippingLineEBLProvider";
import uuid from "uuid";
import { CheckBox } from "../CheckBox";
import { CodeInput } from "../CodeInput";

describe("RefShippingLineEBLProviderItem", () => {
	it("render components and assign properties", () => {
		const providerItem = new RefShippingLineEBLProviderWrapper(
			uuid(),
			"any",
			"prov1",
			true,
			false
		);

		const wrapper = shallow<typeof RefShippingLineEBLProviderItem>(
			<RefShippingLineEBLProviderItem
				onValueChange={jest.fn()}
				onValueChanged={jest.fn()}
				handleDeleteEBLProviderItem={jest.fn()}
				providerItem={providerItem}
				eblProviderDistinctNames={[]}
			/>
		);

		//components exist.
		expect(wrapper.find(CodeInput).at(0).exists()).toBeTruthy();
		expect(wrapper.find(CheckBox).at(0).exists()).toBeTruthy();
		expect(wrapper.find(CheckBox).at(1).exists()).toBeTruthy();

		//properties are correctly assigned.
		expect(wrapper.find(CodeInput).at(0).props().propertyName).toBe("RSE_Name");
		expect(wrapper.find(CheckBox).at(0).props().propertyName).toBe(
			"RSE_IsAvailable"
		);
		expect(wrapper.find(CheckBox).at(1).props().propertyName).toBe(
			"RSE_IsDefault"
		);

		//minus button
		expect(
			wrapper
				.find("input[type='button']")
				.filterWhere((x) => x.prop("value") == "-")
				.exists()
		).toBeTruthy();
	});

	it("render with hidden default checkbox", () => {
		const providerItem = new RefShippingLineEBLProviderWrapper(
			uuid(),
			"any",
			"prov1",
			false,
			false
		);
		const wrapper = shallow<typeof RefShippingLineEBLProviderItem>(
			<RefShippingLineEBLProviderItem
				onValueChange={jest.fn()}
				onValueChanged={jest.fn()}
				handleDeleteEBLProviderItem={jest.fn()}
				providerItem={providerItem}
				eblProviderDistinctNames={[]}
			/>
		);

		expect(wrapper.find(CheckBox).at(1).props().isHidden).toBeTruthy();
	});
});
