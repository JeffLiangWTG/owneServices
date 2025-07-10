import { IEntityManager, ServiceType } from "../EntityManager";
import { mount, ReactWrapper } from "enzyme";
import React from "react";
import RefStlScriptUserViewSearchForm from "../RefStlScriptUserViewSearchForm";
import { Mock, It } from "typemoq";
import { RefStlScriptUserView } from "../models/RefStlScriptUserView";
import { FilterStrip } from "../FilterStrip";
import { act } from "react-dom/test-utils";
import { BrowserRouter } from "react-router-dom";
import { setImmediate } from "timers";
import { TextInput } from "../TextInput";
import Button from "../Button";
import { FilterContext, FilterProvider } from "../FilterContext";
import { Filter, FilterOps } from "../Filter";
import uuid from "uuid";
import { TextFilterModule } from "../TextFilterModule";

const pressFind = async (wrapper: ReactWrapper) => {
	//press find
	wrapper
		.findWhere((x) => x.is(Button) && x.text() === "Find")
		.simulate("mousedown", { button: 0 });
	await act(() => new Promise(setImmediate));
	wrapper.update();
};

describe("<RefStlScriptUserViewSearchForm />", () => {
	it("render data", async () => {
		let stlScript1 = Mock.ofType<RefStlScriptUserView>();
		stlScript1.setup((x) => x.STL_PK).returns(() => "3cb5cbde-49e6-45a7-9eec-008a754ee66f");
		stlScript1.setup((x) => x.STL_FeatureCode).returns(() => "NBK");
		stlScript1.setup((x) => x.STL_RoleName).returns(() => "Forwarding Consolidation");
		stlScript1.setup((x) => x.STL_MinCW1Version).returns(() => "22.7.21.121");
		stlScript1.setup((x) => x.STL_MaxCW1Version).returns(() => "23.9.29.10");
		stlScript1.setup((x) => x.STL_ActiveOn).returns(() => "ALL");
		stlScript1.setup((x) => x.STL_IsPublished).returns(() => true);
		
		let stlScript2 = Mock.ofType<RefStlScriptUserView>();
		stlScript2.setup((x) => x.STL_PK).returns(() => "d9ecd693-c457-4e5a-adb4-006b7a649de7");
		stlScript2.setup((x) => x.STL_FeatureCode).returns(() => "ULC");
		stlScript2.setup((x) => x.STL_RoleName).returns(() => "Customs & Country Specific Integrations");
		stlScript2.setup((x) => x.STL_MinCW1Version).returns(() => "");
		stlScript2.setup((x) => x.STL_MaxCW1Version).returns(() => "");
		stlScript2.setup((x) => x.STL_ActiveOn).returns(() => "ALL");
		stlScript2.setup((x) => x.STL_IsPublished).returns(() => false);
					
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefStlScriptUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() =>
				Promise.resolve([stlScript1.object, stlScript2.object])
			);

		let wrapper = mount<typeof RefStlScriptUserViewSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefStlScriptUserViewSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);
		await pressFind(wrapper as any);
		expect(wrapper.find("tbody > tr")).toHaveLength(2);
		expect(wrapper.find(FilterStrip).at(0).key()).toEqual("Feature Code");
		expect(wrapper.find(FilterStrip).at(1).key()).toEqual("Role Name");
	});

	it("do not call find when filter context is present but filter is empty", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefStlScriptUserView",
					[ServiceType.Safe],
					[
						new Filter(
							"STL_FeatureCode",
							FilterOps.Equals,
							"" as any,
							"string"
						),
					],
					false
				)
			)
			.returns(() =>
				Promise.resolve([{ STL_PK: uuid(), STL_FeatureCode: "WIN" }])
			);

		const mockContextValue = {
			filters: [
				new TextFilterModule(
					"Feature Code",
					new Filter("STL_FeatureCode", FilterOps.Equals, "" as any, "string"),
					3
				),
			],
			setFilters: jest.fn(),
		};

		let wrapper = mount<typeof RefStlScriptUserViewSearchForm>(
			<BrowserRouter>
				<FilterContext.Provider value={mockContextValue}>
					<RefStlScriptUserViewSearchForm
						entityManager={entityManager.object}
					/>
				</FilterContext.Provider>
			</BrowserRouter>
		);
		await act(() => new Promise(setImmediate));
		wrapper.update();

		expect(wrapper.find("tbody > tr")).toHaveLength(0);
	});

	it("calls find when filter context is present", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefStlScriptUserView",
					[ServiceType.Safe],
					[
						new Filter(
							"STL_FeatureCode",
							FilterOps.Equals,
							"WIN" as any,
							"string"
						),
					],
					false
				)
			)
			.returns(() =>
				Promise.resolve([{ STL_PK: uuid(), STL_FeatureCode: "WIN" }])
			);

		const mockContextValue = {
			filters: [
				new TextFilterModule(
					"Feature Code",
					new Filter(
						"STL_FeatureCode",
						FilterOps.Equals,
						"WIN" as any,
						"string"
					),
					3
				),
			],
			setFilters: jest.fn(),
		};

		let wrapper = mount<typeof RefStlScriptUserViewSearchForm>(
			<BrowserRouter>
				<FilterContext.Provider value={mockContextValue}>
					<RefStlScriptUserViewSearchForm
						entityManager={entityManager.object}
					/>
				</FilterContext.Provider>
			</BrowserRouter>
		);
		await act(() => new Promise(setImmediate));
		wrapper.update();

		expect(wrapper.find("tbody > tr")).toHaveLength(1);
	});

	it("onFilterValueChange", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefStlScriptUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([]));
		let wrapper = mount<typeof RefStlScriptUserViewSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefStlScriptUserViewSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);

		const firstFilterStrip = wrapper.find(FilterStrip).at(1);
		expect(firstFilterStrip.find("select").at(0).html()).toContain("Equals");
		firstFilterStrip
			.find("select")
			.at(0)
			.simulate("change", { target: { value: 1 } });

		await act(() => new Promise(setImmediate));
		firstFilterStrip.update();

		expect(firstFilterStrip.find("select").at(0).html()).toContain("Contains");
	});

	it("onFilterValueChanged", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefStlScriptUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([]));
		let wrapper = mount<typeof RefStlScriptUserViewSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefStlScriptUserViewSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);
		const filterStrip = wrapper.find(FilterStrip).at(0);

		let inputToChange = filterStrip.find(TextInput).at(0).find("input").at(0);
		inputToChange.simulate("change", { target: { value: "NBK" } });
		await act(() => new Promise(setImmediate));
		wrapper.update();
		inputToChange.simulate("blur");
		await act(() => new Promise(setImmediate));
		wrapper.update();

		let errors = wrapper
			.find("small")
			.findWhere((x) => x.hasClass("text-danger"));

		expect(errors.text()).toEqual("This value is not in the list");
	});
});
