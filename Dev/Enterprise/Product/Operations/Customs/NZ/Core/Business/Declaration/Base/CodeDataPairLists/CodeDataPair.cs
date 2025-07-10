using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public abstract class CodeDataPair : NonPersistentBusinessObject, IObsoleteValidation, Integration.Customs.NZ.ICodeDataPair
	{
		public CodeDataPair(BusinessObjectFactory factory, CodeDataPairCollection parentCollection)
			: base(factory)
		{
			ParentCollection = parentCollection;
		}
		protected readonly CodeDataPairCollection ParentCollection;

		protected BusinessObject Parent => ParentCollection?.Parent;

		public JobDeclaration Declaration => ParentCollection?.Declaration;

		protected CusClassPartPivot ClassificationPart => (Parent as NZAddInfo)?.ParentObject as CusClassPartPivot;

		protected CusClassification Classification => Parent as CusClassification;

		public static class Schema
		{
			public const string TablePrefix = "ZO_";
			public const string ZO_Code = "ZO_Code";
			public const string ZO_Data = "ZO_Data";
			public const string ZO_Description = "ZO_Description";

			public const int ZO_CodeMaximumLength = 3;
			public const int ZO_DataMaximumLength = 12;
		}

		public override string ToString()
		{
			string result = ZO_Code;
			if (!ZO_Data.IsEmpty)
			{
				result += NZAddInfo.CodeValueSeparator + ZO_Data;
			}
			return result;
		}

		public void LoadFromString(ZString addInfoString)
		{
			if (!addInfoString.IsEmpty)
			{
				ZString[] codeDataPair = addInfoString.Split(NZAddInfo.CodeValueSeparator);

				fZO_Code = codeDataPair[0];
				if (codeDataPair.Length == 2)
				{
					fZO_Data = codeDataPair[1];
				}
			}
		}

		[MaxLength(Schema.ZO_CodeMaximumLength)]
		[ResourceStringData("Enterprise.Customs.NZ.Business.CodeDataPair|ZO_Code", Caption = "Code")]
		public ZString ZO_Code
		{
			get { return fZO_Code.ToUpper(); }
			set
			{
				bool changed = fZO_Code != value;
				CheckMaximumLength(ZO_CodeInfo, value);
				SetNonPersistentPropertyValue(ZO_CodeInfo, ref fZO_Code, value.ToUpper());
				if (changed)
				{
					HasChanges = true;
					if (!ParentCollection?.IsCodesChangedRelatedActionsSuspended ?? true)
					{
						ZO_Code_OnChanged();
						ParentCollection?.FireCodesInListHaveChanged();
					}
				}
				if (!IsValidationSuspended)
				{
					ValidateZO_Code();
				}
			}
		}
		protected ZString fZO_Code;

		public ZPropertyInfo ZO_CodeInfo
		{
			get { return GetZPropertyInfo(Schema.ZO_Code); }
		}

		protected virtual void ZO_Code_OnChanged()
		{
		}

		public virtual void ValidateZO_Code()
		{
			ZO_CodeInfo.ClearAllNotifications();
			if (ZO_Code.IsEmpty)
			{
				ZO_CodeInfo.AddError(MustEnterACode);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(ZO_CodeInfo, ZO_CodeList);
				if (ShouldValidateForDuplicateCodes && ParentCollection != null)
				{
					ParentCollection.ValidateDuplicateCode(this);
				}
			}
		}

		#region ZO_CodeList

		public CodeDescriptionPairList ZO_CodeList => LoadActiveCodeList(() => ImportCodes, () => ExportCodes, () => ImportExportCodes, () => LegacyCodes);

		protected T LoadActiveCodeList<T>(Func<T> getImportCodesFunc, Func<T> getExportCodesFunc, Func<T> getImportExportCodesFunc, Func<T> getLegacyCodesFunc)
		{
			bool isExport = false;
			bool isImport = false;

			if (Declaration is JobDeclaration declaration)
			{
				isExport = IsExportDeclaration(declaration);
				isImport = IsImportDeclaration(declaration);
			}
			else if (ClassificationPart is CusClassPartPivot pivot)
			{
				isExport = pivot.IsExportClassification;
				isImport = pivot.IsImportClassification;
			}
			else if (Classification is CusClassification classification)
			{
				isExport = classification.IsExport || classification.IsBoth;
				isImport = classification.IsImport || classification.IsBoth;
			}

			T codeList;
			if (isExport && isImport)
			{
				codeList = getImportExportCodesFunc();
			}
			else if (isExport)
			{
				codeList = getExportCodesFunc();
			}
			else if (isImport)
			{
				codeList = getImportCodesFunc();
			}
			else
			{
				codeList = getLegacyCodesFunc();
			}

			return codeList;
		}

		protected virtual bool IsExportDeclaration(JobDeclaration declaration) => declaration.IsTSWExportDeclaration;
		protected virtual bool IsImportDeclaration(JobDeclaration declaration) => declaration.IsTSWImportDeclaration;

		protected abstract CodeDescriptionPairList ExportCodes { get; }
		protected abstract CodeDescriptionPairList ImportCodes { get; }
		protected abstract CodeDescriptionPairList LegacyCodes { get; }
		protected abstract string ImportExportCodesCacheKey { get; }
		CodeDescriptionPairList ImportExportCodes => Factory.GetCachedValue(ImportExportCodesCacheKey, () =>
		{
			var importCodes = ImportCodes.ToArray();
			var exportCodes = ExportCodes.ToArray();

			var exportNotInImport = exportCodes.Except(importCodes);
			var importNotInExport = importCodes.Except(exportCodes);
			var inBoth = exportCodes.Intersect(importCodes);

			var all = new List<ICodeDescription>();

			all.AddRange(exportNotInImport.Select(v => new CodeDescriptionWithPrefix(v, ExportPrefix)));
			all.AddRange(importNotInExport.Select(v => new CodeDescriptionWithPrefix(v, ImportPrefix)));
			all.AddRange(inBoth.Select(v => new CodeDescriptionWithPrefix(v, BothPrefix)));

			all.Sort((a, b) => a.Code.CompareTo(b.Code));

			var codeList = new CodeDescriptionPairList();
			codeList.AddPairsIfNotExist(all);
			return codeList;
		});

		public bool IsImportCode => ImportCodes.ContainsCode(ZO_Code);

		public bool IsExportCode => ExportCodes.ContainsCode(ZO_Code);

		#endregion

		[ResourceStringData("Enterprise.Customs.NZ.Business.CodeDataPair|ZO_Description", Caption = "Description")]
		public ZString ZO_Description
		{
			get { return ZO_CodeList.GetDescriptionFromCode(ZO_Code) ?? ZString.Empty; }
		}

		public ZPropertyInfo ZO_DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ZO_Description); }
		}

		[MaxLength(Schema.ZO_DataMaximumLength)]
		[ResourceStringData("Enterprise.Customs.NZ.Business.CodeDataPair|ZO_Data", Caption = "Data")]
		public ZString ZO_Data
		{
			get { return fZO_Data.ToUpper(); }
			set
			{
				bool changed = fZO_Data != value;
				CheckMaximumLength(ZO_DataInfo, value);
				SetNonPersistentPropertyValue(ZO_DataInfo, ref fZO_Data, value.ToUpper());
				if (changed)
				{
					HasChanges = true;
					if (!ParentCollection?.IsCodesChangedRelatedActionsSuspended ?? true)
					{
						ZO_Data_OnChanged();
					}
				}
				if (!IsValidationSuspended)
				{
					ValidateZO_Data();
				}
			}
		}
		protected ZString fZO_Data;

		public ZPropertyInfo ZO_DataInfo
		{
			get { return GetZPropertyInfo(Schema.ZO_Data); }
		}

		protected virtual void ZO_Data_OnChanged()
		{
		}

		protected virtual bool CodeDoesntCareIfItHasDataOrNot
		{
			get { return false; }
		}

		public virtual void ValidateZO_Data()
		{
			ZO_DataInfo.ClearAllNotifications();
			if (ZO_Code.IsEmpty)
			{
				if (!ZO_Data.IsEmpty)
				{
					ZO_DataInfo.AddError(MustEnterACodeForThisData);
				}
			}
			else if (!ZO_CodeInfo.HasNotifications())
			{
				if (CodeRequiresData && ZO_Data.IsEmpty)
				{
					ZO_DataInfo.AddMessageError(ZO_Code + " " + CodesAreNotAllowedWithoutAccompanyingData);
				}
				else if (!CodeRequiresData && !CodeDoesntCareIfItHasDataOrNot && !ZO_Data.IsEmpty)
				{
					ZO_DataInfo.AddMessageError(ZO_Code + " " + CodesDoNotRequireAccompanyingData);
				}
			}
		}
		public const string CodesAreNotAllowedWithoutAccompanyingData = "Codes are not allowed without accompanying Data.";
		public const string CodesDoNotRequireAccompanyingData = "Codes do not require accompanying Data.";
		public const string MustEnterACodeForThisData = "Please enter a Code for this Data.";
		public const string MustEnterACode = "Please enter a Code.";
		public const string ImportPrefix = "IMP";
		public const string ExportPrefix = "EXP";
		public const string BothPrefix = "BOTH";

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateZO_Code();
			ValidateZO_Data();
		}

		public abstract bool ShouldValidateForDuplicateCodes { get; }
		public abstract bool CodeRequiresData { get; }
	}
}
