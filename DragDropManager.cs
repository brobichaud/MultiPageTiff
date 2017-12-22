using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Tools
{
	/// <summary>
	/// Interface to be implemented by parent form
	/// to handle files dropped from Windows Explorer.
	/// </summary>
	public interface IDropFileTarget
	{
		void DroppedFiles(System.Array fileList);
	}

	/// <summary>
	/// DropFileHandler manages file drop operations for a Windows Form
	/// and calls on a known interface to supply the file list to the form
	/// 
	/// To use this class:
	/// 1) Derive a Form class from the IDropFileTarget interface:
	///       public class Form1 : Form, IDropFileTarget
	///     
	/// 2) Implement IDropFileTarget interface in the Form:
	///       public void DroppedFiles(Array fileList)
	///       {
	///           // handle dropped files here
	///       }
	///         
	///  3) Add member of this class to parent form:
	///       private DropFileHandler _dropFileHandler;
	/// 
	///  4) Initialize class instance in parent form Load event:
	///       private void Form1_Load(object sender, System.EventArgs e)
	///       {
	///           _dropFileHandler = new DropFileHandler(this);
	///       }
	/// </summary>
	public class DropFileHandler
	{
		private Form _parentForm;

		// delegate used in asynchronous call to parent form:
		private delegate void HandleFilesCallback(Array fileList);

		public DropFileHandler()
		{
		}

		public DropFileHandler(Form parent)
		{
			Parent = parent;
		}

		/// <summary>
		/// Set reference to parent form and make initialization
		/// </summary>
		public Form Parent
		{
			get { return _parentForm; }
			set
			{
				_parentForm = value;

				// verify correct interface
				if (!(_parentForm is IDropFileTarget))
					throw new Exception("DropFileHandler: Form doesn't implement IDropFileTarget interface");

				// ensure that form allows dropping
				_parentForm.AllowDrop = true;

				// subscribe to form's drag-drop events
				_parentForm.DragEnter += new DragEventHandler(this.OnDragEnter);
				_parentForm.DragDrop += new DragEventHandler(this.OnDragDrop);
			}
		}

		/// <summary>
		/// Handle form DragEnter event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDragEnter(object sender, DragEventArgs e)
		{
			// show copy cursor if a file is being dragged
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
			else
				e.Effect = DragDropEffects.None;
		}

		/// <summary>
		/// Handle form DragDrop event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDragDrop(object sender, DragEventArgs e)
		{
			try
			{
				// get list of dropped files
				Array files = (Array)e.Data.GetData(DataFormats.FileDrop);

				if (files != null)
				{
					// delegate for asynchronous call
					HandleFilesCallback handleFiles = new HandleFilesCallback(((IDropFileTarget)_parentForm).DroppedFiles);

					// marshall to the UI thread
					_parentForm.BeginInvoke(handleFiles, new Object[] { files });
					_parentForm.Activate();  // bring form to front
				}
			}
			catch (Exception exp)
			{
				Trace.WriteLine("Error in DropFileHandler.OnDragDrop: " + exp.Message);
			}
		}
	}
}
