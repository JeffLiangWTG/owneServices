import { shallow } from "enzyme";
import { RefCusCodeListAttributeDetailsForm } from "../RefCusCodeListAttributeDetailsForm";
import { RefCusCodeListAttributeUserView } from "../models/RefCusCodeListAttributeUserView";
import React from "react";
import { Mock, It, Times } from "typemoq";
import { IEntityManager, ServiceType } from "../EntityManager";
import { CodeInput } from "../CodeInput";
import IRefCusCodeListAttributeName from "../models/IRefCusCodeListAttributeName";
import { RefCusCodeListUserView } from "../models/RefCusCodeListUserView";
import { DateTimeInput } from "../DateTimeInput";
import { act } from "react-dom/test-utils";
import { setImmediate } from "timers";

describe("<RefCusCodeListAttributeDetailsForm />", () => {
	it("render", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		let codeListMock = Mock.ofType<RefCusCodeListUserView>();
		let wrapper = shallow<RefCusCodeListAttributeDetailsForm>(
			<RefCusCodeListAttributeDetailsForm
				attributes={[attr1, attr2]}
				addNewAttribute={jest.fn}
				onAttributeValueChange={jest.fn}
				onAttributeValueChanged={jest.fn}
				removeAttribute={jest.fn}
				validationResults={{}}
				entityManager={entityManager.object}
				codeList={codeListMock.object}
			/>
		);

		let codeInputs = wrapper.find(CodeInput);
		expect(codeInputs.length).toEqual(4);
		expect(codeInputs.at(0).props().entity).toEqual(attr1);
		expect(codeInputs.at(0).props().propertyName).toEqual("ZZE_ZXE_NKName");
		expect(codeInputs.at(1).props().entity).toEqual(attr1);
		expect(codeInputs.at(1).props().propertyName).toEqual("ZZE_Value");

		expect(codeInputs.at(2).props().entity).toEqual(attr2);
		expect(codeInputs.at(2).props().propertyName).toEqual("ZZE_ZXE_NKName");
		expect(codeInputs.at(3).props().entity).toEqual(attr2);
		expect(codeInputs.at(3).props().propertyName).toEqual("ZZE_Value");

		wrapper.find(CodeInput).at(1).parent().simulate("focus", "1", "Code");
		await simulateAsyncFunctionAwaiter();

		//default codeListAttribute won't render Start/End dates when code input is not focused.
		let dateTimeInputs = wrapper.find(DateTimeInput);
		expect(dateTimeInputs.length).toEqual(0);
	});

	it("render start and end date components", async () => {	
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync<IRefCusCodeListAttributeName>(
					"RefCusCodeListAttributeName",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([attributeNameIsDateRangeUsed]));
		let wrapper = shallow<RefCusCodeListAttributeDetailsForm>(
			<RefCusCodeListAttributeDetailsForm
				attributes={[attrIsDateRangeUsed]}
				addNewAttribute={jest.fn}
				onAttributeValueChange={jest.fn}
				onAttributeValueChanged={jest.fn}
				removeAttribute={jest.fn}
				validationResults={{}}
				entityManager={entityManager.object}
				codeList={codeList}
			/>
		);

		wrapper.find(CodeInput).at(0).parent().simulate("focus", "1", "Code");
		await act(() => new Promise(setImmediate));

		//default codeListAttribute will render Start/End dates when code input is focused.
		let dateTimeInputs = wrapper.find(DateTimeInput);
		expect(dateTimeInputs.length).toEqual(2);
		expect(dateTimeInputs.at(0).props().entity).toEqual(attrIsDateRangeUsed);
		expect(dateTimeInputs.at(0).props().propertyName).toEqual("ZZE_StartDate");
		expect(dateTimeInputs.at(1).props().entity).toEqual(attrIsDateRangeUsed);
		expect(dateTimeInputs.at(1).props().propertyName).toEqual("ZZE_EndDate");
	});

	it("onComponentDidUpdate", async () => {
		let codeListWithDifferentCountry: RefCusCodeListUserView = {
			ZZD_PK: "12345",
			ZZD_Code: "DD",
			ZZD_CodeType: "TST",
			ZZD_CountryOrGrouping: "ZA",
			ZZD_Description: "A",
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
			ZZD_IsEditable: false,
		};

		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefCusCodeListUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([codeList]));
		let wrapper = shallow<RefCusCodeListAttributeDetailsForm>(
			<RefCusCodeListAttributeDetailsForm
				attributes={[attr1, attr2]}
				addNewAttribute={jest.fn}
				onAttributeValueChange={jest.fn}
				onAttributeValueChanged={jest.fn}
				removeAttribute={jest.fn}
				validationResults={{}}
				entityManager={entityManager.object}
				codeList={codeList}
			/>
		);
		expect(wrapper.instance().hasLoadedAttributeNamesOnce).toBe(false);
		wrapper.find(CodeInput).at(1).parent().simulate("focus", "1", "Code");
		await simulateAsyncFunctionAwaiter();
		entityManager.verify(
			(x) =>
				x.getAsync<IRefCusCodeListAttributeName>(
					"RefCusCodeListAttributeName",
					[ServiceType.Safe],
					It.isAny(),
					false
				),
			Times.atLeastOnce()
		);
		expect(wrapper.instance().hasLoadedAttributeNamesOnce).toBe(true);

		entityManager.reset();

		codeList.ZZD_Description = "A";
		wrapper.setProps({ codeList: codeList });
		entityManager.verify(
			(x) =>
				x.getAsync<IRefCusCodeListAttributeName>(
					"RefCusCodeListAttributeName",
					[ServiceType.Safe],
					It.isAny(),
					false
				),
			Times.never()
		);
		wrapper.setProps({ codeList: codeListWithDifferentCountry });
		entityManager.verify(
			(x) =>
				x.getAsync<IRefCusCodeListAttributeName>(
					"RefCusCodeListAttributeName",
					[ServiceType.Safe],
					It.isAny(),
					false
				),
			Times.atLeastOnce()
		);
	});

	it("onCurrentIdxChange", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefCusCodeListUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([codeList]));
		let codeListMock = Mock.ofType<RefCusCodeListUserView>();
		let wrapper = shallow<RefCusCodeListAttributeDetailsForm>(
			<RefCusCodeListAttributeDetailsForm
				attributes={[attr1, attr2]}
				addNewAttribute={jest.fn}
				onAttributeValueChange={jest.fn}
				onAttributeValueChanged={jest.fn}
				removeAttribute={jest.fn}
				validationResults={{}}
				entityManager={entityManager.object}
				codeList={codeListMock.object}
			/>
		);
		wrapper
			.instance()
			.setState({ attributeNames: [attributeName1, attributeName2] });

		wrapper
			.find(CodeInput)
			.at(1)
			.parent()
			.simulate("focus", "1", attr1.ZZE_ZXE_NKName);
		await simulateAsyncFunctionAwaiter();
		expect(wrapper.state().currentId).toEqual("1");
		expect(wrapper.state().currentValueList).not.toBeNull();
		expect(wrapper.state().currentValueList.length).toEqual(1);

		wrapper
			.find(CodeInput)
			.at(2)
			.parent()
			.simulate("focus", "2", attr2.ZZE_ZXE_NKName);
		await simulateAsyncFunctionAwaiter();
		expect(wrapper.state().currentId).toEqual("2");
		expect(wrapper.state().currentValueList).toEqual([]);
	});

	it("onCurrentIdxChangeDiv", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefCusCodeListUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([codeList]));
		let codeListMock = Mock.ofType<RefCusCodeListUserView>();
		let wrapper = shallow<RefCusCodeListAttributeDetailsForm>(
			<RefCusCodeListAttributeDetailsForm
				attributes={[attr1, attr2]}
				addNewAttribute={jest.fn}
				onAttributeValueChange={jest.fn}
				onAttributeValueChanged={jest.fn}
				removeAttribute={jest.fn}
				validationResults={{}}
				entityManager={entityManager.object}
				codeList={codeListMock.object}
			/>
		);
		wrapper
			.instance()
			.setState({ attributeNames: [attributeName1, attributeName2] });

		wrapper
			.find("div#attribute-0")
			.at(0)
			.simulate("click", "1");
		await simulateAsyncFunctionAwaiter();
		expect(wrapper.state().currentId).toEqual("1");

		wrapper
			.find("div#attribute-1")
			.at(0)
			.simulate("click", "2");
		await simulateAsyncFunctionAwaiter();
		expect(wrapper.state().currentId).toEqual("2");
	});

	function simulateAsyncFunctionAwaiter() {
		return new Promise((resolve) => {
			setTimeout(resolve, 0);
		});
	}

	it("getCodeTypeForValueListFromAttributeNamesList", () => {
		let entityManager = Mock.ofType<IEntityManager>();
		let codeListMock = Mock.ofType<RefCusCodeListUserView>();
		let wrapper = shallow<RefCusCodeListAttributeDetailsForm>(
			<RefCusCodeListAttributeDetailsForm
				attributes={[attr1, attr2]}
				addNewAttribute={jest.fn}
				onAttributeValueChange={jest.fn}
				onAttributeValueChanged={jest.fn}
				removeAttribute={jest.fn}
				validationResults={{}}
				entityManager={entityManager.object}
				codeList={codeListMock.object}
			/>
		);
		wrapper
			.instance()
			.setState({ attributeNames: [attributeName1, attributeName2] });
		expect(
			wrapper
				.instance()
				.getCodeTypeForValueListFromAttributeNamesList(attr1.ZZE_ZXE_NKName)
		).toEqual("TST");
		expect(
			wrapper
				.instance()
				.getCodeTypeForValueListFromAttributeNamesList(attr2.ZZE_ZXE_NKName)
		).toEqual("");
	});

	var attributeName1: IRefCusCodeListAttributeName = {
		ZXE_PK: "123",
		ZXE_Name: "Code",
		ZXE_Description: "",
		ZXE_ZZK_NKCodeType: "TST",
		ZXE_ZZZ_NKDataGrouping: "ZA",
		ZXE_IsMandatory: false,
		ZXE_AllowDuplicates: false,
		ZXE_IsValueMandatory: false,
		ZXE_ZZK_NKCodeTypeForValueList: "TST",
		ZXE_ValueDataType: "string",
		ZXE_MinLengthOrValue: 0,
		ZXE_MaxLengthOrValue: 1,
		ZXE_DecimalPlaces: 1,
		ZXE_ColumnCaption: "",
		ZXE_IsDateRangeUsed: false,
	};

	var attributeName2: IRefCusCodeListAttributeName = {
		ZXE_PK: "456",
		ZXE_Name: "Code2",
		ZXE_Description: "",
		ZXE_ZZK_NKCodeType: "AB",
		ZXE_ZZZ_NKDataGrouping: "ZA",
		ZXE_IsMandatory: false,
		ZXE_AllowDuplicates: false,
		ZXE_IsValueMandatory: false,
		ZXE_ZZK_NKCodeTypeForValueList: null as any,
		ZXE_ValueDataType: "string",
		ZXE_MinLengthOrValue: 0,
		ZXE_MaxLengthOrValue: 1,
		ZXE_DecimalPlaces: 1,
		ZXE_ColumnCaption: "",
		ZXE_IsDateRangeUsed: false,
	};

	var attr1: RefCusCodeListAttributeUserView = {
		ZZE_PK: "1",
		ZZE_ZXE_NKName: attributeName1.ZXE_Name,
		ZZE_Value: "12",
		ZZE_IsAir: true,
		ZZE_IsFix: false,
		ZZE_IsInw: false,
		ZZE_IsMai: false,
		ZZE_IsRai: false,
		ZZE_IsRoa: false,
		ZZE_IsSea: false,
		ZZE_ZZD_CodeList: "90",
		ZZE_CodeType: "AB",
		ZZE_CountryOrGrouping: "ZA",
		ZZE_IsEditable: false,
      ZZE_StartDate: null,
      ZZE_EndDate: null
	};
	var attr2: RefCusCodeListAttributeUserView = {
		ZZE_PK: "2",
		ZZE_ZXE_NKName: attributeName2.ZXE_Name,
		ZZE_Value: "21",
		ZZE_IsAir: false,
		ZZE_IsFix: false,
		ZZE_IsInw: false,
		ZZE_IsMai: false,
		ZZE_IsRai: false,
		ZZE_IsRoa: false,
		ZZE_IsSea: true,
		ZZE_ZZD_CodeList: "90",
		ZZE_CodeType: "AB",
		ZZE_CountryOrGrouping: "ZA",
		ZZE_IsEditable: false,
      ZZE_StartDate: null,
      ZZE_EndDate: null
	};

	var codeList: RefCusCodeListUserView = {
		ZZD_PK: "12345",
		ZZD_Code: "DD",
		ZZD_CodeType: "TST",
		ZZD_CountryOrGrouping: "BR",
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
		ZZD_IsEditable: false,
	};

   let attributeNameIsDateRangeUsed: IRefCusCodeListAttributeName = {
      ZXE_PK: "123",
      ZXE_Name: "Code",
      ZXE_Description: "",
      ZXE_ZZK_NKCodeType: "TST",
      ZXE_ZZZ_NKDataGrouping: "ZA",
      ZXE_IsMandatory: false,
      ZXE_AllowDuplicates: false,
      ZXE_IsValueMandatory: false,
      ZXE_ZZK_NKCodeTypeForValueList: "TST",
      ZXE_ValueDataType: "string",
      ZXE_MinLengthOrValue: 0,
      ZXE_MaxLengthOrValue: 1,
      ZXE_DecimalPlaces: 1,
      ZXE_ColumnCaption: "",
      ZXE_IsDateRangeUsed: true,
   };
   let attrIsDateRangeUsed: RefCusCodeListAttributeUserView = {
      ZZE_PK: "1",
      ZZE_ZXE_NKName: attributeNameIsDateRangeUsed.ZXE_Name,
      ZZE_Value: "12",
      ZZE_IsAir: true,
      ZZE_IsFix: false,
      ZZE_IsInw: false,
      ZZE_IsMai: false,
      ZZE_IsRai: false,
      ZZE_IsRoa: false,
      ZZE_IsSea: false,
      ZZE_ZZD_CodeList: "90",
      ZZE_CodeType: "AB",
      ZZE_CountryOrGrouping: "ZA",
      ZZE_IsEditable: false,
      ZZE_StartDate: "2024-09-09",
      ZZE_EndDate: "2024-10-10"
   };
});
