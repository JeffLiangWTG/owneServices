const HtmlWebPackPlugin = require("html-webpack-plugin");
var webpack = require('webpack');

module.exports = {
	entry: {
		preload: ['babel-polyfill', './src/index.tsx']
	},
	devtool: "source-map",
	target: 'web',
	resolve: {
		extensions: ['.ts', '.tsx', '.js', '.json'],
		alias : {
			'jquery' : __dirname + '/node_modules/jquery/src/jquery'
		},
		fallback: {
			"http": false,
			"https": false
		}
	},
	output: {
		hashFunction: 'xxhash64'
	},
	devServer: {
		historyApiFallback: true
	},
	module: {
		rules: [
			{
				test: /\.(ts|js)x?$/,
				exclude: /node_modules/,
				use: [
					{ loader: "babel-loader" }
				]
			},
			{
				test: /\.html$/,
				use: [
					{
						loader: "html-loader"
					}
				]
			},
			{
				test: /\.css$/,
				use: ['style-loader', 'css-loader']
			},
			{
				test: /\.(eot|svg|ttf|woff|woff2)$/,
				use: ['file-loader']
			}
		]
	},
	plugins: [
		new HtmlWebPackPlugin({
			template: "./src/index.html",
			filename: "./index.html"
		}),
		new webpack.ProvidePlugin({
			$: "jquery",
			jQuery: "jquery",
			moment: "moment"
		})
	]
};
