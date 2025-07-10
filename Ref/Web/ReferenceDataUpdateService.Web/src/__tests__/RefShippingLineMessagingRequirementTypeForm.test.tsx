import { shallow } from "enzyme";
import uuid from "uuid";
import React from "react";
import { CheckBox } from "../CheckBox";
import IRefShippingLineMessagingRequirement from "../models/IRefShippingLineMessagingRequirement";
import IRefShippingLineMessagingRequirementType from "../models/IRefShippingLineMessagingRequirementType";
import { RefShippingLineMessagingRequirementTypeForm } from "../RefShippingLineMessagingRequirementTypeForm";

describe("<RefShippingLineMessagingRequirementTypeForm />", () => {
    it("render", async () => {
        let requirements: IRefShippingLineMessagingRequirement[] = [{
            RSR_IsEManifest: false,
            RSR_IsVerifiedGrossContainerWeight: false,
            RSR_IsBookingRequest: false,
            RSR_IsShippingInstruction: true,
            RSR_IsShippingOrder: false,
            RSR_PK: uuid(),
            RSR_RSL_ShippingLine: "123",
            RSR_RST_NKType: "123"
        }];

        let wrapper = shallow<RefShippingLineMessagingRequirementTypeForm>(<RefShippingLineMessagingRequirementTypeForm requirementTypes={requirementTypes} requirements={requirements} onMessagingRequirementChange={jest.fn()} />);

        expect(wrapper.find(CheckBox).at(0).exists()).toBe(true);
        expect(wrapper.find(CheckBox).at(0).props().propertyName).toEqual("RSR_IsBookingRequest");
        expect(wrapper.find(CheckBox).at(1).props().propertyName).toEqual("RSR_IsShippingOrder");
        expect(wrapper.find(CheckBox).at(2).props().propertyName).toEqual("RSR_IsShippingInstruction");
        expect(wrapper.find(CheckBox).at(3).props().propertyName).toEqual("RSR_IsEManifest");
        expect(wrapper.find(CheckBox).at(4).props().propertyName).toEqual("RSR_IsVerifiedGrossContainerWeight");
    });

    let requirementTypes: IRefShippingLineMessagingRequirementType[] = [{
        RST_Code: "123",
        RST_Description: "TST1",
        RST_PK: uuid(),
    }]
});