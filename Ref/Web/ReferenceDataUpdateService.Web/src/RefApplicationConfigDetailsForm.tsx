import React from "react";
import uuid from "uuid";
import { IEntityManager, ServiceType } from "./EntityManager";
import _ from "underscore";
import { Filter, FilterOps } from "./Filter";
import update from "immutability-helper";
import { TextInput } from "./TextInput";
import { ValidationServiceWrapper } from "./ValidationService";
import { IValidationResults } from "./ValidationResults";
import { ValidationHelper } from "./ValidationHelper";
import { ValidationServiceHelper } from "./ValidationServiceHelper";
import { PopupForm } from "./PopupForm";
import { IParentProps } from "./IParentProps";
import IQrtzJobDetails from "./models/IQrtzJobDetails";
import { IRefApplicationAttribute, IRefApplicationAttributeDefault, RefApplicationAttributeWrapper } from "./models/IRefApplicationAttribute";
import IRefApplicationAttributeType from "./models/IRefApplicationAttributeType"
import { RefApplicationAttributeDetailsForm } from "./RefApplicationAttributeDetailsForm";
import RefApplicationAttributeTypeEnum from "./models/RefApplicationAttributeTypeEnum";
import HttpStatus from "http-status-codes";

export interface IRefApplicationConfigDetailsFormProps {
	id?: string,
	entityManager: IEntityManager,
	onSaved?: () => Promise<void>,
}

interface IRefApplicationConfigDetailsFormState extends IParentProps {
	qrtzJob: IQrtzJobDetails,
	attributes: RefApplicationAttributeWrapper[],
	attributeTypes: IRefApplicationAttributeType[],
	attrValidationResults: {
		[pk: string]: IValidationResults
	},
	saveMessage: string,
	saveButtonDisabled: boolean
}

export class RefApplicationConfigDetailsForm extends React.Component<IRefApplicationConfigDetailsFormProps, IRefApplicationConfigDetailsFormState> {
	constructor(props: IRefApplicationConfigDetailsFormProps) {
		super(props);
		this.loadQrtzJobAndAttributes = this.loadQrtzJobAndAttributes.bind(this);
		this.onAttributeValueChange = this.onAttributeValueChange.bind(this);
		this.onAttributeValueChanged = this.onAttributeValueChanged.bind(this);
		this.onValueChange = this.onValueChange.bind(this);
		this.onValueChanged = this.onValueChanged.bind(this);
		this.updateSaveButtonDisabledProperty = this.updateSaveButtonDisabledProperty.bind(this);
		this.save = this.save.bind(this);
		this.getProgramExePath = this.getProgramExePath.bind(this);
		this.handleHideModal = this.handleHideModal.bind(this);

		this.state = {
			qrtzJob: {
				JOB_PK: uuid.v1(),
				SCHED_NAME: "",
				JOB_NAME: "",
				JOB_GROUP: "",
				DESCRIPTION: "",
				JOB_CLASS_NAME: "",
				CountryCode: "",
				ProgramArgs: "",
				ProgramExePath: ""
			},
			attributes: [],
			attributeTypes: [],
			attrValidationResults: {},
			saveMessage: "",
			saveButtonDisabled: false
		};
		this.validationService = new ValidationServiceWrapper([ValidationServiceHelper.getRefApplicationAttributeValidationService(this.props.entityManager)], this.updateSaveButtonDisabledProperty);
	}

	validationService: ValidationServiceWrapper;
	hasLoadedAttributeType: boolean = false;

	readonly CREDENTIAL_DEFAULT_VALUE: string = "********";
	readonly UNAUTHORIZED_MESSAGE: string = "Unauthorized";

	updateSaveButtonDisabledProperty(isValidating: boolean) {
		this.setState({ saveButtonDisabled: isValidating });
	}

	async componentDidMount() {
		await this.loadQrtzJobAndAttributes();
	}

	// This is a temporary solution util we convert the class to functional component
	async componentWillUnmount() {
		await Promise.all([
			this.props.entityManager.reload("QRTZ_JOB_DETAILS", [ServiceType.Staging], [new Filter("JOB_PK", FilterOps.Equals, this.props.id as any, "guid")]),
			this.props.entityManager.reload("RefApplicationAttribute", [ServiceType.Staging], [new Filter("RAA_ConfigFilePath", FilterOps.Equals, this.configFile as any, "string")]),
		]);
	}

	handleHideModal = () => {
		let message = this.state.saveMessage;
		this.setState({ saveMessage: "" });
		if (message === this.UNAUTHORIZED_MESSAGE) {
			window.history.back();
		}
	}

