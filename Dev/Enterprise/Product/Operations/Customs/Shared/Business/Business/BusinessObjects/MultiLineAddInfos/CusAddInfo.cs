using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.MultiLineAddInfos
{
	[SingleObjectAroundARow]
	[UniversalCopyWithExtendedEntities]
	public abstract partial class CusAddInfo : AutoCusAddInfo, Integration.Customs.ICusAddInfo, IOldParentIDProvider
	{
		public new class Schema : AutoCusAddInfo.Schema
		{
		}

		protected CusAddInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[LightValidationTestExempt]
		public override ZGuid B7_ParentID
		{
			get { return base.B7_ParentID; }
			set
			{
				var oldValue = B7_ParentID;
				base.B7_ParentID = value;
				if (!IsCopying && oldValue != B7_ParentID)
				{
					oldNoneEmptyParentID = !oldValue.IsEmpty && B7_ParentID.IsEmpty ? oldValue : ZGuid.Empty;
				}
			}
		}

		ZGuid IOldParentIDProvider.OldParentID => oldNoneEmptyParentID;
		ZGuid oldNoneEmptyParentID { get; set; }

		public BusinessObject Parent
		{
			get
			{
				if (fParent == null)
				{
					SetParentFromParentTableCodeAndParentIDIfPossible();
				}
				return fParent;
			}
			internal set
			{
				SetParent(value);
			}
		}
		BusinessObject fParent;

		public static readonly CusAddInfoTypeDecider TypeDecider = new CusAddInfoTypeDecider();

		void SetParentFromParentTableCodeAndParentIDIfPossible()
		{
			if (!B7_ParentTableCode.IsEmpty && !B7_ParentID.IsEmpty)
			{
				fParent = Factory.Load(B7_ParentTableCode, B7_ParentID);
			}
		}

		protected void SetParent(BusinessObject parent)
		{
			fParent = parent;
			if (fParent != null && (B7_ParentID != fParent.PK || B7_ParentTableCode != fParent.TablePrefix))
			{
				B7_ParentID = fParent.PK;
				B7_ParentTableCode = fParent.TablePrefix;
			}
			if (parent == null)
			{
				ErrorReporter.ReportOnce("CusAddInfo-NullParent", string.Format("Parent of multi-line CusAddInfo was set to null. Type={0}; Data={1}; ParentID/Code={2}/{3}", GetType().FullName, B7_AddInfoData, B7_ParentID, B7_ParentTableCode));
			}
		}

		public override void OnSaving()
		{
			if (!IsInDatabase && !IsDeleted && !B7_ParentID.IsEmpty && B7_ParentTableCode.IsEmpty)
			{
				var fieldName = GetType().FullName + ".B7_ParentTableCode";
				var parent = Parent;
				ErrorReporter.ReportOnce(fieldName + " Is Empty", string.Format("{0}{1} must be specified.", fieldName, parent == null ? "" : string.Format((NoResString)" (Parent:{0})", parent.GetType().FullName)));
			}
			base.OnSaving();
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				using (B7_ParentID.IsEmpty ? this.EnableOldParentID() : DisposableAction.NoAction)
				{
					FetchForLoadChildEditableObjectsIfNeeded();
					this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				}
			}
			base.Delete();
		}

		#region Cloning

		public virtual CusAddInfo Clone(BusinessObjectCloneArgs args, Dictionary<ZGuid, ZGuid> jobDocAddressPKPairs)
		{
			var result = (CusAddInfo)base.Clone(args);
			((IBusinessObjectInternals)result).IsCopying = true;
			try
			{
				using (result.GetValidationSuspender())
				using (result.SuspendSettingHasChanges())
				{
					ReplaceJobDocAddressPK(result, jobDocAddressPKPairs);
				}
			}
			finally
			{
				((IBusinessObjectInternals)result).IsCopying = false;
			}
			return result;
		}

		protected virtual void ReplaceJobDocAddressPK(CusAddInfo clonedData, Dictionary<ZGuid, ZGuid> jobDocAddressPKPairs)
		{
		}

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if (this.B7_ParentTableCode.IsEmpty)
			{
				this.B7_ParentTableCode = "Z!";
			}
			if (this.B7_ParentID.IsEmpty)
			{
				this.B7_ParentID = ZGuid.NewZGuid();
			}
		}

#endif
	}

	public class CusAddInfo<T> : CusAddInfo, IAddInfoManager where T : BaseAddInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public CusAddInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (AddInfoConstructor == null)
			{
				throw new InvalidOperationException("The Generic Type specified for this object must have a constructor taking a ZPropertyInfo.");
			}
			if (TypeAttribute == null)
			{
				throw new InvalidOperationException("The Generic Type specified for this object must have a valid CusAddInfoTypeAttribute applied against the class.");
			}
		}

		ConstructorInfo AddInfoConstructor
		{
			get { return addInfoConstructor ?? (addInfoConstructor = AddInfoType.GetConstructor(new Type[] { typeof(ZPropertyInfo) })); }
		}
		ConstructorInfo addInfoConstructor;

		protected virtual Type AddInfoType => typeof(T);

		protected CusAddInfoTypeAttribute TypeAttribute
		{
			get { return typeAttribute ?? (typeAttribute = CusAddInfoTypeAttribute.Get(AddInfoType)); }
		}
		CusAddInfoTypeAttribute typeAttribute;

		public T Data
		{
			get
			{
				if (fData == null)
				{
					fData = GetNewAddInfo();
					RegisterEditableChildObject(fData);
					RegisterListChangedCalledRefreshBinding(fData);
				}
				return fData;
			}
		}
		T fData;

		T GetNewAddInfo()
		{
			return (T)AddInfoConstructor.Invoke(new object[] { B7_AddInfoDataInfo });
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CusAddInfoTypeAttribute typeAttribute = TypeAttribute;
			if (typeAttribute != null)
			{
				B7_Type = TypeAttribute.TypeCode;
			}
		}

		[BusinessObjectTestExclude]
		public override ZString B7_AddInfoData
		{
			get => this is INAddInfoSupporter ? AddInfoParser.ConcatAddInfoStrings(base.B7_AddInfoData, B7_NAddInfoData) : base.B7_AddInfoData;
			set
			{
				if (this.GetBaseAddInfoWithNAddInfoSupport() is BaseAddInfo addInfo)
				{
					var addInfoStrings = addInfo.SplitAddInfoString(value);
					base.B7_AddInfoData = addInfoStrings.Item1;
					B7_NAddInfoData = addInfoStrings.Item2;
				}
				else
				{
					base.B7_AddInfoData = value;
				}
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Data.HumanReadableName; }
		}

		#region Cloning
		public new CusAddInfo<T> Clone()
		{
			return (CusAddInfo<T>)base.Clone();
		}

		public new CusAddInfo<T> Clone(BusinessObjectCloneArgs args)
		{
			return (CusAddInfo<T>)base.Clone(args);
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new string[] { Schema.B7_ParentID, Schema.B7_ParentTableCode };
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			if (fData != null && fData.HasChanges)
			{
				fData.UpdateRelatedPropertyInfo();
			}
			return base.CloneInternal(args);
		}
		#endregion

		#region IAddInfoManager Members

		IAddInfo IAddInfoManager.AddInfo
		{
			get { return Data; }
		}

		#endregion
	}
}
