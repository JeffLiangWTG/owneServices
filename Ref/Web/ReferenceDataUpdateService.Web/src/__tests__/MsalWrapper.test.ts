import {
	AuthenticationResult,
	AuthenticationScheme,
} from "@azure/msal-browser";
import MsalWrapper from "../MsalWrapper";

describe("MsalWrapper", () => {
	let msalInstance: any;
	let initializeMock: jest.Mock;
	beforeAll(() => {
		const mockCrypto = {
			getRandomValues: () => [],
			subtle: {},
		} as unknown as Crypto;
		jest.spyOn(window, "crypto", "get").mockReturnValue(mockCrypto);
	});

	beforeEach(() => {
		msalInstance = MsalWrapper.getInstance().getMsalInstance();
		initializeMock = jest.fn().mockResolvedValue(undefined);
		msalInstance.initialize = initializeMock;
	});

	it("acquireTokenSilent", async () => {
		const authenticationResult: AuthenticationResult = {
			authority: "authority",
			uniqueId: "uniqueId",
			tenantId: "tenantId",
			scopes: "scopes".split(" "),
			account: {
				homeAccountId: "homeAccountId",
				environment: "environment",
				tenantId: "tenantId",
				username: "username",
				localAccountId: "localAccountId",
			},
			idToken: "id_token",
			accessToken: "access_token",
			idTokenClaims: {},
			fromCache: true,
			expiresOn: null,
			tokenType: AuthenticationScheme.BEARER,
			correlationId: "correlationId",
			extExpiresOn: undefined,
			state: undefined,
		};
		msalInstance.acquireTokenSilent = jest
			.fn()
			.mockReturnValue(authenticationResult);
		(MsalWrapper.getInstance() as any).msalInitPromise = initializeMock();
		const token = await MsalWrapper.getInstance().getAccessToken();
		expect(msalInstance.initialize).toHaveBeenCalled();
		expect(token).toEqual("access_token");
	});

	it("loginRedirect", async () => {
		msalInstance.loginRedirect = jest.fn();
		await MsalWrapper.getInstance().loginRedirect();
		await new Promise((resolve) => setTimeout(resolve, 0));
		expect(msalInstance.loginRedirect).toHaveBeenCalled();
	});

	it("logoutRedirect", async () => {
		msalInstance.setActiveAccount = jest.fn();
		msalInstance.clearCache = jest.fn();
		(MsalWrapper.getInstance() as any).msalInitPromise = initializeMock();
		await MsalWrapper.getInstance().logoutRedirect();
		await new Promise((resolve) => setTimeout(resolve, 0));
		expect(msalInstance.initialize).toHaveBeenCalled();
		expect(msalInstance.setActiveAccount).toHaveBeenCalledWith(null);
		expect(msalInstance.clearCache).toHaveBeenCalled();
	});

	it("getUserName", async () => {
		msalInstance.getActiveAccount = jest
			.fn()
			.mockReturnValue({ idTokenClaims: { user_name: "test user_name" } });
		const userName = MsalWrapper.getInstance().getUserName();
		expect(userName).toBe("test user_name");

		msalInstance.getActiveAccount = jest.fn();
		const emptyUserName = MsalWrapper.getInstance().getUserName();
		expect(emptyUserName).toBe("");
	});

	it("getUniqueName", async () => {
		msalInstance.getActiveAccount = jest
			.fn()
			.mockReturnValue({ idTokenClaims: { unique_name: "test unique_name" } });
		const userName = MsalWrapper.getInstance().getUniqueName();
		expect(userName).toBe("test unique_name");

		msalInstance.getActiveAccount = jest.fn();
		const emptyUniqueName = MsalWrapper.getInstance().getUniqueName();
		expect(emptyUniqueName).toBe("");
	});

	it("should return empty string when acquireTokenSilent fails", async () => {
		msalInstance.acquireTokenSilent = jest
			.fn()
			.mockRejectedValue(new Error("test error"));
		(MsalWrapper.getInstance() as any).msalInitPromise = initializeMock();
		const token = await MsalWrapper.getInstance().getAccessToken();
		expect(token).toBe("");
	});
});
