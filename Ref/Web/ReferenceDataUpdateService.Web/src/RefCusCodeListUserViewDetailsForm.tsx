import React from "react";
import { RefCusCodeListUserView } from "./models/RefCusCodeListUserView";
import uuid from "uuid";
import { IEntityManager, ServiceType } from "./EntityManager";
import _ from "underscore";
import { Filter, FilterOps } from "./Filter";
import { RefCusCodeListAttributeDetailsForm } from "./RefCusCodeListAttributeDetailsForm";
import update from "immutability-helper";
import { TextInput } from "./TextInput";
import { TextArea } from "./TextArea";
import { CheckBox } from "./CheckBox";
import { DateTimeInput } from "./DateTimeInput";
import { ValidationServiceWrapper } from "./ValidationService";
import { IValidationResults } from "./ValidationResults";
import { ValidationHelper } from "./ValidationHelper";
import { CodeInput } from "./CodeInput";
import { ValidationServiceHelper } from "./ValidationServiceHelper";
import { PopupForm } from "./PopupForm";
import { IParentProps } from "./IParentProps";
import IRefCusCodeListAttributeName from "./models/IRefCusCodeListAttributeName";
import IRefCusCodeType from "./models/IRefCusCodeType";
import { RefCusCodeListAttributeUserView } from "./models/RefCusCodeListAttributeUserView";
import Button from "./Button";

export interface IRefCusCodeListUserViewDetailsFormProps {
    id? : string,
    systemVersion?: string,
    entityManager : IEntityManager,
    onSaved?: () => Promise<void>,
}

interface IRefCusCodeListUserViewDetailsFormState extends IParentProps {
    code : RefCusCodeListUserView,
    attributes : RefCusCodeListAttributeUserView[],
    codeValidationResults : IValidationResults,
    attrValidationResults : {
        [pk : string] : IValidationResults
    },
    saveMessage : string,
	saveButtonDisabled: boolean,
    filteredCodeTypes: IRefCusCodeType[]
}

export class RefCusCodeListUserViewDetailsForm extends React.Component<IRefCusCodeListUserViewDetailsFormProps, IRefCusCodeListUserViewDetailsFormState> {
    constructor(props : IRefCusCodeListUserViewDetailsFormProps){
        super(props);
        this.onAttributeValueChange = this.onAttributeValueChange.bind(this);
        this.onAttributeValueChanged = this.onAttributeValueChanged.bind(this);
        this.onValueChange = this.onValueChange.bind(this);
        this.onValueChanged = this.onValueChanged.bind(this);
        this.addNewAttribute = this.addNewAttribute.bind(this);
        this.removeAttribute = this.removeAttribute.bind(this);
        this.save = this.save.bind(this);
		this.updateSaveButtonDisabledProperty = this.updateSaveButtonDisabledProperty.bind(this);
        let codeList = new RefCusCodeListUserView();
        codeList.ZZD_PK = uuid.v1();
        codeList.ZZD_Code = "";
        codeList.ZZD_CodeType = "";
        codeList.ZZD_Description = "";
        codeList.ZZD_CountryOrGrouping = "";
        codeList.ZZD_StartDate = "";
        codeList.ZZD_EndDate = "2079-06-06T23:59:00Z";
        codeList.ZZD_IsAir = false;
        codeList.ZZD_IsSea = false;
        codeList.ZZD_IsFix = false;
        codeList.ZZD_IsInw = false;
        codeList.ZZD_IsRai = false;
        codeList.ZZD_IsMai = false;
        codeList.ZZD_IsRoa = false;
        codeList.ZZD_IsSystem = true;
        codeList.ZZD_IsPublished = true;
        codeList.ZZD_IsEditable = true;
        this.state = { 
            code : codeList,
            attributes : [],
            codeValidationResults : {},
            attrValidationResults : {},
            saveMessage : "",
            readOnly : false,
            saveButtonDisabled : false,
            filteredCodeTypes: []
        };
        this.validationServices = new ValidationServiceWrapper([ ValidationServiceHelper.getRefCusCodeListValidationService(this.props.entityManager), ValidationServiceHelper.getRefCusCodeListAttributeValidationService(this.props.entityManager) ], this.updateSaveButtonDisabledProperty);
    }

    validationServices : ValidationServiceWrapper;

	updateSaveButtonDisabledProperty(isValidating: boolean)
	{
		this.setState({ saveButtonDisabled: isValidating});
	}

    async componentDidMount(){
        await this.loadCodeList();
    }

    async componentDidUpdate(prevProps: IRefCusCodeListUserViewDetailsFormProps)
    {
        if (prevProps.systemVersion !== this.props.systemVersion) {
            this.props.entityManager.clear();
            await this.loadCodeList();
        }
    }

