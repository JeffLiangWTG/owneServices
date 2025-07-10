import { It, Mock } from "typemoq";
import { IEntityManager, ServiceType } from "../EntityManager";
import { mount, shallow } from "enzyme";
import { Processor } from "../Processor";
import React from "react";
import { FilterStrip } from "../FilterStrip";
import IProcessorStatus from "../models/IProcessorStatus";
import { act } from "react-dom/test-utils";
import { setImmediate } from "timers";

describe("Processor", () => {
	let entityManager = Mock.ofType<IEntityManager>();
	beforeEach(() => {
		entityManager
			.setup((x) =>
				x.getAsync<IProcessorStatus>(
					"ProcessorStatus",
					[ServiceType.Staging],
					[],
					false
				)
			)
			.returns(() =>
				Promise.resolve([
					processStatus1.object,
					processStatus2.object,
					processStatus3.object,
				])
			);
	});

	it("render", () => {
		let entityManager = Mock.ofType<IEntityManager>();
		let wrapper = shallow<typeof Processor>(
			<Processor entityManager={entityManager.object} />
		);

		//two search fields
		expect(wrapper.find(FilterStrip).at(0).key()).toEqual("Quartz Group");
		expect(wrapper.find(FilterStrip).at(1).key()).toEqual("Name");
		expect(wrapper.find(FilterStrip).at(2).key()).toEqual("Last Run Status");

		//search button
		expect(wrapper.find("button").at(0).html()).toContain("Search");

		//table
		let table = wrapper.find("table");
		let tableHeader = table.children("thead");
		let headerRow = tableHeader.children();
		expect(headerRow.find("th").at(0).html()).toContain("Quartz Group");
		expect(headerRow.find("th").at(1).html()).toContain("Name");
		expect(headerRow.find("th").at(2).html()).toContain("Last Run Time(Local)");
		expect(headerRow.find("th").at(3).html()).toContain("Last Run Status");
		expect(headerRow.find("th").at(4).html()).toContain(
			"Last Successful Run Time(Local)"
		);
		expect(headerRow.find("th").at(5).html()).toContain("Link to Kibana");
	});

	it("onFilterValueChange change operation", async () => {
		let wrapper = mount<typeof Processor>(
			<Processor entityManager={entityManager.object} />
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

	it("search", async () => {
		entityManager
			.setup((x) =>
				x.getAsync<IProcessorStatus>(
					"ProcessorStatus",
					[ServiceType.Staging],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([processStatus1.object]));

		let wrapper = mount<typeof Processor>(
			<Processor entityManager={entityManager.object} />
		);
		await act(() => new Promise(setImmediate));
		wrapper.update();
		const tableLines = wrapper.find("table > tbody").find("tr");
		expect(tableLines.length).toEqual(3);

		const firstFilterStrip = wrapper.find(FilterStrip).at(0);
		const textInput = firstFilterStrip.find("input");
		textInput.simulate("change", { target: { value: "AU Customs" } });
		wrapper.find("button").simulate("click");

		await act(() => new Promise(setImmediate));
		wrapper.find("table").update();

		const filteredTableLines = wrapper.find("table > tbody").find("tr");
		expect(filteredTableLines.length).toEqual(1);

		//Country
		expect(filteredTableLines.find("td").at(0).html()).toContain("AU Customs");

		//Name
		expect(filteredTableLines.find("td").at(1).html()).toContain("Job 1");

		//Last Run Time
		expect(filteredTableLines.find("td").at(2).html()).toContain(
			"2017-06-10 23:22:13"
		);

		//Status
		expect(filteredTableLines.find("td").at(3).html()).toContain("Success");

		//Last Successful Run Time
		expect(filteredTableLines.find("td").at(4).html()).toContain(
			"2017-06-11 23:22:13"
		);
	});

	var processStatus1 = Mock.ofType<IProcessorStatus>();
	processStatus1.setup((x) => x.PRC_PK).returns(() => "1");
	processStatus1.setup((x) => x.PRC_JobName).returns(() => "Job 1");
	processStatus1.setup((x) => x.PRC_JobGroup).returns(() => "AU Customs");
	processStatus1
		.setup((x) => x.PRC_LastRunTime)
		.returns(() => new Date("2017-06-10 23:22:13.6746015"));
	processStatus1
		.setup((x) => x.PRC_LastSuccessRunTime)
		.returns(() => new Date("2017-06-11 23:22:13.6746015"));
	processStatus1.setup((x) => x.PRC_Status).returns(() => "PRS");
	var processStatus2 = Mock.ofType<IProcessorStatus>();
	processStatus2.setup((x) => x.PRC_PK).returns(() => "2");
	processStatus2.setup((x) => x.PRC_JobName).returns(() => "Job 2");
	processStatus2.setup((x) => x.PRC_JobGroup).returns(() => "US Customs");
	processStatus2
		.setup((x) => x.PRC_LastRunTime)
		.returns(() => new Date("2017-06-12 23:22:13.6746015"));
	processStatus2
		.setup((x) => x.PRC_LastSuccessRunTime)
		.returns(() => new Date("2017-06-12 23:22:13.6746015"));
	processStatus2.setup((x) => x.PRC_Status).returns(() => "PRS");
	var processStatus3 = Mock.ofType<IProcessorStatus>();
	processStatus3.setup((x) => x.PRC_PK).returns(() => "3");
	processStatus3.setup((x) => x.PRC_JobName).returns(() => "Job 3");
	processStatus3.setup((x) => x.PRC_JobGroup).returns(() => "ZA Customs");
	processStatus3
		.setup((x) => x.PRC_LastRunTime)
		.returns(() => new Date("2017-06-12 23:22:13.6746015"));
	processStatus3
		.setup((x) => x.PRC_LastSuccessRunTime)
		.returns(() => new Date("2017-06-12 23:22:13.6746015"));
	processStatus3.setup((x) => x.PRC_Status).returns(() => "ERR");
});
