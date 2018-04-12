namespace Sitecore.Support.Analytics.Pipelines.EndAnalytics
{
  using System.Reflection;
  using Diagnostics;
  using Extensions;
  using Configuration;
  using Sitecore.Analytics;
  using Sitecore.Analytics.Exceptions;
  using Sitecore.Analytics.Tracking;
  using Sitecore.Pipelines;

  public class ReleaseContact
  {
    public void Process(PipelineArgs args)
    {
      #region Fix 124871 Wrapped into try-catch syntax

      try

        #endregion

      {
        if (Tracker.Current == null)
        {
          Log.Debug("Tracker is not initialized. ReleaseContact processor is skipped");
          return;
        }

        var session = Tracker.Current.Session;
        Assert.IsNotNull(session, "Tracker.Current.Session");

        var transferInProgress =
          (bool) session.GetType()
            .GetProperty("TransferInProcess", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty)
            .GetValue(session);

        if (transferInProgress)
        {
          Log.Debug("Contact is being transferred. ReleaseContact processor is skipped");
          return;
        }

        if (session.Contact == null)
        {
          Log.Debug("Contact is null. ReleaseContact processor is skipped");
          return;
        }

        if (session.Settings.IsTransient)
        {
          Log.Debug("Session is in TRANSIENT MODE. ReleaseContact processor is skipped");
          return;
        }

        if (session.IsReadOnly)
          return;

        var manager = Factory.CreateObject("tracking/contactManager", true) as ContactManager;
        Assert.IsNotNull(manager, "tracking/contactManager");

        #region Fix 198208

        session = session.GetOriginalSession() ?? session;

        #endregion

        manager.SaveAndReleaseContact(session.Contact);
        session.Contact = null;
      }
      catch (ContactLockException exception)
      {
        Log.Error("Contact cannot be locked.", exception, this);
      }
    }
  }
}