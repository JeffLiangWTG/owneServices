using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Customs.ZA.Business
{
	public class CUSDECEDIMessage : SARSEDIMessage, IDocumentSupportable
	{
		public CUSDECEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Override Properties

		public override ZString LocalReferenceNumber
		{
			get
			{
				if (!lrn.HasValue)
				{
					lrn = CUSDECHelper?.LocalReferenceNumber ?? ZString.Empty;
				}
				return lrn.Value;
			}
		}
		ZString? lrn;

		public ZString DeclarationType => declarationType ?? (declarationType = CUSDECHelper?.DeclarationType ?? ZString.Empty);
		string declarationType;

		CUSDECMessageHelper CUSDECHelper => cusdecHelper ?? (cusdecHelper = CUSDECMessageHelper.New(this));
		CUSDECMessageHelper cusdecHelper;

#if DEBUG
		public void ResetDeclarationType()
		{
			declarationType = null;
		}

		public void ResetCUSDECHelper()
		{
			cusdecHelper = null;
		}
#endif

		public override ZString ParentMessageNumber
		{
			get
			{
				if (!parentMessageNumber.HasValue)
				{
					parentMessageNumber = EM_MessageNum;
				}

				return parentMessageNumber.Value;
			}
		}
		ZString? parentMessageNumber;

		#endregion

		public DocumentSupporter DocumentSupporter => ducumentSupporter ?? (ducumentSupporter = new CUSDECEDIMessageDocumentSupporter(this));

		DocumentSupporter ducumentSupporter;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypes.CUSDEC;
		}

		#region VPBAmountForEntryLines

		[ChildEditable]
		public VPBAmountCodeDataCollection VPBAmounts
		{
			get
			{
				if (vpbAmounts == null)
				{
					vpbAmounts = new VPBAmountCodeDataCollection(this);
					vpbAmounts.Load();
					RegisterEditableChildObject(vpbAmounts);
				}
				return vpbAmounts;
			}
		}
		VPBAmountCodeDataCollection vpbAmounts;

		internal void CopyVPBValues()
		{
			var entryHeader = this.EM_LinkedObject as CusEntryHeader;
			if (entryHeader != null)
			{
				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					AddOrUpdateVPBAmountForEntryLine(entryLine);
				}
			}
		}

		internal ZDecimal GetVPBAmountForLine(ZString lineNumberFormatted)
		{
			return GetVPBAmountCodeDataForLine(lineNumberFormatted)?.CY_Value ?? ZDecimal.Zero;
		}

		void AddOrUpdateVPBAmountForEntryLine(CusEntryLine entryLine)
		{
			if (entryLine != null && !entryLine.CL_VPBAmount.IsEmpty)
			{
				var lineNumber = entryLine.CL_LineNumber.ToString();
				var existingVPB = GetVPBAmountCodeDataForLine(lineNumber);
				if (existingVPB == null)
				{
					VPBAmounts.AddNew(lineNumber, entryLine.CL_VPBAmount);
				}
				else
				{
					existingVPB.CY_Value = entryLine.CL_VPBAmount;
				}
			}
		}

		VPBAmountCodeData GetVPBAmountCodeDataForLine(ZString lineNumberFormatted)
		{
			if (!lineNumberFormatted.IsEmpty)
			{
				return VPBAmounts.Cast<VPBAmountCodeData>().FirstOrDefault(x => x.CY_Code == VPBAmountCodeData.FormatToCY_Code(lineNumberFormatted));
			}
			else
			{
				return null;
			}
		}

		#endregion

		protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			var result = base.GetCusCodeDataTypesCore();
			result.Add(CusCodeDataTypeList.Codes.VPBAmount, typeof(VPBAmountCodeData));
			return result;
		}
	}
}
