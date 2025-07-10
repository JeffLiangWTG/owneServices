import { MsalProvider } from "@azure/msal-react";
import ReactDOM from "react-dom";
import React from "react";
import "bootstrap";
import "bootstrap/dist/css/bootstrap.min.css";
import "font-awesome/css/font-awesome.min.css";
import "tempusdominus-bootstrap-4/build/css/tempusdominus-bootstrap-4.css";
import { AuthCallbackForm } from "./AuthCallbackForm";
import { EntityManager } from "./EntityManager";
import { BrowserRouter as Router, Link, Route } from "react-router-dom";
import { LoginForm } from "./LoginForm";
import { MainForm } from "./MainForm";
import MsalWrapper from "./MsalWrapper";
import { ErrorReporter } from "./ErrorReporter";
import RefCusCodeListUserViewSearchForm from "./RefCusCodeListUserViewSearchForm";
import { RefCusCodeListUserViewDetailsForm } from "./RefCusCodeListUserViewDetailsForm";
import RefAccTaxRateUserViewSearchForm from "./RefAccTaxRateUserViewSearchForm";
import RefShippingLineUserViewSearchForm from "./RefShippingLineUserViewSearchForm";
import { RefShippingLineUserViewDetailsForm } from "./RefShippingLineUserViewDetailsForm";
import { RefCusCodeListFRFallbackInvokeForm } from "./RefCusCodeListFRFallbackInvokeForm";
import { RefCusCodeListFRFallbackRevokeForm } from "./RefCusCodeListFRFallbackRevokeForm";
import { RefCusCodeListFRFallbackHistoryForm } from "./RefCusCodeListFRFallbackHistoryForm";
import RefApplicationConfigSearchForm from "./RefApplicationConfigSearchForm";
import { RefApplicationConfigDetailsForm } from "./RefApplicationConfigDetailsForm";
import RefCusProcedureUserViewSearchForm from "./RefCusProcedureUserViewSearchForm";
import { RefCusProcedureUserViewDetailsForm } from "./RefCusProcedureUserViewDetailsForm";
import RefStlScriptUserViewSearchForm from "./RefStlScriptUserViewSearchForm";
import { RefStlScriptUserViewDetailsForm } from "./RefStlScriptUserViewDetailsForm";
import { Dashboard } from "./Dashboard";
import { Processor } from "./Processor";
import { ClientsDataset } from "./ClientsDataset";
import { ClientVersions } from "./ClientVersions";
import { FilterProvider } from "./FilterContext";

declare var BASENAME: string;
let entityManager = new EntityManager();
entityManager.initialise();
let errorReporter = new ErrorReporter(window);
errorReporter.init();

