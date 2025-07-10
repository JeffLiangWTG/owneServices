import React from "react";
import uuid from "uuid";
import moment from "moment";
import { IEntityManager, ServiceType } from "./EntityManager";
import _ from "underscore";
import update from "immutability-helper";
import { TextInput } from "./TextInput";
import { CheckBox } from "./CheckBox";
import { DateTimeInput } from "./DateTimeInput";
import { ValidationServiceWrapper } from "./ValidationService";
import { IValidationResults, ValidationResults } from "./ValidationResults";
import { ValidationHelper } from "./ValidationHelper";
import { ValidationServiceHelper } from "./ValidationServiceHelper";
import { PopupForm } from "./PopupForm";
import { IParentProps } from "./IParentProps";
import { RefCusCodeListUserView } from "./models/RefCusCodeListUserView";
import { RefCusCodeListAttributeUserView } from "./models/RefCusCodeListAttributeUserView";
import MsalWrapper from "./MsalWrapper";
import Button from "./Button";

const DeltaT = "DELTAT";
const DeltaX = "DELTAX";
const DeltaG = "DELTAG";
const Gamma = "GAMMA";
const Ics = "ICS";
const Ecs = "ECS";

const Invoke = "Invoke";
const CommentInvoke = "CommentInvoke";
const CommentRevoke = "CommentRevoke";
const EndDate = "2079-06-06T23:59:00Z";

export interface IApplicationList {
	IsSelectAll: boolean,
	IsDeltaT: boolean,
	IsDeltaG: boolean,
	IsDeltaX: boolean,
	IsGamma: boolean,
	IsIcs: boolean,
	IsEcs: boolean,
	DateAndTime: string,
	Comment: string
}

export interface IRefCusCodeListFRFallbackFormProps {
	id? : string,
	entityManager: IEntityManager
}

interface IRefCusCodeListFRFallbackFormState extends IParentProps {
	applicationList: IApplicationList,
	invokeValidationResults: IValidationResults,
	message: string,
	saveButtonDisabled: boolean
}

export class RefCusCodeListFRFallbackInvokeForm extends React.Component<IRefCusCodeListFRFallbackFormProps, IRefCusCodeListFRFallbackFormState> {
	constructor(props: IRefCusCodeListFRFallbackFormProps) {
		super(props);
		this.getCurrentDate = this.getCurrentDate.bind(this);
		this.addCode = this.addCode.bind(this);
		this.addAttribute = this.addAttribute.bind(this);
		this.addCodeAndAttributes = this.addCodeAndAttributes.bind(this);
		this.UpdateApplications = this.UpdateApplications.bind(this);
		this.onValueChange = this.onValueChange.bind(this);
		this.onValueChanged = this.onValueChanged.bind(this);
		this.save = this.save.bind(this);
		this.reset = this.reset.bind(this);
		this.updateSaveButtonDisabledProperty = this.updateSaveButtonDisabledProperty.bind(this);

		this.state = {
			applicationList: {
				IsSelectAll: false,
				IsDeltaT: false,
				IsDeltaG: false,
				IsDeltaX: false,
				IsGamma: false,
				IsIcs: false,
				IsEcs: false,
				DateAndTime: this.getCurrentDate(),
				Comment: ""
			},
			invokeValidationResults: {},
			message: "",
			readOnly: false,
			saveButtonDisabled: false
		};
		this.props.entityManager.clear();
		this.codeValidationService = new ValidationServiceWrapper([ ValidationServiceHelper.getRefCusCodeFallbackInvokeValidationService(this.props.entityManager) ], this.updateSaveButtonDisabledProperty);
	}

	codeValidationService: ValidationServiceWrapper;

	updateSaveButtonDisabledProperty(isValidating: boolean)
	{
		this.setState({ saveButtonDisabled: isValidating});
	}

	getCurrentDate(): string {
		let time = moment(new Date()).format("YYYY-MM-DDTHH:mm:ss") + "Z";
		return new Date(time).toISOString();
	}

	static getApplicationListName(): string[] {
		return [DeltaT, DeltaG, DeltaX, Gamma, Ics, Ecs];
	}

