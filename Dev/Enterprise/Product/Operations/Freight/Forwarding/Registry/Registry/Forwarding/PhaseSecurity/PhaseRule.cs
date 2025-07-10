using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class PhaseRule : RegistryBusinessObjectTemplate, IPhaseRule
	{
		public PhaseRule()
			: this(null)
		{
		}

		public PhaseRule(Phase parent)
			: base()
		{
			Parent = parent;
			DepartmentPK = ZGuid.Empty;
		}

		public Phase Parent { get; set; }

		#region Schema

		public abstract class Schema
		{
			public const string DepartmentPK = "DepartmentPK";
			public const string Location = "Location";
		}

		#endregion

		#region Properties

		#region DepartmentPK

		[List("Departments")]
		public ZGuid DepartmentPK
		{
			get { return departmentPK; }
			set
			{
				SetNonPersistentPropertyValue(DepartmentPKInfo, ref departmentPK, value);
				if (!IsValidationSuspended)
				{
					ValidateDepartmentPK();
				}
			}
		}

		ZGuid departmentPK;

		public ZPropertyInfo DepartmentPKInfo
		{
			get { return GetZPropertyInfo(Schema.DepartmentPK); }
		}

		void ValidateDepartmentPK()
		{
			DepartmentPKInfo.ClearAllNotifications();
			if (!DepartmentPK.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(DepartmentPKInfo, Departments);
				ValidateUniqueDepartmentAndLocation(DepartmentPKInfo);
			}
		}

		#endregion

		#region Location

		[List("Locations")]
		[MaxLength(100)]
		public ZString Location
		{
			get { return location; }
			set
			{
				SetNonPersistentPropertyValue(LocationInfo, ref location, value);
				if (!IsValidationSuspended)
				{
					ValidateLocation();
				}
			}
		}

		ZString location;

		public ZPropertyInfo LocationInfo
		{
			get { return GetZPropertyInfo(Schema.Location); }
		}

		void ValidateLocation()
		{
			LocationInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LocationInfo);
			ListValidation.ErrorIfInvalidCode(LocationInfo, Locations);
			ValidateUniqueDepartmentAndLocation(LocationInfo);
		}

		#endregion

		#region DepartmentAndLocation

		ZString DepartmentAndLocation
		{
			get
			{
				var locationStr = string.IsNullOrWhiteSpace(Location) ? ZString.Empty : Location;
				var departmentStr = DepartmentPK.IsEmpty ? string.Empty : DepartmentPK.ToString();
				return departmentStr + locationStr;
			}
		}

		void ValidateUniqueDepartmentAndLocation(ZPropertyInfo info)
		{
			var departmentAndLocation = DepartmentAndLocation;
			if (!string.IsNullOrWhiteSpace(departmentAndLocation) && Parent != null)
			{
				if (Parent.Rules.Cast<PhaseRule>().Any(c => c != this && c.DepartmentAndLocation == departmentAndLocation))
				{
					info.AddError(Res.GetString(
						"70e52192-d70a-4a81-80e6-090bc39edf86",
						"{0} {1} - Same Department and Location have already been set for this Phase Code.",
						Parent.Code,
						Parent.HumanReadableName));
				}
			}
		}

		#endregion

		#region DependantsText

		[MaxLength(100)]
		public ZString DependantsText
		{
			get { return Dependants.Any() ? Res.GetString("9283db6e-6cec-4bfd-ad64-51360f9d9796", "Properties customized") : Res.GetString("1af54def-d9cf-4678-b881-4611f6e419fb", "No customization"); }
		}

		public ZPropertyInfo DependantsTextInfo
		{
			get { return GetZPropertyInfo(nameof(DependantsText)); }
		}

		#endregion

		#endregion

		#region Dependants

		[BusinessObjectTestExclude]
		public PhaseDependantCollection Dependants
		{
			get { return dependants ?? (dependants = new PhaseDependantCollection()); }
			private set
			{
				dependants = value;
				RegisterEditableChildObject(dependants);
			}
		}
		PhaseDependantCollection dependants;

		#endregion

		#region MandatoryDependants

		public IEnumerable<PhaseDependant> MandatoryDependants
		{
			get { return Dependants.Cast<PhaseDependant>().Where(x => x.IsMandatory); }
		}

		#endregion

		#region ReadOnlyDependants

		public IEnumerable<PhaseDependant> ReadOnlyDependants
		{
			get { return Dependants.Cast<PhaseDependant>().Where(x => x.IsReadOnly); }
		}

		#endregion

		#region Lookups

		public GlbDepartmentCollection Departments
		{
			get { return departments ?? (departments = (new GlbDepartmentCollection(CurrentFactory, new ZQuery(GlbDepartmentSchema.GE_IsActive, ZBool.True)))); }
		}
		GlbDepartmentCollection departments;

		public CodeDescriptionPairList Locations
		{
			get { return Parent != null && Parent.Parent != null ? Parent.Parent.RuleLocations : new CodeDescriptionPairList(); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDepartmentPK();
			ValidateLocation();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.DepartmentPK, departmentPK.ToString());
			writer.WriteElementString(Schema.Location, Location);
			DependantsSerialiser.Serialize(writer, Dependants);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			DepartmentPK = new ZGuid(reader.ReadElementString(Schema.DepartmentPK));
			Location = reader.ReadElementString(Schema.Location);
			Dependants = (PhaseDependantCollection)DependantsSerialiser.Deserialize(reader);
		}

		ZXmlSerializer DependantsSerialiser
		{
			get { return dependantsSerialiser ?? (dependantsSerialiser = ZXmlSerializer.New(typeof(PhaseDependantCollection))); }
		}
		ZXmlSerializer dependantsSerialiser;

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			PhaseRule result = new PhaseRule(Parent);
			result.Dependants = (PhaseDependantCollection)Dependants.Clone(fallbackLevel, factory);
			return result;
		}

		#endregion

		#region IPhaseRule Members

		IEnumerable<IPhaseDependant> IPhaseRule.MandatoryDependants
		{
			get { return MandatoryDependants.Cast<IPhaseDependant>(); }
		}

		IEnumerable<IPhaseDependant> IPhaseRule.ReadOnlyDependants
		{
			get { return ReadOnlyDependants.Cast<IPhaseDependant>(); }
		}

		IEnumerable<IPhaseDependant> IPhaseRule.Dependants
		{
			get { return Dependants.Cast<IPhaseDependant>(); }
		}

		#endregion
	}
}
