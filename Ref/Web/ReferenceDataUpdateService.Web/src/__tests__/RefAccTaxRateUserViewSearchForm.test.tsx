import { IEntityManager, ServiceType } from "../EntityManager";
import { mount, ReactWrapper, shallow } from "enzyme";
import React from "react";
import RefAccTaxRateUserViewSearchForm from "../RefAccTaxRateUserViewSearchForm";
import { Mock, It, IMock } from "typemoq";
import { FilterStrip } from "../FilterStrip";
import { IEntity } from "../models/IEntity";
import { CodeInput } from "../CodeInput";
import { DateTimeInput } from "../DateTimeInput";
import { RefAccTaxRateUserView } from "../models/RefAccTaxRateUserView";
import { act } from "react-dom/test-utils";
import { setImmediate } from "timers";
import { TextInput } from "../TextInput";
import Button from "../Button";

const pressAdd = async (wrapper: ReactWrapper) => {
	//press Add
	wrapper
		.findWhere((x) => x.is(Button) && x.text() === "Add")
		.simulate("mousedown", { button: 0 });

	await act(() => new Promise(setImmediate));
	wrapper.update();
};

const pressFind = async (wrapper: ReactWrapper) => {
	//press find
	wrapper
		.findWhere((x) => x.is(Button) && x.text() === "Find")
		.simulate("mousedown", { button: 0 });
	await act(() => new Promise(setImmediate));
	wrapper.update();
};

const pressEdit = async (wrapper: ReactWrapper, index: number) => {
	//Press edit
	wrapper
		.findWhere((x) => x.is(Button) && x.text() === "Edit")
		.at(index)
		.simulate("mousedown", { button: 0 });
	await act(() => new Promise(setImmediate));
	wrapper.update();
};

const pressCancel = async (wrapper: ReactWrapper, index: number) => {
	//Press Cancel
	wrapper
		.findWhere((x) => x.is(Button) && x.text() === "Cancel")
		.at(index)
		.simulate("mousedown", { button: 0 });
	await act(() => new Promise(setImmediate));
	wrapper.update();
};

const pressCancelAll = async (wrapper: ReactWrapper, index: number) => {
	//Press Cancel All
	wrapper
		.findWhere((x) => x.is(Button) && x.text() === "Cancel All")
		.at(index)
		.simulate("mousedown", { button: 0 });
	await act(() => new Promise(setImmediate));
	wrapper.update();
};

