import React from "react";
import { Component } from "react";
import { IEntityManager } from "./EntityManager";
import { IParentProps } from "./IParentProps";
import { RefCusProcedureAttributeUserView } from "./models/RefCusProcedureAttributeUserView";
import { TextInput } from "./TextInput";
import { IValidationResults } from "./ValidationResults";
import Button from "./Button";

interface IRefCusProcedureAttributeDetailsProps extends IParentProps {
    attributes: RefCusProcedureAttributeUserView[],
    onAttributeValueChange: (entity : any, name: string, value: object) => void,
    onAttributeValueChanged: (entity : any, name: string) => void,
    addNewAttribute: () => void,
    removeAttribute: (id : string) => void,
    validationResults: {
        [pk: string]: IValidationResults
    },
    entityManager: IEntityManager,
}

interface IRefCusProcedureAttributeDetailsStates {
}

export class RefCusProcedureAttributeDetailsForm extends Component<IRefCusProcedureAttributeDetailsProps, IRefCusProcedureAttributeDetailsStates> {
    constructor(props: IRefCusProcedureAttributeDetailsProps) {
        super(props);
    }

    render() {
        return (
            <div>
                <div className="form-group row">
                    <div className="col-sm-1">
						<Button type='button' className="btn btn-info" onClick={this.props.addNewAttribute} disabled={this.props.readOnly} >Add</Button>
                    </div>
                </div>
                <div className="container">
                    <div className="form-group row">
                        <div className="col-sm-5">Name</div>
                        <div className="col-sm-5">Value</div>
                    </div>
                    {
                        this.props.attributes.map((v, i) => 
                            <div key={v.ZXB_PK}>
                                <div className="form-group row">
                                    <div className="col-sm-5">
                                        <TextInput inputType="Text" propertyName="ZXB_Name" entity={v} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged}
                                        validationResults={this.props.validationResults[v.ZXB_PK]} maxLength={50} />
                                    </div>
                                    <div className="col-sm-5">
                                        <TextInput inputType="Text" propertyName="ZXB_Value" entity={v} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged}
                                        validationResults={this.props.validationResults[v.ZXB_PK]} maxLength={500} />
                                    </div>
                                    <div>
										<Button type='button' className="btn btn-info" onClick={() => this.props.removeAttribute(v.ZXB_PK)} disabled={this.props.readOnly} >-</Button>
                                    </div>
                                </div>
                            </div>
                        )
                    }
                </div>
            </div>
        );
    }
}
