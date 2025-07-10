import { IEntity } from "../models/IEntity";
import { shallow } from "enzyme";
import React from "react";
import { Mock, It } from "typemoq";
import { IEntityManager, ServiceType } from "../EntityManager";
import IRefCusCodeType from "../models/IRefCusCodeType";
import { CodeInput } from "../CodeInput";
import { RefAccTaxRateUserView } from "../models/RefAccTaxRateUserView";

describe("CodeInput", () => {
	it("render", async () => {
		let codeType: IRefCusCodeType = {
			ZZK_PK: "1",
			ZZK_CodeType: "CD",
			ZZK_Description: "Hello",
			ZZK_IsReadonly: true,
			ZZK_MaxLength: 0,
			ZZK_ZZZ_NKDataGrouping: "ZZ"
		}
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IRefCusCodeType>("RefCusCodeType", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([codeType]));
		let codeList: IEntity = { ZZD_CodeType: "AA" };
		let wrapper = shallow<CodeInput>(<CodeInput entity={codeList} label="Code Type" propertyName="ZZD_CodeType" onValueChange={jest.fn} onValueChanged={jest.fn}
			listCodePropertyName="ZZK_CodeType" listDescriptionPropertyName="ZZK_Description" entityManager={entityManager.object} listEntityTypeName="RefCusCodeType" maxLength={5} />);
		await wrapper.instance().componentDidMount();
		expect(wrapper.find("input[type='text']").at(0).html()).toContain('value="AA"');
		expect(wrapper.find("datalist > option").at(0).html()).toContain("CD");
	});

	it("render_listEntityTypeNameIsArray", async () => {
		let statusList: IEntity[] =
			[
				{ Status_Value: "ERR", Status_Description: "Error" },
				{ Status_Value: "PRS", Status_Description: "Success" }
			];
		let entityManager = Mock.ofType<IEntityManager>();
		let processorStatus: IEntity = { PRC_Status: "PRS" };
		let wrapper = shallow<CodeInput>(<CodeInput entity={processorStatus} label="Status" propertyName="PRC_Status" onValueChange={jest.fn} onValueChanged={jest.fn}
			listCodePropertyName="Status_Value" listDescriptionPropertyName="Status_Description" entityManager={entityManager.object} listEntityTypeName={statusList} maxLength={5} />);
		await wrapper.instance().componentDidMount();
		expect(wrapper.find("input[type='text']").at(0).html()).toContain('value="PRS"');
		expect(wrapper.find("datalist > option").at(0).html()).toContain("ERR");
	});

	it("renderWithPrevEntities", () => {
		let code1 = Mock.ofType<RefAccTaxRateUserView>();
		code1.setup(x => x.ZAT_PK).returns(() => "1");
		code1.setup(x => x.ZAT_ReferenceRateType).returns(() => "STD");
		code1.setup(x => x.ZAT_RN_NKCountry).returns(() => "RE");
		code1.setup(x => x.ZAT_RateDenominator).returns(() => 1);
		code1.setup(x => x.ZAT_RateNumerator).returns(() => 21);
		code1.setup(x => x.ZAT_IsSystem).returns(() => true);
		code1.setup(x => x.ZAT_IsPublished).returns(() => true);
		code1.setup(x => x.ZAT_StartDate).returns(() => "1900-01-01T00:00:00Z");
		code1.setup(x => x.ZAT_EndDate).returns(() => "2079-06-06T23:59:00Z");

		let entities = [{
			ZAT_PK: "1",
			ZAT_ReferenceRateType: "LOW",
			ZAT_RN_NKCountry: "RE",
			ZAT_RateDenominator: 10,
			ZAT_RateNumerator: 19,
			ZAT_IsSystem: true,
			ZAT_IsPublished: true,
			ZAT_StartDate: "1900-01-01T00:00:00Z",
			ZAT_EndDate: "2079-06-06T23:59:00Z",
		}];
		let code2: IEntity = { ZAT_RN_NKCountry: "RE" };
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<RefAccTaxRateUserView>("IRefAccTaxRateUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([code1.object]));

		let wrapper = shallow<CodeInput>(<CodeInput prevEntity={entities} entity={code2} propertyName="ZAT_ReferenceRateType" onValueChange={jest.fn} onValueChanged={jest.fn}
			listCodePropertyName="ZAT_ReferenceRateType" listDescriptionPropertyName="*" entityManager={entityManager.object} listEntityTypeName="RefAccTaxRateUserView" maxLength={5} />);
		wrapper.instance().componentDidMount();
		expect(wrapper.state().selectedValues).toEqual([]);
		expect(wrapper.find("datalist > option").at(0).html()).toContain("LOW");
	});

	it("onValueChange", () => {
		let codeType: IRefCusCodeType = {
			ZZK_PK: "1",
			ZZK_CodeType: "CD",
			ZZK_Description: "Hello",
			ZZK_IsReadonly: true,
			ZZK_MaxLength: 0,
			ZZK_ZZZ_NKDataGrouping: "ZZ"
		}
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IRefCusCodeType>("RefCusCodeType", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([codeType]));
		let codeList: IEntity = { ZZD_CodeType: "AA" };
		let wrapper = shallow<CodeInput>(<CodeInput entity={codeList} label="Code Type" propertyName="ZZD_CodeType" onValueChange={jest.fn} onValueChanged={jest.fn}
			listCodePropertyName="ZZK_CodeType" listDescriptionPropertyName="ZZK_Description" entityManager={entityManager.object} listEntityTypeName="RefCusCodeType" maxLength={5} />);
		wrapper.instance().onValueChange("AA" as any);
		expect(wrapper.instance().valueChanged).toEqual(true);
	});

	it("onValueChanged", () => {
		let codeType: IRefCusCodeType = {
			ZZK_PK: "1",
			ZZK_CodeType: "CD",
			ZZK_Description: "Hello",
			ZZK_IsReadonly: true,
			ZZK_MaxLength: 0,
			ZZK_ZZZ_NKDataGrouping: "ZZ"
		}
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IRefCusCodeType>("RefCusCodeType", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([codeType]));
		let codeList: IEntity = { ZZD_CodeType: "AA" };
		let wrapper = shallow<CodeInput>(<CodeInput entity={codeList} label="Code Type" propertyName="ZZD_CodeType" onValueChange={jest.fn} onValueChanged={jest.fn}
			listCodePropertyName="ZZK_CodeType" listDescriptionPropertyName="ZZK_Description" entityManager={entityManager.object} listEntityTypeName="RefCusCodeType" maxLength={5} />);
		wrapper.instance().valueChanged = true;
		wrapper.instance().onValueChanged();
		expect(wrapper.instance().valueChanged).toEqual(false);
	});
});
