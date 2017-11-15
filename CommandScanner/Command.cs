
namespace CommandScanner
{
	internal class Command
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

		public override string ToString()
		{
			return $"{Name}";
		}

		#endregion
	}
}