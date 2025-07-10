import { IComboBoxOptions } from "./ComboBox";
import codeList from "./producerStatusCodeList.json";

export class ProducerStatus implements IComboBoxOptions {
	statusFlags: StatusFlag[];
	flagNames: string[];

	constructor() {
		this.statusFlags = codeList;
		this.flagNames = this.statusFlags.map(x => x.flagName);
	}

	getFlagValue(name: string): number {
		if (this.flagNames.indexOf(name) > -1) {
			return this.statusFlags.filter(x => x.flagName == name)[0].flagValue;
		} else {
			return 0;
		}
	}
}

export class StatusFlag {
	flagName: string;
	flagValue: number;

	constructor(name: string, value: number) {
		this.flagName = name;
		this.flagValue = value;
	}
}