	// This is a temporary solution util we convert the class to functional component
	async componentWillUnmount() {
		await Promise.all([
			this.props.entityManager.reload("RefCusCodeListUserView", [ServiceType.Safe], [new Filter("ZZD_PK", FilterOps.Equals, this.props.id as any, "guid") ]),
			this.props.entityManager.reload("RefCusCodeListAttributeUserView", [ServiceType.Safe], [new Filter("ZZE_ZZD_CodeList", FilterOps.Equals, this.props.id as any, "guid")]),
		]);
	}

    async loadCodeList()
    {
        if (this.props.id) {
            this.setState({readOnly : this.props.systemVersion !== undefined});
            var dataArray = await Promise.all([ 
                this.props.entityManager.getAsync<RefCusCodeListUserView>("RefCusCodeListUserView", [ServiceType.Safe], [new Filter("ZZD_PK", FilterOps.Equals, this.props.id as any, "guid") ], false, this.props.systemVersion),
                this.props.entityManager.getAsync<RefCusCodeListAttributeUserView>("RefCusCodeListAttributeUserView", [ServiceType.Safe], [new Filter("ZZE_ZZD_CodeList", FilterOps.Equals, this.props.id as any, "guid")], false, this.props.systemVersion)
            ]);
            var data = _.first(dataArray[0]);
            if (data) {
                this.setState({ code : data, attributes : dataArray[1]});
            }
        }
        else {
			this.props.entityManager.add(this.state.code, "RefCusCodeListUserView");
        }
    }

    removeAttribute(id : string) : void {
        let index = this.state.attributes.findIndex(x => x.ZZE_PK == id);
        if (index >= 0) {
            this.props.entityManager.remove(this.state.attributes[index]);
            this.setState({ attributes : update(this.state.attributes,  { $splice : [[index, 1]] } )});
            this.state.attributes.map(x =>  this.getAndUpdateAttributeValidations(x, "ZZE_ZXE_NKName"));
        }
    }

    addNewAttribute() : void {
        let attr = new RefCusCodeListAttributeUserView();
        attr.ZZE_PK = uuid.v1();
        attr.ZZE_ZZD_CodeList = this.state.code.ZZD_PK;
        attr.ZZE_ZXE_NKName = "";
        attr.ZZE_Value = "";
        attr.ZZE_IsAir = false;
        attr.ZZE_IsSea = false;
        attr.ZZE_IsFix = false;
        attr.ZZE_IsRai = false;
        attr.ZZE_IsMai = false;
        attr.ZZE_IsInw = false;
        attr.ZZE_IsRoa = false;
        attr.ZZE_CodeType = this.state.code.ZZD_CodeType;
        attr.ZZE_CountryOrGrouping = this.state.code.ZZD_CountryOrGrouping;
        attr.ZZE_IsEditable = true;

        this.props.entityManager.add(attr, "RefCusCodeListAttributeUserView");
        this.setState( { attributes : update(this.state.attributes, { $unshift : [attr] })});
    }

    async onAttributeValueChange(entity : any, name : string, value: object) : Promise<void> { 
        let attr = entity as RefCusCodeListAttributeUserView;
        if (attr) {
            let index = this.state.attributes.findIndex(x => x.ZZE_PK == attr.ZZE_PK);
            if (index >= 0) {
                let updatedAttr = update(this.state.attributes[index], { [name] : { $set : value}  });
                this.props.entityManager.update(updatedAttr);
                this.setState({ attributes : update(this.state.attributes, { $splice: [[index, 1, updatedAttr]] })});
            }
        }
    }

    async onAttributeValueChanged(entity : any, name : string) : Promise<void> { 
        let attr = entity as RefCusCodeListAttributeUserView;
        if (attr) {
            let index = this.state.attributes.findIndex(x => x.ZZE_PK == attr.ZZE_PK);
            if (index >= 0) {
                let updatedAttr = this.state.attributes[index]
                await this.getAndUpdateAttributeValidations(updatedAttr, name);
            }
        }
    }

