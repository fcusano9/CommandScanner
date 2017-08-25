using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		#region Constructor

		public Command(string name, string accessLevel, string description)
		{
			Name = name;
			AccessLevel = accessLevel;
			Description = description;
		}

		#endregion

		public override string ToString()
		{
			return $"{Name}";
		}
	}
}
