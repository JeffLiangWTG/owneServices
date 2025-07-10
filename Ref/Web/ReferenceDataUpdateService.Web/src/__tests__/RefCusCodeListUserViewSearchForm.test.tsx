import { IEntityManager, ServiceType } from "../EntityManager";
import { mount } from "enzyme";
import React from "react";
import RefCusCodeListUserViewSearchForm from "../RefCusCodeListUserViewSearchForm";
import { Mock, It } from "typemoq";
import IRefCusCodeType from "../models/IRefCusCodeType";
import { RefCusCodeListUserView } from "../models/RefCusCodeListUserView";
import { FilterStrip } from "../FilterStrip";
import { act } from "react-dom/test-utils";
import { setImmediate } from "timers";
import { BrowserRouter } from "react-router-dom";
import { IEntity } from "../models/IEntity";
import { CodeInput } from "../CodeInput";
import { FilterContext, FilterProvider } from "../FilterContext";
import { Filter, FilterOps } from "../Filter";
import { TextFilterModule } from "../TextFilterModule";
import uuid from "uuid";

describe("<RefCusCodeListUserViewSearchForm />", () => {
	it("render data", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync<RefCusCodeListUserView>(
					"RefCusCodeListUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([]));
		entityManager
			.setup((x) =>
				x.getAsync<IRefCusCodeType>(
					"RefCusCodeType",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([codeType1.object]));
		entityManager
			.setup((x) =>
				x.getAsync<IEntity>(
					"RefDataGrouping",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([dataGrouping.object]));

		let wrapper = mount<typeof RefCusCodeListUserViewSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefCusCodeListUserViewSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);
		expect(wrapper.find(FilterStrip).at(0).key()).toEqual("Code");
		expect(wrapper.find(FilterStrip).at(1).key()).toEqual("Description");
		expect(wrapper.find(FilterStrip).at(2).key()).toEqual("List Type");
		expect(wrapper.find(FilterStrip).at(3).key()).toEqual(
			"Country or Grouping"
		);
	});

	it("do not call find when filter context is present but filter is empty", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefCusCodeListUserView",
					[ServiceType.Safe],
					[new Filter("ZZD_Code", FilterOps.Equals, "" as any, "string")],
					false
				)
			)
			.returns(() => Promise.resolve([{ ZZD_PK: uuid(), ZZD_Code: "AAA" }]));

		const mockContextValue = {
			filters: [
				new TextFilterModule(
					"Code",
					new Filter("ZZD_Code", FilterOps.Equals, "" as any, "string"),
					RefCusCodeListUserView.ZZD_Code_MaxLength
				),
			],
			setFilters: jest.fn(),
		};

		let wrapper = mount<typeof RefCusCodeListUserViewSearchForm>(
			<BrowserRouter>
				<FilterContext.Provider value={mockContextValue}>
					<RefCusCodeListUserViewSearchForm
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
					"RefCusCodeListUserView",
					[ServiceType.Safe],
					[new Filter("ZZD_Code", FilterOps.Equals, "AAA" as any, "string")],
					false
				)
			)
			.returns(() => Promise.resolve([{ ZZD_PK: uuid(), ZZD_Code: "AAA" }]));

		const mockContextValue = {
			filters: [
				new TextFilterModule(
					"Code",
					new Filter("ZZD_Code", FilterOps.Equals, "AAA" as any, "string"),
					RefCusCodeListUserView.ZZD_Code_MaxLength
				),
			],
			setFilters: jest.fn(),
		};

		let wrapper = mount<typeof RefCusCodeListUserViewSearchForm>(
			<BrowserRouter>
				<FilterContext.Provider value={mockContextValue}>
					<RefCusCodeListUserViewSearchForm
						entityManager={entityManager.object}
					/>
				</FilterContext.Provider>
			</BrowserRouter>
		);
		await act(() => new Promise(setImmediate));
		wrapper.update();

		expect(wrapper.find("tbody > tr")).toHaveLength(1);
	});

	it("onValueChange", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync<IRefCusCodeType>(
					"RefCusCodeType",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([codeType1.object]));
		entityManager
			.setup((x) =>
				x.getAsync<IEntity>(
					"RefDataGrouping",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([dataGrouping.object]));
		let wrapper = mount<typeof RefCusCodeListUserViewSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefCusCodeListUserViewSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);
		const firstFilterStrip = wrapper.find(FilterStrip).at(0);
		expect(firstFilterStrip.find("select").at(0).html()).toContain("Equals");
		firstFilterStrip
			.find("select")
			.at(0)
			.simulate("change", { target: { value: 1 } });

		await act(() => new Promise(setImmediate));
		firstFilterStrip.update();

		expect(firstFilterStrip.find("select").at(0).html()).toContain("Contains");
	});

	it("onValueChanged", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync<IRefCusCodeType>(
					"RefCusCodeType",
					[ServiceType.Safe],
					[],
					false
				)
			)
			.returns(() => Promise.resolve([]));
		entityManager
			.setup((x) =>
				x.getAsync<IEntity>(
					"RefDataGrouping",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([dataGrouping.object]));
		let wrapper = mount<typeof RefCusCodeListUserViewSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefCusCodeListUserViewSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);
		const filterStrip = wrapper.find(FilterStrip).at(2);

		let inputToChange = filterStrip.find(CodeInput).at(0).find("input").at(0);
		inputToChange.simulate("change", { target: { value: "AAA" } });
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

let codeType1 = Mock.ofType<IRefCusCodeType>();
codeType1.setup((x) => x.ZZK_PK).returns(() => "1");
codeType1.setup((x) => x.ZZK_CodeType).returns(() => "AA");
codeType1.setup((x) => x.ZZK_Description).returns(() => "ad");
codeType1.setup((x) => x.ZZK_CodeType_MaxLength).returns(() => 2);
codeType1.setup((x) => x.ZZK_Description_MaxLength).returns(() => 2);

let dataGrouping = Mock.ofType<IEntity>();
dataGrouping.setup((x) => x.ZZZ_PK).returns(() => "1");
dataGrouping.setup((x) => x.ZZZ_DataGrouping).returns(() => "BR");
dataGrouping.setup((x) => x.ZZZ_Description).returns(() => "Brazil");
