import axios from "axios";
import { mount } from "enzyme";
import { ServiceStatus } from "../ServiceStatus";
import React from "react";
import { act } from "react-dom/test-utils";
import { setImmediate } from "timers";

describe("Service Status", () => {
	beforeEach(() => {
		jest.mock("axios");
	});

	it("renders div with correct classes", async () => {
		axios.get = jest.fn().mockReturnValueOnce({ data: "Service Is Ok" });

		const wrapper = mount<typeof ServiceStatus>(
			<ServiceStatus serviceName="service1" serviceUrl="serviceurl" />
		);

		await act(() => new Promise(setImmediate));
		wrapper.update();

		const mainCardDiv = wrapper.find("div").at(0);
		expect(mainCardDiv.hasClass("card")).toBeTruthy();
		expect(mainCardDiv.hasClass("m-1")).toBeTruthy();
		expect(mainCardDiv.hasClass("bg-light")).toBeTruthy();
		//check for special style needed in main card
		expect(mainCardDiv.html()).toContain('style="max-width: 7rem;"');

		const cardBodyDiv = mainCardDiv.children().find("div").at(0);
		expect(cardBodyDiv.hasClass("card-body")).toBeTruthy();
	});

	it("show service name", async () => {
		axios.get = jest.fn().mockReturnValueOnce({ data: "Service Is Ok" });

		const wrapper = mount<typeof ServiceStatus>(
			<ServiceStatus serviceName="service1" serviceUrl="serviceurl" />
		);

		await act(() => new Promise(setImmediate));
		wrapper.update();

		expect(wrapper.find("h6").text()).toBe("service1");
	});

	it("return service status offline when Unhealthy", async () => {
		axios.get = jest.fn().mockReturnValueOnce({ data: "anything" });

		const wrapper = mount<typeof ServiceStatus>(
			<ServiceStatus serviceName="service1" serviceUrl="url1.com" />
		);

		//let useEffect run
		await act(() => new Promise(setImmediate));
		wrapper.update();

		const spanWithInfo = wrapper.find("span").at(0);
		expect(spanWithInfo.hasClass("badge-danger")).toBeTruthy();
		expect(spanWithInfo.text()).toBe("Off");
	});

	it("return service status online when Healthy", async () => {
		axios.get = jest
			.fn()
			.mockReturnValueOnce({ status: 200, data: "Service Is Ok" });

		const wrapper = mount<typeof ServiceStatus>(
			<ServiceStatus serviceName="service2" serviceUrl="url2.com" />
		);

		//let useEffect run
		await act(() => new Promise(setImmediate));
		wrapper.update();

		const spanWithInfo = wrapper.find("span").at(0);
		expect(spanWithInfo.hasClass("badge-success")).toBeTruthy();
		expect(spanWithInfo.text()).toBe("On");
	});

	it("updates status after given time", async () => {
		axios.get = jest
			.fn()
			.mockReturnValueOnce({ status: 401, data: "Unavailable" })
			.mockReturnValueOnce({ status: 200, data: "Service is Ok" });

		const wrapper = mount<typeof ServiceStatus>(
			<ServiceStatus
				serviceName="service2"
				serviceUrl="url2.com"
				updateIntervalInMs={2000}
			/>
		);

		//let useEffect run
		await act(() => new Promise(setImmediate));
		wrapper.update();

		let spanWithInfo = wrapper.find("span").at(0);
		expect(spanWithInfo.hasClass("badge-danger")).toBeTruthy();
		expect(spanWithInfo.text()).toBe("Off");

		await act(
			async () => await new Promise((result) => setTimeout(result, 2000))
		); //wait 1 minute
		wrapper.update();
		spanWithInfo = wrapper.find("span").at(0);
		expect(spanWithInfo.hasClass("badge-success")).toBeTruthy();
		expect(spanWithInfo.text()).toBe("On");
	});
});
