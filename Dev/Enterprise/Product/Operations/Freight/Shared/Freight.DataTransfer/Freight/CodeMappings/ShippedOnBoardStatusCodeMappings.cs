using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	[Immutable]
	public class ShippedOnBoardTypeCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		ShippedOnBoardTypeCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping("CLN", nameof(Xsd.ShippedOnBoardType.CLN));
			yield return new Mapping("LDN", nameof(Xsd.ShippedOnBoardType.LDN));
			yield return new Mapping("RFS", nameof(Xsd.ShippedOnBoardType.RFS));
			yield return new Mapping("SHP", nameof(Xsd.ShippedOnBoardType.SHP));
		}

		public static readonly ShippedOnBoardTypeCodeMappings Instance = new ShippedOnBoardTypeCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("6a1c193f-1036-470c-9e2a-6650ff1e3e12", "Shipped On Board Type"); }
		}

		public new Xsd.ShippedOnBoardType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.ShippedOnBoardType.LDN, errorContext, notify);
		}
	}
}
