using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helps with logging.
/// </summary>
static public class Logger
{
    #region Internal Methods
    /// <summary>
    /// Formats a message to contain information about the sender.
    /// </summary>
    /// <param name="sender">
    /// The sender.
    /// </param>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <returns>
    /// The formatted message.
    /// </returns>
    static private string FormatMessage(object sender, string message)
    {
        // Validate
        if (sender == null) { throw new ArgumentNullException(nameof(sender)); }
        if (message == null) { throw new ArgumentNullException(nameof(message)); }

        // Get the type name
        string typeName = sender.GetType().Name;

        // Try to get the object name
        string objectName = null;
        if (sender is GameObject) { objectName = ((GameObject)sender).name; }
        if (sender is Component) { objectName = ((Component)sender).name; }

        // Format message
        if (objectName != null)
        {
            return $"{typeName} ({objectName}): {message}";
        }
        else
        {
            return $"{typeName}: {message}";
        }
    }
    #endregion // Internal Methods

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="sender">
    /// The sender of the message.
    /// </param>
    /// <param name="message">
    /// The message.
    /// </param>
    static public void LogError(this object sender, string message)
    {
        Debug.LogError(FormatMessage(sender, message));
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="sender">
    /// The sender of the message.
    /// </param>
    /// <param name="message">
    /// The message.
    /// </param>
    static public void LogWarning(this object sender, string message)
    {
        Debug.LogWarning(FormatMessage(sender, message));
    }

    /// <summary>
    /// Logs a standard message.
    /// </summary>
    /// <param name="sender">
    /// The sender of the message.
    /// </param>
    /// <param name="message">
    /// The message.
    /// </param>
    static public void Log(this object sender, string message)
    {
        LogRaw(sender, FormatMessage(sender, message));
    }

    /// <summary>
    /// Logs a standard message without any formatting.
    /// </summary>
    /// <param name="sender">
    /// The sender of the message.
    /// </param>
    /// <param name="message">
    /// The message.
    /// </param>
    static public void LogRaw(this object sender, string message)
    {
        Debug.Log(message);
    }
}