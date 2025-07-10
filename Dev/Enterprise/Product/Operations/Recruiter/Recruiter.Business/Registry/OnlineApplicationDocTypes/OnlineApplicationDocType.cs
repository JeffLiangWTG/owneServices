using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	[XmlSerializerAssembly("Enterprise.Recruiter.Business.XmlSerializers")]
	public class OnlineApplicationDocType : AutoOnlineApplicationDocType
	{
		// You will need the default constructor for deserialising, but probably only want one of the other constructors.
		// You should delete the rest.

		public OnlineApplicationDocType() { }

		public OnlineApplicationDocType(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		#region Properties

		[List("RefDocTypes")]
		public override ZGuid RT_PK
		{
			get { return base.RT_PK; }
			set { base.RT_PK = value; }
		}

		public ZString DocTypeDescription
		{
			get { return DocType == null ? ZString.Empty : DocType.RT_DescMultilingual; }
		}

		public RefDocType DocType
		{
			get { return CurrentFactory.Load<RefDocType>(RT_PK); }
		}

		public RefDocTypeCollection RefDocTypes
		{
			get
			{
				if (refDocTypes == null)
				{
					string[] relevantReferenceTypes = new[] { Core.Constants.ReferenceTypes.All, Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment };
					ZQuery query = new ZQuery(RefDocTypeSchema.RT_ReferenceType, relevantReferenceTypes);
					refDocTypes = new RefDocTypeCollection(CurrentFactory, query);
				}
				return refDocTypes;
			}
		}
		RefDocTypeCollection refDocTypes;

		#endregion

		#region Validation

		public override void ValidateRT_PK()
		{
			base.ValidateRT_PK();
			MandatoryValidation.CheckEntered(RT_PKInfo);
			ListValidation.ErrorIfInvalidPK(RT_PKInfo);
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OnlineApplicationDocType(fallbackLevel, factory);
		}

		#endregion
	}
}
