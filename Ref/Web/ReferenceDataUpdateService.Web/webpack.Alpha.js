const { merge } = require("webpack-merge");
const common = require("./webpack.config");
var webpack = require("webpack");

module.exports = merge(common, {
	output: {
		publicPath: "/PortalAlpha/",
	},
	mode: "production",
	plugins: [
		new webpack.DefinePlugin({
			__SafeAPI__: JSON.stringify("//***updateService***/UpdateAlpha/odata/"),
			__StagingAPI__: JSON.stringify(
				"//***updateService***/StagingAlpha/odata/"
			),
			BASENAME: JSON.stringify("/PortalAlpha"),
			__Authority__: JSON.stringify(
				"https://cargowiseb2ctest01.b2clogin.com/cargowiseb2ctest01.onmicrosoft.com/b2c_1a_signup_signin/v2.0/"
			),
			__AuthorityDomain__: JSON.stringify("cargowiseb2ctest01.b2clogin.com"),
			__ClientId__: JSON.stringify("958b04c0-509d-4c80-a0f8-cdede322ac03"),
			__RedirectUri__: JSON.stringify(
				"https://***updateService***/PortalAlpha/authenCallback"
			),
			__KibanaRootUrl__: JSON.stringify(
				"//eye-test.wtg.ws/s/wisecloud-support/app/discover#"
			),
			__KibanaIndexSearch__: JSON.stringify("idx-*-*-refdatarepo-xmlproducer*"),
			__PostLogoutRedirectUri__: JSON.stringify("/PortalAlpha"),
			__DeliveryServiceHealthCheckURL__: JSON.stringify(
				"//***deliveryService***/ServiceAlpha/wtg/status"
			),
			__UpdateServiceHealthCheckURL__: JSON.stringify(
				"//***updateService***/UpdateAlpha/wtg/status"
			),
			__QuartzHealthCheckURL__: JSON.stringify(
				"//***updateService***/QuartzAlpha/quartz/wtg/health"
			),
		}),
	],
});
