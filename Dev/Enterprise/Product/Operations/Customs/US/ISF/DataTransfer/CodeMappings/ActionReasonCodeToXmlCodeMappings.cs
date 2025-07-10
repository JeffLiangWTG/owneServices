using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	[Immutable]
	class ActionReasonCodeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ActionReasonCodeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(ActionReasonCodeList.Codes.CompliantTransaction, nameof(Xsd.ISFActionReasonCode.CT));
			yield return new Mapping(ActionReasonCodeList.Codes.FlexibleRange, nameof(Xsd.ISFActionReasonCode.FR));
			yield return new Mapping(ActionReasonCodeList.Codes.FlexibleTiming, nameof(Xsd.ISFActionReasonCode.FT));
			yield return new Mapping(ActionReasonCodeList.Codes.FlexibleRangeAndFlexibleTiming, nameof(Xsd.ISFActionReasonCode.FX));
		}

		public static readonly ActionReasonCodeToXmlCodeMappings Instance = new ActionReasonCodeToXmlCodeMappings();

		protected override string Name
		{
			get { return "Action Reason Code"; }
		}

		public new Xsd.ISFActionReasonCode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ISFActionReasonCode.CT, errorContext, notify);
		}
	}
}
