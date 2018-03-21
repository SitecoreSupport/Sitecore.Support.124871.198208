namespace Sitecore.Support.Analytics.Pipelines.EndAnalytics
{
  using System.Reflection;
  using Abstractions;
  using DependencyInjection;
  using Diagnostics;
  using Microsoft.Extensions.DependencyInjection;
  using Sitecore.Analytics;
  using Sitecore.Analytics.Tracking;
  using Sitecore.Pipelines;

  public class ReleaseContact
  {
    private readonly BaseFactory _factory;

    private readonly BaseLog _log;

    public ReleaseContact() : this(ServiceLocator.ServiceProvider.GetRequiredService<BaseFactory>(),
      ServiceLocator.ServiceProvider.GetRequiredService<BaseLog>())
    {
    }

    internal ReleaseContact(BaseFactory factory, BaseLog log)
    {
      Assert.ArgumentNotNull(factory, "factory");
      Assert.ArgumentNotNull(log, "log");
      _factory = factory;
      _log = log;
    }

    public void Process(PipelineArgs args)
    {
      if (Tracker.Current == null)
      {
        _log.Debug("Tracker is not initialized. ReleaseContact processor is skipped");
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
        _log.Debug("Contact is being transferred. ReleaseContact processor is skipped");
        return;
      }

      if (session.Contact == null)
      {
        _log.Debug("Contact is null. ReleaseContact processor is skipped");
        return;
      }

      if (session.Settings.IsTransient)
      {
        _log.Debug("Session is in TRANSIENT MODE. ReleaseContact processor is skipped");
        return;
      }

      if (session.IsReadOnly)
        return;

      var manager = _factory.CreateObject("tracking/contactManager", true) as ContactManager;
      Assert.IsNotNull(manager, "tracking/contactManager");
      manager.SaveAndReleaseContact(session.Contact);
      session.Contact = null;
    }
  }
}