   async getAndUpdateAttributeValidations(attr: any, name: string) : Promise<void> {
        if(name == "ZZE_ZXE_NKName" || name == "ZZE_Value") {
            let validationResultsName = await ValidationHelper.validateProperty(this.validationServices, attr, "ZZE_ZXE_NKName", 1);
            let validationResultsValue = await ValidationHelper.validateProperty(this.validationServices, attr, "ZZE_Value", 1);
            let validationResultsStartDate = await ValidationHelper.validateProperty(this.validationServices, attr, "ZZE_StartDate", 1);
            let validationResultsEndDate = await ValidationHelper.validateProperty(this.validationServices, attr, "ZZE_EndDate", 1);
            let attrValidationResult = update(this.state.attrValidationResults[attr.ZZE_PK] || {}, { 
                ZZE_ZXE_NKName : { $set : validationResultsName },
                ZZE_Value : { $set : validationResultsValue },
                ZZE_StartDate: { $set: validationResultsStartDate },
                ZZE_EndDate: { $set: validationResultsEndDate }
            });
            let attrValidationResults = update(this.state.attrValidationResults, {  [attr.ZZE_PK] : { $set : attrValidationResult } });
            this.setState({attrValidationResults : attrValidationResults});
        }
        else {
            let validationResults = await ValidationHelper.validateProperty(this.validationServices, attr, name, 1);
            let attrValidationResult = update(this.state.attrValidationResults[attr.ZZE_PK] || {}, { [name] : { $set : validationResults } });
            let attrValidationResults = update(this.state.attrValidationResults, {  [attr.ZZE_PK] : { $set : attrValidationResult } });
            this.setState({attrValidationResults : attrValidationResults});
        }
    }

    async onValueChange(entity : any, name : string, value: object) : Promise<void> {
        let code = update(this.state.code, { [name] : { $set : value} });
        this.setState({ code : code });
        this.props.entityManager.update(code);
    }

    async onValueChanged(entity : any, name : string) : Promise<void> {
        if (name == "ZZD_CodeType" || name == "ZZD_CountryOrGrouping") {
            let attributeName = name == "ZZD_CodeType" ? "ZZE_CodeType" : "ZZE_CountryOrGrouping";
            let attributes = this.state.attributes.map(attr => update(attr, { [attributeName] : { $set : entity[name] } }));
            this.setState( {attributes : attributes });

            if (name == "ZZD_CountryOrGrouping") {
                let filters = [
                    new Filter("ZZK_ZZZ_NKDataGrouping", FilterOps.Equals, entity[name] as any, "string" ),
                ];
                let codeTypes = await Promise.all([this.props.entityManager.getAsync<IRefCusCodeType>("RefCusCodeType", [ServiceType.Safe], filters, false)]);
                let codeList = update(this.state.code, { ZZD_CodeType : { $set : "" }});
                this.setState({ 
                    filteredCodeTypes: codeTypes[0],
                    code: codeList
                });
            }
        }
        let code = this.state.code;
        let validationResults = update(this.state.codeValidationResults, { [name] : { $set :  await ValidationHelper.validateProperty(this.validationServices, code, name, 0, false) } });
        this.setState({codeValidationResults : validationResults});
    }

    async getAttributeNamesByCodeAndCountry() : Promise<IRefCusCodeListAttributeName[]> {
        let filters = [
			new Filter("ZXE_ZZK_NKCodeType", FilterOps.Equals, this.state.code.ZZD_CodeType as any, "string" ),
            new Filter("ZXE_ZZZ_NKDataGrouping", FilterOps.Equals, this.state.code.ZZD_CountryOrGrouping as any, "string")
        ];

        return await this.props.entityManager.getAsync<IRefCusCodeListAttributeName>("RefCusCodeListAttributeName", [ServiceType.Safe], filters, false);
    }

    get isKeyReadOnly() : boolean {
        return this.props.entityManager.isInDatabase(this.state.code) && this.state.code.ZZD_IsSystem;
    }

    async save() : Promise<void> {
        let codeValidationResults = await ValidationHelper.validate(this.validationServices, this.state.code, 0, true);
        let attributeValidationResults : {
            [pk : string] : IValidationResults
        } = {};
        await Promise.all(this.state.attributes.map(a => ValidationHelper.validate(this.validationServices, a, 1, true).then(r => attributeValidationResults[a.ZZE_PK] = r )));
        this.setState({
            codeValidationResults : codeValidationResults,
            attrValidationResults : attributeValidationResults
        });

        if (!ValidationHelper.hasErrors(this.getValidationResults())) {
            let result = await this.props.entityManager.saveChanges(ServiceType.Safe);
            this.setState({saveMessage : result.message});
            if (this.props.onSaved){
                this.props.onSaved();
            }
        }
        else {
            this.setState({saveMessage : "Please fix all errors before saving."});
        }
    }

    getValidationResults() : IValidationResults[] {
        return Object.getOwnPropertyNames(this.state.attrValidationResults).map(p => this.state.attrValidationResults[p]).concat(this.state.codeValidationResults);
    }

