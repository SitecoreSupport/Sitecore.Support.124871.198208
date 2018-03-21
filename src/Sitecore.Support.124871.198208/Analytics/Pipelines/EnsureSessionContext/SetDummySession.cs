namespace Sitecore.Support.Analytics.Pipelines.EnsureSessionContext
{
  using System.IO;
  using System.Runtime.Serialization.Formatters.Binary;
  using Abstractions;
  using DependencyInjection;
  using Diagnostics;
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
        var memoryStream = new MemoryStream();
        var formatter = new BinaryFormatter();
        formatter.Serialize(memoryStream, args.Session);
        memoryStream.Position = 0L;
        args.Session = (Session) formatter.Deserialize(memoryStream);
        memoryStream.Close();
        memoryStream.Dispose();
      }
    }
  }
}