import React, { Component } from "react";
import moment from "moment";
import { IEntityManager, ServiceType } from "./EntityManager";
import _ from "underscore";
import { Filter, FilterOps, IFilter } from "./Filter";
import update from "immutability-helper";
import { TextInput } from "./TextInput";
import { DateTimeInput } from "./DateTimeInput";
import { ValidationServiceWrapper } from "./ValidationService";
import { IValidationResults, ValidationResults } from "./ValidationResults";
import { ValidationHelper } from "./ValidationHelper";
import { ValidationServiceHelper } from "./ValidationServiceHelper";
import { PopupForm } from "./PopupForm";
import { IParentProps } from "./IParentProps";
import { RefCusCodeListUserView } from "./models/RefCusCodeListUserView";
import { RefCusCodeListAttributeUserView } from "./models/RefCusCodeListAttributeUserView";
import { RefCusCodeListFRFallbackRevokeView } from "./models/RefCusCodeListFRFallbackRevokeView";
import { IRefCusCodeListFRFallbackFormProps } from "./RefCusCodeListFRFallbackInvokeForm";
import MsalWrapper from "./MsalWrapper";
import Button from "./Button";

const DeltaT = "DELTAT";
const DeltaX = "DELTAX";
const DeltaG = "DELTAG";
const Gamma = "GAMMA";
const Ics = "ICS";
const Ecs = "ECS";

const Revoke = "Revoke";
const CommentInvoke = "CommentInvoke";
const CommentRevoke = "CommentRevoke";
const EndDate = "2079-06-06T23:59:00Z";

interface IRefCusCodeListFRFallbackRevokeFormState extends IParentProps {
	views: RefCusCodeListFRFallbackRevokeView[],
	viewValidationResults: {
		[pk: string]: IValidationResults
	},
	maxStartDate: string,
	revokingAllDate: string,
	revokingAllDateValidationResults: IValidationResults,
	message: string,
	saveButtonDisabled: boolean
}

export class RefCusCodeListFRFallbackRevokeForm extends React.Component<IRefCusCodeListFRFallbackFormProps, IRefCusCodeListFRFallbackRevokeFormState> {
	constructor(props: IRefCusCodeListFRFallbackFormProps) {
		super(props);
		this.formatDate = this.formatDate.bind(this);
		this.getCurrentDate = this.getCurrentDate.bind(this);
		this.onValueChange = this.onValueChange.bind(this);
		this.onValueChanged = this.onValueChanged.bind(this);
		this.editDataRow = this.editDataRow.bind(this);
		this.updateDataRow = this.updateDataRow.bind(this);
		this.validateDataRow = this.validateDataRow.bind(this);
		this.revokeDataRow = this.revokeDataRow.bind(this);
		this.cancelDataRow = this.cancelDataRow.bind(this);
		this.saveDataRow = this.saveDataRow.bind(this);
		this.saveAllDataRows = this.saveAllDataRows.bind(this);
		this.updateSaveButtonDisabledProperty = this.updateSaveButtonDisabledProperty.bind(this);

		let currentDate = this.getCurrentDate();
		this.state = {
			views: [],
			viewValidationResults: {},
			maxStartDate: currentDate,
			revokingAllDate: currentDate,
			revokingAllDateValidationResults: {},
			message: "",
			saveButtonDisabled: false
		};

		this.validationServices = new ValidationServiceWrapper([ValidationServiceHelper.getRefCusCodeFallbackRevokeValidationService(this.props.entityManager),ValidationServiceHelper.getRefCusCodeFallbackRevokeAllValidationService(this.props.entityManager, currentDate)], this.updateSaveButtonDisabledProperty);
	}

	validationServices: ValidationServiceWrapper;

	updateSaveButtonDisabledProperty(isValidating: boolean)
	{
		this.setState({ saveButtonDisabled: isValidating});
	}

	async componentDidMount() {
		await this.loadListRefCusCode();
	}

