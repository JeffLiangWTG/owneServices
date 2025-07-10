using System;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class IcelandForwardingShipmentSupport : CountrySpecificForwardingShipmentSupport
	{
		public const string CustomsOfficeCode = "COC";
		public static string ConsigneeChangedCause
		{
			get { return Res.GetString("b55a12be-74a1-40fa-be59-edbd46789605", "due to change of Consignee by user"); }
		}
		public static string OriginDestinationChangedCause
		{
			get { return Res.GetString("18cb8201-0b18-4f67-9be0-717eddb877a1", "due to change of Origin/Destination by user"); }
		}

		protected override ZString CountryCode
		{
			get { return Constants.CountryCodes.Iceland; }
		}

		protected override bool Apply()
		{
			return base.Apply() && IsIcelandBranch;
		}

		bool IsIcelandBranch
		{
			get { return GlbBranch.CurrentBranch.Country != null && CountryCode == GlbBranch.CurrentBranch.Country.Code; }
		}

		protected override void RegisterCore()
		{
			base.RegisterCore();

			SupportedBO.Factory.Saving += Factory_Saving;
			SupportedBO.ImportExportChanged += OnValueChangedToSetIcelandCustomsHouseCode;
			SupportedBO.ConsigneeDocumentaryAddressValueChanged += OnConsigneeChangedToSetIcelandCustomsHouseCode;
			SupportedBO.NumbersLoaded += Shipment_NumbersLoaded;
			SupportedBO.HasChangesChanged += Shipment_HasChangesChanged;
		}

		protected override void UnregisterCore()
		{
			base.UnregisterCore();

			SupportedBO.Factory.Saving -= Factory_Saving;
			SupportedBO.ImportExportChanged -= OnValueChangedToSetIcelandCustomsHouseCode;
			SupportedBO.ConsigneeDocumentaryAddressValueChanged -= OnConsigneeChangedToSetIcelandCustomsHouseCode;
			SupportedBO.NumbersLoaded -= Shipment_NumbersLoaded;
			SupportedBO.HasChangesChanged -= Shipment_HasChangesChanged;
		}

		#region Event Handlers

		void Shipment_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			IcelandCustomsOfficeCodeHelper.ProcessIcelandCustomsOfficeCode(SupportedBO.Numbers, FindIcelandCustomsOfficeCode(), Env.Security.MaintainShipmentCOCOverride.IsAllowed);
		}

		void Shipment_NumbersLoaded(object sender, EventArgs e)
		{
			IcelandCustomsOfficeCodeHelper.ProcessIcelandCustomsOfficeCode(SupportedBO.Numbers, FindIcelandCustomsOfficeCode(), Env.Security.MaintainShipmentCOCOverride.IsAllowed);
		}

		void OnValueChangedToSetIcelandCustomsHouseCode(object sender, EventArgs e)
		{
			IcelandCustomsOfficeCodeHelper.SetIcelandCustomsHouseCode(SupportedBO.Numbers, FindIcelandCustomsOfficeCode(), OriginDestinationChangedCause, Env.Security.MaintainShipmentCOCOverride.IsAllowed);
		}

		void OnConsigneeChangedToSetIcelandCustomsHouseCode(object sender, EventArgs e)
		{
			IcelandCustomsOfficeCodeHelper.SetIcelandCustomsHouseCode(SupportedBO.Numbers, FindIcelandCustomsOfficeCode(), ConsigneeChangedCause, Env.Security.MaintainShipmentCOCOverride.IsAllowed);
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			if (!SupportedBO.IsDeleted)
			{
				LogDocAddressChanges(SupportedBO.ConsigneeDocumentaryAddress, Res.GetString("498a4c93-b37f-4907-9e47-795551817bcf", "Consignee"));
				LogDocAddressChanges(SupportedBO.ConsignorDocumentaryAddress, Res.GetString("984348a4-d915-4493-9bb4-2dc12da0449b", "Consignor"));
			}
		}

		#endregion

		#region Previous Consignor/Consignee

		internal void LoadPreviousDocAddressValues(string name, ref bool loaded, ref OrgHeader company, ref ZString companyName)
		{
			if (!loaded && IsIcelandBranch)
			{
				company = null;
				companyName = ZString.Empty;
				loaded = true;
				string prefix = name;
				prefix += (NoResString)" changed:"; // Filter related
				ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
				query.AddToFilter(StmALogSchema.SL_IsCancelled, ZBool.False);
				query.AddToFilter(JoinCondition.And, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, prefix);
				query.OrderBy = StmALog.Schema.SL_EventTime + " DESC";
				query.MaximumRows = 2;
				var logs = SupportedBO.Logs.Find(query);
				if (logs != null && logs.Length > 1)
				{
					StmALog log = logs[1];
					Regex regex = new Regex(prefix + "(.*)<<cn>>:(.*)");
					Match match = regex.Match(log.SL_Reference);
					if (match.Success && match.Groups.Count > 2)
					{
						ZString logCompanyCode = match.Groups[1].Value;
						ZString logCompanyName = match.Groups[2].Value;
						if (!logCompanyCode.IsEmpty)
						{
							company = SupportedBO.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, logCompanyCode);
							if (company != null && !company.IsMiscellaneous)
							{
								companyName = company.OH_FullNameTruncated;
							}
						}

						if (companyName.IsEmpty)
						{
							companyName = logCompanyName;
						}
					}
				}
			}
		}

		void LogDocAddressChanges(JobDocAddress docAddress, string name)
		{
			if (docAddress.HasChanges && IsIcelandBranch)
			{
				ZGuid oldOrgPk = ZGuid.Empty;
				ZGuid newOrgPk = ZGuid.Empty;
				ZString newOrgCode = ZString.Empty;
				if (docAddress.HasRealOrganisation)
				{
					newOrgPk = docAddress.OrganisationPK;
					newOrgCode = docAddress.Organisation.OH_Code;
				}

				bool isOrgChanged = false;
				if (!docAddress.IsInDatabase)
				{
					isOrgChanged = true;
				}
				else if (docAddress.E2_OA_AddressInfo.HasChanges)
				{
					if (docAddress.E2_OA_AddressInfo.OriginalValue != null)
					{
						ZGuid oldAddressPk = (ZGuid)docAddress.E2_OA_AddressInfo.OriginalValue;
						if (oldAddressPk.IsValid && !oldAddressPk.IsEmpty)
						{
							var oldAddress = SupportedBO.Factory.Load<OrgAddress>(oldAddressPk);
							if (oldAddress != null)
							{
								oldOrgPk = oldAddress.OA_OH;
							}
						}
					}

					if (oldOrgPk != newOrgPk)
					{
						isOrgChanged = true;
					}
				}

				bool companyNameOverridden = !docAddress.HasRealOrganisation && docAddress.E2_AddressOverride &&
					(docAddress.E2_CompanyNameInfo.HasChanges || !docAddress.IsInDatabase);

				if (isOrgChanged || companyNameOverridden)
				{
					StringBuilder strBuilder = new StringBuilder();
					strBuilder.Append(name).Append(" " + Res.GetString("163b5cee-eeb4-4869-bf43-144431f8253c", "changed:"));
					if (!newOrgCode.IsEmpty)
					{
						strBuilder.Append(newOrgCode);
					}

					strBuilder.Append((NoResString)"<<cn>>:"); // This is a code
					if (companyNameOverridden && !docAddress.E2_CompanyName.IsEmpty)
					{
						strBuilder.Append(docAddress.E2_CompanyNameTruncated);
					}

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					SupportedBO.Logs.AddNew(Events.EditedARecord, strBuilder.ToString());
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

					SupportedBO.SetPreviousConsignorConsigneeAsNotLoaded();
				}
			}
		}

		#endregion

		#region COC

		ZString FindIcelandCustomsOfficeCode()
		{
			RefUNLOCO relevantLoco = null;
			if (SupportedBO.IsImport() && SupportedBO.Consignee != null)
			{
				relevantLoco = SupportedBO.Consignee.UNLOCO;
			}
			else if (SupportedBO.IsExport() && SupportedBO.DepartureConsol != null)
			{
				relevantLoco = SupportedBO.DepartureConsol.LoadPort;
			}

			return relevantLoco != null ? relevantLoco.RefLocoMaps.LocalCodeForUsageAndCountry(ISLocoMapSystemUsageList.Codes.CustomsOfficeCode,
				Core.Constants.CountryCodes.Iceland) : ZString.Empty;
		}

		internal ZString GetPreviousCOC(ForwardingShipment shipment)
		{
			ZString result = ZString.Empty;

			if (IsIcelandBranch)
			{
				string prefix = (NoResString)"COC: From";                 // Filter related
				ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
				query.AddToFilter(StmALogSchema.SL_IsCancelled, ZBool.False);
				query.AddToFilter(JoinCondition.And, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, prefix);
				query.OrderBy = StmALog.Schema.SL_EventTime + " DESC";

				CusEntryNumber cusEntryNumber = IcelandCustomsOfficeCodeHelper.FindIcelandCustomsHouseEntry(shipment.Numbers);
				if (cusEntryNumber != null)
				{
					StmALog[] logs = cusEntryNumber.Logs.Find(query);
					if (logs != null && logs.Length > 0)
					{
						StmALog log = logs[0];
						Regex regex = new Regex("^COC: From <(\\w*)> to <\\w*>.*$");
						Match match = regex.Match(log.SL_Reference);

						if (match.Success && match.Groups.Count > 1)
						{
							result = match.Groups[1].Value;
						}
					}
				}
			}

			return result;
		}

		#endregion
	}
}
