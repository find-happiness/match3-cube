/// <summary>
/// This class handles error and exception messages, and makes sure they are added to the Quality category 
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class GA_Debug
{
	public bool SubmitErrors;
	public int MaxErrorCount;
	public bool SubmitErrorStackTrace;
	public bool SubmitErrorSystemInfo;
	
	private  int _errorCount = 0;
	
	private  bool _showLogOnGUI = true;
	public  List<string> Messages;
	
	/// <summary>
	/// If SubmitErrors is enabled on the GA object this makes sure that any exceptions or errors are submitted to the GA server
	/// </summary>
	/// <param name="logString">
	/// The message <see cref="System.String"/>
	/// </param>
	/// <param name="stackTrace">
	/// The exception stack trace <see cref="System.String"/>
	/// </param>
	/// <param name="type">
	/// The type of the log message (we only submit errors and exceptions to the GA server) <see cref="LogType"/>
	/// </param>
	public  void HandleLog(string logString, string stackTrace, LogType type)
	{
		//Only used if the GA_DebugGUI script is added to the GA object (for testing)
		if (_showLogOnGUI)
		{
			if (Messages == null)
			{
				Messages = new List<string>();
			}
			Messages.Add(logString);
		}
		
		//We only submit exceptions and errors
        if (SubmitErrors && _errorCount < MaxErrorCount && (type == LogType.Exception || type == LogType.Error))
		{
			// Might be worth looking into: http://www.doogal.co.uk/exception.php
			
			_errorCount++;
			
			bool errorSubmitted = false;
			
			string eventID = "";
			
			try
			{
				eventID = logString.Split(':')[0];
			}
			catch
			{
				eventID = logString;
			}
			
			if (SubmitErrorStackTrace)
			{
				SubmitError(eventID, stackTrace);
				errorSubmitted = true;
			}
			
			if (SubmitErrorSystemInfo)
			{
				List<Dictionary<string, object>> systemspecs = GA.API.GenericInfo.GetGenericInfo(eventID);
			
				foreach (Dictionary<string, object> spec in systemspecs)
				{
					GA_Queue.AddItem(spec, GA_Submit.CategoryType.GA_Log, false);
				}
				
				errorSubmitted = true;
			}
			
			if (!errorSubmitted)
			{
				SubmitError(eventID, null);
			}
		}
    }
	
	public  void SubmitError(string eventName, string message)
	{
		Vector3 target = Vector3.zero;
		if (GA.Settings.TrackTarget != null)
			target = GA.Settings.TrackTarget.position;
		
		GA.API.Quality.NewErrorEvent(eventName, message, target.x, target.y, target.z);
	}
}
