import { mount, ReactWrapper } from "enzyme";
import React from "react";
import { It, Mock } from "typemoq";
import { IEntityManager, ServiceType } from "../EntityManager";
import { FilterStrip } from "../FilterStrip";
import { RefCusProcedureUserView } from "../models/RefCusProcedureUserView";
import RefCusProcedureUserViewSearchForm from "../RefCusProcedureUserViewSearchForm";
import { setImmediate } from "timers";
import { act } from "react-dom/test-utils";
import { BrowserRouter } from "react-router-dom";
import { IEntity } from "../models/IEntity";
import { CodeInput } from "../CodeInput";
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

describe("<RefCusProcedureUserViewSearchForm />", () => {
	it("render data", async () => {
		let procedure1 = Mock.ofType<RefCusProcedureUserView>();
		procedure1.setup((x) => x.ZZ6_PK).returns(() => "1");
		procedure1.setup((x) => x.ZZ6_Category).returns(() => "01");
		procedure1.setup((x) => x.ZZ6_ProcedureCode).returns(() => "53");
		procedure1.setup((x) => x.ZZ6_PreviousProcedureCode).returns(() => "00");
		procedure1.setup((x) => x.ZZ6_Concession).returns(() => "008");
		procedure1.setup((x) => x.ZZ6_Description).returns(() => "Test");
		procedure1.setup((x) => x.ZZ6_CountryOrGrouping).returns(() => "GB");
		procedure1.setup((x) => x.ZZ6_ShipmentType).returns(() => "IMP");
		procedure1.setup((x) => x.ZZ6_CalculateDuty).returns(() => false);
		procedure1.setup((x) => x.ZZ6_Group).returns(() => "IFD,ISD");
		procedure1.setup((x) => x.ZZ6_LandedCost).returns(() => false);
		procedure1.setup((x) => x.ZZ6_IntoWarehouse).returns(() => "N");
		procedure1.setup((x) => x.ZZ6_OutOfWarehouse).returns(() => "N");
		procedure1.setup((x) => x.ZZ6_IntoInwardProcessing).returns(() => "N");
		procedure1.setup((x) => x.ZZ6_OutOfInwardProcessing).returns(() => "N");
		procedure1.setup((x) => x.ZZ6_IntoOutwardProcessing).returns(() => "N");
		procedure1.setup((x) => x.ZZ6_OutofOutwardProcessing).returns(() => "N");
		procedure1.setup((x) => x.ZZ6_IntoTemporaryImport).returns(() => "N");
		procedure1.setup((x) => x.ZZ6_OutOfTemporaryImport).returns(() => "N");
		procedure1.setup((x) => x.ZZ6_IntoTemporaryExport).returns(() => "N");
		procedure1.setup((x) => x.ZZ6_OutOfTemporaryExport).returns(() => "N");
		procedure1
			.setup((x) => x.ZZ6_StartDate)
			.returns(() => "1900-01-01T00:00:00Z");
		procedure1
			.setup((x) => x.ZZ6_EndDate)
			.returns(() => "2079-06-06T23:59:00Z");
		procedure1.setup((x) => x.ZZ6_CalculateVAT).returns(() => true);
		procedure1.setup((x) => x.ZZ6_IsGuaranteeConsumed).returns(() => "N");
		procedure1.setup((x) => x.ZZ6_IsGuaranteeReleased).returns(() => "N");
		procedure1.setup((x) => x.ZZ6_IsTransit).returns(() => "N");

		let procedure2 = Mock.ofType<RefCusProcedureUserView>();
		procedure2.setup((x) => x.ZZ6_PK).returns(() => "2");
		procedure2.setup((x) => x.ZZ6_Category).returns(() => "01");
		procedure2.setup((x) => x.ZZ6_ProcedureCode).returns(() => "40");
		procedure2.setup((x) => x.ZZ6_PreviousProcedureCode).returns(() => "00");
		procedure2.setup((x) => x.ZZ6_Concession).returns(() => "007");
		procedure2.setup((x) => x.ZZ6_Description).returns(() => "Test");
		procedure2.setup((x) => x.ZZ6_CountryOrGrouping).returns(() => "CDS");
		procedure2.setup((x) => x.ZZ6_ShipmentType).returns(() => "EXP");
		procedure2.setup((x) => x.ZZ6_CalculateDuty).returns(() => false);
		procedure2.setup((x) => x.ZZ6_Group).returns(() => "IFD,ISD");
		procedure2.setup((x) => x.ZZ6_LandedCost).returns(() => false);
		procedure2.setup((x) => x.ZZ6_IntoWarehouse).returns(() => "N");
		procedure2.setup((x) => x.ZZ6_OutOfWarehouse).returns(() => "N");
		procedure2.setup((x) => x.ZZ6_IntoInwardProcessing).returns(() => "N");
		procedure2.setup((x) => x.ZZ6_OutOfInwardProcessing).returns(() => "N");
		procedure2.setup((x) => x.ZZ6_IntoOutwardProcessing).returns(() => "N");
		procedure2.setup((x) => x.ZZ6_OutofOutwardProcessing).returns(() => "N");
		procedure2.setup((x) => x.ZZ6_IntoTemporaryImport).returns(() => "N");
		procedure2.setup((x) => x.ZZ6_OutOfTemporaryImport).returns(() => "N");
		procedure2.setup((x) => x.ZZ6_IntoTemporaryExport).returns(() => "N");
		procedure2.setup((x) => x.ZZ6_OutOfTemporaryExport).returns(() => "N");
		procedure2
			.setup((x) => x.ZZ6_StartDate)
			.returns(() => "1900-01-01T00:00:00Z");
		procedure2
			.setup((x) => x.ZZ6_EndDate)
			.returns(() => "2079-06-06T23:59:00Z");
		procedure2.setup((x) => x.ZZ6_CalculateVAT).returns(() => true);
		procedure2.setup((x) => x.ZZ6_IsGuaranteeConsumed).returns(() => "N");
		procedure2.setup((x) => x.ZZ6_IsGuaranteeReleased).returns(() => "N");
		procedure2.setup((x) => x.ZZ6_IsTransit).returns(() => "N");

		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefCusProcedureUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([procedure1.object, procedure2.object]));
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

		let wrapper = mount<typeof RefCusProcedureUserViewSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefCusProcedureUserViewSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);
		await pressFind(wrapper as any);
		expect(wrapper.find("tbody > tr")).toHaveLength(2);
		expect(wrapper.find(FilterStrip).at(0).key()).toEqual("Procedure Code");
		expect(wrapper.find(FilterStrip).at(1).key()).toEqual(
			"Previous Procedure Code"
		);
		expect(wrapper.find(FilterStrip).at(2).key()).toEqual("Concession");
		expect(wrapper.find(FilterStrip).at(3).key()).toEqual("Shipment Type");
		expect(wrapper.find(FilterStrip).at(4).key()).toEqual(
			"Country or Grouping"
		);
	});

	it("do not call find when filter context is present but filter is empty", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"RefCusProcedureUserView",
					[ServiceType.Safe],
					[
						new Filter(
							"ZZ6_ProcedureCode",
							FilterOps.Equals,
							"" as any,
							"string"
						),
					],
					false
				)
			)
			.returns(() =>
				Promise.resolve([{ ZZ6_PK: uuid(), ZZ6_ProcedureCode: "AAA" }])
			);

		const mockContextValue = {
			filters: [
				new TextFilterModule(
					"Procedure Code",
					new Filter(
						"ZZ6_ProcedureCode",
						FilterOps.Equals,
						"" as any,
						"string"
					),
					5
				),
			],
			setFilters: jest.fn(),
		};

		let wrapper = mount<typeof RefCusProcedureUserViewSearchForm>(
			<BrowserRouter>
				<FilterContext.Provider value={mockContextValue}>
					<RefCusProcedureUserViewSearchForm
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
					"RefCusProcedureUserView",
					[ServiceType.Safe],
					[
						new Filter(
							"ZZ6_ProcedureCode",
							FilterOps.Equals,
							"AAA" as any,
							"string"
						),
					],
					false
				)
			)
			.returns(() =>
				Promise.resolve([{ ZZ6_PK: uuid(), ZZ6_ProcedureCode: "AAA" }])
			);

		const mockContextValue = {
			filters: [
				new TextFilterModule(
					"Procedure Code",
					new Filter(
						"ZZ6_ProcedureCode",
						FilterOps.Equals,
						"AAA" as any,
						"string"
					),
					5
				),
			],
			setFilters: jest.fn(),
		};

		let wrapper = mount<typeof RefCusProcedureUserViewSearchForm>(
			<BrowserRouter>
				<FilterContext.Provider value={mockContextValue}>
					<RefCusProcedureUserViewSearchForm
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
					"RefCusProcedureUserView",
					[ServiceType.Safe],
					It.isAny(),
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

		let wrapper = mount<typeof RefCusProcedureUserViewSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefCusProcedureUserViewSearchForm
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
					"RefCusProcedureUserView",
					[ServiceType.Safe],
					It.isAny(),
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
			.returns(() => Promise.resolve([]));

		let wrapper = mount<typeof RefCusProcedureUserViewSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefCusProcedureUserViewSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);
		const filterStrip = wrapper.find(FilterStrip).at(4);

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

let dataGrouping = Mock.ofType<IEntity>();
dataGrouping.setup((x) => x.ZZZ_PK).returns(() => "1");
dataGrouping.setup((x) => x.ZZZ_DataGrouping).returns(() => "AU");
dataGrouping.setup((x) => x.ZZZ_Description).returns(() => "Australia");
