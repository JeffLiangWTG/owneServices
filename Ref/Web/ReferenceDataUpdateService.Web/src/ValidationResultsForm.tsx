import { IValidationResults } from "./ValidationResults";
import React from "react";
import { ValidationHelper } from "./ValidationHelper";

export interface IValidationResultsFormProps {
    validationResults? : IValidationResults[]
}

export class ValidationResultsForm extends React.Component<IValidationResultsFormProps, any> {
    constructor(props : IValidationResultsFormProps) {
        super(props);
    }

    render() {
        if (this.props.validationResults) {
			return <ul className="list-group">
				{this.props.validationResults!.filter(r => ValidationHelper.hasErrors([r])).map((r, ei) => <li className="list-group-item" key={"Entity_" + ei}>
                    Entity:
                        <ul className="list-group">
                            {Object.getOwnPropertyNames(r).filter(p => ValidationHelper.getValidationResults(p, r).length >0)
                            .map((p, pi) => <li className="list-group-item" key={"Entity_" + ei + "_" + p}>
                                {p}:
                                    <ul className="list-group">
                                        {ValidationHelper.getValidationResults(p, r).map((v, vi) => <li className="list-group-item" key={"Entity_" + ei + "_" + p + "_" + vi}>{v}</li>)}
                                    </ul>
                            </li>
                            )}
                        </ul>
                </li>)           
                }
            </ul>
        }
        else {
            return <div></div>
        }
    }
}
