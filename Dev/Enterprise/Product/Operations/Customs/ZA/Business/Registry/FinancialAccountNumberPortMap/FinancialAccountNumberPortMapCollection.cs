using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using RefCountry = Enterprise.MasterFiles.Business.RefCountry;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.ZA.Business.XmlSerializers")]
	public class FinancialAccountNumberPortMapCollection : RegistryBusinessObjectCollectionTemplate
	{
		public FinancialAccountNumberPortMapCollection()
			: base(new BusinessObjectFactory() { NameForDebugging = "FinancialAccountNumberPortMapCollection_Ctor" })
		{
		}

		public FinancialAccountNumberPortMapCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new FinancialAccountNumberPortMap this[int i]
		{
			get { return (FinancialAccountNumberPortMap)Elements[i]; }
		}

		public new FinancialAccountNumberPortMap AddNew()
		{
			return (FinancialAccountNumberPortMap)base.AddNew();
		}

		public ZGuid GetCreditorFor(ZGuid orgPK, ZString customsOfficeCode)
		{
			foreach (FinancialAccountNumberPortMap mapping in this)
			{
				if (mapping.OrganizationPK == orgPK && mapping.CustomsOfficeCode == customsOfficeCode)
				{
					return mapping.CreditorPK;
				}
			}
			return ZGuid.Empty;
		}

		public ZString GetFinancialAccountNumberFor(ZGuid orgPK, ZString customsOfficeCode)
		{
			foreach (FinancialAccountNumberPortMap mapping in this)
			{
				if (mapping.OrganizationPK == orgPK && mapping.CustomsOfficeCode == customsOfficeCode)
				{
					return mapping.FinancialAccountNumber;
				}
			}
			return ZString.Empty;
		}

		public FinancialAccountNumberPortMap GetMappingFor(RefCountry countryOfAgentCode, ZString agentCode, ZString customsOfficeCode, ZString paymentMethod)
		{
			switch (paymentMethod)
			{
				case PaymentMethodCodeList.Codes.Cash:
					return GetCashMapping(countryOfAgentCode, agentCode) ?? GetCustomsOfficeCodeAndAgentCodeMapping(countryOfAgentCode, agentCode, customsOfficeCode);
				case PaymentMethodCodeList.Codes.Free:
					return GetCustomsOfficeCodeAndAgentCodeMapping(countryOfAgentCode, agentCode, customsOfficeCode) ?? GetCashMapping(countryOfAgentCode, agentCode);
				default:
					return GetCustomsOfficeCodeAndAgentCodeMapping(countryOfAgentCode, agentCode, customsOfficeCode);
			}
		}

		FinancialAccountNumberPortMap GetCashMapping(RefCountry countryOfAgentCode, ZString agentCode)
		{
			return this.OfType<FinancialAccountNumberPortMap>().FirstOrDefault(x => x.Cash && (x.Organization?.GetAgentCode(countryOfAgentCode) ?? ZString.Empty) == agentCode);
		}

		FinancialAccountNumberPortMap GetCustomsOfficeCodeAndAgentCodeMapping(RefCountry countryOfAgentCode, ZString agentCode, ZString customsOfficeCode)
		{
			return this.OfType<FinancialAccountNumberPortMap>().FirstOrDefault(x => x.CustomsOfficeCode == customsOfficeCode && (x.Organization?.GetAgentCode(countryOfAgentCode) ?? ZString.Empty) == agentCode);
		}

		public FinancialAccountNumberPortMap GetByFinancialAccountNumber(ZString faNumber)
		{
			return this.OfType<FinancialAccountNumberPortMap>()?.FirstOrDefault(x => x.FinancialAccountNumber == faNumber);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FinancialAccountNumberPortMapCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FinancialAccountNumberPortMap(CurrentFallbackLevel, CurrentFactory, this);
		}
	}
}
