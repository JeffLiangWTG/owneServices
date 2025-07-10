import { Mock } from "typemoq";
import ISourceDataUserView from "../models/ISourceDataUserView";
import { mount } from "enzyme";
import { KibanaLink } from "../KibanaLink";
import React from "react";
import IProcessorStatus from "../models/IProcessorStatus";
import uuid from "uuid";

describe("Kibana Link", () => {
	it("render ISourceDataUserView", () => {
		let sourceDataQUEMock = Mock.ofType<ISourceDataUserView>();
		sourceDataQUEMock.setup((x) => x.SDA_Status).returns(() => "QUE");
		sourceDataQUEMock.setup((x) => x.SDA_PK).returns(() => uuid());
		sourceDataQUEMock.setup((x) => x.SDA_CreatedTime).returns(() => new Date());

		let wrapperQUE = mount<typeof KibanaLink>(
			<KibanaLink entity={sourceDataQUEMock.object} />
		);
		// does not create link for QUE
		expect(wrapperQUE.html()).toBeNull();

		let sourceDataPRSMock = Mock.ofType<ISourceDataUserView>();
		sourceDataPRSMock.setup((x) => x.SDA_Status).returns(() => "PRS");
		sourceDataPRSMock.setup((x) => x.SDA_CreatedTime).returns(() => new Date());
		sourceDataPRSMock.setup((x) => x.SDA_PK).returns(() => uuid());

		let wrapperPRS = mount<typeof KibanaLink>(
			<KibanaLink entity={sourceDataPRSMock.object} />
		);
		// does not create link for PRS
		expect(wrapperPRS.html()).toBeNull();

		let sourceDataMERMock = Mock.ofType<ISourceDataUserView>();
		sourceDataMERMock.setup((x) => x.SDA_Status).returns(() => "MER");
		sourceDataMERMock.setup((x) => x.SDA_CreatedTime).returns(() => new Date());
		sourceDataMERMock.setup((x) => x.SDA_PK).returns(() => uuid());

		let wrapperMER = mount<typeof KibanaLink>(
			<KibanaLink entity={sourceDataMERMock.object} />
		);
		// does not create link for MER
		expect(wrapperMER.html()).toBeNull();

		let sourceDataFINMock = Mock.ofType<ISourceDataUserView>();
		sourceDataFINMock.setup((x) => x.SDA_Status).returns(() => "FIN");
		sourceDataFINMock.setup((x) => x.SDA_CreatedTime).returns(() => new Date());
		sourceDataFINMock.setup((x) => x.SDA_PK).returns(() => uuid());

		let wrapperFIN = mount<typeof KibanaLink>(
			<KibanaLink entity={sourceDataFINMock.object} />
		);
		// does not create link for FIN
		expect(wrapperFIN.html()).toBeNull();

		let sourceDataERRMock = Mock.ofType<ISourceDataUserView>();
		sourceDataERRMock.setup((x) => x.SDA_Status).returns(() => "ERR");
		sourceDataERRMock.setup((x) => x.SDA_CreatedTime).returns(() => new Date());
		sourceDataERRMock.setup((x) => x.SDA_PK).returns(() => uuid());

		let wrapperERR = mount<typeof KibanaLink>(
			<KibanaLink entity={sourceDataERRMock.object} />
		);
		// create link for ERR
		expect(wrapperERR.html()).not.toBeNull();

		let sourceDataFIEMock = Mock.ofType<ISourceDataUserView>();
		sourceDataFIEMock.setup((x) => x.SDA_Status).returns(() => "FIE");
		sourceDataFIEMock.setup((x) => x.SDA_CreatedTime).returns(() => new Date());
		sourceDataFIEMock.setup((x) => x.SDA_PK).returns(() => uuid());

		let wrapperFIE = mount<typeof KibanaLink>(
			<KibanaLink entity={sourceDataFIEMock.object} />
		);
		// create link for FIE
		expect(wrapperFIE.html()).not.toBeNull();
	});

	it("link construction ISourceDataUserView", () => {
		let sourceDataERRMock = Mock.ofType<ISourceDataUserView>();
		sourceDataERRMock.setup((x) => x.SDA_Status).returns(() => "ERR");
		sourceDataERRMock
			.setup((x) => x.SDA_CreatedTime)
			.returns(() => new Date("2024-04-10T10:00:00"));
		sourceDataERRMock
			.setup((x) => x.SDA_PK)
			.returns(() => "546988C8-8F63-4AE8-904F-786CDBD3AFBA");

		let wrapperERR = mount<typeof KibanaLink>(
			<KibanaLink entity={sourceDataERRMock.object} />
		);

		const newNode = document.createElement("div");
		newNode.innerHTML = wrapperERR.html();
		const link = newNode.querySelector("a");

		// create link for ERR
		expect(link?.outerHTML).toContain(
			"time:(from:'2024-04-10 09:30:00',to:'now')"
		);
		expect(link?.outerHTML).toContain(
			"query:(language:kuery,query:&quot;546988C8-8F63-4AE8-904F-786CDBD3AFBA&quot;)"
		);
		expect(link?.outerHTML).toContain(
			"_a=(columns:!(fields.AppLog.Message)"
		);
	});

	it("render IProcessorStatus", () => {
		let processorStatusMock = Mock.ofType<IProcessorStatus>();
		processorStatusMock
			.setup((x) => x.PRC_JobName)
			.returns(() => "Processor1");
		processorStatusMock
			.setup((x) => x.PRC_LastRunTime)
			.returns(() => new Date());
		processorStatusMock.setup((x) => x.PRC_PK).returns(() => uuid());

		let wrapper = mount<typeof KibanaLink>(
			<KibanaLink entity={processorStatusMock.object} />
		);
		expect(wrapper.html()).not.toBeNull();
	});

	it("link construction IProcessorStatus", () => {
		let processorStatusMock = Mock.ofType<IProcessorStatus>();
		processorStatusMock
			.setup((x) => x.PRC_LastRunTime)
			.returns(() => new Date("2024-04-10T10:00:00"));
		processorStatusMock
			.setup((x) => x.PRC_JobName)
			.returns(() => "Processor 1");
		processorStatusMock.setup((x) => x.PRC_PK).returns(() => uuid());

		let wrapperERR = mount<typeof KibanaLink>(
			<KibanaLink entity={processorStatusMock.object} />
		);

		const newNode = document.createElement("div");
		newNode.innerHTML = wrapperERR.html();
		const link = newNode.querySelector("a");

		expect(link?.outerHTML).toContain(
			"time:(from:'2024-04-10 09:30:00',to:'now')"
		);
		expect(link?.outerHTML).toContain(
			"key:fields.SourceContext,negate:!f,params:(query:'Processor 1'),type:phrase),query:(match_phrase:(fields.SourceContext:'Processor 1'))"
		);
		expect(link?.outerHTML).toContain(
			"query:(language:kuery,query:'')"
		);
		expect(link?.outerHTML).toContain(
			"_a=(columns:!(fields.AppLog.Message)"
		);
	});
});
