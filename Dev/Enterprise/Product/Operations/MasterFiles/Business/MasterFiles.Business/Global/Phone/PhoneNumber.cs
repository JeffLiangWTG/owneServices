using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class PhoneNumber : NonPersistentBusinessObject, ICustomTextTemplateContext
	{
		public PhoneNumber(ZPropertyInfo formattedInfo, ZPropertyInfo publishInfoIfAny, ZPropertyInfo formattedLocalNumberIfLoggedInSameCountryInfoIfAny, ZPropertyInfo isManuallyVerifiedInfo = null)
		{
			Argument.NotNull(formattedInfo, "formattedInfo");

			FormattedInfo = formattedInfo;
			PublishInfo = publishInfoIfAny;
			FormattedLocalNumberIfLoggedInSameCountryInfo = formattedLocalNumberIfLoggedInSameCountryInfoIfAny;
			IsManuallyVerifiedInfo = isManuallyVerifiedInfo;
		}

		public PhoneNumber(OrgAddress parent, ZPropertyInfo formattedInfo, ZPropertyInfo publishInfoIfAny, ZPropertyInfo formattedLocalNumberIfLoggedInSameCountryInfoIfAny, ZPropertyInfo isManuallyVerifiedInfo = null)
			: this(formattedInfo, publishInfoIfAny, formattedLocalNumberIfLoggedInSameCountryInfoIfAny, isManuallyVerifiedInfo)
		{
			Parent = parent;
		}

		public readonly ZPropertyInfo FormattedInfo;
		readonly ZPropertyInfo PublishInfo;
		public readonly ZPropertyInfo FormattedLocalNumberIfLoggedInSameCountryInfo;
		public readonly ZPropertyInfo IsManuallyVerifiedInfo;
		readonly OrgAddress Parent;

		#region Properties

		[BusinessObjectTestExclude]
		public ZString FormattedForBinding
		{
			get { return (ZString)FormattedInfo.Value; }
			set
			{
				if ((ZString)FormattedInfo.Value != value)
				{
					FormattedInfo.Value = value;
					FormattedForBindingInfo.RefreshBinding();
					Parent?.Header?.FindDuplicates();
				}
			}
		}

		public ZPropertyInfo FormattedForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(FormattedForBinding), x => FormattedInfo); }
		}

		public int FormattedForBinding_MaxLength
		{
			get { return FormattedInfo.MaxLength; }
		}

		public bool FormattedForBinding_ReadOnly
		{
			get { return FormattedInfo.ReadOnly; }
		}

		[BusinessObjectTestExclude]
		public ZBool IsPublishedForBinding
		{
			get
			{
				var result = false;
				if (PublishInfo != null)
				{
					result = (ZBool)PublishInfo.Value;
				}
				return result;
			}
			set
			{
				if (PublishInfo != null)
				{
					PublishInfo.Value = value;
				}
				IsPublishedForBindingInfo.RefreshBinding();
			}
		}

		public bool IsPublishedForBinding_ReadOnly
		{
			get { return PublishInfo == null; }
		}

		public ZPropertyInfo IsPublishedForBindingInfo
		{
			get
			{
				if (PublishInfo != null)
				{
					return GetWrappedZPropertyInfo(nameof(IsPublishedForBinding), x => PublishInfo);
				}
				else
				{
					return GetZPropertyInfo(nameof(IsPublishedForBinding));
				}
			}
		}

		[BusinessObjectTestExclude]
		public ZString FormattedLocalNumberIfLoggedInSameCountryForBinding
		{
			get
			{
				var result = string.Empty;
				if (FormattedLocalNumberIfLoggedInSameCountryInfo != null)
				{
					result = (ZString)FormattedLocalNumberIfLoggedInSameCountryInfo.Value;
				}
				return result;
			}
		}

		public ZPropertyInfo FormattedLocalNumberIfLoggedInSameCountryForBindingInfo
		{
			get
			{
				if (FormattedLocalNumberIfLoggedInSameCountryInfo != null)
				{
					return GetWrappedZPropertyInfo(nameof(FormattedLocalNumberIfLoggedInSameCountryForBinding), x => FormattedLocalNumberIfLoggedInSameCountryInfo);
				}
				else
				{
					return GetZPropertyInfo(nameof(FormattedLocalNumberIfLoggedInSameCountryForBinding));
				}
			}
		}

		#endregion

		#region RefreshFormat

		/// <summary>
		/// Refreshes the format.
		/// </summary>
		public void RefreshFormat()
		{
			FormattedInfo.Value = FormattedInfo.Value;
			FormattedForBindingInfo.RefreshBinding();
			FormattedLocalNumberIfLoggedInSameCountryForBindingInfo.RefreshBinding();
		}

		#endregion

		#region Implementation

		public override string ToString()
		{
			return FormattedForBinding;
		}

		#region ICustomTextTemplateContext Members

		public BusinessObject[] GetTextTemplateContextBusinessObject(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			return new[] { GetTextTemplateContextBusinessObject(dataSource) };
		}

		public string GetTextTemplateContextID(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			return GetTextTemplateContextBusinessObject(dataSource).TableName + "." + bindingMemberInfo.BindingField;
		}

		BusinessObject GetTextTemplateContextBusinessObject(object dataSource)
		{
			var businessObject = dataSource as BusinessObject;
			return businessObject ?? this;
		}

		#endregion

		#endregion
	}
}
