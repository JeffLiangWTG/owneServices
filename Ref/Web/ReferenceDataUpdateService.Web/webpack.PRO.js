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
				"//***updateService***/Update/odata/"
			),
			__StagingAPI__: JSON.stringify(
				"//***updateService***/Staging/odata/"
			),
			BASENAME: JSON.stringify("/Portal"),
			__Authority__: JSON.stringify(
				"https://cargowiseb2c01.b2clogin.com/cargowiseb2c01.onmicrosoft.com/b2c_1a_signup_signin/v2.0/"
			),
			__AuthorityDomain__: JSON.stringify("cargowiseb2c01.b2clogin.com"),
			__ClientId__: JSON.stringify("a94d40e7-a323-476c-8649-ea2baeebabd4"),
			__RedirectUri__: JSON.stringify(
				"https://***updateService***/Portal/authenCallback"
			),
			__PostLogoutRedirectUri__: JSON.stringify("/Portal"),
			__KibanaRootUrl__: JSON.stringify(
				"//eye.wtg.ws/s/wisecloud-support/app/discover#"
			),
			__KibanaIndexSearch__: JSON.stringify(
				"idx-*-*-refdatarepo-xmlproducer-prod*"
			),
			__DeliveryServiceHealthCheckURL__: JSON.stringify(
				"//refdbrepo.wisegrid.net/wtg/status"
			),
			__UpdateServiceHealthCheckURL__: JSON.stringify(
				"//***updateService***/Update/wtg/status"
			),
			__QuartzHealthCheckURL__: JSON.stringify(
				"//refdbrepoquartz.wisecloud.zone/quartz/wtg/health"
			),
		}),
	],
});
