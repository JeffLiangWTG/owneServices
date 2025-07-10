import React from "react";
import { CheckBox } from "./CheckBox";
import { IValidationResults, ValidationResults } from "./ValidationResults";
import { EntityHelper } from "./EntityHelper";
import { IParentProps } from "./IParentProps";
import { CodeInput } from "./CodeInput";
import { IEntityManager, ServiceType } from "./EntityManager";
import { Filter, IFilter, FilterOps } from "./Filter";
import { RefCusCodeListUserView } from "./models/RefCusCodeListUserView";
import IRefCusCodeListAttributeName from "./models/IRefCusCodeListAttributeName";
import { RefCusCodeListAttributeUserView } from "./models/RefCusCodeListAttributeUserView"
import { DateTimeInput } from "./DateTimeInput";
import Button from "./Button";

export interface IRefCusCodeListAttributeDetailsProps extends IParentProps {
    attributes : RefCusCodeListAttributeUserView[],
    onAttributeValueChange : (entity : any, name: string, value: object) => void,
    onAttributeValueChanged : (entity : any, name: string) => void,
    addNewAttribute : () => void,
    removeAttribute : (id : string) => void,
    validationResults : {
        [pk : string] : IValidationResults
    },
    entityManager : IEntityManager,
    codeList: RefCusCodeListUserView,
}

interface IRefCusCodeListAttributeDetailsStates {
    currentId : string;
    currentValueList: RefCusCodeListUserView[];
    attributeNames: IRefCusCodeListAttributeName[];
}

export class RefCusCodeListAttributeDetailsForm extends React.Component<IRefCusCodeListAttributeDetailsProps, IRefCusCodeListAttributeDetailsStates> {
    constructor(props : IRefCusCodeListAttributeDetailsProps) {
        super(props);
        let att = new RefCusCodeListAttributeUserView();
        this.state = {
            currentId : "",
            currentValueList: [],
            attributeNames: []
        }
        this.onCurrentIdxChangeOnFocusElement = this.onCurrentIdxChangeOnFocusElement.bind(this);
        this.onCurrentIdxChangeOnClick = this.onCurrentIdxChangeOnClick.bind(this);
    }

    codeTypeCodeListCache: Map<string, RefCusCodeListUserView[]> = new Map<string, RefCusCodeListUserView[]>();
    hasLoadedAttributeNamesOnce: boolean = false;

    getCodeListFilters(codeType: string, country: string) : IFilter[] {
        return [
            new Filter("ZZD_CodeType", FilterOps.Equals, codeType as any, "string"),
            new Filter("ZZD_CountryOrGrouping", FilterOps.Equals, country as any, "string")
        ]
    }

    getCurrentEntity() : RefCusCodeListAttributeUserView | null {
        return this.props.attributes.find(x => x.ZZE_PK == this.state.currentId) || null;
    }

    getCodeTypeForValueListFromAttributeNamesList(attrName: string) : string {
        if (this.state.attributeNames.length > 0) {
            let attr = this.state.attributeNames.find(x => x.ZXE_Name == attrName);
            if (attr && attr.ZXE_ZZK_NKCodeTypeForValueList) {
                return attr.ZXE_ZZK_NKCodeTypeForValueList;
            }
        }
        return "";
    }

    async getAttributeNamesByCodeAndCountry(codeType: any, countryOrGrouping: any) : Promise<IRefCusCodeListAttributeName[]> {
        let filters = [
			new Filter("ZXE_ZZK_NKCodeType", FilterOps.Equals, codeType, "string" ),
            new Filter("ZXE_ZZZ_NKDataGrouping", FilterOps.Equals, countryOrGrouping, "string")
        ];
        return await this.props.entityManager.getAsync<IRefCusCodeListAttributeName>("RefCusCodeListAttributeName", [ServiceType.Safe], filters, false);
    }

