using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGAttributeZZ : AutoUNDGAttributeZZ
	{
		public UNDGAttributeZZ(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		[List("Lookups.Languages")]
		[ResourceStringData("UNDGAttributeZZ|DAZ_Language", Caption = "Language", ShortCaption = "Lang", FullDescription = "The Language of this detail.")]
		public override ZString DAZ_Language
		{
			get => base.DAZ_Language;
			set => base.DAZ_Language = value;
		}

		[ResourceStringData("UNDGAttributeZZ|DAZ_Descriptor", Caption = "Details", ShortCaption = "Details")]
		public override ZString DAZ_Descriptor
		{
			get => base.DAZ_Descriptor;
			set => base.DAZ_Descriptor = value;
		}
	}
}
