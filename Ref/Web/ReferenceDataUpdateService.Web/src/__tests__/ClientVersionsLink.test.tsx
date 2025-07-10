import { mount } from "enzyme";
import { ClientVersionsLink } from "../ClientVersionsLink";
import React from "react";

describe("ClientVersionsLink", () => {
	it("renders with dataSet param", () => {
		const wrapper = mount<typeof ClientVersionsLink>(
			<ClientVersionsLink lastUpdatedUTCFrom="2024-08-27" dataSet={"DS1"} />
		);
		expect(wrapper.find("a").prop("href")).toBe(
			"/ClientVersions?lastUpdatedUTCFrom=2024-08-27&isLate=true&dataSet=DS1"
		);
	});
	it("renders with systemType param", () => {
		const wrapper = mount<typeof ClientVersionsLink>(
			<ClientVersionsLink lastUpdatedUTCFrom="2024-08-27" systemType={"ST1"} />
		);
		expect(wrapper.find("a").prop("href")).toBe(
			"/ClientVersions?lastUpdatedUTCFrom=2024-08-27&isLate=true&systemType=ST1"
		);
	});
	it("renders with clientId param", () => {
		const wrapper = mount<typeof ClientVersionsLink>(
			<ClientVersionsLink lastUpdatedUTCFrom="2024-08-27" clientId={"CBC"} />
		);
		expect(wrapper.find("a").prop("href")).toBe(
			"/ClientVersions?lastUpdatedUTCFrom=2024-08-27&isLate=true&clientId=CBC"
		);
	});
	it("renders with dataSet & systemType params", () => {
		const wrapper = mount<typeof ClientVersionsLink>(
			<ClientVersionsLink
				lastUpdatedUTCFrom="2024-08-27"
				dataSet={"DS1"}
				systemType={"ST1"}
			/>
		);
		expect(wrapper.find("a").prop("href")).toBe(
			"/ClientVersions?lastUpdatedUTCFrom=2024-08-27&isLate=true&dataSet=DS1&systemType=ST1"
		);
	});
	it("renders with dataSet & clientId params", () => {
		const wrapper = mount<typeof ClientVersionsLink>(
			<ClientVersionsLink
				lastUpdatedUTCFrom="2024-08-27"
				dataSet={"DS1"}
				clientId={"CBC"}
			/>
		);
		expect(wrapper.find("a").prop("href")).toBe(
			"/ClientVersions?lastUpdatedUTCFrom=2024-08-27&isLate=true&dataSet=DS1&clientId=CBC"
		);
	});
	it("renders with systemType & clientId params", () => {
		const wrapper = mount<typeof ClientVersionsLink>(
			<ClientVersionsLink
				lastUpdatedUTCFrom="2024-08-27"
				systemType={"ST1"}
				clientId={"CBC"}
			/>
		);
		expect(wrapper.find("a").prop("href")).toBe(
			"/ClientVersions?lastUpdatedUTCFrom=2024-08-27&isLate=true&systemType=ST1&clientId=CBC"
		);
	});
	it("renders with dataSet, clientId & systemType params", () => {
		const wrapper = mount<typeof ClientVersionsLink>(
			<ClientVersionsLink
				lastUpdatedUTCFrom="2024-08-27"
				systemType={"ST1"}
				clientId={"CBC"}
				dataSet={"DS1"}
			/>
		);
		expect(wrapper.find("a").prop("href")).toBe(
			"/ClientVersions?lastUpdatedUTCFrom=2024-08-27&isLate=true&dataSet=DS1&systemType=ST1&clientId=CBC"
		);
	});
});