    async getCodeListByCodeType(codeType: string) : Promise<RefCusCodeListUserView[]> {
        if(!codeType || codeType == "")
        {
            return [];
        }
        else {
            if(!this.codeTypeCodeListCache.has(codeType)) {
                this.codeTypeCodeListCache.set(codeType, await this.props.entityManager.getAsync<RefCusCodeListUserView>("RefCusCodeListUserView", [ServiceType.Safe], this.getCodeListFilters(codeType, this.props.codeList.ZZD_CountryOrGrouping), false));
            }
            let codeLists = this.codeTypeCodeListCache.get(codeType);
            return codeLists ? codeLists : [];
        }
    }

    async onCurrentIdxChangeOnFocusElement(e : React.SyntheticEvent<HTMLElement>, id : string, attrName: string = "") : Promise<void> {
        this.setState({ currentId : id});
        if(attrName != "") {
            let codeTypeForValueList = this.getCodeTypeForValueListFromAttributeNamesList(attrName);
            this.setState({currentValueList : await this.getCodeListByCodeType(codeTypeForValueList)});
         }
    }

    onCurrentIdxChangeOnClick(id : string) : void {
        if (this.state.currentId !== id) {
            this.setState({ currentId : id});
        }
    }

    async componentDidUpdate(prevProps: IRefCusCodeListAttributeDetailsProps) {
        if (prevProps.codeList.ZZD_CountryOrGrouping.length > 1 && prevProps.codeList.ZZD_CodeType.length > 0
            && ((prevProps.codeList.ZZD_CountryOrGrouping != this.props.codeList.ZZD_CountryOrGrouping || prevProps.codeList.ZZD_CodeType != this.props.codeList.ZZD_CodeType) 
            || !this.hasLoadedAttributeNamesOnce)) {
                this.setState({ attributeNames : await this.getAttributeNamesByCodeAndCountry(this.props.codeList.ZZD_CodeType, this.props.codeList.ZZD_CountryOrGrouping) });
                this.hasLoadedAttributeNamesOnce = true;
        }
    }

    isDateRangeUsedForCurrentAttributeName (currentName:string) {
        return this.state.attributeNames?.find(x => x.ZXE_Name == currentName)?.ZXE_IsDateRangeUsed;
    }

