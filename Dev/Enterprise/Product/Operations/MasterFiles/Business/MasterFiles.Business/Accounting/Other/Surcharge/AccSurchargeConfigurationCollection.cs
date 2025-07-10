using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccSurchargeConfigurationCollection : BusinessObjectCollection<AccSurchargeConfiguration>
	{
		public AccSurchargeConfigurationCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccSurchargeConfigurationCollection(BusinessObjectFactory factory, ZGuid companyPK) : base(factory, GetZQuery(companyPK))
		{
			this.CompanyPK = companyPK;
		}

		public AccSurchargeConfigurationCollection(BusinessObjectFactory factory, ZGuid companyPK, bool applicableOnly) : this(factory, companyPK)
		{
			this.ApplicableOnly = applicableOnly;
		}

		ZGuid CompanyPK { get; }

		readonly bool ApplicableOnly;

		static ZQuery GetZQuery(ZGuid companyPK)
		{
			return new ZQuery(AccSurchargeConfigurationSchema.ASC_GC_Company, companyPK);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var config = (AccSurchargeConfiguration)child;
			config.ASC_GC_Company = CompanyPK;
			config.ASC_Type = SurchargeTypeList.Codes.PER;
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			((AccSurchargeConfiguration)bizO).AccSurchargeBasises.RemoveAndDeleteAll();
			base.OnRemoving(bizO);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			var company = Factory.Load<GlbCompany>(CompanyPK);
			ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccountingLogHelper().AddLog(company, (AccSurchargeConfiguration)bizO, true);
		}

		protected override bool AllowNewCore => !ApplicableOnly && base.AllowNewCore;

		protected override bool AllowRemoveCore => !ApplicableOnly && base.AllowRemoveCore;

		public ZString ApplicableSurchargesAsString
		{
			get
			{
				var result = string.Empty;
				var applicableSurcharges = this.Where(o => o.IsApplicable);
				if (applicableSurcharges.Any())
				{
					if (applicableSurcharges.Count() == this.Count)
					{
						result = AccountingMasterFilesConstants.ReserveSurchargeCodes.All;
					}
					else
					{
						result = applicableSurcharges.Select(o => o.ASC_Code).Aggregate((m, n) => $"{m}, {n}");
					}
				}
				else
				{
					result = AccountingMasterFilesConstants.ReserveSurchargeCodes.Non;
				}
				return result;
			}
			set
			{
				var applicableSurchargeCodes = value.Split(',').Select(o => o.Trim());

				foreach (AccSurchargeConfiguration surcharge in this)
				{
					if (value == AccountingMasterFilesConstants.ReserveSurchargeCodes.All)
					{
						surcharge.IsApplicable = true;
					}
					else if (value == AccountingMasterFilesConstants.ReserveSurchargeCodes.Non)
					{
						surcharge.IsApplicable = false;
					}
					else
					{
						surcharge.IsApplicable = applicableSurchargeCodes.Contains(surcharge.ASC_Code);
					}
				}
			}
		}
	}
}
