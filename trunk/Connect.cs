// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Connect.cs" company="">
//   
// </copyright>
// <summary>
//   The connect.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using System;
using System.Reflection;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using JS_addin.Addin.UI;
using Microsoft.VisualStudio.CommandBars;

namespace JS_addin.Addin
{
	/// <summary>
	/// The connect.
	/// </summary>
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
		/// <summary>
		/// The _initialize.
		/// </summary>
		private static bool _initialize;

		/// <summary>
		/// The _navigation tree view.
		/// </summary>
		private NavigationTreeView _navigationTreeView;

		/// <summary>
		/// The document events.
		/// </summary>
		private DocumentEvents documentEvents;

		/// <summary>
		/// The m_addin.
		/// </summary>
		private AddIn m_addin;

		/// <summary>
		/// The m_dte.
		/// </summary>
		private DTE m_dte;

		/// <summary>
		/// The m_tool window.
		/// </summary>
		private Window m_toolWindow;

		/// <summary>
		/// The solution events.
		/// </summary>
		private SolutionEvents solutionEvents;

		/// <summary>
		/// The text editor events.
		/// </summary>
		private TextEditorEvents textEditorEvents;

		/// <summary>
		/// The window events.
		/// </summary>
		private WindowEvents windowEvents;

		#region IDTCommandTarget Members

		/// <summary>
		/// The query status.
		/// </summary>
		/// <param name="commandName">
		/// The command name.
		/// </param>
		/// <param name="neededText">
		/// The needed text.
		/// </param>
		/// <param name="status">
		/// The status.
		/// </param>
		/// <param name="commandText">
		/// The command text.
		/// </param>
		public void QueryStatus(
			string commandName,
			vsCommandStatusTextWanted neededText,
			ref vsCommandStatus status,
			ref object commandText)
		{
			if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{
				bool commandEnabled = false;
				if (commandName == "JS_addin.Addin.Connect.JS_addin")
				{
					commandEnabled = true;
				}

				status = vsCommandStatus.vsCommandStatusSupported;
				if (commandEnabled)
					status |= vsCommandStatus.vsCommandStatusEnabled;
				else
					status |= vsCommandStatus.vsCommandStatusInvisible;
			}
		}

		/// <summary>
		/// The exec method.
		/// </summary>
		/// <param name="commandName">
		/// The command name.
		/// </param>
		/// <param name="executeOption">
		/// The execute option.
		/// </param>
		/// <param name="varIn">
		/// The var in.
		/// </param>
		/// <param name="varOut">
		/// The var out.
		/// </param>
		/// <param name="handled">
		/// The handled.
		/// </param>
		public void Exec(
			string commandName,
			vsCommandExecOption executeOption,
			ref object varIn,
			ref object varOut,
			ref bool handled)
		{
			handled = false;
			if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
			{
				if (commandName == "JS_addin.Addin.Connect.JS_addin")
				{
					ShowWindow();
					handled = true;
					return;
				}
			}
		}

		#endregion

		#region IDTExtensibility2 Members

		/// <summary>
		/// The on connection.
		/// </summary>
		/// <param name="application">
		/// The application.
		/// </param>
		/// <param name="connectMode">
		/// The connect mode.
		/// </param>
		/// <param name="addInInst">
		/// The add in inst.
		/// </param>
		/// <param name="custom">
		/// The custom.
		/// </param>
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			// System.Diagnostics.Debugger.Break();
			m_dte = (DTE) application;
			m_addin = (AddIn) addInInst;

