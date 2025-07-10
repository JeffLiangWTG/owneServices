using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	#region DepartmentMapping

	public class DepartmentMapping : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string Dept1Code = "Dept1Code";
			public const string Dept2Code = "Dept2Code";
		}

		#endregion

		public DepartmentMapping(string dept1, string dept2, DepartmentMappingCollection parent) : base(new BusinessObjectFactory())
		{
			fDept1Code = dept1;
			fDept2Code = dept2;
			this.Parent = parent;
		}

		#region Properties

		#region Dept1Code

		protected ZString fDept1Code;
		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Dept1Code
		{
			get { return fDept1Code; }
			set
			{
				if (fDept1Code != value)
				{
					CheckMaximumLength(Dept1CodeInfo, value);
					fDept1Code = value;
					if (!IsValidationSuspended)
					{
						ValidateValidDepartment(Dept1CodeInfo);
					}
					Dept1CodeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo Dept1CodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Dept1Code); }
		}

		#endregion

		#region Dept2Code

		protected ZString fDept2Code;
		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Dept2Code
		{
			get { return fDept2Code; }
			set
			{
				if (fDept2Code != value)
				{
					CheckMaximumLength(Dept2CodeInfo, value);
					fDept2Code = value;
					if (!IsValidationSuspended)
					{
						ValidateValidDepartment(Dept2CodeInfo);
					}
					Dept2CodeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo Dept2CodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Dept2Code); }
		}

		#endregion

		#endregion

		#region Validation

		public void ValidateValidDepartment(ZPropertyInfo info)
		{
			info.ClearAllNotifications();
			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info, Departments);
		}

		#endregion

		#region Collections

		public GlbDepartmentCollection Departments
		{
			get { return Parent.Departments; }
		}

		#endregion

		#region To/From String

		public static DepartmentMapping FromString(string value, DepartmentMappingCollection parent)
		{
			try
			{
				int separator = value.IndexOf('|');
				if (separator >= 0)
				{
					ZString dept1 = value.Substring(0, separator);
					ZString dept2 = value.Substring(separator + 1);
					return new DepartmentMapping(dept1, dept2, parent);
				}
			}
			catch (ArgumentNullException)
			{
			}
			catch (FormatException)
			{
			}

			return null;
		}

		public override string ToString()
		{
			return Dept1Code.ToString() + "|" + Dept2Code.ToString();
		}

		#endregion

		readonly DepartmentMappingCollection Parent;
	}

	#endregion

	#region DepartmentMappingCollection

	public class DepartmentMappingCollection : NonPersistentBusinessObjectCollection<DepartmentMapping>
	{
		public DepartmentMappingCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DepartmentMappingCollection(string value) : this(new BusinessObjectFactory())
		{
			Load(value);
		}

		public void Load(string value)
		{
			string[] mappingStrings = value.Split(';');
			foreach (string mappingString in mappingStrings)
			{
				DepartmentMapping mapping = DepartmentMapping.FromString(mappingString, this);
				if (mapping != null)
				{
					Add(mapping);
				}
			}
		}

		public override string ToString()
		{
			string result = "";

			foreach (DepartmentMapping elem in this)
			{
				if (!string.IsNullOrEmpty(result))
				{
					result += ";";
				}

				result += elem.ToString();
			}

			return result;
		}

		public ZString GetMapping(ZString department)
		{
			foreach (DepartmentMapping elem in this)
			{
				if (!elem.Dept1Code.IsEmpty && !elem.Dept2Code.IsEmpty)
				{
					if (elem.Dept1Code == department)
					{
						return elem.Dept2Code;
					}
				}
			}

			return ZString.Empty;
		}

		#region Locations

		GlbDepartmentCollection fDepartments;
		public GlbDepartmentCollection Departments
		{
			get
			{
				if (fDepartments == null)
				{
					fDepartments = new GlbDepartmentCollection(Factory);
				}
				return fDepartments;
			}
		}

		#endregion

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DepartmentMapping(null, null, this);
		}
	}

	#endregion

	#region DepartmentMappingCollectionWrapper

	public class DepartmentMappingCollectionWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DepartmentMappingCollectionWrapper(string value)
		{
			fMappings = new DepartmentMappingCollection(value);
		}

		readonly DepartmentMappingCollection fMappings;
		public DepartmentMappingCollection Mappings
		{
			get { return fMappings; }
		}
	}

	#endregion
}
