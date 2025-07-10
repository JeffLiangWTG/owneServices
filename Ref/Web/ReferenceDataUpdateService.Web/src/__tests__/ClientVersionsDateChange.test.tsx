import { mount } from "enzyme";
import { ClientVersions } from "../ClientVersions";
import React from "react";
import { It, Mock } from "typemoq";
import { IEntityManager, ServiceType } from "../EntityManager";
import { IClientRefDbVersionControl } from "../models/IClientRefDbVersionControl";
import { useLocation } from "react-router-dom";
import moment from "moment";
import { act } from "react-dom/test-utils";
import { DateTimeInput } from "../DateTimeInput";
import axios from "axios";
import uuid from "uuid";
import { setImmediate } from "timers";

const mockHistoryPush = jest.fn();
jest.mock("react-router-dom", () => ({
	...jest.requireActual("react-router-dom"),
	useLocation: jest.fn().mockReturnValue({
		pathname: "/ClientVersions",
		search: "?lastUpdatedUTCFrom=2024-06-09T23:51:08.529Z",
		hash: "",
		state: null,
		key: "testKey",
	}),
	useHistory: () => ({ push: mockHistoryPush }),
}));

describe("ClientVersion onDateChange event", () => {
	let entityManager = Mock.ofType<IEntityManager>();
	beforeEach(() => {
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

	it("onDateChange pushes new search url to the history", async () => {
		const div = global.document.createElement("div");
		global.document.body.appendChild(div);

		let wrapper = mount<typeof ClientVersions>(
			<ClientVersions entityManager={entityManager.object} />,
			{ attachTo: div }
		);

		//let useEffect run
		await act(() => new Promise(setImmediate));
		wrapper.update();

		const datetimepickerInput = wrapper.find(DateTimeInput);
		const textInput = datetimepickerInput
			.children()
			.find("input[type='text']")
			.getDOMNode();

		($("#" + textInput.id) as any).datetimepicker(
			"date",
			moment(new Date("2021-06-05 23:51:08"))
		);

		await act(() => new Promise(setImmediate));

		expect(useLocation).toHaveBeenCalled();
		expect(mockHistoryPush).toHaveBeenCalledWith({
			pathname: "/ClientVersions",
			search: "lastUpdatedUTCFrom=2021-06-05T23%3A51%3A08",
		});

		wrapper.detach();
		global.document.body.removeChild(div);
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
