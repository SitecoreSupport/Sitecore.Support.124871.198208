namespace Sitecore.Support.Analytics.Pipelines.EnsureSessionContext
{
  using System.IO;
  using System.Runtime.Serialization.Formatters.Binary;
  using Abstractions;
  using DependencyInjection;
  using Diagnostics;
  using Extensions;
  using Microsoft.Extensions.DependencyInjection;
  using Sitecore.Analytics.Pipelines.InitializeTracker;
  using Sitecore.Analytics.Tracking;

  public class SetDummySession : InitializeTrackerProcessor
  {
    private readonly BaseLog _log;

    public SetDummySession() : this(ServiceLocator.ServiceProvider.GetRequiredService<BaseLog>())
    {
    }

    public SetDummySession(BaseLog log)
    {
      Assert.ArgumentNotNull(log, "log");
      _log = log;
    }

    public int MaxPageIndexThreshold { get; set; }

    public override void Process(InitializeTrackerArgs args)
    {
      if (args.Session != null && args.Session.Interaction != null &&
          args.Session.Interaction.PageCount >= MaxPageIndexThreshold)
      {
        #region Fix Introduce warning

        if (!args.Session.CustomData.ContainsKey("MaxPageIndexThresholdWarningLogged"))
        {
          _log.Warn(
            string.Format(
              "Session has reached the max page threshold of {0}. If you see this message regularly, you should increase configuration parameter MaxPageIndexThreshold to avoid loss of valid data.",
              MaxPageIndexThreshold), this);
          args.Session.CustomData.Add("MaxPageIndexThresholdWarningLogged", true);
        }

        #endregion


        #region Fix session local variable

        var sessionSource = args.Session;

        #endregion

        var memoryStream = new MemoryStream();
        var formatter = new BinaryFormatter();
        formatter.Serialize(memoryStream, sessionSource);
        memoryStream.Position = 0L;
        var sessionDestination = (Session) formatter.Deserialize(memoryStream);

        #region Fix return local session object as args.Session

        sessionDestination.SetOriginalSession(sessionSource);
        args.Session = sessionDestination;

        #endregion

        memoryStream.Close();
        memoryStream.Dispose();
      }
    }
  }
}