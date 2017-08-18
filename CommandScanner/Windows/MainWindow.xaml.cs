using CommandScanner.HelperClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommandScanner
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ConnectionType _connectionType;
		private ConnectionService _connection;

		public MainWindow()
		{
			InitializeComponent();
		}


		#region Event Handlers

		#region Connection Type Combo Box

		/// <summary>
		/// Sets the connection type based on what the user chooses
		/// </summary>
		private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var comboBox = (ComboBox)sender;
			string selection = (string)comboBox.SelectedItem;

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
			List<string> data = new List<string>();
			data.Add("Auto Detect");
			data.Add("SSH");
			data.Add("CTP");

			var comboBox = (ComboBox)sender;

			comboBox.ItemsSource = data;
			comboBox.SelectedIndex = 0;
		}

		#endregion

		#region Connect Button

		private void connect_Click(object sender, RoutedEventArgs e)
		{
			string address = hostName.Text;

			_connection = new ConnectionService(address, _connectionType);


		}

		#endregion

		#region Scan Button
		private void button_Click(object sender, RoutedEventArgs e)
		{
			string fileName = hostName.Text;
			string path = $@"C:\Users\fcusano\Documents\Visual Studio 2015\Projects\CommandScanner\{fileName}.html";

			FileStream fs;
			try
			{
				fs = File.OpenWrite(path);
				fs.Close();
			}
			catch
			{
				fs = File.Create(path);
				fs.Close();
			}

			StringBuilder htmlFile = new StringBuilder();

			htmlFile.AppendLine("<!DOCTYPE HTML>");
			htmlFile.AppendLine("<html>");
			htmlFile.AppendLine("<head>  <title>TEST FILE</title>\n  <meta name=\"utility_author\" content=\"Frank Cusano\">");
			htmlFile.AppendLine("  <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">");
			htmlFile.AppendLine("  <style type=\"text/css\">");
			htmlFile.AppendLine("      table { page-break-inside:auto }");
			htmlFile.AppendLine("      tr    { page-break-inside:avoid; page-break-after:auto }");
			htmlFile.AppendLine("      thead { display:table-header-group }");
			htmlFile.AppendLine("      tfoot { display:table-footer-group }");
			htmlFile.AppendLine("  </style>");
			htmlFile.AppendLine("</head>\n<body>\n<font face='arial'>");

			htmlFile.AppendLine("<h1>This is a test</h1>");
			htmlFile.AppendLine("<table border=\"1\" cellpadding=\"5\" cellspacing=\"5\" style=\"border-collapse:collapse;\" width=\"100%\">");
			htmlFile.AppendLine("</table>\n</font>\n</body>\n</html>");


			File.WriteAllText(path, htmlFile.ToString());
		}

		#endregion

		#endregion


	}
}
