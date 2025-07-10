using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[SystemDefinedValues]
	[DescriptionProperty("PackageReferenceAndPackageQty")]
	public class WHSPack : AutoWHSPack, ICodeDescription
	{
		public WHSPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString PackageReferenceAndPackageQty
		{
			get { return string.Format("{0} ({1})", US_PackageReference, US_PackageQty); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.WHSPack|US_PackageQty", Caption = "Number Of Packages", ShortCaption = "No. Of Pkgs.")]
		public override ZInt US_PackageQty
		{
			get { return base.US_PackageQty; }
			set { base.US_PackageQty = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.WHSPack|US_PackageReference", Caption = "Package Reference")]
		public override ZString US_PackageReference
		{
			get { return base.US_PackageReference; }
			set
			{
				var oldValue = US_PackageReference;
				base.US_PackageReference = value;
				if (!IsCopying && oldValue != US_PackageReference)
				{
					var declaration = Parent;
					if (declaration != null)
					{
						declaration.WHSPacks.RefreshPackageReferenceList();
					}
				}
			}
		}

		public new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent; }
		}

		public IReadOnlyList<WHSPackLine> WHSPackLines
		{
			get
			{
				if (whsPackLines == null)
				{
					var declaration = Parent;
					if (declaration != null)
					{
						whsPackLines = declaration.WHSPackLines.OfType<WHSPackLine>().Where(x => x.US_B7_WHSPack == PK).ToArray();
					}
				}
				return whsPackLines;
			}
		}
		WHSPackLine[] whsPackLines;

		public void RefreshWHSPackLines()
		{
			whsPackLines = null;
		}

		public override void Delete()
		{
			var declaration = Parent;
			if (declaration != null)
			{
				declaration.WHSPackLines.OfType<WHSPackLine>().Where(x => x.US_B7_WHSPack == PK).DeleteAll();
			}
			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B7_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			UpdateAddInfoProperties();
			return base.CloneInternal(args);
		}

		protected void UpdateAddInfoProperties()
		{
			if (Data != null && HasChanges)
			{
				Data.UpdateRelatedPropertyInfo();
			}
		}

		#region ICodeDescription Members

		string ICodeDescription.Code
		{
			get { return US_PackageReference; }
		}

		string ICodeDescription.Description
		{
			get { return PackageReferenceAndPackageQty; }
		}

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		#endregion
	}
}
