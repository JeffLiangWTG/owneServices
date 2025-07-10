using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	[Immutable]
	public class OrderContainerModeToXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		OrderContainerModeToXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.ContainerModes.AIR, nameof(Xsd.OrderContainerMode.AIR));
			yield return new Mapping(Core.Constants.ContainerModes.BreakBulk, nameof(Xsd.OrderContainerMode.BBK));
			yield return new Mapping(Core.Constants.ContainerModes.Bulk, nameof(Xsd.OrderContainerMode.BLK));
			yield return new Mapping(Core.Constants.ContainerModes.FCL, nameof(Xsd.OrderContainerMode.FCL));
			yield return new Mapping(Core.Constants.ContainerModes.LCL, nameof(Xsd.OrderContainerMode.LCL));
			yield return new Mapping(Core.Constants.ContainerModes.Loose, nameof(Xsd.OrderContainerMode.LSE));
			yield return new Mapping(Core.Constants.ContainerModes.ULD, nameof(Xsd.OrderContainerMode.ULD));
			yield return new Mapping(Core.Constants.ContainerModes.AgentConsol, nameof(Xsd.OrderContainerMode.CON));
			yield return new Mapping(Core.Constants.ContainerModes.RollOnRollOff, nameof(Xsd.OrderContainerMode.ROR));
			yield return new Mapping(Core.Constants.ContainerModes.Other, nameof(Xsd.OrderContainerMode.OTH));

			yield return new Mapping(Core.Constants.ContainerModes.FTL, nameof(Xsd.OrderContainerMode.FTL));
			yield return new Mapping(Core.Constants.ContainerModes.Liquid, nameof(Xsd.OrderContainerMode.LQD));
			yield return new Mapping(Core.Constants.ContainerModes.LTL, nameof(Xsd.OrderContainerMode.LTL));
			yield return new Mapping(Core.Constants.ContainerModes.Mail, nameof(Xsd.OrderContainerMode.MAI));
			yield return new Mapping(Core.Constants.ContainerModes.OnBoardCourier, nameof(Xsd.OrderContainerMode.OBC));
			yield return new Mapping(Core.Constants.ContainerModes.Unaccompanied, nameof(Xsd.OrderContainerMode.UNA));
		}

		public static readonly OrderContainerModeToXmlCodeMappings Instance = new OrderContainerModeToXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("c81c6538-0ec1-4c4d-821e-84195e6840c1", "Order Container Mode"); }
		}

		public new Xsd.OrderContainerMode GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.OrderContainerMode.FCL, errorContext, notify);
		}
	}
}
