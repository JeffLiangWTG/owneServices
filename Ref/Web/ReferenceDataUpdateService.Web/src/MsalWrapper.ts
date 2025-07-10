import {
	Configuration,
	EventType,
	IPublicClientApplication,
	InteractionRequiredAuthError,
	PublicClientApplication,
	RedirectRequest,
} from "@azure/msal-browser";
import { useIsAuthenticated } from "@azure/msal-react";
import { loginRequest, msalConfig } from "./AuthConfig";

class MsalWrapper {
	private static instance: MsalWrapper;
	private msalInstance: IPublicClientApplication;
	private taskQueue: (() => Promise<void>)[] = [];
	private isProcessingQueue: boolean = false;
	private msalInitialize: Promise<void>;

	/**
	 * MSAL should be instantiated outside of the component tree to prevent it from being re-instantiated on re-renders.
	 * For more, visit: https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-react/docs/getting-started.md
	 */
	private constructor(config: Configuration) {
		this.msalInstance = new PublicClientApplication(config);
		if (
			!this.msalInstance.getActiveAccount() &&
			this.msalInstance.getAllAccounts().length > 0
		) {
			this.msalInstance.setActiveAccount(this.msalInstance.getAllAccounts()[0]);
		}

		this.msalInstance.addEventCallback((event: any) => {
			if (
				(event.eventType === EventType.LOGIN_SUCCESS ||
					event.eventType === EventType.ACQUIRE_TOKEN_SUCCESS ||
					event.eventType === EventType.SSO_SILENT_SUCCESS) &&
				event.payload.account
			) {
				this.msalInstance.setActiveAccount(event.payload.account);
			}
		});
		//https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/errors.md#uninitialized_public_client_application
		this.msalInitialize = this.msalInstance.initialize();
	}

	public static getInstance(): MsalWrapper {
		if (!MsalWrapper.instance) {
			MsalWrapper.instance = new MsalWrapper(msalConfig);
		}
		return MsalWrapper.instance;
	}

	public getMsalInstance(): IPublicClientApplication {
		return this.msalInstance;
	}

	/**
	 * We use a TaskQueue to manage concurrency and prevent conflicting errors that may arise from simultaneously calling the MSALinstance.
	 */
	private async processQueue() {
		if (this.isProcessingQueue) return;
		this.isProcessingQueue = true;

		try {
			while (this.taskQueue.length > 0) {
				const task = this.taskQueue.shift();
				if (task) {
					await task();
				}
			}
		} finally {
			this.isProcessingQueue = false;
		}
	}

	private addToQueue(task: () => Promise<void>) {
		this.taskQueue.push(task);
		this.processQueue();
	}

	private async ensureInitialized() {
		await this.msalInitialize;
	}

	public async loginRedirect(request?: RedirectRequest): Promise<void> {
		this.addToQueue(async () => {
			try {
				await this.ensureInitialized();
				return await this.msalInstance.loginRedirect(request);
			} catch (error) {
				console.log(error);
			}
		});
	}

	public async logoutRedirect(): Promise<void> {
		this.addToQueue(async () => {
			await this.ensureInitialized();
			this.msalInstance.setActiveAccount(null);
			await this.msalInstance.clearCache();
			window.location.href = msalConfig.auth.postLogoutRedirectUri;
		});
	}

	public isAuthenticated(): boolean {
		return useIsAuthenticated();
	}

	public async getAccessToken(): Promise<string> {
		try {
			await this.ensureInitialized();
			const response = await this.msalInstance.acquireTokenSilent(loginRequest);
			if (response && response.accessToken) {
				return response.accessToken;
			}
		} catch (error) {
			if (error instanceof InteractionRequiredAuthError) {
				this.addToQueue(async () => {
					alert("Your token is expired, redirect to sign in");
					await this.msalInstance.acquireTokenRedirect(loginRequest);
				});
			}
		}
		return "";
	}

	public getUserName(): string {
		let activeAccount = this.msalInstance.getActiveAccount();
		let idTokenClaims = activeAccount?.idTokenClaims as { [key: string]: any };
		return idTokenClaims?.user_name ?? "";
	}

	public getUniqueName(): string {
		let activeAccount = this.msalInstance.getActiveAccount();
		let idTokenClaims = activeAccount?.idTokenClaims as { [key: string]: any };
		return idTokenClaims?.unique_name ?? "";
	}
}

export default MsalWrapper;