	addCode(name: string, invokeDate: string): string {
		let pk: string = uuid.v1();
		let codeOfZZDValue = new Date().getTime().toString();
		let code: RefCusCodeListUserView = {
			ZZD_PK: pk,
			ZZD_Code: codeOfZZDValue + ',' + name,
			ZZD_CodeType: "FBK",
			ZZD_CountryOrGrouping: "FR",
			ZZD_Description: Invoke + "=" + MsalWrapper.getInstance().getUniqueName(),
			ZZD_StartDate: invokeDate,
			ZZD_EndDate: EndDate,
			ZZD_IsAir: false,
			ZZD_IsSea: false,
			ZZD_IsFix: false,
			ZZD_IsInw: false,
			ZZD_IsRai: false,
			ZZD_IsMai: false,
			ZZD_IsRoa: false,
			ZZD_IsSystem: true,
			ZZD_IsPublished: true,
			ZZD_IsEditable: true
		};
		this.props.entityManager.add(code, "RefCusCodeListUserView");
		return pk;
	}

	addAttribute(pk: string, name: string, value: string): void {
		let attr: RefCusCodeListAttributeUserView = {
			ZZE_PK: uuid.v1(),
			ZZE_ZZD_CodeList: pk,
			ZZE_ZXE_NKName: name,
			ZZE_Value: value,
			ZZE_CodeType: "FBK",
			ZZE_CountryOrGrouping: "FR",
			ZZE_IsAir: false,
			ZZE_IsSea: false,
			ZZE_IsFix: false,
			ZZE_IsRai: false,
			ZZE_IsMai: false,
			ZZE_IsInw: false,
			ZZE_IsRoa: false,
			ZZE_IsEditable: true,
			ZZE_StartDate: null,
			ZZE_EndDate: null
		};
		this.props.entityManager.add(attr, "RefCusCodeListAttributeUserView");
	}

	addCodeAndAttributes(name: string) {
		let pk = this.addCode(name, this.state.applicationList.DateAndTime);
		this.addAttribute(pk, CommentInvoke, this.state.applicationList.Comment);
		this.addAttribute(pk, CommentRevoke, "");
	}

	UpdateApplications(): void {
		this.props.entityManager.clear();

		let isDeltaT = this.state.applicationList.IsDeltaT;
		let isDeltaG = this.state.applicationList.IsDeltaG;
		let isDeltaX = this.state.applicationList.IsDeltaX;
		let isGamma = this.state.applicationList.IsGamma;
		let isIcs = this.state.applicationList.IsIcs;
		let isEcs = this.state.applicationList.IsEcs;

		if (isDeltaT) {
			this.addCodeAndAttributes(DeltaT);
		}
		if (isDeltaG) {
			this.addCodeAndAttributes(DeltaG);
		}
		if (isDeltaX) {
			this.addCodeAndAttributes(DeltaX);
		}
		if (isGamma) {
			this.addCodeAndAttributes(Gamma);
		}
		if (isIcs) {
			this.addCodeAndAttributes(Ics);
		}
		if (isEcs) {
			this.addCodeAndAttributes(Ecs);
		}
	}

	async onValueChange(entity: any, name: string, value: object): Promise<void> {
		let application = update(this.state.applicationList, { [name]: { $set: value} });
		if (name == "IsSelectAll") {
			let selectValue: boolean = application.IsSelectAll;
			application = update(this.state.applicationList
				, {
					["IsSelectAll"]: { $set: selectValue }
					, ["IsDeltaT"]: { $set: selectValue }
					, ["IsDeltaG"]: { $set: selectValue }
					, ["IsDeltaX"]: { $set: selectValue }
					, ["IsGamma"]: { $set: selectValue }
					, ["IsIcs"]: { $set: selectValue }
					, ["IsEcs"]: { $set: selectValue }
				});
		}
		this.setState({
			applicationList: application,
		});
	}

	async onValueChanged(entity: any, name: string): Promise<void> {
		let application = this.state.applicationList
		let validaitionResults = update(this.state.invokeValidationResults, { [name]: { $set: await ValidationHelper.validateProperty(this.codeValidationService, application, name) } });
		this.setState({
			invokeValidationResults: validaitionResults
		});
	}

