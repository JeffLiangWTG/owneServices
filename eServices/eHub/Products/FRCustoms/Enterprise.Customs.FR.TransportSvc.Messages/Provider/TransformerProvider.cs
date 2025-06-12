using Enterprise.Customs.FR.TransportSvc.Utilities;

namespace Enterprise.Customs.FR.TransportSvc.Messages
{
	public class TransformerProvider
	{
		public TransformerProvider(string schemaIdOrApplication)
		{
			this.schemaIdOrApplication = schemaIdOrApplication;
		}

		string schemaIdOrApplication;

		public string MessageTransformer => GetMessageTransformer();

		string GetMessageTransformer()
		{
			var transformer = string.Empty;

			switch (schemaIdOrApplication)
			{
				case Constants.MessageSchemas.DeltaCImportDeclarationSchema:
				case Constants.MessageSchemas.DeltaCExportDeclarationSchema:
				case Constants.MessageSchemas.DeltaDImportDeclarationSchema:
				case Constants.MessageSchemas.DeltaDExportDeclarationSchema:
				case Constants.MessageSchemas.DcgSchema:
				case Constants.MessageSchemas.CodSchema:
					transformer = DeltaGTransformer;
					break;
				case Constants.MessageSchemas.CIN745Schema:
					transformer = CIN745Transformer;
					break;
				case Constants.MessageSchemas.CIN750Schema:
					transformer = CIN750Transformer;
					break;
				case Constants.MessageSchemas.CIN755Schema:
					transformer = CIN755Transformer;
					break;
				case Constants.MessageSchemas.IE007Schema:
				case Constants.MessageSchemas.IE013Schema:
				case Constants.MessageSchemas.IE014Schema:
				case Constants.MessageSchemas.IE015Schema:
				case Constants.MessageSchemas.IEF15Schema:
				case Constants.MessageSchemas.IE044Schema:
				case Constants.MessageSchemas.IE141Schema:
					transformer = DeltaTPhase4Transformer;
					break;
				case Constants.MessageSchemas.CC007CSchema:
				case Constants.MessageSchemas.CC013CSchema:
				case Constants.MessageSchemas.CC014CSchema:
				case Constants.MessageSchemas.CC015CSchema:
				case Constants.MessageSchemas.CC034CSchema:
				case Constants.MessageSchemas.CC044CSchema:
				case Constants.MessageSchemas.CC141CSchema:
				case Constants.MessageSchemas.CC170CSchema:
					transformer = DeltaTPhase5Transformer;
					break;
				case Constants.MessageSchemas.IE507Schema:
				case Constants.MessageSchemas.IE618Schema:
					transformer = ECSTransformer;
					break;
				case Constants.MessageSchemas.APPLUSSchema:
					transformer = APPLUSTransformer;
					break;
				case Constants.MessageSchemas.IE413Schema:
				case Constants.MessageSchemas.IE414Schema:
				case Constants.MessageSchemas.IE415Schema:
				case Constants.MessageSchemas.IE432Schema:
				case Constants.MessageSchemas.IE433Schema:
					transformer = DeltaIETransformer;
					break;
				case Constants.MessageSchemas.IETS007Schema:
				case Constants.MessageSchemas.IETS015Schema:
				case Constants.MessageSchemas.IETS115Schema:
				case Constants.MessageSchemas.IETS413Schema:
				case Constants.MessageSchemas.IETS414Schema:
					transformer = PNTSTransformer;
					break;
				default:
					transformer = string.Empty;
					break;
			}

			return transformer;
		}

		public string ResponseTransformer => GetResponseTransformer();

		string GetResponseTransformer()
		{
			var transformer = string.Empty;

			switch (schemaIdOrApplication)
			{
				case Constants.ApplicationTypes.DeltaC:
				case Constants.ApplicationTypes.DeltaD:
				case Constants.ApplicationTypes.DeltaCG:
				case Constants.ApplicationTypes.DeltaDG:
					transformer = DeltaGResponseTransformer;
					break;
				case Constants.MessageSchemas.CIN745Schema:
					transformer = CIN745ResponseTransformer;
					break;
				case Constants.MessageSchemas.CIN755Schema:
					transformer = CIN755ResponseTransformer;
					break;
				case Constants.ApplicationTypes.DeltaT:
					transformer = DeltaTResponseTransformer;
					break;
				case Constants.ApplicationTypes.TP5:
					transformer = DeltaTPhase5ResponseTransformer;
					break;
				case Constants.ApplicationTypes.DeltaIE:
					transformer = DeltaIEResponseTransformer;
					break;
				case Constants.MessageSchemas.APPLUSSchema:
					transformer = APPLUSResponseTransformer;
					break;
				case Constants.ApplicationTypes.PNTS:
					transformer = PNTSResponseTransformer;
					break;
				default:
					transformer = string.Empty;
					break;
			}

			return transformer;
		}

		const string DeltaGTransformer = @"Message\Transformation\MessagePlateformDeltaG.xslt";
		const string DeltaTPhase4Transformer = @"Message\Transformation\MessagePlateformDeltaT.xslt";
		const string DeltaTPhase5Transformer = @"Message\Transformation\MessagePlateformDeltaTPhase5.xslt";
		const string CIN745Transformer = @"Message\Transformation\MessagePlateformCIN745.xslt";
		const string CIN750Transformer = @"Message\Transformation\MessagePlateformCIN750.xslt";
		const string CIN755Transformer = @"Message\Transformation\MessagePlateformCIN755.xslt";
		const string ECSTransformer = @"Message\Transformation\MessagePlateformECS.xslt";
		const string DeltaIETransformer = @"Message\Transformation\MessagePlateformDeltaIE.xslt";
		const string PNTSTransformer = @"Message\Transformation\MessagePlateformPNTS.xslt";
		const string DeltaGResponseTransformer = @"Message\Transformation\DeltaGResponse.xslt";
		const string DeltaTResponseTransformer = @"Message\Transformation\DeltaTResponse.xslt";
		const string DeltaTPhase5ResponseTransformer = @"Message\Transformation\DeltaTPhase5Response.xslt";
		const string DeltaIEResponseTransformer = @"Message\Transformation\DeltaIEResponse.xslt";
		const string CIN745ResponseTransformer = @"Message\Transformation\CIN745Response.xslt";
		const string CIN755ResponseTransformer = @"Message\Transformation\CIN755Response.xslt";
		const string APPLUSTransformer = @"Message\Transformation\MessagePlateformAPPLUS.xslt";
		const string APPLUSResponseTransformer = @"Message\Transformation\APPLUSResponse.xslt";
		const string PNTSResponseTransformer = @"Message\Transformation\PNTSResponse.xslt";
	}
}
