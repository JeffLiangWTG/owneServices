import React from 'react';
import { shallow } from 'enzyme';
import Button from '../Button';

describe('Button', () => {
	it('renders with default props', () => {
		const wrapper = shallow(<Button className='btn btn-info' disabled={false} hidden={false} onClick={jest.fn()} />);

		const button = wrapper.find('button');
		expect(button).toHaveLength(1);
		expect(button.prop('className')).toBe('btn btn-info');
		expect(button.prop('disabled')).toBe(false);
		expect(button.prop('hidden')).toBe(false);
	});

	it('renders children correctly', () => {
		const wrapper = shallow(<Button onClick={jest.fn()}>Click Me</Button>);

		const button = wrapper.find('button');
		expect(button.text()).toBe('Click Me');
	});

	it('calls onClick when clicked', (done) => {
		const handleClick = jest.fn(() => {
			done();
		});
		const wrapper = shallow(<Button onClick={handleClick}>Save Me</Button>);
		const button = wrapper.find('button');
		button.simulate('mouseDown', { button: 0 });
		setTimeout(() => {
			expect(handleClick).toHaveBeenCalledTimes(1);
			done();
		}, 0);
	});

	it('does not call onClick when disabled', () => {
		const handleClick = jest.fn();
		const wrapper = shallow(<Button onClick={handleClick} disabled={true}>Click Me</Button>);

		const button = wrapper.find('button');
		button.simulate('mouseDown', { button: 0 });

		expect(handleClick).not.toHaveBeenCalled();
	});

	it('renders with title', () => {
		const title = "This is a save button";
		const wrapper = shallow(<Button onClick={jest.fn()} title={title} />);

		const button = wrapper.find('button');
		expect(button.prop('title')).toBe(title);
	});

	it('renders hidden button', () => {
		const wrapper = shallow(<Button onClick={jest.fn()} hidden={true} />);

		const button = wrapper.find('button');
		expect(button.prop('hidden')).toBe(true);
	});

	it('renders with custom className', () => {
		const customClass = 'custom-class';
		const wrapper = shallow(<Button onClick={jest.fn()} className={customClass} />);

		const button = wrapper.find('button');
		expect(button.prop('className')).toBe(`${customClass}`);
	});
});
