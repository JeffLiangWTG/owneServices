import { Component } from "react";
import _ from "underscore";
import uuid from "uuid";
import { IEntityManager, ServiceType } from "./EntityManager";
import { Filter, FilterOps } from "./Filter";
import update from "immutability-helper";
import { IParentProps } from "./IParentProps";
import { RefCusProcedureAttributeUserView } from "./models/RefCusProcedureAttributeUserView";
import { RefCusProcedureUserView } from "./models/RefCusProcedureUserView";
import { IValidationResults } from "./ValidationResults";
import { ValidationServiceWrapper } from "./ValidationService";
import { ValidationServiceHelper } from "./ValidationServiceHelper";
import { ValidationHelper } from "./ValidationHelper";
import React from "react";
import { PopupForm } from "./PopupForm";
import { TextInput } from "./TextInput";
import { TextArea } from "./TextArea";
import { CodeInput } from "./CodeInput";
import { DateTimeInput } from "./DateTimeInput";
import { CheckBox } from "./CheckBox";
import { RefCusProcedureAttributeDetailsForm } from "./RefCusProcedureAttributeDetailsForm";
import { TextSelect } from "./TextSelect";
import Button from "./Button";

interface IRefCusProcedureUserViewDetailsFormProps {
    id?: string,
    systemVersion?: string,
    entityManager: IEntityManager,
    onSaved?: () => Promise<void>
}

interface IRefCusProcedureUserViewDetailsFormState extends IParentProps {
    procedure: RefCusProcedureUserView,
    attributes: RefCusProcedureAttributeUserView[],
    procedureValidationResults: IValidationResults,
    attrValidationResults: {
        [pk: string]: IValidationResults
    },
    saveMessage: string,
    saveButtonDisabled: boolean
}

export class RefCusProcedureUserViewDetailsForm extends Component<IRefCusProcedureUserViewDetailsFormProps, IRefCusProcedureUserViewDetailsFormState> {
    constructor(props: IRefCusProcedureUserViewDetailsFormProps) {
        super(props);
        this.onAttributeValueChange = this.onAttributeValueChange.bind(this);
        this.onAttributeValueChanged = this.onAttributeValueChanged.bind(this);
        this.onValueChange = this.onValueChange.bind(this);
        this.onValueChanged = this.onValueChanged.bind(this);
        this.addNewAttribute = this.addNewAttribute.bind(this);
        this.removeAttribute = this.removeAttribute.bind(this);
        this.save = this.save.bind(this);
		this.updateSaveButtonDisabledProperty = this.updateSaveButtonDisabledProperty.bind(this);
        let procedureView = new RefCusProcedureUserView();
        procedureView.ZZ6_PK = uuid.v1();
        procedureView.ZZ6_Category = "";
        procedureView.ZZ6_ProcedureCode = "";
        procedureView.ZZ6_PreviousProcedureCode = "";
        procedureView.ZZ6_Concession = "";
        procedureView.ZZ6_Description = "";
        procedureView.ZZ6_CountryOrGrouping = "";
        procedureView.ZZ6_ShipmentType = "";
        procedureView.ZZ6_CalculateDuty = true;
        procedureView.ZZ6_Group = "";
        procedureView.ZZ6_LandedCost = false;
        procedureView.ZZ6_IntoWarehouse = "N";
        procedureView.ZZ6_OutOfWarehouse = "N";
        procedureView.ZZ6_IntoInwardProcessing = "N";
        procedureView.ZZ6_OutOfInwardProcessing = "N";
        procedureView.ZZ6_IntoOutwardProcessing = "N";
        procedureView.ZZ6_OutofOutwardProcessing = "N";
        procedureView.ZZ6_IntoTemporaryImport = "N";
        procedureView.ZZ6_OutOfTemporaryImport = "N";
        procedureView.ZZ6_IntoTemporaryExport = "N";
        procedureView.ZZ6_OutOfTemporaryExport = "N";
        procedureView.ZZ6_StartDate = new Date().toISOString();
        procedureView.ZZ6_EndDate = "2079-06-06T23:59:00Z";
        procedureView.ZZ6_CalculateVAT = true;
        procedureView.ZZ6_IsGuaranteeConsumed = "N";
        procedureView.ZZ6_IsGuaranteeReleased = "N";
        procedureView.ZZ6_IsTransit = "N";
        procedureView.ZZ6_IsPublished = true;
        procedureView.ZZ6_IsEditable = true;

        this.state = {
            procedure: procedureView,
            attributes: [],
            procedureValidationResults: {},
            attrValidationResults: {},
            saveMessage: "",
            readOnly: false,
            saveButtonDisabled: false
        };
        this.validationServices = new ValidationServiceWrapper([ValidationServiceHelper.getRefCusProcedureValidationService(this.props.entityManager), ValidationServiceHelper.getRefCusProcedureAttributeValidationService(this.props.entityManager)], this.updateSaveButtonDisabledProperty);
    }

