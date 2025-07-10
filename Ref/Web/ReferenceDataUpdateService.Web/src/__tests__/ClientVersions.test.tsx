import { mount } from "enzyme";
import { ClientVersions } from "../ClientVersions";
import React from "react";
import { DateTimeInput } from "../DateTimeInput";
import { IClientRefDbVersionControl } from "../models/IClientRefDbVersionControl";
import axios from "axios";
import { act } from "react-dom/test-utils";
import uuid from "uuid";
import { setImmediate } from "timers";
import { IEntityManager, ServiceType } from "../EntityManager";
import { It, Mock, Times } from "typemoq";
import { Filter, FilterOps } from "../Filter";
import { MemoryRouter } from "react-router-dom";

describe("ClientVersions", () => {
	let entityManager = Mock.ofType<IEntityManager>();
	let fakeNow = new Date("2024-08-08");

	beforeEach(() => {
		jest.useFakeTimers({ now: fakeNow });
		jest.mock("axios");
		entityManager.reset();
		entityManager
			.setup((x) =>
				x.getAsync<IClientRefDbVersionControl>(
					"ClientRefDbVersionControl",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([clientRefDbVersionControl1.object]));

		axios.get = jest.fn().mockReturnValue({ data: dataSetTimeStamps });
	});

	it("render components structure with mandatory lastUpdatedUTCFrom parameter only", async () => {
		const newUrl = "?lastUpdatedUTCFrom=2024-06-09T23:51:08.529Z";

		let wrapper = mount<typeof ClientVersions>(
			<MemoryRouter initialEntries={[{ pathname: "/", search: newUrl }]}>
				<ClientVersions entityManager={entityManager.object} />
			</MemoryRouter>
		);

		//let useEffect run
		await act(() => new Promise(setImmediate));
		wrapper.update();

		//date input
		expect(wrapper.find(DateTimeInput).at(0)).not.toBeNull();
		//search button
		expect(wrapper.find("button").at(0).html()).toContain("Update");

		//table
		const table = wrapper.find("table").at(0);
		const tableHeader = table.find("thead");
		const headerRow = tableHeader.children();
		expect(headerRow.find("th").at(0).html()).toContain("Data Set");
		expect(headerRow.find("th").at(1).html()).toContain("Data Set Timestamp");
		expect(headerRow.find("th").at(2).html()).toContain("Client Id");
		expect(headerRow.find("th").at(3).html()).toContain("Client Timestamp");
		expect(headerRow.find("th").at(4).html()).toContain("System Type");
	});

	it("updates search with query params lastUpdatedUTCFrom", async () => {
		const newUrl = "?lastUpdatedUTCFrom=2024-06-09T23:51:08.529Z";

		let wrapper = mount<typeof ClientVersions>(
			<MemoryRouter initialEntries={[{ pathname: "/", search: newUrl }]}>
				<ClientVersions entityManager={entityManager.object} />
			</MemoryRouter>
		);

		//let useEffect run
		await act(() => new Promise(setImmediate));
		wrapper.update();

		entityManager.verify(
			(x) =>
				x.getAsync<IClientRefDbVersionControl>(
					"ClientRefDbVersionControl",
					[ServiceType.Safe],
					[
						new Filter(
							"CVC_LastUpdatedTimeUTC",
							FilterOps.DateRange,
							`2024-06-09T23:51:08.529Z to ${fakeNow.toISOString()}` as any,
							"datetime"
						),
					],
					false
				),
			Times.once()
		);
	});

	it("updates search with query params lastUpdatedUTCFrom & dataSet", async () => {
		const newUrl =
			"?lastUpdatedUTCFrom=2024-06-09T23:51:08.529Z&dataSet=GBCustomsTariff";

		let wrapper = mount<typeof ClientVersions>(
			<MemoryRouter initialEntries={[{ pathname: "/", search: newUrl }]}>
				<ClientVersions entityManager={entityManager.object} />
			</MemoryRouter>
		);

		//let useEffect run
		await act(() => new Promise(setImmediate));
		wrapper.update();

		entityManager.verify(
			(x) =>
				x.getAsync(
					"ClientRefDbVersionControl",
					[ServiceType.Safe],
					[
						new Filter(
							"CVC_LastUpdatedTimeUTC",
							FilterOps.DateRange,
							`2024-06-09T23:51:08.529Z to ${fakeNow.toISOString()}` as any,
							"datetime"
						),
						new Filter(
							"CVC_DataSet",
							FilterOps.Equals,
							"GBCustomsTariff" as any,
							"string"
						),
					],
					false
				),
			Times.once()
		);
	});

	it("updates search with query params lastUpdatedUTCFrom & dataSet & clientId & systemType", async () => {
		const newUrl =
			"?lastUpdatedUTCFrom=2024-06-09T23:51:08.529Z&dataSet=GBCustomsTariff&clientId=EUN&systemType=PRO";

		let wrapper = mount<typeof ClientVersions>(
			<MemoryRouter initialEntries={[{ pathname: "/", search: newUrl }]}>
				<ClientVersions entityManager={entityManager.object} />
			</MemoryRouter>
		);

		//let useEffect run
		await act(() => new Promise(setImmediate));
		wrapper.update();

		entityManager.verify(
			(x) =>
				x.getAsync(
					"ClientRefDbVersionControl",
					[ServiceType.Safe],
					[
						new Filter(
							"CVC_LastUpdatedTimeUTC",
							FilterOps.DateRange,
							`2024-06-09T23:51:08.529Z to ${fakeNow.toISOString()}` as any,
							"datetime"
						),
						new Filter(
							"CVC_DataSet",
							FilterOps.Equals,
							"GBCustomsTariff" as any,
							"string"
						),
						new Filter(
							"CVC_ClientId",
							FilterOps.Equals,
							"EUN" as any,
							"string"
						),
						new Filter(
							"CVC_SystemType",
							FilterOps.Equals,
							"PRO" as any,
							"string"
						),
					],
					false
				),
			Times.once()
		);
	});
});

var dataSetTimeStamps = {
	DS1: "2024-08-08 00:00:00",
};
var clientRefDbVersionControl1 = Mock.ofType<IClientRefDbVersionControl>();
clientRefDbVersionControl1.setup((x) => x.CVC_ClientId).returns(() => "CVC1");
clientRefDbVersionControl1.setup((x) => x.CVC_DataSet).returns(() => "DS1");
clientRefDbVersionControl1
	.setup((x) => x.CVC_DataSetTimestamp)
	.returns(() => new Date("2024-08-01 00:00:00"));
clientRefDbVersionControl1
	.setup((x) => x.CVC_DateSetCheckpoint)
	.returns(() => "");
clientRefDbVersionControl1.setup((x) => x.CVC_IsInUse).returns(() => true);
clientRefDbVersionControl1
	.setup((x) => x.CVC_LastUpdatedTimeUTC)
	.returns(() => new Date("2024-08-08 00:00:00"));
clientRefDbVersionControl1.setup((x) => x.CVC_PK).returns(() => uuid());
clientRefDbVersionControl1.setup((x) => x.CVC_SystemType).returns(() => "PRO");
