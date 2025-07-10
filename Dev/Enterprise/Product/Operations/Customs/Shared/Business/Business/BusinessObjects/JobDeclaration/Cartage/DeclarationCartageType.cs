using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public abstract class DeclarationCartageType : CartageType
	{
		protected DeclarationCartageType(BaseJobDeclaration declaration) : base(declaration) { }

		protected BaseJobDeclaration DeclarationParent
		{
			get { return (BaseJobDeclaration)CartageParent; }
		}

		public override IReadOnlyCollection<ICartageContainer> CartageContainers
		{
			get { return (ICartageContainer[])DeclarationParent.CusContainers.ToArray(typeof(ICartageContainer)); }
		}

		public override IReadOnlyCollection<ICartageLooseCargo> CartageLooseCargo
		{
			get { return new ICartageLooseCargo[] { DeclarationParent }; }
		}

		public override JobDocAddress GetCartageAddress(ZString orgType)
		{
			switch (orgType)
			{
				case LocalCartageJobOrgTypeList.Codes.CFS:
					return DeclarationParent.DepotDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CNE:
					return DeclarationParent.ImporterDeliveryAddress;
				case LocalCartageJobOrgTypeList.Codes.CNR:
					return DeclarationParent.SupplierPickupAddress;
				case LocalCartageJobOrgTypeList.Codes.CTO:
					return DeclarationParent.ContainerTerminalOperatorDocAddress;
				case LocalCartageJobOrgTypeList.Codes.CYD:
					return DeclarationParent.ContainerYardDocAddress;
				default:
					return null;
			}
		}

		public override ZString PortOfLoading
		{
			get { return DeclarationParent.JE_RL_NKPortOfLoading; }
		}

		public override ZString PortOfDischarge
		{
			get { return DeclarationParent.JE_RL_NKPortOfArrival; }
		}

		public override ZString Vessel
		{
			get { return DeclarationParent.JE_VesselName; }
		}

		public override ZString VoyageFlight
		{
			get { return DeclarationParent.JE_VoyageFlightNo; }
		}

		public override ZDateTime E_ARV
		{
			get { return DeclarationParent.JE_DateOfArrival; }
		}

		public override ZDateTime E_DEP
		{
			get { return DeclarationParent.JE_ExportDate; }
		}

		public override ZDateTime A_ARV
		{
			get { return DeclarationParent.JE_DateOfArrival; }
		}

		public override ZDateTime A_DEP
		{
			get { return DeclarationParent.JE_ExportDate; }
		}

		public override ZDateTime FCLAvailabilityDate
		{
			get { return DeclarationParent.DocsAndCartage.JP_FCLAvailable; }
		}

		public override ZDateTime FCLStorageDate
		{
			get { return DeclarationParent.DocsAndCartage.JP_FCLStorageCommences; }
		}

		public override ZDateTime LCLAvailabilityDate
		{
			get { return DeclarationParent.DocsAndCartage.JP_LCLAvailable; }
		}

		public override ZDateTime LCLStorageDate
		{
			get { return DeclarationParent.DocsAndCartage.JP_LCLStorageCommences; }
		}

		public override ZDateTime FCLCutOff
		{
			get { return ZDateTime.Empty; }
		}

		public override ZDateTime FCLReceivalCommences
		{
			get { return ZDateTime.Empty; }
		}

		public override ZDateTime LCLCutOff
		{
			get { return ZDateTime.Empty; }
		}

		public override ZDateTime LCLReceivalCommences
		{
			get { return ZDateTime.Empty; }
		}
	}
}