	reset(): void {
		let applications: IApplicationList = {
				IsSelectAll: false,
				IsDeltaT: false,
				IsDeltaG: false,
				IsDeltaX: false,
				IsGamma: false,
				IsIcs: false,
				IsEcs: false,
				DateAndTime: this.getCurrentDate(),
				Comment: ""
		};
		this.setState({
			applicationList: applications,
			invokeValidationResults: {},
			message: ""
		});
	}

	async save(): Promise<void> {
		let validationResult = await ValidationHelper.validate(this.codeValidationService, this.state.applicationList, 0, true);
		this.setState({
			invokeValidationResults: validationResult,
		});

		if (!ValidationHelper.hasErrors([validationResult])) {
			this.UpdateApplications();
			let result = await this.props.entityManager.saveChanges(ServiceType.Safe);
			this.setState({ message: result.message });
			if (result.success) {
				this.reset();
			}
			this.props.entityManager.clear();
		}
		else {
			this.setState({ message: "Please fix all errors before saving." });
		};
	}

	render() {
		return <div>
			<div className="row">
				<div className="col-sm-7">
					<div className="form-group row">
						<label className="col-sm-4 col-form-label">Applications affected</label>
						<div className="col-sm-2"  >
							<CheckBox parentProps={this.state} propertyName="IsSelectAll" entity={this.state.applicationList} onValueChange={this.onValueChange} label="All" inlineLabel={true} onValueChanged={this.onValueChanged} />
							<CheckBox parentProps={this.state} propertyName="IsDeltaT" entity={this.state.applicationList} onValueChange={this.onValueChange} label="Delta T" inlineLabel={true} validationResults={this.state.invokeValidationResults} onValueChanged={this.onValueChanged} />
							<CheckBox parentProps={this.state} propertyName="IsDeltaG" entity={this.state.applicationList} onValueChange={this.onValueChange} label="Delta G" inlineLabel={true} validationResults={this.state.invokeValidationResults} onValueChanged={this.onValueChanged} />
							<CheckBox parentProps={this.state} propertyName="IsDeltaX" entity={this.state.applicationList} onValueChange={this.onValueChange} label="Delta X" inlineLabel={true} validationResults={this.state.invokeValidationResults} onValueChanged={this.onValueChanged} />
							<CheckBox parentProps={this.state} propertyName="IsGamma" entity={this.state.applicationList} onValueChange={this.onValueChange} label="Gamma" inlineLabel={true} validationResults={this.state.invokeValidationResults} onValueChanged={this.onValueChanged} />
							<CheckBox parentProps={this.state} propertyName="IsIcs" entity={this.state.applicationList} onValueChange={this.onValueChange} label="ICS" inlineLabel={true} validationResults={this.state.invokeValidationResults} onValueChanged={this.onValueChanged} />
							<CheckBox parentProps={this.state} propertyName="IsEcs" entity={this.state.applicationList} onValueChange={this.onValueChange} label="ECS" inlineLabel={true} validationResults={this.state.invokeValidationResults} onValueChanged={this.onValueChanged} />
						</div>
					</div>
					<div className="form-group row">
						<label className="col-sm-4 col-form-label">Date and Time</label>
						<div className="col-sm-4">
							<DateTimeInput parentProps={this.state} entity={this.state.applicationList} propertyName="DateAndTime" onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} validationResult={this.state.invokeValidationResults} format="DD/MM/YYYY HH:mm:SS" />
						</div>
					</div>
					<div className="form-group row" >
						<label className="col-sm-4 col-form-label">Comment</label>
						<div className="col-sm-4">
							<TextInput parentProps={this.state} inputType="Text" entity={this.state.applicationList} propertyName="Comment" onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} validationResults={this.state.invokeValidationResults} maxLength={255} />
						</div>
					</div>
					<div className="form-group row">
						<div className="col-sm-1">
							<Button type='button' className="btn btn-info" onClick={this.save} disabled={this.state.readOnly || this.state.saveButtonDisabled}>Submit</Button>
						</div>
					</div>
				</div>
			</div>
			{this.state.message.length > 0 ? <PopupForm title="Information" message={this.state.message} handleHideModal={() => this.setState({ message: "" })} validationResults={[this.state.invokeValidationResults]} /> : null}
		</div>
	}
}
