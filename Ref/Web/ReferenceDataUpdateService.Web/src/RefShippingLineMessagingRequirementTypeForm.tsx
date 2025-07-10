import React from "react";
import _ from "underscore";
import { CheckBox } from "./CheckBox";
import { IParentProps } from "./IParentProps";
import IRefShippingLineMessagingRequirement from "./models/IRefShippingLineMessagingRequirement";
import IRefShippingLineMessagingRequirementType from "./models/IRefShippingLineMessagingRequirementType";
import { IEntity } from "./models/IEntity";

interface IRefShippingLineMessagingRequirementTypeFormProps extends IParentProps {
    requirements: IRefShippingLineMessagingRequirement[],
    requirementTypes: IRefShippingLineMessagingRequirementType[],
    readonly?: boolean,
    onMessagingRequirementChange(entity: any, name: string, value: object): Promise<void>
}

export class RefShippingLineMessagingRequirementTypeForm extends React.Component<IRefShippingLineMessagingRequirementTypeFormProps> {
    constructor(props: IRefShippingLineMessagingRequirementTypeFormProps) {
        super(props);
        this.onMessagingRequirementChange = this.onMessagingRequirementChange.bind(this);
    }

    getRequirement(type: string) {
        return this.props.requirements.find(x => x.RSR_RST_NKType == type);
    }

    async onMessagingRequirementChange(entity: any, name: string, value: object): Promise<void> {
        await this.props.onMessagingRequirementChange(entity, name, value);
    }

    render() {
        return <div className="container border">
            <table className="table table-striped">
                <thead>
                    <tr>
                        <th scope="col" className="col-sm-6">Messaging Requirements</th>
                        <th scope="col">Booking Request</th>
                        <th scope="col">Shipping Order</th>
                        <th scope="col">Shipping Instruction</th>
                        <th scope="col">eManifest</th>
                        <th scope="col">VGM</th>
                    </tr>
                </thead>

                <tbody>
                    {this.props.requirementTypes.map((x) => {
                        return <tr key={x.RST_PK}>
                            <td>({x.RST_Code}) {x.RST_Description}</td>
                            <td style={{ textAlign: "center" }}>
                                <CheckBox propertyName="RSR_IsBookingRequest" isReadOnly={this.props.readOnly} entity={this.getRequirement(x.RST_Code) as IEntity} onValueChange={this.onMessagingRequirementChange} />
                            </td>
                            <td style={{ textAlign: "center" }}>
                                <CheckBox propertyName="RSR_IsShippingOrder" isReadOnly={this.props.readOnly} entity={this.getRequirement(x.RST_Code) as IEntity} onValueChange={this.onMessagingRequirementChange} />
                            </td>
                            <td style={{ textAlign: "center" }}>
                                <CheckBox propertyName="RSR_IsShippingInstruction" isReadOnly={this.props.readOnly} entity={this.getRequirement(x.RST_Code) as IEntity} onValueChange={this.onMessagingRequirementChange} />
                            </td>
                            <td style={{ textAlign: "center" }}>
                                <CheckBox propertyName="RSR_IsEManifest" isReadOnly={this.props.readOnly} entity={this.getRequirement(x.RST_Code) as IEntity} onValueChange={this.onMessagingRequirementChange} />
                            </td>
                            <td style={{ textAlign: "center" }}>
                                <CheckBox propertyName="RSR_IsVerifiedGrossContainerWeight" isReadOnly={this.props.readOnly} entity={this.getRequirement(x.RST_Code) as IEntity} onValueChange={this.onMessagingRequirementChange} />
                            </td>
                        </tr>
                    })}
                </tbody>
            </table>
        </div>
    }
}