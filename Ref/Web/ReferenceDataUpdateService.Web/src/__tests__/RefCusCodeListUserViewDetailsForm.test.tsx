import { shallow } from "enzyme";
import { Mock, It } from "typemoq";
import { IEntityManager, ServiceType } from "../EntityManager";
import React from "react";
import { RefCusCodeListUserViewDetailsForm } from "../RefCusCodeListUserViewDetailsForm";
import { RefCusCodeListUserView } from "../models/RefCusCodeListUserView";
import { RefCusCodeListAttributeUserView } from "../models/RefCusCodeListAttributeUserView";
import { CodeInput } from "../CodeInput";
import { TextInput } from "../TextInput";
import { TextArea } from "../TextArea";
import { CheckBox } from "../CheckBox";
import { DateTimeInput } from "../DateTimeInput";
import IRefCusCodeListAttributeName from "../models/IRefCusCodeListAttributeName";
import { ValidationServiceWrapper } from "../ValidationService";
import IRefCusCodeType from "../models/IRefCusCodeType";
import uuid from "uuid";
import { FilterOps } from "../Filter";

describe("<RefCusCodeListUserViewDetailsForm />", () => {
    it("render", async () => {
       let entityManager = Mock.ofType<IEntityManager>();
       entityManager.setup(x => x.getAsync<RefCusCodeListUserView>("RefCusCodeListUserView", [ServiceType.Safe], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([code]));

       let wrapper = shallow<RefCusCodeListUserViewDetailsForm>(<RefCusCodeListUserViewDetailsForm entityManager={entityManager.object} id="12345" />);      
       await wrapper.instance().componentDidMount();
       expect(wrapper.find(CodeInput).at(0).props().propertyName).toEqual("ZZD_CountryOrGrouping");
       expect(wrapper.find(TextInput).at(0).props().propertyName).toEqual("ZZD_Code");
       expect(wrapper.find(TextArea).at(0).props().propertyName).toEqual("ZZD_Description");
       expect(wrapper.find(CodeInput).at(1).props().propertyName).toEqual("ZZD_CodeType");
       expect(wrapper.find(CheckBox).at(0).props().propertyName).toEqual("ZZD_IsRoa");
       expect(wrapper.find(CheckBox).at(1).props().propertyName).toEqual("ZZD_IsRai");
       expect(wrapper.find(CheckBox).at(2).props().propertyName).toEqual("ZZD_IsInw");
       expect(wrapper.find(CheckBox).at(3).props().propertyName).toEqual("ZZD_IsSea");
       expect(wrapper.find(CheckBox).at(4).props().propertyName).toEqual("ZZD_IsMai");
       expect(wrapper.find(CheckBox).at(5).props().propertyName).toEqual("ZZD_IsAir");
       expect(wrapper.find(CheckBox).at(6).props().propertyName).toEqual("ZZD_IsFix");
       expect(wrapper.find(DateTimeInput).at(0).props().propertyName).toEqual("ZZD_StartDate");
       expect(wrapper.find(DateTimeInput).at(1).props().propertyName).toEqual("ZZD_EndDate");
       expect(wrapper.find(CheckBox).at(7).props().propertyName).toEqual("ZZD_IsSystem");
       expect(wrapper.find(CheckBox).at(8).props().propertyName).toEqual("ZZD_IsPublished");
    });

    it("onValueChange", async () => {
        let entityManager = Mock.ofType<IEntityManager>();
        let wrapper = shallow<RefCusCodeListUserViewDetailsForm>(<RefCusCodeListUserViewDetailsForm entityManager={entityManager.object} id="12345" />);

        await wrapper.instance().componentDidMount();
        await wrapper.instance().onValueChange(null, "ZZD_Code", "AAA" as any);
        expect(wrapper.state().code.ZZD_Code).toEqual("AAA");
    });

    it("onValueChanged", async () => {
        let entityManager = Mock.ofType<IEntityManager>();
        let wrapper = shallow<RefCusCodeListUserViewDetailsForm>(<RefCusCodeListUserViewDetailsForm entityManager={entityManager.object} id="12345" />);
        let codeValidationService = {
            ZZD_Code : [(e: any, p: any) => "There is error"]
        }
        let codeValidationServiceWrapper = new ValidationServiceWrapper([codeValidationService], wrapper.instance().updateSaveButtonDisabledProperty);
        wrapper.instance().validationServices = codeValidationServiceWrapper;
        await wrapper.instance().componentDidMount();
        await wrapper.instance().onValueChanged(null, "ZZD_Code");
        expect(wrapper.state().codeValidationResults["ZZD_Code"]).toEqual(["There is error"]);
    });

    it("onCountryOrGroupingChange", async () => {
        let entityManager = Mock.ofType<IEntityManager>();
        let codeType : IRefCusCodeType = {
            ZZK_CodeType: "AAA",
            ZZK_Description: "TST",
            ZZK_PK: uuid.v1(),
            ZZK_ZZZ_NKDataGrouping: "EUN",
            ZZK_IsReadonly: false,
            ZZK_MaxLength: 3
        };
        entityManager.setup(x => x.getAsync<IRefCusCodeType>("RefCusCodeType", [ServiceType.Safe], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([codeType]));
        let wrapper = shallow<RefCusCodeListUserViewDetailsForm>(<RefCusCodeListUserViewDetailsForm entityManager={entityManager.object} id="12345" />);

        await wrapper.instance().componentDidMount();
        await wrapper.instance().onValueChange(null, "ZZD_CountryOrGrouping", "EUN" as any);
        await wrapper.instance().onValueChanged(codeType, "ZZD_CountryOrGrouping");
        expect(wrapper.state().code.ZZD_CountryOrGrouping).toEqual("EUN");
        expect(wrapper.state().filteredCodeTypes).toContain(codeType);
    });

    it("onAttributeValueChange", async () => {
        let entityManager = Mock.ofType<IEntityManager>();
        entityManager.setup(x => x.getAsync<RefCusCodeListUserView>("RefCusCodeListUserView", [ServiceType.Safe], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([code]));
        entityManager.setup(x => x.getAsync<RefCusCodeListAttributeUserView>("RefCusCodeListAttributeUserView", [ServiceType.Safe], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attr]));
        entityManager.setup(x => x.getAsync<IRefCusCodeListAttributeName>("RefCusCodeListAttributeName", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([]));

        let wrapper = shallow<RefCusCodeListUserViewDetailsForm>(<RefCusCodeListUserViewDetailsForm entityManager={entityManager.object} id="12345" />);
        await wrapper.instance().componentDidMount();
        await wrapper.instance().onAttributeValueChange(attr, "ZZE_ZXE_NKName", "AAA" as any);
        expect(wrapper.state().attributes[0].ZZE_ZXE_NKName).toEqual("AAA");
    });

    it("onAttributeValueChanged", async () => {
        let entityManager = Mock.ofType<IEntityManager>();
        entityManager.setup(x => x.getAsync<RefCusCodeListUserView>("RefCusCodeListUserView", [ServiceType.Safe], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([code]));
        entityManager.setup(x => x.getAsync<RefCusCodeListAttributeUserView>("RefCusCodeListAttributeUserView", [ServiceType.Safe], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attr]));
        entityManager.setup(x => x.getAsync<IRefCusCodeListAttributeName>("RefCusCodeListAttributeName", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([]));

        let wrapper = shallow<RefCusCodeListUserViewDetailsForm>(<RefCusCodeListUserViewDetailsForm entityManager={entityManager.object} id="12345" />);
        let codeValidationService = {
            ZZD_Code : [(e: any, p: any) => "There is error"]
        }
        let attrValidationService = {
            ZZE_ZXE_NKName : [(e: any, p: any) => "There is error"]
        }
        let attrValidationServiceWrapper = new ValidationServiceWrapper([codeValidationService, attrValidationService], wrapper.instance().updateSaveButtonDisabledProperty);
        wrapper.instance().validationServices = attrValidationServiceWrapper;
        await wrapper.instance().componentDidMount();
        await wrapper.instance().onAttributeValueChanged(attr, "ZZE_ZXE_NKName");
        expect(wrapper.state().attrValidationResults[attr.ZZE_PK]["ZZE_ZXE_NKName"]).toEqual(["There is error"]);
    });

	it("calls entityManager.reload for both entities on unmount", async () => {
		const entityManager = Mock.ofType<IEntityManager>();
		const reloadMock = jest.fn().mockResolvedValue(undefined);
		entityManager.setup(x => x.reload(
			"RefCusCodeListUserView",
			[ServiceType.Safe],
			It.isAny()
		)).returns(reloadMock);
		entityManager.setup(x => x.reload(
			"RefCusCodeListAttributeUserView",
			[ServiceType.Safe],
			It.isAny()
		)).returns(reloadMock);

		const wrapper = shallow<RefCusCodeListUserViewDetailsForm>(
			<RefCusCodeListUserViewDetailsForm entityManager={entityManager.object} id="12345" />
		);

		await wrapper.instance().componentWillUnmount();

		expect(reloadMock).toHaveBeenCalledWith(
			"RefCusCodeListUserView",
			[ServiceType.Safe],
			[expect.objectContaining({ propertyName: "ZZD_PK", operation: FilterOps.Equals, value: "12345", type: "guid" })]
		);
		expect(reloadMock).toHaveBeenCalledWith(
			"RefCusCodeListAttributeUserView",
			[ServiceType.Safe],
			[expect.objectContaining({ propertyName: "ZZE_ZZD_CodeList", operation: FilterOps.Equals, value: "12345", type: "guid" })]
		);
	});

    var code : RefCusCodeListUserView = {
        ZZD_PK: "12345",
        ZZD_Code: "AA",
        ZZD_CodeType: "AB",
        ZZD_CountryOrGrouping: "ZA",
        ZZD_Description: "ab",
        ZZD_StartDate: "1900-01-01T00:00:00Z",
        ZZD_EndDate: "2079-06-06T23:59:00Z",
        ZZD_IsAir: true,
        ZZD_IsFix: false,
        ZZD_IsInw: false,
        ZZD_IsPublished: false,
        ZZD_IsRoa: false,
        ZZD_IsSea: false,
        ZZD_IsMai: false,
        ZZD_IsRai: false,
        ZZD_IsSystem: false,
        ZZD_IsEditable: false
    };
    var attr : RefCusCodeListAttributeUserView  = {
        ZZE_PK : "1",
        ZZE_ZXE_NKName : "Code",
        ZZE_Value : "12",
        ZZE_IsAir : true,
        ZZE_IsFix : false,
        ZZE_IsInw : false,
        ZZE_IsMai : false,
        ZZE_IsRai : false,
        ZZE_IsRoa : false,
        ZZE_IsSea : false,
        ZZE_ZZD_CodeList : "90",
        ZZE_CodeType : "AB",
        ZZE_CountryOrGrouping : "ZA",
        ZZE_IsEditable : false,
        ZZE_StartDate: null,
        ZZE_EndDate: null
     };
});
