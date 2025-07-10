using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.InterfaceImplementations
{
	public class JobDeclarationCustomsCharges : ICustomsCharges
	{
		public JobDeclarationCustomsCharges(BaseJobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		protected readonly BaseJobDeclaration declaration;

		#region ICustomsCharges Members

		CustomsCharge[] ICustomsCharges.GetCustomsCharges(ILogger logger)
		{
			return GetCustomsCharges(logger);
		}

		protected CustomsCharge[] GetCustomsCharges(ILogger logger)
		{
			Dictionary<string, CustomsCharge> declarationCustomsCharges = new Dictionary<string, CustomsCharge>();
			foreach (CusEntryHeader entryHeader in GetEntries())
			{
				ICustomsCharges customsChargesHost = ServiceLocator.GetService<ICustomsCharges>(entryHeader);

				if (customsChargesHost.IsActive)
				{
					foreach (CustomsCharge customsCharge in customsChargesHost.GetCustomsCharges(logger))
					{
						string key = customsCharge.Description + "|" + customsCharge.IsPaidByBroker.ToString() + "|" + customsCharge.EntryReference + "|" + customsCharge.CreditorPK + "|" + customsCharge.DebtorPK + "|" + customsCharge.OverrideCurrency;
						if (declarationCustomsCharges.ContainsKey(key))
						{
							declarationCustomsCharges[key].AddAmount(customsCharge.Amount, customsCharge.GST);
						}
						else
						{
							var charge = new CustomsCharge(customsCharge.ChargeCodePK, customsCharge.Description, customsCharge.Amount, customsCharge.GST, customsCharge.IsPaidByBroker, customsCharge.CreditorPK, customsCharge.OverrideCurrency, customsCharge.EntryReference, customsCharge.DebtorPK);
							AddAdditionalDescriptions(charge);
							declarationCustomsCharges.Add(key, charge);
						}
					}
				}
			}
			CustomsCharge[] result = new CustomsCharge[declarationCustomsCharges.Count];
			declarationCustomsCharges.Values.CopyTo(result, 0);
			return result;
		}

		protected virtual CusEntryHeader[] GetEntries()
		{
			return (CusEntryHeader[])declaration.ActiveEntryHeaders.ToArray(typeof(CusEntryHeader));
		}

		ZBool ICustomsCharges.IsActive
		{
			get
			{
				return DoesDeclarationBelongToCurrentCompany && IsCustomsChargesActiveCore;
			}
		}

		bool DoesDeclarationBelongToCurrentCompany
		{
			get
			{
				GlbBranch branch = declaration.Branch;
				return branch != null && branch.Company != null && branch.Company.PK == GlbCompany.CurrentCompany.PK;
			}
		}

		protected virtual ZBool IsCustomsChargesActiveCore
		{
			get { return true; }
		}

		#endregion

		protected virtual void AddAdditionalDescriptions(CustomsCharge charge)
		{
		}
	}
}
