using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Services.Calendar;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using StatusList = Enterprise.MasterFiles.Business.CollectionNoteStatusList;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(AutoOrgCollectionNote.Schema.PN_CallDetailNote)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class OrgCollectionNote : AutoOrgCollectionNote, IDocManagerSupport
	{
		public OrgCollectionNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New Properties

		#region Overdue Amount

		ZDecimal OverdueAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (Header != null)
				{
					string sqlText = @"SELECT SUM(AH_OutstandingAmount) as OverdueAmount 
									   FROM dbo.AccTransactionHeader  
  									  WHERE AH_IsCancelled <> 1
 									   AND AH_OH = @OrgHeader
									   AND AH_Ledger = @Ledger
 									   AND AH_GC = @Company
									   AND AH_TransactionType in (@INV,@CRD,@ADJ,@JNL)
 									   AND AH_DueDate < @TodayDate";

					DynamicBusinessObjectCollection query = new DynamicBusinessObjectCollection(Factory);
					ZSqlParameterCollection @params = new ZSqlParameterCollection();
					@params.Add("@OrgHeader", Header.PK.ToGuid(), AccTransactionHeaderSchema.AH_OH);
					@params.Add("@Ledger", ZArchitecture.Core.LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
					@params.Add("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranchSchema.GB_GC);
					@params.Add("@TodayDate", Env.Time.CurrentLocalDate, AccTransactionHeaderSchema.AH_DueDate);
					@params.Add("@INV", ZArchitecture.Core.TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionType);
					@params.Add("@CRD", ZArchitecture.Core.TransactionTypes.CreditNote, AccTransactionHeaderSchema.AH_TransactionType);
					@params.Add("@ADJ", ZArchitecture.Core.TransactionTypes.AdjustmentNote, AccTransactionHeaderSchema.AH_TransactionType);
					@params.Add("@JNL", ZArchitecture.Core.TransactionTypes.Journal, AccTransactionHeaderSchema.AH_TransactionType);
					query.Load(sqlText, @params);

					result = new ZDecimal(query[0][nameof(OverdueAmount)]);
				}

				return result;
			}
		}

		#endregion

		#region Outstanding Amount

		ZDecimal OutstandingAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (Header != null)
				{
					string sqlText = @"SELECT SUM(AH_OutstandingAmount) as OutstandingAmount 
                                       FROM dbo.AccTransactionHeader 
                                      WHERE AH_OH = @OrgHeader
                                       AND AH_Ledger = @Ledger
                                       AND AH_TransactionType != @TransactionType
                                       AND AH_GC = @Company";

					DynamicBusinessObjectCollection query = new DynamicBusinessObjectCollection(Factory);
					ZSqlParameterCollection @params = new ZSqlParameterCollection();
					@params.Add("@OrgHeader", Header.PK.ToGuid(), AccTransactionHeaderSchema.AH_OH);
					@params.Add("@Ledger", ZArchitecture.Core.LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
					@params.Add("@TransactionType", TransactionTypes.InvoiceBatch, AccTransactionHeaderSchema.AH_TransactionType);
					@params.Add("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranchSchema.GB_GC);
					query.Load(sqlText, @params);

					result = new ZDecimal(query[0][nameof(OutstandingAmount)]);
				}

				return result;
			}
		}

		#endregion

		#region Telephone Number

		[ReadOnly(true)]
		public ZString TelephoneNumber
		{
			get
			{
				ZString result = ZString.Empty;

				if (Contact != null)
				{
					result = Contact.PhoneFallbackToOrganisation;
				}
				else if (Header != null && Header.MainAddress != null)
				{
					result = Header.MainAddress.OA_Phone;
				}

				return result;
			}
		}

		public ZPropertyInfo TelephoneNumberInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(TelephoneNumber), "Telephone Number");
			}
		}

		#endregion

		#region Email Address

		[ReadOnly(true)]
		public ZString EmailAddress
		{
			get
			{
				ZString result = ZString.Empty;

				if (Contact != null)
				{
					result = Contact.EmailFallbackToOrganisation;
				}
				else if (Header != null && Header.MainAddress != null)
				{
					result = Header.MainAddress.OA_Email;
				}

				return result;
			}
		}

		public ZPropertyInfo EmailAddressInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(EmailAddress), "Email Address");
			}
		}

		#endregion

		#region Header

		public OrgHeader Header
		{
			get
			{
				if (fHeader == null && IsInDatabase && PN_OB.IsValid)
				{
					OrgCompanyData companyData = Factory.Load<OrgCompanyData>(this.PN_OB);

					if (companyData != null)
					{
						fHeader = Factory.Load<OrgHeader>(companyData.OB_OH);
					}
				}

				return fHeader;
			}
			set
			{
				fHeader = value;

				if (fHeader != null)
				{
					if (PN_OB.IsEmpty && Header != null && Header.CompanyData != null)
					{
						PN_OB = Header.CompanyData.PK;
					}

					SetDefaultContact();
				}

				if (!IsInDatabase)
				{
					PN_TotalOutstandingValueAtCallTime = OutstandingAmount;
					PN_AmountOverdueAtCallTime = OverdueAmount;
				}

				PN_OCInfo.RefreshBinding();
			}
		}
		OrgHeader fHeader;

		internal void SetDefaultContact()
		{
			if (PN_OC.IsEmpty && Header.CollectionNotes.Count > 0)
			{
				PN_OC = Header.CollectionNotes[Header.CollectionNotes.Count - 1].PN_OC;
			}
		}

		#endregion

		#region Calling Sales Rep

		GlbStaff CallingSalesRep
		{
			get { return (GlbStaff)Factory.LoadFromUniqueKey(typeof(GlbStaff), GlbStaffSchema.GS_Code, PN_SystemCreateUser); }
		}

		#endregion

		#endregion

		#region Business Object Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase)
			{
				if (PN_SystemCreateUser.IsEmpty)
				{
					PN_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
				}
				if (PN_SystemCreateTimeUtc.IsEmpty)
				{
					PN_SystemCreateTimeUtc = Factory.TransactionStartedTime;
				}
			}
		}

		[List("Lookups.StaffMembers")]
		public override ZString PN_SystemCreateUser
		{
			get { return base.PN_SystemCreateUser; }
			set
			{
				base.PN_SystemCreateUser = value;

				if (!value.IsEmpty && PN_Status == StatusList.Codes.Open)
				{
					base.PN_Status = StatusList.Codes.Working;
				}
			}
		}

		[List("Lookups.CallStatusList")]
		public override ZString PN_Status
		{
			get { return base.PN_Status; }
			set
			{
				if (PN_Status == StatusList.Codes.Open && value == StatusList.Codes.Working)
				{
					base.PN_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
				}

				base.PN_Status = value;
			}
		}

		[List("Lookups.CallDispositionList")]
		public override ZString PN_CallDisposition
		{
			get
			{
				return base.PN_CallDisposition;
			}
			set
			{
				base.PN_CallDisposition = value;
			}
		}

		[List("Lookups.DependentContacts")]
		public override ZGuid PN_OC
		{
			get
			{
				return base.PN_OC;
			}
			set
			{
				base.PN_OC = value;
			}
		}

		#region Decimal Places

		public int DecimalPlaces => GlbCompany.CurrentCompany.GetLocalDecimals();

		#endregion

		[ReadOnly(true)]
		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal PN_AmountOverdueAtCallTime
		{
			get { return base.PN_AmountOverdueAtCallTime; }
			set { base.PN_AmountOverdueAtCallTime = value; }
		}

		[ReadOnly(true)]
		[DecimalPlaces(nameof(DecimalPlaces))]
		public override ZDecimal PN_TotalOutstandingValueAtCallTime
		{
			get { return base.PN_TotalOutstandingValueAtCallTime; }
			set { base.PN_TotalOutstandingValueAtCallTime = value; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			PN_SystemCreateTimeUtc = ZDateTime.Now;
			base.PN_Status = StatusList.Codes.Open;

			if (ObjectFactory.Get<IAccounting>().CollectionCallFollowUpDays > 0)
			{
				PN_CallBackDate = ZDateTime.Today.AddDays(ObjectFactory.Get<IAccounting>().CollectionCallFollowUpDays);
			}

			HasChanges = true;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			OriginalPN_CallBackDate = PN_CallBackDate;
		}

		ZDateTime OriginalPN_CallBackDate;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			bool result = false;

			switch (property.Name)
			{
				case OrgCollectionNoteSchema.Constants.PN_CallDisposition:
				case OrgCollectionNoteSchema.Constants.PN_CallBackDate:
				case OrgCollectionNoteSchema.Constants.PN_CallDetailNote:
				case OrgCollectionNoteSchema.Constants.PN_SystemCreateTimeUtc:
				case OrgCollectionNoteSchema.Constants.PN_SystemCreateUser:
				case OrgCollectionNoteSchema.Constants.PN_OC:
					result = !Env.Security.ReceivablesCollectionCallsEdit.IsAllowed || (IsInDatabase && PN_Status == StatusList.Codes.Closed);
					break;

				case OrgCollectionNoteSchema.Constants.PN_Status:
					result = !Env.Security.ReceivablesCollectionCallsEdit.IsAllowed;
					break;
			}

			result = result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return result;
		}

		#endregion

		#region Calendar Reminders

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded && ShouldCreateReminder)
			{
				NextCallDueReminder.CreateAppointment();
			}
		}

		internal bool ShouldCreateReminder
		{
			get { return ObjectFactory.Get<IAccounting>().ShouldCollectionCallCreateFollowUpAppointments && PN_CallBackDate != OriginalPN_CallBackDate && (PN_CallBackDate.IsValid || OriginalPN_CallBackDate.IsValid) && CallingSalesRep != null; }
		}

		internal Reminder NextCallDueReminder
		{
			get
			{
				ZStringBuilder body = new ZStringBuilder();

				body.Append(Res.GetString("1c96688a-dbda-4c5e-8cb5-56f49749f883", "Follow up collection call due for client {0} ({1})", Header.OH_FullNameTruncated, Header.OH_Code));
				body.Append(ZString.Empty);
				body.Append(Res.GetString("9c29ba42-ae30-4b35-9548-91f5ae83ed23", "CONTACT DETAILS:"));

				if (Contact != null)
				{
					body.Append(Res.GetString("2464597f-26c5-46c9-a279-6f91cccee264", "Name: {0}", Contact.OC_ContactName));
					body.Append(Res.GetString("678476b7-1fb2-4c0d-ab12-87f6fc4c461c", "Email: {0}", Contact.EmailFallbackToOrganisation));
					body.Append(Res.GetString("8bfd7a3e-55b9-4084-b7c3-138646678351", "Fax: {0}", Contact.FaxFallbackToOrganisation));
					body.Append(Res.GetString("b42b30e9-a15a-47ec-ba6e-092fbec8474f", "Phone: {0}", Contact.PhoneFallbackToOrganisation));
					body.Append(ZString.Empty);
				}

				if (Header.Addresses.MainAddress != null)
				{
					body.Append(Header.Addresses.MainAddress.OA_Address1);

					if (!Header.Addresses.MainAddress.OA_Address2.IsEmpty)
					{
						body.Append(Header.Addresses.MainAddress.OA_Address2);
					}

					ZString addressLine3 = Header.Addresses.MainAddress.OA_City;
					addressLine3 += " " + Header.Addresses.MainAddress.OA_State;
					addressLine3 += " " + Header.Addresses.MainAddress.OA_PostCode;
					body.Append(addressLine3);

					body.Append(Res.GetString("678476b7-1fb2-4c0d-ab12-87f6fc4c461c", "Email: {0}", Header.Addresses.MainAddress.OA_Email));
					body.Append(Res.GetString("8bfd7a3e-55b9-4084-b7c3-138646678351", "Fax: {0}", Header.Addresses.MainAddress.OA_Fax));
					body.Append(Res.GetString("b42b30e9-a15a-47ec-ba6e-092fbec8474f", "Phone: {0}", Header.Addresses.MainAddress.OA_Phone));
					body.Append(ZString.Empty);
				}

				body.Append(Res.GetString("1a5af959-6202-45cc-86c9-146c13fd3d81", "CALL DETAILS:"));
				body.Append(Res.GetString("930fed7f-dd9a-4738-8ba2-e9daadddaa7d", "Call Status:") + " " + Lookups.CallStatusList.GetDescriptionFromCode(PN_Status));
				body.Append(Res.GetString("7a492b91-2d61-4297-9b0f-bf65aef76f3d", "Call Disposition:") + " " + Lookups.CallDispositionList.GetDescriptionFromCode(PN_CallDisposition));
				body.Append(Res.GetString("07a283a2-49b1-4870-ab55-699c5f64fa06", "Amount Overdue at Call Time:") + " " + PN_AmountOverdueAtCallTime.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals));
				body.Append(Res.GetString("e38f5858-bf80-4e6f-9abe-c3223b6f440f", "Total Outstanding Value at Call Time:") + " " + PN_TotalOutstandingValueAtCallTime.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals));
				body.Append(ZString.Empty);

				body.Append(Res.GetString("9a10db71-5d62-47cd-a947-269b8050e0fe", "INTERNAL CALL NOTES:"));
				body.Append(PN_CallDetailNote.Substring(0, 1000));
				body.Append(ZString.Empty);

				var reminderTime = PN_CallBackDate.IsValid ? PN_CallBackDate : OriginalPN_CallBackDate;
				var reminderType = PN_CallBackDate.IsValid ? ReminderType.Confirmed : ReminderType.Cancellation;
				var htmlBody = string.Format(CultureInfo.InvariantCulture, "<HTML><HEAD><TITLE></TITLE></HEAD><BODY>{0}</BODY></HTML>", body.ToStringWithNewLineBetweenAppends());
				var result = new Reminder(PK.ToString() + "FollowUpCall", Header.PK, new ZString(Header.TableName), DateTimeKind.Local, reminderTime, reminderTime, Res.GetString("56099c86-f444-41c9-8537-37e6cc127a98", "Follow up Collection Call for client {0}", Header.OH_FullName),
					body.ToStringWithNewLineBetweenAppends(),
					htmlBody)
				{
					ReminderType = reminderType
				};

				if (CallingSalesRep != null)
				{
					result.Recipients.Add(CallingSalesRep.GS_FullName, CallingSalesRep.GS_EmailAddress);
				}

				return result;
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, "CLN");
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion
	}
}
