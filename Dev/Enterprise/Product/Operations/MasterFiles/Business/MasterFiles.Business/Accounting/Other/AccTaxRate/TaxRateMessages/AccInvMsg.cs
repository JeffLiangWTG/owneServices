using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(AccInvMsgSchema.Constants.A9_Code), DescriptionProperty(AccInvMsgSchema.Constants.A9_Description)]
	[RestrictedFilteredItem]
	public class AccInvMsg : AutoAccInvMsg, Integration.IAccInvMsg, IDocManagerSupport, IEDocsParsingSupport
	{
		public AccInvMsg(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (!IsInDatabase && A9_RN_NKCountryCode.IsEmpty)
			{
				A9_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			}
		}

		protected override void OnFactorySaving()
		{
			if (!IsInDatabase && A9_RN_NKCountryCode.IsEmpty)
			{
				A9_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			}
			base.OnFactorySaving();
		}

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.InvoiceTaxMessage);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		[TranslatableDataField(Schema.TableName, Schema.A9_EnglishMsg, MaxLength = Schema.A9_EnglishMsgMaxLength, Type = typeof(AccInvMsg), SecurityCheckpoint = "InvoiceMessagesEdit", Asmid = ResString.AssemblyId)]
		public override ZString A9_EnglishMsg
		{
			get { return base.A9_EnglishMsg; }
			set { base.A9_EnglishMsg = value; }
		}

		public MultilingualString A9_EnglishMsgMultilingual
		{
			get { return GetMultilingual(A9_EnglishMsgInfo); }
		}

		protected override ZString HumanReadableNameCore => Res.GetString("237A589F-0DB4-49FC-8B12-922B9F668C2C", "Invoice Tax Message - {0}", CalculateShortcutName());

		[List("Lookups.TaxGroupCodes")]
		public override ZString A9_TaxGroupCode
		{
			get { return base.A9_TaxGroupCode; }
			set { base.A9_TaxGroupCode = value; }
		}

		public static CodeDescriptionPairList GetTaxGroupCodesFromBinaryValues(byte[] value)
		{
			if (value != null && value.Length > 0)
			{
				var result = (CodeDescriptionBoolCollection)AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.DataType?.Deserialise(value);
				if (result != null)
				{
					return result.GetActiveCodeDescriptionPairList();
				}
			}
			return new CodeDescriptionPairList();
		}

		public ZString TaxGroupGovtCode
		{
			get
			{
				var regValues = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);
				return ((CodeDescriptionBoolRelatedItem)regValues?.FindByCode(A9_TaxGroupCode))?.RelatedItemCode ?? ZString.Empty;
			}
		}
	}
}
