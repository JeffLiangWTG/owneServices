import { BrowserCacheLocation } from "@azure/msal-browser";
import { msalConfig } from "../AuthConfig";

describe("AuthConfig non-environment specific configurations", () => {
	it("Keep user logged in between tabs", () => {
        expect(msalConfig.cache.cacheLocation).toBe(BrowserCacheLocation.LocalStorage);
    });

	it("Do not redirects user back to the page he originally logged in", () => {
        expect(msalConfig.auth.navigateToLoginRequestUrl).toBe(false);
    });
});
