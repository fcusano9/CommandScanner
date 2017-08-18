﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;

namespace CommandScanner.HelperClasses
{
	internal class ConnectionService
	{
		#region Constructors

		/// <summary>
		/// Establish a new SSH or CTP client
		/// </summary>
		/// <param name="hostName">The host name or IP sddress of the device you wish to connect to</param>
		/// <param name="connectionType"></param>
		public ConnectionService(string hostName, ConnectionType connectionType)
		{
			DeviceConnectionType = connectionType;

			switch (DeviceConnectionType)
			{
				case ConnectionType.Ssh:
					Address = hostName;
					Port = 22;

					const string userName = "crestron";
					const string password = "";
					var passwordAuthentication = new PasswordAuthenticationMethod(userName, password);

					DeviceConnectionInfo = new ConnectionInfo(Address, Port, userName, passwordAuthentication);
					break;

				case ConnectionType.Ctp:
					Address = hostName;
					Port = 41795;
					break;
			}
		}

		#endregion

		#region Fields

		private SshClient _sshClient;

		private TcpClient _ctpClient;

		private ConnectionInfo DeviceConnectionInfo { get; }
		private ConnectionType DeviceConnectionType { get; }
		private string Address { get; }
		private int Port { get; }

		#endregion

		#region Destructors

		/// <summary>
		/// Dispose of the SSH/CTP client
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
		}

		#endregion

		#region Methods

		private void Connect()
		{
			switch (DeviceConnectionType)
			{
				case ConnectionType.Ssh:
					_sshClient = new SshClient(DeviceConnectionInfo);
					_sshClient.Connect();
					break;

				case ConnectionType.Ctp:
					_ctpClient = new TcpClient(Address, Port);
					_ctpClient.Connect(Address, Port);
					break;
			}
		}

		/// <summary>
		/// Connect to the client and send a command
		/// </summary>
		/// <param name="inputCommand">The command with aguments</param>
		/// <returns>The command response</returns>
		public string SendCommand(string inputCommand)
		{
			try
			{
				string commandResult;

				switch (DeviceConnectionType)
				{
					case ConnectionType.Ssh:
						using (var sshClient = new SshClient(DeviceConnectionInfo))
						{
							sshClient.Connect();
							commandResult = SendCommand(sshClient, inputCommand);
							sshClient.Disconnect();
						}
						return commandResult;

					case ConnectionType.Ctp:
						using(var ctpClient = new TcpClient(Address, Port))
						{
							NetworkStream stream = ctpClient.GetStream();
							commandResult = SendCommand(stream, inputCommand);
							stream.Close();
						}
						return commandResult;

					case ConnectionType.Auto:
						// TODO

					default:
					{
						return null;
					}
				}
			}
			catch (Exception exception)
			{
				return exception.Message;
			}
		}

		private string SendCommand(SshClient sshClient, string inputCommand)
		{
			SshCommand result = sshClient.RunCommand(inputCommand);
			if (result.ExitStatus != 0)
				throw new Exception($"Send Command Error: {result.Error} for command {inputCommand}");

			return result.Result;
		}

		private string SendCommand(NetworkStream stream, string inputCommand)
		{
			var writer = new StreamWriter(stream);
			var reader = new StreamReader(stream);

			writer.WriteLine(inputCommand);
			writer.Flush();

			var prompt = ">";
			var size = 4096;
			var buffer = new char[size];
			var resultString = new StringBuilder();

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			while (!resultString.ToString().Contains(prompt) && stopwatch.ElapsedMilliseconds < 30000)
			{
				Thread.Sleep(200); // wait for the buffer to fill up
				while (stream.DataAvailable)
				{
					try
					{
						int readsize = reader.Read(buffer, 0, buffer.Length);
						resultString.Append(buffer, 0, readsize);
					}
					catch (Exception exception)
					{
						throw exception;
					}
				}
			}

			stopwatch.Stop();
			return resultString.ToString();
		}



		#endregion
	}
}