	async loadQrtzJobAndAttributes() {
		if (this.props.id) {
			if (!this.hasLoadedAttributeType) {
				let typeArray = await this.props.entityManager.getAsync<IRefApplicationAttributeType>("RefApplicationAttributeType", [ServiceType.Staging], [new Filter("RAT_Type", FilterOps.NotEquals, "" as any, "string")], false);
				this.setState({ attributeTypes: typeArray });
				this.hasLoadedAttributeType = true;
			}
			let jobArray = await Promise.all([
				this.props.entityManager.getAsync<IQrtzJobDetails>("QRTZ_JOB_DETAILS", [ServiceType.Staging], [new Filter("JOB_PK", FilterOps.Equals, this.props.id as any, "guid")], false)
			]);
			let job = _.first(jobArray[0]);
			if (job) {
				if (job.CountryCode == null) {
					job.CountryCode = "";
				}
				if (job.ProgramArgs == null) {
					job.ProgramArgs = "";
				}
				let programExePath = this.getProgramExePath(job.ProgramExePath);
				if (job.ProgramExePath.includes("net8.0\\")) {
					this.configFile = programExePath.replace(".exe", "") + ".config.json";
				} else {
					this.configFile = programExePath + ".config";
				}

				let attributeArray: RefApplicationAttributeWrapper[] = [];
				let dbAttributes: IRefApplicationAttribute[] = [];
				let serverDefaults : IRefApplicationAttributeDefault[] = [];

				try {
					dbAttributes = await this.props.entityManager.getAsync<IRefApplicationAttribute>("RefApplicationAttribute", [ServiceType.Staging], [new Filter("RAA_ConfigFilePath", FilterOps.Equals, this.configFile as any, "string")], false);
					serverDefaults = await this.props.entityManager.getAsync<IRefApplicationAttributeDefault>("RefApplicationAttributeDefault", [ServiceType.Staging], [new Filter("RAA_ConfigFilePath", FilterOps.Equals, this.configFile as any, "string")], false);
				} catch (error: any) {
					if (error.statusCode === HttpStatus.UNAUTHORIZED || error.statusCode === HttpStatus.FORBIDDEN) {
						this.setState({ saveMessage: this.UNAUTHORIZED_MESSAGE });
					} else {
						throw error;
					}
				}

				if (serverDefaults) {
					await Promise.all(serverDefaults.map(x => {
						let dbRecord = dbAttributes.find(a => a.RAA_AttributeName == x.RAA_AttributeName);
						if (!dbRecord) {
							attributeArray.push(new RefApplicationAttributeWrapper(uuid.v1(), x.RAA_ConfigFilePath, x.RAA_AttributeName, x.RAA_Value, x.RAA_RAT_NKType, job.JOB_GROUP, x.RAA_Content, true, x));
						} else {
							if (dbRecord.RAA_RAT_NKType == RefApplicationAttributeTypeEnum.Credential) {
								dbRecord.RAA_Value = this.CREDENTIAL_DEFAULT_VALUE;
							}
							attributeArray.push(new RefApplicationAttributeWrapper(dbRecord.RAA_PK, dbRecord.RAA_ConfigFilePath, dbRecord.RAA_AttributeName, dbRecord.RAA_Value, dbRecord.RAA_RAT_NKType, job.JOB_GROUP, dbRecord.RAA_Content, false, x));
						}
					}));
				}

				this.setState({ qrtzJob: job, attributes: attributeArray });
			}
		}
	}

	configFile: string;

	getProgramExePath(path: string): string {
		if (path != undefined && path != null) {
			path = path.substring(path.lastIndexOf("\\") + 1);
		}
		return path;
	}

	async onAttributeValueChange(entity: any, name: string, value: object): Promise<void> {
		let attr = entity as RefApplicationAttributeWrapper;
		if (attr) {
			let index = this.state.attributes.findIndex(x => x.RAA_PK == attr.RAA_PK);
			if (index >= 0) {
				let updatedAttr = update(this.state.attributes[index], { [name]: { $set: value } });
				if (name == "RAA_RAT_NKType") {
					updatedAttr.RAA_Value = updatedAttr.RAA_RAT_NKType == RefApplicationAttributeTypeEnum.Boolean ? "false" : updatedAttr.RAA_Value;
					updatedAttr.RAA_Content = null;
				}

				if (name == "RAA_IsDefault" && updatedAttr.RAA_IsDefault) {
					updatedAttr.RAA_Value = updatedAttr.defaultRecord!.RAA_Value.toString();
					updatedAttr.RAA_RAT_NKType = updatedAttr.defaultRecord!.RAA_RAT_NKType;
					this.props.entityManager.remove(updatedAttr.objectToInterface());
				}
				else if (name == "RAA_IsDefault" && !updatedAttr.RAA_IsDefault) {
					let newAttr = updatedAttr.objectToInterface();
					this.props.entityManager.add(newAttr, "RefApplicationAttribute");
				}
				else {
					let newAttr = updatedAttr.objectToInterface();
					this.props.entityManager.update(newAttr);
				}
				this.setState({ attributes: update(this.state.attributes, { $splice: [[index, 1, updatedAttr]] }) });
			}
		}
	}

