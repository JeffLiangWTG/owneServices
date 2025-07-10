using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class CIMEDIInterchange : EDIInterchange, Integration.Forwarding.ICIMEDIInterchange
	{
		public CIMEDIInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EI_ApplicationCode = EDIInterchange.ApplicationCodes.CIM;
			EI_InterchangeType = EI_ApplicationCode;
			EI_FooterText = ((char)4).ToString();
		}

		public static ZString GetERouterFromCode()
		{
			var enterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
			return (enterpriseCode.Length == 0 ? (string)GlbCompany.CurrentCompany.GC_Code : enterpriseCode) + ForwardingConfigurationRegistry.Instance.CargoIMPClientCodeSuffix.Value;
		}

		public static ZString GetERouterToCode()
		{
			return (NoResString)"EDI CCN"; // External System Code
		}
	}
}
