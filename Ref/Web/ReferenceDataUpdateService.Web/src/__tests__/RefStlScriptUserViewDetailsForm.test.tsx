import React from "react";
import { mount, shallow } from "enzyme";
import { Mock, It, Times } from "typemoq";
import { act } from "react-dom/test-utils";
import { setImmediate } from "timers";
import { CheckBox } from "../CheckBox";
import { TextInput } from "../TextInput";
import { TextSelect } from "../TextSelect";
import { DateTimeInput } from "../DateTimeInput";
import { CollapsibleTextbox } from "../CollapsibleTextbox";
import { IEntityManager, ServiceType } from "../EntityManager";
import { RefStlScriptUserView } from "../models/RefStlScriptUserView";
import { RefStlScriptUserViewDetailsForm } from "../RefStlScriptUserViewDetailsForm";
import { FilterOps } from "../Filter";

describe("<RefStlScriptUserViewDetailsForm />", () => {
	it("Feature Code, Min CW Version, Max CW Version, Active On is read-only", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup((x) => x.isInDatabase(It.isAny())).returns(() => true);

		let wrapper = mount(
			<RefStlScriptUserViewDetailsForm entityManager={entityManager.object} id="12345" />
		);
		await act(() => new Promise(setImmediate));
		wrapper.update();

		expect(wrapper.find(TextInput).at(0).props().readOnly).toBeTruthy();
		expect(wrapper.find(TextInput).at(1).props().readOnly).toBeTruthy();
		expect(wrapper.find(TextInput).at(2).props().readOnly).toBeTruthy();
		expect(wrapper.find(TextSelect).at(0).props().readOnly).toBeTruthy();
	});

	it("render", async () => {
		let entityManager = Mock.ofType<IEntityManager>();

		let wrapper = mount(
			<RefStlScriptUserViewDetailsForm entityManager={entityManager.object} id="12345" />
		);
		await act(() => new Promise(setImmediate));
		wrapper.update();

		expect(wrapper.find(TextInput).at(0).props().propertyName).toEqual(
			"STL_FeatureCode"
		);
		expect(wrapper.find(TextInput).at(1).props().propertyName).toEqual(
			"STL_MinCW1Version"
		);
		expect(wrapper.find(TextInput).at(2).props().propertyName).toEqual(
			"STL_MaxCW1Version"
		);
		expect(wrapper.find(TextSelect).at(0).props().propertyName).toEqual(
			"STL_ActiveOn"
		);
		expect(wrapper.find(CheckBox).at(0).props().propertyName).toEqual(
			"STL_IsPublished"
		);
		expect(wrapper.find(CheckBox).at(1).props().propertyName).toEqual(
			"STL_WithOptionRecompile"
		);
		expect(wrapper.find(CheckBox).at(2).props().propertyName).toEqual(
			"STL_UsedInBilling"
		);
		expect(wrapper.find(TextSelect).at(1).props().propertyName).toEqual(
			"STL_DateType"
		);
		expect(wrapper.find(TextSelect).at(2).props().propertyName).toEqual(
			"STL_DataGranularity"
		);
		expect(wrapper.find(DateTimeInput).at(0).props().propertyName).toEqual(
			"STL_CollectionStartDateUtc"
		);
		expect(wrapper.find(TextInput).at(3).props().propertyName).toEqual(
			"STL_RoleName"
		);
		expect(wrapper.find(TextInput).at(4).props().propertyName).toEqual(
			"STL_ModuleName"
		);
		expect(wrapper.find(TextInput).at(5).props().propertyName).toEqual(
			"STL_FunctionName"
		);
		expect(wrapper.find(TextInput).at(6).props().propertyName).toEqual(
			"STL_FeatureName"
		);
		expect(wrapper.find(CollapsibleTextbox).at(0).props().propertyName).toEqual(
			"STL_CompanyCode"
		);
		expect(wrapper.find(CollapsibleTextbox).at(1).props().propertyName).toEqual(
			"STL_BranchCode"
		);
		expect(wrapper.find(CollapsibleTextbox).at(2).props().propertyName).toEqual(
			"STL_TransactionDateUtc"
		);
		expect(wrapper.find(CollapsibleTextbox).at(3).props().propertyName).toEqual(
			"STL_CreatingUserCode"
		);
		expect(wrapper.find(CollapsibleTextbox).at(4).props().propertyName).toEqual(
			"STL_GuidReference"
		);
		expect(wrapper.find(CollapsibleTextbox).at(5).props().propertyName).toEqual(
			"STL_BillingReference1"
		);
		expect(wrapper.find(CollapsibleTextbox).at(6).props().propertyName).toEqual(
			"STL_BillingReference2"
		);
		expect(wrapper.find(CollapsibleTextbox).at(7).props().propertyName).toEqual(
			"STL_BillingReference3"
		);
		expect(wrapper.find(CollapsibleTextbox).at(8).props().propertyName).toEqual(
			"STL_BillingReference4"
		);
		expect(wrapper.find(CollapsibleTextbox).at(9).props().propertyName).toEqual(
			"STL_AdditionalRefs"
		);
		expect(wrapper.find(CollapsibleTextbox).at(10).props().propertyName).toEqual(
			"STL_TransactionCount"
		);
		expect(wrapper.find(CollapsibleTextbox).at(11).props().propertyName).toEqual(
			"STL_PreparationScript"
		);
		expect(wrapper.find(CollapsibleTextbox).at(12).props().propertyName).toEqual(
			"STL_FromClause"
		);
		expect(wrapper.find(CollapsibleTextbox).at(13).props().propertyName).toEqual(
			"STL_WhereClause"
		);
	});

	it("save", async () => {
		let refStlScripts: RefStlScriptUserView[] = [
			{
				STL_PK: "8E0B13AD-B9D5-FE86-9B61-94D4AFF76E42",
				STL_FeatureCode: "AAA",
				STL_RoleName: "",
				STL_ModuleName: "",
				STL_FunctionName: "",
				STL_FeatureName: "",
				STL_DataGranularity: "TRN",
				STL_CompanyCode: "",
				STL_BranchCode: "",
				STL_TransactionDateUtc: "wd.WD_SystemCreateTimeUtc",
				STL_CreatingUserCode: "",
				STL_GuidReference: "wd.WD_PK",
				STL_BillingReference1: "",
				STL_BillingReference2: "",
				STL_BillingReference3: "",
				STL_BillingReference4: "",
				STL_AdditionalRefs: "",
				STL_TransactionCount: "1",
				STL_PreparationScript: "",
				STL_FromClause: "STL_FromClause",
				STL_WhereClause: "",
				STL_WithOptionRecompile: false,
				STL_UsedInBilling: true,
				STL_ActiveOn: "ALL",
				STL_MinCW1Version: "22.7.21.121",
				STL_MaxCW1Version: "23.9.29.10",
				STL_DateType: "DTE",
				STL_CollectionStartDateUtc: null,
				STL_IsSystem: true,
				STL_IsPublished: true,
				STL_IsEditable: true
			},
		];

		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync<RefStlScriptUserView>(
					"RefStlScriptUserView",
					[ServiceType.Safe],
					It.isAny(),
					false,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve(refStlScripts));
		entityManager
			.setup((x) => x.saveChanges(It.isAny()))
			.returns(() => Promise.resolve({ success: true, message: "" }));

		let wrapper = mount(
			<RefStlScriptUserViewDetailsForm entityManager={entityManager.object} id="123" />
		);
		const textBox = wrapper.find(CollapsibleTextbox).at(0);
		const textInput = textBox.find("textarea");
		textInput.simulate("change", { target: { value: "Hello" } });
		const saveButton = wrapper.find("button");
		saveButton.simulate("mousedown", { button: 0 });

		await act(() => new Promise(setImmediate));
		wrapper.update();
		entityManager.verify(x => x.saveChanges(ServiceType.Safe), Times.once());
	});

	it("calls entityManager.reload on unmount", async () => {
		const reloadMock = jest.fn().mockResolvedValue(undefined);
		const entityManager : IEntityManager = {
			reload: reloadMock,
			clear: jest.fn(),
			isInDatabase: jest.fn().mockReturnValue(false),
			add: jest.fn(),
			getAsync: jest.fn().mockResolvedValue([]),
			update: jest.fn(),
			saveChanges: jest.fn(),
			remove: jest.fn(),
			filterEntities: jest.fn(),
			getTypeFromMetaData: jest.fn(),
			getEntityMetaData: jest.fn(),
			initialise: jest.fn(),
		};

		const wrapper = mount(
			<RefStlScriptUserViewDetailsForm entityManager={entityManager} id="123" />
		);

		await act(() => new Promise(setImmediate));
		wrapper.update();

		wrapper.unmount();

		await act(() => new Promise(setImmediate));

		expect(reloadMock).toHaveBeenCalledWith(
			"RefStlScriptUserView",
			[ServiceType.Safe],
			[expect.objectContaining({
				propertyName: "STL_PK",
				operation: FilterOps.Equals,
				value: "123",
				type: "guid"
			})]
		);
	});
});
