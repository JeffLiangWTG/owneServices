import React from "react";
import { shallow } from "enzyme";
import { Mock, It } from "typemoq";
import { IEntityManager, ServiceType } from "../EntityManager";
import { TextInput } from "../TextInput";
import { CheckBox } from "../CheckBox";
import { ValidationServiceWrapper } from "../ValidationService";
import { RefApplicationConfigDetailsForm } from "../RefApplicationConfigDetailsForm";
import { RefApplicationAttributeDetailsForm } from "../RefApplicationAttributeDetailsForm";
import IQrtzJobDetails from "../models/IQrtzJobDetails";
import { IRefApplicationAttribute, IRefApplicationAttributeDefault, RefApplicationAttributeWrapper } from "../models/IRefApplicationAttribute";
import IRefApplicationAttributeType from "../models/IRefApplicationAttributeType";
import { FilterOps } from "../Filter";

describe("<RefApplicationConfigDetailsForm />", () => {
	it("render", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IQrtzJobDetails>("QRTZ_JOB_DETAILS", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([job]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttribute>("RefApplicationAttribute", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attribute1, attribute2, attribute3, attribute4]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeType>("RefApplicationAttributeType", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attributeType1, attributeType2, attributeType3, attributeType4]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeDefault>("RefApplicationAttributeDefault", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attributeDefault1, attributeDefault2, attributeDefault3, attributeDefault4]));

		let wrapper = shallow<RefApplicationConfigDetailsForm>(<RefApplicationConfigDetailsForm entityManager={entityManager.object} id="12345" />);
		await wrapper.instance().componentDidMount();
		let textInputs = wrapper.find(TextInput);
		expect(textInputs.length).toEqual(4);
		expect(textInputs.at(0).props().propertyName).toEqual("JOB_NAME");
		expect(textInputs.at(1).props().propertyName).toEqual("ProgramExePath");
		expect(textInputs.at(2).props().propertyName).toEqual("ProgramArgs");
		expect(textInputs.at(3).props().propertyName).toEqual("CountryCode");
		expect(textInputs.map(x => x.props().readOnly)).toEqual([true, true, true, true]);

		let attributeDetailsForm = wrapper.find(RefApplicationAttributeDetailsForm);
		expect(attributeDetailsForm.length).toEqual(1);
		expect(attributeDetailsForm.props().attributeTypes).toEqual([attributeType1, attributeType2, attributeType3, attributeType4]);
		expect(attributeDetailsForm.props().attributes[0].RAA_RAT_NKType).toEqual(attributeType1.RAT_Type);
		expect(attributeDetailsForm.props().attributes[1].RAA_RAT_NKType).toEqual(attributeType2.RAT_Type);
		expect(attributeDetailsForm.props().attributes[2].RAA_RAT_NKType).toEqual(attributeType3.RAT_Type);
		expect(attributeDetailsForm.props().attributes[3].RAA_RAT_NKType).toEqual(attributeType4.RAT_Type);
		expect(attributeDetailsForm.props().attributes[3].RAA_Value).toEqual("********");
		expect(attributeDetailsForm.props().attributes[0].defaultRecord).toEqual(attributeDefault1);
		expect(attributeDetailsForm.props().attributes[1].defaultRecord).toEqual(attributeDefault2);
		expect(attributeDetailsForm.props().attributes[2].defaultRecord).toEqual(attributeDefault3);
		expect(attributeDetailsForm.props().attributes[3].defaultRecord).toEqual(attributeDefault4);
	});

	it("loadDefaultAttributes", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IQrtzJobDetails>("QRTZ_JOB_DETAILS", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([job]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttribute>("RefApplicationAttribute", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attribute1, attribute2]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeType>("RefApplicationAttributeType", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attributeType1, attributeType2]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeDefault>("RefApplicationAttributeDefault", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attributeDefault1, attributeDefault2]));

		let wrapper = shallow<RefApplicationConfigDetailsForm>(<RefApplicationConfigDetailsForm entityManager={entityManager.object} id="12345" />);
		await wrapper.instance().componentDidMount();

		let attributeDetailsForm = wrapper.find(RefApplicationAttributeDetailsForm);
		expect(attributeDetailsForm.length).toEqual(1);
		expect(attributeDetailsForm.props().attributeTypes).toEqual([attributeType1, attributeType2]);
		expect(attributeDetailsForm.props().attributes.length).toEqual(2);
		expect(attributeDetailsForm.props().attributes[0].defaultRecord).toEqual(attributeDefault1);
		expect(attributeDetailsForm.props().attributes[1].defaultRecord).toEqual(attributeDefault2);
		expect(wrapper.state().attributes.every(a => a.RAA_JobGroup == job.JOB_GROUP));
	});

	it("prioritizeDbRecordsOverDefaults", async () => {
		let configFilePath = "AAA.exe.config";
		let defaultAttribute: IRefApplicationAttributeDefault = {
			RAA_PK: "000",
			RAA_ConfigFilePath: configFilePath,
			RAA_AttributeName: "DefaultAttribute",
			RAA_Value: "DefaultValue",
			RAA_RAT_NKType: "",
			RAA_Content: null,
		};
		let attributeFromDb: IRefApplicationAttribute = {
			RAA_PK: "123",
			RAA_ConfigFilePath: configFilePath,
			RAA_AttributeName: "DefaultAttribute",
			RAA_Value: "Value1",
			RAA_RAT_NKType: attributeType1.RAT_Type,
			RAA_JobGroup: "",
			RAA_Content: null,
			RAA_IsDefault: false
		};
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IQrtzJobDetails>("QRTZ_JOB_DETAILS", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([job]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttribute>("RefApplicationAttribute", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attributeFromDb]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeType>("RefApplicationAttributeType", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attributeType1, attributeType2]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeDefault>("RefApplicationAttributeDefault", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([defaultAttribute]));

		let wrapper = shallow<RefApplicationConfigDetailsForm>(<RefApplicationConfigDetailsForm entityManager={entityManager.object} id="12345" />);
		await wrapper.instance().componentDidMount();

		let attributeDetailsForm = wrapper.find(RefApplicationAttributeDetailsForm);
		expect(attributeDetailsForm.length).toEqual(1);
		expect(attributeDetailsForm.props().attributeTypes).toEqual([attributeType1, attributeType2]);
		expect(attributeDetailsForm.props().attributes.length).toEqual(1);
		expect(attributeDetailsForm.props().attributes[0].defaultRecord).toEqual(defaultAttribute);
		expect(attributeDetailsForm.props().attributes[0].RAA_RAT_NKType).toEqual(attributeType1.RAT_Type);
		expect(attributeDetailsForm.props().attributes[0].RAA_Value).toEqual(attributeFromDb.RAA_Value);
	});

	it("defaultCheckBox", async () => {
		var defaultAttributeNotInDb: IRefApplicationAttributeDefault = {
			RAA_PK: "456",
			RAA_ConfigFilePath: "Default.exe.config",
			RAA_AttributeName: "DefaultAttr",
			RAA_Value: "Default",
			RAA_RAT_NKType: "",
			RAA_Content: null
		};

		var defaultAttributeOfDbRecord: IRefApplicationAttributeDefault = {
			RAA_PK: "2020",
			RAA_ConfigFilePath: "BBB.exe.config",
			RAA_AttributeName: "Attribute2",
			RAA_Value: "Default",
			RAA_RAT_NKType: attributeType2.RAT_Type,
			RAA_Content: null
		}

		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IQrtzJobDetails>("QRTZ_JOB_DETAILS", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([job]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttribute>("RefApplicationAttribute", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attribute1, attribute2]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeType>("RefApplicationAttributeType", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attributeType1, attributeType2]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeDefault>("RefApplicationAttributeDefault", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([defaultAttributeNotInDb, defaultAttributeOfDbRecord]));

		let wrapper = shallow<RefApplicationConfigDetailsForm>(<RefApplicationConfigDetailsForm entityManager={entityManager.object} id="12345" />);
		await wrapper.instance().componentDidMount();

		let attributeDetailsForm = wrapper.find(RefApplicationAttributeDetailsForm);
		let shallowed = attributeDetailsForm.shallow();

		expect(shallowed.find(CheckBox).length).toEqual(2);
		let firstEntity = shallowed.find(CheckBox).at(0).props().entity as RefApplicationAttributeWrapper;
		expect(firstEntity.RAA_Value).toEqual(attribute2.RAA_Value);
		expect(firstEntity.RAA_IsDefault).toEqual(false);
		expect(shallowed.find(CheckBox).at(0).props().propertyName).toEqual("RAA_IsDefault");

		let secondEntity = shallowed.find(CheckBox).at(1).props().entity as RefApplicationAttributeWrapper;
		expect(secondEntity.RAA_Value).toEqual(defaultAttributeNotInDb.RAA_Value);
		expect(secondEntity.RAA_IsDefault).toEqual(true);
		expect(shallowed.find(CheckBox).at(1).props().propertyName).toEqual("RAA_IsDefault");
	});

	it("onValueChange", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		let wrapper = shallow<RefApplicationConfigDetailsForm>(<RefApplicationConfigDetailsForm entityManager={entityManager.object} id="12345" />);
		await wrapper.instance().componentDidMount();
		await wrapper.instance().onValueChange(null, "JOB_NAME", "AAA" as any);
		expect(wrapper.state().qrtzJob.JOB_NAME).toEqual("AAA");
	});

	it("onAttributeValueChange", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IQrtzJobDetails>("QRTZ_JOB_DETAILS", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([job]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttribute>("RefApplicationAttribute", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attribute1, attribute2, attribute3]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeType>("RefApplicationAttributeType", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attributeType1, attributeType2, attributeType3]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeDefault>("RefApplicationAttributeDefault", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attributeDefault1, attributeDefault2, attributeDefault3]));
		let wrapper = shallow<RefApplicationConfigDetailsForm>(<RefApplicationConfigDetailsForm entityManager={entityManager.object} id="12345" />);

		await wrapper.instance().componentDidMount();
		await wrapper.instance().onAttributeValueChange(attribute1, "RAA_RAT_NKType", "AAA" as any);
		await wrapper.instance().onAttributeValueChange(attribute3, "RAA_Value", "5" as any);
		expect(wrapper.state().attributes[0].RAA_RAT_NKType).toEqual("AAA");
		expect(wrapper.state().attributes[0].RAA_Value).toEqual(attribute1.RAA_Value);
		expect(wrapper.state().attributes[2].RAA_Value).toEqual("5");
	});

	it("onAttributeValueChanged", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IQrtzJobDetails>("QRTZ_JOB_DETAILS", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([job]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttribute>("RefApplicationAttribute", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attribute1, attribute2]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeDefault>("RefApplicationAttributeDefault", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attributeDefault1, attributeDefault2]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeType>("RefApplicationAttributeType", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attributeType1, attributeType2]));
		let wrapper = shallow<RefApplicationConfigDetailsForm>(<RefApplicationConfigDetailsForm entityManager={entityManager.object} id="12345" />);

		let attrValidationService = {
			RAA_RAT_NKType: [(e: any, p: any) => "There is error"]
		};
		let attrValidationServiceWrapper = new ValidationServiceWrapper([attrValidationService], wrapper.instance().updateSaveButtonDisabledProperty);
		wrapper.instance().validationService = attrValidationServiceWrapper;
		await wrapper.instance().componentDidMount();
		await wrapper.instance().onAttributeValueChanged(attribute1, "RAA_RAT_NKType");
		expect(wrapper.state().attrValidationResults[attribute1.RAA_PK]["RAA_RAT_NKType"]).toEqual(["There is error"]);
	});

	it("getProgramExePath", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		let wrapper = shallow<RefApplicationConfigDetailsForm>(<RefApplicationConfigDetailsForm entityManager={entityManager.object} id="12345" />);

		let programPath = "CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.exe";
		expect(wrapper.instance().getProgramExePath(programPath)).toEqual("CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.exe");
		programPath = "..\\UniversalXMLProducers\\CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe";
		expect(wrapper.instance().getProgramExePath(programPath)).toEqual("CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe");
	});

	it("AfterSavingCredentialTypeValuesAreAsterisks", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IQrtzJobDetails>("QRTZ_JOB_DETAILS", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([job]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttribute>("RefApplicationAttribute", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attribute1, attribute2, attribute3, attribute4]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeType>("RefApplicationAttributeType", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attributeType1, attributeType2, attributeType3, attributeType4]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeDefault>("RefApplicationAttributeDefault", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attributeDefault1, attributeDefault2, attributeDefault3, attributeDefault4]));
		entityManager.setup((x) => x.saveChanges(It.isAny())).returns(() => Promise.resolve({ success: true, message: "saved" }));
		let wrapper = shallow<RefApplicationConfigDetailsForm>(<RefApplicationConfigDetailsForm entityManager={entityManager.object} id="12345" />);
		await wrapper.instance().componentDidMount();
		await wrapper.instance().save();
		expect(wrapper.state().attributes[3].RAA_Value).toEqual("********");
	});

	it("UnauthorizedWhenGet401Response", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IQrtzJobDetails>("QRTZ_JOB_DETAILS", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([job]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttribute>("RefApplicationAttribute", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.reject({ statusCode: 401 }));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeDefault>("RefApplicationAttributeDefault", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attributeDefault1, attributeDefault2]));

		let wrapper = shallow<RefApplicationConfigDetailsForm>(<RefApplicationConfigDetailsForm entityManager={entityManager.object} id="12345" />);
		await wrapper.instance().componentDidMount();
		expect(wrapper.state().saveMessage).toEqual("Unauthorized");
	});

	it("UnauthorizedWhenGet403Response", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IQrtzJobDetails>("QRTZ_JOB_DETAILS", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([job]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttribute>("RefApplicationAttribute", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attribute1, attribute2, attribute3, attribute4]));
		entityManager.setup(x => x.getAsync<IRefApplicationAttributeDefault>("RefApplicationAttributeDefault", [ServiceType.Staging], It.isAny(), false, It.isAny())).returns(() => Promise.reject({ statusCode: 403 }));

		let wrapper = shallow<RefApplicationConfigDetailsForm>(<RefApplicationConfigDetailsForm entityManager={entityManager.object} id="12345" />);
		await wrapper.instance().componentDidMount();
		expect(wrapper.state().saveMessage).toEqual("Unauthorized");
	});

	it("calls entityManager.reload for both entities on unmount", async () => {
		const entityManager = Mock.ofType<IEntityManager>();
		const reloadMock = jest.fn().mockResolvedValue(undefined);
		entityManager.setup(x => x.reload(
			"QRTZ_JOB_DETAILS",
			[ServiceType.Staging],
			It.isAny()
		)).returns(reloadMock);
		entityManager.setup(x => x.reload(
			"RefApplicationAttribute",
			[ServiceType.Staging],
			It.isAny()
		)).returns(reloadMock);

		const wrapper = shallow<RefApplicationConfigDetailsForm>(
			<RefApplicationConfigDetailsForm entityManager={entityManager.object} id="job-123" />
		);
			
		wrapper.instance().configFile = "test.config";

		await wrapper.instance().componentWillUnmount();

		expect(reloadMock).toHaveBeenCalledWith(
			"QRTZ_JOB_DETAILS",
			[ServiceType.Staging],
			[expect.objectContaining({ propertyName: "JOB_PK", operation: FilterOps.Equals, value: "job-123", type: "guid" })]
		);
		expect(reloadMock).toHaveBeenCalledWith(
			"RefApplicationAttribute",
			[ServiceType.Staging],
			[expect.objectContaining({ propertyName: "RAA_ConfigFilePath", operation: FilterOps.Equals, value: "test.config", type: "string" })]
		);
	});

	var job: IQrtzJobDetails = {
		JOB_PK: "12345",
		SCHED_NAME: "AA",
		JOB_GROUP: "Group A",
		JOB_CLASS_NAME: "",
		DESCRIPTION: "",
		JOB_NAME: "Xml Producer",
		CountryCode: "ZA",
		ProgramArgs: "DAILY",
		ProgramExePath: "AAA.exe"
	};

	var attributeType1: IRefApplicationAttributeType = {
		RAT_PK: "type-1",
		RAT_Type: "String",
		RAT_Description: "string type"
	};

	var attributeType2: IRefApplicationAttributeType = {
		RAT_PK: "type-2",
		RAT_Type: "Number",
		RAT_Description: "number type"
	};

	var attributeType3: IRefApplicationAttributeType = {
		RAT_PK: "type-3",
		RAT_Type: "StatusFlag",
		RAT_Description: "status flag"
	};

	var attributeType4: IRefApplicationAttributeType = {
		RAT_PK: "type-4",
		RAT_Type: "Credential",
		RAT_Description: "credential type"
	}

	var attribute1: IRefApplicationAttribute = {
		RAA_PK: "123",
		RAA_ConfigFilePath: "AAA.exe.config",
		RAA_AttributeName: "Attribute1",
		RAA_Value: "Value1",
		RAA_RAT_NKType: attributeType1.RAT_Type,
		RAA_JobGroup: "",
		RAA_Content: null
	};

	var attribute2: IRefApplicationAttribute = {
		RAA_PK: "456",
		RAA_ConfigFilePath: "BBB.exe.config",
		RAA_AttributeName: "Attribute2",
		RAA_Value: "10",
		RAA_RAT_NKType: attributeType2.RAT_Type,
		RAA_JobGroup: "",
		RAA_Content: null
	};

	var attribute3: IRefApplicationAttribute = {
		RAA_PK: "789",
		RAA_ConfigFilePath: "CCC.exe.config",
		RAA_AttributeName: "Attribute3",
		RAA_Value: "7",
		RAA_RAT_NKType: attributeType3.RAT_Type,
		RAA_JobGroup: "",
		RAA_Content: null
	};

	var attribute4: IRefApplicationAttribute = {
		RAA_PK: "1000",
		RAA_ConfigFilePath: "CCC.exe.config",
		RAA_AttributeName: "Attribute4",
		RAA_Value: "",
		RAA_RAT_NKType: attributeType4.RAT_Type,
		RAA_JobGroup: "",
		RAA_Content: null
	};

	var attributeDefault1: IRefApplicationAttributeDefault = {
		RAA_PK: "123",
		RAA_ConfigFilePath: "AAA.exe.config",
		RAA_AttributeName: "Attribute1",
		RAA_Value: "Value1",
		RAA_RAT_NKType: attributeType1.RAT_Type,
		RAA_Content: null
	};

	var attributeDefault2: IRefApplicationAttributeDefault = {
		RAA_PK: "456",
		RAA_ConfigFilePath: "BBB.exe.config",
		RAA_AttributeName: "Attribute2",
		RAA_Value: "10",
		RAA_RAT_NKType: "",
		RAA_Content: null
	};

	var attributeDefault3: IRefApplicationAttributeDefault = {
		RAA_PK: "789",
		RAA_ConfigFilePath: "CCC.exe.config",
		RAA_AttributeName: "Attribute3",
		RAA_Value: "7",
		RAA_RAT_NKType: attributeType3.RAT_Type,
		RAA_Content: null
	}

	var attributeDefault4: IRefApplicationAttributeDefault = {
		RAA_PK: "1000",
		RAA_ConfigFilePath: "CCC.exe.config",
		RAA_AttributeName: "Attribute4",
		RAA_Value: "",
		RAA_RAT_NKType: attributeType4.RAT_Type,
		RAA_Content: null
	}
});
