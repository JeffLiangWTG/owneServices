import { HttpOData } from "ts-odatajs";
import { Mock } from "typemoq";
import MsalWrapper from "../MsalWrapper";
import { OData } from "../OData";

jest.mock('ts-odatajs', () => ({
    oData: {
        read: jest.fn()
    }
}));

describe("OData", () => {
	it("setAuthenticationHeader", async () => {
		MsalWrapper.getInstance = jest.fn().mockReturnValue({
			getAccessToken: () => {
				return "abcdefg";
			},
		});
		let request = Mock.ofType<HttpOData.Request>();
		let headers: any = {};
		request.setup((x) => x.headers).returns(() => headers);
		await new OData().setAuthenticationHeader(request.object);
		expect(headers["Authorization"]).toEqual("Bearer abcdefg");
	});

	it("rejects when read failed", async () => {
		const { oData } = require("ts-odatajs");
		let odataInstance = new OData();
		let errorResponse = { message: "403: Error Reading data", statusCode: "403", requestUri: "http://example.com/data" , statusText: "Forbidden" };
		oData.read.mockImplementation(
			(
				request: any,
				successCallback: () => void,
				errorCallback: (err: HttpOData.Response) => void
			) => {
				errorCallback({
					response: errorResponse,
					requestUri: "http://example.com/data",
					statusCode: "403",
					statusText: "Forbidden"
				});
			}
		);

		await expect(odataInstance.read("http://example.com/data")).rejects.toEqual(
			errorResponse
		);
	});
});

describe("OData BroadcastChannel", () => {
	beforeEach(() => {
		jest.clearAllMocks();
	});

	it("should broadcast clear_cache when clearCache is called", async () => {
		const odata = new OData();
		odata.cache["foo"] = { value: [1, 2, 3], expiredAt: Date.now() + 1000 };
		await odata.clearCache();
		expect(odata.cache).toEqual({});
		expect(odata.broadcastChannel.postMessage).toHaveBeenCalledWith("clear_cache");
	});

	it("should clear local cache when receiving clear_cache broadcast", async () => {
		const odata = new OData();
		odata.cache["foo"] = { value: [1, 2, 3], expiredAt: Date.now() + 1000 };
		odata.broadcastChannel.onmessage &&
		odata.broadcastChannel.onmessage({ data: "clear_cache" } as any);
		expect(odata.cache).toEqual({});
	});

	it("should not clear cache for unrelated broadcast messages", async () => {
		const odata = new OData();
		odata.cache["foo"] = { value: [1, 2, 3], expiredAt: Date.now() + 1000 };
		odata.broadcastChannel.onmessage &&
		odata.broadcastChannel.onmessage({ data: "something_else" } as any);
		expect(odata.cache).not.toEqual({});
	});
});
