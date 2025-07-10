import { IEntity } from "../models/IEntity";
import React from 'react';
import { shallow } from 'enzyme';
import { PasswordInput } from "../PasswordInput";

describe('PasswordInput', () => {
	it('should render the input and toggle button', () => {
		let dummyPassword: IEntity = { RAA_Value: "Test1234" };
		const wrapper = shallow(<PasswordInput entity={dummyPassword} propertyName="RAA_Value" onValueChange={jest.fn} onValueChanged={jest.fn} maxLength={400} />);
		expect(wrapper.find('input')).toHaveLength(1);
		expect(wrapper.find('.toggle-password')).toHaveLength(1);
	});

	it('should toggle the password visibility on click', () => {
		let dummyPassword: IEntity = { RAA_Value: "abc" };
		const wrapper = shallow(<PasswordInput entity={dummyPassword} propertyName="RAA_Value" onValueChange={jest.fn} onValueChanged={jest.fn} maxLength={400} />);
		expect(wrapper.state('passwordVisible')).toBe(false);
		wrapper.find('.toggle-password').simulate('click');
		expect(wrapper.state('passwordVisible')).toBe(true);
	});

	it('should display the correct icon based on password visibility', () => {
		let dummyPassword: IEntity = { RAA_Value: "abc" };
		const wrapper = shallow(<PasswordInput entity={dummyPassword} propertyName="RAA_Value" onValueChange={jest.fn} onValueChanged={jest.fn} maxLength={400} />);
		expect(wrapper.find('.fa-eye-slash')).toHaveLength(1);
		expect(wrapper.find('.fa-eye')).toHaveLength(0);

		wrapper.find('.toggle-password').simulate('click');
		expect(wrapper.find('.fa-eye')).toHaveLength(1);
		expect(wrapper.find('.fa-eye-slash')).toHaveLength(0);
	});
});
