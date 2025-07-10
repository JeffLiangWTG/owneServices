import { shallow } from "enzyme";
import React from "react";
import { AuthCallbackForm } from "../AuthCallbackForm";

describe("AuthCallbackForm", () => {
	it("renders the welcome message correctly", () => {
		const wrapper = shallow(<AuthCallbackForm />);
		const welcomeMessage = wrapper.find("h1");
		expect(welcomeMessage.text()).toBe("Welcome to Reference Service");
	});
});
