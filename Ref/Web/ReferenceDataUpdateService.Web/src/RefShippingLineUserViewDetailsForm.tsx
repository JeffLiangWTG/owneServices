import React from "react";
import uuid from "uuid";
import { IEntityManager, ServiceType } from "./EntityManager";
import _ from "underscore";
import { Filter, FilterOps } from "./Filter";
import update from "immutability-helper";
import { TextInput } from "./TextInput";
import { CheckBox } from "./CheckBox";
import { ValidationServiceWrapper } from "./ValidationService";
import { IValidationResults } from "./ValidationResults";
import { ValidationHelper } from "./ValidationHelper";
import { ValidationServiceHelper } from "./ValidationServiceHelper";
import { PopupForm } from "./PopupForm";
import { IParentProps } from "./IParentProps";
import { RefShippingLineUserView } from "./models/RefShippingLineUserView";
import { RefShippingLineMessagingRequirementTypeForm } from "./RefShippingLineMessagingRequirementTypeForm";
import IRefShippingLineMessagingRequirementType from "./models/IRefShippingLineMessagingRequirementType";
import IRefShippingLineMessagingRequirement from "./models/IRefShippingLineMessagingRequirement";
import { RefShippingLineEBLProviders } from "./RefShippingLineEBLProviders";
import IRefShippingLineEBLProvider, {
	RefShippingLineEBLProviderName,
	RefShippingLineEBLProviderWrapper,
} from "./models/IRefShippingLineEBLProvider";
import axios from "axios";
import Button from "./Button";

export interface IRefShippingLineUserViewDetailsFormProps {
	id?: string;
	systemVersion?: string;
	entityManager: IEntityManager;
	onSaved?: () => Promise<void>;
}

interface IRefShippingLineUserViewDetailsFormState extends IParentProps {
	shippingLine: RefShippingLineUserView;
	messagingRequirementTypes: IRefShippingLineMessagingRequirementType[];
	messagingRequirements: IRefShippingLineMessagingRequirement[];
	validationResults: IValidationResults;
	eblProviderValidationResults: {
		[pk: string]: IValidationResults;
	};
	saveMessage: string;
	saveButtonDisabled: boolean;
	hasLoadedMessagingRequirements: boolean;
	eBLProviders: RefShippingLineEBLProviderWrapper[];
	eBLProvidersAddButtonDisabled: boolean;
	eblProviderDistinctNames: RefShippingLineEBLProviderName[];
}

declare var __SafeAPI__: string;

export class RefShippingLineUserViewDetailsForm extends React.Component<
	IRefShippingLineUserViewDetailsFormProps,
	IRefShippingLineUserViewDetailsFormState
