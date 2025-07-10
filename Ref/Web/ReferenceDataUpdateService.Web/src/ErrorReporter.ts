import _, { forEach } from "underscore";
import axios from "axios";

export interface IErrorReporter {
	reportError: (errorEvent: ErrorEvent) => void;
	reportPromiseRejection: (rejectionEvent: PromiseRejectionEvent) => void;
}

declare var __SafeAPI__: string;

export class ErrorReporter implements IErrorReporter {
	window: Window;
	safeApi: string;
	errorMap: Map<string, number>;

	constructor(localWindow: Window) {
		this.window = localWindow;
		this.errorMap = new Map<string, number>();
		this.safeApi = __SafeAPI__.replace("odata", "api") + "ErrorReport/ReportError";
	}

	static readonly axiosConfig = {
		headers: { "Content-Type": "application/json" }
	};

	init() {
		this.window.addEventListener("error", async (e) => {
			await this.reportError(e);
		});
		this.window.addEventListener("unhandledrejection", async (e) => {
			await this.reportPromiseRejection(e);
		});
	}

	async reportError(errorEvent: ErrorEvent) {
		let error = errorEvent.error;
		if (this.shouldReportError(error)) {
			await axios.post(this.safeApi, { Message: error.message, StackTrace: error.stack, ErrorEventJsonString: JSON.stringify(error) }, ErrorReporter.axiosConfig);
		}
	}

	async reportPromiseRejection(rejectionEvent: PromiseRejectionEvent) {
		let error = rejectionEvent.reason;
		if (this.shouldReportError(error)) {
			await axios.post(this.safeApi, { Message: error.message, StackTrace: error.stack, ErrorEventJsonString: JSON.stringify(error) }, ErrorReporter.axiosConfig);
		}
	}

	private shouldReportError(error: any): boolean {
		let errorCount = this.errorMap.get(error.message);
		if (!errorCount) {
			errorCount = 0;
		}
		errorCount++;
		if (errorCount > 3) {
			return false;
		}
		this.errorMap.set(error.message, errorCount);
		return true;
	}
}
