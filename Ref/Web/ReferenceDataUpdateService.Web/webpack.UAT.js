const { merge } = require("webpack-merge");
const common = require("./webpack.config");
var webpack = require("webpack");

module.exports = merge(common, {
	output: {
		publicPath: "/Portal/",
	},
	mode: "production",
	plugins: [
		new webpack.DefinePlugin({
			__SafeAPI__: JSON.stringify(
				"//refdbrepoupdate-uat.wtg.zone/Update/odata/"
			),
			__StagingAPI__: JSON.stringify(
				"//refdbrepoupdate-uat.wtg.zone/Staging/odata/"
			),
			BASENAME: JSON.stringify("/Portal"),
			__Authority__: JSON.stringify(
				"https://cargowiseb2ctest01.b2clogin.com/cargowiseb2ctest01.onmicrosoft.com/b2c_1a_signup_signin/v2.0/"
			),
			__AuthorityDomain__: JSON.stringify("cargowiseb2ctest01.b2clogin.com"),
			__ClientId__: JSON.stringify("958b04c0-509d-4c80-a0f8-cdede322ac03"),
			__RedirectUri__: JSON.stringify(
				"https://refdbrepoupdate-uat.wtg.zone/Portal/authenCallback"
			),
			__PostLogoutRedirectUri__: JSON.stringify("/Portal"),
			__KibanaRootUrl__: JSON.stringify(
				"//eye.wtg.ws/s/wisecloud-support/app/discover#"
			),
			__KibanaIndexSearch__: JSON.stringify(
				"idx-*-*-refdatarepo-xmlproducer-uat*"
			),
			__DeliveryServiceHealthCheckURL__: JSON.stringify(
				"//refdbrepo-uat.wtg.zone/wtg/status"
			),
			__UpdateServiceHealthCheckURL__: JSON.stringify(
				"//refdbrepoupdate-uat.wtg.zone/Update/wtg/status"
			),
			__QuartzHealthCheckURL__: JSON.stringify(
				"//refdbrepoquartz-uat.wtg.zone/quartz/wtg/health"
			),
		}),
	],
});
