
namespace CommandScanner.SystemCommands
{
	internal class Command
	{
		#region Properties & Fields

		public string Name { get; }
		public string AccessLevel { get; }
		public string Description { get; }
		public string Help { get; set; }

		#endregion

		#region Constructors

		public Command(string name, string accessLevel, string description)
		{
			Name = name;
			AccessLevel = accessLevel;
			Description = description;
		}

		#endregion

		#region Public Methods

		public override string ToString()
		{
			return $"{Name}";
		}

		#endregion
	}
}