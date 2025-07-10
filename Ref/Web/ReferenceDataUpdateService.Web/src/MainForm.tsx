import { Component } from "react";
import React from "react";
import { IDataSetChangeHistory } from "./models/IDataSetChangeHistory";
import { IEntityManager, ServiceType } from "./EntityManager";
import { Filter, FilterOps } from "./Filter";

interface IMainFormProps {
    render: (onSaved: () => Promise<void>, systemVersion? : string) => React.ReactNode;
    id? : string;
    parentCode: string;
    entityManager : IEntityManager;
}

interface IMainFormState {
    showHistory : boolean;
    changeHistory: IDataSetChangeHistory[];
    systemVersion? : string;
}

export class MainForm extends Component<IMainFormProps, IMainFormState> {
    constructor(props: IMainFormProps) {
        super(props);

        this.reloadChangeHistories = this.reloadChangeHistories.bind(this);

        this.state = {
            showHistory : false,
            changeHistory: [],
        }        
    }

    async componentDidMount() {
        await this.reloadChangeHistories();
    }

    async reloadChangeHistories() : Promise<void>
    {
        if (this.props.id) {
            let filters = [
                new Filter("DCH_ParentPK", FilterOps.Equals, this.props.id as any, "guid"),
                new Filter("DCH_ParentCode", FilterOps.Equals, this.props.parentCode as any, "string")
            ];
            let changeHistory = await this.props.entityManager.getAsync<IDataSetChangeHistory>("DataSetChangeHistory", [ServiceType.Safe], filters, false);
            this.setState({changeHistory : []});
            this.setState({
                changeHistory : changeHistory.sort((a, b) => a.DCH_ChangeTime < b.DCH_ChangeTime ? 1 : -1)
            })
        }
    }

    render() {
        return <div className="row">
            <nav className={this.state.showHistory ? "col-md-3" : "d-none"}>
                <div className="list-group">
                    {this.state.changeHistory.map((x, i) => <a key={x.DCH_PK} className="list-group-item list-group-item-action" data-toggle="list" href="#" onClick={_ => { this.setState({ systemVersion : i == 0 ? undefined : x.DCH_ChangeTime}); }}><small>{new Date(x.DCH_ChangeTime).toLocaleString()}</small></a>)}
                </div>
            </nav>
            <main className={this.state.showHistory ? "col-md-9" : "col-md-12"}>
                <button className="btn btn-info" type="button" hidden={this.props.id ? false : true} onClick={_ => this.setState( {showHistory : !this.state.showHistory} )}>History</button>
                {this.props.render(this.reloadChangeHistories, this.state.systemVersion)}  
            </main>
        </div>
    }
}