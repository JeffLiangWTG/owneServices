import { oData, HttpOData, Batch } from "ts-odatajs";
//@ts-ignore
import MsalWrapper from "./MsalWrapper";

export interface IOData {
	read<T>(url: string): Promise<T[]>;
	batch(baseUrl: string, batchRequest: Batch.BatchRequest): Promise<HttpOData.Response>;
	clearCache(): void;
}

export interface IStorageData {
	value: any;
	expiredAt: number;
}

export interface IODataRequestCache {
	[url: string]: IStorageData,
}

export class OData implements IOData {
	cache: IODataRequestCache = {};
	expiredInSeconds: number = 60;
	clearCacheEventName: string = "clear_cache";
	broadcastChannel: BroadcastChannel;

	constructor() {
		this.broadcastChannel = new BroadcastChannel("odata_cache_channel");
		this.broadcastChannel.onmessage = (event) => {
			if (event.data === this.clearCacheEventName) {
				this.clearLocalCache();
			}
		};
	}

	async clearCache() {
		this.clearLocalCache();
		this.broadcastChannel.postMessage(this.clearCacheEventName);
	}

	async clearLocalCache() {
		this.cache = {};
	}

	async read<T>(url: string): Promise<T[]> {
		if (!this.cache[url] || this.cache[url].expiredAt < new Date().valueOf()) {
			let request: HttpOData.Request = {
				requestUri: url
			}
			await this.setAuthenticationHeader(request);
			let result = new Promise<T[]>((resolve,reject) => {
				oData.read(request, data => resolve(data.value), err => reject(err.response));
			});
			let cacheData: IStorageData = {
				value: result, expiredAt: new Date().valueOf() + this.expiredInSeconds * 1000
			};
			this.cache[url] = cacheData;
			return result;
		}
		else {
			return this.cache[url].value;
		}
	}

	async batch(baseUrl: string, batchRequest: Batch.BatchRequest): Promise<HttpOData.Response> {
		let request: HttpOData.Request = {
			requestUri: baseUrl + "$batch",
			data: batchRequest,
			method: "POST"
		}
		await this.setAuthenticationHeader(request);
		return new Promise<HttpOData.Response>(resolve => oData.request(request, (data, res) => resolve(res), err => resolve(<HttpOData.Response>err.response), oData.batch.batchHandler));
	}

	async setAuthenticationHeader(request: HttpOData.Request): Promise<void> {
		let token: string = await MsalWrapper.getInstance().getAccessToken();
		if (token) {
			if (!request.headers) {
				request.headers = {};
			}
			request.headers["Authorization"] = "Bearer " + token;
		}
	}
}
