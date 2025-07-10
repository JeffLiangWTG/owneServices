using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.GUI.Testing
{
	class DummyScreeningPartyProviderWithJobDeclaration : DummyScreeningPartyProvider, Enterprise.Integration.Customs.IBaseJobDeclaration, ICancellable
	{
		#region IBaseJobDeclaration Members

		public ZString JE_AddInfo
		{
			get;
			set;
		}

		public ZString JE_DeclarationReference
		{
			get;
			set;
		}

		public ZGuid JE_GB
		{
			get;
			set;
		}

		public ZGuid JE_GC
		{
			get;
			set;
		}

		public ZString JE_HouseBill
		{
			get;
			set;
		}

		public ZGuid JE_JS
		{
			get;
			set;
		}

		public ZString JE_MasterBill
		{
			get;
			set;
		}

		public ZString JE_MessageType
		{
			get;
			set;
		}

		public ZDateTime JE_SystemCreateTimeUtc
		{
			get;
			set;
		}

		public ZBool JE_IsCancelled
		{
			get;
			set;
		}

		public ZString JE_ApplicationCode
		{
			get;
			set;
		}

		public ZString JE_OwnerRef
		{
			get;
			set;
		}

		public ZString JE_EntryStatus
		{
			get;
			set;
		}

		public ZDateTime JE_EntrySubmittedDate
		{
			get;
			set;
		}

		public ZGuid JE_OH_Supplier
		{
			get;
			set;
		}

		public ZGuid JE_OH_Importer
		{
			get;
			set;
		}

		public ZString JE_ScreeningStatus
		{
			get;
			set;
		}

		public ZString JE_RL_NKOrigin
		{
			get;
			set;
		}

		public ZString JE_RL_NKFinalDestination
		{
			get;
			set;
		}

		public ZString JE_AgentsReference
		{
			get;
			set;
		}

		public ZString JE_ContainerMode
		{
			get;
			set;
		}

		public ZGuid CompanyPK
		{
			get { return ZGuid.Empty; }
		}

		public bool IsDeclarationMatchSpecificCountry(ZString countryCode)
		{
			return false;
		}

		public ZInt JE_LandedPieces { get; set; }

		public ZInt JE_TotalNoOfPacks { get; set; }

		public ZBool IsExport
		{
			get { return false; }
		}

		public ZBool IsReciprocalRates
		{
			get { return false; }
		}

		public ZString LocalCurrencyCode
		{
			get { return ZString.Empty; }
		}

		public ZString GetContainerMode(ZString transportMode, ZString shipmentPackingMode)
		{
			return ZString.Empty;
		}

		public ZBool JE_OverrideFreightDefaults { get; set; }

		public ZInt JE_ClusterKey { get; set; }

		public ZString GetCreditCheckMessage()
		{
			return ZString.Empty;
		}

		public Enterprise.Integration.Customs.IInvoiceHeaderActiveCollection Invoices => throw new NotImplementedException();

		public bool IsDeclarationIntegrated => throw new NotImplementedException();

		public IBusinessObjectCollection InvoiceLines => throw new NotImplementedException();

		public bool IsInvoiceLinesLoaded => throw new NotImplementedException();

		#endregion

		#region ICancellable Members

		public string CanCancel()
		{
			return null;
		}

		public string CanReactivate()
		{
			return null;
		}

		public bool IsCancelled
		{
			get;
			set;
		}

		public bool IsCancelledHasChanged
		{
			get { return false; }
		}

		public ZDate DateForDutyRate => throw new NotImplementedException();

		#endregion ICancellable Members
	}
}
