import React from "react";
import { shallow } from "enzyme";
import { Mock, It } from "typemoq";
import { IEntityManager } from "../EntityManager";
import { CodeInput } from "../CodeInput";
import { TextInput } from "../TextInput";
import { DateTimeInput } from "../DateTimeInput";
import { CheckBox } from "../CheckBox";
import { IRefApplicationAttribute, IRefApplicationAttributeDefault, RefApplicationAttributeWrapper } from "../models/IRefApplicationAttribute";
import IRefApplicationAttributeType from "../models/IRefApplicationAttributeType";
import { RefApplicationAttributeDetailsForm } from "../RefApplicationAttributeDetailsForm";
import { PasswordInput } from "../PasswordInput";
import Button from "../Button";

describe("<RefApplicationAttributeDetailsForm />", () => {
	it("render", () => {
		let entityManager = Mock.ofType<IEntityManager>();
		let attribute1Obj = RefApplicationAttributeWrapper.interfaceToObject(attribute1, false, null);
		let attribute2Obj = RefApplicationAttributeWrapper.interfaceToObject(attribute2, false, null);
		let attribute3Obj = RefApplicationAttributeWrapper.interfaceToObject(attribute3, false, null);
		let attribute4Obj = RefApplicationAttributeWrapper.interfaceToObject(attribute4, false, attributeDefault1);
		let attribute5Obj = RefApplicationAttributeWrapper.interfaceToObject(attribute5, false, null);
		let attribute6Obj = RefApplicationAttributeWrapper.interfaceToObject(attribute6, false, null);

		let wrapper = shallow<RefApplicationAttributeDetailsForm>(<RefApplicationAttributeDetailsForm attributes={[attribute1Obj, attribute2Obj, attribute3Obj, attribute4Obj, attribute5Obj, attribute6Obj]} attributeTypes={[attributeType1, attributeType2, attributeType3, attributeType4, attributeType5, attributeType6]} save={jest.fn} onAttributeValueChange={jest.fn} onAttributeValueChanged={jest.fn} validationResults={{}} entityManager={entityManager.object} />);

		expect(wrapper.find(CodeInput).at(0).props().entity).toEqual(attribute1Obj);
		expect(wrapper.find(CodeInput).at(0).props().propertyName).toEqual("RAA_RAT_NKType");
		expect(wrapper.find(TextInput).at(0).props().entity).toEqual(attribute1Obj);
		expect(wrapper.find(TextInput).at(0).props().propertyName).toEqual("RAA_AttributeName");
		expect(wrapper.find(TextInput).at(1).props().entity).toEqual(attribute1Obj);
		expect(wrapper.find(TextInput).at(1).props().propertyName).toEqual("RAA_Value");

		expect(wrapper.find(DateTimeInput).at(0).props().entity).toEqual(attribute2Obj);
		expect(wrapper.find(DateTimeInput).at(0).props().propertyName).toEqual("RAA_Value");
		expect(wrapper.find(CheckBox).at(0).props().entity).toEqual(attribute4Obj);
		expect(wrapper.find(CheckBox).at(0).props().propertyName).toEqual("RAA_IsDefault");
		expect(wrapper.find(CheckBox).at(1).props().propertyName).toEqual("RAA_Value");
		expect(wrapper.find(PasswordInput).at(0).props().entity).toEqual(attribute5Obj);

		expect(wrapper.find(Button).at(3).props().hidden).toEqual(false);
		expect(wrapper.find(Button).at(6).props().hidden).toEqual(true);

		expect(wrapper.find("input[id='file-selector_123']").length).toEqual(1);
		expect(wrapper.find("input[id='file-selector_123']").props().hidden).toEqual(true);
		expect(wrapper.find("input[id='file-selector_567']").length).toEqual(1);
		expect(wrapper.find("input[id='file-selector_567']").props().hidden).toEqual(false);
	});

	it("arrayBufferToBase64", () => {
		let attribute1Obj = RefApplicationAttributeWrapper.interfaceToObject(attribute1, false, attributeDefault1);
		let attribute2Obj = RefApplicationAttributeWrapper.interfaceToObject(attribute2, false, attributeDefault2);
		let attribute3Obj = RefApplicationAttributeWrapper.interfaceToObject(attribute3, false, attributeDefault3);

		let entityManager = Mock.ofType<IEntityManager>();
		let wrapper = shallow<RefApplicationAttributeDetailsForm>(<RefApplicationAttributeDetailsForm attributes={[attribute1Obj, attribute2Obj, attribute3Obj]} attributeTypes={[attributeType1, attributeType2, attributeType3]} save={jest.fn} onAttributeValueChange={jest.fn} onAttributeValueChanged={jest.fn} validationResults={{}} entityManager={entityManager.object} />);
		let buffer = Uint8Array.from([34, 78, 79, 86, 65, 73, 82, 34, 32, 76, 105, 109, 105, 116, 101, 100, 32]);
		expect(wrapper.instance().arrayBufferToBase64(buffer)).toEqual("Ik5PVkFJUiIgTGltaXRlZCA=");
	});

	it("addDefaultCheckBox", () => {
		let attribute1Obj = RefApplicationAttributeWrapper.interfaceToObject(attribute1, false, attributeDefault1);
		let attribute2Obj = RefApplicationAttributeWrapper.interfaceToObject(attribute2, false, attributeDefault3);
		let attribute3Obj = RefApplicationAttributeWrapper.interfaceToObject(attribute3, false, null);

		let entityManager = Mock.ofType<IEntityManager>();
		let wrapper = shallow<RefApplicationAttributeDetailsForm>(<RefApplicationAttributeDetailsForm attributes={[attribute1Obj, attribute2Obj, attribute3Obj]} attributeTypes={[attributeType1, attributeType2, attributeType3]} save={jest.fn} onAttributeValueChange={jest.fn} onAttributeValueChanged={jest.fn} validationResults={{}} entityManager={entityManager.object} />);

		expect(wrapper.instance().addDefaultCheckBox(attribute1Obj)).not.toBeUndefined();
		expect(wrapper.instance().addDefaultCheckBox(attribute2Obj)).not.toBeUndefined();
		expect(wrapper.instance().addDefaultCheckBox(attribute3Obj)).toBeUndefined();
	});

	var attributeType1: IRefApplicationAttributeType = {
		RAT_PK: "type-1",
		RAT_Type: "String",
		RAT_Description: "string type"
	};

	var attributeType2: IRefApplicationAttributeType = {
		RAT_PK: "type-2",
		RAT_Type: "DateTime",
		RAT_Description: "datetime type"
	};

	var attributeType3: IRefApplicationAttributeType = {
		RAT_PK: "type-3",
		RAT_Type: "File",
		RAT_Description: "file type"
	};

	var attributeType4: IRefApplicationAttributeType = {
		RAT_PK: "type-4",
		RAT_Type: "Boolean",
		RAT_Description: "boolean type"
	};

	var attributeType5: IRefApplicationAttributeType = {
		RAT_PK: "type-5",
		RAT_Type: "Credential",
		RAT_Description: "credential type"
	};

	var attributeType6: IRefApplicationAttributeType = {
		RAT_PK: "type-6",
		RAT_Type: "SecretFile",
		RAT_Description: "secret file"
	};

	var attribute1: IRefApplicationAttribute = {
		RAA_PK: "123",
		RAA_ConfigFilePath: "AAA.exe.config",
		RAA_AttributeName: "Attribute1",
		RAA_Value: "Value1",
		RAA_RAT_NKType: attributeType1.RAT_Type,
		RAA_JobGroup: "",
		RAA_Content: null,
		RAA_IsDefault: false
	};

	var attribute2: IRefApplicationAttribute = {
		RAA_PK: "345",
		RAA_ConfigFilePath: "BBB.exe.config",
		RAA_AttributeName: "Attribute2",
		RAA_Value: "2021-04-10 15:05:30",
		RAA_RAT_NKType: attributeType2.RAT_Type,
		RAA_JobGroup: "",
		RAA_Content: null,
		RAA_IsDefault: false
	};

	var attribute3: IRefApplicationAttribute = {
		RAA_PK: "567",
		RAA_ConfigFilePath: "CCC.exe.config",
		RAA_AttributeName: "Attribute3",
		RAA_Value: "",
		RAA_RAT_NKType: attributeType3.RAT_Type,
		RAA_JobGroup: "",
		RAA_Content: "Ik5PVkFJUiIgTGltaXRlZCBMaWFiaWxpdHkgQ29tcGFueSAgICAg",
		RAA_IsDefault: false
	};

	var attribute4: IRefApplicationAttribute = {
		RAA_PK: "789",
		RAA_ConfigFilePath: "DDD.exe.config",
		RAA_AttributeName: "Attribute4",
		RAA_Value: "true",
		RAA_RAT_NKType: attributeType4.RAT_Type,
		RAA_JobGroup: "",
		RAA_Content: null,
		RAA_IsDefault: false
	};

	var attribute5: IRefApplicationAttribute = {
		RAA_PK: "1000",
		RAA_ConfigFilePath: "EEE.exe.config",
		RAA_AttributeName: "Attribute5",
		RAA_Value: "",
		RAA_RAT_NKType: attributeType5.RAT_Type,
		RAA_JobGroup: "",
		RAA_Content: null,
		RAA_IsDefault: false
	};

	var attribute6: IRefApplicationAttribute = {
		RAA_PK: "1001",
		RAA_ConfigFilePath: "CCC.exe.config",
		RAA_AttributeName: "Attribute6",
		RAA_Value: "",
		RAA_RAT_NKType: attributeType6.RAT_Type,
		RAA_JobGroup: "",
		RAA_Content: null,
		RAA_IsDefault: false
	};

	var attributeDefault1: IRefApplicationAttributeDefault = {
		RAA_PK: "123",
		RAA_ConfigFilePath: "AAA.exe.config",
		RAA_AttributeName: "Attribute1",
		RAA_Value: "Value1",
		RAA_RAT_NKType: attributeType1.RAT_Type,
		RAA_Content: null,
		RAA_IsDefault: false
	};

	var attributeDefault2: IRefApplicationAttributeDefault = {
		RAA_PK: "345",
		RAA_ConfigFilePath: "BBB.exe.config",
		RAA_AttributeName: "Attribute2",
		RAA_Value: "2021-04-10 15:05:30",
		RAA_RAT_NKType: attributeType2.RAT_Type,
		RAA_Content: null,
		RAA_IsDefault: false
	};

	var attributeDefault3: IRefApplicationAttributeDefault = {
		RAA_PK: "567",
		RAA_ConfigFilePath: "CCC.exe.config",
		RAA_AttributeName: "Attribute3",
		RAA_Value: "",
		RAA_RAT_NKType: attributeType3.RAT_Type,
		RAA_Content: "Ik5PVkFJUiIgTGltaXRlZCBMaWFiaWxpdHkgQ29tcGFueSAgICAg",
		RAA_IsDefault: false
	};
});