	async onAttributeValueChanged(entity: any, name: string): Promise<void> {
		let attr = entity as RefApplicationAttributeWrapper;
		if (attr) {
			let index = this.state.attributes.findIndex(x => x.RAA_PK == attr.RAA_PK);
			if (index >= 0) {
				let validationResults = await ValidationHelper.validateProperty(this.validationService, attr, name, 0);
				let attrValidationResult = update(this.state.attrValidationResults[attr.RAA_PK] || {}, { [name]: { $set: validationResults } });
				let attrValidationResults = update(this.state.attrValidationResults, { [attr.RAA_PK]: { $set: attrValidationResult } });
				this.setState({ attrValidationResults: attrValidationResults });
			}
		}
	}

	async onValueChange(entity: any, name: string, value: object): Promise<void> {
		let job = update(this.state.qrtzJob, { [name]: { $set: value } });
		this.setState({ qrtzJob: job });
		this.props.entityManager.update(job);
	}

	async onValueChanged(entity: any, name: string): Promise<void> {
	}

	async save(): Promise<void> {
		let attributeValidationResults: {
			[pk: string]: IValidationResults
		} = {};
		await Promise.all(this.state.attributes.map(a => ValidationHelper.validate(this.validationService, a, 0, true).then(r => attributeValidationResults[a.RAA_PK] = r)));
		this.setState({ attrValidationResults: attributeValidationResults });

		if (!ValidationHelper.hasErrors(Object.getOwnPropertyNames(attributeValidationResults).map(p => attributeValidationResults[p]))) {
			let result = await this.props.entityManager.saveChanges(ServiceType.Staging);
			this.setState(
				{
					saveMessage: result.message,
					attributes: this.state.attributes.map(attr => {
						if (attr.RAA_RAT_NKType === RefApplicationAttributeTypeEnum.Credential) {
							attr.RAA_Value = this.CREDENTIAL_DEFAULT_VALUE;
						}
						return attr;
					})
				}
			);
			if (this.props.onSaved) {
				this.props.onSaved();
			}
		}
		else {
			this.setState({ saveMessage: "Please fix all errors before saving." });
		}
	}

	render() {
		return <div>
			<div className="row">
				<div className="col-sm-9">
					<TextInput parentProps={this.state} inputType="Text" label="Job Name" propertyName="JOB_NAME" entity={this.state.qrtzJob} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} readOnly={true} maxLength={150} />
					<TextInput parentProps={this.state} inputType="Text" label="Program Path" propertyName="ProgramExePath" entity={this.state.qrtzJob} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} readOnly={true} maxLength={200} />
				</div>
			</div>
			<div className="row">
				<div className="col-sm-9">
					<TextInput parentProps={this.state} inputType="Text" label="Args" propertyName="ProgramArgs" entity={this.state.qrtzJob} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} readOnly={true} maxLength={100} />
				</div>
				<div className="col-sm-3">
					<TextInput parentProps={this.state} inputType="Text" label="Country" propertyName="CountryCode" entity={this.state.qrtzJob} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} readOnly={true} maxLength={3} />
				</div>
			</div>
			<div className="row">
				<div className="col">
					<RefApplicationAttributeDetailsForm attributes={this.state.attributes} attributeTypes={this.state.attributeTypes} onAttributeValueChange={this.onAttributeValueChange} onAttributeValueChanged={this.onAttributeValueChanged}
						save={this.save} validationResults={this.state.attrValidationResults} entityManager={this.props.entityManager} />
				</div>
			</div>
			{this.state.saveMessage.length > 0 ? <PopupForm title="Information" message={this.state.saveMessage} handleHideModal={this.handleHideModal} validationResults={Object.getOwnPropertyNames(this.state.attrValidationResults).map(p => this.state.attrValidationResults[p])} /> : null}
		</div >
	}
}
