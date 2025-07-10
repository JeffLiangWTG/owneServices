import React from "react";
import { IEntityManager, ServiceType } from "../EntityManager";
import { mount, ReactWrapper } from "enzyme";
import { Mock, It } from "typemoq";
import { FilterStrip } from "../FilterStrip";
import { Filter, FilterOps } from "../Filter";
import IQrtzJobDetails from "../models/IQrtzJobDetails";
import RefApplicationConfigSearchForm from "../RefApplicationConfigSearchForm";
import { act } from "react-dom/test-utils";
import { setImmediate } from "timers";
import { BrowserRouter } from "react-router-dom";
import { CodeInput } from "../CodeInput";
import Button from "../Button";
import { FilterContext, FilterProvider } from "../FilterContext";
import { TextFilterModule } from "../TextFilterModule";

const pressFind = async (wrapper: ReactWrapper) => {
	//press find
	wrapper
		.findWhere((x) => x.is(Button) && x.text() === "Find")
		.simulate("mousedown", { button: 0 });
	await act(() => new Promise(setImmediate));
	wrapper.update();
};

describe("<RefApplicationConfigSearchForm />", () => {
	it("render data", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync("QRTZ_JOB_DETAILS", [ServiceType.Staging], It.isAny(), false)
			)
			.returns(() => Promise.resolve([job1.object, job2.object]));

		let wrapper = mount<typeof RefApplicationConfigSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefApplicationConfigSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);
		await pressFind(wrapper as any);

		expect(wrapper.find("tbody > tr")).toHaveLength(2);
		expect(wrapper.find(FilterStrip).at(0).key()).toEqual("Job Name");
		expect(wrapper.find(FilterStrip).at(1).key()).toEqual("Country");
	});

	it("do not call find when filter context is present but filter is empty", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync(
					"QRTZ_JOB_DETAILS",
					[ServiceType.Staging],
					[new Filter("JOB_NAME", FilterOps.Equals, "" as any, "string")],
					false
				)
			)
			.returns(() => Promise.resolve([job1.object]));

		const mockContextValue = {
			filters: [
				new TextFilterModule(
					"Job Name",
					new Filter("JOB_NAME", FilterOps.Equals, "" as any, "string"),
					150
				),
				new TextFilterModule(
					"Country",
					new Filter("CountryCode", FilterOps.Equals, "" as any, "string"),
					3,
					"CountryCode",
					"CountryCode",
					"QRTZ_JOB_DETAILS"
				),
			],
			setFilters: jest.fn(),
		};

		let wrapper = mount<typeof RefApplicationConfigSearchForm>(
			<BrowserRouter>
				<FilterContext.Provider value={mockContextValue}>
					<RefApplicationConfigSearchForm
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
					"QRTZ_JOB_DETAILS",
					[ServiceType.Staging],
					[new Filter("JOB_NAME", FilterOps.Equals, "A" as any, "string")],
					false
				)
			)
			.returns(() => Promise.resolve([job1.object]));

		const mockContextValue = {
			filters: [
				new TextFilterModule(
					"Job Name",
					new Filter("JOB_NAME", FilterOps.Equals, "A" as any, "string"),
					150
				),
				new TextFilterModule(
					"Country",
					new Filter("CountryCode", FilterOps.Equals, "" as any, "string"),
					3,
					"CountryCode",
					"CountryCode",
					"QRTZ_JOB_DETAILS"
				),
			],
			setFilters: jest.fn(),
		};

		let wrapper = mount<typeof RefApplicationConfigSearchForm>(
			<BrowserRouter>
				<FilterContext.Provider value={mockContextValue}>
					<RefApplicationConfigSearchForm
						entityManager={entityManager.object}
					/>
				</FilterContext.Provider>
			</BrowserRouter>
		);
		await act(() => new Promise(setImmediate));
		wrapper.update();

		expect(wrapper.find("tbody > tr")).toHaveLength(1);
	});

	it("getProgramArgs", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync("QRTZ_JOB_DETAILS", [ServiceType.Staging], It.isAny(), false)
			)
			.returns(() => Promise.resolve([job1.object, job2.object]));
		let wrapper = mount<typeof RefApplicationConfigSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefApplicationConfigSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);
		await pressFind(wrapper as any);

		expect(wrapper.find("tbody > tr")).toHaveLength(2);
		expect(wrapper.find("tbody > tr").at(0).find("td").at(2).text()).toEqual(
			"Short program args"
		);
		expect(wrapper.find("tbody > tr").at(1).find("td").at(2).text()).toEqual(
			"This is a very long program ar..."
		);
	});

	it("getProgramExePath", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync("QRTZ_JOB_DETAILS", [ServiceType.Staging], It.isAny(), false)
			)
			.returns(() => Promise.resolve([job1.object, job2.object, job3.object]));
		let wrapper = mount<typeof RefApplicationConfigSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefApplicationConfigSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);

		await pressFind(wrapper as any);

		expect(wrapper.find("tbody > tr")).toHaveLength(3);
		expect(wrapper.find("tbody > tr").at(0).find("td").at(3).text()).toEqual(
			"FrenchExchangeRate.exe"
		);
		expect(wrapper.find("tbody > tr").at(1).find("td").at(3).text()).toEqual(
			"UniversalXMLProducers.EUNTariffDataProducer.exe"
		);
		expect(wrapper.find("tbody > tr").at(2).find("td").at(3).text()).toEqual(
			"AUReferenceData.CmdLine.exe"
		);
	});

	it("onValueChange", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		let wrapper = mount<typeof RefApplicationConfigSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefApplicationConfigSearchForm
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
				x.getAsync<IQrtzJobDetails>(
					"QRTZ_JOB_DETAILS",
					[ServiceType.Staging],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([]));
		let wrapper = mount<typeof RefApplicationConfigSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefApplicationConfigSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);
		const filterStrip = wrapper.find(FilterStrip).at(1);

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

	it("loadDistinctQrtzJobs", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync<IQrtzJobDetails>(
					"QRTZ_JOB_DETAILS",
					[ServiceType.Staging],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([job1.object, job2.object]));
		let wrapper = mount<typeof RefApplicationConfigSearchForm>(
			<BrowserRouter>
				<FilterProvider>
					<RefApplicationConfigSearchForm
						entityManager={entityManager.object}
					/>
				</FilterProvider>
			</BrowserRouter>
		);

		await act(() => new Promise(setImmediate));
		wrapper.update();

		expect(wrapper.find(FilterStrip).at(1).props().prevEntity).not.toBeNull();
		const prevEntity = wrapper.find(FilterStrip).at(1).props().prevEntity;
		expect(prevEntity).toHaveLength(1);
		expect((prevEntity![0] as IQrtzJobDetails).CountryCode).toEqual("ZA");
	});

	var job1 = Mock.ofType<IQrtzJobDetails>();
	job1.setup((x) => x.JOB_PK).returns(() => "1");
	job1.setup((x) => x.SCHED_NAME).returns(() => "AA");
	job1.setup((x) => x.JOB_NAME).returns(() => "A");
	job1.setup((x) => x.CountryCode).returns(() => "ZA");
	job1.setup((x) => x.ProgramArgs).returns(() => "Short program args");
	job1.setup((x) => x.ProgramExePath).returns(() => "FrenchExchangeRate.exe");

	var job2 = Mock.ofType<IQrtzJobDetails>();
	job2.setup((x) => x.JOB_PK).returns(() => "2");
	job2.setup((x) => x.SCHED_NAME).returns(() => "BB");
	job2.setup((x) => x.JOB_NAME).returns(() => "B");
	job2.setup((x) => x.CountryCode).returns(() => "ZA");
	job2
		.setup((x) => x.ProgramArgs)
		.returns(() => "This is a very long program args. 1234567890");
	job2
		.setup((x) => x.ProgramExePath)
		.returns(
			() =>
				"CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.exe"
		);

	var job3 = Mock.ofType<IQrtzJobDetails>();
	job3.setup((x) => x.JOB_PK).returns(() => "3");
	job3.setup((x) => x.SCHED_NAME).returns(() => "CC");
	job3.setup((x) => x.JOB_NAME).returns(() => "C");
	job3.setup((x) => x.CountryCode).returns(() => "ZA");
	job3.setup((x) => x.ProgramArgs).returns(() => "TARIFF");
	job3
		.setup((x) => x.ProgramExePath)
		.returns(
			() =>
				"..\\UniversalXMLProducers\\CargoWise.RefDbRepo.AUReferenceData.CmdLine.exe"
		);
});
