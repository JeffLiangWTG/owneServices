export interface IClientDataSetVersionSummary {
	PK: string;
	DataSet: string;
	LastDataChangedTime: Date;
	NoOfCustomersUpdated: number;
	NoOfCustomersFailed: number;
	NoOfProductionCustomersFailed: number;
}

export class ClientDataSetVersionSummary
	implements IClientDataSetVersionSummary
{
	constructor(
		PK: string,
		DataSet: string,
		LastDataChangedTime: Date,
		NoOfCustomersUpdated: number,
		NoOfCustomersFailed: number,
		NoOfProductionCustomersFailed: number
	) {
		this.PK = PK;
		this.DataSet = DataSet;
		this.LastDataChangedTime = LastDataChangedTime;
		this.NoOfCustomersUpdated = NoOfCustomersUpdated;
		this.NoOfCustomersFailed = NoOfCustomersFailed;
		this.NoOfProductionCustomersFailed = NoOfProductionCustomersFailed;
	}

	PK: string;
	DataSet: string;
	LastDataChangedTime: Date;
	NoOfCustomersUpdated: number;
	NoOfCustomersFailed: number;
	NoOfProductionCustomersFailed: number;
}
