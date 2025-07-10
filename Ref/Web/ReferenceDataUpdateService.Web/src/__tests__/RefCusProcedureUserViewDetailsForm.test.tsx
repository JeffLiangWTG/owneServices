import { shallow } from "enzyme";
import React from "react";
import { It, Mock } from "typemoq";
import { CheckBox } from "../CheckBox";
import { CodeInput } from "../CodeInput";
import { DateTimeInput } from "../DateTimeInput";
import { IEntityManager, ServiceType } from "../EntityManager";
import { RefCusProcedureAttributeUserView } from "../models/RefCusProcedureAttributeUserView";
import { RefCusProcedureUserView } from "../models/RefCusProcedureUserView";
import { RefCusProcedureUserViewDetailsForm } from "../RefCusProcedureUserViewDetailsForm";
import { TextArea } from "../TextArea";
import { TextInput } from "../TextInput";
import { TextSelect } from "../TextSelect";
import { ValidationServiceWrapper } from "../ValidationService";
import { FilterOps } from "../Filter";

describe("<RefCusProcedureUserViewDetailsForm />", () => {
    it("render data", async () => {
        let entityManager = Mock.ofType<IEntityManager>();
        entityManager.setup(x => x.getAsync("RefCusProcedureUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([procedure]));

        let wrapper = shallow<RefCusProcedureUserViewDetailsForm>(<RefCusProcedureUserViewDetailsForm entityManager={entityManager.object} id="1" />);
        await wrapper.instance().componentDidMount();
        expect(wrapper.find(TextInput).at(0).props().propertyName).toEqual("ZZ6_ProcedureCode");
        expect(wrapper.find(TextInput).at(1).props().propertyName).toEqual("ZZ6_PreviousProcedureCode");
        expect(wrapper.find(TextInput).at(2).props().propertyName).toEqual("ZZ6_Concession");
        expect(wrapper.find(TextArea).at(0).props().propertyName).toEqual("ZZ6_Description");
        expect(wrapper.find(CodeInput).at(0).props().propertyName).toEqual("ZZ6_CountryOrGrouping");
        expect(wrapper.find(TextSelect).at(0).props().propertyName).toEqual("ZZ6_ShipmentType");
        expect(wrapper.find(TextInput).at(3).props().propertyName).toEqual("ZZ6_Group");
        expect(wrapper.find(TextInput).at(4).props().propertyName).toEqual("ZZ6_Category");
        expect(wrapper.find(DateTimeInput).at(0).props().propertyName).toEqual("ZZ6_StartDate");
        expect(wrapper.find(DateTimeInput).at(1).props().propertyName).toEqual("ZZ6_EndDate");
        expect(wrapper.find(CheckBox).at(0).props().propertyName).toEqual("ZZ6_CalculateDuty");
        expect(wrapper.find(CheckBox).at(1).props().propertyName).toEqual("ZZ6_LandedCost");
        expect(wrapper.find(CheckBox).at(2).props().propertyName).toEqual("ZZ6_CalculateVAT");
        expect(wrapper.find(CheckBox).at(3).props().propertyName).toEqual("ZZ6_IsPublished");
        expect(wrapper.find(TextSelect).at(1).props().propertyName).toEqual("ZZ6_IntoWarehouse");
        expect(wrapper.find(TextSelect).at(2).props().propertyName).toEqual("ZZ6_OutOfWarehouse");
        expect(wrapper.find(TextSelect).at(3).props().propertyName).toEqual("ZZ6_IntoInwardProcessing");
        expect(wrapper.find(TextSelect).at(4).props().propertyName).toEqual("ZZ6_OutOfInwardProcessing");
        expect(wrapper.find(TextSelect).at(5).props().propertyName).toEqual("ZZ6_IntoOutwardProcessing");
        expect(wrapper.find(TextSelect).at(6).props().propertyName).toEqual("ZZ6_OutofOutwardProcessing");
        expect(wrapper.find(TextSelect).at(7).props().propertyName).toEqual("ZZ6_IntoTemporaryImport");
        expect(wrapper.find(TextSelect).at(8).props().propertyName).toEqual("ZZ6_OutOfTemporaryImport");
        expect(wrapper.find(TextSelect).at(9).props().propertyName).toEqual("ZZ6_IntoTemporaryExport");
        expect(wrapper.find(TextSelect).at(10).props().propertyName).toEqual("ZZ6_OutOfTemporaryExport");
        expect(wrapper.find(TextSelect).at(11).props().propertyName).toEqual("ZZ6_IsGuaranteeConsumed");
        expect(wrapper.find(TextSelect).at(12).props().propertyName).toEqual("ZZ6_IsGuaranteeReleased");
        expect(wrapper.find(TextSelect).at(13).props().propertyName).toEqual("ZZ6_IsTransit");
    });

    it("onValueChange", async () => {
        let entityManager = Mock.ofType<IEntityManager>();
        let wrapper = shallow<RefCusProcedureUserViewDetailsForm>(<RefCusProcedureUserViewDetailsForm entityManager={entityManager.object} id="1" />);

        await wrapper.instance().componentDidMount();
        await wrapper.instance().onValueChange(null, "ZZ6_ProcedureCode", "40" as any);

        expect(wrapper.state().procedure.ZZ6_ProcedureCode).toEqual("40");
    });

    it("onValueChanged", async () => {
        let entityManager = Mock.ofType<IEntityManager>();
        let wrapper = shallow<RefCusProcedureUserViewDetailsForm>(<RefCusProcedureUserViewDetailsForm entityManager={entityManager.object} id="1" />);
        let procedureValidationService = {
            ZZ6_ProcedureCode : [(e: any, p: any) => "There is an error"]
        };
        let procedureValidationServiceWrapper = new ValidationServiceWrapper([procedureValidationService], wrapper.instance().updateSaveButtonDisabledProperty);
        wrapper.instance().validationServices = procedureValidationServiceWrapper;
        await wrapper.instance().componentDidMount();
        await wrapper.instance().onValueChanged(null, "ZZ6_ProcedureCode");

        expect(wrapper.state().procedureValidationResults["ZZ6_ProcedureCode"]).toEqual(["There is an error"]);
    });

    it("onAttributeValueChange", async () => {
        let entityManager = Mock.ofType<IEntityManager>();
        entityManager.setup(x => x.getAsync<RefCusProcedureUserView>("RefCusProcedureUserView", [ServiceType.Safe], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([procedure]));
        entityManager.setup(x => x.getAsync<RefCusProcedureAttributeUserView>("RefCusProcedureAttributeUserView", [ServiceType.Safe], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attr]));

        let wrapper = shallow<RefCusProcedureUserViewDetailsForm>(<RefCusProcedureUserViewDetailsForm entityManager={entityManager.object} id="1" />);
        await wrapper.instance().componentDidMount();
        await wrapper.instance().onAttributeValueChange(attr, "ZXB_Name", "ABC" as any);

        expect(wrapper.state().attributes[0].ZXB_Name).toEqual("ABC");
    });

    it("onAttributeValueChanged", async () => {
        let entityManager = Mock.ofType<IEntityManager>();
        entityManager.setup(x => x.getAsync<RefCusProcedureUserView>("RefCusProcedureUserView", [ServiceType.Safe], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([procedure]));
        entityManager.setup(x => x.getAsync<RefCusProcedureAttributeUserView>("RefCusProcedureAttributeUserView", [ServiceType.Safe], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([attr]));

        let wrapper = shallow<RefCusProcedureUserViewDetailsForm>(<RefCusProcedureUserViewDetailsForm entityManager={entityManager.object} id="1" />);
        let procedureValidationService = {
            ZZ6_ProcedureCode : [(e: any, p: any) => "There is an error"]
        };
        let attrValidationService = {
            ZXB_Name : [(e: any, p: any) => "There is an error"]
        }
        let attrValidationServiceWrapper = new ValidationServiceWrapper([procedureValidationService, attrValidationService], wrapper.instance().updateSaveButtonDisabledProperty);
        wrapper.instance().validationServices = attrValidationServiceWrapper;
        await wrapper.instance().componentDidMount();
        await wrapper.instance().onAttributeValueChanged(attr, "ZXB_Name");

        expect(wrapper.state().attrValidationResults[attr.ZXB_PK]["ZXB_Name"]).toEqual(["There is an error"]);
    })

	it("calls entityManager.reload for both entities on unmount", async () => {
        const entityManager = Mock.ofType<IEntityManager>();
        const reloadMock = jest.fn().mockResolvedValue(undefined);
        entityManager.setup(x => x.reload(
            "RefCusProcedureUserView",
            [ServiceType.Safe],
            It.isAny()
        )).returns(reloadMock);
        entityManager.setup(x => x.reload(
            "RefCusProcedureAttributeUserView",
            [ServiceType.Safe],
            It.isAny()
        )).returns(reloadMock);

        const wrapper = shallow<RefCusProcedureUserViewDetailsForm>(
            <RefCusProcedureUserViewDetailsForm entityManager={entityManager.object} id="1" />
        );

        await wrapper.instance().componentWillUnmount();

        expect(reloadMock).toHaveBeenCalledWith(
            "RefCusProcedureUserView",
            [ServiceType.Safe],
            [expect.objectContaining({ propertyName: "ZZ6_PK", operation: FilterOps.Equals, value: "1", type: "guid" })]
        );
        expect(reloadMock).toHaveBeenCalledWith(
            "RefCusProcedureAttributeUserView",
            [ServiceType.Safe],
            [expect.objectContaining({ propertyName: "ZXB_ZZ6_ProcedureCode", operation: FilterOps.Equals, value: "1", type: "guid" })]
        );
    });
});

var procedure: RefCusProcedureUserView = {
    ZZ6_PK: "1",
    ZZ6_Category: "01",
    ZZ6_ProcedureCode: "53",
    ZZ6_PreviousProcedureCode: "00",
    ZZ6_Concession: "008",
    ZZ6_Description: "Test",
    ZZ6_CountryOrGrouping: "GB",
    ZZ6_ShipmentType: "IMP",
    ZZ6_CalculateDuty: false,
    ZZ6_Group: "IFD,ISD",
    ZZ6_LandedCost: false,
    ZZ6_IntoWarehouse: "N",
    ZZ6_OutOfWarehouse: "N",
    ZZ6_IntoInwardProcessing: "N",
    ZZ6_OutOfInwardProcessing: "N",
    ZZ6_IntoOutwardProcessing: "N",
    ZZ6_OutofOutwardProcessing: "N",
    ZZ6_IntoTemporaryImport: "N",
    ZZ6_OutOfTemporaryImport: "N",
    ZZ6_IntoTemporaryExport: "N",
    ZZ6_OutOfTemporaryExport: "N",
    ZZ6_StartDate: "1900-01-01T00:00:00Z",
    ZZ6_EndDate: "2079-06-06T23:59:00Z",
    ZZ6_CalculateVAT: true,
    ZZ6_IsGuaranteeConsumed: "N",
    ZZ6_IsGuaranteeReleased: "N",
    ZZ6_IsTransit: "N",
    ZZ6_IsPublished: true,
    ZZ6_IsEditable: true
};

var attr: RefCusProcedureAttributeUserView = {
    ZXB_PK: "123",
	ZXB_ZZ6_ProcedureCode: "1",
	ZXB_Name: "SSGPaymentMethod",
	ZXB_Value: "CASH",
	ZXB_CountryOrGrouping: "GB",
    ZXB_IsEditable: true
};
