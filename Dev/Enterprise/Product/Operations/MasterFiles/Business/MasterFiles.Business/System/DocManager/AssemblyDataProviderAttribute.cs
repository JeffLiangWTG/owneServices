using CargoWise.Definitions;

namespace Enterprise.ZArchitecture.Modules.DocumentScanning
{
	using System;
	using System.Xml.Serialization;

	[Serializable]
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public sealed class AssemblyDataProviderAttribute : AssemblyMetaDataAttributeWithType, IAssemblyDataProvider, IEquatable<AssemblyDataProviderAttribute>
	{
		public AssemblyDataProviderAttribute()
		{ }

		public AssemblyDataProviderAttribute(Type type, string docManagerCode)
			: base(type)
		{
			DocManagerCode = docManagerCode;
		}

		/// <summary>
		/// 3-letter code used for lookup data. Must be unique for each business object, and must match the DocManagerCode implemented in IDocManagerSupport.
		/// Stored in StorageMain.SM_Type.
		/// </summary>
		public string DocManagerCode { get; set; }

		public string Country { get; set; }

		#region Equality members

		public bool Equals(AssemblyDataProviderAttribute other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other) && (DocManagerCode, Country).Equals((other.DocManagerCode, other.Country));
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is AssemblyDataProviderAttribute other && Equals(other);
		}

		public override int GetHashCode() => (base.GetHashCode(), DocManagerCode, Country).GetHashCode();

		#endregion
	}
}
