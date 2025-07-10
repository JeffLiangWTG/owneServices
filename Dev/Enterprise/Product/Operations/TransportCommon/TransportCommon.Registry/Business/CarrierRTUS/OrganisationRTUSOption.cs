using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.RTUS.Interface;
using static System.FormattableString;

namespace Enterprise.TransportCommon.Registry
{
	[XmlSerializerAssembly("Enterprise.TransportCommon.Registry.XmlSerializers")]
	public class OrganisationRTUSOption : RegistryBusinessObjectTemplate, IOrganisationRTUSOption
	{
		#region Schema

		static class Schema
		{
			public const string OrganisationPK = nameof(OrganisationPK);
			public const string CBACode = nameof(CBACode);
			public const string Url = nameof(Url);
		}

		#endregion

		#region DefaultUrls

		static class DefaultUrls
		{
			// cannot default smartfreight and transstream because they vary per client
			public const string SmartFreight = "";
			public const string Transtream = "";

			public static string[] All() => new[]
			{
				SmartFreight,
				Transtream,
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default registry value")]
		const string NotSupportedMessage = "This CBA is not yet supported";

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new OrganisationRTUSOption();

			using (clone.GetValidationSuspender())
			{
				clone.CBACode = CBACode;
				clone.OrganisationPK = organisationPK;
				clone.Url = url;
			}

			return clone;
		}

		#endregion

		#region Properties

		#region OrganisationPK

		[List(nameof(Organisations))]
		public ZGuid OrganisationPK
		{
			get { return organisationPK; }
			set
			{
				SetNonPersistentPropertyValue(OrganisationPKInfo, ref organisationPK, value);
				if (!IsValidationSuspended)
				{
					ValidateOrganisationPK();
					ValidateCBACode();
				}
			}
		}

		public ZPropertyInfo<ZGuid> OrganisationPKInfo => (ZPropertyInfo<ZGuid>)GetZPropertyInfo(Schema.OrganisationPK);

		ZGuid organisationPK;

		#endregion

		#region OrganisationName

		public ZString OrganisationName
		{
			get
			{
				var orgName = ZString.Empty;

				if (OrganisationPK.IsValid)
				{
					orgName = CurrentFactory.Load<OrgHeader>(OrganisationPK)?.OH_FullName ?? ZString.Empty;
				}

				return orgName;
			}
		}

		#endregion

		#region CBACode

		[List(nameof(CBAList))]
		public ZString CBACode
		{
			get { return cbaCode; }
			set
			{
				SetNonPersistentPropertyValue(CBACodeInfo, ref cbaCode, value);
				if (!IsValidationSuspended)
				{
					ValidateCBACode();
					ValidateOrganisationPK();
				}

				SetDefaultUrl();
			}
		}

		void SetDefaultUrl()
		{
			if (CanOverrideUrl())
			{
				Url = GetDefaultUrl();
			}

			string GetDefaultUrl()
			{
				switch (CBACode)
				{
					case Shared.CBAList.Codes.SmartFreight:
						return DefaultUrls.SmartFreight;
					case Shared.CBAList.Codes.SaaS:
						return NotSupportedMessage;
					case Shared.CBAList.Codes.Teknowlogi:
						return NotSupportedMessage;
					case Shared.CBAList.Codes.TMS3G:
						return NotSupportedMessage;
					case Shared.CBAList.Codes.Transtream:
						return DefaultUrls.Transtream;
					case Shared.CBAList.Codes.Trinium:
						return NotSupportedMessage;
					default:
						return NotSupportedMessage;
				}
			}
		}

		bool CanOverrideUrl()
		{
			return DefaultUrls.All().Concat(new[] { NotSupportedMessage, string.Empty }).Contains(Url.ToString());
		}

		public ZPropertyInfo<ZString> CBACodeInfo => (ZPropertyInfo<ZString>)GetZPropertyInfo(Schema.CBACode);

		ZString cbaCode;

		#endregion

		#region CBADescription

		public ZString CBADescription => CBAList.GetDescriptionFromCode(CBACode);

		#endregion

		#region RTUSCBA