describe("<RefAccTaxRateUserViewSearchForm />", () => {
	let code1: IMock<RefAccTaxRateUserView>;
	let code2: IMock<RefAccTaxRateUserView>;
	let codeCountry: IMock<IEntity>;
	let entityManager: IMock<IEntityManager>;

	beforeEach(() => {
		code1 = Mock.ofType<RefAccTaxRateUserView>();
		code1.setup((x) => x.ZAT_PK).returns(() => "1");
		code1.setup((x) => x.ZAT_ReferenceRateType).returns(() => "STD");
		code1.setup((x) => x.ZAT_RN_NKCountry).returns(() => "RE");
		code1.setup((x) => x.ZAT_RateDenominator).returns(() => 1);
		code1.setup((x) => x.ZAT_RateNumerator).returns(() => 21);
		code1.setup((x) => x.ZAT_IsSystem).returns(() => true);
		code1.setup((x) => x.ZAT_IsPublished).returns(() => true);
		code1.setup((x) => x.ZAT_StartDate).returns(() => "1900-01-01");
		code1.setup((x) => x.ZAT_EndDate).returns(() => "2079-06-06");

		code2 = Mock.ofType<RefAccTaxRateUserView>();
		code2.setup((x) => x.ZAT_PK).returns(() => "2");
		code2.setup((x) => x.ZAT_ReferenceRateType).returns(() => "LOW");
		code2.setup((x) => x.ZAT_RN_NKCountry).returns(() => "BJ");
		code2.setup((x) => x.ZAT_RateDenominator).returns(() => 10);
		code2.setup((x) => x.ZAT_RateNumerator).returns(() => 19);
		code2.setup((x) => x.ZAT_IsSystem).returns(() => true);
		code2.setup((x) => x.ZAT_IsPublished).returns(() => true);
		code2.setup((x) => x.ZAT_StartDate).returns(() => "1900-01-01");
		code2.setup((x) => x.ZAT_EndDate).returns(() => "2079-06-06");

		codeCountry = Mock.ofType<IEntity>();
		codeCountry.setup((x) => x.RN_Code).returns(() => "RE");

		entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefAccTaxRateUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([code1.object, code2.object]));
		entityManager
			.setup((x) =>
				x.getAsync("RefCountry", [ServiceType.Safe], It.isAny(), false)
			)
			.returns(() => Promise.resolve([codeCountry.object]));
	});

	it("render data", async () => {
		var wrapper = mount<typeof RefAccTaxRateUserViewSearchForm>(
			<RefAccTaxRateUserViewSearchForm entityManager={entityManager.object} />
		);

		await pressFind(wrapper as any);

		expect(wrapper.find("tbody > tr")).toHaveLength(2);
		expect(wrapper.find("tbody > tr").at(0).html()).toContain(
			'<tr><td>RE</td><td>STD</td><td>21</td><td>1</td><td>1900-01-01</td><td>2079-06-06</td><td><input type="checkbox" readonly="" checked=""></td><td><input type="checkbox" readonly="" checked=""></td><td><button type="button" class="btn btn-info btn-sm">Edit</button></td></tr>'
		);
		expect(wrapper.find(FilterStrip).at(0).key()).toEqual("Country");
		expect(wrapper.find(FilterStrip).at(1).key()).toEqual("Reference Type");
	});

	it("readonly", async () => {
		var wrapper = mount<typeof RefAccTaxRateUserViewSearchForm>(
			<RefAccTaxRateUserViewSearchForm entityManager={entityManager.object} />
		);

		await pressFind(wrapper as any);
		await pressEdit(wrapper as any, 0);

		expect(
			wrapper
				.find(CodeInput)
				.filterWhere((x) => x.props().propertyName == "ZAT_RN_NKCountry")
				.at(0)
				.props().readOnly
		).toEqual(true); //Country of record being edited
		expect(
			wrapper
				.find(CodeInput)
				.filterWhere((x) => x.props().propertyName == "ZAT_ReferenceRateType")
				.at(0)
				.props().readOnly
		).toEqual(true); //Reference Type of record being edited
		expect(
			wrapper
				.find(DateTimeInput)
				.filterWhere((x) => x.props().propertyName == "ZAT_StartDate")
				.at(0)
				.props().readOnly
		).toEqual(true); //Start Date of record being edited

		await pressAdd(wrapper as any);

		expect(
			wrapper
				.find(CodeInput)
				.filterWhere((x) => x.props().propertyName == "ZAT_RN_NKCountry")
				.at(0)
				.props().readOnly
		).toEqual(false); //Country of record being added
		expect(
			wrapper
				.find(CodeInput)
				.filterWhere((x) => x.props().propertyName == "ZAT_ReferenceRateType")
				.at(0)
				.props().readOnly
		).toEqual(false); //Reference Type of record being added
		expect(
			wrapper
				.find(DateTimeInput)
				.filterWhere((x) => x.props().propertyName == "ZAT_StartDate")
				.at(0)
				.props().readOnly
		).toEqual(false); //Start Date of record being added
	});

	it("onFilterValueChange", async () => {
		var wrapper = mount<typeof RefAccTaxRateUserViewSearchForm>(
			<RefAccTaxRateUserViewSearchForm entityManager={entityManager.object} />
		);
		const firstFilterStrip = wrapper.find(FilterStrip).at(0);
		expect(firstFilterStrip.find("select").at(0).html()).toContain("Equals");
		firstFilterStrip
			.find("select")
			.at(0)
			.simulate("change", { target: { value: 1 } });

		await act(() => new Promise(setImmediate));
		firstFilterStrip.update();

		expect(firstFilterStrip.find("select").at(0).html()).toContain("Contains");
	});

	it("onAddingButtonClick", async () => {
		let wrapper = mount<typeof RefAccTaxRateUserViewSearchForm>(
			<RefAccTaxRateUserViewSearchForm entityManager={entityManager.object} />
		);

		await pressAdd(wrapper as any);
		expect(wrapper.find("tbody > tr")).toHaveLength(1);
		await pressAdd(wrapper as any);
		expect(wrapper.find("tbody > tr")).toHaveLength(2);
	});

	it("onCancelButtonClick", async () => {
		let wrapper = mount<typeof RefAccTaxRateUserViewSearchForm>(
			<RefAccTaxRateUserViewSearchForm entityManager={entityManager.object} />
		);

		await pressFind(wrapper as any);
		expect(wrapper.find("tbody > tr")).toHaveLength(2);
		await pressAdd(wrapper as any);
		expect(wrapper.find("tbody > tr")).toHaveLength(3);
		await pressCancel(wrapper as any, 0);
		expect(wrapper.find("tbody > tr")).toHaveLength(2);

		let code3 = Mock.ofType<RefAccTaxRateUserView>();
		code3.setup((x) => x.ZAT_PK).returns(() => "2");
		code3.setup((x) => x.ZAT_ReferenceRateType).returns(() => "LOW");
		code3.setup((x) => x.ZAT_RN_NKCountry).returns(() => "BJ");
		code3.setup((x) => x.ZAT_RateDenominator).returns(() => 10);
		code3.setup((x) => x.ZAT_RateNumerator).returns(() => 20);
		code3.setup((x) => x.ZAT_IsSystem).returns(() => true);
		code3.setup((x) => x.ZAT_IsPublished).returns(() => true);
		code3.setup((x) => x.ZAT_StartDate).returns(() => "1900-01-01");
		code3.setup((x) => x.ZAT_EndDate).returns(() => "2079-06-06");

		entityManager
			.setup((x) =>
				x.getAsync(
					"RefAccTaxRateUserView",
					[ServiceType.Safe],
					It.isAny(),
					true
				)
			)
			.returns(() => Promise.resolve([code3.object]));
		entityManager.setup((x) => x.isInDatabase("2")).returns(() => true);
		await pressFind(wrapper as any);
		expect(wrapper.find("tbody > tr")).toHaveLength(2);
		await pressEdit(wrapper as any, 1);
		await pressCancel(wrapper as any, 0);
		expect(wrapper.find("tbody > tr")).toHaveLength(2);
		expect(wrapper.find("tbody > tr")).toHaveLength(2);
		expect(wrapper.find("tbody > tr").at(1).find("td").at(2).text()).toEqual(
			"20"
		); //check ZAT_RateNumerator
	});

	it("onGridValueChange", async () => {
		let wrapper = mount<typeof RefAccTaxRateUserViewSearchForm>(
			<RefAccTaxRateUserViewSearchForm entityManager={entityManager.object} />
		);
		await pressAdd(wrapper as any);
		expect(wrapper.find("tbody > tr")).toHaveLength(1);
		const columnToChange = wrapper.find("tbody > tr").at(0).find("td").at(2);
		const inputToChange = columnToChange
			.find(TextInput)
			.at(0)
			.find("input")
			.at(0);
		expect(inputToChange.props().value).toEqual(0);
		inputToChange.simulate("change", { target: { value: 11 } });
		await act(() => new Promise(setImmediate));
		wrapper.update();
		expect(
			wrapper
				.find("tbody > tr")
				.at(0)
				.find("td")
				.at(2)
				.find(TextInput)
				.find("input")
				.at(0)
				.props().value
		).toEqual(11); //check ZAT_RateNumerator
	});

	it("onGridValueChanged_ZAT_RateDenominator", async () => {
		entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync("RefCountry", [ServiceType.Safe], It.isAny(), false)
			)
			.returns(() => Promise.resolve([]));
		let wrapper = mount<typeof RefAccTaxRateUserViewSearchForm>(
			<RefAccTaxRateUserViewSearchForm entityManager={entityManager.object} />
		);
		await pressAdd(wrapper as any);

		//ZAT_RateDenominator
		let columnToChange = wrapper.find("tbody > tr").at(0).find("td").at(2);
		let inputToChange = columnToChange
			.find(TextInput)
			.at(0)
			.find("input")
			.at(0);
		inputToChange.simulate("change", { target: { value: -1 } });
		await act(() => new Promise(setImmediate));
		wrapper.update(); //change ZAT_RateDenominator to a incorrect number
		inputToChange.simulate("blur");
		await act(() => new Promise(setImmediate));
		wrapper.update();

		let errors = wrapper
			.find("tbody > tr")
			.at(0)
			.find("small")
			.findWhere((x) => x.hasClass("text-danger"));

		expect(errors.text()).toEqual(
			"This field is required to be greater than 0"
		);
	});

	it("onGridValueChanged_ZAT_RN_NKCountry", async () => {
		entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync("RefCountry", [ServiceType.Safe], It.isAny(), false)
			)
			.returns(() => Promise.resolve([]));
		let wrapper = mount<typeof RefAccTaxRateUserViewSearchForm>(
			<RefAccTaxRateUserViewSearchForm entityManager={entityManager.object} />
		);
		await pressAdd(wrapper as any);

		//ZAT_RN_NKCountry
		let columnToChange = wrapper.find("tbody > tr").at(0).find("td").at(0);
		let inputToChange = columnToChange
			.find(CodeInput)
			.at(0)
			.find("input")
			.at(0);
		inputToChange.simulate("change", { target: { value: "AAA" } });
		await act(() => new Promise(setImmediate));
		wrapper.update();
		inputToChange.simulate("blur");
		await act(() => new Promise(setImmediate));
		wrapper.update();

		let errors = wrapper
			.find("small")
			.findWhere((x) => x.hasClass("text-danger"));

		expect(errors.text()).toEqual("This value is not in the list");
	});

	it("onGridValueChanged_ZAT_ReferenceRateType", async () => {
		entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync("RefCountry", [ServiceType.Safe], It.isAny(), false)
			)
			.returns(() => Promise.resolve([]));
		let wrapper = mount<typeof RefAccTaxRateUserViewSearchForm>(
			<RefAccTaxRateUserViewSearchForm entityManager={entityManager.object} />
		);
		await pressAdd(wrapper as any);

		//ZAT_ReferenceRateType
		let columnToChange = wrapper.find("tbody > tr").at(0).find("td").at(1);
		let inputToChange = columnToChange
			.find(CodeInput)
			.at(0)
			.find("input")
			.at(0);
		inputToChange.simulate("change", { target: { value: "" } });
		await act(() => new Promise(setImmediate));
		wrapper.update();
		inputToChange.simulate("blur");
		await act(() => new Promise(setImmediate));
		wrapper.update();

		let errors = wrapper
			.find("small")
			.findWhere((x) => x.hasClass("text-danger"));

		expect(errors.text()).toEqual("This field is required.");
	});

	it("onCancelAllButtonClick", async () => {
		let wrapper = mount<typeof RefAccTaxRateUserViewSearchForm>(
			<RefAccTaxRateUserViewSearchForm entityManager={entityManager.object} />
		);

		await pressFind(wrapper as any);
		expect(wrapper.find("tbody > tr")).toHaveLength(2);
		await pressAdd(wrapper as any);
		expect(wrapper.find("tbody > tr")).toHaveLength(3);

		let code3 = Mock.ofType<RefAccTaxRateUserView>();
		code3.setup((x) => x.ZAT_PK).returns(() => "2");
		code3.setup((x) => x.ZAT_ReferenceRateType).returns(() => "LOW");
		code3.setup((x) => x.ZAT_RN_NKCountry).returns(() => "BJ");
		code3.setup((x) => x.ZAT_RateDenominator).returns(() => 10);
		code3.setup((x) => x.ZAT_RateNumerator).returns(() => 20);
		code3.setup((x) => x.ZAT_IsSystem).returns(() => true);
		code3.setup((x) => x.ZAT_IsPublished).returns(() => true);
		code3.setup((x) => x.ZAT_StartDate).returns(() => "1900-01-01");
		code3.setup((x) => x.ZAT_EndDate).returns(() => "2079-06-06");

		entityManager
			.setup((x) =>
				x.getAsync(
					"RefAccTaxRateUserView",
					[ServiceType.Safe],
					It.isAny(),
					true
				)
			)
			.returns(() => Promise.resolve([code3.object]));
		entityManager.setup((x) => x.isInDatabase("2")).returns(() => true);
		await pressEdit(wrapper as any, 1);
		await pressCancelAll(wrapper as any, 0);
		expect(wrapper.find("tbody > tr")).toHaveLength(2);
		expect(wrapper.find("tbody > tr").at(1).find("td").at(2).text()).toEqual(
			"20"
		); //check ZAT_RateNumerator
	});
});