const App = () => {
	/**
	 * We recommend wrapping most or all of your components in the MsalProvider component. It's best to render the MsalProvider as close to the root as possible.
	 */
	return (
		<MsalProvider instance={MsalWrapper.getInstance().getMsalInstance()}>
			<Router basename={BASENAME}>
				<div>
					<nav className="navbar navbar-expand-lg navbar-dark bg-dark">
						<div
							className="collapse navbar-collapse"
							id="navbarSupportedContent"
						>
							<ul className="navbar-nav mr-auto">
								<li className="nav-item dropdown">
									<a
										className="nav-link dropdown-toggle"
										href="#"
										id="navbarCustoms"
										data-toggle="dropdown"
										aria-haspopup="true"
										aria-expanded="false">
										Customs
									</a>
									<ul className="dropdown-menu" aria-labelledby="navbarCustoms">
										<li>
											<Link
												className="dropdown-item"
												to="/RefCusCodeListUserViewSearchForm"
											>
												Global Codes
											</Link>
										</li>
										<li>
											<Link
												className="dropdown-item"
												to="/RefCusProcedureUserViewSearchForm"
											>
												Procedure Codes
											</Link>
										</li>
										<li className="dropdown-submenu">
											<a className="dropdown-item dropdown-toggle" href="#">FR Customs Fallback</a>
											<ul className="dropdown-menu">
												<li>
													<Link
														className="dropdown-item"
														to="/RefCusCodeListFRFallbackInvokeForm"
													>
														Invoke
													</Link>
												</li>
												<li>
													<Link
														className="dropdown-item"
														to="/RefCusCodeListFRFallbackRevokeForm"
													>
														Revoke
													</Link>
												</li>
												<li>
													<Link
														className="dropdown-item"
														to="/RefCusCodeListFRFallbackHistoryForm"
													>
														History
													</Link>
												</li>
											</ul>
										</li>
									</ul>
								</li>
								<li className="nav-item dropdown">
									<a
										className="nav-link dropdown-toggle"
										href="#"
										id="navbarAccounting"
										data-toggle="dropdown"
										aria-haspopup="true"
										aria-expanded="false">
										Accounting
									</a>
									<ul className="dropdown-menu" aria-labelledby="navbarAccounting">
										<li>
											<Link
												className="dropdown-item"
												to="/RefAccTaxRateUserViewSearchForm"
											>
												Tax Rate
											</Link>
										</li>
									</ul>
								</li>
								<li className="nav-item">
									<Link
										className="nav-link"
										to="/RefStlScriptUserViewSearchForm"
									>
										STL Collectors
									</Link>
								</li>
								<li className="nav-item dropdown">
									<a
										className="nav-link dropdown-toggle"
										href="#"
										id="navbarInternational"
										data-toggle="dropdown"
										aria-haspopup="true"
										aria-expanded="false">
										International
									</a>
									<ul className="dropdown-menu" aria-labelledby="navbarInternational">
										<li>
											<Link
												className="dropdown-item"
												to="/RefShippingLineUserViewSearchForm"
											>
												Shipping Line
											</Link>
										</li>
									</ul>
								</li>
								<li className="nav-item dropdown">
									<a
										className="nav-link dropdown-toggle"
										href="#"
										id="navbarMiscellaneous"
										data-toggle="dropdown"
										aria-haspopup="true"
										aria-expanded="false">
										Miscellaneous
									</a>
									<ul className="dropdown-menu" aria-labelledby="navbarMiscellaneous">
										<li>
											<Link
												className="dropdown-item"
												to="/RefApplicationConfigSearchForm"
											>
												Application Config
											</Link>
										</li>
										<li className="dropdown-submenu">
											<a className="dropdown-item dropdown-toggle" href="#">Monitoring Tools</a>
											<ul className="dropdown-menu">
												<li>
													<Link className="dropdown-item" to="/Dashboard">
														Dashboard
													</Link>
												</li>
												<li>
													<Link className="dropdown-item" to="/Processor">
														Processor
													</Link>
												</li>
												<li>
													<Link className="dropdown-item" to="/ClientsDataset">
														Client's Data Set
													</Link>
												</li>
											</ul>
										</li>
									</ul>
								</li>
							</ul>
							<LoginForm />
						</div>
					</nav>
					<hr />
					<Route
						path="/Dashboard"
						render={() => <Dashboard entityManager={entityManager} />}
					/>
					<Route path="/ClientsDataset" render={() => <ClientsDataset />} />
					<Route
						path="/ClientVersions"
						render={() => <ClientVersions entityManager={entityManager} />}
					/>
					<Route
						path="/Processor"
						render={() => <Processor entityManager={entityManager} />}
					/>
					<FilterProvider>
						<Route
							path="/RefCusCodeListUserViewSearchForm"
							render={() => (
								<RefCusCodeListUserViewSearchForm entityManager={entityManager} />
							)}
						/>
					</FilterProvider>
					<Route
						path="/RefCusCodeListUserViewDetailsForm/:id?"
						render={(props) => (
							<MainForm
								render={(s, v) => (
									<RefCusCodeListUserViewDetailsForm
										id={props.match.params.id}
										entityManager={entityManager}
										systemVersion={v}
										onSaved={s}
									/>
								)}
								parentCode="ZZD"
								id={props.match.params.id}
								entityManager={entityManager}
							/>
						)}
					/>
					<Route
						path="/RefAccTaxRateUserViewSearchForm"
						render={() => (
							<RefAccTaxRateUserViewSearchForm entityManager={entityManager} />
						)}
					/>
					<FilterProvider>
						<Route
							path="/RefShippingLineUserViewSearchForm"
							render={() => (
								<RefShippingLineUserViewSearchForm
									entityManager={entityManager}
								/>
							)}
						/>
					</FilterProvider>
					<Route
						path="/RefShippingLineUserViewDetailsForm/:id?"
						render={(props) => (
							<MainForm
								render={(s, v) => (
									<RefShippingLineUserViewDetailsForm
										id={props.match.params.id}
										entityManager={entityManager}
										systemVersion={v}
										onSaved={s}
									/>
								)}
								parentCode="RSL"
								id={props.match.params.id}
								entityManager={entityManager}
							/>
						)}
					/>
					<Route
						path="/RefCusCodeListFRFallbackInvokeForm"
						render={() => (
							<RefCusCodeListFRFallbackInvokeForm
								entityManager={entityManager}
							/>
						)}
					/>
					<Route
						path="/RefCusCodeListFRFallbackRevokeForm"
						render={() => (
							<RefCusCodeListFRFallbackRevokeForm
								entityManager={entityManager}
							/>
						)}
					/>
					<Route
						path="/RefCusCodeListFRFallbackHistoryForm"
						render={() => (
							<RefCusCodeListFRFallbackHistoryForm
								entityManager={entityManager}
							/>
						)}
					/>
					<FilterProvider>
						<Route
							path="/RefApplicationConfigSearchForm"
							render={() => (
								<RefApplicationConfigSearchForm entityManager={entityManager} />
							)}
						/>
					</FilterProvider>
					<Route
						path="/RefApplicationConfigDetailsForm/:id?"
						render={(props) => (
							<MainForm
								render={(s, v) => (
									<RefApplicationConfigDetailsForm
										id={props.match.params.id}
										entityManager={entityManager}
										onSaved={s}
									/>
								)}
								parentCode="RAA"
								entityManager={entityManager}
							/>
						)}
					/>
					<FilterProvider>
						<Route
							path="/RefCusProcedureUserViewSearchForm"
							render={() => (
								<RefCusProcedureUserViewSearchForm
									entityManager={entityManager}
								/>
							)}
						/>
					</FilterProvider>
					<Route
						path="/RefCusProcedureUserViewDetailsForm/:id?"
						render={(props) => (
							<MainForm
								render={(s, v) => (
									<RefCusProcedureUserViewDetailsForm
										id={props.match.params.id}
										entityManager={entityManager}
										systemVersion={v}
										onSaved={s}
									/>
								)}
								parentCode="ZZ6"
								id={props.match.params.id}
								entityManager={entityManager}
							/>
						)}
					/>
					<FilterProvider>
						<Route
							path="/RefStlScriptUserViewSearchForm"
							render={() => (
								<RefStlScriptUserViewSearchForm entityManager={entityManager} />
							)}
						/>
					</FilterProvider>
					<Route
						path="/RefStlScriptUserViewDetailsForm/:id?"
						render={(props) => (
							<MainForm
								render={(s, v) => (
									<RefStlScriptUserViewDetailsForm
										id={props.match.params.id}
										entityManager={entityManager}
										systemVersion={v}
										onSaved={s}
									/>
								)}
								parentCode="STL"
								id={props.match.params.id}
								entityManager={entityManager}
							/>
						)}
					/>
					<Route path="/authenCallback" component={AuthCallbackForm} />
				</div>
			</Router>
		</MsalProvider>
	);
};

const wrapper = document.getElementById("my-app");
wrapper ? ReactDOM.render(<App />, wrapper) : false;