    validationServices: ValidationServiceWrapper;

    updateSaveButtonDisabledProperty(isValidating: boolean)
	{
		this.setState({ saveButtonDisabled: isValidating});
	}

    async componentDidMount() {
        await this.loadProcedure();
    }

    async componentDidUpdate(prevProps: IRefCusProcedureUserViewDetailsFormProps) {
        if (prevProps.systemVersion !== this.props.systemVersion) {
            this.props.entityManager.clear();
            await this.loadProcedure();
        }
    }

	// This is a temporary solution util we convert the class to functional component
	async componentWillUnmount() {
		await Promise.all([
			this.props.entityManager.reload("RefCusProcedureUserView", [ServiceType.Safe], [new Filter("ZZ6_PK", FilterOps.Equals, this.props.id as any, "guid")]),
			this.props.entityManager.reload("RefCusProcedureAttributeUserView", [ServiceType.Safe], [new Filter("ZXB_ZZ6_ProcedureCode", FilterOps.Equals, this.props.id as any, "guid")]),
		]);
	}

    async loadProcedure() {
        if (this.props.id) {
            this.setState({readOnly: this.props.systemVersion !== undefined});
            var dataArray = await Promise.all([
                this.props.entityManager.getAsync<RefCusProcedureUserView>("RefCusProcedureUserView", [ServiceType.Safe], [new Filter("ZZ6_PK", FilterOps.Equals, this.props.id as any, "guid")], false, this.props.systemVersion),
                this.props.entityManager.getAsync<RefCusProcedureAttributeUserView>("RefCusProcedureAttributeUserView", [ServiceType.Safe], [new Filter("ZXB_ZZ6_ProcedureCode", FilterOps.Equals, this.props.id as any, "guid")], false, this.props.systemVersion)
            ]);
            var procedureData = _.first(dataArray[0]);
            if (procedureData) {
                this.setState({procedure: procedureData, attributes: dataArray[1]});
            }
        }
        else {
            this.props.entityManager.add(this.state.procedure, "RefCusProcedureUserView");
        }
    }

    removeAttribute(id: string) {
        let index = this.state.attributes.findIndex(x => x.ZXB_PK == id);
        if (index >= 0) {
            this.props.entityManager.remove(this.state.attributes[index]);
            this.setState({ attributes : update(this.state.attributes, {$splice: [[index, 1]]})});
        }
    }

    addNewAttribute() {
        let attr = new RefCusProcedureAttributeUserView();
        attr.ZXB_PK = uuid.v1();
        attr.ZXB_ZZ6_ProcedureCode = this.state.procedure.ZZ6_PK;
        attr.ZXB_Name = "";
        attr.ZXB_Value = "";
        attr.ZXB_CountryOrGrouping = this.state.procedure.ZZ6_CountryOrGrouping;
        attr.ZXB_IsEditable = true;
        this.props.entityManager.add(attr, "RefCusProcedureAttributeUserView");
        this.setState({attributes: update(this.state.attributes, {$unshift: [attr]})});
    }

