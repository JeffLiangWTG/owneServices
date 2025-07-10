import { IValidationResults } from "../ValidationResults";
import { shallow } from "enzyme";
import { ValidationResultsForm } from "../ValidationResultsForm";
import React from "react";

describe("<ValidationResultsForm>", () => {
    it("render", () => {
        let validationResult : IValidationResults = {
            "ZZK_CodeType" : ["This field is required."]
        };
        let wrapper = shallow<ValidationResultsForm>(<ValidationResultsForm validationResults={[validationResult]} />);
        expect(wrapper.findWhere(x => x.key() == "Entity_0_ZZK_CodeType").at(0).html()).toContain("ZZK_CodeType");
        expect(wrapper.findWhere(x => x.key() == "Entity_0_ZZK_CodeType_0").at(0).html()).toContain("This field is required.");
    });
});