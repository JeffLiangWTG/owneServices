using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class ShipmentDeclarationGenerator : IShipmentDeclarationGenerator
	{
		#region Constructor

		protected ShipmentDeclarationGenerator()
		{
		}

		public static ShipmentDeclarationGenerator New()
		{
			ShipmentDeclarationGenerator result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden();
			}
			else
			{
				var customsCountry = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				if (customsCountry == Core.Constants.CountryCodes.Australia)
				{
					result = New(ObjectFactory.GetType<Integration.Customs.AU.IAUShipmentDeclarationGenerator>());
				}
				else if (customsCountry == Core.Constants.CountryCodes.NewZealand)
				{
					result = New(ObjectFactory.GetType<Integration.Customs.NZ.INZShipmentDeclarationGenerator>());
				}
				else if (customsCountry == Core.Constants.CountryCodes.UnitedStates)
				{
					result = New(ObjectFactory.GetType<Integration.Customs.US.IUSShipmentDeclarationGenerator>());
				}
			}

			return result;
		}

		static ShipmentDeclarationGenerator New(Type type)
		{
			return (ShipmentDeclarationGenerator)Activator.CreateInstance(type);
		}

		protected delegate ShipmentDeclarationGenerator NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		void IShipmentDeclarationGenerator.CreateDeclarationForShipment(ForwardingShipment shipment, Xsd.InvoiceHeaderCollection invoicesValue, IValueObjectImportContext context)
		{
			context.Notify(new InfoNotification(string.Format((NoResString)"Importing declaration for shipment with HouseBill '{0}'", shipment.JS_HouseBill)));

			BaseJobDeclaration jobDeclaration = context.Factory.New<BaseJobDeclaration>();

			jobDeclaration.JE_JS = shipment.PK;

			JobDeclarationSynchroniser synchroniser = new JobDeclarationSynchroniser(jobDeclaration);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			SetDefaultValues(jobDeclaration);
			if (invoicesValue != null)
			{
				InvoicesGeneratorFromXSD invGenerator = GetNewInvoiceGenerator(jobDeclaration);
				invGenerator.ImportInvoicesDetails(invoicesValue, jobDeclaration, context);
			}

			context.Notify(new DeclarationCreatedFromXmlNotification(jobDeclaration));
		}

		#region Override

		protected virtual void SetDefaultValues(BaseJobDeclaration declaration)
		{
		}

		protected virtual InvoicesGeneratorFromXSD GetNewInvoiceGenerator(BaseJobDeclaration declaration)
		{
			return new InvoicesGeneratorFromXSD(declaration);
		}

		#endregion

	}
}