    render() {
        return <div>
        <div className="row">
        <div className="col-sm-7">
            <CodeInput parentProps={this.state} label="Grouping" propertyName="ZZD_CountryOrGrouping" entity={this.state.code} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} readOnly={this.isKeyReadOnly} validationResults={this.state.codeValidationResults}
            entityManager={this.props.entityManager} listCodePropertyName="ZZZ_DataGrouping" listDescriptionPropertyName="ZZZ_Description" listEntityTypeName="RefDataGrouping" />
            <CodeInput parentProps={this.state} label="List Type" propertyName="ZZD_CodeType" entity={this.state.code} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} readOnly={this.isKeyReadOnly} validationResults={this.state.codeValidationResults} 
            entityManager={this.props.entityManager} listCodePropertyName="ZZK_CodeType" listDescriptionPropertyName="ZZK_Description" listEntityTypeName="RefCusCodeType" prevEntity={this.state.filteredCodeTypes} />
            <TextInput parentProps={this.state} inputType="Text" label="Code" propertyName="ZZD_Code" entity={this.state.code} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} readOnly={this.isKeyReadOnly}
            validationResults={this.state.codeValidationResults} />
            <TextArea parentProps={this.state} label="Description" propertyName="ZZD_Description" entity={this.state.code} onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} validationResults={this.state.codeValidationResults} />
            <div className="form-group row">
                <label className="col-sm-4 col-form-label">Transport Mode</label>
                <div className="col-sm-2">
                    <CheckBox parentProps={this.state} propertyName="ZZD_IsRoa" entity={this.state.code} onValueChange={this.onValueChange} label="ROA" inlineLabel={true} validationResults={this.state.codeValidationResults}/>
                    <CheckBox parentProps={this.state} propertyName="ZZD_IsRai" entity={this.state.code} onValueChange={this.onValueChange} label="RAI" inlineLabel={true} validationResults={this.state.codeValidationResults}/>
                    <CheckBox parentProps={this.state} propertyName="ZZD_IsInw" entity={this.state.code} onValueChange={this.onValueChange} label="INW" inlineLabel={true} validationResults={this.state.codeValidationResults}/>
                </div>
                <div className="col-sm-2">
                    <CheckBox parentProps={this.state} propertyName="ZZD_IsSea" entity={this.state.code} onValueChange={this.onValueChange} label="SEA" inlineLabel={true} validationResults={this.state.codeValidationResults}/>
                    <CheckBox parentProps={this.state} propertyName="ZZD_IsMai" entity={this.state.code} onValueChange={this.onValueChange} label="MAI" inlineLabel={true} validationResults={this.state.codeValidationResults}/>
                </div>
                <div className="col-sm-2">
                    <CheckBox parentProps={this.state} propertyName="ZZD_IsAir" entity={this.state.code} onValueChange={this.onValueChange} label="AIR" inlineLabel={true} validationResults={this.state.codeValidationResults}/>
                    <CheckBox parentProps={this.state} propertyName="ZZD_IsFix" entity={this.state.code} onValueChange={this.onValueChange} label="FIX" inlineLabel={true} validationResults={this.state.codeValidationResults}/>
                </div>
            </div>
            <DateTimeInput parentProps={this.state} label="Start Date" entity={this.state.code} propertyName="ZZD_StartDate" onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} format="DD/MM/YYYY" validationResult={this.state.codeValidationResults} />
            <DateTimeInput parentProps={this.state} label="End Date" entity={this.state.code} propertyName="ZZD_EndDate" onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} format="DD/MM/YYYY"  validationResult={this.state.codeValidationResults}/>
            <CheckBox parentProps={this.state} label="Is System" propertyName="ZZD_IsSystem" entity={this.state.code} onValueChange={this.onValueChange} validationResults={this.state.codeValidationResults} isReadOnly={true}/>
            <CheckBox parentProps={this.state} label="Is Published" propertyName="ZZD_IsPublished" entity={this.state.code} onValueChange={this.onValueChange} validationResults={this.state.codeValidationResults}/>
             <div className="form-group row">
                 <div className="col-sm-1">
                     <Button type='button' className="btn btn-info" onClick={this.save} disabled={this.state.readOnly || this.state.saveButtonDisabled} >Save</Button>
                 </div>
             </div>
        </div>
        <div className="col-sm-5">
            <RefCusCodeListAttributeDetailsForm attributes={this.state.attributes} onAttributeValueChange={this.onAttributeValueChange} onAttributeValueChanged={this.onAttributeValueChanged} 
                addNewAttribute={this.addNewAttribute} removeAttribute={this.removeAttribute} validationResults={this.state.attrValidationResults} readOnly={this.state.readOnly} entityManager={this.props.entityManager} codeList={this.state.code} />
        </div>
        </div>
        {this.state.saveMessage.length > 0 ?  <PopupForm title="Information" message={this.state.saveMessage} handleHideModal={() => this.setState({saveMessage : ""}) } validationResults={this.getValidationResults()}/> : null}
        </div>
    }
}
