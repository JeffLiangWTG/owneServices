import { BrowserCacheLocation, LogLevel } from "@azure/msal-browser";

declare var __Authority__: string;
declare var __AuthorityDomain__: string;
declare var __ClientId__: string;
declare var __RedirectUri__: string;
declare var __PostLogoutRedirectUri__: string;

/**
 * Configuration object to be passed to MSAL instance on creation.
 * For a full list of MSAL.js configuration parameters, visit:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/configuration.md
 *
 * Also please refer to: https://github.com/Azure-Samples/ms-identity-javascript-react-tutorial/blob/main/1-Authentication/2-sign-in-b2c/SPA/src/authConfig.js
 */
export const msalConfig = {
	auth: {
		clientId: __ClientId__,
		authority: __Authority__,
		knownAuthorities: [__AuthorityDomain__],
		redirectUri: __RedirectUri__,
		postLogoutRedirectUri: __PostLogoutRedirectUri__,
		navigateToLoginRequestUrl: false
	},
	cache: {
		cacheLocation: BrowserCacheLocation.LocalStorage,
		storeAuthStateInCookie: false
	},
	system: {
		loggerOptions: {
			loggerCallback: (level: LogLevel, message: string, containsPii: any) => {
				if (containsPii) {
					return;
				}
				switch (level) {
					case LogLevel.Error:
						console.error(message);
						return;
					case LogLevel.Info:
						console.info(message);
						return;
					case LogLevel.Verbose:
						console.debug(message);
						return;
					case LogLevel.Warning:
						console.warn(message);
						return;
					default:
						return;
				}
			},
		},
	},
};

export const loginRequest = {
	scopes: ["openid", __ClientId__],
	domainHint: "Azure"
};