			if (!_initialize)
			{
				var contextGUIDS = new object[] {};
				var commands = (Commands2) m_dte.Commands;
				string appInName = "JS_addin";

				CommandBar menuBarCommandBar = ((CommandBars) m_dte.CommandBars)["MenuBar"];
				CommandBarControl toolsControl = menuBarCommandBar.Controls["Tools"];
				var toolsPopup = (CommandBarPopup) toolsControl;
				try
				{
					Command command = commands.AddNamedCommand2(m_addin, appInName,
					                                            "JS addin", "Shows the javascript utility plugin", true, 59,
					                                            ref contextGUIDS,
					                                            (int) vsCommandStatus.vsCommandStatusSupported +
					                                            (int) vsCommandStatus.vsCommandStatusEnabled,
					                                            (int) vsCommandStyle.vsCommandStylePictAndText,
					                                            vsCommandControlType.vsCommandControlTypeButton);

					if ((command != null) && (toolsPopup != null))
						command.AddControl(toolsPopup.CommandBar, 1);
				}
				catch (ArgumentException e)
				{
				}

				Events events = m_dte.Events;
				documentEvents = events.get_DocumentEvents(null);
				textEditorEvents = events.get_TextEditorEvents(null);
				solutionEvents = events.SolutionEvents;
				windowEvents = events.get_WindowEvents(null);

				documentEvents.DocumentClosing += documentEvents_DocumentClosing;
				documentEvents.DocumentOpened += documentEvents_DocumentOpened;
				documentEvents.DocumentSaved += documentEvents_DocumentSaved;
				solutionEvents.Opened += solutionEvents_Opened;
				windowEvents.WindowActivated += windowEvents_WindowActivated;
				_initialize = true;
			}
		}

		/// <summary>
		/// The on disconnection.
		/// </summary>
		/// <param name="disconnectMode">
		/// The disconnect mode.
		/// </param>
		/// <param name="custom">
		/// The custom.
		/// </param>
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		/// <summary>
		/// The on add ins update.
		/// </summary>
		/// <param name="custom">
		/// The custom.
		/// </param>
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>
		/// The on startup complete.
		/// </summary>
		/// <param name="custom">
		/// The custom.
		/// </param>
		public void OnStartupComplete(ref Array custom)
		{
		}

		/// <summary>
		/// The on begin shutdown.
		/// </summary>
		/// <param name="custom">
		/// The custom.
		/// </param>
		public void OnBeginShutdown(ref Array custom)
		{
		}

		#endregion

		/// <summary>
		/// The window events_ window activated.
		/// </summary>
		/// <param name="GotFocus">
		/// The got focus.
		/// </param>
		/// <param name="LostFocus">
		/// The lost focus.
		/// </param>
		private void windowEvents_WindowActivated(Window GotFocus, Window LostFocus)
		{
			if (GotFocus == null || GotFocus.Kind != "Document") return;
			if (_navigationTreeView == null || GotFocus.Document == null) return;

			try
			{
				bool debugActive = m_addin.Description.Contains("Debug");
				_navigationTreeView.Init(m_dte, GotFocus.Document, debugActive);
				_navigationTreeView.LoadFunctionList();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + Environment.NewLine + ex.Source);
			}
		}

		/// <summary>
		/// The solution events_ opened.
		/// </summary>
		private void solutionEvents_Opened()
		{
			ShowWindow();
		}

		/// <summary>
		/// Creates control.
		/// </summary>
		/// <returns>
		/// The ensure window created.
		/// </returns>
		private bool EnsureWindowCreated()
		{
			if (_navigationTreeView != null) return true;
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				object obj = null;
				string guid = "{cae4f06e-3b94-46e5-9721-e135c20260a4}";
				var windows2 = (Windows2) m_dte.Windows;
				Assembly asm = Assembly.GetExecutingAssembly();

				try
				{
					m_toolWindow = windows2.CreateToolWindow2(m_addin, asm.Location, "JS_addin.Addin.NavigationTreeView",
					                                          "JavaScript Parser", guid, ref obj);
				}
				catch
				{
					return false;
				}

				_navigationTreeView = obj as NavigationTreeView;
				if (_navigationTreeView == null || m_toolWindow == null) return false;
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Show control.
		/// </summary>
		/// <returns>
		/// The show window.
		/// </returns>
		private bool ShowWindow()
		{
			if (EnsureWindowCreated())
			{
				if (!m_toolWindow.Linkable)
					m_toolWindow.Linkable = true;
				if (m_toolWindow.IsFloating)
					m_toolWindow.IsFloating = false;
				if (!m_toolWindow.Visible)
					m_toolWindow.Visible = true;
				return true;
			}
			else
				return false;
		}

		#region Documetn Event handlers

		/// <summary>
		/// The document events_ document opened.
		/// </summary>
		/// <param name="Document">
		/// The document.
		/// </param>
		private void documentEvents_DocumentOpened(Document Document)
		{
		}

		/// <summary>
		/// The document events_ document saved.
		/// </summary>
		/// <param name="doc">
		/// The doc.
		/// </param>
		private void documentEvents_DocumentSaved(Document doc)
		{
			if (_navigationTreeView != null)
				_navigationTreeView.LoadFunctionList();
		}

		/// <summary>
		/// The document events_ document opening.
		/// </summary>
		/// <param name="DocumentPath">
		/// The document path.
		/// </param>
		/// <param name="ReadOnly">
		/// The read only.
		/// </param>
		private void documentEvents_DocumentOpening(string DocumentPath, bool ReadOnly)
		{
		}

		/// <summary>
		/// The document events_ document closing.
		/// </summary>
		/// <param name="doc">
		/// The doc.
		/// </param>
		private void documentEvents_DocumentClosing(Document doc)
		{
			if (_navigationTreeView != null)
				_navigationTreeView.Clear();
		}

		#endregion
	}
}