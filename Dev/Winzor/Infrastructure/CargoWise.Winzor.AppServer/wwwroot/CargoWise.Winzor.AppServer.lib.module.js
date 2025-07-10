export function beforeServerStart(options) {
	window.restartOnDisconnect = false;
	window.enableRestartOnDisconnect = () => {
		window.restartOnDisconnect = true;
	};
	options.reconnectionHandler = {
		onConnectionDown: (options, error) => {
			if (window.cargoWiseClient && typeof window.cargoWiseClient.reportConnectionDown === 'function') {
				window.cargoWiseClient.reportConnectionDown();
			}

			if (window.restartOnDisconnect) {
				window.restart();
			} else {
				Blazor.defaultReconnectionHandler.onConnectionDown.bind(Blazor.defaultReconnectionHandler)(options, error);
			}
		},
		onConnectionUp: (options, error) => {
			if (window.cargoWiseClient && typeof window.cargoWiseClient.reportConnectionUp == 'function') {
				window.cargoWiseClient.reportConnectionUp();
			}

			Blazor.defaultReconnectionHandler.onConnectionUp.bind(Blazor.defaultReconnectionHandler)(options, error);
		}
	};
	options.reconnectionOptions = {
		...options.reconnectionOptions,
		...window.winzorReconnectionOptions ?? {}
	};

	window.restart = () => {
		var launchHandler = localStorage.CargoWiseClientLaunchProtocol ?? "cargowiseclient";
		var url = localStorage.crashRecoveryUrl ?? window.location.origin;
		window.open(`${launchHandler}:${url}`);
		cargoWiseClient.shutDownApplication();
	};

	window.exit = () => {
		cargoWiseClient.shutDownApplication();
	};
}