		public RTUSCBA RTUSCBA
		{
			get
			{
				switch (CBACode)
				{
					case Shared.CBAList.Codes.SaaS:
						return RTUSCBA.SaaSTransportation;
					case Shared.CBAList.Codes.SmartFreight:
						return RTUSCBA.SmartFreight;
					case Shared.CBAList.Codes.Teknowlogi:
						return RTUSCBA.Teknowlogi;
					case Shared.CBAList.Codes.TMS3G:
						return RTUSCBA.TMS3G;
					case Shared.CBAList.Codes.Transtream:
						return RTUSCBA.Pierbridge;
					case Shared.CBAList.Codes.Trinium:
						return RTUSCBA.Trinium;
					default:
						throw new InvalidOperationException(Invariant($"Invalid RTUSCBA '{CBACode}'."));
				}
			}
		}

		#endregion

		#region Url

		public Uri WrappedUrl => new Uri(url);

		public ZString Url
		{
			get { return url; }
			set
			{
				SetNonPersistentPropertyValue(UrlInfo, ref url, value);
				if (!IsValidationSuspended)
				{
					ValidateUrl();
					ValidateOrganisationPK();
					ValidateCBACode();
				}
			}
		}

		public ZPropertyInfo<ZString> UrlInfo => (ZPropertyInfo<ZString>)GetZPropertyInfo(Schema.Url);

		ZString url;

		#endregion

		#endregion

		#region Lists

		#region Organisations

		public OrgHeaderCollection Organisations => CurrentFactory.GetCachedValue("6A7B876E-D12B-49F9-BE89-0F6A6F208A13", () => new OrgHeaderCollection(CurrentFactory));

		#endregion

		#region CBAList

		public CodeDescriptionPairList CBAList => CurrentFactory.GetCachedValue("B1790FD6-D001-4687-8983-A3A5FDC911FD", () => new CBAList());

		#endregion

		#endregion

		#region Validations

		#region ValidateOrganisationPK

		public void ValidateOrganisationPK()
		{
			OrganisationPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OrganisationPKInfo);
			ValidateInvalidOrganisationPK(OrganisationPKInfo);
			ValidateUnique(OrganisationPKInfo, option => option.OrganisationPK == OrganisationPK, ResString.GetMultilingualString("FDA7922E-7E70-48E7-B485-54A117DEEB98", "This organization has already been assigned to another CBA."));
		}

		void ValidateInvalidOrganisationPK(ZPropertyInfo info)
		{
			if (!info.HasErrors() && !info.Value.IsEmpty && !info.Value.IsValid)
			{
				info.AddError(ResString.GetMultilingualString("71A80CBB-67FD-4901-A247-2AA10E5C28FD", "Please select a valid organization."));
			}
		}

		#endregion

		#region ValidateCBACode

		public void ValidateCBACode()
		{
			CBACodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CBACodeInfo);
			ListValidation.ErrorIfInvalidCode(CBACodeInfo);
		}

		void ValidateUnique(ZPropertyInfo info, Func<OrganisationRTUSOption, bool> hasSameProperty, ResourceString errorMessage)
		{
			var parentCollection = GetParentCollection(this, typeof(OrganisationRTUSCollection));

			if (!info.HasErrors() && parentCollection != null)
			{
				foreach (OrganisationRTUSOption option in parentCollection)
				{
					if (hasSameProperty(option) && option.PK != PK)
					{
						info.AddError(errorMessage);
					}
				}
			}
		}

		#endregion

		#region ValidateUrl

		public void ValidateUrl()
		{
			UrlInfo.ClearAllNotifications();
			if (!UrlInfo.HasErrors() && (UrlInfo.Value.IsEmpty || !Uri.IsWellFormedUriString(Url, UriKind.Absolute)))
			{
				UrlInfo.AddError(ResString.GetMultilingualString("f01d4e05-e85b-4cc1-a3af-95ba4f8670a9", "Please enter a valid URL."));
			}
		}

		#endregion

		#endregion

		#region Delete

		public override bool CanDelete => true;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.OrganisationPK, OrganisationPK.ToString());
			writer.WriteElementString(Schema.CBACode, CBACode);
			writer.WriteElementString(Schema.Url, Url);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			using (GetValidationSuspender())
			{
				if (ZGuid.TryParse(reader.ReadElementString(Schema.OrganisationPK), out var storedPK))
				{
					OrganisationPK = storedPK;
				}

				CBACode = reader.ReadElementString(Schema.CBACode);
				Url = reader.ReadElementString(Schema.Url);
			}
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateUrl();
			ValidateOrganisationPK();
			ValidateCBACode();
		}
	}
}
