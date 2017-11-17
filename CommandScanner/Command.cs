
using System;

namespace CommandScanner
{
	internal class Command : IComparable<Command>
	{
		#region Constructors

		public Command(string name, string accessLevel, string description)
		{
			Name = name;
			AccessLevel = accessLevel;
			Description = description;
			IsHidden = false;
		}

		public Command(string name, string accessLevel, string description, bool hidden)
		{
			Name = name;
			AccessLevel = accessLevel;
			Description = description;
			IsHidden = hidden;
		}

		#endregion

		#region Properties & Fields

		public string Name { get; }
		public string AccessLevel { get; }
		public string Description { get; }
		public string Help { get; set; }
		public bool IsHidden { get; }

		#endregion

		#region Public Methods

		/// <summary>
		/// Default comparer for sorting Command types
		/// </summary>
		/// <param name="compareCom"></param>
		/// <returns></returns>
		public int CompareTo(Command compareCom)
		{
			return compareCom == null ? 1 : string.Compare(Name, compareCom.Name, StringComparison.OrdinalIgnoreCase);
		}

		public override string ToString()
		{
			return $"{Name}";
		}

		#endregion
	}
}