> {
	constructor(props: IRefShippingLineUserViewDetailsFormProps) {
		super(props);
		this.onValueChange = this.onValueChange.bind(this);
		this.onValueChanged = this.onValueChanged.bind(this);
		this.handleAddEBLProviderItem = this.handleAddEBLProviderItem.bind(this);
		this.handleDeleteEBLProviderItem =
			this.handleDeleteEBLProviderItem.bind(this);
		this.onEBLProviderItemChange = this.onEBLProviderItemChange.bind(this);
		this.onEBLProviderItemChanged = this.onEBLProviderItemChanged.bind(this);
		this.onMessagingRequirementChange =
			this.onMessagingRequirementChange.bind(this);
		this.save = this.save.bind(this);
		this.updateSaveButtonDisabledProperty =
			this.updateSaveButtonDisabledProperty.bind(this);
		this.updateMessagingRequirements =
			this.updateMessagingRequirements.bind(this);
		let shippingLineView = new RefShippingLineUserView();
		shippingLineView.RSL_PK = uuid.v1();
		shippingLineView.RSL_IsActive = true;
		shippingLineView.RSL_IsNVO = false;
		shippingLineView.RSL_CarrierName = "";
		shippingLineView.RSL_StandardCarrierAlphaCode = "";
		shippingLineView.RSL_CargoWiseOneCode = "";
		shippingLineView.RSL_OceanCarrierMessagingAvailable = false;
		shippingLineView.RSL_GlobalSailingScheduleAvailable = false;
		shippingLineView.RSL_ContainerAutomationAvailable = false;
		shippingLineView.RSL_CargoSphereRatesAvailable = false;
		shippingLineView.RSL_InvoiceAvailable = false;
		shippingLineView.RSL_IsCW1User = false;
		shippingLineView.RSL_EHubIds = "";
		shippingLineView.RSL_IsSystem = true;
		shippingLineView.RSL_IsPublished = true;
		shippingLineView.RSL_IsEditable = true;
		shippingLineView.RSL_BookingRequestAvailable = false;
		shippingLineView.RSL_ShippingInstructionAvailable = false;
		shippingLineView.RSL_VerifiedGrossContainerWeightAvailable = false;
		shippingLineView.RSL_ShippingOrderAvailable = false;
		shippingLineView.RSL_EManifestAvailable = false;
		shippingLineView.RSL_IsShippingLine = false;

		this.state = {
			shippingLine: shippingLineView,
			validationResults: {},
			eblProviderValidationResults: {},
			saveMessage: "",
			readOnly: false,
			saveButtonDisabled: false,
			messagingRequirementTypes: [],
			messagingRequirements: [],
			hasLoadedMessagingRequirements: false,
			eBLProviders: [],
			eBLProvidersAddButtonDisabled: true,
			eblProviderDistinctNames: [],
		};
		this.validationService = new ValidationServiceWrapper(
			[
				ValidationServiceHelper.getRefShippingLineValidationService(
					this.props.entityManager
				),
				ValidationServiceHelper.getRefShippingLineEBLProviderValidationService(
					this.props.entityManager
				),
			],
			this.updateSaveButtonDisabledProperty
		);
		this.isInitialLoad = true;
		this.safeApi = __SafeAPI__.replace("odata", "api");
		this.axiosConfig = {
			headers: { "Content-Type": "application/json" },
		};
	}

	validationService: ValidationServiceWrapper;
	isInitialLoad: boolean;
	safeApi: string;
	axiosConfig: any;

	updateSaveButtonDisabledProperty(isValidating: boolean) {
		this.setState({ saveButtonDisabled: isValidating });
	}

	async componentDidMount() {
		await this.loadShippingLine();
	}

	async componentDidUpdate(
		prevProps: IRefShippingLineUserViewDetailsFormProps
	) {
		if (prevProps.systemVersion !== this.props.systemVersion) {
			this.props.entityManager.clear();
			await this.loadShippingLine();
		}
	}

	// This is a temporary solution util we convert the class to functional component
	async componentWillUnmount() {
		await Promise.all([
			this.props.entityManager.reload("RefShippingLineUserView", [ServiceType.Safe], [
						new Filter(
							"RSL_PK",
							FilterOps.Equals,
							this.props.id
								? this.props.id
								: (this.state.shippingLine.RSL_PK as any),
							"guid"
						),
					]),
			this.props.entityManager.reload("RefShippingLineMessagingRequirement", [ServiceType.Safe], [
				new Filter(
					"RSR_RSL_ShippingLine",
					FilterOps.Equals,
					this.state.shippingLine.RSL_PK as any,
					"guid"
				)
			]),
			this.props.entityManager.reload("RefShippingLineEBLProvider", [ServiceType.Safe], [
				new Filter(
					"RSE_RSL_ShippingLine",
					FilterOps.Equals,
					this.state.shippingLine.RSL_PK as any,
					"guid"
				)
			])
		]);
	}

	async loadShippingLine() {
		if (
			this.props.id ||
			this.props.entityManager.isInDatabase(this.state.shippingLine)
		) {
			this.setState({ readOnly: this.props.systemVersion !== undefined });
			var dataArray = await Promise.all([
				this.props.entityManager.getAsync<RefShippingLineUserView>(
					"RefShippingLineUserView",
					[ServiceType.Safe],
					[
						new Filter(
							"RSL_PK",
							FilterOps.Equals,
							this.props.id
								? this.props.id
								: (this.state.shippingLine.RSL_PK as any),
							"guid"
						),
					],
					false,
					this.props.systemVersion
				),
				this.props.entityManager.getAsync<IRefShippingLineMessagingRequirementType>(
					"RefShippingLineMessagingRequirementType",
					[ServiceType.Safe],
					[],
					false,
					this.props.systemVersion
				),
			]);
			let shippingLine = _.first(dataArray[0]);
			if (shippingLine) {
				let requirementTypesAndRequirements =
					await this.getMessagingRequirementTypesAndRequirements(
						shippingLine.RSL_PK
					);
				let eBLProviders = await this.getEBLProviders(shippingLine.RSL_PK);
				let eBLProvidersNamesDistinct =
					await this.getEBLProviderNamesDistinct();
				this.setState({
					shippingLine: shippingLine,
					messagingRequirementTypes: requirementTypesAndRequirements[0],
					messagingRequirements: requirementTypesAndRequirements[1],
					hasLoadedMessagingRequirements: true,
					eblProviderDistinctNames: eBLProvidersNamesDistinct,

					eBLProviders: eBLProviders,
				});
			}
		} else {
			this.props.entityManager.add(
				this.state.shippingLine,
				"RefShippingLineUserView"
			);
			let eBLProvidersNamesDistinct = await this.getEBLProviderNamesDistinct();
			let requirementTypesAndRequirements =
				await this.getMessagingRequirementTypesAndRequirements(
					this.state.shippingLine.RSL_PK
				);
			this.setState({
				messagingRequirementTypes: requirementTypesAndRequirements[0],
				messagingRequirements: requirementTypesAndRequirements[1],
				hasLoadedMessagingRequirements: true,

				eblProviderDistinctNames: eBLProvidersNamesDistinct,
			});
		}
	}

	async getEBLProviderNamesDistinct(): Promise<
		RefShippingLineEBLProviderName[]
	> {
		let result = await axios.get(
			`${this.safeApi}RefShippingLineEBLProvider/GetDistinctNames`,
			this.axiosConfig
		);
		return result && result.data
			? (result.data as any[]).map((x) => new RefShippingLineEBLProviderName(x))
			: [];
	}

	async getEBLProviders(
		shippingLinePk: string
	): Promise<RefShippingLineEBLProviderWrapper[]> {
		const providers =
			await this.props.entityManager.getAsync<IRefShippingLineEBLProvider>(
				"RefShippingLineEBLProvider",
				[ServiceType.Safe],
				[
					new Filter(
						"RSE_RSL_ShippingLine",
						FilterOps.Equals,
						shippingLinePk as any,
						"guid"
					),
				],
				true,
				this.props.systemVersion,
				{ orderBy: ["RSE_Name"] }
			);

		const defaultProvider = providers?.find((x) => x.RSE_IsDefault);
		return (
			providers?.map(
				(x) =>
					new RefShippingLineEBLProviderWrapper(
						x.RSE_PK,
						x.RSE_RSL_ShippingLine,
						x.RSE_Name,
						x.RSE_IsAvailable,
						x.RSE_IsDefault,
						defaultProvider ? defaultProvider.RSE_PK != x.RSE_PK : undefined //if not default then Default should be hidden
					)
			) ?? []
		);
	}

	async getMessagingRequirementTypesAndRequirements(
		shippingLinePk: string
	): Promise<
		[
			IRefShippingLineMessagingRequirementType[],
			IRefShippingLineMessagingRequirement[]
		]
	> {
		let messagingRequirements: IRefShippingLineMessagingRequirement[] = [];
		let messagingRequirementTypes =
			await this.props.entityManager.getAsync<IRefShippingLineMessagingRequirementType>(
				"RefShippingLineMessagingRequirementType",
				[ServiceType.Safe],
				[],
				false,
				this.props.systemVersion
			);
		if (messagingRequirementTypes) {
			let requirements = await this.loadMessagingRequirements(
				messagingRequirementTypes,
				shippingLinePk
			);
			if (requirements) {
				messagingRequirements = requirements;
				if (
					requirements.find(
						(x) => x.RSR_RST_NKType == "BLP" && x.RSR_IsShippingInstruction
					)
				) {
					this.setState({ eBLProvidersAddButtonDisabled: false });
				}
			}
		}
		return [messagingRequirementTypes, messagingRequirements];
	}

	async loadMessagingRequirements(
		messagingRequirementTypes: IRefShippingLineMessagingRequirementType[],
		shippingLinePk: string
	): Promise<IRefShippingLineMessagingRequirement[] | undefined> {
		if (this.isInitialLoad || !this.state.hasLoadedMessagingRequirements) {
			let filter = new Filter(
				"RSR_RSL_ShippingLine",
				FilterOps.Equals,
				shippingLinePk as any,
				"guid"
			);
			let requirements =
				await this.props.entityManager.getAsync<IRefShippingLineMessagingRequirement>(
					"RefShippingLineMessagingRequirement",
					[ServiceType.Safe],
					[filter],
					true,
					this.props.systemVersion
				);
			return this.createOrLoadDefaultRequirements(
				messagingRequirementTypes,
				requirements,
				shippingLinePk
			);
		} else {
			return undefined;
		}
	}

	createOrLoadDefaultRequirements(
		messagingRequirementTypes: IRefShippingLineMessagingRequirementType[],
		currentRequirements: IRefShippingLineMessagingRequirement[],
		shippingLinePk: string
	): IRefShippingLineMessagingRequirement[] {
		let requirements: IRefShippingLineMessagingRequirement[] = [];
		messagingRequirementTypes.map((x) => {
			let currentRequirement = currentRequirements.find(
				(t) => t.RSR_RST_NKType == x.RST_Code
			) as IRefShippingLineMessagingRequirement;
			if (currentRequirement) {
				requirements.push(currentRequirement);
			} else {
				let newRequirement: IRefShippingLineMessagingRequirement = {
					RSR_IsEManifest: false,
					RSR_IsVerifiedGrossContainerWeight: false,
					RSR_IsBookingRequest: false,
					RSR_IsShippingInstruction: false,
					RSR_IsShippingOrder: false,
					RSR_RSL_ShippingLine: shippingLinePk,
					RSR_RST_NKType: x.RST_Code,
					RSR_PK: uuid(),
				};
				this.props.entityManager.add(
					newRequirement,
					"RefShippingLineMessagingRequirement"
				);
				requirements.push(newRequirement);
			}
		});
		return requirements;
	}

	shouldShowMessagingRequirements(): boolean {
		return (
			this.state.messagingRequirementTypes &&
			this.state.messagingRequirementTypes.length > 0 &&
			this.state.shippingLine.RSL_OceanCarrierMessagingAvailable
		);
	}

	shouldShowIntegrations(): boolean {
		return this.state.shippingLine.RSL_OceanCarrierMessagingAvailable;
	}

	async onMessagingRequirementChange(
		entity: any,
		name: string,
		value: object
	): Promise<void> {
		let requirement = entity as IRefShippingLineMessagingRequirement;
		let index = this.state.messagingRequirements.findIndex(
			(x) => x.RSR_PK == requirement.RSR_PK
		);
		let updatedRequirement = update(requirement, { [name]: { $set: value } });
		this.props.entityManager.update(updatedRequirement);
		this.isInitialLoad = false;
		this.setState({
			messagingRequirements: update(this.state.messagingRequirements, {
				$splice: [[index, 1, updatedRequirement]],
			}),
		});

		if (
			name == "RSR_IsShippingInstruction" &&
			requirement.RSR_RST_NKType == "BLP"
		) {
			this.setState({ eBLProvidersAddButtonDisabled: !value });

			if (!value) {
				this.state.eBLProviders.map((x) =>
					this.handleDeleteEBLProviderItem(x.RSE_PK, false)
				);
				this.setState({ eBLProviders: [] });
			} else {
				let eBLProviders = await this.getEBLProviders(
					this.state.shippingLine.RSL_PK
				);
				this.setState({ eBLProviders: eBLProviders });
			}
		}

		if (value) {
			let propertyName: string = "";
			switch (name) {
				case "RSR_IsBookingRequest": {
					propertyName = "RSL_BookingRequestAvailable";
					break;
				}
				case "RSR_IsShippingOrder": {
					propertyName = "RSL_ShippingOrderAvailable";
					break;
				}
				case "RSR_IsShippingInstruction": {
					propertyName = "RSL_ShippingInstructionAvailable";
					break;
				}
				case "RSR_IsEManifest": {
					propertyName = "RSL_EManifestAvailable";
					break;
				}
				case "RSR_IsVerifiedGrossContainerWeight": {
					propertyName = "RSL_VerifiedGrossContainerWeightAvailable";
					break;
				}
			}
			if (propertyName != "") {
				let shipLine = update(this.state.shippingLine, {
					[propertyName]: { $set: true },
				});
				this.setState({ shippingLine: shipLine });
				this.props.entityManager.update(shipLine);
			}
		}
	}

	get isKeyReadOnly(): boolean {
		return (
			this.props.entityManager.isInDatabase(this.state.shippingLine) &&
			this.state.shippingLine.RSL_IsSystem
		);
	}

	get isMessagingRequirementsHidden(): boolean {
		return !this.state.shippingLine.RSL_OceanCarrierMessagingAvailable;
	}

	async onValueChange(entity: any, name: string, value: object): Promise<void> {
		let shipLine = update(this.state.shippingLine, { [name]: { $set: value } });
		if (name == "RSL_OceanCarrierMessagingAvailable") {
			if (shipLine.RSL_OceanCarrierMessagingAvailable) {
				if (this.isInitialLoad || !this.state.hasLoadedMessagingRequirements) {
					let messagingRequirements = await this.loadMessagingRequirements(
						this.state.messagingRequirementTypes,
						this.state.shippingLine.RSL_PK
					);
					if (messagingRequirements) {
						let hasUpdatedShippingLineBookingRequestAvailable = false;
						let hasUpdatedShippingLineShippingOrderAvailable = false;
						let hasUpdatedShippingLineShippingInstructionAvailable = false;
						let hasUpdatedShippingLineEManifestAvailable = false;
						let hasUpdatedShippingLineVerifiedGrossContainerWeightAvailable =
							false;
						//navigate the messaging requirements, triggering the 'onChange' events. Only when ticked.
						//updates shippingLine only once, hence the controller variables.
						messagingRequirements.map((x) => {
							if (x.RSR_IsBookingRequest) {
								if (!hasUpdatedShippingLineBookingRequestAvailable) {
									shipLine = update(shipLine, {
										RSL_BookingRequestAvailable: { $set: true },
									});
									hasUpdatedShippingLineBookingRequestAvailable = true;
								}
								this.onMessagingRequirementChange(
									x,
									"RSR_IsBookingRequest",
									true as any
								);
							}
							if (x.RSR_IsShippingOrder) {
								if (!hasUpdatedShippingLineShippingOrderAvailable) {
									shipLine = update(shipLine, {
										RSL_ShippingOrderAvailable: { $set: true },
									});
									hasUpdatedShippingLineShippingOrderAvailable = true;
								}
								this.onMessagingRequirementChange(
									x,
									"RSR_IsShippingOrder",
									true as any
								);
							}
							if (x.RSR_IsShippingInstruction) {
								if (!hasUpdatedShippingLineShippingInstructionAvailable) {
									shipLine = update(shipLine, {
										RSL_ShippingInstructionAvailable: { $set: true },
									});
									hasUpdatedShippingLineShippingInstructionAvailable = true;
								}
								this.onMessagingRequirementChange(
									x,
									"RSR_IsShippingInstruction",
									true as any
								);
							}
							if (x.RSR_IsEManifest) {
								if (!hasUpdatedShippingLineEManifestAvailable) {
									shipLine = update(shipLine, {
										RSL_EManifestAvailable: { $set: true },
									});
									hasUpdatedShippingLineEManifestAvailable = true;
								}
								this.onMessagingRequirementChange(
									x,
									"RSR_IsEManifest",
									true as any
								);
							}
							if (x.RSR_IsVerifiedGrossContainerWeight) {
								if (
									!hasUpdatedShippingLineVerifiedGrossContainerWeightAvailable
								) {
									shipLine = update(shipLine, {
										RSL_VerifiedGrossContainerWeightAvailable: { $set: true },
									});
									hasUpdatedShippingLineVerifiedGrossContainerWeightAvailable =
										true;
								}
								this.onMessagingRequirementChange(
									x,
									"RSR_IsVerifiedGrossContainerWeight",
									true as any
								);
							}
						});
						this.setState({
							messagingRequirements: messagingRequirements,
							hasLoadedMessagingRequirements: true,
						});
					}
				}
			} else if (!shipLine.RSL_OceanCarrierMessagingAvailable) {
				shipLine = update(shipLine, {
					RSL_BookingRequestAvailable: { $set: false },
					RSL_ShippingInstructionAvailable: { $set: false },
					RSL_VerifiedGrossContainerWeightAvailable: { $set: false },
					RSL_ShippingOrderAvailable: { $set: false },
					RSL_EManifestAvailable: { $set: false },
				});
				this.state.messagingRequirements.map((x, i) => {
					this.props.entityManager.remove(x);
					this.setState({
						messagingRequirements: update(this.state.messagingRequirements, {
							$splice: [[i, 1]],
						}),
						hasLoadedMessagingRequirements: false,
					});
				});
				this.state.eBLProviders.map((x) =>
					this.handleDeleteEBLProviderItem(x.RSE_PK, false)
				);
				this.setState({
					eBLProviders: [],
					eBLProvidersAddButtonDisabled: true,
				});
			}
		} else if (name == "RSL_ShippingInstructionAvailable") {
			if (!value) {
				this.state.eBLProviders.map((x) =>
					this.handleDeleteEBLProviderItem(x.RSE_PK, false)
				);
				this.setState({
					eBLProviders: [],
					eBLProvidersAddButtonDisabled: true,
				});
			}
		}
		this.isInitialLoad = false;
		this.setState({ shippingLine: shipLine });
		this.props.entityManager.update(shipLine);
		this.updateMessagingRequirements(shipLine, name);
	}

	async updateMessagingRequirements(
		shippingLine: RefShippingLineUserView,
		name: string
	): Promise<void> {
		if (this.shouldShowMessagingRequirements()) {
			let propertyName: string = "";
			let updatedRequirements: IRefShippingLineMessagingRequirement[] = [];
			switch (name) {
				case "RSL_BookingRequestAvailable": {
					if (!shippingLine.RSL_BookingRequestAvailable) {
						propertyName = "RSR_IsBookingRequest";
					}
					break;
				}
				case "RSL_ShippingInstructionAvailable": {
					if (!shippingLine.RSL_ShippingInstructionAvailable) {
						propertyName = "RSR_IsShippingInstruction";
					}
					break;
				}
				case "RSL_ShippingOrderAvailable": {
					if (!shippingLine.RSL_ShippingOrderAvailable) {
						propertyName = "RSR_IsShippingOrder";
					}
					break;
				}
				case "RSL_EManifestAvailable": {
					if (!shippingLine.RSL_EManifestAvailable) {
						propertyName = "RSR_IsEManifest";
					}
					break;
				}
				case "RSL_VerifiedGrossContainerWeightAvailable": {
					if (!shippingLine.RSL_VerifiedGrossContainerWeightAvailable) {
						propertyName = "RSR_IsVerifiedGrossContainerWeight";
					}
					break;
				}
			}
			if (propertyName != "") {
				this.state.messagingRequirements.forEach((m) => {
					let updatedRequirement = update(m, {
						[propertyName]: { $set: false },
					});
					updatedRequirements.push(updatedRequirement);
					this.props.entityManager.update(updatedRequirement);
				});
				this.setState({ messagingRequirements: updatedRequirements });
			}
		}
	}

	async onValueChanged(entity: any, name: string): Promise<void> {
		let shipLine = this.state.shippingLine;
		let shippingLineValidationResults =
			name == "RSL_IsActive"
				? update(this.state.validationResults, {
					["RSL_CargoWiseOneCode"]: {
						$set: await ValidationHelper.validateProperty(
							this.validationService,
							shipLine,
							"RSL_CargoWiseOneCode"
						),
					},
				})
				: update(this.state.validationResults, {
					[name]: {
						$set: await ValidationHelper.validateProperty(
							this.validationService,
							shipLine,
							name
						),
					},
				});
		this.setState({ validationResults: shippingLineValidationResults });
	}

	onEBLProviderItemChange(
		entity: IRefShippingLineEBLProvider,
		name: string,
		value: object
	): void {
		let updatedEBLProviders = [...this.state.eBLProviders];
		let eblProviderItemIndex = this.state.eBLProviders.findIndex(
			(x) => x.RSE_PK == entity.RSE_PK
		);
		let updatedEBLProviderItem = update(
			updatedEBLProviders[eblProviderItemIndex],
			{ [name]: { $set: value } }
		);
		updatedEBLProviders[eblProviderItemIndex] = updatedEBLProviderItem;
		this.setState({ eBLProviders: updatedEBLProviders });
		this.props.entityManager.update(updatedEBLProviderItem.objectToInterface());
	}

	async onEBLProviderItemChanged(
		entity: IRefShippingLineEBLProvider,
		name: string
	): Promise<void> {
		let stateEntity = this.state.eBLProviders.find(
			(x) => x.RSE_PK == entity.RSE_PK
		);
		if (stateEntity) {
			switch (name) {
				case "RSE_Name":
					let eblProviderValidationResult = update(
						this.state.eblProviderValidationResults[stateEntity.RSE_PK] || {},
						{
							[name]: {
								$set: await ValidationHelper.validateProperty(
									this.validationService,
									stateEntity,
									name,
									1
								),
							},
						}
					);
					this.setState({
						eblProviderValidationResults: update(
							this.state.eblProviderValidationResults,
							{
								[entity.RSE_PK]: { $set: eblProviderValidationResult },
							}
						),
					});
					break;
			}
		}
		this.isInitialLoad = false;
	}

	handleAddEBLProviderItem(): void {
		let newEblProviderWrapper = new RefShippingLineEBLProviderWrapper(
			uuid(),
			this.state.shippingLine.RSL_PK,
			"",
			false,
			false
		);
		this.setState({
			eBLProviders: [...this.state.eBLProviders, newEblProviderWrapper],
		});
		let newEblProvider = newEblProviderWrapper.objectToInterface();
		this.props.entityManager.add(newEblProvider, "RefShippingLineEBLProvider");
		this.isInitialLoad = false;
	}

	handleDeleteEBLProviderItem(
		eblProviderItemPk: string,
		shouldSetState: boolean = true
	): void {
		const newItems = [...this.state.eBLProviders];
		if (shouldSetState) {
			this.setState({
				eBLProviders: newItems.filter((x) => x.RSE_PK != eblProviderItemPk),
			});
		}
		this.props.entityManager.remove(
			newItems.find((x) => x.RSE_PK == eblProviderItemPk)!.objectToInterface()
		);
		this.isInitialLoad = false;
	}

	getValidationResults() {
		return Object.getOwnPropertyNames(this.state.eblProviderValidationResults)
			.map((p) => this.state.eblProviderValidationResults[p])
			.concat(this.state.validationResults);
	}

	async save(): Promise<void> {
		if (this.isInitialLoad) {
			this.setState({ saveMessage: "There is nothing to save." });
			if (this.props.onSaved) {
				this.props.onSaved();
			}
		} else {
			if (!this.state.shippingLine.RSL_IsPublished) {
				this.state.shippingLine.RSL_StandardCarrierAlphaCode = "";
			}
			let shippingLineValidationResults = await ValidationHelper.validate(
				this.validationService,
				this.state.shippingLine,
				0,
				true
			);
			let eblProviderValidationResults: {
				[pk: string]: IValidationResults;
			} = {};

			if (this.state.eBLProviders) {
				await Promise.all(
					this.state.eBLProviders.map((x) =>
						ValidationHelper.validate(this.validationService, x, 1, true).then(
							(r) => (eblProviderValidationResults[x.RSE_PK] = r)
						)
					)
				);
			}

			this.setState({
				validationResults: shippingLineValidationResults,
				eblProviderValidationResults: eblProviderValidationResults,
			});

			if (!ValidationHelper.hasErrors(this.getValidationResults())) {
				this.state.messagingRequirements.map((x) => {
					if (
						!(
							x.RSR_IsBookingRequest ||
							x.RSR_IsShippingInstruction ||
							x.RSR_IsShippingOrder ||
							x.RSR_IsEManifest ||
							x.RSR_IsVerifiedGrossContainerWeight
						)
					)
						this.props.entityManager.remove(x);
				});

				let result = await this.props.entityManager.saveChanges(
					ServiceType.Safe
				);
				this.setState({ saveMessage: result.message });
				this.isInitialLoad = true;
				await this.loadShippingLine();
			} else {
				this.setState({ saveMessage: "Please fix all errors before saving." });
			}
		}
	}

	render() {
		return (
			<div>
				<div className="row">
					<div className="col-sm-6">
						<TextInput
							parentProps={this.state}
							inputType="Text"
							label="CW1 Code"
							propertyName="RSL_CargoWiseOneCode"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							onValueChanged={this.onValueChanged}
							validationResults={this.state.validationResults}
						/>
						<TextInput
							parentProps={this.state}
							inputType="Text"
							label="SCAC Code"
							propertyName="RSL_StandardCarrierAlphaCode"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							onValueChanged={this.onValueChanged}
							validationResults={this.state.validationResults}
						/>
						<TextInput
							parentProps={this.state}
							inputType="Text"
							label="Carrier Name"
							propertyName="RSL_CarrierName"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							onValueChanged={this.onValueChanged}
							readOnly={this.isKeyReadOnly}
							validationResults={this.state.validationResults}
						/>
						<TextInput
							parentProps={this.state}
							inputType="Text"
							label="EHub IDs"
							propertyName="RSL_EHubIds"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							onValueChanged={this.onValueChanged}
							validationResults={this.state.validationResults}
						/>
					</div>
					<div className="col-sm-1">
						<CheckBox
							parentProps={this.state}
							label="Is Active"
							inlineLabel={true}
							propertyName="RSL_IsActive"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							onValueChanged={this.onValueChanged}
							validationResults={this.state.validationResults}
						/>
						<CheckBox
							parentProps={this.state}
							label="Is NVO"
							inlineLabel={true}
							propertyName="RSL_IsNVO"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							validationResults={this.state.validationResults}
						/>
						<CheckBox
							parentProps={this.state}
							label="Is ShippingLine"
							inlineLabel={true}
							propertyName="RSL_IsShippingLine"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							validationResults={this.state.validationResults}
						/>
						<CheckBox
							parentProps={this.state}
							label="Is CW1User"
							inlineLabel={true}
							propertyName="RSL_IsCW1User"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							validationResults={this.state.validationResults}
						/>
						<CheckBox
							parentProps={this.state}
							label="Is System"
							inlineLabel={true}
							propertyName="RSL_IsSystem"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							validationResults={this.state.validationResults}
							isReadOnly={true}
						/>
						<CheckBox
							parentProps={this.state}
							label="Is Published"
							inlineLabel={true}
							propertyName="RSL_IsPublished"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							validationResults={this.state.validationResults}
						/>
					</div>
					<div className="col-sm-4">
						<RefShippingLineEBLProviders
							handleAddEBLProviderItem={this.handleAddEBLProviderItem}
							handleDeleteEBLProviderItem={this.handleDeleteEBLProviderItem}
							onValueChange={this.onEBLProviderItemChange}
							onValueChanged={this.onEBLProviderItemChanged}
							providerItems={this.state.eBLProviders}
							eblProviderValidationResults={
								this.state.eblProviderValidationResults
							}
							isAddButtonDisabled={this.state.eBLProvidersAddButtonDisabled}
							eblProviderDistinctNames={this.state.eblProviderDistinctNames}
						/>
					</div>
				</div>
				<div className="form-group row">
					<label className="col-sm-2 col-form-label">
						Available Integrations
					</label>
					<div className="col-sm-2">
						<CheckBox
							parentProps={this.state}
							propertyName="RSL_OceanCarrierMessagingAvailable"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							label="Ocean Carrier Messaging"
							inlineLabel={true}
							validationResults={this.state.validationResults}
						/>
						{this.shouldShowIntegrations() ? (
							<div className="col-sm-10">
								<CheckBox
									parentProps={this.state}
									propertyName="RSL_BookingRequestAvailable"
									entity={this.state.shippingLine}
									onValueChange={this.onValueChange}
									label="Booking Request"
									inlineLabel={true}
									validationResults={this.state.validationResults}
								/>
								<CheckBox
									parentProps={this.state}
									propertyName="RSL_ShippingInstructionAvailable"
									entity={this.state.shippingLine}
									onValueChange={this.onValueChange}
									label="Shipping Instruction"
									inlineLabel={true}
									validationResults={this.state.validationResults}
								/>
								<CheckBox
									parentProps={this.state}
									propertyName="RSL_VerifiedGrossContainerWeightAvailable"
									entity={this.state.shippingLine}
									onValueChange={this.onValueChange}
									label="Verified Gross Container Weight (VGM)"
									inlineLabel={true}
									validationResults={this.state.validationResults}
								/>
								<CheckBox
									parentProps={this.state}
									propertyName="RSL_ShippingOrderAvailable"
									entity={this.state.shippingLine}
									onValueChange={this.onValueChange}
									label="Shipping Order (China)"
									inlineLabel={true}
									validationResults={this.state.validationResults}
								/>
								<CheckBox
									parentProps={this.state}
									propertyName="RSL_EManifestAvailable"
									entity={this.state.shippingLine}
									onValueChange={this.onValueChange}
									label="eManifest (China)"
									inlineLabel={true}
									validationResults={this.state.validationResults}
								/>
							</div>
						) : (
							""
						)}
						<CheckBox
							parentProps={this.state}
							propertyName="RSL_GlobalSailingScheduleAvailable"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							label="Global Sailing Schedule"
							inlineLabel={true}
							validationResults={this.state.validationResults}
						/>
						<CheckBox
							parentProps={this.state}
							propertyName="RSL_ContainerAutomationAvailable"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							label="Container Automation"
							inlineLabel={true}
							validationResults={this.state.validationResults}
						/>
						<CheckBox
							parentProps={this.state}
							propertyName="RSL_CargoSphereRatesAvailable"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							label="Cargo Sphere Rates"
							inlineLabel={true}
							validationResults={this.state.validationResults}
						/>
						<CheckBox
							parentProps={this.state}
							propertyName="RSL_InvoiceAvailable"
							entity={this.state.shippingLine}
							onValueChange={this.onValueChange}
							label="Invoice"
							inlineLabel={true}
							validationResults={this.state.validationResults}
						/>
					</div>
					<div className="col-sm-8">
						{this.shouldShowMessagingRequirements() ? (
							<RefShippingLineMessagingRequirementTypeForm
								requirementTypes={this.state.messagingRequirementTypes}
								requirements={this.state.messagingRequirements}
								onMessagingRequirementChange={this.onMessagingRequirementChange}
							/>
						) : (
							""
						)}
					</div>
				</div>
				<div className="form-group-row">
					<div className="col-sm-1">
						<Button
							type="button"
							className="btn btn-info"
							onClick={() => this.save()}
							disabled={this.state.readOnly || this.state.saveButtonDisabled}
						>
							Save
						</Button>
					</div>
				</div>
				{this.state.saveMessage.length > 0 ? (
					<PopupForm
						title="Information"
						message={this.state.saveMessage}
						handleHideModal={() => this.setState({ saveMessage: "" })}
						validationResults={this.getValidationResults()}
					/>
				) : null}
			</div>
		);
	}
}
