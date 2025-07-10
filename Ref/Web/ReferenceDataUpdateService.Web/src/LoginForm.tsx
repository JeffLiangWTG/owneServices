import React from "react";
import { loginRequest } from "./AuthConfig";
import MsalWrapper from "./MsalWrapper";

export const LoginForm = () => {

	const handleLoginRedirect = async () => {
		await MsalWrapper.getInstance().loginRedirect(loginRequest);
	};

	const handleLogoutRedirect = async () => {
		await MsalWrapper.getInstance().logoutRedirect();
	};

	return (
		<div className="form-inline">
			{ MsalWrapper.getInstance().isAuthenticated() ?
				(
				<>
					<span className="navbar-text mr-sm-2">
						Hello, {MsalWrapper.getInstance().getUserName()}
					</span>
					<button className="btn btn-primary btn-sm" onClick={handleLogoutRedirect}>
						Sign Out
					</button>
				</>
				) :
				(
					<button className="btn btn-primary btn-sm" onClick={handleLoginRedirect}>
						Sign In
					</button>
				)
			}
		</div>
	);
};
