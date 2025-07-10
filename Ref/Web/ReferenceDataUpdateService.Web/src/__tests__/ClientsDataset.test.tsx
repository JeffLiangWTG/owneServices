import { mount, shallow } from "enzyme";
import { DateTimeInput } from "../DateTimeInput";
import { ClientsDataset } from "../ClientsDataset";
import React from "react";
import axios from "axios";
import {
	ClientDataSetVersionSummary,
	IClientDataSetVersionSummary,
} from "../models/IClientDataSetVersionSummary";
import uuid from "uuid";
import moment from "moment";
import { act } from "react-dom/test-utils";
import { setImmediate } from "timers";
import { ClientVersionsLink } from "../ClientVersionsLink";

describe("ClientsDataset", () => {
	it("render", () => {
		let wrapper = shallow<typeof ClientsDataset>(<ClientsDataset />);

		//date input
		expect(wrapper.find(DateTimeInput).at(0)).not.toBeNull();
		//search button
		expect(wrapper.find("button").at(0).html()).toContain("Search");

		//table
		const table = wrapper.find("table").at(0);
		const tableHeader = table.find("thead");
		const headerRow = tableHeader.children();
		expect(headerRow.find("th").at(0).html()).toContain("Data Set");
		expect(headerRow.find("th").at(1).html()).toContain(
			"Last Data Changed Time"
		);
		expect(headerRow.find("th").at(2).html()).toContain(
			"No. Of Customers Updated"
		);
		expect(headerRow.find("th").at(3).html()).toContain(
			"No. Of Customers Failed (Production)"
		);
	});

	it("searchs", async () => {
		jest.mock("axios");
		const dateInputFormat = "DD-MMM-yyyy HH:mm:ss A";
		const clientVersionSummary: IClientDataSetVersionSummary[] = [
			new ClientDataSetVersionSummary(
				uuid(),
				"ds1",
				new Date("2023-06-12 23:22:13"),
				0,
				2,
				1
			),
			new ClientDataSetVersionSummary(
				uuid(),
				"ds2",
				new Date("2024-06-12 23:22:13"),
				100,
				0,
				0
			),
			new ClientDataSetVersionSummary(
				uuid(),
				"ds3",
				new Date("2022-06-12 23:22:13"),
				5,
				1,
				3
			),
			new ClientDataSetVersionSummary(
				uuid(),
				"ds4",
				new Date("2021-06-12 23:22:13"),
				7,
				1,
				1
			),
			new ClientDataSetVersionSummary(
				uuid(),
				"ds5",
				new Date("2020-06-12 23:22:13"),
				4,
				1,
				1
			),
			new ClientDataSetVersionSummary(
				uuid(),
				"ds6",
				new Date("2019-06-12 23:22:13"),
				5,
				1,
				1
			),
		];
		axios.get = jest.fn().mockReturnValueOnce({ data: clientVersionSummary });

		let wrapper = mount<typeof ClientsDataset>(<ClientsDataset />);

		//let useEffect run
		await act(() => new Promise(setImmediate));
		wrapper.update();

		//set date
		const inputToFilter = new Date("2021-06-12 22:22:13");
		let dateTimeInput = wrapper.find(DateTimeInput).at(0);
		dateTimeInput.simulate("change", {
			target: {
				value: moment(inputToFilter).format(dateInputFormat),
			},
		});

		//mock again to get less records
		axios.get = jest.fn().mockReturnValueOnce({
			data: clientVersionSummary.filter(
				(x) => x.LastDataChangedTime > inputToFilter
			),
		});
		//press Search
		wrapper.find("button").at(0).simulate("click");

		//let state be set
		await act(() => new Promise(setImmediate));
		wrapper.find("table").update();

		const filteredTableLines = wrapper.find("table > tbody").find("tr");
		expect(filteredTableLines.length).toEqual(4);

		const firstLine = filteredTableLines.at(0);
		expect(
			firstLine
				.find("td")
				.at(0)
				.find(ClientVersionsLink)
				.children()
				.find("span")
				.text()
		).toBe("ds1");
		expect(firstLine.find("td").at(1).text()).toBe(
			moment(new Date("2023-06-12 23:22:13")).format("DD-MMM-yyyy HH:mm:ss")
		);
		expect(firstLine.find("td").at(2).text()).toBe("0");
		expect(
			firstLine
				.find("td")
				.at(3)
				.find(ClientVersionsLink)
				.at(0)
				.children()
				.find("span")
				.text()
		).toBe("2");
		expect(
			firstLine
				.find("td")
				.at(3)
				.find(ClientVersionsLink)
				.at(1)
				.children()
				.find("span")
				.text()
		).toBe("(1)");
	});

	it("onDateFilterValueChange sets dateObj correctly for value and null", async () => {
		jest.mock("axios");
		const clientVersionSummary: IClientDataSetVersionSummary[] = [
			new ClientDataSetVersionSummary(
				uuid(),
				"ds1",
				new Date("2023-06-12 23:22:13"),
				0,
				2,
				1
			)
		];
		axios.get = jest.fn().mockReturnValueOnce({ data: clientVersionSummary });

		const wrapper = mount(<ClientsDataset />);
		await act(() => Promise.resolve());
		wrapper.update();

		const dateTimeInput = wrapper.find(DateTimeInput).at(0);
		const onValueChange = dateTimeInput.prop("onValueChange");

		const testValue = "2024-01-01T12:00:00";
		await act(async () => {
			await onValueChange({}, "dateToFilter", testValue as any);
		});
		wrapper.update();
		expect(wrapper.find(DateTimeInput).prop("entity")?.dateToFilter).toBe(testValue);

		await act(async () => {
			await onValueChange({}, "dateToFilter", null as any);
		});
		wrapper.update();
		const defaultDate = wrapper.find(DateTimeInput).prop("entity")?.dateToFilter;
		// The default is set at module load, so compare format
		expect(moment(defaultDate, "YYYY-MM-DDTHH:mm:ss", true).isValid()).toBe(true);
	});
});
