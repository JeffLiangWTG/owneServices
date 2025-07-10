using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class DocAddressCreatorHost : AutoDocAddressCreatorHost
	{
		public delegate ZString DocAddressTypeCodeFormatter(DocAddressType docAddressType);

		public DocAddressCreatorHost(IDocAddresses docAddressParent, DocAddressTypeCodeFormatter docAddressTypeCodeFormatter, DocAddressType defaultDocAddressType, BusinessObjectFactory factory)
			: base(factory)
		{
			using (SuspendSettingHasChanges())
			{
				DocAddressParent = docAddressParent;
				CodeFormatter = docAddressTypeCodeFormatter;
				AddressTypeCode = CodeFormatter(defaultDocAddressType);
				IsEditingExistingJobDocAddress = false;
			}
		}

		/// <summary>
		/// Use this constructor if you want to edit existing JobDocAddress on QuickAddressForm. 
		/// </summary>
		/// <param name="docAddressToEdit">JobDocAddress that will be modified.</param>
		/// <param name="docAddressTypeCodeFormatter">Display format of address.</param>
		/// <param name="factory"></param>
		public DocAddressCreatorHost(JobDocAddress docAddressToEdit, BusinessObjectFactory factory)//, DocAddressTypeCodeFormatter docAddressTypeCodeFormatter, BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(docAddressToEdit, "docAddressToEdit");

			using (SuspendSettingHasChanges())
			{
				docAddress = docAddressToEdit;
				DocAddressParent = DocAddress.Parent;
				RefreshAddressCaption();
				IsEditingExistingJobDocAddress = true;
			}
		}

		public DocAddressTypeCodeFormatter CodeFormatter;
		public IDocAddresses DocAddressParent { get; private set; }
		public readonly bool IsEditingExistingJobDocAddress;

		#region AddressTypeCode

		[BusinessObjectTestExclude()]
		[List("Lookups.AddressTypes")]
		public override ZString AddressTypeCode
		{
			get { return base.AddressTypeCode; }
			set
			{
				base.AddressTypeCode = value;
				ZString validAdressTypeCode = Lookups.AddressTypes.ContainsCode(AddressTypeCode) ? AddressTypeCode : ZString.Empty;
				DocAddress.DocAddressType = GetDocAddressType(validAdressTypeCode);
				RefreshAddressCaption();
			}
		}

		#endregion

		#region DocAddress

		public JobDocAddress DocAddress
		{
			get
			{
				if (DocAddressParent != null && (docAddress == null || docAddress.IsDeleted))
				{
					docAddress = DocAddressParent.DocAddresses.AddNew(GetDocAddressType(AddressTypeCode));
					using (docAddress.SuspendSettingHasChanges())
					{
						var req = new JobDocAddressRequirement();
						req.IsMandatory = true;
						req.DefaultMax = 0;
						docAddress.OverrideRequirement = req;
					}

					RegisterEditableChildObject(docAddress);
				}
				return docAddress;
			}
		}
		JobDocAddress docAddress;

		#endregion

		#region RefreshAddressCaption

		void RefreshAddressCaption()
		{
			AddressCaption = DocAddressTypes.GetDescription(Factory, DocAddress.DocAddressType);
		}

		#endregion

		#region GetDocAddressType

		DocAddressType GetDocAddressType(ZString addressTypeCode)
		{
			ZString docAddressTypeCode;
			DocAddressTypesByAddressTypeCode.TryGetValue(addressTypeCode, out docAddressTypeCode);
			return DocAddressTypes.GetDocAddressTypeFromCode(Factory, docAddressTypeCode);
		}

		#endregion

		#region DocAddressTypesByAddressTypeCode

		Dictionary<ZString, ZString> DocAddressTypesByAddressTypeCode
		{
			get
			{
				if (docAddressTypesByAddressTypeCode == null)
				{
					docAddressTypesByAddressTypeCode = new Dictionary<ZString, ZString>();

					if (DocAddressParent != null && CodeFormatter != null)
					{
						foreach (DocAddressType addrType in DocAddressParent.SupportedAddressTypes)
						{
							docAddressTypesByAddressTypeCode.Add(CodeFormatter(addrType), DocAddressTypes.GetCode(Factory, addrType));
						}
					}
				}
				return docAddressTypesByAddressTypeCode;
			}
		}
		Dictionary<ZString, ZString> docAddressTypesByAddressTypeCode;

		#endregion

		#region Lookups

		public DocAddressCreatorHostLookups Lookups
		{
			get { return lookups ?? (lookups = new DocAddressCreatorHostLookups(this)); }
		}
		DocAddressCreatorHostLookups lookups;

		#endregion
	}
}