	async loadListRefCusCode() {
		let filters = [
			new Filter("ZZD_CodeType", FilterOps.Equals, "FBK" as any, "string"),
			new Filter("ZZD_CountryOrGrouping", FilterOps.Equals, "FR" as any, "string"),
			new Filter("ZZD_EndDate", FilterOps.Equals, EndDate as any, "datetime")
		];
		let RefCusCodeListRows = await this.props.entityManager.getAsync<RefCusCodeListUserView>("RefCusCodeListUserView", [ServiceType.Safe], filters, false);

		if (RefCusCodeListRows) {
			let maxDate: string = this.state.revokingAllDate;
			let entities: RefCusCodeListUserView[] = RefCusCodeListRows.filter(a =>
				a.ZZD_Code.includes(DeltaT)
				|| a.ZZD_Code.includes(DeltaX)
				|| a.ZZD_Code.includes(DeltaG)
				|| a.ZZD_Code.includes(Gamma)
				|| a.ZZD_Code.includes(Ics)
				|| a.ZZD_Code.includes(Ecs));
			let rowsView: RefCusCodeListFRFallbackRevokeView[] = [];
			await Promise.all(entities.map(async (row) => {
				let applicationValue = "";
				if (row.ZZD_Code) {
					applicationValue = row.ZZD_Code.substring(row.ZZD_Code.indexOf(",") + 1);
				}

				let filters = [new Filter("ZZE_ZZD_CodeList", FilterOps.Equals, row.ZZD_PK as any, "guid")];
				let RefCusCodeAttributeListRows = await this.props.entityManager.getAsync<RefCusCodeListAttributeUserView>("RefCusCodeListAttributeUserView", [ServiceType.Safe], filters, false)

				if (RefCusCodeAttributeListRows.length > 0) {
					let commentInvokeAttr: RefCusCodeListAttributeUserView = RefCusCodeAttributeListRows.filter(a => a.ZZE_ZXE_NKName == CommentInvoke)[0];
					let commentRevokeAttr: RefCusCodeListAttributeUserView = RefCusCodeAttributeListRows.filter(a => a.ZZE_ZXE_NKName == CommentRevoke)[0];
					var viewItem = new RefCusCodeListFRFallbackRevokeView(row.ZZD_PK, row.ZZD_StartDate, row.ZZD_EndDate
						, commentInvokeAttr.ZZE_Value, commentInvokeAttr.ZZE_Value, applicationValue
						, row.ZZD_Description, row.ZZD_Code
						, commentRevokeAttr.ZZE_PK
						, row
					);
					rowsView.push(viewItem);
					if (new Date(row.ZZD_StartDate) > new Date(maxDate)) {
						maxDate = row.ZZD_StartDate;
					}
				}
			}));

			this.setState({ views: rowsView, maxStartDate: maxDate });
		}
	}

	formatDate(input: string) {
		return moment.utc(input).format("YYYY-MM-DD HH:mm");
	}

	getCurrentDate(): string {
		let time = moment(new Date()).format("YYYY-MM-DDTHH:mm:00") + "Z";
		return new Date(time).toISOString();
	}

	async onValueChange(entity: any, name: string, value: object): Promise<void> {
		if (name == "revokingAllDate") {
			let dateRevoking = String(value);
			let revokingUpdated = update(this.state, { [name]: { $set: dateRevoking } });
			this.setState({ revokingAllDate: dateRevoking });
			await Promise.all(this.state.views.map((a, index) => this.updateDataRow(index, a, name, value)));
		}
		else {
			let index = this.state.views.findIndex(x => x.ZZD_PK == entity.ZZD_PK);
			if (index >= 0) {
				await this.updateDataRow(index, entity, name, value);
			}
		}
	}

	async onValueChanged(entity: any, name: string): Promise<void> {
		if (name == "revokingAllDate") {
			let validaitionResults = update(this.state.revokingAllDateValidationResults
				, { [name]: { $set: await ValidationHelper.validateProperty(this.validationServices, this.state, name, 1) } });
			this.setState({ revokingAllDateValidationResults: validaitionResults });
			await Promise.all(this.state.views.map((a, index) => this.validateDataRow(index, a, name)));
		}
		else {
			let index = this.state.views.findIndex(x => x.ZZD_PK == entity.ZZD_PK);
			if (index >= 0) {
				await this.validateDataRow(index, entity, name);
			}
		}
	}

