import { IValidationResults } from "./ValidationResults";
import React from "react";
import { ValidationHelper } from "./ValidationHelper";

interface IValidationStripProps {
	validationResults?: IValidationResults;
	propertyName: string;
}

export class ValidationStrip extends React.Component<IValidationStripProps> {
	constructor(props: IValidationStripProps) {
		super(props);
	}

	render() {
		return (
			<div className="ml-3">
				{ValidationHelper.getValidationResults(
					this.props.propertyName,
					this.props.validationResults
				).map((r, i) => (
					<small key={i} className="text-danger row">
						{r}
					</small>
				))}
			</div>
		);
	}
}
