using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Common.DataTransfer
{
	[Immutable]
	public class ContainerModeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ContainerModeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.ContainerModes.AgentConsol, nameof(Xsd.ContainerMode.CON));
			yield return new Mapping(Core.Constants.ContainerModes.AIR, nameof(Xsd.ContainerMode.AIR));
			yield return new Mapping(Core.Constants.ContainerModes.All, nameof(Xsd.ContainerMode.ALL));
			yield return new Mapping(Core.Constants.ContainerModes.BreakBulk, nameof(Xsd.ContainerMode.BBK));
			yield return new Mapping(Core.Constants.ContainerModes.Bulk, nameof(Xsd.ContainerMode.BLK));
			yield return new Mapping(Core.Constants.ContainerModes.BuyersConsol, nameof(Xsd.ContainerMode.BCN));
			yield return new Mapping(Core.Constants.ContainerModes.ShippersConsol, nameof(Xsd.ContainerMode.SCN));
			yield return new Mapping(Core.Constants.ContainerModes.FCL, nameof(Xsd.ContainerMode.FCL));
			yield return new Mapping(Core.Constants.ContainerModes.FTL, nameof(Xsd.ContainerMode.FTL));
			yield return new Mapping(Core.Constants.ContainerModes.Groupage, nameof(Xsd.ContainerMode.GRP));
			yield return new Mapping(Core.Constants.ContainerModes.LCL, nameof(Xsd.ContainerMode.LCL));
			yield return new Mapping(Core.Constants.ContainerModes.Loose, nameof(Xsd.ContainerMode.LSE));
			yield return new Mapping(Core.Constants.ContainerModes.LTL, nameof(Xsd.ContainerMode.LTL));
			yield return new Mapping(Core.Constants.ContainerModes.Mail, nameof(Xsd.ContainerMode.MAI));
			yield return new Mapping(Core.Constants.ContainerModes.OnBoardCourier, nameof(Xsd.ContainerMode.OBC));
			yield return new Mapping(Core.Constants.ContainerModes.Other, nameof(Xsd.ContainerMode.OTH));
			yield return new Mapping(Core.Constants.ContainerModes.ULD, nameof(Xsd.ContainerMode.ULD));
			yield return new Mapping(Core.Constants.ContainerModes.Unaccompanied, nameof(Xsd.ContainerMode.UNA));
			yield return new Mapping(Core.Constants.ContainerModes.FreightAllKind, nameof(Xsd.ContainerMode.FAK));
			yield return new Mapping(Core.Constants.ContainerModes.FCLMixedShipper, nameof(Xsd.ContainerMode.FCX));
			yield return new Mapping(Core.Constants.ContainerModes.Empty, nameof(Xsd.ContainerMode.EMP));
			yield return new Mapping(Core.Constants.ContainerModes.RollOnRollOff, nameof(Xsd.ContainerMode.ROR));
			yield return new Mapping(Core.Constants.ContainerModes.Combination, nameof(Xsd.ContainerMode.COM));
			yield return new Mapping(Core.Constants.ContainerModes.Containerised, nameof(Xsd.ContainerMode.CNT));
			yield return new Mapping(Core.Constants.ContainerModes.NonContainerised, nameof(Xsd.ContainerMode.NCT));
			yield return new Mapping(Core.Constants.ContainerModes.Liquid, nameof(Xsd.ContainerMode.LQD));
		}

		public static readonly ContainerModeToXmlCodeMappings Instance = new ContainerModeToXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("93f56c34-402e-40d1-b3fe-693360fdc48d", "Container Mode"); }
		}

		public new Xsd.ContainerMode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ContainerMode.FCL, errorContext, notify);
		}
	}
}
