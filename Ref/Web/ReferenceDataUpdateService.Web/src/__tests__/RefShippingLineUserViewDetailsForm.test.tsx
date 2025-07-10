import { mount, shallow } from "enzyme";
import { Mock, It, Times } from "typemoq";
import { IEntityManager, ServiceType } from "../EntityManager";
import React from "react";
import { RefShippingLineUserViewDetailsForm } from "../RefShippingLineUserViewDetailsForm";
import { TextInput } from "../TextInput";
import { CheckBox } from "../CheckBox";
import { ValidationServiceWrapper } from "../ValidationService";
import IRefShippingLineMessagingRequirementType from "../models/IRefShippingLineMessagingRequirementType";
import uuid from "uuid";
import IRefShippingLineMessagingRequirement from "../models/IRefShippingLineMessagingRequirement";
import { RefShippingLineUserView } from "../models/RefShippingLineUserView";
import IRefShippingLineEBLProvider from "../models/IRefShippingLineEBLProvider";
import { Filter, FilterOps } from "../Filter";
import { act } from "react-dom/test-utils";
import { setImmediate } from "timers";
import axios from "axios";

jest.mock("axios");

describe("<RefShippingLineUserViewDetailsForm />", () => {
	it("Carrier Name is read-only", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup((x) => x.isInDatabase(It.isAny())).returns(() => true);

		let wrapper = shallow<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id="12345"
			/>
		);
		await wrapper.instance().componentDidMount();
		expect(wrapper.find(TextInput).at(2).props().readOnly).toBeTruthy();
	});

	it("render", async () => {
		let entityManager = Mock.ofType<IEntityManager>();

		let wrapper = shallow<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id="12345"
			/>
		);
		await wrapper.instance().componentDidMount();
		expect(wrapper.find(TextInput).at(0).props().propertyName).toEqual(
			"RSL_CargoWiseOneCode"
		);
		expect(wrapper.find(TextInput).at(1).props().propertyName).toEqual(
			"RSL_StandardCarrierAlphaCode"
		);
		expect(wrapper.find(TextInput).at(2).props().propertyName).toEqual(
			"RSL_CarrierName"
		);
		expect(wrapper.find(TextInput).at(3).props().propertyName).toEqual(
			"RSL_EHubIds"
		);
		expect(wrapper.find(CheckBox).at(0).props().propertyName).toEqual(
			"RSL_IsActive"
		);
		expect(wrapper.find(CheckBox).at(1).props().propertyName).toEqual(
			"RSL_IsNVO"
		);
		expect(wrapper.find(CheckBox).at(2).props().propertyName).toEqual(
			"RSL_IsShippingLine"
		);
		expect(wrapper.find(CheckBox).at(3).props().propertyName).toEqual(
			"RSL_IsCW1User"
		);
		expect(wrapper.find(CheckBox).at(4).props().propertyName).toEqual(
			"RSL_IsSystem"
		);
		expect(wrapper.find(CheckBox).at(5).props().propertyName).toEqual(
			"RSL_IsPublished"
		);
		expect(wrapper.find(CheckBox).at(6).props().propertyName).toEqual(
			"RSL_OceanCarrierMessagingAvailable"
		);
		expect(wrapper.find(CheckBox).at(7).props().propertyName).toEqual(
			"RSL_GlobalSailingScheduleAvailable"
		);
		expect(wrapper.find(CheckBox).at(8).props().propertyName).toEqual(
			"RSL_ContainerAutomationAvailable"
		);
		expect(wrapper.find(CheckBox).at(9).props().propertyName).toEqual(
			"RSL_CargoSphereRatesAvailable"
		);
		expect(wrapper.find(CheckBox).at(10).props().propertyName).toEqual(
			"RSL_InvoiceAvailable"
		);

		await wrapper
			.instance()
			.onValueChange(null, "RSL_OceanCarrierMessagingAvailable", true as any);
		expect(wrapper.find(CheckBox).at(6).props().propertyName).toEqual(
			"RSL_OceanCarrierMessagingAvailable"
		);
		expect(wrapper.find(CheckBox).at(7).props().propertyName).toEqual(
			"RSL_BookingRequestAvailable"
		);
		expect(wrapper.find(CheckBox).at(8).props().propertyName).toEqual(
			"RSL_ShippingInstructionAvailable"
		);
		expect(wrapper.find(CheckBox).at(9).props().propertyName).toEqual(
			"RSL_VerifiedGrossContainerWeightAvailable"
		);
		expect(wrapper.find(CheckBox).at(10).props().propertyName).toEqual(
			"RSL_ShippingOrderAvailable"
		);
		expect(wrapper.find(CheckBox).at(11).props().propertyName).toEqual(
			"RSL_EManifestAvailable"
		);
		expect(wrapper.find(CheckBox).at(12).props().propertyName).toEqual(
			"RSL_GlobalSailingScheduleAvailable"
		);
		expect(wrapper.find(CheckBox).at(13).props().propertyName).toEqual(
			"RSL_ContainerAutomationAvailable"
		);
		expect(wrapper.find(CheckBox).at(14).props().propertyName).toEqual(
			"RSL_CargoSphereRatesAvailable"
		);
		expect(wrapper.find(CheckBox).at(15).props().propertyName).toEqual(
			"RSL_InvoiceAvailable"
		);
	});

	it("onValueChange", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		let wrapper = shallow<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id="12345"
			/>
		);

		await wrapper.instance().componentDidMount();
		await wrapper
			.instance()
			.onValueChange(null, "RSL_CargoWiseOneCode", "AAA" as any);
		expect(wrapper.state().shippingLine.RSL_CargoWiseOneCode).toEqual("AAA");

		let shippingLine = Object.assign({}, shippingLines[0]);
		shippingLine.RSL_BookingRequestAvailable = true;
		let messagingRequirement = Object.assign({}, requirements[0]);
		entityManager
			.setup((x) =>
				x.getAsync<RefShippingLineUserView>(
					"RefShippingLineUserView",
					[ServiceType.Safe],
					It.isAny(),
					false,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve([shippingLine]));
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineMessagingRequirementType>(
					"RefShippingLineMessagingRequirementType",
					[ServiceType.Safe],
					It.isAny(),
					false,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve(requirementTypes));
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineMessagingRequirement>(
					"RefShippingLineMessagingRequirement",
					[ServiceType.Safe],
					It.isAny(),
					true,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve([messagingRequirement]));
		wrapper = shallow<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id="12345"
			/>
		);

		await wrapper.instance().componentDidMount();
		await wrapper
			.instance()
			.onValueChange(null, "RSL_OceanCarrierMessagingAvailable", false as any);
		expect(wrapper.state().shippingLine.RSL_BookingRequestAvailable).toEqual(
			false
		);
		expect(
			wrapper.state().shippingLine.RSL_ShippingInstructionAvailable
		).toEqual(false);
		expect(
			wrapper.state().shippingLine.RSL_VerifiedGrossContainerWeightAvailable
		).toEqual(false);
		expect(wrapper.state().shippingLine.RSL_ShippingOrderAvailable).toEqual(
			false
		);
		expect(wrapper.state().shippingLine.RSL_EManifestAvailable).toEqual(false);

		messagingRequirement.RSR_IsBookingRequest = true;
		await wrapper
			.instance()
			.onValueChange(null, "RSL_OceanCarrierMessagingAvailable", true as any);
		expect(wrapper.state().shippingLine.RSL_BookingRequestAvailable).toEqual(
			true
		);
	});

	it("onValueChanged", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		let wrapper = shallow<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id="12345"
			/>
		);
		let validationService = {
			RSL_CargoWiseOneCode: [(e: any, p: any) => "There is an error"],
		};
		wrapper.instance().validationService = new ValidationServiceWrapper(
			[validationService],
			(isValidating: boolean) => void {}
		);

		await wrapper.instance().componentDidMount();
		await wrapper.instance().onValueChanged(null, "RSL_CargoWiseOneCode");
		expect(wrapper.state().validationResults["RSL_CargoWiseOneCode"]).toEqual([
			"There is an error",
		]);
	});

	it("onValueChanged_RSL_IsActive", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		let wrapper = shallow<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id="12345"
			/>
		);
		let validationService = {
			RSL_CargoWiseOneCode: [(e: any, p: any) => "There is an error"],
		};
		wrapper.instance().validationService = new ValidationServiceWrapper(
			[validationService],
			(isValidating: boolean) => void {}
		);

		await wrapper.instance().componentDidMount();
		await wrapper.instance().onValueChanged(null, "RSL_IsActive");
		expect(wrapper.state().validationResults["RSL_CargoWiseOneCode"]).toEqual([
			"There is an error",
		]);
	});

	it("onOceanCarrierMessagingAvailableTicked", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync<RefShippingLineUserView>(
					"RefShippingLineUserView",
					[ServiceType.Safe],
					It.isAny(),
					false,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve(shippingLines));
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineMessagingRequirementType>(
					"RefShippingLineMessagingRequirementType",
					[ServiceType.Safe],
					It.isAny(),
					false,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve(requirementTypes));
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineMessagingRequirement>(
					"RefShippingLineMessagingRequirement",
					[ServiceType.Safe],
					It.isAny(),
					true,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve(requirements));
		let wrapper = shallow<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id="123"
			/>
		);

		await wrapper.instance().componentDidMount();
		await wrapper
			.instance()
			.onValueChange(null, "RSL_OceanCarrierMessagingAvailable", false as any);
		expect(
			wrapper.state().shippingLine.RSL_OceanCarrierMessagingAvailable
		).toBe(false);
		expect(wrapper.instance().shouldShowMessagingRequirements()).toBe(false);
		expect(wrapper.instance().shouldShowIntegrations()).toBe(false);

		await wrapper
			.instance()
			.onValueChange(null, "RSL_OceanCarrierMessagingAvailable", true as any);
		expect(
			wrapper.state().shippingLine.RSL_OceanCarrierMessagingAvailable
		).toBe(true);
		expect(wrapper.instance().shouldShowMessagingRequirements()).toBe(true);
		expect(wrapper.instance().shouldShowIntegrations()).toBe(true);
	});

	it("updateMessagingRequirements", async () => {
		let entityManager = Mock.ofType<IEntityManager>();
		let shippingLine = Object.assign({}, shippingLines[0]);
		let messagingRequirement = Object.assign({}, requirements[0]);
		entityManager
			.setup((x) =>
				x.getAsync<RefShippingLineUserView>(
					"RefShippingLineUserView",
					[ServiceType.Safe],
					It.isAny(),
					false,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve([shippingLine]));
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineMessagingRequirementType>(
					"RefShippingLineMessagingRequirementType",
					[ServiceType.Safe],
					It.isAny(),
					false,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve(requirementTypes));
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineMessagingRequirement>(
					"RefShippingLineMessagingRequirement",
					[ServiceType.Safe],
					It.isAny(),
					true,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve([messagingRequirement]));
		let wrapper = shallow<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id="123"
			/>
		);

		shippingLine.RSL_OceanCarrierMessagingAvailable = true;
		messagingRequirement.RSR_IsBookingRequest = true;
		messagingRequirement.RSR_IsShippingInstruction = true;
		messagingRequirement.RSR_IsShippingOrder = true;
		messagingRequirement.RSR_IsEManifest = true;
		messagingRequirement.RSR_IsVerifiedGrossContainerWeight = true;
		await wrapper.instance().componentDidMount();
		await wrapper
			.instance()
			.updateMessagingRequirements(shippingLine, "RSL_BookingRequestAvailable");
		wrapper.state().messagingRequirements.forEach((x) => {
			expect(x.RSR_IsBookingRequest).toEqual(false);
			expect(x.RSR_IsShippingInstruction).toEqual(true);
			expect(x.RSR_IsShippingOrder).toEqual(true);
			expect(x.RSR_IsEManifest).toEqual(true);
			expect(x.RSR_IsVerifiedGrossContainerWeight).toEqual(true);
		});

		await wrapper
			.instance()
			.updateMessagingRequirements(
				shippingLine,
				"RSL_ShippingInstructionAvailable"
			);
		wrapper.state().messagingRequirements.forEach((x) => {
			expect(x.RSR_IsBookingRequest).toEqual(false);
			expect(x.RSR_IsShippingInstruction).toEqual(false);
			expect(x.RSR_IsShippingOrder).toEqual(true);
			expect(x.RSR_IsEManifest).toEqual(true);
			expect(x.RSR_IsVerifiedGrossContainerWeight).toEqual(true);
		});
		await wrapper
			.instance()
			.updateMessagingRequirements(shippingLine, "RSL_ShippingOrderAvailable");
		wrapper.state().messagingRequirements.forEach((x) => {
			expect(x.RSR_IsBookingRequest).toEqual(false);
			expect(x.RSR_IsShippingInstruction).toEqual(false);
			expect(x.RSR_IsShippingOrder).toEqual(false);
			expect(x.RSR_IsEManifest).toEqual(true);
			expect(x.RSR_IsVerifiedGrossContainerWeight).toEqual(true);
		});
		await wrapper
			.instance()
			.updateMessagingRequirements(shippingLine, "RSL_EManifestAvailable");
		wrapper.state().messagingRequirements.forEach((x) => {
			expect(x.RSR_IsBookingRequest).toEqual(false);
			expect(x.RSR_IsShippingInstruction).toEqual(false);
			expect(x.RSR_IsShippingOrder).toEqual(false);
			expect(x.RSR_IsEManifest).toEqual(false);
			expect(x.RSR_IsVerifiedGrossContainerWeight).toEqual(true);
		});
		await wrapper
			.instance()
			.updateMessagingRequirements(
				shippingLine,
				"RSL_VerifiedGrossContainerWeightAvailable"
			);
		wrapper.state().messagingRequirements.forEach((x) => {
			expect(x.RSR_IsBookingRequest).toEqual(false);
			expect(x.RSR_IsShippingInstruction).toEqual(false);
			expect(x.RSR_IsShippingOrder).toEqual(false);
			expect(x.RSR_IsEManifest).toEqual(false);
			expect(x.RSR_IsVerifiedGrossContainerWeight).toEqual(false);
		});
	});

	it("loadMessagingRequirementsWithoutRequirement", async () => {
		let myRequirementTypes: IRefShippingLineMessagingRequirementType[] = [
			{
				RST_Code: "123",
				RST_Description: "TST1",
				RST_PK: uuid(),
			},
			{
				RST_Code: "321",
				RST_Description: "TST2",
				RST_PK: uuid(),
			},
		];

		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync<RefShippingLineUserView>(
					"RefShippingLineUserView",
					[ServiceType.Safe],
					It.isAny(),
					false,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve(shippingLines));
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineMessagingRequirementType>(
					"RefShippingLineMessagingRequirementType",
					[ServiceType.Safe],
					It.isAny(),
					false,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve(myRequirementTypes));
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineMessagingRequirement>(
					"RefShippingLineMessagingRequirement",
					[ServiceType.Safe],
					It.isAny(),
					true,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve(requirements));

		let wrapper = shallow<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id="123"
			/>
		);

		await wrapper.instance().componentDidMount();
		wrapper.instance().setState({ hasLoadedMessagingRequirements: false });
		await wrapper
			.instance()
			.onValueChange(null, "RSL_OceanCarrierMessagingAvailable", true as any);

		expect(
			wrapper
				.state()
				.messagingRequirements.find((x) => x.RSR_RST_NKType == "321")
				?.RSR_IsShippingInstruction
		).toEqual(false);
		expect(
			wrapper
				.state()
				.messagingRequirements.find((x) => x.RSR_RST_NKType == "321")
				?.RSR_IsBookingRequest
		).toEqual(false);
	});

	it("AfterSavingloadShippingLineFunctionShouldNotBecalled", async () => {
		let refShippingLines: RefShippingLineUserView[] = [
			{
				RSL_CargoSphereRatesAvailable: false,
				RSL_CargoWiseOneCode: "code",
				RSL_CarrierName: "carrierName",
				RSL_ContainerAutomationAvailable: false,
				RSL_PK: "123",
				RSL_EHubIds: "abcd",
				RSL_GlobalSailingScheduleAvailable: false,
				RSL_InvoiceAvailable: false,
				RSL_OceanCarrierMessagingAvailable: false,
				RSL_IsActive: true,
				RSL_IsCW1User: true,
				RSL_IsEditable: true,
				RSL_IsNVO: true,
				RSL_IsPublished: true,
				RSL_IsSystem: true,
				RSL_StandardCarrierAlphaCode: "code",
				RSL_BookingRequestAvailable: false,
				RSL_ShippingInstructionAvailable: false,
				RSL_VerifiedGrossContainerWeightAvailable: false,
				RSL_ShippingOrderAvailable: false,
				RSL_EManifestAvailable: false,
				RSL_IsShippingLine: false,
				RSL_ShippingLineLogo: new Uint8Array(),
			},
		];
		let validationService = {};
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager
			.setup((x) =>
				x.getAsync<RefShippingLineUserView>(
					"RefShippingLineUserView",
					[ServiceType.Safe],
					It.isAny(),
					false,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve(refShippingLines));
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineMessagingRequirementType>(
					"RefShippingLineMessagingRequirementType",
					[ServiceType.Safe],
					It.isAny(),
					false,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve(requirementTypes));
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineMessagingRequirement>(
					"RefShippingLineMessagingRequirement",
					[ServiceType.Safe],
					It.isAny(),
					true,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve(requirements));
		entityManager
			.setup((x) => x.saveChanges(It.isAny()))
			.returns(() => Promise.resolve({ success: true, message: "saved" }));
		let wrapper = shallow<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id="123"
			/>
		);
		wrapper.instance().validationService = new ValidationServiceWrapper(
			[validationService],
			(isValidating: boolean) => void {}
		);

		const instance = wrapper.instance();
		instance.setState({ hasLoadedMessagingRequirements: false });
		await instance.onValueChange(
			null,
			"RSL_OceanCarrierMessagingAvailable",
			true as any
		);
		await instance.onValueChanged(null, "RSL_OceanCarrierMessagingAvailable");
		const loadShippingLine = jest.spyOn(instance, "loadShippingLine");
		await instance.save();
		expect(loadShippingLine).toBeCalledTimes(1);
	});

	it("calls entityManager.reload for all entities on unmount", async () => {
        const entityManager = Mock.ofType<IEntityManager>();
        const reloadMock = jest.fn().mockResolvedValue(undefined);

        entityManager.setup(x => x.reload(
            "RefShippingLineUserView",
            [ServiceType.Safe],
            It.isAny()
        )).returns(reloadMock);
        entityManager.setup(x => x.reload(
            "RefShippingLineMessagingRequirement",
            [ServiceType.Safe],
            It.isAny()
        )).returns(reloadMock);
        entityManager.setup(x => x.reload(
            "RefShippingLineEBLProvider",
            [ServiceType.Safe],
            It.isAny()
        )).returns(reloadMock);

		const wrapper = shallow<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm entityManager={entityManager.object} id="123" />
		);

		wrapper.setState({ shippingLine: {
				RSL_CargoSphereRatesAvailable: false,
				RSL_CargoWiseOneCode: "code",
				RSL_CarrierName: "carrierName",
				RSL_ContainerAutomationAvailable: false,
				RSL_PK: "123",
				RSL_EHubIds: "abcd",
				RSL_GlobalSailingScheduleAvailable: false,
				RSL_InvoiceAvailable: false,
				RSL_OceanCarrierMessagingAvailable: false,
				RSL_IsActive: true,
				RSL_IsCW1User: true,
				RSL_IsEditable: true,
				RSL_IsNVO: true,
				RSL_IsPublished: true,
				RSL_IsSystem: true,
				RSL_StandardCarrierAlphaCode: "code",
				RSL_BookingRequestAvailable: false,
				RSL_ShippingInstructionAvailable: false,
				RSL_VerifiedGrossContainerWeightAvailable: false,
				RSL_ShippingOrderAvailable: false,
				RSL_EManifestAvailable: false,
				RSL_IsShippingLine: false,
				RSL_ShippingLineLogo: new Uint8Array(),
		} });

		await wrapper.instance().componentWillUnmount();

		expect(reloadMock).toHaveBeenCalledWith(
			"RefShippingLineUserView",
			[ServiceType.Safe],
			[expect.objectContaining({ propertyName: "RSL_PK", operation: FilterOps.Equals, value: "123", type: "guid" })]
		);
		expect(reloadMock).toHaveBeenCalledWith(
			"RefShippingLineMessagingRequirement",
			[ServiceType.Safe],
			[expect.objectContaining({ propertyName: "RSR_RSL_ShippingLine", operation: FilterOps.Equals, value: "123", type: "guid" })]
		);
		expect(reloadMock).toHaveBeenCalledWith(
			"RefShippingLineEBLProvider",
			[ServiceType.Safe],
			[expect.objectContaining({ propertyName: "RSE_RSL_ShippingLine", operation: FilterOps.Equals, value: "123", type: "guid" })]
		);
	});
});

