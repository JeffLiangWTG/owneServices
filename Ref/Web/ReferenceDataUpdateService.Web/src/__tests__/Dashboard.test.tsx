import { It, Mock } from "typemoq";
import { IEntityManager, ServiceType } from "../EntityManager";
import { mount } from "enzyme";
import { Dashboard } from "../Dashboard";
import React from "react";
import { FilterStrip } from "../FilterStrip";
import ISourceDataUserView from "../models/ISourceDataUserView";
import { act } from "react-dom/test-utils";
import { setImmediate } from "timers";
import { ServiceStatus } from "../ServiceStatus";
import axios from "axios";

describe("Dashboard", () => {
	let entityManager = Mock.ofType<IEntityManager>();
	//mock axios for ServiceStatus component.
	jest.mock("axios");
	axios.get = jest.fn().mockReturnValue({ data: "Service Is Ok" });

	beforeEach(() => {
		entityManager
			.setup((x) =>
				x.getAsync<ISourceDataUserView>(
					"SourceDataUserView",
					[ServiceType.Staging],
					[],
					false,
					undefined,
					It.isAny()
				)
			)
			.returns(() =>
				Promise.resolve([
					sourceData1,
					sourceData2,
					sourceData3,
				])
			);
	});

	it("render", async () => {
		let wrapper = mount<typeof Dashboard>(
			<Dashboard entityManager={entityManager.object} />
		);

		await act(() => new Promise(setImmediate));
		wrapper.update();

		//three search fields
		expect(wrapper.find(FilterStrip).at(0).key()).toEqual("XML Data Source");
		expect(wrapper.find(FilterStrip).at(1).key()).toEqual("File Name");
		expect(wrapper.find(FilterStrip).at(2).key()).toEqual("Start Run Time");

		//search button
		expect(wrapper.find("button").at(0).html()).toContain("Search");

		//service statuses
		let serviceStatusDiv = wrapper.find("#serviceStatus");
		let serviceStatusComponents = serviceStatusDiv
			.children()
			.find(ServiceStatus);
		expect(serviceStatusComponents.length).toBe(3);
		expect(serviceStatusComponents.at(0).html()).toContain("<h6>Quartz</h6>");
		expect(serviceStatusComponents.at(1).html()).toContain(
			"<h6>Delivery Service</h6>"
		);
		expect(serviceStatusComponents.at(2).html()).toContain(
			"<h6>Update Service</h6>"
		);

		//table
		let table = wrapper.find("table");
		let tableHeader = table.children("thead");
		let headerRow = tableHeader.children();
		expect(headerRow.find("th").at(0).html()).toContain("XML Data Source");
		expect(headerRow.find("th").at(1).html()).toContain("XML Publication Time");
		expect(headerRow.find("th").at(2).html()).toContain("Source");
		expect(headerRow.find("th").at(3).html()).toContain("PK");
		expect(headerRow.find("th").at(4).html()).toContain("File Name");
		expect(headerRow.find("th").at(5).html()).toContain("Start Run Time");
		expect(headerRow.find("th").at(6).html()).toContain("XML Status");
		expect(headerRow.find("th").at(7).html()).toContain("Processing Status");
		expect(headerRow.find("th").at(8).html()).toContain("Link to Kibana");
	});

	it("onFilterValueChange change operation", async () => {
		let wrapper = mount<typeof Dashboard>(
			<Dashboard entityManager={entityManager.object} />
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
				x.getAsync<ISourceDataUserView>(
					"SourceDataUserView",
					[ServiceType.Staging],
					It.isAny(),
					false,
					undefined,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve([sourceData1]));
		let wrapper = mount<typeof Dashboard>(
			<Dashboard entityManager={entityManager.object} />
		);
		await act(() => new Promise(setImmediate));
		wrapper.update();
		const tableLines = wrapper.find("table > tbody").find("tr");
		expect(tableLines.length).toEqual(3);

		const firstFilterStrip = wrapper.find(FilterStrip).at(0);
		const textInput = firstFilterStrip.find("input");
		textInput.simulate("change", { target: { value: "SubSource 1" } });
		wrapper.find("button").simulate("click");

		await act(() => new Promise(setImmediate));
		wrapper.find("table").update();

		const filteredTableLines = wrapper.find("table > tbody").find("tr");
		expect(filteredTableLines.length).toEqual(1);

		//Checking only "special" columns
		//XML Publication Time
		expect(filteredTableLines.find("td").at(1).html()).toContain(
			"2017-06-12 23:22:13"
		);

		//File Name
		expect(filteredTableLines.find("td").at(4).html()).toContain(
			"Filename 1.xml"
		);

		//Start Run Time
		expect(filteredTableLines.find("td").at(5).html()).toContain(
			"2017-06-12 23:22:13"
		);
		//XML Status
		expect(filteredTableLines.find("td").at(6).html()).toMatch(new RegExp(getRegExpMatchTd("Queued")));
	});

	it("search MER and DUP", async () => {
		entityManager
			.setup((x) =>
				x.getAsync<ISourceDataUserView>(
					"SourceDataUserView",
					[ServiceType.Staging],
					It.isAny(),
					false,
					undefined,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve([sourceData4_1, sourceData4_2]));
	
		let wrapper = mount<typeof Dashboard>(
			<Dashboard entityManager={entityManager.object} />
		);
	
		const firstFilterStrip = wrapper.find(FilterStrip).at(0);
		const textInput = firstFilterStrip.find("input");
		textInput.simulate("change", { target: { value: "SubSource 4" } });
		wrapper.find("button").simulate("click");
	
		await act(() => new Promise(setImmediate));
		wrapper.find("table").update();
	
		const filteredTableLines = wrapper.find("table > tbody").find("tr");
		expect(filteredTableLines.length).toEqual(2);
	
		const headerCells = wrapper.find("table thead th");
		let xmlStatusIndex = -1;
		headerCells.forEach((th, idx) => {
			if (th.text().includes("XML Status")) {
				xmlStatusIndex = idx;
			}
		});
		expect(xmlStatusIndex).toBeGreaterThanOrEqual(0);
	
		expect(filteredTableLines.at(0).find("td").at(xmlStatusIndex).html()).toMatch(
			new RegExp(getRegExpMatchTd("Merged"))
		);
		expect(filteredTableLines.at(1).find("td").at(xmlStatusIndex).html()).toMatch(
			new RegExp(getRegExpMatchTd("Duplicated"))
		);
	});

	it("Processing Status for every SDA_Status", async () => {
		// mocking source data 
		var mockSourceData = createMockSourceDataUserView("1", "Filename.xml",
			"Source 1", sourceDate, createDate, notProcessedUntilDate, "ERR", "SubSource 1");
		await checkXmlAndProcessingStatus(mockSourceData, true, "", getRegExpMatchTd(""));
		var mockSourceDataCopy = { ...mockSourceData };
		mockSourceDataCopy.SDA_Status = "QUE";
		await checkXmlAndProcessingStatus(mockSourceDataCopy, true, "Waiting", getRegExpMatchTd("Waiting"));
		mockSourceDataCopy.SDA_Status = "MER";
		await checkXmlAndProcessingStatus(mockSourceDataCopy, true, "", getRegExpMatchTd(""));
		mockSourceDataCopy.SDA_Status = "DUP";
		await checkXmlAndProcessingStatus(mockSourceDataCopy, true, "", getRegExpMatchTd(""));
		mockSourceDataCopy.SDA_Status = "FIE";
		await checkXmlAndProcessingStatus(mockSourceDataCopy, true, "", getRegExpMatchTd(""));
		mockSourceDataCopy.SDA_Status = "FIN";
		await checkXmlAndProcessingStatus(mockSourceDataCopy, true, "", getRegExpMatchTd(""));
		mockSourceDataCopy.SDA_Status = "PRS";
		mockSourceDataCopy.SDA_NotProcessedUntil = null;
		await checkXmlAndProcessingStatus(mockSourceDataCopy, true, "", getRegExpMatchTd(""));
		mockSourceDataCopy.SDA_NotProcessedUntil = new Date(Date.now());
		await checkXmlAndProcessingStatus(mockSourceDataCopy, true, "", getRegExpMatchTd("Merging"));
	});

	const checkXmlAndProcessingStatus = async (
		data: ISourceDataUserView,
		isIgnoreCheckXmlStatus: boolean,
		expectXmlStatus: string,
		expectProcessingStatus: string
	) => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync<ISourceDataUserView>(
					"SourceDataUserView",
					[ServiceType.Staging],
					It.isAny(),
					false,
					undefined,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve([data]));
		let wrapper = mount<typeof Dashboard>(
			<Dashboard entityManager={entityManager.object} />
		);
		await act(() => new Promise(setImmediate));
		wrapper.find("table").update();

		const filteredTableLines = wrapper.find("table > tbody").find("tr");
		expect(filteredTableLines.length).toEqual(1);
		//XML Status
		if (!isIgnoreCheckXmlStatus) {
			expect(filteredTableLines.find("td").at(6).html()).toMatch(new RegExp(expectXmlStatus));
		}
		//Processing Status
		expect(filteredTableLines.find("td").at(7).html()).toMatch(new RegExp(expectProcessingStatus));
	}

	const createMockSourceDataUserView = (
		pk: string,
		filename: string,
		source: string,
		sourceTime: Date,
		createdTime: Date,
		notProcessedUntil: Date | null,
		status: string,
		subSource: string,
	): ISourceDataUserView => {
		var sourceData = Mock.ofType<ISourceDataUserView>();
		sourceData.setup((x) => x.SDA_PK).returns(() => pk);
		sourceData.setup((x) => x.SDA_Filename).returns(() => filename);
		sourceData.setup((x) => x.SDA_Source).returns(() => source);
		sourceData.setup((x) => x.SDA_SourceTime).returns(() => sourceTime);
		sourceData.setup((x) => x.SDA_CreatedTime).returns(() => createdTime);
		sourceData.setup((x) => x.SDA_NotProcessedUntil).returns(() => notProcessedUntil);
		sourceData.setup((x) => x.SDA_Status).returns(() => status);
		sourceData.setup((x) => x.SDA_SubSource).returns(() => subSource);

		return sourceData.object;
	}

	const getRegExpMatchTd = (
		tdValue: string | null,
	): string => {
		return `<td[\\s\\S]*?>${tdValue}<\\/td>`;  //blank <td></td>	
	}

	//Global variable definitions
	var sourceDate = new Date("2017-06-12 23:22:13.6746015");
	var createDate = sourceDate;
	var notProcessedUntilDate = new Date("2024-06-12 23:22:13.6746015");
	var sourceData1 = createMockSourceDataUserView("1", "C:\\RefDbRepo\\Something\\Filename 1.xml",
		"Source 1", sourceDate, createDate, notProcessedUntilDate, "QUE", "SubSource 1");
	var sourceData2 = createMockSourceDataUserView("2", "",
		"Source 2", sourceDate, createDate, notProcessedUntilDate, "PRS", "SubSource 2");
	var sourceData3 = createMockSourceDataUserView("3", "C:\\RefDbRepo\\Something\\Filename 3.xml",
		"Source 3", sourceDate, createDate, notProcessedUntilDate, "FIN", "SubSource 3");
	var sourceData4_1 = createMockSourceDataUserView("4", "C:\\RefDbRepo\\Something\\Filename 4_1.xml",
		"Source 4", sourceDate, createDate, notProcessedUntilDate, "MER", "SubSource 4");
	var sourceData4_2 = createMockSourceDataUserView("5", "C:\\RefDbRepo\\Something\\Filename 4_1.xml",
		"Source 4", sourceDate, createDate, notProcessedUntilDate, "DUP", "SubSource 4");
});
