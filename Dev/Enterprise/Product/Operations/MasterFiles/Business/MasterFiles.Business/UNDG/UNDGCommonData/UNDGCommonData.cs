using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(UNDGCommonData.Schema.DC_Type)]
	[DescriptionProperty(UNDGCommonData.Schema.DC_Descriptor)]
	[ProvideMetaDataProperty("ReadOnly", MetaDataTypes.ReadOnly)]
	public class UNDGCommonData : AutoUNDGCommonData, ITemplateCopyable
	{
		public UNDGCommonData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ReadOnly

		protected bool GetReadOnly(PropertyDescriptor property)
		{
			bool result = DC_Language == Core.Constants.Languages.English;
			result = result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return result;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (DC_Language == Core.Constants.Languages.English && IsInDatabase)
			{
				throw new CannotDeleteException("English details cannot be changed or deleted for system defined Dangerous Goods.");
			}
			else
			{
				base.Delete();
			}
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Properties

		[List("Lookups.Languages")]
		public override ZString DC_Language
		{
			get { return base.DC_Language; }
			set { base.DC_Language = value; }
		}

		[List("Lookups.Types")]
		public override ZString DC_Type
		{
			get { return base.DC_Type; }
			set { base.DC_Type = value; }
		}

		public ZString TypeDescription
		{
			get { return Lookups.Types.GetDescriptionFromCode(DC_Type); }
		}

		protected override ZString HumanReadableNameCore => Res.GetString("07A2F711-27D5-4197-B723-5EF471CAA84C", "Dangerous Goods Common Provisions - {0}", CalculateShortcutName());

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			UNDGCommonData data = Factory.New<UNDGCommonData>();
			data.DC_Index = DC_Index;
			data.DC_Type = DC_Type;
			data.DC_Language = ZString.Empty;
			data.DC_Descriptor = DC_Descriptor;

			return data;
		}

		#endregion
	}
}
