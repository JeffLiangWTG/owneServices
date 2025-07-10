//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgDebtorGroupBankCurrentOverrideLookups
//
//    This class should be used for overriding collections in AutoOrgDebtorGroupBankCurrentOverrideLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgDebtorGroupBankCurrentOverrideLookups : AutoOrgDebtorGroupBankCurrentOverrideLookups
	{
		public OrgDebtorGroupBankCurrentOverrideLookups(AutoOrgDebtorGroupBankCurrentOverride parent) : base(parent)
		{
		}

		#region Currency List

		public IActiveBusinessObjectCollection CurrencyList
		{
			get { return fCurrencyList ?? (fCurrencyList = (IActiveBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Integration.IRefCurrencyCollection>(), new object[] { Factory })); }
		}
		IActiveBusinessObjectCollection fCurrencyList;

		#endregion

		#region BankAccount List

		public BusinessObjectCollection BankAccountList
		{
			get
			{
				if (fBankAccountList == null)
				{
					object[] parameters = new object[] { Factory, new ZQuery(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK) };
					fBankAccountList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Integration.IAccBankAccountCollection>(), parameters);
				}

				fBankAccountList.Load();
				return fBankAccountList;
			}
		}
		BusinessObjectCollection fBankAccountList;

		#endregion

	}
}
