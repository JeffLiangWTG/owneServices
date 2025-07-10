using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes
{
	[XmlSerializerAssembly("Enterprise.TransportCommon.Registry.XmlSerializers")]
	public class MobilityDocumentType : AutoMobilityDocumentType
	{
		public MobilityDocumentType() { }

		public MobilityDocumentType(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		#region Properties

		[List("RefDocTypes")]
		public override ZGuid RT_PK
		{
			get { return base.RT_PK; }
			set
			{
				base.RT_PK = value;
				base.RT_DocType = DocType == null ? ZString.Empty : DocType.RT_DocType;
			}
		}

		public override ZString RT_DocType
		{
			get { return base.RT_DocType; }
			set
			{
				CheckMaximumLength(RT_DocTypeInfo, value);
				UseDocTypeToSetPK(value);
				base.RT_DocType = value;
			}
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
					var relevantReferenceTypes = new[] { Core.Constants.ReferenceTypes.SupplyChainLogistics };
					var query = new ZQuery(RefDocTypeSchema.RT_ReferenceType, relevantReferenceTypes);
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
			return new MobilityDocumentType(fallbackLevel, factory);
		}

		#endregion

		void UseDocTypeToSetPK(ZString docTypeCode)
		{
			RT_PK = ZGuid.Empty;

			if (docTypeCode == string.Empty)
			{
				return;
			}

			var refDocType = new RefDocTypeCollection(CurrentFactory).Find(t => t.RT_DocType == docTypeCode).FirstOrDefault();
			if (refDocType != null)
			{
				RT_PK = refDocType.PK;
			}
		}
	}
}
