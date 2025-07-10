using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class StandAloneInvoiceValueObjectDataAdapter : InvoiceChargeValueObjectDataAdapter<BaseJobComInvoiceHeader, Xsd.InvoiceHeader>
	{
		protected StandAloneInvoiceValueObjectDataAdapter()
		{
		}

		/// <summary>
		/// This is for returning a country-specific adapter for a stand-alone commercial invoice.
		/// </summary>
		public static StandAloneInvoiceValueObjectDataAdapter New()
		{
			StandAloneInvoiceValueObjectDataAdapter result;

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
					result = New(ObjectFactory.GetType<Integration.Customs.AU.IAUStandAloneInvoiceValueObjectDataAdapter>());
				}
				else if (customsCountry == Core.Constants.CountryCodes.NewZealand)
				{
					result = New(ObjectFactory.GetType<Integration.Customs.NZ.INZStandAloneInvoiceValueObjectDataAdapter>());
				}
				else if (customsCountry == Core.Constants.CountryCodes.UnitedStates)
				{
					result = New(ObjectFactory.GetType<Integration.Customs.US.IUSStandAloneInvoiceDataAdapter>());
				}
				else
				{
					result = new StandAloneInvoiceValueObjectDataAdapter();
				}
			}
			return result;
		}

		static StandAloneInvoiceValueObjectDataAdapter New(Type type)
		{
			return (StandAloneInvoiceValueObjectDataAdapter)Activator.CreateInstance(type);
		}

		protected delegate StandAloneInvoiceValueObjectDataAdapter NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#region Overrides

		public override string RootCollectionElementName
		{
			get { return (NoResString)"Invoices"; }
		}

		public override string RootElementName
		{
			get { return "InvoiceHeader"; }
		}

		public override System.Xml.Schema.XmlSchema Schema
		{
			get { return CustomsXmlSchemaDefinitions.Instance.SingleInvoiceSchema; }
		}

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { return CustomsXmlSchemaDefinitions.Instance.InvoicesSchema; }
		}

		protected override BaseJobComInvoiceHeader FindBusinessObject(Xsd.InvoiceHeader value, IValueObjectImportContext context)
		{
			return null;
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(BaseJobComInvoiceHeader bizObj, Xsd.InvoiceHeader value, IValueObjectImportContext context)
		{
			ImportFromBranchDetails(bizObj, context);
			InvoiceDataTransferTool.ImportInvoiceDetails(bizObj, value, context);
			ImportInvoiceChargesDetails(value.InvoiceCharges, bizObj.Charges, context);
			XsdPlannedLegObjectHelper.ImportPlannedLegs(bizObj.Transports, value.Routings, context, (NoResString)"Routings");

			AddImportEvent(bizObj);

			if (bizObj.JobDeclaration != null)
			{
				bizObj.JobDeclaration.ResumeApportionment();
			}
		}

		void ImportFromBranchDetails(BaseJobComInvoiceHeader bizObj, IValueObjectImportContext context)
		{
			var xmlInterchange = context.Interchange as Xsd.XmlInterchange;
			var branchCode = (xmlInterchange != null && xmlInterchange.IsSpecified && xmlInterchange.InterchangeInfo.IsSpecified && xmlInterchange.InterchangeInfo.Target.IsSpecified) ? xmlInterchange.InterchangeInfo.Target.BranchCode : ZString.Empty;

			if (branchCode.IsEmpty)
			{
				context.Add(new InfoNotification(Res.GetString("89432be2-8e02-4119-bc85-5266f91260a9", "Branch Code is not specified - Invoice branch is set to the current Login Branch '{0}'", GlbBranch.CurrentBranch.GB_Code)));
				bizObj.JZ_GB = Env.CurrentBranch.PK;
			}
			else
			{
				var branch = GetBranch(branchCode, context.Factory);
				if (branch == null)
				{
					context.Add(new WarningNotification(Res.GetString("0cd66f61-38e9-44c2-b41c-0f072e1d8d0b", "Branch '{0}' is not found or inactive - Invoice branch is set to the current Login Branch '{1}'", branchCode, GlbBranch.CurrentBranch.GB_Code)));
					bizObj.JZ_GB = Env.CurrentBranch.PK;
				}
				else
				{
					context.Add(new InfoNotification(Res.GetString("0764397d-769f-4d4f-bfdd-d75246c18416", "Invoice branch is set to '{0}'", branchCode)));
					bizObj.JZ_GB = branch.PK;
				}
			}
		}

		protected override BaseJobComInvoiceHeader NewBusinessObject(Xsd.InvoiceHeader value, IValueObjectImportContext context)
		{
			BaseJobComInvoiceHeader result = context.Factory.New<BaseJobComInvoiceHeader>();

			new FakeDeclarationCreatorForInvoice(result);

			result.SuspendValidation();

			return result;
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(BaseJobComInvoiceHeader bizObj, Xsd.InvoiceHeader constructedValueObject, IValueObjectExportContext context)
		{
			InvoiceDataTransferTool.ExportInvoiceHeaderValues(bizObj, constructedValueObject, context);
			constructedValueObject.InvoiceCharges = ExportInvoiceCharges(bizObj.Charges);
			ExportRoutings(bizObj, constructedValueObject, context);
			AddExportEvent(constructedValueObject, bizObj, context);
		}

		void ExportRoutings(BaseJobComInvoiceHeader bizObj, Xsd.InvoiceHeader constructedValueObject, IValueObjectExportContext context)
		{
			Xsd.PlannedLegCollection routings = constructedValueObject.Routings;
			routings.IsSpecified = false;
			foreach (Transport transport in bizObj.Transports)
			{
				Xsd.PlannedLeg plannedLeg = routings.AddNew();
				XsdPlannedLegObjectHelper.ExportPlannedLeg(plannedLeg, transport, context, (NoResString)"Routings");
				routings.IsSpecified = true;
			}
		}

		GlbBranch GetBranch(string branchCode, BusinessObjectFactory factory)
		{
			var query = new ZQuery(GlbBranchSchema.GB_Code, branchCode);
			query.AddToFilter(GlbBranchSchema.GB_IsActive, true);
			return factory.LoadTop1<GlbBranch>(query);
		}

		#endregion

		#region Implementation

		protected InvoiceDataTransferTool InvoiceDataTransferTool
		{
			get { return fInvoiceDataTransferTool ?? (fInvoiceDataTransferTool = GetInvoiceDataTransferTool()); }
		}
		InvoiceDataTransferTool fInvoiceDataTransferTool;

		protected virtual InvoiceDataTransferTool GetInvoiceDataTransferTool()
		{
			return new InvoiceDataTransferTool(true);
		}

		#endregion
	}
}