describe("EBLProviders", () => {
	let entityManager = Mock.ofType<IEntityManager>();

	beforeEach(() => {
		entityManager.reset();
		entityManager
			.setup((x) =>
				x.getAsync<RefShippingLineUserView>(
					"RefShippingLineUserView",
					[ServiceType.Safe],
					It.isAny(),
					It.isAny(),
					It.isAny()
				)
			)
			.returns(() => Promise.resolve<RefShippingLineUserView[]>(shippingLines));
	});

	it("loadShippingLine with existing shippingLine loads eblProviders and orders by RSE_Name", async () => {
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineEBLProvider>(
					"RefShippingLineEBLProvider",
					It.isAny(),
					It.isAny(),
					It.isAny(),
					It.isAny(),
					It.isAny()
				)
			)
			.returns(() => Promise.resolve([]));

		let wrapper = mount<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id={shippingLines[0].RSL_PK}
			/>
		);

		//let component actually mount.
		await act(() => new Promise(setImmediate));

		await wrapper.instance().loadShippingLine();

		entityManager.verify(
			(x) =>
				x.getAsync<IRefShippingLineEBLProvider>(
					"RefShippingLineEBLProvider",
					[ServiceType.Safe],
					[
						new Filter(
							"RSE_RSL_ShippingLine",
							FilterOps.Equals,
							shippingLines[0].RSL_PK as any,
							"guid"
						),
					],
					true,
					wrapper.instance().props.systemVersion,
					{ orderBy: ["RSE_Name"] }
				),
			Times.atLeastOnce()
		);
	});

	it("loadShippingLine with non-existent shippingLine does not load eblProviders", async () => {
		let entityManagerNoShippingLine = Mock.ofType<IEntityManager>();
		let wrapper = mount<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManagerNoShippingLine.object}
				id={shippingLines[0].RSL_PK}
			/>
		);

		//let component actually mount.
		await act(() => new Promise(setImmediate));

		await wrapper.instance().loadShippingLine();

		entityManagerNoShippingLine.verify(
			(x) =>
				x.getAsync<IRefShippingLineEBLProvider>(
					"RefShippingLineEBLProvider",
					[ServiceType.Safe],
					[
						new Filter(
							"RSE_RSL_ShippingLine",
							FilterOps.Equals,
							shippingLines[0].RSL_PK as any,
							"guid"
						),
					],
					true,
					It.isAny(),
					It.isAny()
				),
			Times.never()
		);
	});

	it("button add is clicked and handleAddEBLProviderItem is called", async () => {
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineEBLProvider>(
					"RefShippingLineEBLProvider",
					It.isAny(),
					It.isAny(),
					It.isAny(),
					It.isAny(),
					It.isAny()
				)
			)
			.returns(() => Promise.resolve([]));

		let wrapper = mount<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id={shippingLines[0].RSL_PK}
			/>
		);

		//let component actually mount.
		await act(() => new Promise(setImmediate));

		//before adding, state records must be empty
		expect(wrapper.instance().state.eBLProviders.length).toBe(0);
		wrapper.instance().handleAddEBLProviderItem();

		expect(wrapper.instance().state.eBLProviders.length).toBe(1);
		//add to entity manager is called
		entityManager.verify(
			(x) => x.add(It.isAny(), "RefShippingLineEBLProvider"),
			Times.atLeastOnce()
		);

		//initialLoad is set to false
		expect(wrapper.instance().isInitialLoad).toBeFalsy();
	});

	it("button add is disabled and existing eBL Providers are removed when RSR_IsShippingInstruction (BLP) is unticked", async () => {
		let providerItem = {
			RSE_IsAvailable: false,
			RSE_IsDefault: false,
			RSE_Name: "name1",
			RSE_PK: "any",
			RSE_RSL_ShippingLine: shippingLines[0].RSL_PK,
		};
		let requirementType: IRefShippingLineMessagingRequirementType = {
			RST_Code: "BLP",
			RST_Description: "Blp",
			RST_PK: uuid(),
		};
		let requirement: IRefShippingLineMessagingRequirement = {
			RSR_IsEManifest: false,
			RSR_IsVerifiedGrossContainerWeight: false,
			RSR_IsBookingRequest: false,
			RSR_IsShippingInstruction: true, //set to true
			RSR_IsShippingOrder: false,
			RSR_PK: uuid(),
			RSR_RSL_ShippingLine: shippingLines[0].RSL_PK,
			RSR_RST_NKType: "BLP",
		};
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineEBLProvider>(
					"RefShippingLineEBLProvider",
					It.isAny(),
					It.isAny(),
					It.isAny(),
					It.isAny(),
					It.isAny()
				)
			)
			.returns(() => Promise.resolve([providerItem]));
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineMessagingRequirementType>(
					"RefShippingLineMessagingRequirementType",
					[ServiceType.Safe],
					It.isAny(),
					false,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve([requirementType]));
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineMessagingRequirement>(
					"RefShippingLineMessagingRequirement",
					[ServiceType.Safe],
					It.isAny(),
					true,
					It.isAny()
				)
			)
			.returns(() => Promise.resolve([requirement]));

		let wrapper = mount<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id={shippingLines[0].RSL_PK}
			/>
		);

		//let component actually mount.
		await act(() => new Promise(setImmediate));

		//must have one record
		expect(wrapper.instance().state.eBLProviders.length).toBe(1);
		//button add should be enabled
		expect(wrapper.instance().state.eBLProvidersAddButtonDisabled).toBeFalsy();

		//untick RSR_IsShippingInstruction
		await wrapper
			.instance()
			.onMessagingRequirementChange(
				requirement,
				"RSR_IsShippingInstruction",
				false as any
			);

		//empty list of providers.
		expect(wrapper.instance().state.eBLProviders.length).toBe(0);
		//button add should be disabled
		expect(wrapper.instance().state.eBLProvidersAddButtonDisabled).toBeTruthy();

		//tick RSR_IsShippingInstruction back
		await wrapper
			.instance()
			.onMessagingRequirementChange(
				requirement,
				"RSR_IsShippingInstruction",
				true as any
			);

		//must have one record coming back
		expect(wrapper.instance().state.eBLProviders.length).toBe(1);
		//button add should be enabled again
		expect(wrapper.instance().state.eBLProvidersAddButtonDisabled).toBeFalsy();
	});

	it("button '-' is clicked and handleDeleteEBLProviderItem is called", async () => {
		let providerItem = {
			RSE_IsAvailable: false,
			RSE_IsDefault: false,
			RSE_Name: "name1",
			RSE_PK: "any",
			RSE_RSL_ShippingLine: shippingLines[0].RSL_PK,
		};
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineEBLProvider>(
					"RefShippingLineEBLProvider",
					It.isAny(),
					It.isAny(),
					It.isAny(),
					It.isAny(),
					It.isAny()
				)
			)
			.returns(() =>
				Promise.resolve<IRefShippingLineEBLProvider[]>([providerItem])
			);

		let wrapper = mount<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id={shippingLines[0].RSL_PK}
			/>
		);

		//let component actually mount.
		await act(() => new Promise(setImmediate));

		//before deleting, state records must have one record
		expect(wrapper.instance().state.eBLProviders.length).toBe(1);
		wrapper.instance().handleDeleteEBLProviderItem("any"); // where 'any' is my item pk

		expect(wrapper.instance().state.eBLProviders.length).toBe(0);
		//remove to entity manager is called
		entityManager.verify((x) => x.remove(providerItem), Times.atLeastOnce());

		//initialLoad is set to false
		expect(wrapper.instance().isInitialLoad).toBeFalsy();
	});

	it("eblProvider name is required", async () => {
		let providerItemEmptyName = {
			RSE_IsAvailable: false,
			RSE_IsDefault: false,
			RSE_Name: "",
			RSE_PK: "any",
			RSE_RSL_ShippingLine: shippingLines[0].RSL_PK,
		};
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineEBLProvider>(
					"RefShippingLineEBLProvider",
					It.isAny(),
					It.isAny(),
					It.isAny(),
					It.isAny(),
					It.isAny()
				)
			)
			.returns(() =>
				Promise.resolve<IRefShippingLineEBLProvider[]>([providerItemEmptyName])
			);

		let wrapper = mount<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id={shippingLines[0].RSL_PK}
			/>
		);

		//let component actually mount.
		await act(() => new Promise(setImmediate));
		await wrapper
			.instance()
			.onEBLProviderItemChanged(providerItemEmptyName, "RSE_Name");

		//assert validation message
		expect(
			wrapper.state().eblProviderValidationResults[
				providerItemEmptyName.RSE_PK
			]["RSE_Name"][0]
		).toBe("This field is required.");
	});

	it("eblProvider name cannot be equal for a given shipping line", async () => {
		let providerItem = {
			RSE_IsAvailable: false,
			RSE_IsDefault: false,
			RSE_Name: "name1",
			RSE_PK: "any",
			RSE_RSL_ShippingLine: shippingLines[0].RSL_PK,
		};
		entityManager
			.setup((x) =>
				x.getAsync<IRefShippingLineEBLProvider>(
					"RefShippingLineEBLProvider",
					It.isAny(),
					It.isAny(),
					It.isAny(),
					It.isAny(),
					It.isAny()
				)
			)
			.returns(() =>
				Promise.resolve<IRefShippingLineEBLProvider[]>([providerItem])
			);

		let wrapper = mount<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id={shippingLines[0].RSL_PK}
			/>
		);

		//let component actually mount.
		await act(() => new Promise(setImmediate));
		wrapper.instance().handleAddEBLProviderItem();

		const newProvider = wrapper.instance().state.eBLProviders[1];
		wrapper
			.instance()
			.onEBLProviderItemChange(newProvider, "RSE_Name", "name1" as any);
		await wrapper.instance().onEBLProviderItemChanged(newProvider, "RSE_Name");

		//assert validation message
		expect(
			wrapper.state().eblProviderValidationResults[newProvider.RSE_PK][
				"RSE_Name"
			][0]
		).toBe(
			`RefShippingLineEBLProvider (name1,${shippingLines[0].RSL_PK}) already exists.`
		);
	});

	it("getDistinctByName is called on component mounting, call to axios api and save to state", async () => {
		axios.get = jest
			.fn()
			.mockReturnValueOnce({ data: ["Name1", "Name2", "Name3"] });

		let wrapper = mount<RefShippingLineUserViewDetailsForm>(
			<RefShippingLineUserViewDetailsForm
				entityManager={entityManager.object}
				id={shippingLines[0].RSL_PK}
			/>
		);

		//let component actually mount.
		await act(() => new Promise(setImmediate));

		expect(wrapper.state().eblProviderDistinctNames).toHaveLength(3);
	});
});

