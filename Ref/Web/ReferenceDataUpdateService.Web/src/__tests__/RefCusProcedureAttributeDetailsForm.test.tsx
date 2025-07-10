import { shallow } from "enzyme";
import React from "react";
import { Mock } from "typemoq";
import { IEntityManager } from "../EntityManager";
import { RefCusProcedureAttributeUserView } from "../models/RefCusProcedureAttributeUserView";
import { RefCusProcedureAttributeDetailsForm } from "../RefCusProcedureAttributeDetailsForm";
import { TextInput } from "../TextInput";

describe("<RefCusProcedureAttributeDetailsForm />", () => {
    it("render data", () => {
        let entityManager = Mock.ofType<IEntityManager>();
        let wrapper = shallow<RefCusProcedureAttributeDetailsForm>(<RefCusProcedureAttributeDetailsForm attributes={[attr1, attr2]} addNewAttribute={jest.fn} onAttributeValueChange={jest.fn} onAttributeValueChanged={jest.fn} removeAttribute={jest.fn} validationResults={{}} entityManager={entityManager.object} />);

        let textInputs = wrapper.find(TextInput);
        expect(textInputs.length).toEqual(4);
        expect(textInputs.at(0).props().entity).toEqual(attr1);
        expect(textInputs.at(0).props().propertyName).toEqual("ZXB_Name");
        expect(textInputs.at(1).props().entity).toEqual(attr1);
        expect(textInputs.at(1).props().propertyName).toEqual("ZXB_Value");

        expect(textInputs.at(2).props().entity).toEqual(attr2);
        expect(textInputs.at(2).props().propertyName).toEqual("ZXB_Name");
        expect(textInputs.at(3).props().entity).toEqual(attr2);
        expect(textInputs.at(3).props().propertyName).toEqual("ZXB_Value");
    })
});

var attr1: RefCusProcedureAttributeUserView = {
    ZXB_PK: "1",
    ZXB_ZZ6_ProcedureCode: "12345",
    ZXB_Name: "SSGPaymentMethod",
    ZXB_Value: "CASH",
    ZXB_CountryOrGrouping: "GB",
    ZXB_IsEditable: true
};

var attr2: RefCusProcedureAttributeUserView = {
    ZXB_PK: "2",
    ZXB_ZZ6_ProcedureCode: "12345",
    ZXB_Name: "TPFPaymentMethod",
    ZXB_Value: "DEFERRED",
    ZXB_CountryOrGrouping: "GB",
    ZXB_IsEditable: true
}