    async onAttributeValueChange(entity: any, name: string, value: object) {
        let attr = entity as RefCusProcedureAttributeUserView;
        if (attr) {
            let index = this.state.attributes.findIndex(x => x.ZXB_PK == attr.ZXB_PK);
            if (index >= 0) {
                let updatedAttr = update(this.state.attributes[index], {[name]: {$set: value}});
                this.props.entityManager.update(updatedAttr);
                this.setState({attributes: update(this.state.attributes, {$splice: [[index, 1, updatedAttr]]})});
            }
        }
    }

    async onAttributeValueChanged(entity : any, name : string) {
        let attr = entity as RefCusProcedureAttributeUserView;
        if (attr) {
            let index = this.state.attributes.findIndex(x => x.ZXB_PK == attr.ZXB_PK);
            if (index >= 0) {
                let updatedAttr = this.state.attributes[index]
                await this.getAndUpdateAttributeValidations(updatedAttr, name);
            }
        }
    }

    async getAndUpdateAttributeValidations(attr: RefCusProcedureAttributeUserView, name: string) {
        let validationResults = await ValidationHelper.validateProperty(this.validationServices, attr, name, 1);
        let attrValidationResult = update(this.state.attrValidationResults[attr.ZXB_PK] || {}, {[name]: {$set: validationResults}});
        let attrValidationResults = update(this.state.attrValidationResults, {[attr.ZXB_PK]: {$set: attrValidationResult}});
        this.setState({attrValidationResults: attrValidationResults});
    }

    get isFieldReadOnly() {
        return this.props.entityManager.isInDatabase(this.state.procedure);
    }

    async onValueChange(entity : any, name : string, value: object) {
        let procedure = update(this.state.procedure, {[name]: {$set: value}});
        this.setState({procedure: procedure});
        this.props.entityManager.update(procedure);
    }

    async onValueChanged(entity : any, name : string) {
        if (name == "ZZ6_CountryOrGrouping") {
            let propertyName = "ZXB_CountryOrGrouping";
            let attributes = this.state.attributes.map(attr => update(attr, {[propertyName]: {$set: entity[name]}}));
            this.setState({attributes: attributes});
        }
        let procedure = this.state.procedure;
        let validaitionResults = update(this.state.procedureValidationResults, {[name]: {$set: await ValidationHelper.validateProperty(this.validationServices, procedure, name, 0)}});
        this.setState({procedureValidationResults : validaitionResults});
    }

    async save() {
        let procedureValidationResults = await ValidationHelper.validate(this.validationServices, this.state.procedure, 0, true);
        let attributeValidationResults: {
            [pk: string]: IValidationResults
        } = {};
        await Promise.all(this.state.attributes.map(a => ValidationHelper.validate(this.validationServices, a, 1, true).then(r => attributeValidationResults[a.ZXB_PK] = r)));
        this.setState({
            procedureValidationResults : procedureValidationResults,
            attrValidationResults : attributeValidationResults
        });

        if (!ValidationHelper.hasErrors(this.getValidationResults())) {
            let result = await this.props.entityManager.saveChanges(ServiceType.Safe);
            this.setState({saveMessage: result.message});
            if (this.props.onSaved) {
                this.props.onSaved();
            }
        }
        else {
            this.setState({saveMessage: "Please fix all errors before saving."});
        }
    }

    getValidationResults() {
        return Object.getOwnPropertyNames(this.state.attrValidationResults).map(p => this.state.attrValidationResults[p]).concat(this.state.procedureValidationResults);
    }

