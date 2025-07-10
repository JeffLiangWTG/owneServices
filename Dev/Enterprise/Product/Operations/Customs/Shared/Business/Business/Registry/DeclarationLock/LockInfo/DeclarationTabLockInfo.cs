using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	public sealed class DeclarationTabLockInfo : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string TabPage = "TabPage";
			public const string TabPageDescription = "TabPageDescription";
		}

		#endregion

		public DeclarationTabLockInfo()
		{
		}

		public DeclarationTabLockInfo(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Properties

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(DeclarationTabLockInfoLookups.TabPageList))]
		public ZString TabPage
		{
			get => tabPage;
			set
			{
				if (tabPage != value)
				{
					CheckMaximumLength(TabPageInfo, value);
					SetNonPersistentPropertyValue(TabPageInfo, ref tabPage, value);

					TabPageDescriptionInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						ValidateTabPage();
					}
				}
			}
		}

		public ZPropertyInfo TabPageInfo => GetZPropertyInfo(Schema.TabPage);

		ZString tabPage;

		public ZString TabPageDescription => string.IsNullOrWhiteSpace(TabPage) ? string.Empty : Lookups.TabPageList.GetDescriptionFromCode(TabPage);

		public ZPropertyInfo TabPageDescriptionInfo => GetZPropertyInfo(Schema.TabPageDescription);

		public DeclarationLockConfig LockConfig => ((DeclarationTabLockInfoCollection)GetParentCollection(this, typeof(DeclarationTabLockInfoCollection)))?.LockConfig;

		#endregion

		#region Lookups

		public DeclarationTabLockInfoLookups Lookups => lookups ?? (lookups = new DeclarationTabLockInfoLookups(this));
		DeclarationTabLockInfoLookups lookups;

		public void RefreshLookups()
		{
			lookups = null;
		}
		#endregion

		#region Validation

		public void ValidateTabPage()
		{
			TabPageInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(TabPageInfo);
			ListValidation.ErrorIfInvalidCode(TabPageInfo, Lookups.TabPageList);

			if (!TabPageInfo.HasErrors())
			{
				if (TabPage == Core.Constants.Customs.DeclarationTabPages.Codes.Declaration)
				{
					var warning = Enterprise.Customs.Business.Res.GetString("AEAC3E17-810D-4E80-A3D9-7B0FEE027E7A",
						"The selection of Declaration will cause controls on a declaration to be locked with the exception of the controls on each of the tabs specified below.\r\n\r\nDeclaration - Services\r\n\r\nDeclaration - Organizations\r\n\r\nDeclaration - Pickup/Delivery\r\n\r\nDeclaration - Orders\r\n\r\nDeclaration - Custom\r\n\r\nDeclaration - Numbers");

					TabPageInfo.AddWarning(warning);
				}

				UniqueCheck();
			}
		}

		void UniqueCheck()
		{
			var parentCollection = GetParentCollection(this, typeof(DeclarationTabLockInfoCollection)) as DeclarationTabLockInfoCollection;

			if (parentCollection != null)
			{
				if (TabPage != Core.Constants.Customs.DeclarationTabPages.Codes.All && parentCollection.Cast<DeclarationTabLockInfo>().Any(c => c != this && c.TabPage == Core.Constants.Customs.DeclarationTabPages.Codes.All && c.CurrentFallbackLevel == CurrentFallbackLevel))
				{
					var message = Enterprise.Customs.Business.Res.GetString("5FD9B07F-1A3E-4BFA-9506-7A15E24E902F", "You have chosen all tabs.");
					TabPageInfo.AddError(message);
				}

				if (parentCollection.Cast<DeclarationTabLockInfo>().Any(c => c != this && c.TabPage == TabPage && c.CurrentFallbackLevel == CurrentFallbackLevel))
				{
					var message = Enterprise.Customs.Business.Res.GetString("0820C762-9546-4753-9723-ABE3EBC2282D", "There is an existing item with {0}", TabPage);
					TabPageInfo.AddError(message);
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateTabPage();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.TabPage, TabPage);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			TabPage = reader.ReadElementString(Schema.TabPage);
		}

		#endregion

		#region Override

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new DeclarationTabLockInfo(fallbackLevel, factory);
		#endregion
	}
}
