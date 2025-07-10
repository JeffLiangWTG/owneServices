import { IEntityManager, ServiceType } from "../EntityManager";
import { mount, ReactWrapper } from "enzyme";
import React from "react";
import RefShippingLineUserViewSearchForm from "../RefShippingLineUserViewSearchForm";
import { Mock, It } from "typemoq";
import { RefShippingLineUserView } from "../models/RefShippingLineUserView";
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

describe("<RefShippingLineUserViewSearchForm />", () => {
	it("render data", async () => {
		let shippingLine1 = Mock.ofType<RefShippingLineUserView>();
		shippingLine1.setup((x) => x.RSL_PK).returns(() => "1");
		shippingLine1.setup((x) => x.RSL_IsActive).returns(() => true);
		shippingLine1.setup((x) => x.RSL_IsNVO).returns(() => false);
		shippingLine1.setup((x) => x.RSL_IsCW1User).returns(() => false);
		shippingLine1.setup((x) => x.RSL_CargoWiseOneCode).returns(() => "CW11");
		shippingLine1
			.setup((x) => x.RSL_StandardCarrierAlphaCode)
			.returns(() => "SCA1");
		shippingLine1
			.setup((x) => x.RSL_CarrierName)
			.returns(() => "Shipping Line1");
		shippingLine1.setup((x) => x.RSL_EHubIds).returns(() => "");
		shippingLine1
			.setup((x) => x.RSL_OceanCarrierMessagingAvailable)
			.returns(() => false);
		shippingLine1
			.setup((x) => x.RSL_GlobalSailingScheduleAvailable)
			.returns(() => false);
		shippingLine1
			.setup((x) => x.RSL_ContainerAutomationAvailable)
			.returns(() => false);
		shippingLine1
			.setup((x) => x.RSL_CargoSphereRatesAvailable)
			.returns(() => false);
		shippingLine1.setup((x) => x.RSL_InvoiceAvailable).returns(() => false);
		shippingLine1.setup((x) => x.RSL_IsSystem).returns(() => true);

		let shippingLine2 = Mock.ofType<RefShippingLineUserView>();
		shippingLine2.setup((x) => x.RSL_PK).returns(() => "2");
		shippingLine2.setup((x) => x.RSL_IsActive).returns(() => true);
		shippingLine2.setup((x) => x.RSL_IsNVO).returns(() => false);
		shippingLine2.setup((x) => x.RSL_IsCW1User).returns(() => false);
		shippingLine2.setup((x) => x.RSL_CargoWiseOneCode).returns(() => "CW22");
		shippingLine2
			.setup((x) => x.RSL_StandardCarrierAlphaCode)
			.returns(() => "SCA2");
		shippingLine2
			.setup((x) => x.RSL_CarrierName)
			.returns(() => "Shipping Line2");
		shippingLine2.setup((x) => x.RSL_EHubIds).returns(() => "");
		shippingLine2
			.setup((x) => x.RSL_OceanCarrierMessagingAvailable)
			.returns(() => false);
		shippingLine2
			.setup((x) => x.RSL_GlobalSailingScheduleAvailable)
			.returns(() => false);
		shippingLine2
			.setup((x) => x.RSL_ContainerAutomationAvailable)
			.returns(() => false);
		shippingLine2
			.setup((x) => x.RSL_CargoSphereRatesAvailable)
			.returns(() => false);
		shippingLine2.setup((x) => x.RSL_InvoiceAvailable).returns(() => false);
		shippingLine2.setup((x) => x.RSL_IsSystem).returns(() => true);

		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefShippingLineUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() =>
				Promise.resolve([shippingLine1.object, shippingLine2.object])
			);

		let wrapper = mount<typeof RefShippingLineUserViewSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefShippingLineUserViewSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);
		await pressFind(wrapper as any);
		expect(wrapper.find("tbody > tr")).toHaveLength(2);
		expect(wrapper.find(FilterStrip).at(0).key()).toEqual("Carrier Name");
		expect(wrapper.find(FilterStrip).at(1).key()).toEqual("SCAC Code");
		expect(wrapper.find(FilterStrip).at(2).key()).toEqual("CW1 Code");
	});

	it("do not call find when filter context is present but filter is empty", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefShippingLineUserView",
					[ServiceType.Safe],
					[
						new Filter(
							"RSL_CarrierName",
							FilterOps.Equals,
							"" as any,
							"string"
						),
					],
					false
				)
			)
			.returns(() =>
				Promise.resolve([{ RSL_PK: uuid(), RSL_CarrierName: "AAA" }])
			);

		const mockContextValue = {
			filters: [
				new TextFilterModule(
					"Carrier Name",
					new Filter("RSL_CarrierName", FilterOps.Equals, "" as any, "string"),
					75
				),
			],
			setFilters: jest.fn(),
		};

		let wrapper = mount<typeof RefShippingLineUserViewSearchForm>(
			<BrowserRouter>
				<FilterContext.Provider value={mockContextValue}>
					<RefShippingLineUserViewSearchForm
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
					"RefShippingLineUserView",
					[ServiceType.Safe],
					[
						new Filter(
							"RSL_CarrierName",
							FilterOps.Equals,
							"AAA" as any,
							"string"
						),
					],
					false
				)
			)
			.returns(() =>
				Promise.resolve([{ RSL_PK: uuid(), RSL_CarrierName: "AAA" }])
			);

		const mockContextValue = {
			filters: [
				new TextFilterModule(
					"Carrier Name",
					new Filter(
						"RSL_CarrierName",
						FilterOps.Equals,
						"AAA" as any,
						"string"
					),
					75
				),
			],
			setFilters: jest.fn(),
		};

		let wrapper = mount<typeof RefShippingLineUserViewSearchForm>(
			<BrowserRouter>
				<FilterContext.Provider value={mockContextValue}>
					<RefShippingLineUserViewSearchForm
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
					"RefShippingLineUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([]));
		let wrapper = mount<typeof RefShippingLineUserViewSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefShippingLineUserViewSearchForm
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

	it("onFilterValueChanged", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefShippingLineUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([]));
		let wrapper = mount<typeof RefShippingLineUserViewSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefShippingLineUserViewSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);
		const filterStrip = wrapper.find(FilterStrip).at(1);

		let inputToChange = filterStrip.find(TextInput).at(0).find("input").at(0);
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
