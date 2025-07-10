import { shallow } from "enzyme";
import { Mock, It } from "typemoq";
import { IEntityManager, ServiceType } from "../EntityManager";
import React from "react";
import { RefCusCodeListFRFallbackInvokeForm } from "../RefCusCodeListFRFallbackInvokeForm";
import { TextInput } from "../TextInput";
import { CheckBox } from "../CheckBox";
import { DateTimeInput } from "../DateTimeInput";
import { ValidationServiceWrapper } from "../ValidationService";

describe("<RefCusCodeListFRFallbackInvokeForm />", () => {
	it("render", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		let wrapper = shallow<RefCusCodeListFRFallbackInvokeForm>(<RefCusCodeListFRFallbackInvokeForm entityManager={entityManager.object} />);

		expect(wrapper.find(CheckBox).at(0).props().propertyName).toEqual("IsSelectAll");
		expect(wrapper.find(CheckBox).at(1).props().propertyName).toEqual("IsDeltaT");
		expect(wrapper.find(CheckBox).at(2).props().propertyName).toEqual("IsDeltaG");
		expect(wrapper.find(CheckBox).at(3).props().propertyName).toEqual("IsDeltaX");
		expect(wrapper.find(CheckBox).at(4).props().propertyName).toEqual("IsGamma");
		expect(wrapper.find(CheckBox).at(5).props().propertyName).toEqual("IsIcs");
		expect(wrapper.find(CheckBox).at(6).props().propertyName).toEqual("IsEcs");
		expect(wrapper.find(DateTimeInput).at(0).props().propertyName).toEqual("DateAndTime");
		expect(wrapper.find(TextInput).at(0).props().propertyName).toEqual("Comment");
	});
});

it("onValueChange", async () => {
	let entityManager = Mock.ofType<IEntityManager>();
	let wrapper = shallow<RefCusCodeListFRFallbackInvokeForm>(<RefCusCodeListFRFallbackInvokeForm entityManager={entityManager.object} />);

	await wrapper.instance().onValueChange(null, "DateAndTime", "" as any);
	expect(wrapper.state().applicationList.DateAndTime).toEqual("");

	await wrapper.instance().onValueChange(null, "Comment", "AAA" as any);
	expect(wrapper.state().applicationList.Comment).toEqual("AAA");
});

it("onValueChanged", async () => {
	let entityManager = Mock.ofType<IEntityManager>();
	let wrapper = shallow<RefCusCodeListFRFallbackInvokeForm>(<RefCusCodeListFRFallbackInvokeForm entityManager={entityManager.object} />);
	let codeValidationService = {
		DateAndTime: [(e: any, p: any) => "There is error"],
		Comment: [(e: any, p: any) => "There is error"]
	}
	wrapper.instance().codeValidationService = new ValidationServiceWrapper([codeValidationService], (isValidating: boolean) => void { });

	await wrapper.instance().onValueChanged(null, "DateAndTime");
	expect(wrapper.state().invokeValidationResults["DateAndTime"]).toEqual(["There is error"]);

	await wrapper.instance().onValueChanged(null, "Comment");
	expect(wrapper.state().invokeValidationResults["Comment"]).toEqual(["There is error"]);
});
