import { IEntity } from "../models/IEntity";
import { shallow } from "enzyme";
import { CheckBox } from "../CheckBox";
import { Mock } from "typemoq";
import React, { ChangeEvent } from "react";

describe("CheckBox", () => {
	it("render", () => {
		var codeList: IEntity = { Is_System: true };
		let wrapper = shallow(<CheckBox entity={codeList} label="Is System" propertyName="Is_System" onValueChange={jest.fn} onValueChanged={jest.fn} />);
		expect(wrapper.find("input[type='checkbox']").at(0).html()).toContain("checked");
	});

	it("render hidden", () => {
		var codeList: IEntity = { Is_System: true };
		let wrapper = shallow(<CheckBox entity={codeList} label="Is System" propertyName="Is_System" onValueChange={jest.fn} onValueChanged={jest.fn} isHidden={true} />);
		expect(wrapper.find("input[type='checkbox']").prop("hidden")).toBeTruthy();
	})

	it("onValueChange", () => {
		var codeList: IEntity = { Is_System: true };
		let wrapper = shallow<CheckBox>(<CheckBox entity={codeList} label="Is System" propertyName="Is_System" onValueChange={jest.fn} onValueChanged={jest.fn} />);
		let event = Mock.ofType<ChangeEvent<HTMLInputElement>>();
		event.setup(x => x.target.checked).returns(() => true);
		wrapper.instance().onValueChange(event.object);
		expect(wrapper.instance().valueChanged).toEqual(true);
	});

	it("onValueChanged", () => {
		var codeList: IEntity = { Is_System: true };
		let wrapper = shallow<CheckBox>(<CheckBox entity={codeList} label="Is System" propertyName="Is_System" onValueChange={jest.fn} onValueChanged={jest.fn} />);
		wrapper.instance().valueChanged = true;
		wrapper.instance().onValueChanged();
		expect(wrapper.instance().valueChanged).toEqual(false);
	});

	it("getValueSafe", () => {
		var codeList: IEntity = { Value1: true, Value2: false, Value3: "true", Value4: "false", Value5: "0", Value6: "1", Value7: "", Value8: undefined };
		let wrapper1 = shallow<CheckBox>(<CheckBox entity={codeList} label="Value1" propertyName="Value1" onValueChange={jest.fn} onValueChanged={jest.fn} />);
		let wrapper2 = shallow<CheckBox>(<CheckBox entity={codeList} label="Value2" propertyName="Value2" onValueChange={jest.fn} onValueChanged={jest.fn} />);
		let wrapper3 = shallow<CheckBox>(<CheckBox entity={codeList} label="Value3" propertyName="Value3" onValueChange={jest.fn} onValueChanged={jest.fn} />);
		let wrapper4 = shallow<CheckBox>(<CheckBox entity={codeList} label="Value4" propertyName="Value4" onValueChange={jest.fn} onValueChanged={jest.fn} />);
		let wrapper5 = shallow<CheckBox>(<CheckBox entity={codeList} label="Value5" propertyName="Value5" onValueChange={jest.fn} onValueChanged={jest.fn} />);
		let wrapper6 = shallow<CheckBox>(<CheckBox entity={codeList} label="Value6" propertyName="Value6" onValueChange={jest.fn} onValueChanged={jest.fn} />);
		let wrapper7 = shallow<CheckBox>(<CheckBox entity={codeList} label="Value7" propertyName="Value7" onValueChange={jest.fn} onValueChanged={jest.fn} />);
		let wrapper8 = shallow<CheckBox>(<CheckBox entity={codeList} label="Value8" propertyName="Value8" onValueChange={jest.fn} onValueChanged={jest.fn} />);

		expect(wrapper1.instance().getValueSafe()).toEqual(true);
		expect(wrapper2.instance().getValueSafe()).toEqual(false);
		expect(wrapper3.instance().getValueSafe()).toEqual(true);
		expect(wrapper4.instance().getValueSafe()).toEqual(false);
		expect(wrapper5.instance().getValueSafe()).toEqual(false);
		expect(wrapper6.instance().getValueSafe()).toEqual(true);
		expect(wrapper7.instance().getValueSafe()).toEqual(false);
		expect(wrapper8.instance().getValueSafe()).toEqual(false);
	});
});
