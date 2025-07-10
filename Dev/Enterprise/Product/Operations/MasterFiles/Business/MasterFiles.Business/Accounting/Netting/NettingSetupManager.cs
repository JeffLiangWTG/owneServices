using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Accounting.Netting
{
	public class NettingSetupManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public NettingSetupManager(BusinessObjectFactory factory, ZGuid[] companyPks)
			: base(factory)
		{
			this.companyPks = companyPks;
		}

		readonly ZGuid[] companyPks;

		#region NettingSystemCompany

		[MaxLength(3)]
		[List(nameof(ApplicableCompanyList))]
		[RelatedBusinessObject(nameof(NettingSystemCompany))]
		public ZString NettingSystemCompanyCode
		{
			get { return nettingSystemCompanyCode; }
			set
			{
				if (value != nettingSystemCompanyCode)
				{
					SetNonPersistentPropertyValue(NettingSystemCompanyCodeInfo, ref nettingSystemCompanyCode, value);
				}
				ValidateNettingSystemCompany();
			}
		}
		ZString nettingSystemCompanyCode;

		void ValidateNettingSystemCompany()
		{
			NettingSystemCompanyCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(NettingSystemCompanyCodeInfo, ApplicableCompanyList, ResString.GetMultilingualString("0E674F48-ADF4-4B63-A43D-02A343392283", "Netting System Company"));
		}

		public CodeDescriptionPairList ApplicableCompanyList
		{
			get
			{
				if (applicableCompanyList == null)
				{
					applicableCompanyList = new CodeDescriptionPairList();
					foreach (var company in SelectedCompanies)
					{
						applicableCompanyList.AddPair(company.GC_Code, company.GC_Name);
					}
				}

				return applicableCompanyList;
			}
		}
		CodeDescriptionPairList applicableCompanyList;

		public ZPropertyInfo NettingSystemCompanyCodeInfo => GetZPropertyInfo(nameof(NettingSystemCompanyCode));

		GlbCompany[] SelectedCompanies => Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, companyPks));

		public GlbCompany NettingSystemCompany => Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, NettingSystemCompanyCode);

		#endregion

		#region NettingSystemCode
		[MaxLength(10)]
		public ZString NettingSystemCode
		{
			get { return nettingSystemCode; }
			set
			{
				if (value != nettingSystemCode)
				{
					SetNonPersistentPropertyValue(NettingSystemCodeInfo, ref nettingSystemCode, value);
				}
				ValidateNettingSystemCode();
			}
		}

		void ValidateNettingSystemCode()
		{
			NettingSystemCodeInfo.ClearAllNotifications();
			if (nettingSystemCode.IsEmpty)
			{
				NettingSystemCodeInfo.AddError(ResString.GetMultilingualString("2239D852-0BC9-4509-BC52-9D0C5C36D5C5", "Netting System Code"));
			}
			if (!Regex.IsMatch(NettingSystemCode, @"^[a-zA-Z0-9]{1,10}$"))
			{
				NettingSystemCodeInfo.AddError(Res.GetString("9AFD2898-25CD-4FE0-B420-99EC404E2AD7", "Netting System Code should consist only of letters and digits."));
			}
		}

		ZString nettingSystemCode;

		public ZPropertyInfo NettingSystemCodeInfo => GetZPropertyInfo(nameof(NettingSystemCode));

		#endregion

		#region NettingSystemDescription
		[MaxLength(80)]
		public ZString NettingSystemDescription
		{
			get { return nettingSystemDescription; }
			set
			{
				if (value != nettingSystemDescription)
				{
					SetNonPersistentPropertyValue(NettingSystemDescriptionInfo, ref nettingSystemDescription, value);
				}
				ValidateNettingSystemDescription();
			}
		}

		void ValidateNettingSystemDescription()
		{
			NettingSystemDescriptionInfo.ClearAllNotifications();
			if (nettingSystemDescription.IsEmpty)
			{
				NettingSystemDescriptionInfo.AddError(ResString.GetMultilingualString("BCFE500E-3F98-45A3-B879-05798A01C801", "Netting System Description"));
			}
		}

		ZString nettingSystemDescription;

		public ZPropertyInfo NettingSystemDescriptionInfo => GetZPropertyInfo(nameof(NettingSystemDescription));

		#endregion

		#region NettingCycleStartDate
		public ZDateTime NettingCycleStartDate
		{
			get { return nettingCycleStartDate; }
			set
			{
				if (value != nettingCycleStartDate)
				{
					SetNonPersistentPropertyValue(NettingCycleStartDateInfo, ref nettingCycleStartDate, value);
				}
				ValidateNettingSystemDescription();
			}
		}
		ZDateTime nettingCycleStartDate = new DateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 1); //first day of current month

		public ZPropertyInfo NettingCycleStartDateInfo => GetZPropertyInfo(nameof(NettingCycleStartDate));

		void ValidateNettingCycleStartDate()
		{
			NettingCycleStartDateInfo.ClearAllNotifications();
			if (NettingCycleStartDate.IsEmpty)
			{
				NettingCycleStartDateInfo.AddError(Res.GetString("4E6E174E-2C54-4F7C-B577-793B9C82E912", "Please pick a date which will be the starting date of the first Netting Cycle."));
			}
		}
		#endregion

		public ZBool IsSetupSuccessful { get; private set; }

		#region override

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateNettingSystemCompany();
			ValidateNettingSystemCode();
			ValidateNettingSystemDescription();
			ValidateNettingCycleStartDate();
		}

		#endregion

		#region Implementation

		public void SetupNetting()
		{
			try
			{
				using (var manager = Connection.BeginTransactionWithManager())
				{
					var nettingSystemPK = ZGuid.NewZGuid();

					if (CreateNettingSystem(nettingSystemPK) == 1)
					{
						CreateNettingPeriods(nettingSystemPK);
						SetupNettingCentreAndParticipants();
						SetupRegistrySettings();

						manager.CommitTransaction();
						IsSetupSuccessful = true;
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw;
			}
		}

		void SetupRegistrySettings()
		{
			var nettingControlAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, NettingControlAccountNumber));
			if (nettingControlAccount == null)
			{
				nettingControlAccount = Factory.New<AccGLHeader>();
				nettingControlAccount.AG_AccountNum = NettingControlAccountNumber;
				nettingControlAccount.AG_Description = NettingControlAccountName;
				nettingControlAccount.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				nettingControlAccount.AG_DebitCredit = Core.Constants.DebitCredit.Debit;
				nettingControlAccount.AG_ControlAccount = false;
				nettingControlAccount.AG_DisallowDirectPosting = false;
				nettingControlAccount.AG_Column = AccGLHeader.Constants.SectionTypes.Codes.TradingStatement;
			}

			var registry = ObjectFactory.Get<IAccounting>().Registry;
			registry.IsNettingSystem.SetValue(NettingSystemCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			registry.NettingControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, nettingControlAccount.PK.ToGuid());

			foreach (var company in SelectedCompanies)
			{
				registry.NettingParticipationStartDate.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, NettingCycleStartDate.ToDateTime());
				registry.NettingSystemOrganisation.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, NettingSystemCompany.GC_OH_OrgProxy.ToGuid());
			}

			RawDataRegistry.Instance.EHubTesting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();
		}

		void SetupNettingCentreAndParticipants()
		{
			using (var table = new DataTable())
			{
				table.Locale = CultureInfo.InvariantCulture;
				table.Columns.Add((NoResString)"Value", typeof(string));

				foreach (string value in ApplicableCompanyList.GetAllCodes())
				{
					if (value != NettingSystemCompanyCode)
					{
						table.Rows.Add(value);
					}
				}

				var rego = ObjectFactory.Get<IProductRegistration>();
				var registrationKey = rego.Key;
				var eHubID = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", registrationKey.EnterpriseCode, NettingSystemCompanyCode, registrationKey.ServerCode);

				using (var cmd = Connection.Command("SetupNettingCentre"))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@CompanyCode", SqlDbType.VarChar, NettingSystemCompanyCode.ToString());
					cmd.AddParameter("@eHubID", SqlDbType.VarChar, eHubID);
					cmd.AddParameter("@CurrentUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
					cmd.AddTableValuedParameter("@ParticipantCodes", TVPHelper.TVP_varchar, table);

					cmd.ExecuteNonQuery();
				}

				using (var cmd = Connection.Command("SetupNettingParticipants"))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@NettingCentreCompanyCode", SqlDbType.VarChar, NettingSystemCompanyCode.ToString());
					cmd.AddParameter("@NettingCentreEHubID", SqlDbType.VarChar, eHubID);
					cmd.AddParameter("@CurrentUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
					cmd.AddTableValuedParameter("@ParticipantCodes", TVPHelper.TVP_varchar, table);

					cmd.ExecuteNonQuery();
				}
			}
		}

		void CreateNettingPeriods(ZGuid nettingSystemPK)
		{
			for (int i = 0; i < 12; i++)
			{
				var description = string.Format(CultureInfo.InvariantCulture, (NoResString)"Cycle {0}", i + 1);
				using (var cmd = Connection.Command("CreateNettingPeriod"))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@NettingSystemPK", SqlDbType.UniqueIdentifier, nettingSystemPK.ToGuid());
					cmd.AddParameter("@StartDate", SqlDbType.DateTime, NettingCycleStartDate.AddMonths(i).ToDateTime());
					cmd.AddParameter("@Description", SqlDbType.VarChar, description);

					cmd.ExecuteNonQuery();
				}
			}
		}

		int CreateNettingSystem(ZGuid nettingSystemPK)
		{
			using (var cmd = Connection.Command("CreateNettingSystem"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, nettingSystemPK.ToGuid());
				cmd.AddParameter("@Code", SqlDbType.VarChar, NettingSystemCode.ToString());
				cmd.AddParameter("@Description", SqlDbType.VarChar, NettingSystemDescription.ToString());
				cmd.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, NettingSystemCompany.PK.ToGuid());

				return cmd.ExecuteProcedureWithReturnValue();
			}
		}

		DbConnection Connection
		{
			get { return connection ?? (connection = Db.Connection); }
		}
		protected DbConnection connection;

		public const string NettingControlAccountNumber = "8790.00.01";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const gl account name, can be edited afterwards")]
		const string NettingControlAccountName = "Netting Clearing Account";
		#endregion

	}
}
