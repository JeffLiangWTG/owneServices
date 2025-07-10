using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Integration.Customs.US.ISF;

namespace Enterprise.Customs.US.DocumentWrappers
{
	[CodeAlive("This is used by reflection code in DocumentWrapper.")]
	public class FreightWrapperFromISF : FreightWrapper
	{
		public static FreightWrapperFromISF New(ZGuid pK, BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "Factory");
			return new FreightWrapperFromISF(factory.Load<CusISFHeader>(pK), factory);
		}

		public static FreightWrapperFromISF New(ICusISFHeader isfToWrap, BusinessObjectFactory factory)
		{
			return New(isfToWrap.PK, factory);
		}

		public FreightWrapperFromISF(CusISFHeader headerBO, BusinessObjectFactory factory)
			: base(headerBO, factory)
		{
			this.headerBO = headerBO;
		}
		readonly CusISFHeader headerBO;

		protected override Job GetJob()
		{
			Job result = null;
			if (headerBO != null)
			{
				ZQuery query = new ZQuery(JobHeaderSchema.JH_ParentID, headerBO.PK);
				Job[] headers = Factory.Load<Job>(query);
				if (headers.Length > 0)
				{
					result = headers[0];
				}
			}
			return result;
		}

		protected override ICusISFHeader GetImporterSecurityFiling()
		{
			return headerBO;
		}

		protected override ZString GetMasterBill()
		{
			return headerBO.BF_OceanBill.IsEmpty ? headerBO.BF_MasterBill : headerBO.BF_OceanBill;
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("7b60b470-5b50-4cd4-b94c-9ff6e2b790d6", "ISF Job");
		}

		protected override ZString GetJobNumber()
		{
			return headerBO.BF_JobReference;
		}

		protected override ZString GetOwnerReference()
		{
			return headerBO.BF_OwnerReference;
		}

		protected override ZString GetHouseBill()
		{
			return headerBO.BF_HouseBill;
		}

		protected override ZString GetMasterBillHeading()
		{
			return headerBO.BF_OceanBill.IsEmpty ? Res.GetString("c0b7abec-32d3-476c-a60b-4748bb026c35", "Master Bill") : Res.GetString("e26a3456-efea-4d96-95d0-9114f0024d04", "Ocean Bill");
		}

		protected override ZString GetHouseBillHeading()
		{
			return Res.GetString("14f33850-0049-4d4c-a7bb-eeb53ccde97f", "House Bill");
		}

		protected override OrganisationWrapper GetMainShipToParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.MainShipToParty, headerBO.MainShipToParty, Factory);
		}

		protected override OrganisationWrapper GetSellingParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.SellingParty, headerBO.SellingParty, Factory);
		}

		protected override OrganisationWrapper GetConsolidator()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consolidator, headerBO.Consolidator, Factory);
		}

		protected override OrganisationWrapper GetStuffingLocation()
		{
			return new OrganisationWrapper(OrganisationUsageType.StuffingLocation, headerBO.StuffingLocation, Factory);
		}

		protected override OrganisationWrapper GetBuyer()
		{
			return new OrganisationWrapper(OrganisationUsageType.Buyer, headerBO.BuyingParty, Factory);
		}

		protected override OrganisationWrapper GetImportAgent()
		{
			var importer = headerBO.Importer;
			var orgAddress = importer.GetCustomsAddressDetailsFallingBackToMainAddress();
			return new OrganisationWrapper(OrganisationUsageType.ImportAgent, orgAddress, ContactType.All, Factory);
		}

		protected override OrganisationWrapper GetBookingParty()
		{
			return new OrganisationWrapper(OrganisationUsageType.BookingParty, headerBO.BookingParty, Factory);
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new ISFRouteWrapperCollection(headerBO, Factory);
		}

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new ISFCustomsEntryWrapperCollection(headerBO, Factory);
		}

		protected override ZString GetCustomsEntryNumber()
		{
			return headerBO.BF_CustomsReference;
		}

		protected override ContainerWrapperCollection GetContainers()
		{
			return new ISFContainerWrapperCollection(headerBO, Factory);
		}

		protected override RequiredDocumentsWrapperCollection GetRequiredDocuments()
		{
			return new RequiredDocumentsWrapperCollection(headerBO.RequiredDocuments, Factory);
		}
	}
}
