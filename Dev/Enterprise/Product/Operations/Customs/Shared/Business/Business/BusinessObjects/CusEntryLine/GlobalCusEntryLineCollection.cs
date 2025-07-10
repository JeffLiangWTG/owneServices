using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class GlobalCusEntryLineCollection : BusinessObjectCollection<CusEntryLine>
	{
		public GlobalCusEntryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlobalCusEntryLineCollection(BusinessObjectFactory factory, BaseJobDeclaration jobDec)
			: base(factory)
		{
			fJobDec = jobDec;
			DefaultModuleFilterFields();
		}

		public GlobalCusEntryLineCollection(BusinessObjectFactory factory, BaseJobComInvoiceLine invoiceLine)
			: base(factory)
		{
			fInvoiceLine = invoiceLine;
			if (fInvoiceLine != null)
			{
				fJobDec = fInvoiceLine.Declaration;
			}

			DefaultModuleFilterFields();
		}

		protected readonly BaseJobDeclaration fJobDec;
		protected readonly BaseJobComInvoiceLine fInvoiceLine;

		public void DefaultModuleFilterFields()
		{
			if (fJobDec != null && Importer != null)
			{
				if (Importer != null)
				{
					FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer", "Property", Importer.PK));
				}

				if (LinePart != null)
				{
					FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Product", "Property", LinePart.PK));
				}

				if (LinePart == null && !TariffCode.IsEmpty)
				{
					FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Tariff Number", "Property", TariffCode));
				}
			}
		}

		public OrgHeader Importer
		{
			get { return fJobDec != null ? fJobDec.Importer : null; }
		}

		public OrgSupplierPart LinePart
		{
			get { return fInvoiceLine != null ? fInvoiceLine.Part : null; }
		}

		public ZString TariffCode
		{
			get { return fInvoiceLine != null ? fInvoiceLine.JI_Tariff : ZString.Empty; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			((CusEntryLine)bizOAdded).IsInGlobalCusEntryLineCollection = true;
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			((CusEntryLine)bizO).IsInGlobalCusEntryLineCollection = false;
		}

		#region FindBox List Provider

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new GlobalCusEntryLineListProvider(this); }
		}

		public class GlobalCusEntryLineListProvider : FindBoxListProvider
		{
			public GlobalCusEntryLineListProvider(GlobalCusEntryLineCollection collection)
				: base(collection)
			{
			}

			protected override IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
			{
				return Enumerable.Empty<BusinessObject>();
			}

			protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
			{
				return Enumerable.Empty<BusinessObject>();
			}

			public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
			{
				return (code, false);
			}
		}

		#endregion
	}
}