	async editDataRow(entity: RefCusCodeListFRFallbackRevokeView) {
		let index = this.state.views.findIndex(x => x.ZZD_PK == entity.ZZD_PK);
		if (index >= 0) {
			{
				entity.ZZD_Edit = true;
				entity.ZZD_EndDate = this.getCurrentDate();
				this.setState({
					views: update(this.state.views, { $splice: [[index, 1, entity]] })
				 });
			}
		}
	}

	async updateDataRow(index: any, entity: any, name: string, value: any) {
		if (index >= 0) {
			let updateRowView = update(this.state.views[index], { [name]: { $set: value } });
			this.setState({
				views: update(this.state.views, { $splice: [[index, 1, updateRowView]] }),
			});
		}
	}

	async validateDataRow(index: any, entity: any, name: string) {
		if (index >= 0) {
			let updateRowView = this.state.views[index];
			let validationResult = update(this.state.viewValidationResults[entity.ZZD_PK] || {}
				, { [name]: { $set: await ValidationHelper.validateProperty(this.validationServices, updateRowView, name) } });
			let validationResults = update(this.state.viewValidationResults, { [entity.ZZD_PK]: { $set: validationResult } });
			this.setState({
				viewValidationResults: validationResults,
			});
		}
	}

	async revokeDataRow(entity: RefCusCodeListFRFallbackRevokeView, all: boolean) {
		let index = this.state.views.findIndex(x => x.ZZD_PK == entity.ZZD_PK);
		if (index >= 0) {
			let revokeEndDate = entity.ZZD_EndDate;
			if (all) {
				revokeEndDate = this.state.revokingAllDate;
			}

			let code: RefCusCodeListUserView = {
				ZZD_PK: entity.ZZD_PK,
				ZZD_Code: entity.ZZD_Code,
				ZZD_CodeType: "FBK",
				ZZD_CountryOrGrouping: "FR",
				ZZD_Description: entity.ZZD_User + ";" + Revoke + "=" + MsalWrapper.getInstance().getUniqueName(),
				ZZD_StartDate: entity.ZZD_StartDate,
				ZZD_EndDate: revokeEndDate,
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
			this.props.entityManager.update(code);

			let revokeAttrArray: RefCusCodeListFRFallbackRevokeView[] = this.state.views.filter(a => a.ZZD_PK == entity.ZZD_PK);
			let comment = "";
			if (entity.ZZD_RevokeComments) {
				comment = entity.ZZD_RevokeComments;
			}
			if (all) {
				comment = "Comment revoke all";
			}
			let idRevokeAttr: string = "";
			if (revokeAttrArray.length > 0) {
				idRevokeAttr = revokeAttrArray[0].ZZE_PK_RevokeComment;
			}
			let commentRevokeAttr: RefCusCodeListAttributeUserView = {
				ZZE_PK: idRevokeAttr,
				ZZE_ZZD_CodeList: entity.ZZD_PK,
				ZZE_ZXE_NKName: CommentRevoke,
				ZZE_Value: comment,
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
			this.props.entityManager.update(commentRevokeAttr);
		}
	}

	async cancelDataRow(entity: RefCusCodeListFRFallbackRevokeView) {
		let id = entity.ZZD_PK;
		let index = this.state.views.findIndex(x => x.ZZD_PK == id);
		if (index >= 0) {
			entity.ZZD_Edit = false;
			entity.ZZD_EndDate = EndDate;
			entity.ZZD_RevokeComments = entity.ZZD_InvokeComments;
			this.setState({
				views : update(this.state.views, { $splice: [[index, 1, entity]] })
			});
		}
	}

	async saveDataRow(entity: RefCusCodeListFRFallbackRevokeView) {
		let viewValidation: { [pk: string]: IValidationResults } = {};
		await Promise.all(this.state.views.map(a => ValidationHelper.validate(this.validationServices, a, 0, true)
			.then(r => viewValidation[a.ZZD_PK] = r)));
		this.setState({ viewValidationResults: viewValidation });
		if (!ValidationHelper.hasErrors(Object.getOwnPropertyNames(viewValidation).map(p => viewValidation[p]))) {
			await this.revokeDataRow(entity, false);
			let result = await this.props.entityManager.saveChanges(ServiceType.Safe);
			this.setState({ message: result.message });
			if (result.success) {
				await this.componentDidMount();
			}
			else {
				this.setState({ message: result.message });
			}
		}
		else {
			this.setState({ message: "Please fix all errors before saving." })
		}
	}

	async saveAllDataRows() {
		let revokeAllDateValidationResults = await ValidationHelper.validate(this.validationServices, this.state, 1, true);
		this.setState({ revokingAllDateValidationResults: revokeAllDateValidationResults });
		if (!ValidationHelper.hasErrors(this.getValidationResults())) {
			await Promise.all(this.state.views.map(async (row, index) => {
				await this.updateDataRow(index, row, "EndDate", this.state.revokingAllDate);
				await this.revokeDataRow(row, true);
			}));

			let result = await this.props.entityManager.saveChanges(ServiceType.Safe);
			this.setState({ message: result.message });
			if (result.success) {
				await this.componentDidMount();
			}
			else {
				this.setState({ message: result.message });
			}
		}
		else {
			this.setState({ message: "Please fix all errors before saving." })
		}
	}

	getValidationResults(): IValidationResults[] {
		return Object.getOwnPropertyNames(this.state.revokingAllDateValidationResults).map(p => this.state.revokingAllDateValidationResults);
	}

	render() {
		return (<div>
			<table className="table table-striped">
				<thead>
					<tr>
						<th scope="col">ID</th>
						<th scope="col">Start Date</th>
						<th scope="col">End Date</th>
						<th scope="col">Comment</th>
						<th scope="col">Application</th>
						<th scope="col"></th>
						<th scope="col"></th>
					</tr>
				</thead>
				<tbody>
					{this.state.views.map((r, idx) => {
						if (r.ZZD_Edit) {
							return <tr key={idx}>
								<td>{idx + 1}</td>
								<td>{this.formatDate(r.ZZD_StartDate)}</td>
								<td>
									<DateTimeInput entity={r} propertyName="ZZD_EndDate" onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} validationResult={this.state.viewValidationResults[r.ZZD_PK]} format="DD/MM/YYYY HH:mm:SS" />
								</td>
								<td>
									<TextInput entity={r} inputType="string" maxLength={255} propertyName="ZZD_RevokeComments" onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} validationResults={this.state.viewValidationResults[r.ZZD_PK]} />
								</td>
								<td>{r.ZZD_Application}</td>
								<td><Button type='button' className="btn btn-info btn-sm" onClick={() => this.cancelDataRow(r)} >Cancel</Button></td>
								<td><Button type='button' className="btn btn-info btn-sm" onClick={() => this.saveDataRow(r)} disabled={this.state.saveButtonDisabled} >Save</Button></td>
							</tr>
						}
						else {
							return <tr key={idx}>
								<td>{idx + 1}</td>
								<td>{this.formatDate(r.ZZD_StartDate)}</td>
								<td>{this.formatDate(r.ZZD_EndDate)}</td>
								<td>{r.ZZD_InvokeComments}</td>
								<td>{r.ZZD_Application}</td>
								<td><Button type='button' className="btn btn-info btn-sm" onClick={() => this.editDataRow(r)} >Edit</Button></td>
								<td><Button type='button' className="btn btn-info btn-sm" disabled={true} onClick={() => { }} >Save</Button></td>
							</tr>
						}
					})}
				</tbody>
			</table>
			<div className="form-group row" >
				<label className="col-sm-2 col-form-label">Revoking all</label>
				<div className="col-sm-3">
					<DateTimeInput entity={this.state} propertyName="revokingAllDate" onValueChange={this.onValueChange} onValueChanged={this.onValueChanged} validationResult={this.state.revokingAllDateValidationResults} format="DD/MM/YYYY HH:mm:SS" />
				</div>
				<div className="col-sm-1">
					<Button type='button' onClick={this.saveAllDataRows} disabled={this.state.views.length == 0 || this.state.saveButtonDisabled} >Revoke All</Button>
				</div>
			</div>
			<div>
				{this.state.message.length > 0 ? <PopupForm title="Information" message={this.state.message} handleHideModal={() => this.setState({ message: "" })} /> : null}
			</div>
		</div>);
	}
}
