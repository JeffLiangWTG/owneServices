import React from "react";
//@ts-ignore
import $ from "jquery";
import { IValidationResults } from "./ValidationResults";
import { ValidationResultsForm } from "./ValidationResultsForm";

interface IPopupFormProps {
    title : string,
    message : string,
    validationResults? : IValidationResults[]
    handleHideModal : () => void
}

export class PopupForm extends React.Component<IPopupFormProps, any> {
    constructor(props : IPopupFormProps) {
        super(props);
    }

    private modal : HTMLElement | null = null;

    componentDidMount() {
        ($(this.modal!) as any).modal("show");
        ($(this.modal!) as any).on("hidden.bs.modal", this.props.handleHideModal);
    }

    render() {
        return <div ref={c => this.modal = c} className="modal fade" tabIndex={-1} role="dialog" aria-labelledby="exampleModalLongTitle" aria-hidden="true">
        <div className="modal-dialog" role="document">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title" >{this.props.title}</h5>
              <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>
            <div className="modal-body">
              {this.props.message}
              <ValidationResultsForm validationResults={this.props.validationResults} />
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" data-dismiss="modal">Close</button>
            </div>
          </div>
        </div>
      </div>
    }
}