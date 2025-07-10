using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public class EntryMessageLine : NonPersistentBusinessObject, IEntryMessageLineBlocks, IACEEntryLineBlocks, IObsoleteValidation
	{
		public EntryMessageLine(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		// ABI Message
		List<ENS43> line_ens43List;
		List<ENS62> line_ens62List;
		List<ENS81> line_ens81List;

		// ACE Message
		List<AENS43> line_aens43List;
		List<AENS44> line_aens44List;
		List<AENS47> line_aens47List;
		List<AENS50> line_aens50List;
		List<AENS52> line_aens52List;
		List<AENS53> line_aens53List;
		List<AENS54> line_aens54List;
		List<IChargeBlock> line_aens62List;

		internal ZString MPFRate
		{
			get;
			set;
		}

		#region IEntryMessageLineBlocks Members

		public ENS40 ens40
		{
			get;
			set;
		}

		public ENS42 ens42
		{
			get;
			set;
		}

		public List<ENS43> ens43
		{
			get { return line_ens43List ?? new List<ENS43>(); }
			set { line_ens43List = value; }
		}

		public ENS50 ens50
		{
			get;
			set;
		}

		public ENS51 ens51
		{
			get;
			set;
		}

		public ENS52 ens52
		{
			get;
			set;
		}

		public ENS57 ens57
		{
			get;
			set;
		}

		public ENS60 ens60
		{
			get;
			set;
		}

		public List<ENS62> ens62
		{
			get { return line_ens62List ?? new List<ENS62>(); }
			set { line_ens62List = value; }
		}

		public ENS70 ens70
		{
			get;
			set;
		}

		public ENS80 ens80
		{
			get;
			set;
		}

		public List<ENS81> ens81
		{
			get { return line_ens81List ?? new List<ENS81>(); }
			set { line_ens81List = value; }
		}

		#endregion

		#region IACEEntryLineBlocks Members

		public AENS40 aens40
		{
			get;
			set;
		}

		public AENS41 aens41
		{
			get;
			set;
		}

		public List<AENS43> aens43
		{
			get { return line_aens43List ?? new List<AENS43>(); }
			set { line_aens43List = value; }
		}

		public List<AENS44> aens44
		{
			get { return line_aens44List ?? new List<AENS44>(); }
			set { line_aens44List = value; }
		}

		public List<AENS47> aens47
		{
			get { return line_aens47List ?? new List<AENS47>(); }
			set { line_aens47List = value; }
		}

		public List<AENS50> aens50
		{
			get { return line_aens50List ?? new List<AENS50>(); }
			set { line_aens50List = value; }
		}

		public AENS51 aens51
		{
			get;
			set;
		}

		public List<AENS52> aens52
		{
			get { return line_aens52List ?? new List<AENS52>(); }
			set { line_aens52List = value; }
		}

		public List<AENS53> aens53
		{
			get { return line_aens53List ?? new List<AENS53>(); }
			set { line_aens53List = value; }
		}

		public List<AENS54> aens54
		{
			get { return line_aens54List ?? new List<AENS54>(); }
			set { line_aens54List = value; }
		}

		public AENS60 aens60
		{
			get;
			set;
		}

		public List<IChargeBlock> aens62
		{
			get { return line_aens62List ?? new List<IChargeBlock>(); }
			set { line_aens62List = value; }
		}

		#endregion
	}
}
