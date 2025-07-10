using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// JobComInvoiceLine.JI_CL points to the only entry line. 
	/// If there are additional entries for the invoice line 
	/// like ZA RIB entry or US InBond or Cargo Release, then this link has the information. 
	/// </summary>
	[SingleObjectAroundARow]
	public class AdditionalInvoiceLineEntryLineLink : AutoCusUnderbondDec, IClusterKeyWorker
	{
		public AdditionalInvoiceLineEntryLineLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public BaseJobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<BaseJobComInvoiceLine>(BU_JI); }
		}

		public CusEntryLine EntryLine
		{
			get { return Factory.Load<CusEntryLine>(BU_CL); }
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != "ER")
			{
				BaseJobComInvoiceLine invoiceLine = this.InvoiceLine;

				if (invoiceLine != null && invoiceLine.Declaration != null && !invoiceLine.Declaration.NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine)
				{
					ErrorReporter.ReportOnce("AdditionalInvoiceLineEntryLineLink is about to be saved even when Declaration.NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine is false", "Please add the country code to JobDeclaration.GetCountriesNeedingAdditionalLink if you need a pivot between JobComInvoiceLine and CusEntryLine");
				}
			}
		}

		#region Overridden Properties

		public override ZGuid BU_JI
		{
			get => base.BU_JI;
			set
			{
				base.BU_JI = value;
				ReportErrorIfEntryLineAndInvoiceLineIsIrrelevant();
			}
		}

		public override ZGuid BU_CL
		{
			get => base.BU_CL;
			set
			{
				base.BU_CL = value;
				ReportErrorIfEntryLineAndInvoiceLineIsIrrelevant();
			}
		}

		void ReportErrorIfEntryLineAndInvoiceLineIsIrrelevant()
		{
			if (HasChanges && InvoiceLine?.InvoiceHeader?.JobDeclaration is BaseJobDeclaration decForInvoiceLine && EntryLine?.Header?.Declaration is BaseJobDeclaration decForEntryLine && decForInvoiceLine.PK != decForEntryLine.PK && !decForEntryLine.AllowEntryLinesToBeLinkedToAnotherJob)
			{
				var errorMessage = $@"Entry line is linked to irrelevant invoice line.
---------------Details For Invoice Line---------------
Invoice Line PK			: {InvoiceLine.PK}
Invoice Line ClusterKey	: {InvoiceLine.JI_ClusterKey}
Invoice PK				: {InvoiceLine.InvoiceHeader.PK}
Invoice ClusterKey		: {InvoiceLine.InvoiceHeader.JZ_ClusterKey}
Invoice Number			: {InvoiceLine.InvoiceHeader.JZ_InvoiceNumber}
Declaration PK			: {decForInvoiceLine.PK}
Declaration ClusterKey	: {decForInvoiceLine.JE_ClusterKey}
Declaration Type		: {decForInvoiceLine.JE_MessageType}
Declaration Sub Type	: {decForInvoiceLine.JE_MessageSubType}
Declaration Reference	: {decForInvoiceLine.JE_DeclarationReference}

---------------Details For Entry Line-----------------
Entry Line PK			: {EntryLine.PK}
Entry Line ClusterKey	: {EntryLine.CL_ClusterKey}
Entry PK				: {EntryLine.Header.PK}
Entry ClusterKey		: {EntryLine.Header.CH_ClusterKey}
Entry Type				: {EntryLine.Header.CH_MessageType}
Declaration PK			: {decForEntryLine.PK}
Declaration ClusterKey	: {decForEntryLine.JE_ClusterKey}
Declaration Type		: {decForEntryLine.JE_MessageType}
Declaration Sub Type	: {decForEntryLine.JE_MessageSubType}
Declaration Reference	: {decForEntryLine.JE_DeclarationReference}";
				ExceptionReporter.Instance.ReportDeveloperException((NoResString)"Entry line is linked to irrelevant invoice line", errorMessage, new InvalidOperationException(errorMessage));
			}
		}

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)BU_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(CusEntryLine);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)BU_CLInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}
