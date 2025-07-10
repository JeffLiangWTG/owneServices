using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public interface ISaveAndPrintUI
	{
		bool Ask(string question);
		void ShowError(string message);
		void ShowWarning(string message);
	}

	public enum StatusClass
	{
		None,
		Clear,
		Warning,
		Held,
		Underbonded
	}

	public interface IStatusClassProvider
	{
		StatusClass StatusClass { get; }
	}

	public interface IHasStatusProvider
	{
		CFSShipmentStatusProvider StatusProvider { get; }
	}

	public abstract class CFSShipmentStatusProvider
	{
		protected CFSShipmentStatusProvider(CFSShipment shipment)
		{
			this.shipment = shipment;
		}

		#region Type Deciding / Factory

		public static CFSShipmentStatusProvider New(CFSShipment shipment)
		{
#if DEBUG
			if (DummyForTest != null)
			{
				CFSShipmentStatusProvider result = DummyForTest;
				DummyForTest = null;    // So we don't get other tests hitting it.
				return result;
			}
#endif
			return (CFSShipmentStatusProvider)Activator.CreateInstance(GetTypeForNew(), new object[] { shipment });
		}

		public static Type GetTypeForNew()
		{
			Type result = typeof(BlankCFSShipmentStatusProvider);

			switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				case Constants.CountryCodes.Australia:
					result = ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICFSShipmentStatusProvider>();
					break;
				case Constants.CountryCodes.Canada:
					result = ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICFSShipmentStatusProvider>();
					break;
			}

			return result;
		}

#if DEBUG
		[ThreadStatic]
		static CFSShipmentStatusProvider DummyForTest;

		public static void SetDummyForTest(CFSShipmentStatusProvider dummyForTest) => DummyForTest = dummyForTest;
#endif

		#endregion

		#region Properties

		#region Status

		public ZString Status
		{
			get { return StatusCore(); }
		}

		protected abstract ZString StatusCore();

		#endregion

		#region Short Status

		public ZString ShortStatus
		{
			get { return ShortStatusCore(); }
		}

		protected abstract ZString ShortStatusCore();

		#endregion

		#region DetailsProvider

		public ZString DetailsFromMessages
		{
			get { return DetailsFromMessagesCore; }
		}

		protected abstract ZString DetailsFromMessagesCore
		{
			get;
		}

		#endregion

		#region StatusClass

		public StatusClass StatusClass
		{
			get { return StatusClassCore(); }
		}

		protected abstract StatusClass StatusClassCore();

		#endregion

		#endregion

		#region Saving and Printing

		public bool CanSaveAndPrint(ISaveAndPrintUI ui)
		{
			return CanSaveAndPrintCore(ui);
		}

		protected abstract bool CanSaveAndPrintCore(ISaveAndPrintUI ui);

		#endregion

		#region Implementation

		protected readonly CFSShipment shipment;

		#endregion
	}
}
