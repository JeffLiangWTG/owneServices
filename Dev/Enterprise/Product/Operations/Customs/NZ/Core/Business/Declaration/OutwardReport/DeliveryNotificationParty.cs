using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[SystemDefinedValues]
	public class DeliveryNotificationParty : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DeliveryNotificationParty(BusinessObject parent)
			: base(parent.Factory)
		{
			Parent = parent;
		}

		public static class Schema
		{
			public const string E2_OA_DeliveryNotificationParty = "E2_OA_DeliveryNotificationParty";
			public const string DeliveryNotificationPartyName = "DeliveryNotificationPartyName";
			public const string DeliveryNotificationPartyEmail = "DeliveryNotificationPartyEmail";
			public const string DeliveryNotificationPartyPort = "DeliveryNotificationPartyPort";
		}

		public BusinessObject Parent { get; }

		public DeliveryNotificationPartyValidation Validation => validation ?? (validation = new DeliveryNotificationPartyValidation(this));
		DeliveryNotificationPartyValidation validation;

		#region Override

		protected override ZGuid GetPK()
		{
			return Parent != null ? Parent.PK : ZGuid.Empty;
		}

		protected override void SetPKAndDefaults()
		{
		}

		public override string TablePrefix => Parent.TablePrefix;

		public override bool IsInDatabase
		{
			get { return Parent.IsInDatabase; }
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		#endregion

		#region DeliveryNotificationParty

		[List(nameof(Lookups) + "." + nameof(DeliveryNotificationPartyLookups.NotifyParty_List))]
		public ZGuid E2_OA_DeliveryNotificationParty
		{
			get { return this.GetSystemDefinedValue<ZGuid>(Schema.E2_OA_DeliveryNotificationParty); }
			set
			{
				var partiesWereBlank = AllPartiesAreBlank;
				this.SetSystemDefinedValue(Schema.E2_OA_DeliveryNotificationParty, value);
				Validate(partiesWereBlank, E2_OA_DeliveryNotificationParty.IsEmpty, Validation.ValidateOutwardReportNotificationParty);
				E2_OA_DeliveryNotificationPartyInfo.RefreshBinding();
			}
		}

		#region ZAddress

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress E2_OA_DeliveryNotificationParty_ZAddress
		{
			get
			{
				if (fE2_OA_DeliveryNotificationParty_ZAddress == null)
				{
					fE2_OA_DeliveryNotificationParty_ZAddress = GetNewDeliveryNotificationParty_ZAddress();
				}
				return fE2_OA_DeliveryNotificationParty_ZAddress;
			}
		}
		ZAddress fE2_OA_DeliveryNotificationParty_ZAddress;

		#endregion

		public ZPropertyInfo E2_OA_DeliveryNotificationPartyInfo
		{
			get { return GetZPropertyInfo(Schema.E2_OA_DeliveryNotificationParty); }
		}

		protected ZAddress GetNewDeliveryNotificationParty_ZAddress()
		{
			var result = new ZAddress(E2_OA_DeliveryNotificationPartyInfo);
			result.GetDefaultAddress = header => GetMainAddressPK(header as OrgHeader);
			return result;
		}

		ZGuid GetMainAddressPK(OrgHeader header)
		{
			return header != null ? header.MainAddress.PK : ZGuid.Empty;
		}

		#endregion

		#region DeliveryNotificationPartyName

		[MaxLength(70)]
		public ZString DeliveryNotificationPartyName
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.DeliveryNotificationPartyName); }
			set
			{
				var hasChanges = value != DeliveryNotificationPartyName;
				if (hasChanges)
				{
					var partiesWereBlank = AllPartiesAreBlank;
					CheckMaximumLength(DeliveryNotificationPartyNameInfo, value);
					this.SetSystemDefinedValue(Schema.DeliveryNotificationPartyName, value);
					Validate(partiesWereBlank, DeliveryNotificationPartyName.IsEmpty, Validation.ValidateOutwardReportNotificationPartyName, Validation.ValidateOutwardReportNotificationPartyEmail);
				}

				DeliveryNotificationPartyNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeliveryNotificationPartyNameInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryNotificationPartyName); }
		}

		#endregion

		#region DeliveryNotificationPartyEmail

		[MaxLength(50)]
		public ZString DeliveryNotificationPartyEmail
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.DeliveryNotificationPartyEmail); }
			set
			{
				var hasChanges = value != DeliveryNotificationPartyEmail;
				if (hasChanges)
				{
					var partiesWereBlank = AllPartiesAreBlank;
					CheckMaximumLength(DeliveryNotificationPartyEmailInfo, value);
					this.SetSystemDefinedValue(Schema.DeliveryNotificationPartyEmail, value);
					Validate(partiesWereBlank, DeliveryNotificationPartyEmail.IsEmpty, Validation.ValidateOutwardReportNotificationPartyEmail, Validation.ValidateOutwardReportNotificationPartyName);
				}

				DeliveryNotificationPartyEmailInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeliveryNotificationPartyEmailInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryNotificationPartyEmail); }
		}

		#endregion

		#region DeliveryNotificationPartyPort

		[MaxLength(5)]
		[List(nameof(Lookups) + "." + nameof(DeliveryNotificationPartyLookups.DeliveryNotificationPartyPorts))]
		public ZString DeliveryNotificationPartyPort
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.DeliveryNotificationPartyPort); }
			set
			{
				var hasChanges = value != DeliveryNotificationPartyPort;
				if (hasChanges)
				{
					var partiesWereBlank = AllPartiesAreBlank;
					CheckMaximumLength(DeliveryNotificationPartyPortInfo, value);
					this.SetSystemDefinedValue(Schema.DeliveryNotificationPartyPort, value);
					Validate(partiesWereBlank, DeliveryNotificationPartyPort.IsEmpty, Validation.ValidateOutwardReportNotificationPartyPort);
				}

				DeliveryNotificationPartyPortInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeliveryNotificationPartyPortInfo
		{
			get { return GetZPropertyInfo(Schema.DeliveryNotificationPartyPort); }
		}

		#endregion

		void Validate(bool allPartiesWereBlank, bool thisPartyIsBlank, params System.Action[] validationFuncs)
		{
			ClearAllPartiesAreBlank();

			if ((AllPartiesAreBlank && !allPartiesWereBlank) || (allPartiesWereBlank && !thisPartyIsBlank))
			{
				Validation.ValidateAll();
			}
			else
			{
				validationFuncs?.ForEach(f => f.Invoke());
			}
		}

		internal bool AllPartiesAreBlank
		{
			get
			{
				if (!allPartiesAreBlank.HasValue)
				{
					allPartiesAreBlank = E2_OA_DeliveryNotificationParty.IsEmpty && DeliveryNotificationPartyPort.IsEmpty && DeliveryNotificationPartyName.IsEmpty && DeliveryNotificationPartyEmail.IsEmpty;
				}
				return allPartiesAreBlank.Value;
			}
		}
		bool? allPartiesAreBlank;

		internal void ClearAllPartiesAreBlank() => allPartiesAreBlank = null;

		public DeliveryNotificationPartyLookups Lookups => lookups ?? (lookups = new DeliveryNotificationPartyLookups(this));
		DeliveryNotificationPartyLookups lookups;
	}
}
