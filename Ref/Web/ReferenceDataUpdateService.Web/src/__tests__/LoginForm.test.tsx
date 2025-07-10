import { mount, shallow } from "enzyme";
import React from "react";
import { LoginForm } from "../LoginForm";
import MsalWrapper from "../MsalWrapper";
import { setImmediate } from "timers";

describe("LoginForm", () => {
	it("renders the Sign In button when no active account is provided", () => {
		MsalWrapper.getInstance = jest.fn().mockReturnValue({
			isAuthenticated: () => {
				return false;
			},
		});

		const wrapper = shallow(<LoginForm />);
		const signInButton = wrapper.find("button");

		expect(signInButton.text()).toBe("Sign In");
	});

	it("renders the Sign Out button and user name when an active account is provided", () => {
		MsalWrapper.getInstance = jest.fn().mockReturnValue({
			getUserName: () => {
				return "Reference.Data";
			},
			isAuthenticated: () => {
				return true;
			},
		});

		const wrapper = shallow(<LoginForm />);
		const signOutButton = wrapper.find("button");
		const userNameText = wrapper.find("span.navbar-text");

		expect(signOutButton.text()).toBe("Sign Out");
		expect(userNameText.text()).toBe("Hello, Reference.Data");
	});

	test("calls loginRedirect with await on button click", async () => {
		let loginRedirectResolved = false;
		const loginRedirectMock = jest.fn(async () => {
			await new Promise(resolve => setTimeout(resolve, 100));
			loginRedirectResolved = true;
		});

		MsalWrapper.getInstance = jest.fn().mockReturnValue({
			isAuthenticated: () => {
				return false;
			},
			loginRedirect: loginRedirectMock,
		});

		const wrapper = mount(<LoginForm />);

		// Get the onClick handler from the Sign In button
		const signInButton = wrapper.find('button');
		const onClickHandler = signInButton.prop('onClick') as Function;
		// Check if the handler is an async function
		// This will fail if we don't have define the function as async
		expect(onClickHandler!.constructor.name).toBe('AsyncFunction');

		// Call the handler
		const handlerPromise = onClickHandler();

		// Verify it returns a promise
		expect(handlerPromise).toBeInstanceOf(Promise);
		// Verify loginRedirect was called
		expect(MsalWrapper.getInstance().loginRedirect).toHaveBeenCalled();
	
		// Before the promise resolves, loginRedirectResolved should be false
		expect(loginRedirectResolved).toBe(false);
	
		// Wait for the handler to complete
		await handlerPromise;
	
		// After the promise resolves, loginRedirectResolved should be true
		// This will fail if we don't have "await" in the handler
		expect(loginRedirectResolved).toBe(true);
	});

	test("calls logoutRedirect with await on button click", async () => {
		let logoutRedirectResolved = false;
		const logoutRedirectMock = jest.fn(async () => {
			await new Promise(resolve => setTimeout(resolve, 100));
			logoutRedirectResolved = true;
		});
		MsalWrapper.getInstance = jest.fn().mockReturnValue({
			getUserName: () => {
				return "Reference.Data";
			},
			isAuthenticated: () => {
				return true;
			},
			logoutRedirect: logoutRedirectMock,
		});

		const wrapper = mount(<LoginForm />);
		// Get the onClick handler from the Sign Out button
		const signInButton = wrapper.find('button');
		const onClickHandler = signInButton.prop('onClick') as Function;
		// Check if the handler is an async function
		// This will fail if we don't have define the function as async
		expect(onClickHandler!.constructor.name).toBe('AsyncFunction');

		// Call the handler
		const handlerPromise = onClickHandler();

		// Verify it returns a promise
		expect(handlerPromise).toBeInstanceOf(Promise);
		// Verify loginRedirect was called
		expect(MsalWrapper.getInstance().logoutRedirect).toHaveBeenCalled();
	
		// Before the promise resolves, loginRedirectResolved should be false
		expect(logoutRedirectResolved).toBe(false);
	
		// Wait for the handler to complete
		await handlerPromise;
	
		// After the promise resolves, loginRedirectResolved should be true
		// This will fail if we don't have "await" in the handler
		expect(logoutRedirectResolved).toBe(true);
	});
});