    render() {
        return (
            <div>
                <div className="row">
                    <div className="col-sm-7">
                        <TextInput parentProps={this.state} inputType="Text" label="Procedure Code" propertyName="ZZ6_ProcedureCode" entity={this.state.procedure} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} readOnly={this.isFieldReadOnly}
                        validationResults={this.state.procedureValidationResults} />
                        <TextInput parentProps={this.state} inputType="Text" label="Previous Procedure Code" propertyName="ZZ6_PreviousProcedureCode" entity={this.state.procedure} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} readOnly={this.isFieldReadOnly}
                        validationResults={this.state.procedureValidationResults} />
                        <TextInput parentProps={this.state} inputType="Text" label="Concession" propertyName="ZZ6_Concession" entity={this.state.procedure} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} readOnly={this.isFieldReadOnly}
                        validationResults={this.state.procedureValidationResults} />
                        <TextArea parentProps={this.state} label="Description" propertyName="ZZ6_Description" entity={this.state.procedure} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} />
                        <CodeInput parentProps={this.state} label="Country Or Grouping" propertyName="ZZ6_CountryOrGrouping" entity={this.state.procedure} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} readOnly={this.isFieldReadOnly}
                        entityManager={this.props.entityManager} listCodePropertyName="ZZZ_DataGrouping" listDescriptionPropertyName="ZZZ_Description" listEntityTypeName="RefDataGrouping" />
                        <TextSelect parentProps={this.state} label="Shipment Type" propertyName="ZZ6_ShipmentType" entity={this.state.procedure} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults}
                        options={["TNP", "EXW,IMX", "OUT", "EXP", "IMP", "IMP,IMX", "IMP,EXW", "WAD", "IMP,EXP", "INP", "IPT", "EXW", "Blank"]} values={["TNP", "EXW,IMX", "OUT", "EXP", "IMP", "IMP,IMX", "IMP,EXW", "WAD", "IMP,EXP", "INP", "IPT", "EXW", ""]} />
                        <TextInput parentProps={this.state} inputType="Text" label="Group" propertyName="ZZ6_Group" entity={this.state.procedure} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged}
                        validationResults={this.state.procedureValidationResults} />
                        <TextInput parentProps={this.state} inputType="Text" label="Category" propertyName="ZZ6_Category" entity={this.state.procedure} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged}
                            validationResults={this.state.procedureValidationResults} />
                        <DateTimeInput parentProps={this.state} label="Start Date" entity={this.state.procedure} propertyName="ZZ6_StartDate" onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} format="DD/MM/YYYY" validationResult={this.state.procedureValidationResults} />
                        <DateTimeInput parentProps={this.state} label="End Date" entity={this.state.procedure} propertyName="ZZ6_EndDate" onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} format="DD/MM/YYYY" validationResult={this.state.procedureValidationResults}/>
                        <div className="form-group row">
                            <div className="col-sm-6">
                                <CheckBox parentProps={this.state} label="Calculate Duty" propertyName="ZZ6_CalculateDuty" entity={this.state.procedure} onValueChange={this.onValueChange} validationResults={this.state.procedureValidationResults} />
                            </div>
                            <div className="col-sm-6">
                                <CheckBox parentProps={this.state} label="Landed Cost" propertyName="ZZ6_LandedCost" entity={this.state.procedure} onValueChange={this.onValueChange} validationResults={this.state.procedureValidationResults} />
                            </div>
                        </div>
                        <div className="form-group row">
                            <div className="col-sm-6">
                                <CheckBox parentProps={this.state} label="Calculate VAT" propertyName="ZZ6_CalculateVAT" entity={this.state.procedure} onValueChange={this.onValueChange} validationResults={this.state.procedureValidationResults} />
                            </div>
                            <div className="col-sm-6">
                                <CheckBox parentProps={this.state} label="Is Published" propertyName="ZZ6_IsPublished" entity={this.state.procedure} onValueChange={this.onValueChange} validationResults={this.state.procedureValidationResults} />
                            </div>
                        </div>
                        <div className="form-group row">
                            <div className="col-sm-6">
                                <TextSelect parentProps={this.state} label="Into Warehouse" propertyName="ZZ6_IntoWarehouse" entity={this.state.procedure} onValueChange={this.onValueChange} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} />
                            </div>
                            <div className="col-sm-6">
                                <TextSelect parentProps={this.state} label="Out of Warehouse" propertyName="ZZ6_OutOfWarehouse" entity={this.state.procedure} onValueChange={this.onValueChange} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} />
                            </div>
                            <div className="col-sm-6">
                                <TextSelect parentProps={this.state} label="Into Inward Processing" propertyName="ZZ6_IntoInwardProcessing" entity={this.state.procedure} onValueChange={this.onValueChange} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} />
                            </div>
                            <div className="col-sm-6">
                                <TextSelect parentProps={this.state} label="Out of Inward Processing" propertyName="ZZ6_OutOfInwardProcessing" entity={this.state.procedure} onValueChange={this.onValueChange} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} />
                            </div>
                            <div className="col-sm-6">
                                <TextSelect parentProps={this.state} label="Into Outward Processing" propertyName="ZZ6_IntoOutwardProcessing" entity={this.state.procedure} onValueChange={this.onValueChange} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} />
                            </div>
                            <div className="col-sm-6">
                                <TextSelect parentProps={this.state} label="Out of Outward Processing" propertyName="ZZ6_OutofOutwardProcessing" entity={this.state.procedure} onValueChange={this.onValueChange} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} />
                            </div>
                            <div className="col-sm-6">
                                <TextSelect parentProps={this.state} label="Into Temporary Import" propertyName="ZZ6_IntoTemporaryImport" entity={this.state.procedure} onValueChange={this.onValueChange} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} />
                            </div>
                            <div className="col-sm-6">
                                <TextSelect parentProps={this.state} label="Out of Temporary Import" propertyName="ZZ6_OutOfTemporaryImport" entity={this.state.procedure} onValueChange={this.onValueChange} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} />
                            </div>
                            <div className="col-sm-6">
                                <TextSelect parentProps={this.state} label="Into Temporary Export" propertyName="ZZ6_IntoTemporaryExport" entity={this.state.procedure} onValueChange={this.onValueChange} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} />
                            </div>
                            <div className="col-sm-6">
                                <TextSelect parentProps={this.state} label="Out of Temporary Export" propertyName="ZZ6_OutOfTemporaryExport" entity={this.state.procedure} onValueChange={this.onValueChange} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} />
                            </div>
                            <div className="col-sm-6">
                                <TextSelect parentProps={this.state} label="Is Guarantee Consumed" propertyName="ZZ6_IsGuaranteeConsumed" entity={this.state.procedure} onValueChange={this.onValueChange} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} />
                            </div>
                            <div className="col-sm-6">
                                <TextSelect parentProps={this.state} label="Is Guarantee Released" propertyName="ZZ6_IsGuaranteeReleased" entity={this.state.procedure} onValueChange={this.onValueChange} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} />
                            </div>
                            <div className="col-sm-6">
                                <TextSelect parentProps={this.state} label="Is Transit" propertyName="ZZ6_IsTransit" entity={this.state.procedure} onValueChange={this.onValueChange} options={["Yes", "No", "Ignore"]} values={["Y", "N", "I"]} onValueChanged={this.onValueChanged} validationResults={this.state.procedureValidationResults} />
                            </div>
                        </div>
                        <div className="form-group row">
                            <div className="col-sm-1">
                                <Button type='button' className="btn btn-info" onClick={this.save} disabled={this.state.readOnly || this.state.saveButtonDisabled} >Save</Button>
                            </div>
                        </div>
                    </div>
                    <div className="col-sm-5">
                        <RefCusProcedureAttributeDetailsForm attributes={this.state.attributes} onAttributeValueChange={this.onAttributeValueChange} onAttributeValueChanged={this.onAttributeValueChanged} 
                        addNewAttribute={this.addNewAttribute} removeAttribute={this.removeAttribute} validationResults={this.state.attrValidationResults} readOnly={this.state.readOnly} entityManager={this.props.entityManager} />
                    </div>
                </div>
                {this.state.saveMessage.length > 0 ?  <PopupForm title="Information" message={this.state.saveMessage} handleHideModal={() => this.setState({saveMessage: ""})} validationResults={this.getValidationResults()} /> : null}
            </div>
        );
    }
}
