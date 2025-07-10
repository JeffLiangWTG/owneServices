using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.eManifest.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.NumberFountain;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eManifest.Business
{
	[UniversalDataContext(DataContextType.eManifest)]
	public class SupplierBookingHeader : AutoSupplierBookingHeader, ISupplierBookingHeader, IJobNumber, IDocumentSupportable, IValidateForCustomsMessagingSupporter
	{
		public SupplierBookingHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BookingLines

		[ChildEditable]
		public SupplierBookingLineDependentCollection BookingLines
		{
			get { return bookingLines ?? (bookingLines = GetNewBookingLinesCollection()); }
		}

		SupplierBookingLineDependentCollection bookingLines;

		SupplierBookingLineDependentCollection GetNewBookingLinesCollection()
		{
			var result = new SupplierBookingLineDependentCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		#endregion

		#region GS1 Prefix

		/// <summary>
		/// If a booking line's consignment reference number is empty it will be generated using a SSCC reference, which requires a GS1 Prefix.
		/// If the booking header's consignor has no a GS1 prefix, fall back to the current company's org proxy.
		/// </summary>
		internal ZString GS1Prefix
		{
			get
			{
				InitialiseGS1IfRequired();
				return gs1Prefix;
			}
		}

		internal INumberFountainProxy GS1Fountain
		{
			get
			{
				InitialiseGS1IfRequired();
				return gs1Fountain;
			}
		}

		void InitialiseGS1IfRequired()
		{
			if (!hasInitialisedGS1)
			{
				hasInitialisedGS1 = true;
				var gs1OrgPK = ZGuid.Empty;
				gs1Fountain = null;

				if (Consignor != null)
				{
					var consignorPrefix = GetGS1PrefixFromOrg(Consignor.Header, Consignor.PK);
					if (SSCCBarCodeChecker.IsSSCCBarCodePrefix(consignorPrefix))
					{
						gs1Prefix = consignorPrefix;
						gs1OrgPK = Consignor.OA_OH;
					}
				}

				if (gs1Prefix.IsEmpty)
				{
					var orgProxyPrefix = GetGS1PrefixFromOrg(Env.CurrentCompany.OrganisationPK, ZGuid.Empty);
					if (SSCCBarCodeChecker.IsSSCCBarCodePrefix(orgProxyPrefix))
					{
						gs1Prefix = orgProxyPrefix;
						gs1OrgPK = Env.CurrentCompany.OrganisationPK;
					}
				}

				var org = Factory.Load<OrgHeader>(gs1OrgPK);
				if (org != null)
				{
					gs1Fountain = org.OrgFountains.TryGetNumberFountain(OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers, GS1Prefix) ?? Env.NumberFountains.SSCCBarCode(GS1Prefix);
				}
			}
		}

		ZString gs1Prefix;
		INumberFountainProxy gs1Fountain;
		bool hasInitialisedGS1;

		ZString GetGS1PrefixFromOrg(ZGuid orgPK, ZGuid preferredOrgAddressPK)
		{
			return GetGS1PrefixFromOrg(Factory.Load<OrgHeader>(orgPK), preferredOrgAddressPK);
		}

		ZString GetGS1PrefixFromOrg(OrgHeader orgHeader, ZGuid preferredOrgAddressPK)
		{
			if (orgHeader != null)
			{
				var gs1Codes = orgHeader.CustomsCodes.Find(CustomsCodeQuery).Cast<OrgCusCode>().ToArray();

				if (!gs1Codes.Any())
				{
					return ZString.Empty;
				}

				return gs1Codes.Length == 1 ? gs1Codes[0].OK_CustomsRegNo : GetMostRelevantGS1PrefixByAddress(gs1Codes, preferredOrgAddressPK);
			}

			return ZString.Empty;
		}

		ZString GetMostRelevantGS1PrefixByAddress(OrgCusCode[] gs1Codes, ZGuid preferredAddressPK)
		{
			if (gs1Codes == null || !gs1Codes.Any())
			{
				return ZString.Empty;
			}

			OrgCusCode cusCode = null;
			if (!preferredAddressPK.IsEmpty)
			{
				cusCode = gs1Codes.FirstOrDefault(code => code.OK_OA_PremisesAddress == preferredAddressPK);
			}

			if (cusCode == null)
			{
				cusCode = gs1Codes.FirstOrDefault(code => code.PremisesAddress != null && code.PremisesAddress.OA_RL_NKRelatedPortCode == Env.CurrentBranch.NKUNLOCO);
			}

			if (cusCode == null)
			{
				cusCode = gs1Codes.FirstOrDefault(code => code.OK_OA_PremisesAddress.IsEmpty);
			}

			return cusCode != null ? cusCode.OK_CustomsRegNo : ZString.Empty;
		}

		static ZQuery CustomsCodeQuery
		{
			get { return new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.GS1); }
		}

		#endregion

		public override void Delete()
		{
			BookingLines.RemoveAndDeleteAll();
			base.Delete();
		}

		#region FillWithValidTestDataCore Override
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			DH_OA_Consignor = supplier.MainAddress.PK;

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
		#endregion

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = Res.GetString("65d6ae2e-f94b-4f25-89b7-a091ab86d99f", "Supplier Booking");

				if (!DH_SupplierReference.IsEmpty)
				{
					result += " " + DH_SupplierReference;
				}

				result += " " + Res.GetString("55a00b61-15b7-4075-994a-2d9779509870", "(Supplier='{0}')", SupplierCode);

				return result;
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			SetSupplierReferenceIfNeeded();
		}

		void SetSupplierReferenceIfNeeded()
		{
			if (!IsInDatabase && DH_SupplierReference.IsEmpty && ShouldSetSupplierReference)
			{
				DH_SupplierReference = Env.NumberFountains.SupplierBookingNumber.GetNextFormatted(Factory);
			}
		}

		public bool ShouldSetSupplierReference { get; set; }

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded && shouldUpdateLineStatus)
			{
				UpdateSupplierBookingLineStatus();
			}
		}

		void UpdateSupplierBookingLineStatus()
		{
			try
			{
				const string sqlText = "EXEC UpdateIncompleteSupplierBookingLineStatus @HeaderPk, @IncompleteStatus, @Status";
				DbCommand command = Db.Connection.Command(sqlText);
				command.AddParameter("@HeaderPk", SqlDbType.UniqueIdentifier, PK.ToGuid());
				command.AddParameter("@IncompleteStatus", SqlDbType.VarChar, Constants.SupplierBookingLineStatus.Codes.Incomplete);
				command.AddParameter("@Status", SqlDbType.VarChar, Constants.SupplierBookingLineStatus.Codes.Confirmed);
				command.ExecuteNonQuery();
			}
			catch (SqlException) { }
		}

		string SupplierCode
		{
			get
			{
				var consignorAddress = Consignor;
				if (consignorAddress != null)
				{
					var consignor = consignorAddress.Header;
					if (consignor != null)
					{
						return consignor.OH_Code;
					}
				}
				return Res.GetString("17f35a13-37e3-4a48-ba6e-410b445f5ad0", "{none}");
			}
		}

		public string JobNumber
		{
			get { return DH_SupplierReference + " / " + SupplierCode; }
		}

		#region DH_IsShipperApproved

		public override ZBool DH_IsShipperApproved
		{
			get { return base.DH_IsShipperApproved; }
			set
			{
				base.DH_IsShipperApproved = value;
				shouldUpdateLineStatus = value;
			}
		}

		bool shouldUpdateLineStatus;

		#endregion

		#region IDocumentSupportable

		public DocumentSupporter DocumentSupporter
		{
			get { return new SupplierBookingHeaderDocumentSupporter(this); }
		}

		#endregion

		#region IValidateForCustomsMessagingSupporter

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging
		{
			get { return true; }
		}

		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerAction)
		{
			return this;
		}

		#endregion
	}
}
