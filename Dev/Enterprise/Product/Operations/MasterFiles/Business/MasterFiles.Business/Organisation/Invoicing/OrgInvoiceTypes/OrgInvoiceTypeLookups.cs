using CargoWise.Application;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgInvoiceTypeLookups : AutoOrgInvoiceTypeLookups
	{
		public OrgInvoiceTypeLookups(AutoOrgInvoiceType parent)
			: base(parent)
		{
		}

		new OrgInvoiceType Parent
		{
			get { return (OrgInvoiceType)base.Parent; }
		}

		#region ModuleList

		public InvoiceTypeModuleList ModuleList
		{
			get
			{
				InvoiceTypeModuleList fModuleList = new InvoiceTypeModuleList();

				if (!ObjectFactory.Get<IAccounting>().IsMiscInvoiceInPeriodicInvoiceEnabled(GlbCompany.CurrentCompany.PK.ToGuid()))
				{
					fModuleList.RemoveCode(InvoiceTypeModuleList.Codes.MSC);
				}
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.UnitedStates)
				{
					fModuleList.RemoveCode(InvoiceTypeModuleList.Codes.ISF);
				}

				return fModuleList;
			}
		}

		#endregion

		#region BillingInterval

		public InvoiceTypeBillingInterval BillingInterval
		{
			get { return new InvoiceTypeBillingInterval(); }
		}

		#endregion

		#region ChargeInclusionType

		public InvoiceTypeChargeInclusionTypeList ChargeInclusionType
		{
			get { return new InvoiceTypeChargeInclusionTypeList(); }
		}

		#endregion

		#region Commence On

		public CodeDescriptionPairList CommenceOn
		{
			get
			{
				CodeDescriptionPairList result = null;

				if (Parent.PI_Interval == InvoiceTypeBillingInterval.Codes.WKY)
				{
					result = new DayOfWeekCodeList();
				}
				else if (Parent.PI_Interval == InvoiceTypeBillingInterval.Codes.MTH)
				{
					result = new InvoiceTypeMonthCommencement();
				}
				else
				{
					result = new CodeDescriptionPairList();
				}

				return result;
			}
		}

		#endregion

		#region InvoiceLayout

		public InvoiceTypeLayoutList InvoiceLayout
		{
			get { return InvoiceTypeLayoutList.New(); }
		}

		#endregion

		#region SecondaryInvoiceLayout

		public InvoiceTypeLayoutList SecondaryInvoiceLayout
		{
			get
			{
				var secondaryLayoutList = InvoiceTypeLayoutList.New();
				secondaryLayoutList.RemoveCode(InvoiceTypeLayoutList.Codes.NON);
				return secondaryLayoutList;
			}
		}

		#endregion

		public CodeDescriptionPairList JobTypeList
		{
			get { return Parent.OrgInvoiceTypeLookupHelper.JobTypeList; }
		}

		public CodeDescriptionPairList TransportModeList
		{
			get { return Parent.OrgInvoiceTypeLookupHelper.TransportModeList; }
		}

		public CodeDescriptionPairList ServiceDirectionList
		{
			get { return Parent.OrgInvoiceTypeLookupHelper.ServiceDirectionList; }
		}

		public CodeDescriptionPairList ServiceLevelList => Parent.OrgInvoiceTypeLookupHelper.ServiceLevelList;
	}
}
