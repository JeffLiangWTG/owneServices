import { mount } from "enzyme";
import { RefShippingLineEBLProviders } from "../RefShippingLineEBLProviders";
import React from "react";
import uuid from "uuid";
import { RefShippingLineEBLProviderItem } from "../RefShippingLineEBLProviderItem";
import { CheckBox } from "../CheckBox";
import { RefShippingLineEBLProviderWrapper } from "../models/IRefShippingLineEBLProvider";

describe("RefShippingLineEBLProviders", () => {
	it("renders components with no items", () => {
		const wrapper = mount<typeof RefShippingLineEBLProviders>(
			<RefShippingLineEBLProviders
				onValueChange={jest.fn()}
				onValueChanged={jest.fn()}
				providerItems={[]}
				handleAddEBLProviderItem={jest.fn()}
				handleDeleteEBLProviderItem={jest.fn()}
				isAddButtonDisabled={false}
				eblProviderDistinctNames={[]}
			/>
		);

		//div as columns
		expect(wrapper.find("div div span").at(0).html()).toContain("eBL Provider");
		expect(wrapper.find("div div span").at(1).html()).toContain("Is Available");
		expect(wrapper.find("div div span").at(2).html()).toContain("Default");

		//add button
		expect(
			wrapper.find("#refShippingLineEBLProviderItemAdd").exists()
		).toBeTruthy();

		//no eBL Provider items found
		expect(wrapper.find(RefShippingLineEBLProviderItem).exists()).toBeFalsy();
		//since there are no items, minus button can't be find
		expect(
			wrapper
				.find("input[type='button']")
				.filterWhere((x) => x.prop("value") == "-")
				.exists()
		).toBeFalsy();
	});

	it("renders provider items and minus button", () => {
		const wrapper = mount<typeof RefShippingLineEBLProviders>(
			<RefShippingLineEBLProviders
				onValueChange={jest.fn()}
				onValueChanged={jest.fn()}
				providerItems={[
					new RefShippingLineEBLProviderWrapper(
						uuid(),
						"any",
						"prov1",
						true,
						false
					),
				]}
				handleAddEBLProviderItem={jest.fn()}
				handleDeleteEBLProviderItem={jest.fn()}
				isAddButtonDisabled={false}
				eblProviderDistinctNames={[]}
			/>
		);

		//no eBL Provider items found
		expect(wrapper.find(RefShippingLineEBLProviderItem).exists()).toBeTruthy();
		//since there are items, minus button can be find
		expect(
			wrapper
				.find("input[type='button']")
				.filterWhere((x) => x.prop("value") == "-")
				.exists()
		).toBeTruthy();
	});

	it("renders with disabled add button", async () => {
		const wrapper = mount<typeof RefShippingLineEBLProviders>(
			<RefShippingLineEBLProviders
				onValueChange={jest.fn()}
				onValueChanged={jest.fn()}
				providerItems={[
					new RefShippingLineEBLProviderWrapper(
						uuid(),
						"any",
						"prov1",
						true,
						false
					),
				]}
				handleAddEBLProviderItem={jest.fn()}
				handleDeleteEBLProviderItem={jest.fn()}
				isAddButtonDisabled={true}
				eblProviderDistinctNames={[]}
			/>
		);

		//add button is disabled
		expect(
			wrapper.find("#refShippingLineEBLProviderItemAdd").prop("disabled")
		).toBeTruthy();
		//add button has "title"
		expect(
			wrapper.find("#refShippingLineEBLProviderItemAdd").prop("title")
		).toBe(
			"Requires '(BPL) Electronic Bill of Lading Provider Mandatory' for 'Shipping Instructions' to be selected in 'Messaging Requirements' section."
		);
	});

	it("onChange IsAvailable is tick, IsDefault is shown", () => {
		const providerItem = new RefShippingLineEBLProviderWrapper(
			uuid(),
			"any",
			"prov1",
			false,
			false
		);

		const wrapper = mount<typeof RefShippingLineEBLProviders>(
			<RefShippingLineEBLProviders
				onValueChange={jest.fn()}
				onValueChanged={jest.fn()}
				providerItems={[providerItem]}
				handleAddEBLProviderItem={jest.fn()}
				handleDeleteEBLProviderItem={jest.fn()}
				isAddButtonDisabled={false}
				eblProviderDistinctNames={[]}
			/>
		);

		//check for IsDefaultHidden prop in the provider item, should be true.
		expect(
			(wrapper.prop("providerItems") as RefShippingLineEBLProviderWrapper[]).at(
				0
			)?.IsDefaultHidden
		).toBeTruthy();

		//tick IsAvailable
		wrapper
			.find(RefShippingLineEBLProviderItem)
			.at(0)
			.find(CheckBox)
			.at(0)
			.prop("onValueChange")!(providerItem, "RSE_IsAvailable", true as any);

		wrapper.update();
		//check for IsDefaultHidden prop in the provider item, should be false.
		expect(
			(wrapper.prop("providerItems") as RefShippingLineEBLProviderWrapper[]).at(
				0
			)?.IsDefaultHidden
		).toBeFalsy();
	});
	it("onChange IsAvailable is untick, IsDefault is hidden and untick", () => {
		const providerItem = new RefShippingLineEBLProviderWrapper(
			uuid(),
			"any",
			"prov1",
			true,
			false
		);

		const wrapper = mount<typeof RefShippingLineEBLProviders>(
			<RefShippingLineEBLProviders
				onValueChange={jest.fn()}
				onValueChanged={jest.fn()}
				providerItems={[providerItem]}
				handleAddEBLProviderItem={jest.fn()}
				handleDeleteEBLProviderItem={jest.fn()}
				isAddButtonDisabled={false}
				eblProviderDistinctNames={[]}
			/>
		);

		//check for IsDefaultHidden prop in the provider item, should be false.
		expect(
			(wrapper.prop("providerItems") as RefShippingLineEBLProviderWrapper[]).at(
				0
			)?.IsDefaultHidden
		).toBeFalsy();

		//untick IsAvailable
		wrapper
			.find(RefShippingLineEBLProviderItem)
			.at(0)
			.find(CheckBox)
			.at(0)
			.prop("onValueChange")!(providerItem, "RSE_IsAvailable", false as any);

		wrapper.update();
		//check for IsDefaultHidden prop in the provider item, should be true.
		expect(
			(wrapper.prop("providerItems") as RefShippingLineEBLProviderWrapper[]).at(
				0
			)?.IsDefaultHidden
		).toBeTruthy();
	});
	it("onChange IsDefault is tick, other IsDefault is hidden", () => {
		const providerItemAvailable1 = new RefShippingLineEBLProviderWrapper(
			uuid(),
			"any",
			"prov1",
			true,
			false
		);
		const providerItemAvailable2 = new RefShippingLineEBLProviderWrapper(
			uuid(),
			"any",
			"prov1",
			true,
			false
		);
		const providerItemAvailable3 = new RefShippingLineEBLProviderWrapper(
			uuid(),
			"any",
			"prov1",
			true,
			false
		);

		const wrapper = mount<typeof RefShippingLineEBLProviders>(
			<RefShippingLineEBLProviders
				onValueChange={jest.fn()}
				onValueChanged={jest.fn()}
				providerItems={[
					providerItemAvailable1,
					providerItemAvailable2,
					providerItemAvailable3,
				]}
				handleAddEBLProviderItem={jest.fn()}
				handleDeleteEBLProviderItem={jest.fn()}
				isAddButtonDisabled={false}
				eblProviderDistinctNames={[]}
			/>
		);

		//check for IsDefaultHidden prop in the provider items, should be false.
		expect(
			(wrapper.prop("providerItems") as RefShippingLineEBLProviderWrapper[]).at(
				0
			)?.IsDefaultHidden
		).toBeFalsy();
		expect(
			(wrapper.prop("providerItems") as RefShippingLineEBLProviderWrapper[]).at(
				1
			)?.IsDefaultHidden
		).toBeFalsy();
		expect(
			(wrapper.prop("providerItems") as RefShippingLineEBLProviderWrapper[]).at(
				2
			)?.IsDefaultHidden
		).toBeFalsy();

		//tick IsDefault on provider item 2
		wrapper
			.find(RefShippingLineEBLProviderItem)
			.at(0)
			.find(CheckBox)
			.at(0)
			.prop("onValueChange")!(
			providerItemAvailable2,
			"RSE_IsDefault",
			true as any
		);

		wrapper.update();
		//check for IsDefaultHidden prop in the provider items 1 and 3, should be true.
		expect(
			(wrapper.prop("providerItems") as RefShippingLineEBLProviderWrapper[]).at(
				0
			)?.IsDefaultHidden
		).toBeTruthy();
		expect(
			(wrapper.prop("providerItems") as RefShippingLineEBLProviderWrapper[]).at(
				2
			)?.IsDefaultHidden
		).toBeTruthy();
	});
	it("onChange IsDefault is untick, other IsDefault is shown", () => {
		const providerItemAvailable1 = new RefShippingLineEBLProviderWrapper(
			uuid(),
			"any",
			"prov1",
			true,
			false
		);
		const providerItemAvailable2 = new RefShippingLineEBLProviderWrapper(
			uuid(),
			"any",
			"prov1",
			true,
			true
		);
		const providerItemAvailable3 = new RefShippingLineEBLProviderWrapper(
			uuid(),
			"any",
			"prov1",
			true,
			false
		);

		const wrapper = mount<typeof RefShippingLineEBLProviders>(
			<RefShippingLineEBLProviders
				onValueChange={jest.fn()}
				onValueChanged={jest.fn()}
				providerItems={[
					providerItemAvailable1,
					providerItemAvailable2,
					providerItemAvailable3,
				]}
				handleAddEBLProviderItem={jest.fn()}
				handleDeleteEBLProviderItem={jest.fn()}
				isAddButtonDisabled={false}
				eblProviderDistinctNames={[]}
			/>
		);

		//untick IsDefault on provider item 2
		wrapper
			.find(RefShippingLineEBLProviderItem)
			.at(0)
			.find(CheckBox)
			.at(0)
			.prop("onValueChange")!(
			providerItemAvailable2,
			"RSE_IsDefault",
			false as any
		);

		wrapper.update();
		//check for IsDefaultHidden prop in the provider items, should be false.
		expect(
			(wrapper.prop("providerItems") as RefShippingLineEBLProviderWrapper[]).at(
				0
			)?.IsDefaultHidden
		).toBeFalsy();
		expect(
			(wrapper.prop("providerItems") as RefShippingLineEBLProviderWrapper[]).at(
				1
			)?.IsDefaultHidden
		).toBeFalsy();
		expect(
			(wrapper.prop("providerItems") as RefShippingLineEBLProviderWrapper[]).at(
				2
			)?.IsDefaultHidden
		).toBeFalsy();
	});

	it("IsDefault item is deleted, all other records which are available have their IsDefault shown", () => {
		const providerItemAvailable1 = new RefShippingLineEBLProviderWrapper(
			uuid(),
			"any",
			"prov1",
			true,
			false
		);
		const providerItemAvailable2 = new RefShippingLineEBLProviderWrapper(
			uuid(),
			"any",
			"prov1",
			true,
			true
		);
		const providerItemAvailable3 = new RefShippingLineEBLProviderWrapper(
			uuid(),
			"any",
			"prov1",
			true,
			false
		);
		const wrapper = mount<typeof RefShippingLineEBLProviders>(
			<RefShippingLineEBLProviders
				onValueChange={jest.fn()}
				onValueChanged={jest.fn()}
				providerItems={[
					providerItemAvailable1,
					providerItemAvailable2,
					providerItemAvailable3,
				]}
				handleAddEBLProviderItem={jest.fn()}
				handleDeleteEBLProviderItem={jest.fn()}
				isAddButtonDisabled={false}
				eblProviderDistinctNames={[]}
			/>
		);

		//press "-" button and delete the default record.
		wrapper
			.find("input[type='button']")
			.filterWhere((x) => x.prop("value") == "-")
			.at(1)
			.simulate("click");

		wrapper.update();

		//check for IsDefaultHidden prop in the provider items, should be false.
		//reminder that this "deletion" doesn't actually delete the item, so we would still consider three items.
		expect(
			(wrapper.prop("providerItems") as RefShippingLineEBLProviderWrapper[]).at(
				0
			)?.IsDefaultHidden
		).toBeFalsy();
		expect(
			(wrapper.prop("providerItems") as RefShippingLineEBLProviderWrapper[]).at(
				2
			)?.IsDefaultHidden
		).toBeFalsy();
	});
});