    render() {
        return <div>
            <div className="container row">
                <div className="col-sm-auto ml-2">
                      <h3>Attributes</h3>
                 </div>
                 <div className="col">
					<Button type='button' className="btn btn-info" onClick={this.props.addNewAttribute} disabled={this.props.readOnly}>Add</Button>
                 </div>
            </div>
            <div className="container">
                {this.props.attributes.map((r,i) => 
                <div className="border rounded" id={`attribute-${i}`} onClick={() => this.onCurrentIdxChangeOnClick(r.ZZE_PK)} style={{padding:10, margin:10}} key={r.ZZE_PK}>
                    <div className="form-group row">
                        <div className="col-sm-5">Name</div>
                        <div className="col-sm-5">Value</div>
                        <div></div>
                    </div>
                    <div className="form-group row">
                            <div className="col-sm-5" onFocus={e => this.onCurrentIdxChangeOnFocusElement(e, r.ZZE_PK, r.ZZE_ZXE_NKName)}>
                                <CodeInput readOnly={this.props.readOnly} propertyName="ZZE_ZXE_NKName" entity={r} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} validationResults={this.props.validationResults[r.ZZE_PK]} listCodePropertyName="ZXE_Name"
                                    listDescriptionPropertyName="ZXE_Name" listEntityTypeName="RefCusCodeListAttributeName" maxLength={32} entityManager={this.props.entityManager} prevEntity={this.state.attributeNames} />
                            </div>
                            <div className="col-sm-5" onFocus={e => this.onCurrentIdxChangeOnFocusElement(e, r.ZZE_PK, r.ZZE_ZXE_NKName)}>
                                <CodeInput readOnly={this.props.readOnly} propertyName="ZZE_Value" entity={r} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} validationResults={this.props.validationResults[r.ZZE_PK]} listCodePropertyName="ZZD_Code"
                                    listDescriptionPropertyName="ZZD_Description" listEntityTypeName="RefCusCodeListUserView" maxLength={255} entityManager={this.props.entityManager} prevEntity={this.state.currentValueList} />
                            </div>
                            <div>
								<Button type='button' className="btn btn-info" onClick={()=>this.props.removeAttribute(r.ZZE_PK)} disabled={this.props.readOnly || !EntityHelper.isEditable(r)}>-</Button>
                            </div>
                    </div>
                    {r.ZZE_PK === this.state.currentId ?
                    <>
                        { this.isDateRangeUsedForCurrentAttributeName(r.ZZE_ZXE_NKName) ? <div className="panel panel-default">
                            <div className="panel-body">
                                <div className="form-group row">
                                    <div className="col-sm-5">Start Date</div>
                                    <div className="col-sm-5">End Date</div>
                                    <div></div>
                                </div>
                                <div className="form-group row">
                                    <div className="col-sm-5">
                                        <DateTimeInput readOnly={this.props.readOnly} entity={r} format="DD/MM/YYYY" onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} propertyName="ZZE_StartDate" validationResult={this.props.validationResults[r.ZZE_PK]} />
                                    </div>
                                    <div className="col-sm-5">
                                        <DateTimeInput readOnly={this.props.readOnly} entity={r} format="DD/MM/YYYY" onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} propertyName="ZZE_EndDate" validationResult={this.props.validationResults[r.ZZE_PK]} />
                                    </div>
                                </div>
                            </div>
                        </div> : ""}
                        <div className="panel panel-default">
                            <div className="panel-heading">Transport Mode</div>
                            <div className="panel-body">
                                <div className="form-group row">
                                    <div className="col-sm-1"></div>
                                    <div className="col-sm-3">
                                        <CheckBox parentProps={this.props} label="ROA" propertyName="ZZE_IsRoa" entity={this.getCurrentEntity()} onValueChange={(_, p, v) => this.props.onAttributeValueChange(this.getCurrentEntity(), p, v)} inlineLabel={true} validationResults={this.props.validationResults[this.state.currentId]}/>
                                        <CheckBox parentProps={this.props} label="RAI" propertyName="ZZE_IsRai" entity={this.getCurrentEntity()} onValueChange={(_, p, v) => this.props.onAttributeValueChange(this.getCurrentEntity(), p, v)} inlineLabel={true} validationResults={this.props.validationResults[this.state.currentId]}/>
                                        <CheckBox parentProps={this.props} label="INW" propertyName="ZZE_IsInw" entity={this.getCurrentEntity()} onValueChange={(_, p, v) => this.props.onAttributeValueChange(this.getCurrentEntity(), p, v)} inlineLabel={true} validationResults={this.props.validationResults[this.state.currentId]}/>
                                    </div>
                                    <div className="col-sm-3">
                                        <CheckBox parentProps={this.props} label="SEA" propertyName="ZZE_IsSea" entity={this.getCurrentEntity()} onValueChange={(_, p, v) => this.props.onAttributeValueChange(this.getCurrentEntity(), p, v)} inlineLabel={true} validationResults={this.props.validationResults[this.state.currentId]}/>
                                        <CheckBox parentProps={this.props} label="MAI" propertyName="ZZE_IsMai" entity={this.getCurrentEntity()} onValueChange={(_, p, v) => this.props.onAttributeValueChange(this.getCurrentEntity(), p, v)} inlineLabel={true} validationResults={this.props.validationResults[this.state.currentId]}/>
                                    </div>
                                    <div className="col-sm-3">
                                        <CheckBox parentProps={this.props} label="AIR" propertyName="ZZE_IsAir" entity={this.getCurrentEntity()} onValueChange={(_, p, v) => this.props.onAttributeValueChange(this.getCurrentEntity(), p, v)} inlineLabel={true} validationResults={this.props.validationResults[this.state.currentId]}/>
                                        <CheckBox parentProps={this.props} label="FIX" propertyName="ZZE_IsFix" entity={this.getCurrentEntity()} onValueChange={(_, p, v) => this.props.onAttributeValueChange(this.getCurrentEntity(), p, v)} inlineLabel={true} validationResults={this.props.validationResults[this.state.currentId]}/>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </> : null}
                </div>
                )}
            </div>       
        </div>
    }
}
