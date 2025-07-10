using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	[Immutable]
	public class AgentTypeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		AgentTypeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.AgentType.Direct, nameof(Xsd.ConsolType.Direct));
			yield return new Mapping(Core.Constants.AgentType.CoLoad, nameof(Xsd.ConsolType.CoLoad));
			yield return new Mapping(Core.Constants.AgentType.Agent, nameof(Xsd.ConsolType.Agent));
			yield return new Mapping(Core.Constants.AgentType.Charter, nameof(Xsd.ConsolType.Charter));
			yield return new Mapping(Core.Constants.AgentType.OnBoardCourier, nameof(Xsd.ConsolType.OnBoardCourier));
			yield return new Mapping(Core.Constants.AgentType.Other, nameof(Xsd.ConsolType.Other));
			yield return new Mapping(Core.Constants.AgentType.AWBCoload, nameof(Xsd.ConsolType.AWBCoload));
			yield return new Mapping(Core.Constants.AgentType.AWBMaster, nameof(Xsd.ConsolType.MultiAWBMaster));
			yield return new Mapping(Core.Constants.AgentType.Courier, nameof(Xsd.ConsolType.Courier));
		}

		public static readonly AgentTypeToXmlCodeMappings Instance = new AgentTypeToXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("29f29999-e5da-44c5-855d-e8a762c54f8c", "Consol Agent Type"); }
		}

		public new Xsd.ConsolType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ConsolType.Agent, errorContext, notify);
		}
	}
}
