import React, { Component } from "react";
import moment from "moment";
import { IEntityManager, ServiceType } from "./EntityManager";
import _ from "underscore";
import { Filter, FilterOps, IFilter } from "./Filter";
import { PopupForm } from "./PopupForm";
import { IParentProps } from "./IParentProps";
import { RefCusCodeListUserView } from "./models/RefCusCodeListUserView";
import { RefCusCodeListAttributeUserView } from "./models/RefCusCodeListAttributeUserView";
import { RefCusCodeListFRFallbackRevokeView } from "./models/RefCusCodeListFRFallbackRevokeView";
import { IRefCusCodeListFRFallbackFormProps } from "./RefCusCodeListFRFallbackInvokeForm";

const DeltaT = "DELTAT";
const DeltaX = "DELTAX";
const DeltaG = "DELTAG";
const Gamma = "GAMMA";
const Ics = "ICS";
const Ecs = "ECS";
const Revoke = "Revoke";
const CommentInvoke = "CommentInvoke";
const CommentRevoke = "CommentRevoke";

interface IRefCusCodeListFRFallbackHistoricFormState extends IParentProps {
	view: RefCusCodeListFRFallbackRevokeView[],
	message: string
}

export class RefCusCodeListFRFallbackHistoryForm extends React.Component<IRefCusCodeListFRFallbackFormProps, IRefCusCodeListFRFallbackHistoricFormState>
{
	constructor(props: IRefCusCodeListFRFallbackFormProps) {
		super(props);
		this.state = {
			view: [],
			message: ""
		};
	}

	async componentDidMount() {
		await this.loadListRefCusCode();
	}

	async loadListRefCusCode() {
		let filters = [
			new Filter("ZZD_CodeType", FilterOps.Equals, "FBK" as any, "string"),
			new Filter("ZZD_CountryOrGrouping", FilterOps.Equals, "FR" as any, "string"),
			new Filter("ZZD_Description", FilterOps.Contains, Revoke as any, "string")
		];
		let RefCusCodeListRows = await this.props.entityManager.getAsync<RefCusCodeListUserView>("RefCusCodeListUserView", [ServiceType.Safe], filters, false);

		if (RefCusCodeListRows) {
			let entities: RefCusCodeListUserView[] = RefCusCodeListRows.filter(a =>
				a.ZZD_Code.includes(DeltaT)
				|| a.ZZD_Code.includes(DeltaX)
				|| a.ZZD_Code.includes(DeltaG)
				|| a.ZZD_Code.includes(Gamma)
				|| a.ZZD_Code.includes(Ics)
				|| a.ZZD_Code.includes(Ecs)
			);
			let rowsView: RefCusCodeListFRFallbackRevokeView[] = [];
			await Promise.all(entities.map(async (row) => {
				let applicationValue = "";
				if (row.ZZD_Code) {
					applicationValue = row.ZZD_Code.substring(row.ZZD_Code.indexOf(",") + 1);
				}

				let filters = [new Filter("ZZE_ZZD_CodeList", FilterOps.Equals, row.ZZD_PK as any, "guid"),];
				let RefCusCodeAttributeListRows = await this.props.entityManager.getAsync<RefCusCodeListAttributeUserView>("RefCusCodeListAttributeUserView", [ServiceType.Safe], filters, false);
				if (RefCusCodeAttributeListRows.length > 0) {
					let commentInvokeAttr: RefCusCodeListAttributeUserView = RefCusCodeAttributeListRows.filter(a => a.ZZE_ZXE_NKName == CommentInvoke)[0];
					let commentRevokeAttr: RefCusCodeListAttributeUserView = RefCusCodeAttributeListRows.filter(a => a.ZZE_ZXE_NKName == CommentRevoke)[0];
					let descriptionValues: string[] = row.ZZD_Description.split(';');
					let user: string = "";
					if (descriptionValues.length == 2) {
						user = descriptionValues[0] + "\n" + descriptionValues[1];
					}

					var viewItem = new RefCusCodeListFRFallbackRevokeView(row.ZZD_PK, row.ZZD_StartDate, row.ZZD_EndDate
						, commentInvokeAttr.ZZE_Value, commentRevokeAttr.ZZE_Value, applicationValue
						, user, row.ZZD_Code
						, commentRevokeAttr.ZZE_PK
						, row
					);
					rowsView.push(viewItem);
				}
			}));

			rowsView.sort((a, b) => Date.parse(a.ZZD_StartDate) - Date.parse(b.ZZD_StartDate));
			this.setState({ view: rowsView });
		}
	}

	formatDate(input: string) {
		return moment.utc(input).format("YYYY-MM-DD HH:mm");
	}

	render() {
		return (<div>
			<table className="table table-striped">
				<thead>
					<tr>
						<th scope="col">ID</th>
						<th scope="col">Application</th>
						<th scope="col">Start Date</th>
						<th scope="col">End Date</th>
						<th scope="col">Invoke Comment</th>
						<th scope="col">Revoke Comment</th>
						<th scope="col">User</th>
					</tr>
				</thead>
				<tbody>
					{this.state.view.map((r, idx) => {
						return <tr key={idx}>
							<td>{idx + 1}</td>
							<td>{r.ZZD_Application}</td>
							<td>{this.formatDate(r.ZZD_StartDate)}</td>
							<td>{this.formatDate(r.ZZD_EndDate)}</td>
							<td>{r.ZZD_InvokeComments}</td>
							<td>{r.ZZD_RevokeComments}</td>
							<td>{r.ZZD_User}</td>
						</tr>
					})}
				</tbody>
			</table>
			<div>
				{this.state.message.length > 0 ? <PopupForm title="Information" message={this.state.message} handleHideModal={() => this.setState({ message: "" })} /> : null}
			</div>
		</div>);
	}
}