var shippingLines: RefShippingLineUserView[] = [
	{
		RSL_CargoSphereRatesAvailable: false,
		RSL_CargoWiseOneCode: "code",
		RSL_CarrierName: "carrierName",
		RSL_ContainerAutomationAvailable: false,
		RSL_PK: uuid(),
		RSL_EHubIds: "",
		RSL_GlobalSailingScheduleAvailable: false,
		RSL_InvoiceAvailable: false,
		RSL_OceanCarrierMessagingAvailable: false,
		RSL_IsActive: true,
		RSL_IsCW1User: true,
		RSL_IsEditable: true,
		RSL_IsNVO: true,
		RSL_IsPublished: true,
		RSL_IsSystem: true,
		RSL_StandardCarrierAlphaCode: "code",
		RSL_BookingRequestAvailable: false,
		RSL_ShippingInstructionAvailable: false,
		RSL_VerifiedGrossContainerWeightAvailable: false,
		RSL_ShippingOrderAvailable: false,
		RSL_EManifestAvailable: false,
		RSL_IsShippingLine: false,
		RSL_ShippingLineLogo: new Uint8Array(),
	},
];

var requirementTypes: IRefShippingLineMessagingRequirementType[] = [
	{
		RST_Code: "123",
		RST_Description: "TST1",
		RST_PK: uuid(),
	},
];
var requirements: IRefShippingLineMessagingRequirement[] = [
	{
		RSR_IsEManifest: false,
		RSR_IsVerifiedGrossContainerWeight: false,
		RSR_IsBookingRequest: false,
		RSR_IsShippingInstruction: false,
		RSR_IsShippingOrder: false,
		RSR_PK: uuid(),
		RSR_RSL_ShippingLine: "123",
		RSR_RST_NKType: "123",
	},
];
