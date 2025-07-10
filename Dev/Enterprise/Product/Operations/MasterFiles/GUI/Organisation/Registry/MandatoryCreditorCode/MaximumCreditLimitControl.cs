using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.MasterFiles.GUI
{
	public partial class MaximumCreditLimitControl : RegistryZUserControl
	{
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Only english words")]
		const string CreditLimitReference = "Maximum Credit Limit Exceeded";
		const string CreditLimitType = "CRL";
		const string CreditLimitReason = "MCL";

		protected virtual int BatchSize => 100;

		public MaximumCreditLimitControl()
		{
			InitializeComponent();
			ActionLabel.CaptionResourceString = Res.GetData("AD3B309F-46A9-47E0-8CBC-6FE4BBAAE47E", @"Apply mandatory Credit Report purchase to all Organizations that exceed Credit Limit set in Maximum Credit Limit registry.
- This can be a time-consuming process.
- This process cannot be reversed.

If this process is run during business hours on large system it can potentially slow operational processing.
The safest approach is to run out of business hours.");
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			MaximumCreditLimitRegistryGrid.ReadOnly = readOnly;
		}

		MaximumCreditLimitCollection BoundCollection => (MaximumCreditLimitCollection)DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			ApplyToAllOrganizationsButton.Enabled = false;
			BoundCollection.HasChangesChanged += BoundBusinessObject_HasChangesChanged;
			SetupGrid();
		}

		void SetupGrid()
		{
			if (BoundCollection?.CurrentFallbackLevel?.Level == RegistryStorageFlags.Company)
			{
				MaximumCreditLimitRegistryGrid.MaximumRows = 1;
				MaximumCreditLimitRegistryGrid.Columns[MaximumCreditLimitItem.Schema.CurrencyPK].ColumnStyle.ReadOnly = true;
			}
		}

		void BoundBusinessObject_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (BoundCollection.HasChanges)
			{
				ApplyToAllOrganizationsButton.Enabled = BoundCollection.Count > 0;
			}
		}

		void ApplyToAllOrganizationsButton_Click(object sender, EventArgs e)
		{
			ApplyToAllOrganizationsButton.Enabled = false;

			if (BoundCollection.HasChanges)
			{
				Globals.Message.Show(Res.GetString("32BF5538-B65D-4EEA-8651-920DC2F1E982", "Please save the data before performing credit limit check."));
			}
			else
			{
				var ohPkGcCodeDictionary = new Dictionary<Guid, List<string>>();
				var ohPks = new List<Guid>();
				var dataTable = CreateDataTable();

				var currentFallbackLevel = BoundCollection.CurrentFallbackLevel;
				if (currentFallbackLevel != null && currentFallbackLevel.Level == RegistryStorageFlags.Company)
				{
					if (BoundCollection != null && BoundCollection.Count > 0 && BoundCollection.Cast<MaximumCreditLimitItem>().Any(x => !string.IsNullOrEmpty(x.CreditLimit)))
					{
						PopulateDataTable(dataTable, currentFallbackLevel.CompanyPK(false), (MaximumCreditLimitItem)BoundCollection.First());
					}
				}
				else
				{
					var registryItem = OrganisationRegistry.Instance.MaximumCreditLimit;
					var companies = new GlbCompanyCollection(new BusinessObjectFactory(), new ZQuery(GlbCompanySchema.GC_IsActive, true)).Where(x => !x.IsDemoCompany);
					foreach (var company in companies)
					{
						var companyPk = company.PK.ToGuid();
						var companyCreditLimitCollection = registryItem.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty);
						if (companyCreditLimitCollection.Count > 0)
						{
							PopulateDataTable(dataTable, companyPk, (MaximumCreditLimitItem)companyCreditLimitCollection.First());
						}
						else
						{
							var registryItemTag = new RegistryItemTag(registryItem);
							var companyFallbackLevel = new FallbackLevel(companyPk, Guid.Empty, Guid.Empty);
							registryItemTag.SetFallback(companyFallbackLevel);
							if (!registryItemTag.HasValue)
							{
								var systemCreditLimitItem = BoundCollection.Cast<MaximumCreditLimitItem>().FirstOrDefault(x => x.CurrencyPK == company.LocalCurrency.PK);
								if (systemCreditLimitItem != null)
								{
									PopulateDataTable(dataTable, companyPk, systemCreditLimitItem);
								}
							}
						}
					}
				}

				var sqlCommand = @"
SELECT OB_OH, GC_Code
FROM dbo.OrgCompanyData
JOIN dbo.GlbCompany ON OB_GC = GC_PK
JOIN @CompanyCreditLimitInfo ON GC_PK = CompanyPK AND GC_RX_NKLocalCurrency = LocalCurrency
WHERE OB_ARCreditLimit > CreditLimit
";

				using (var command = Db.Connection.Command(sqlCommand))
				{
					command.AddTableValuedParameter("@CompanyCreditLimitInfo", "dbo.TVP_CompanyLevelCreditLimitInfo", dataTable);
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var ohPk = (Guid)reader[0];

							if (ohPkGcCodeDictionary.ContainsKey(ohPk))
							{
								ohPkGcCodeDictionary[ohPk].Add((string)reader[1]);
							}
							else
							{
								ohPks.Add(ohPk);
								ohPkGcCodeDictionary.Add(ohPk, new List<string> { (string)reader[1] });
							}
						}
					}
				}

				var countOfAllEvents = ohPks.Count;

				if (countOfAllEvents > 0)
				{
					var factoryProvider = new BusinessObjectFactoryProvider();

					using (var progressForm = new ProgressForm())
					{
						progressForm.ShowCancelButton = false;
						progressForm.Show();

						while (ohPks.Count > 0)
						{
							var orgHeaders = factoryProvider.Current.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, ohPks.Take(BatchSize)));

							foreach (var orgHeader in orgHeaders)
							{
								AddCreditReportEvent(orgHeader, ohPkGcCodeDictionary[orgHeader.PK.ToGuid()]);
							}

							factoryProvider.SaveCurrentReclaimMemoryAndCreateNew();

							ohPks = ohPks.Skip(BatchSize).ToList();

							progressForm.SetStatusAndPercentComplete(Res.GetString("F2E85A61-C1B2-43B7-B0A1-86B666B4BF0A", "There are {0} more events to be added.", ohPks.Count), (countOfAllEvents - ohPks.Count) * 100 / countOfAllEvents);
						}
					}

					Globals.Message.Show(Res.GetString("FDF2BAA0-901F-46D7-A298-E0AE1784BED5", "A total of {0} event(s) for organization exceeding credit limit are created.", countOfAllEvents));
				}
				else
				{
					Globals.Message.Show(Res.GetString("9DA7A723-B998-4C02-82FE-0BEFD68DC76C", "No organization has exceeded credit limit."));
				}
			}
		}

		DataTable CreateDataTable()
		{
			var dataTable = new DataTable();
			dataTable.Columns.Add("CompanyPK", typeof(Guid));
			dataTable.Columns.Add("LocalCurrency", typeof(string));
			dataTable.Columns.Add("CreditLimit", typeof(decimal));

			return dataTable;
		}

		void PopulateDataTable(DataTable dataTable, Guid companyPk, MaximumCreditLimitItem item)
		{
			var row = dataTable.NewRow();
			row["CompanyPK"] = companyPk;
			row["LocalCurrency"] = item.Currency.Code;
			row["CreditLimit"] = Convert.ToDecimal(item.CreditLimit);
			dataTable.Rows.Add(row);
		}

		[SuppressMessage("Performance", "CA1845:Use span-based 'string.Concat' and 'AsSpan' instead of 'Substring'")]
		static void AddCreditReportEvent(BusinessObject businessObject, IEnumerable<string> companyCodeList)
		{
			var companyCodes = string.Join(", ", companyCodeList);

			var logParameters = new Dictionary<string, string>
			{
				[Params.Codes.Type] = CreditLimitType,
				[Params.Codes.Reason] = CreditLimitReason,
				[Params.Codes.Company] = companyCodes.Length > 800 ? companyCodes.Substring(0, 800) + "..." : companyCodes
			};

			businessObject.GetLogs().AddNew(AutoEvents.CreditCheckEvent, CreditLimitReference, logParameters.ToArray());
		}
	}
}
