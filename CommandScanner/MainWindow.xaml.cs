using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CommandScanner.HelperClasses;
using System.ComponentModel;
using System.Linq;

namespace CommandScanner
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		#region Properties & Fields

		private ConnectionService Connection { get; set; }
		private ConnectionType _connectionType;

		#endregion

		#region Constructors

		public MainWindow()
		{
			InitializeComponent();
		}

		#endregion

		#region Event Handlers

		#region Host Name & Connection Type

		/// <summary>
		/// Sets the connection type based on what the user chooses
		/// </summary>
		private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var comboBox = (ComboBox)sender;
			var selection = (string)comboBox.SelectedItem;

			switch (selection)
			{
				case "SSH":
					_connectionType = ConnectionType.Ssh;
					break;
				case "CTP":
					_connectionType = ConnectionType.Ctp;
					break;
				case "Auto Detect":
					_connectionType = ConnectionType.Auto;
					break;
			}
		}

		/// <summary>
		/// Loads the available selections to the connectionTypeBox
		/// </summary>
		private void comboBox_Loaded(object sender, RoutedEventArgs e)
		{
			//var data = new List<string> { "Auto Detect", "SSH", "CTP" };
			var data = new List<string> { "SSH" };

			var comboBox = (ComboBox)sender;

			comboBox.ItemsSource = data;
			comboBox.SelectedIndex = 0;
		}

		private void hostname_ContentChanged(object sender, TextChangedEventArgs textChangedEventArgs)
		{
			ScanDevice.IsEnabled = false;
		}

		#endregion

		#region Connect Button

		/// <summary>
		/// Establish a connection with the device
		/// </summary>
		private void connect_Click(object sender, RoutedEventArgs e)
		{
			// TODO add a 'connecting...' prompt

			var address = HostName.Text;
			Connection = new ConnectionService(address, _connectionType);

			var connected = Connection.Connect();

			if (connected)
				ScanDevice.IsEnabled = true;

			// TODO change prompt to say 'connected' if successful
		}

		#endregion

		#region Scan Button

		/// <summary>
		/// Scan the device for the commands
		/// </summary>
		private void button_Click(object sender, RoutedEventArgs e)
		{
			BackgroundWorker worker = new BackgroundWorker();
			worker.RunWorkerCompleted += worker_RunWorkerCompleted;
			worker.WorkerReportsProgress = true;
			worker.DoWork += worker_DoWork;
			worker.ProgressChanged += worker_ProgressChanged;
			worker.RunWorkerAsync();
		}

		#endregion

		#endregion

		#region Private Methods

		/// <summary>
		/// Update the progress bar
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			ScanningProgress.Value = e.ProgressPercentage;
			ProgressTextBlock.Text = (string)e.UserState;
		}

		/// <summary>
		/// Scans the device for commands when the Scan button is clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				ScanningText.Visibility = Visibility.Visible;
				ScanningProgress.Visibility = Visibility.Visible;
				ProgressTextBlock.Visibility = Visibility.Visible;
			});

			var worker = (BackgroundWorker)sender;
			worker.ReportProgress(0, "Processing Command:");

			List<Command> commands = GetAllCommands(worker);
			WriteCommandsToFile(commands);

			Dispatcher.Invoke(() => { ScanningText.Visibility = Visibility.Hidden; });

			worker.ReportProgress(100, "Scanning Complete!");
		}

		private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			MessageBox.Show("Finished Scanning Commands");
			ScanningProgress.Value = 0;
			ProgressTextBlock.Text = "";

			ScanningProgress.Visibility = Visibility.Hidden;
			ProgressTextBlock.Visibility = Visibility.Hidden;
		}

		/// <summary>
		/// Search the control system for all hidden and visible commands
		/// </summary>
		/// <param name="worker"></param>
		/// <returns></returns>
		private List<Command> GetAllCommands(BackgroundWorker worker)
		{
			const string helpCommand = "help all";
			const string hidHelpCommand = "hidhelp all";

			var visibleCommands = new List<Command>();
			var hiddenCommands = new List<Command>();
			var allCommands = new List<Command>();

			var visComResult = Connection.SendCommand(helpCommand);
			var allComResult = Connection.SendCommand(hidHelpCommand);

			var visibleComs = visComResult.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			var allComs = allComResult.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

			int commandTotal = allComs.Length;
			int commandNum = 1;

			// Create a list of all of the visible commands
			foreach (var command in visibleComs)
			{
				// Parse the list of commands for the Command info
				var data = command.Trim().Split(new[] { "  " }, StringSplitOptions.RemoveEmptyEntries);

				Command com = CreateCommand(data, false);
				if (com == null)
					continue;

				visibleCommands.Add(com);
				worker.ReportProgress(commandNum * 100 / commandTotal, $"Processing Command: \"{com?.Name}\"");
				commandNum += 1;
			}

			// Create a list of all of the hidden commands
			foreach (var command in allComs)
			{
				var data = command.Trim().Split(new[] { "  " }, StringSplitOptions.RemoveEmptyEntries);

				// Check to see if command is already in the list of visible commands which means it is not a hidden command
				bool found = visibleCommands.Any(c => c.Name == data[0].Trim());
				if (found)
					continue;


				Command com = CreateCommand(data, true);
				if (com == null)
					continue;

				hiddenCommands.Add(com);
				worker.ReportProgress(commandNum * 100 / commandTotal, $"Processing Command: \"{com?.Name}\"");
				commandNum += 1;
			}

			allCommands.AddRange(visibleCommands);
			allCommands.AddRange(hiddenCommands);

			return allCommands;
		}

		private Command CreateCommand(string[] commandInfo, bool isHidden)
		{
			Command com;
			if (commandInfo.Length == 3)
			{
				com = new Command(commandInfo[0].Trim(), commandInfo[1].Trim(), commandInfo[2].Trim(), isHidden);
				GetCommandHelp(ref com);
				return com;
			}
			if (commandInfo.Length == 2)
			{
				com = new Command(commandInfo[0].Trim(), "none", commandInfo[1].Trim(), isHidden);
				GetCommandHelp(ref com);
				return com;
			}
			return null;
		}

		/// <summary>
		/// Get the command help by calling the command followed by '?'
		/// </summary>
		/// <param name="command"></param>
		private void GetCommandHelp(ref Command command)
		{
			var result = Connection.SendCommand($"{command.Name} ?").Trim();

			if (!CheckIfValidHelp(result))
			{
				result = Connection.SendCommand($"{command.Name} help").Trim();

				if (!CheckIfValidHelp(result))
				{
					result = "There is no help for this command.";
				}
			}

			command.Help = result;
		}

		/// <summary>
		/// Verify that the help value returned valid
		/// </summary>
		/// <param name="helpResult"></param>
		/// <returns></returns>
		private static bool CheckIfValidHelp(string helpResult)
		{
			return helpResult != "\r\n" && helpResult != "\r" && helpResult != "\n" && helpResult != "" && helpResult != " "
				&& helpResult != "Authentication is not on. Command not allowed.";
		}

		/// <summary>
		/// Write all of the commands to an easy to read HTML file
		/// </summary>
		/// <param name="commands"></param>
		private void WriteCommandsToFile(List<Command> commands)
		{
			string fileName = "";
			Dispatcher.Invoke(() => { fileName = HostName.Text; });

			//var fileName = HostName.Text;
			const string path = @"..\..\..\Scans";
			var file = $"{fileName}.html";
			var fullPath = Path.Combine(path, file);

			Directory.CreateDirectory(path);
			FileStream fs;
			try
			{
				fs = File.OpenWrite(fullPath);
				fs.Close();
			}
			catch
			{
				fs = File.Create(fullPath);
				fs.Close();
			}

			var htmlFile = new StringBuilder();

			htmlFile.AppendLine("<!DOCTYPE HTML>");
			htmlFile.AppendLine("<html>");
			htmlFile.AppendLine("<head>  <title>TEST FILE</title>");
			htmlFile.AppendLine("    <meta name=\"utility_author\" content=\"Frank Cusano\">");
			htmlFile.AppendLine("    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">");
			htmlFile.AppendLine("    <style type=\"text/css\">");
			htmlFile.AppendLine("        table { page-break-inside:auto }");
			htmlFile.AppendLine("        tr    { page-break-inside:avoid; page-break-after:auto }");
			htmlFile.AppendLine("        thead { display:table-header-group }");
			htmlFile.AppendLine("        tfoot { display:table-footer-group }");
			htmlFile.AppendLine("    </style>");
			htmlFile.AppendLine("</head>\n<body>\n<font face='arial'>");
			htmlFile.AppendLine("<h1>This is a test</h1>");
			htmlFile.AppendLine($"<p>&nbsp;&nbsp; {commands.Count} normal commands found.</p>");
			htmlFile.AppendLine("<table border=\"1\" cellpadding=\"5\" cellspacing=\"5\" style=\"border-collapse:collapse;\" width=\"100%\">\n");

			string color = "#000000";
			foreach (var command in commands)
			{
				if (command.IsHidden)
					color = "#0000FF";

				htmlFile.AppendLine("<tr bgcolor=\"#C0C0C0\">");
				htmlFile.AppendLine($"    <th width=\"20%\"><font color=\"{color}\">{command.Name}</font></th>");
				htmlFile.AppendLine($"    <th align=\"left\"><font color=\"{color}\">{command.Description} </font></th>");
				htmlFile.AppendLine("</tr>");

				htmlFile.AppendLine("<tr>");
				htmlFile.AppendLine("<td colspan=\"2\">");
				htmlFile.AppendLine($"<pre>\n{command.Help}\n</pre>");
				htmlFile.AppendLine("</td>");
				htmlFile.AppendLine("</tr>\n");
			}
			htmlFile.AppendLine("</table>\n</font>\n</body>\n</html>");

			File.WriteAllText(fullPath, htmlFile.ToString());
		}

		#endregion
	}
}